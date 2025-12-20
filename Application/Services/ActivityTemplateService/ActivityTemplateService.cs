using AutoMapper;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using System.Linq;
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

    public async Task<ActivityTemplateDto> CreateFromFormDataAsync(CreateActivityTemplateFromFormDto dto)
    {
        // Chuyển đổi form data thành PrefillData dictionary
        var prefillData = new Dictionary<string, object>();

        // Basic info
        if (!string.IsNullOrEmpty(dto.Title)) prefillData["title"] = dto.Title;
        if (!string.IsNullOrEmpty(dto.Description)) prefillData["description"] = dto.Description;
        prefillData["category"] = (int)dto.Category;
        prefillData["subType"] = dto.SubType;
        if (!string.IsNullOrEmpty(dto.Location)) prefillData["location"] = dto.Location;
        if (!string.IsNullOrEmpty(dto.Organizer)) prefillData["organizer"] = dto.Organizer;
        if (!string.IsNullOrEmpty(dto.ThumbnailUrl)) prefillData["thumbnailUrl"] = dto.ThumbnailUrl;

        // Dates
        if (dto.StartDate.HasValue) prefillData["startDate"] = dto.StartDate.Value;
        if (dto.EndDate.HasValue) prefillData["endDate"] = dto.EndDate.Value;
        if (dto.RegisterDate.HasValue) prefillData["registerDate"] = dto.RegisterDate.Value;
        if (dto.EndRegisterDate.HasValue) prefillData["endRegisterDate"] = dto.EndRegisterDate.Value;
        if (dto.SubmissionDeadline.HasValue) prefillData["submissionDeadline"] = dto.SubmissionDeadline.Value;

        // Participants
        if (dto.MaxParticipants.HasValue) prefillData["maxParticipants"] = dto.MaxParticipants.Value;
        if (dto.OnlyTeacherCanRegister.HasValue) prefillData["onlyTeacherCanRegister"] = dto.OnlyTeacherCanRegister.Value;

        // SportsFestival
        if (!string.IsNullOrEmpty(dto.CompetitionType)) prefillData["competitionType"] = dto.CompetitionType;
        if (dto.SportsCategories != null && dto.SportsCategories.Any()) prefillData["sportsCategories"] = dto.SportsCategories;
        if (dto.SportsConfigurations != null && dto.SportsConfigurations.Any()) prefillData["sportsConfigurations"] = dto.SportsConfigurations;

        // CreativeContest
        if (!string.IsNullOrEmpty(dto.Theme)) prefillData["theme"] = dto.Theme;
        if (!string.IsNullOrEmpty(dto.Genre)) prefillData["genre"] = dto.Genre;
        if (!string.IsNullOrEmpty(dto.PaperSize)) prefillData["paperSize"] = dto.PaperSize;
        if (!string.IsNullOrEmpty(dto.DrawingMedium)) prefillData["drawingMedium"] = dto.DrawingMedium;
        if (!string.IsNullOrEmpty(dto.TimeLimit)) prefillData["timeLimit"] = dto.TimeLimit;
        if (!string.IsNullOrEmpty(dto.SubmissionFormat)) prefillData["submissionFormat"] = dto.SubmissionFormat;

        // Problem/Submission
        if (!string.IsNullOrEmpty(dto.ProblemText)) prefillData["problemText"] = dto.ProblemText;
        if (!string.IsNullOrEmpty(dto.ProblemFileUrl)) prefillData["problemFileUrl"] = dto.ProblemFileUrl;

        // Settings
        if (dto.IsGrade.HasValue) prefillData["isGrade"] = dto.IsGrade.Value;
        if (!string.IsNullOrEmpty(dto.GradingSettings)) prefillData["gradingSettings"] = dto.GradingSettings;
        if (!string.IsNullOrEmpty(dto.RegistrationSettings)) prefillData["registrationSettings"] = dto.RegistrationSettings;
        if (!string.IsNullOrEmpty(dto.StarPointRewards)) prefillData["starPointRewards"] = dto.StarPointRewards;

        // Collections
        if (dto.Rules != null && dto.Rules.Any()) prefillData["rules"] = dto.Rules;
        if (dto.Speakers != null && dto.Speakers.Any()) prefillData["speakers"] = dto.Speakers;
        if (dto.ProgramItems != null && dto.ProgramItems.Any()) prefillData["programItems"] = dto.ProgramItems;

        var template = new ActivityTemplate
        {
            Name = dto.TemplateName,
            Description = dto.TemplateDescription,
            SubType = dto.SubType,
            PrefillData = prefillData.Any() ? JsonSerializer.Serialize(prefillData, JsonOptions) : null,
            Checklist = dto.Checklist != null && dto.Checklist.Any() ? JsonSerializer.Serialize(dto.Checklist, JsonOptions) : null,
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

