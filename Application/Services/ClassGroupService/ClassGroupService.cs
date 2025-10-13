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
            StartYear = c.StartYear,
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
                    StartYear = c.StartYear,
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
        // Kiểm tra tên lớp đã tồn tại chưa
        if (!string.IsNullOrEmpty(dto.Name) && await _repository.IsNameExistsAsync(dto.Name))
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

        // Kiểm tra tên lớp đã tồn tại chưa (trừ chính nó)
        if (!string.IsNullOrEmpty(dto.Name) && 
            await _repository.IsNameExistsAsync(dto.Name, dto.Id))
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

    public async Task<IEnumerable<ClassGroupDto>> GetClassesByFilterAsync(ClassGroupFilterDto filter)
    {
        var classGroups = await _repository.GetByFilterAsync(
            name: filter.Name,
            grade: filter.Grade,
            startYear: filter.StartYear,
            isDeleted: filter.IsDeleted
        );
        return _mapper.Map<IEnumerable<ClassGroupDto>>(classGroups);
    }

    public async Task<IEnumerable<ClassGroupDto>> GetWithoutStartYearAsync()
    {
        var classGroups = await _repository.GetWithoutStartYearAsync();
        return _mapper.Map<IEnumerable<ClassGroupDto>>(classGroups);
    }

    public async Task<IEnumerable<ClassGroupDto>> GetWithoutGradeAsync()
    {
        var classGroups = await _repository.GetWithoutGradeAsync();
        return _mapper.Map<IEnumerable<ClassGroupDto>>(classGroups);
    }
}
