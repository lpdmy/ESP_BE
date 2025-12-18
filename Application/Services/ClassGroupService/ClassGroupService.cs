using AutoMapper;
using EduShpere.Application.DTOs.ClassGroupDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Infrastructure;
using EduShpere.Shared.Constants;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;
using EduShpere.Shared;
using EduShpere.Application.DTOs.AuthDto;

namespace EduShpere.Application.Services.ClassGroupService;

public class ClassGroupService : IClassGroupService
{
    private readonly IClassGroupRepository _repository;
    private readonly IAuditService _auditService;
    private readonly IPaginationService _paginationService;
    private readonly IMapper _mapper;

    public ClassGroupService(
        IClassGroupRepository repository,
        IAuditService auditService,
        IPaginationService paginationService,
        IMapper mapper)
    {
        _repository = repository;
        _auditService = auditService;
        _paginationService = paginationService;
        _mapper = mapper;
    }

    public async Task<PaginationResponseDto<ClassGroupDto>> GetAllAsync(PaginationRequestDto paginationRequest)
    {
        var query = await _repository.GetQueryableAsync();
        
        var pagedResult = await _paginationService.GetPagedResultAsync(query, paginationRequest);

        var classGroupDtos = pagedResult.Data.Select(c => new ClassGroupDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Grade = c.Grade,
            AcademicYearId = c.AcademicYearId,
            AcademicYearName = c.AcademicYears?.Name,
            CurrentStudentCount = c.ClassGroupMembers.Count,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            IsDeleted = c.IsDeleted
        });

        return new PaginationResponseDto<ClassGroupDto>
        {
            Data = classGroupDtos,
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<ClassGroupDto?> GetByIdAsync(int id)
    {
        var classGroup = await _repository.GetByIdAsync(id);
        if (classGroup == null || classGroup.IsDeleted)
            return null;

        return _mapper.Map<ClassGroupDto>(classGroup);
    }

    public async Task<ClassGroupDto?> GetByNameAsync(string name)
    {
        var classGroup = await _repository.GetByNameAsync(name);
        if (classGroup == null)
            return null;

        return _mapper.Map<ClassGroupDto>(classGroup);
    }

    public async Task<ClassGroupDashboardDto> GetDashboardDataAsync(int? academicYearId = null)
    {
        // Tối ưu: Tính statistics ở database thay vì load toàn bộ data
        var statisticsDomain = await _repository.GetStatisticsAsync(academicYearId);
        
        // Chỉ load classes cần thiết cho ClassesByGrade (không load ClassGroupMembers nếu không cần)
        var allClasses = await _repository.GetByFilterAsync(
            grade: null,
            academicYearId: academicYearId,
            isDeleted: false
        );
        
        Console.WriteLine($"GetDashboardDataAsync called with academicYearId: {academicYearId}");
        Console.WriteLine($"Total classes: {allClasses.Count()}");

        // Tính student count cho mỗi class bằng cách query riêng (hiệu quả hơn)
        var classIds = allClasses.Select(c => c.Id).ToList();
        var studentCounts = new Dictionary<int, int>();
        
        if (classIds.Any())
        {
            // Query student counts từ database thay vì load toàn bộ ClassGroupMembers
            var counts = await _repository.GetStudentCountsByClassIdsAsync(classIds);
            studentCounts = counts;
        }

        var classesByGrade = allClasses
            .GroupBy(c => c.Grade)
            .Select(g => new ClassGroupByGradeDto
            {
                Grade = g.Key,
                GradeName = g.Key.HasValue ? $"Khối {g.Key}" : "Không xác định",
                ClassCount = g.Count(),
                StudentCount = g.Sum(c => studentCounts.GetValueOrDefault(c.Id, 0)),
                Classes = g.Select(c => new ClassGroupDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Grade = c.Grade,
                    AcademicYearId = c.AcademicYearId,
                    AcademicYearName = c.AcademicYears?.Name,
                    CurrentStudentCount = studentCounts.GetValueOrDefault(c.Id, 0),
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsDeleted = c.IsDeleted
                }).ToList()
            })
            .OrderBy(g => g.Grade)
            .ToList();

        return new ClassGroupDashboardDto
        {
            Statistics = new ClassGroupStatisticsDto
            {
                TotalClasses = statisticsDomain.TotalClasses,
                TotalStudents = statisticsDomain.TotalStudents,
                TotalTeachers = statisticsDomain.TotalTeachers,
                DeletedClasses = statisticsDomain.DeletedClasses
            },
            ClassesByGrade = classesByGrade
        };
    }

