using AutoMapper;
using EduShpere.Application.DTOs.ClassGroupDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Infrastructure;
using EduShpere.Shared.Constants;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;
using EduShpere.Shared;

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

    public async Task<ClassGroupDashboardDto> GetDashboardDataAsync()
    {
        // Get all classes with members
        var allClasses = await _repository.GetAllAsync();
        var allClassesIncludingDeleted = await _repository.GetByFilterAsync(isDeleted: null);

        // Calculate statistics
        var statistics = new ClassGroupStatisticsDto
        {
            TotalClasses = allClasses.Count(),
            TotalStudents = allClasses.Sum(c => c.ClassGroupMembers?.Count ?? 0),
            DeletedClasses = allClassesIncludingDeleted.Count(c => c.IsDeleted)
        };

        // Group by grade
        var classesByGrade = allClasses
            .GroupBy(c => c.Grade)
            .Select(g => new ClassGroupByGradeDto
            {
                Grade = g.Key,
                GradeName = g.Key.HasValue ? $"Khối {g.Key}" : "Không xác định",
                ClassCount = g.Count(),
                StudentCount = g.Sum(c => c.ClassGroupMembers?.Count ?? 0),
                Classes = g.Select(c => new ClassGroupDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Grade = c.Grade,
                    AcademicYearId = c.AcademicYearId,
                    AcademicYearName = c.AcademicYears?.Name,
                    CurrentStudentCount = c.ClassGroupMembers?.Count ?? 0,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsDeleted = c.IsDeleted
                }).ToList()
            })
            .OrderBy(g => g.Grade)
            .ToList();

        return new ClassGroupDashboardDto
        {
            Statistics = statistics,
            ClassesByGrade = classesByGrade
        };
    }

    public async Task<ClassGroupDto> CreateAsync(CreateClassGroupDto dto)
    {
        // Kiểm tra tồn tại theo tổ hợp Name + Grade + AcademicYearId
        if (!string.IsNullOrEmpty(dto.Name) && await _repository.IsExistsAsync(dto.Name, dto.Grade, dto.AcademicYearId))
        {
            throw new BadRequestException(ErrorMessages.ClassGroup.NameAlreadyExists);
        }

        var classGroup = _mapper.Map<ClassGroup>(dto);
        
        // Set audit fields
        _auditService.SetAuditFieldsForCreate(classGroup);
        
        await _repository.AddAsync(classGroup);
        
        return _mapper.Map<ClassGroupDto>(classGroup);
    }

    public async Task<ClassGroupDto> UpdateAsync(UpdateClassGroupDto dto)
    {
        var classGroup = await _repository.GetByIdAsync(dto.Id);
        if (classGroup == null || classGroup.IsDeleted)
        {
            throw new NotFoundException(ErrorMessages.ClassGroup.NotFound);
        }

        // Kiểm tra tồn tại theo tổ hợp Name + Grade + AcademicYearId (trừ chính nó)
        if (!string.IsNullOrEmpty(dto.Name) && 
            await _repository.IsExistsAsync(dto.Name, dto.Grade, dto.AcademicYearId, dto.Id))
        {
            throw new BadRequestException(ErrorMessages.ClassGroup.NameAlreadyExists);
        }

        _mapper.Map(dto, classGroup);
        
        // Set audit fields
        _auditService.SetAuditFieldsForUpdate(classGroup);
        
        await _repository.UpdateAsync(classGroup);
        
        return _mapper.Map<ClassGroupDto>(classGroup);
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

        // Natural sort: by Grade, then alpha prefix then numeric suffix, then name fallback
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
            IsDeleted = classGroup.IsDeleted
        };
    }

    public async Task<IEnumerable<ClassGroupStudentDto>> GetStudentsInClassAsync(int classGroupId)
    {
        var students = await _repository.GetStudentsInClassAsync(classGroupId);
        return students.Select(s => new ClassGroupStudentDto
        {
            Id = s.Id,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Email = s.Email,
            Birthdate = s.Birthdate
        });
    }

    public async Task<AddStudentToClassResponseDto> AddStudentToClassAsync(int classGroupId, AddStudentToClassDto dto)
    {
        // Get the target class to determine academic year
        var targetClass = await _repository.GetByIdAsync(classGroupId);
        if (targetClass == null)
        {
            return new AddStudentToClassResponseDto
            {
                Success = false,
                Message = "Không tìm thấy lớp học này."
            };
        }

        // Find student by email
        var student = await _repository.GetStudentByEmailAsync(dto.Email);
        if (student == null)
        {
            return new AddStudentToClassResponseDto
            {
                Success = false,
                Message = $"Không tìm thấy học sinh với email: {dto.Email}. Vui lòng kiểm tra lại email hoặc học sinh chưa được đăng ký trong hệ thống."
            };
        }

        // Check if student is already in this specific class
        var isAlreadyInClass = await _repository.IsStudentInClassAsync(classGroupId, student.Id);
        if (isAlreadyInClass)
        {
            return new AddStudentToClassResponseDto
            {
                Success = false,
                Message = $"Học sinh {student.FirstName} {student.LastName} đã có trong lớp này."
            };
        }

        // Check if student is already in another class of the same grade AND same academic year
        if (targetClass.Grade.HasValue && targetClass.AcademicYearId.HasValue)
        {
            var currentClassInSameGradeAndYear = await _repository.GetStudentCurrentClassInSameGradeAndAcademicYearAsync(student.Id, targetClass.Grade.Value, targetClass.AcademicYearId.Value);
            if (currentClassInSameGradeAndYear != null)
            {
                return new AddStudentToClassResponseDto
                {
                    Success = false,
                    Message = $"Học sinh {student.FirstName} {student.LastName} đã có trong lớp {targetClass.Grade.Value}{currentClassInSameGradeAndYear.Name}",
                    CurrentClassName = currentClassInSameGradeAndYear.Name,
                    CurrentClassId = currentClassInSameGradeAndYear.Id
                };
            }
        }

        // Add student to class
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
}
