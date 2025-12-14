using AutoMapper;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace EduShpere.Application.Services;

public class ActivityTemplateService : IActivityTemplateService
{
    private readonly IActivityTemplateRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAuditService _auditService;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ActivityTemplateService(
        IActivityTemplateRepository repository,
        IMapper mapper,
        IAuditService auditService)
    {
        _repository = repository;
        _mapper = mapper;
        _auditService = auditService;
    }

    public async Task<IEnumerable<ActivityTemplateDto>> GetAllAsync()
    {
        try
        {
            var templates = await _repository.GetAllAsync();
            return templates.Select(MapToDto);
        }
        catch (Exception ex)
        {
            // Log error và trả về empty list nếu có lỗi (ví dụ: bảng chưa tồn tại)
            Console.WriteLine($"Error getting templates: {ex.Message}");
            return new List<ActivityTemplateDto>();
        }
    }

    public async Task<ActivityTemplateDto?> GetByIdAsync(int id)
    {
        var template = await _repository.GetByIdAsync(id);
        return template != null ? MapToDto(template) : null;
    }

    public async Task<IEnumerable<ActivityTemplateDto>> GetBySubTypeAsync(string subType)
    {
        var templates = await _repository.GetBySubTypeAsync(subType);
        return templates.Select(MapToDto);
    }

    public async Task<ActivityTemplateDto> CreateAsync(CreateActivityTemplateDto dto)
    {
        var template = new ActivityTemplate
        {
            Name = dto.Name,
            Description = dto.Description,
            SubType = dto.SubType,
            PrefillData = dto.PrefillData != null ? JsonSerializer.Serialize(dto.PrefillData, JsonOptions) : null,
            Checklist = dto.Checklist != null ? JsonSerializer.Serialize(dto.Checklist, JsonOptions) : null,
            IsSystemTemplate = false,
            UsageCount = 0
        };

        _auditService.SetAuditFieldsForCreate(template);
        await _repository.AddAsync(template);

        return MapToDto(template);
    }

    public async Task<ActivityTemplateDto> UpdateAsync(UpdateActivityTemplateDto dto)
    {
        var template = await _repository.GetByIdAsync(dto.Id);
        if (template == null)
            throw new NotFoundException("Mẫu hoạt động không tồn tại.");

        if (template.IsSystemTemplate)
            throw new BadRequestException("Không thể chỉnh sửa mẫu hệ thống.");

        template.Name = dto.Name;
        template.Description = dto.Description;
        template.SubType = dto.SubType;
        template.PrefillData = dto.PrefillData != null ? JsonSerializer.Serialize(dto.PrefillData, JsonOptions) : null;
        template.Checklist = dto.Checklist != null ? JsonSerializer.Serialize(dto.Checklist, JsonOptions) : null;

        _auditService.SetAuditFieldsForUpdate(template);
        await _repository.UpdateAsync(template);

        return MapToDto(template);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var template = await _repository.GetByIdAsync(id);
        if (template == null)
            return false;

        if (template.IsSystemTemplate)
            throw new BadRequestException("Không thể xóa mẫu hệ thống.");

        _auditService.SetAuditFieldsForDelete(template);
        await _repository.UpdateAsync(template);

        return true;
    }

    public async Task IncrementUsageCountAsync(int id)
    {
        await _repository.IncrementUsageCountAsync(id);
    }

    private ActivityTemplateDto MapToDto(ActivityTemplate template)
    {
        var dto = new ActivityTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            SubType = template.SubType,
            IsSystemTemplate = template.IsSystemTemplate,
            UsageCount = template.UsageCount,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };

        // Deserialize PrefillData
        if (!string.IsNullOrEmpty(template.PrefillData))
        {
            try
            {
                dto.PrefillData = JsonSerializer.Deserialize<Dictionary<string, object>>(template.PrefillData, JsonOptions);
            }
            catch
            {
                dto.PrefillData = null;
            }
        }

        // Deserialize Checklist
        if (!string.IsNullOrEmpty(template.Checklist))
        {
            try
            {
                dto.Checklist = JsonSerializer.Deserialize<List<string>>(template.Checklist, JsonOptions);
            }
            catch
            {
                dto.Checklist = null;
            }
        }

        return dto;
    }
}