    public async Task<ClassGroupDto> CreateAsync(CreateClassGroupDto dto)
    {
        if (!string.IsNullOrEmpty(dto.Name) && await _repository.IsExistsAsync(dto.Name, dto.Grade, dto.AcademicYearId))
        {
            throw new BadRequestException(ErrorMessages.ClassGroup.NameAlreadyExists);
        }

        var classGroup = _mapper.Map<ClassGroup>(dto);
        
        // Xử lý schedules nếu có
        if (dto.Schedules != null && dto.Schedules.Any())
        {
            classGroup.Schedules = dto.Schedules.Select(scheduleDto =>
            {
                var schedule = _mapper.Map<ClassGroupSchedule>(scheduleDto);
                schedule.ClassGroupId = classGroup.Id; // Sẽ được set sau khi save
                _auditService.SetAuditFieldsForCreate(schedule);
                return schedule;
            }).ToList();
        }
        
        _auditService.SetAuditFieldsForCreate(classGroup);
        
        await _repository.AddAsync(classGroup);
        
        // Load lại với schedules để return
        var createdClassGroup = await _repository.GetByIdAsync(classGroup.Id);
        return _mapper.Map<ClassGroupDto>(createdClassGroup);
    }

    public async Task<ClassGroupDto> UpdateAsync(UpdateClassGroupDto dto)
    {
        var classGroup = await _repository.GetByIdAsync(dto.Id);
        if (classGroup == null || classGroup.IsDeleted)
        {
            throw new NotFoundException(ErrorMessages.ClassGroup.NotFound);
        }

        if (!string.IsNullOrEmpty(dto.Name) && 
            await _repository.IsExistsAsync(dto.Name, dto.Grade, dto.AcademicYearId, dto.Id))
        {
            throw new BadRequestException(ErrorMessages.ClassGroup.NameAlreadyExists);
        }

        _mapper.Map(dto, classGroup);
        
        // Xử lý schedules nếu có (thay thế toàn bộ lịch cũ)
        if (dto.Schedules != null)
        {
            // Soft delete các schedules cũ
            if (classGroup.Schedules != null)
            {
                foreach (var oldSchedule in classGroup.Schedules.Where(s => !s.IsDeleted))
                {
                    _auditService.SetAuditFieldsForDelete(oldSchedule);
                }
            }

            // Thêm schedules mới
            classGroup.Schedules ??= new List<ClassGroupSchedule>();
            foreach (var scheduleDto in dto.Schedules)
            {
                var schedule = _mapper.Map<ClassGroupSchedule>(scheduleDto);
                schedule.ClassGroupId = classGroup.Id;
                _auditService.SetAuditFieldsForCreate(schedule);
                classGroup.Schedules.Add(schedule);
            }
        }
        
        _auditService.SetAuditFieldsForUpdate(classGroup);
        
        await _repository.UpdateAsync(classGroup);
        
        // Load lại với schedules để return
        var updatedClassGroup = await _repository.GetByIdAsync(classGroup.Id);
        return _mapper.Map<ClassGroupDto>(updatedClassGroup);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var classGroup = await _repository.GetByIdAsync(id);
        if (classGroup == null || classGroup.IsDeleted)
            return false;

        // Soft delete
        _auditService.SetAuditFieldsForDelete(classGroup);
        
        await _repository.UpdateAsync(classGroup);
        return true;
    }

    public async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
    {
        return await _repository.IsNameExistsAsync(name, excludeId);
    }

    public async Task<bool> IsExistsAsync(string name, int? grade, int? academicYearId, int? excludeId = null)
    {
        return await _repository.IsExistsAsync(name, grade, academicYearId, excludeId);
    }

    public async Task<IEnumerable<ClassGroupDto>> GetClassesByFilterAsync(ClassGroupFilterDto filter)
    {
        var classGroups = await _repository.GetByFilterAsync(
            name: filter.Name,
            grade: filter.Grade,
            academicYearId: filter.AcademicYearId,
            isDeleted: filter.IsDeleted
        );

        (string alpha, int number) SplitNameParts(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return (string.Empty, 0);
            var letters = new List<char>();
            var digits = new List<char>();
            foreach (var ch in name)
            {
                if (char.IsLetter(ch) && digits.Count == 0) letters.Add(char.ToLowerInvariant(ch));
                else if (char.IsDigit(ch)) digits.Add(ch);
                else break;
            }
            var alpha = new string(letters.ToArray());
            var number = int.TryParse(new string(digits.ToArray()), out var n) ? n : 0;
            return (alpha, number);
        }

        classGroups = classGroups
            .OrderBy(c => c.Grade)
            .ThenBy(c => SplitNameParts(c.Name).alpha)
            .ThenBy(c => SplitNameParts(c.Name).number)
            .ThenBy(c => c.Name);

        return _mapper.Map<IEnumerable<ClassGroupDto>>(classGroups);
    }

