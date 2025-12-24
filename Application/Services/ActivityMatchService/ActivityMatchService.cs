using AutoMapper;
using EduShpere.Application.DTOs.ActivityMatchDto;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Application.Services
{
    public class ActivityMatchService : IActivityMatchService
    {
        private readonly IActivityMatchRepository _matchRepo;
        private readonly IActivityRepository _activityRepo;
        private readonly IActivityParticipantRepository _participantRepo;
        private readonly IClassGroupRepository _classGroupRepo;
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;
        private readonly EduShpereDbContext _context;

        public ActivityMatchService(
            IActivityMatchRepository matchRepo,
            IActivityRepository activityRepo,
            IActivityParticipantRepository participantRepo,
            IClassGroupRepository classGroupRepo,
            IAuditService auditService,
            IMapper mapper,
            EduShpereDbContext context)
        {
            _matchRepo = matchRepo;
            _activityRepo = activityRepo;
            _participantRepo = participantRepo;
            _classGroupRepo = classGroupRepo;
            _auditService = auditService;
            _mapper = mapper;
            _context = context;
        }

        private Task EnsureSchemaAsync()
            => ActivityMatchSchemaHelper.EnsureIsPublishedColumnExistsAsync(_context);

        public async Task<BracketResponseDto> GenerateSingleEliminationBracketAsync(GenerateBracketDto dto)
        {
            await EnsureSchemaAsync();
            // 1. Kiểm tra Activity tồn tại
            var activity = await _activityRepo.GetByIdWithIncludesAsync(dto.ActivityId);
            if (activity == null)
            {
                throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);
            }

            // 2. Kiểm tra Sport tồn tại
            var sport = activity.Sports?.FirstOrDefault(s => s.Id == dto.SportId && !s.IsDeleted);
            if (sport == null)
            {
                throw new NotFoundException(ErrorMessages.ActivityMatch.SportNotFound);
            }

            // 3. Kiểm tra đã có bracket chưa
            if (await _matchRepo.HasMatchesForActivityAndSportAsync(dto.ActivityId, dto.SportId, dto.Grade))
            {
                throw new BadRequestException(ErrorMessages.ActivityMatch.BracketAlreadyExists);
            }

            // 4. Lấy danh sách lớp đã đăng ký (có ClassGroupId)
            var participants = await _context.ActivityParticipants
                .Where(ap => ap.ActivityId == dto.ActivityId && 
                            ap.ClassGroupId.HasValue && 
                            !ap.IsDeleted)
                .Include(ap => ap.ClassGroup)
                    .ThenInclude(cg => cg!.AcademicYears)
                .ToListAsync();

            if (dto.Grade.HasValue)
            {
                participants = participants
                    .Where(ap => ap.ClassGroup?.Grade == dto.Grade.Value)
                    .ToList();
            }

            var classGroups = participants
                .Where(ap => ap.ClassGroup != null)
                .Select(ap => ap.ClassGroup!)
                .Distinct()
                .OrderBy(cg => cg.Id)
                .ToList();

            if (classGroups.Count < 2)
            {
                throw new BadRequestException(ErrorMessages.ActivityMatch.InsufficientClasses);
            }

            // 5. Group by Grade để xử lý từng khối
            var groupsByGrade = classGroups.GroupBy(cg => cg.Grade).ToList();

            var allMatches = new List<ActivityMatch>();
            int matchIdCounter = 1;

            foreach (var gradeGroup in groupsByGrade)
            {
                var gradeClassGroups = gradeGroup.ToList();
                var gradeValue = gradeGroup.Key;

                // 6. Tính số vòng và số slot cần thiết
                int numParticipants = gradeClassGroups.Count;
                int numRounds = (int)Math.Ceiling(Math.Log2(numParticipants));
                int totalSlots = (int)Math.Pow(2, numRounds);
                int numByes = totalSlots - numParticipants;

                // 7. Xáo trộn ngẫu nhiên các lớp
                var shuffled = gradeClassGroups.OrderBy(x => Guid.NewGuid()).ToList();

                // 8. Tạo các trận đấu vòng 1
                int participantIndex = 0;
                int matchNumber = 1;

                var round1Matches = new List<ActivityMatch>();
                var nextRoundMatchNumbers = new List<int>();

                for (int i = 0; i < totalSlots; i += 2)
                {
                    var match = new ActivityMatch
                    {
                        ActivityId = dto.ActivityId,
                        SportId = dto.SportId,
                        Grade = gradeValue,
                        Round = 1,
                        RoundName = GetRoundName(1, numRounds),
                        MatchNumber = matchNumber++,
                        Status = MatchStatus.Pending,
                        IsDeleted = false
                    };

                    if (i < numByes)
                    {
                        // Bye match - chỉ có 1 lớp, tự động thắng
                        match.ClassGroup1Id = shuffled[participantIndex++].Id;
                        match.ClassGroup2Id = null;
                        match.IsBye = true;
                        match.WinnerClassGroupId = match.ClassGroup1Id; // Tự động thắng
                        match.Status = MatchStatus.Completed;
                    }
                    else
                    {
                        // Normal match - 2 lớp
                        match.ClassGroup1Id = shuffled[participantIndex++].Id;
                        match.ClassGroup2Id = shuffled[participantIndex++].Id;
                        match.IsBye = false;
                    }

                    _auditService.SetAuditFieldsForCreate(match);
                    round1Matches.Add(match);
                    allMatches.Add(match);
                }

                // 9. Tạo các vòng tiếp theo
                int currentRound = 2;
                int currentRoundMatchCount = totalSlots / 2;
                var previousRoundMatches = round1Matches;

                while (currentRoundMatchCount > 1)
                {
                    var currentRoundMatches = new List<ActivityMatch>();
                    matchNumber = 1;

                    for (int i = 0; i < currentRoundMatchCount; i += 2)
                    {
                        var match1 = previousRoundMatches[i];
                        var match2 = previousRoundMatches[i + 1];

                        var nextMatch = new ActivityMatch
                        {
                            ActivityId = dto.ActivityId,
                            SportId = dto.SportId,
                            Grade = gradeValue,
                            Round = currentRound,
                            RoundName = GetRoundName(currentRound, numRounds),
                            MatchNumber = matchNumber++,
                            Status = MatchStatus.Pending,
                            IsDeleted = false,
                            ClassGroup1Id = null, // Sẽ được fill khi match1 hoàn thành
                            ClassGroup2Id = null  // Sẽ được fill khi match2 hoàn thành
                        };

                        _auditService.SetAuditFieldsForCreate(nextMatch);
                        currentRoundMatches.Add(nextMatch);
                        allMatches.Add(nextMatch);
                    }

                    previousRoundMatches = currentRoundMatches;
                    currentRoundMatchCount /= 2;
                    currentRound++;
                }

            }

            // 10. Lưu tất cả matches (2 bước: tạo matches trước, sau đó link NextMatchId)
            BracketResponseDto? resultBracket = null;
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Bước 1: Tạo tất cả matches (chưa có NextMatchId)
                    await _context.ActivityMatches.AddRangeAsync(allMatches);
                    await _context.SaveChangesAsync(); // Save để có ID

                    // Bước 2: Link NextMatchId sau khi đã có ID
                    // Với mỗi khối, tạo mapping từ round này sang round tiếp theo
                    foreach (var gradeGroup in groupsByGrade)
                    {
                        var gradeValue = gradeGroup.Key;
                        var gradeMatches = allMatches.Where(m => m.Grade == gradeValue).ToList();
                        
                        // Group by round
                        var matchesByRound = gradeMatches.GroupBy(m => m.Round).OrderBy(g => g.Key).ToList();
                        
                        for (int roundIndex = 0; roundIndex < matchesByRound.Count - 1; roundIndex++)
                        {
                            var currentRound = matchesByRound[roundIndex].OrderBy(m => m.MatchNumber).ToList();
                            var nextRound = matchesByRound[roundIndex + 1].OrderBy(m => m.MatchNumber).ToList();
                            
                            // Mỗi 2 trận ở round hiện tại sẽ đi vào 1 trận ở round tiếp theo
                            for (int i = 0; i < currentRound.Count; i += 2)
                            {
                                if (i < currentRound.Count)
                                {
                                    currentRound[i].NextMatchId = nextRound[i / 2].Id;
                                }
                                if (i + 1 < currentRound.Count)
                                {
                                    currentRound[i + 1].NextMatchId = nextRound[i / 2].Id;
                                }
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // 11. Return bracket
                    resultBracket = await GetBracketByActivityAsync(dto.ActivityId, dto.SportId, dto.Grade);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            return resultBracket!;
        }

        private string GetRoundName(int round, int totalRounds)
        {
            if (round == totalRounds)
                return "Chung kết";
            else if (round == totalRounds - 1)
                return "Bán kết";
            else if (round == totalRounds - 2)
                return "Tứ kết";
            else
                return $"Vòng {round}";
        }

        public async Task<BracketResponseDto> GetBracketByActivityAsync(int activityId, int sportId, int? grade = null)
        {
            await EnsureSchemaAsync();
            var matches = await _matchRepo.GetByActivityAndSportAndGradeAsync(activityId, sportId, grade);

            if (!matches.Any())
            {
                throw new NotFoundException(ErrorMessages.ActivityMatch.BracketNotFound);
            }

            // Tối ưu: Chỉ query SportName thay vì load toàn bộ Activity
            var sportName = await _context.ActivitySports
                .Where(s => s.ActivityId == activityId && s.Id == sportId && !s.IsDeleted)
                .Select(s => s.SportName)
                .FirstOrDefaultAsync();

            var groupedRounds = matches
                .GroupBy(m => m.Round)
                .OrderBy(g => g.Key)
                .ToList();

            var totalRounds = groupedRounds.Count;

            var rounds = groupedRounds
                .Select(g => new RoundDto
                {
                    RoundNumber = g.Key,
                    // Ưu tiên chuẩn hóa tên vòng theo tổng số vòng để tránh nhãn sai (vd: bán kết khi chỉ có 2 vòng)
                    RoundName = GetRoundName(g.Key, totalRounds),
                    Matches = g.OrderBy(m => m.MatchNumber)
                        .Select(m => _mapper.Map<MatchResponseDto>(m))
                        .ToList()
                })
                .ToList();

            return new BracketResponseDto
            {
                ActivityId = activityId,
                SportId = sportId,
                SportName = sportName,
                Grade = grade ?? matches.First().Grade,
                TotalRounds = totalRounds,
                TotalMatches = matches.Count(),
                Rounds = rounds
            };
        }

        public async Task<IEnumerable<MatchResponseDto>> GetMatchesByRoundAsync(int activityId, int sportId, int round, int? grade = null)
        {
            await EnsureSchemaAsync();
            var matches = await _matchRepo.GetByRoundAsync(activityId, sportId, round, grade);
            return matches.Select(m => _mapper.Map<MatchResponseDto>(m));
        }

        public async Task<MatchResponseDto> GetMatchByIdAsync(int matchId)
        {
            await EnsureSchemaAsync();
            var match = await _matchRepo.GetByIdWithIncludesAsync(matchId);
            if (match == null)
            {
                throw new NotFoundException(ErrorMessages.ActivityMatch.NotFound);
            }

            return _mapper.Map<MatchResponseDto>(match);
        }

        public async Task<MatchResponseDto> CreateMatchAsync(CreateMatchDto dto)
        {
            await EnsureSchemaAsync();
            // Validate Activity tồn tại
            var activity = await _activityRepo.GetByIdWithIncludesAsync(dto.ActivityId);
            if (activity == null)
            {
                throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);
            }

            // Validate Sport thuộc Activity
            var sport = activity.Sports?.FirstOrDefault(s => s.Id == dto.SportId && !s.IsDeleted);
            if (sport == null)
            {
                throw new NotFoundException(ErrorMessages.ActivityMatch.SportNotFound);
            }

            // Validate
            if (dto.ClassGroup1Id.HasValue && dto.ClassGroup2Id.HasValue && 
                dto.ClassGroup1Id == dto.ClassGroup2Id)
            {
                throw new BadRequestException(ErrorMessages.ActivityMatch.SameClassGroup);
            }

            // Validate các lớp đã đăng ký Activity
            if (dto.ClassGroup1Id.HasValue)
            {
                var isRegistered1 = await _context.ActivityParticipants
                    .AnyAsync(ap => ap.ActivityId == dto.ActivityId && 
                                   ap.ClassGroupId == dto.ClassGroup1Id.Value && 
                                   !ap.IsDeleted);
                if (!isRegistered1)
                {
                    throw new BadRequestException(ErrorMessages.ActivityMatch.ClassGroupNotRegistered);
                }
            }

            if (dto.ClassGroup2Id.HasValue)
            {
                var isRegistered2 = await _context.ActivityParticipants
                    .AnyAsync(ap => ap.ActivityId == dto.ActivityId && 
                                   ap.ClassGroupId == dto.ClassGroup2Id.Value && 
                                   !ap.IsDeleted);
                if (!isRegistered2)
                {
                    throw new BadRequestException(ErrorMessages.ActivityMatch.ClassGroupNotRegistered);
                }
            }

            // Validate cùng khối
            if (dto.ClassGroup1Id.HasValue && dto.ClassGroup2Id.HasValue)
            {
                var classGroup1 = await _classGroupRepo.GetClassGroupByIdWithAcademicYearAsync(dto.ClassGroup1Id.Value);
                var classGroup2 = await _classGroupRepo.GetClassGroupByIdWithAcademicYearAsync(dto.ClassGroup2Id.Value);

                if (classGroup1 == null || classGroup2 == null)
                {
                    throw new BadRequestException(ErrorMessages.ActivityParticipant.InvalidClassGroup);
                }

                if (classGroup1.Grade != classGroup2.Grade)
                {
                    throw new BadRequestException(ErrorMessages.ActivityMatch.DifferentGrades);
                }
            }

            var match = _mapper.Map<ActivityMatch>(dto);
            match.Status = MatchStatus.Pending;

            _auditService.SetAuditFieldsForCreate(match);
            await _matchRepo.AddAsync(match);

            return _mapper.Map<MatchResponseDto>(match);
        }

        public async Task<MatchResponseDto> UpdateMatchResultAsync(int matchId, UpdateMatchResultDto dto)
        {
            await EnsureSchemaAsync();
            var match = await _matchRepo.GetByIdWithIncludesAsync(matchId);
            if (match == null)
            {
                throw new NotFoundException(ErrorMessages.ActivityMatch.NotFound);
            }

            if (match.Status == MatchStatus.Completed && dto.MarkAsCompleted)
            {
                throw new BadRequestException(ErrorMessages.ActivityMatch.MatchAlreadyCompleted);
            }

            var markAsCompleted = dto.MarkAsCompleted;
            var hasWinner = dto.WinnerClassGroupId.HasValue;

            // Validate winner nếu có
            if (hasWinner)
            {
                if (dto.WinnerClassGroupId != match.ClassGroup1Id &&
                    dto.WinnerClassGroupId != match.ClassGroup2Id)
                {
                    throw new BadRequestException(ErrorMessages.ActivityMatch.InvalidWinner);
                }
            }

            match.Score1 = dto.Score1;
            match.Score2 = dto.Score2;

            // Tự động set status = Completed khi có winner (dù chưa tới ngày)
            if (hasWinner)
            {
                match.WinnerClassGroupId = dto.WinnerClassGroupId;
                match.Status = MatchStatus.Completed;
                if (!match.ActualStartTime.HasValue)
                {
                    match.ActualStartTime = DateTime.UtcNow;
                }

                // Cập nhật trận tiếp theo với đội thắng
                if (match.NextMatchId.HasValue)
                {
                    var nextMatch = await _matchRepo.GetByIdWithIncludesAsync(match.NextMatchId.Value);
                    if (nextMatch != null && !nextMatch.IsDeleted)
                    {
                        // Tìm vị trí trống trong nextMatch (ClassGroup1Id hoặc ClassGroup2Id)
                        if (!nextMatch.ClassGroup1Id.HasValue)
                        {
                            nextMatch.ClassGroup1Id = match.WinnerClassGroupId;
                        }
                        else if (!nextMatch.ClassGroup2Id.HasValue)
                        {
                            nextMatch.ClassGroup2Id = match.WinnerClassGroupId;
                        }

                        _auditService.SetAuditFieldsForUpdate(nextMatch);
                        await _matchRepo.UpdateAsync(nextMatch);
                    }
                }
            }
            else if (markAsCompleted)
            {
                // Nếu markAsCompleted nhưng không có winner, chỉ set InProgress
                match.Status = MatchStatus.InProgress;
                match.WinnerClassGroupId = null;
                if (!match.ActualStartTime.HasValue)
                {
                    match.ActualStartTime = DateTime.UtcNow;
                }
            }
            else
            {
                // Chỉ cập nhật tỉ số, giữ nguyên status
                if (match.Status == MatchStatus.Pending)
                {
                    match.Status = MatchStatus.InProgress;
                }
                if (!match.ActualStartTime.HasValue)
                {
                    match.ActualStartTime = DateTime.UtcNow;
                }
            }

            _auditService.SetAuditFieldsForUpdate(match);
            await _matchRepo.UpdateAsync(match);

            return _mapper.Map<MatchResponseDto>(match);
        }

        public async Task<MatchResponseDto> UpdateMatchAsync(int matchId, UpdateMatchDto dto)
        {
            await EnsureSchemaAsync();
            var match = await _matchRepo.GetByIdWithIncludesAsync(matchId);
            if (match == null)
            {
                throw new NotFoundException(ErrorMessages.ActivityMatch.NotFound);
            }

            // Validate ClassGroup1 nếu có thay đổi
            if (dto.ClassGroup1Id.HasValue && dto.ClassGroup1Id.Value != match.ClassGroup1Id)
            {
                // Kiểm tra lớp đã đăng ký Activity
                var isRegistered1 = await _context.ActivityParticipants
                    .AnyAsync(ap => ap.ActivityId == match.ActivityId && 
                                   ap.ClassGroupId == dto.ClassGroup1Id.Value && 
                                   !ap.IsDeleted);
                if (!isRegistered1)
                {
                    throw new BadRequestException(ErrorMessages.ActivityMatch.ClassGroupNotRegistered);
                }

                // Kiểm tra cùng khối với ClassGroup2 (nếu có)
                if (match.ClassGroup2Id.HasValue)
                {
                    var classGroup1 = await _classGroupRepo.GetClassGroupByIdWithAcademicYearAsync(dto.ClassGroup1Id.Value);
                    var classGroup2 = await _classGroupRepo.GetClassGroupByIdWithAcademicYearAsync(match.ClassGroup2Id.Value);
                    if (classGroup1?.Grade != classGroup2?.Grade)
                    {
                        throw new BadRequestException(ErrorMessages.ActivityMatch.DifferentGrades);
                    }
                }

                match.ClassGroup1Id = dto.ClassGroup1Id.Value;
            }

            // Validate ClassGroup2 nếu có thay đổi
            if (dto.ClassGroup2Id.HasValue && dto.ClassGroup2Id.Value != match.ClassGroup2Id)
            {
                // Kiểm tra lớp đã đăng ký Activity
                var isRegistered2 = await _context.ActivityParticipants
                    .AnyAsync(ap => ap.ActivityId == match.ActivityId && 
                                   ap.ClassGroupId == dto.ClassGroup2Id.Value && 
                                   !ap.IsDeleted);
                if (!isRegistered2)
                {
                    throw new BadRequestException(ErrorMessages.ActivityMatch.ClassGroupNotRegistered);
                }

                // Kiểm tra cùng khối với ClassGroup1 (nếu có)
                var currentClassGroup1Id = dto.ClassGroup1Id ?? match.ClassGroup1Id;
                if (currentClassGroup1Id.HasValue)
                {
                    var classGroup1 = await _classGroupRepo.GetClassGroupByIdWithAcademicYearAsync(currentClassGroup1Id.Value);
                    var classGroup2 = await _classGroupRepo.GetClassGroupByIdWithAcademicYearAsync(dto.ClassGroup2Id.Value);
                    if (classGroup1?.Grade != classGroup2?.Grade)
                    {
                        throw new BadRequestException(ErrorMessages.ActivityMatch.DifferentGrades);
                    }
                }

                match.ClassGroup2Id = dto.ClassGroup2Id.Value;
            }

            // Validate không trùng lớp
            if (match.ClassGroup1Id.HasValue && match.ClassGroup2Id.HasValue && 
                match.ClassGroup1Id == match.ClassGroup2Id)
            {
                throw new BadRequestException(ErrorMessages.ActivityMatch.SameClassGroup);
            }

            // Update các trường khác
            if (dto.MatchDate.HasValue)
                match.MatchDate = dto.MatchDate.Value;

            if (dto.StartTime.HasValue)
                match.StartTime = dto.StartTime.Value;

            if (dto.EndTime.HasValue)
                match.EndTime = dto.EndTime.Value;

            if (dto.Location != null)
                match.Location = dto.Location;

            if (dto.Status.HasValue)
            {
                if (dto.Status.Value == MatchStatus.InProgress && match.Status != MatchStatus.InProgress && !match.ActualStartTime.HasValue)
                {
                    match.ActualStartTime = DateTime.UtcNow;
                }
                match.Status = dto.Status.Value;
            }

            if (dto.Notes != null)
                match.Notes = dto.Notes;

            _auditService.SetAuditFieldsForUpdate(match);
            await _matchRepo.UpdateAsync(match);

            return _mapper.Map<MatchResponseDto>(match);
        }

        public async Task<bool> DeleteMatchAsync(int matchId)
        {
            await EnsureSchemaAsync();
            var match = await _matchRepo.GetByIdAsync(matchId);
            if (match == null || match.IsDeleted)
            {
                throw new NotFoundException(ErrorMessages.ActivityMatch.NotFound);
            }

            // Kiểm tra nếu match đã hoàn thành và có NextMatch đã được fill
            // Nếu match này đã có winner và winner đã được đưa vào NextMatch, 
            // thì không nên xóa (hoặc cần xử lý phức tạp hơn)
            // Tạm thời chỉ cho phép xóa nếu match chưa hoàn thành
            if (match.Status == MatchStatus.Completed && match.NextMatchId.HasValue)
            {
                var nextMatch = await _matchRepo.GetByIdAsync(match.NextMatchId.Value);
                if (nextMatch != null && !nextMatch.IsDeleted)
                {
                    // Nếu winner đã được đưa vào NextMatch, cần xóa nó ra
                    if (nextMatch.ClassGroup1Id == match.WinnerClassGroupId)
                    {
                        nextMatch.ClassGroup1Id = null;
                        _auditService.SetAuditFieldsForUpdate(nextMatch);
                        await _matchRepo.UpdateAsync(nextMatch);
                    }
                    else if (nextMatch.ClassGroup2Id == match.WinnerClassGroupId)
                    {
                        nextMatch.ClassGroup2Id = null;
                        _auditService.SetAuditFieldsForUpdate(nextMatch);
                        await _matchRepo.UpdateAsync(nextMatch);
                    }
                }
            }

            // Soft delete
            match.IsDeleted = true;
            _auditService.SetAuditFieldsForUpdate(match);
            await _matchRepo.UpdateAsync(match);

            return true;
        }

        public async Task<bool> DeleteBracketAsync(int activityId, int sportId, int? grade = null)
        {
            await EnsureSchemaAsync();
            await _matchRepo.DeleteBracketAsync(activityId, sportId, grade);
            return true;
        }

        public async Task<IEnumerable<EligibleClassGroupsByGradeDto>> GetEligibleClassGroupsAsync(int activityId, int sportId)
        {
            await EnsureSchemaAsync();
            // Lấy danh sách lớp đã đăng ký Activity và có ClassGroupId
            var participants = await _context.ActivityParticipants
                .Where(ap => ap.ActivityId == activityId && 
                            ap.ClassGroupId.HasValue && 
                            !ap.IsDeleted)
                .Include(ap => ap.ClassGroup)
                    .ThenInclude(cg => cg!.AcademicYears)
                .Include(ap => ap.ClassGroup)
                    .ThenInclude(cg => cg!.Teacher)
                .ToListAsync();

            var classGroups = participants
                .Where(ap => ap.ClassGroup != null)
                .Select(ap => ap.ClassGroup!)
                .Distinct()
                .OrderBy(cg => cg.Grade)
                .ThenBy(cg => cg.Name)
                .ToList();

            var result = classGroups
                .GroupBy(cg => cg.Grade)
                .Select(g => new EligibleClassGroupsByGradeDto
                {
                    Grade = g.Key,
                    ClassCount = g.Count(),
                    ClassGroups = g.Select(cg => new EligibleClassGroupDto
                    {
                        ClassGroupId = cg.Id,
                        ClassGroupName = cg.Name,
                        Grade = cg.Grade,
                        AcademicYearId = cg.AcademicYearId ?? 0,
                        AcademicYearName = cg.AcademicYears?.Name,
                        HomeroomTeacherId = cg.TeacherId,
                        HomeroomTeacherName = cg.Teacher != null 
                            ? $"{cg.Teacher.FirstName} {cg.Teacher.LastName}" 
                            : null
                    }).ToList()
                })
                .ToList();

            return result;
        }
    }
}