    public async Task<IEnumerable<ClassGroupDto>> GetWithoutAcademicYearAsync()
    {
        var classGroups = await _repository.GetWithoutAcademicYearAsync();
        return _mapper.Map<IEnumerable<ClassGroupDto>>(classGroups);
    }

    public async Task<IEnumerable<ClassGroupDto>> GetWithoutGradeAsync()
    {
        var classGroups = await _repository.GetWithoutGradeAsync();
        return _mapper.Map<IEnumerable<ClassGroupDto>>(classGroups);
    }

    public async Task<ClassGroupDetailDto?> GetDetailByIdAsync(int id)
    {
        var classGroup = await _repository.GetByIdAsync(id);
        if (classGroup == null || classGroup.IsDeleted)
            return null;

        var homeroomTeacher = await _repository.GetHomeroomTeacherAsync(id);
        
        // Map schedules
        var schedules = classGroup.Schedules != null 
            ? classGroup.Schedules.Where(s => !s.IsDeleted)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.Period)
                .Select(s => _mapper.Map<ClassGroupScheduleDto>(s))
                .ToList()
            : null;
        
        return new ClassGroupDetailDto
        {
            Id = classGroup.Id,
            Name = classGroup.Name,
            Description = classGroup.Description,
            Grade = classGroup.Grade,
            AcademicYearId = classGroup.AcademicYearId,
            AcademicYearName = classGroup.AcademicYears?.Name,
            CurrentStudentCount = classGroup.ClassGroupMembers?.Count ?? 0,
            HomeroomTeacher = homeroomTeacher != null ? new HomeroomTeacherDto
            {
                Id = homeroomTeacher.Id,
                FirstName = homeroomTeacher.FirstName,
                LastName = homeroomTeacher.LastName,
                Email = homeroomTeacher.Email
            } : null,
            CreatedAt = classGroup.CreatedAt,
            UpdatedAt = classGroup.UpdatedAt,
            IsDeleted = classGroup.IsDeleted,
            Schedules = schedules
        };
    }

    public async Task<IEnumerable<ClassGroupStudentDto>> GetStudentsInClassAsync(int classGroupId, string? sortBy = null, string? sortOrder = "asc")
    {
        var students = await _repository.GetStudentsInClassAsync(classGroupId);
        
        // Apply sorting at database level for better performance
        var sortedStudents = students.AsQueryable();
        
        if (!string.IsNullOrEmpty(sortBy))
        {
            sortedStudents = sortBy.ToLower() switch
            {
                "name" => sortOrder?.ToLower() == "desc" 
                    ? sortedStudents.OrderByDescending(s => s.LastName).ThenByDescending(s => s.FirstName)
                    : sortedStudents.OrderBy(s => s.LastName).ThenBy(s => s.FirstName),
                "email" => sortOrder?.ToLower() == "desc"
                    ? sortedStudents.OrderByDescending(s => s.Email)
                    : sortedStudents.OrderBy(s => s.Email),
                "studentcode" => sortOrder?.ToLower() == "desc"
                    ? sortedStudents.OrderByDescending(s => s.StudentProfile != null ? s.StudentProfile.StudentNumber : "")
                    : sortedStudents.OrderBy(s => s.StudentProfile != null ? s.StudentProfile.StudentNumber : ""),
                _ => sortedStudents.OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
            };
        }
        else
        {
            sortedStudents = sortedStudents.OrderBy(s => s.LastName).ThenBy(s => s.FirstName);
        }
        
        return sortedStudents.Select(s => new ClassGroupStudentDto
        {
            Id = s.Id,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Email = s.Email,
            Birthdate = s.Birthdate,
            StudentCode = s.StudentProfile != null ? s.StudentProfile.StudentNumber : null
        });
    }

    public async Task<AddStudentToClassResponseDto> AddStudentToClassAsync(int classGroupId, AddStudentToClassDto dto)
    {
        var targetClass = await _repository.GetByIdAsync(classGroupId);
        if (targetClass == null)
        {
            return new AddStudentToClassResponseDto
            {
                Success = false,
                Message = "Không tìm thấy lớp học này."
            };
        }

        var student = await _repository.GetStudentByEmailAsync(dto.Email);
        if (student == null)
        {
            return new AddStudentToClassResponseDto
            {
                Success = false,
                Message = $"Không tìm thấy học sinh với email: {dto.Email}. Vui lòng kiểm tra lại email hoặc học sinh chưa được đăng ký trong hệ thống."
            };
        }

        var isAlreadyInClass = await _repository.IsStudentInClassAsync(classGroupId, student.Id);
        if (isAlreadyInClass)
        {
            return new AddStudentToClassResponseDto
            {
                Success = false,
                Message = $"Học sinh {student.FirstName} {student.LastName} đã có trong lớp này."
            };
        }

        if (targetClass.AcademicYearId.HasValue)
        {
            var currentClassInSameAcademicYear = await _repository.GetStudentCurrentClassInSameAcademicYearAsync(student.Id, targetClass.AcademicYearId.Value);
            if (currentClassInSameAcademicYear != null)
            {
                return new AddStudentToClassResponseDto
                {
                    Success = false,
                    Message = $"Học sinh {student.FirstName} {student.LastName} đã có trong lớp {currentClassInSameAcademicYear.Grade.Value}{currentClassInSameAcademicYear.Name}",
                    CurrentClassName = currentClassInSameAcademicYear.Name,
                    CurrentClassId = currentClassInSameAcademicYear.Id
                };
            }
        }

        var result = await _repository.AddStudentToClassAsync(classGroupId, student.Id);
        if (result)
        {
            return new AddStudentToClassResponseDto
            {
                Success = true,
                Message = $"Đã thêm học sinh {student.FirstName} {student.LastName} vào lớp thành công."
            };
        }
        else
        {
            return new AddStudentToClassResponseDto
            {
                Success = false,
                Message = "Có lỗi xảy ra khi thêm học sinh vào lớp. Vui lòng thử lại."
            };
        }
    }

    public async Task<bool> RemoveStudentFromClassAsync(int classGroupId, int studentId)
    {
        return await _repository.RemoveStudentFromClassAsync(classGroupId, studentId);
    }

    public async Task<IEnumerable<AcademicYearDto>> GetAllAcademicYearsAsync()
    {
        var academicYears = await _repository.GetAllAcademicYearsAsync();
        return academicYears.Select(ay => new AcademicYearDto
        {
            Id = ay.Id,
            Name = ay.Name,
            StartDate = ay.StartDate,
            EndDate = ay.EndDate,
            IsCurrent = ay.IsCurrent
        });
    }

    public async Task<AcademicYearDto?> GetCurrentAcademicYearAsync()
    {
        var currentAcademicYear = await _repository.GetCurrentAcademicYearAsync();
        if (currentAcademicYear == null) return null;

        return new AcademicYearDto
        {
            Id = currentAcademicYear.Id,
            Name = currentAcademicYear.Name,
            StartDate = currentAcademicYear.StartDate,
            EndDate = currentAcademicYear.EndDate,
            IsCurrent = currentAcademicYear.IsCurrent
        };
    }

    public async Task<CurrentClassDto?> GetCurrentClassByUserIdAsync(int userId)
    {
        var classGroup = await _repository.GetCurrentClassByUserIdAsync(userId);
        if (classGroup == null) return null;

        // Lấy thông tin giáo viên chủ nhiệm
        var homeroomTeacher = await _repository.GetHomeroomTeacherAsync(classGroup.Id);
        
        // Lấy số lượng học sinh
        var studentCount = await _repository.GetStudentCountAsync(classGroup.Id);

        // Xác định role của user
        var userRole = homeroomTeacher?.Id == userId ? "Teacher" : "Student";

        // Map schedules
        var schedules = classGroup.Schedules != null 
            ? classGroup.Schedules.Where(s => !s.IsDeleted)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.Period)
                .Select(s => _mapper.Map<ClassGroupScheduleDto>(s))
                .ToList()
            : null;

        return new CurrentClassDto
        {
            Id = classGroup.Id,
            Name = classGroup.Name,
            Grade = classGroup.Grade,
            AcademicYear = new AcademicYearDto
            {
                Id = classGroup.AcademicYears?.Id ?? 0,
                Name = classGroup.AcademicYears?.Name ?? string.Empty,
                StartDate = classGroup.AcademicYears?.StartDate ?? DateTime.MinValue,
                EndDate = classGroup.AcademicYears?.EndDate ?? DateTime.MinValue,
                IsCurrent = classGroup.AcademicYears?.IsCurrent ?? false
            },
            HomeroomTeacher = homeroomTeacher != null ? new HomeroomTeacherDto
            {
                Id = homeroomTeacher.Id,
                FirstName = homeroomTeacher.FirstName,
                LastName = homeroomTeacher.LastName,
                Email = homeroomTeacher.Email
            } : null,
            StudentCount = studentCount,
            UserRole = userRole,
            JoinedAt = userRole == "Student" ? DateTime.UtcNow : null, // TODO: Lấy từ ClassGroupMember
            AssignedAt = userRole == "Teacher" ? DateTime.UtcNow : null, // TODO: Lấy từ ClassGroup
            Schedules = schedules
        };
    }

    public async Task<AssignHomeroomTeacherResponseDto> AssignHomeroomTeacherAsync(int classGroupId, AssignHomeroomTeacherDto dto)
    {
        var targetClass = await _repository.GetByIdAsync(classGroupId);
        if (targetClass == null)
        {
            return new AssignHomeroomTeacherResponseDto
            {
                Success = false,
                Message = "Không tìm thấy lớp học này."
            };
        }

        var teacher = await _repository.GetTeacherByEmailAsync(dto.Email);
        if (teacher == null)
        {
            return new AssignHomeroomTeacherResponseDto
            {
                Success = false,
                Message = $"Không tìm thấy giáo viên với email: {dto.Email}. Vui lòng kiểm tra lại email hoặc giáo viên chưa được đăng ký trong hệ thống."
            };
        }

        if (targetClass.AcademicYearId.HasValue)
        {
            var currentClassInSameAcademicYear = await _repository.GetTeacherCurrentHomeroomClassInSameAcademicYearAsync(teacher.Id, targetClass.AcademicYearId.Value);
            if (currentClassInSameAcademicYear != null)
            {
                return new AssignHomeroomTeacherResponseDto
                {
                    Success = false,
                    Message = $"Giáo viên {teacher.FirstName} {teacher.LastName} đã là giáo viên chủ nhiệm lớp {currentClassInSameAcademicYear.Grade.Value}{currentClassInSameAcademicYear.Name}",
                    CurrentClassName = currentClassInSameAcademicYear.Name,
                    CurrentClassId = currentClassInSameAcademicYear.Id
                };
            }
        }

        var result = await _repository.AssignHomeroomTeacherAsync(classGroupId, teacher.Id);
        if (result)
        {
            return new AssignHomeroomTeacherResponseDto
            {
                Success = true,
                Message = $"Đã thêm giáo viên {teacher.FirstName} {teacher.LastName} làm chủ nhiệm lớp thành công."
            };
        }
        else
        {
            return new AssignHomeroomTeacherResponseDto
            {
                Success = false,
                Message = "Có lỗi xảy ra khi thêm giáo viên chủ nhiệm. Vui lòng thử lại."
            };
        }
    }

    public async Task<bool> RemoveHomeroomTeacherAsync(int classGroupId)
    {
        return await _repository.RemoveHomeroomTeacherAsync(classGroupId);
    }

    public async Task<UserDto?> GetHomeroomTeacherAsync(int classGroupId)
    {
        var teacher = await _repository.GetHomeroomTeacherAsync(classGroupId);
        if (teacher == null)
            return null;

        return new UserDto
        {
            Id = teacher.Id,
            Username = teacher.Username,
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            Email = teacher.Email,
            PhoneNumber = teacher.PhoneNumber,
            Role = teacher.Role,
            AvatarUrl = teacher.AvatarUrl,
            Status = teacher.Status
        };
    }

    public async Task<Dictionary<int, UserDto?>> GetHomeroomTeachersAsync(IEnumerable<int> classGroupIds)
    {
        var teachers = await _repository.GetHomeroomTeachersAsync(classGroupIds);
        
        return teachers.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value == null ? null : new UserDto
            {
                Id = kvp.Value.Id,
                Username = kvp.Value.Username,
                FirstName = kvp.Value.FirstName,
                LastName = kvp.Value.LastName,
                Email = kvp.Value.Email,
                PhoneNumber = kvp.Value.PhoneNumber,
                Role = kvp.Value.Role,
                AvatarUrl = kvp.Value.AvatarUrl,
                Status = kvp.Value.Status
            }
        );
    }
}
