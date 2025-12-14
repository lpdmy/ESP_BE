using AutoMapper;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EduShpere.Application.Services
{
    public class ActivityDraftService : IActivityDraftService
    {
        private readonly IActivityDraftRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAuditService _auditService;
        private readonly IPaginationService _paginationService;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ActivityDraftService(
            IActivityDraftRepository repository,
            IMapper mapper,
            IAuditService auditService,
            IPaginationService paginationService)
        {
            _repository = repository;
            _mapper = mapper;
            _auditService = auditService;
            _paginationService = paginationService;
        }

        public async Task<ActivityDraftResponseDto> CreateAsync(CreateActivityDraftDto dto, int userId)
        {
            var draft = _mapper.Map<ActivityDraft>(dto);
            
            // Serialize collections to JSON
            draft.Rules = dto.Rules != null && dto.Rules.Any() 
                ? JsonSerializer.Serialize(dto.Rules, JsonOptions) 
                : null;
            draft.SportsCategories = dto.SportsCategories != null && dto.SportsCategories.Any()
                ? JsonSerializer.Serialize(dto.SportsCategories, JsonOptions)
                : null;
            draft.SportsConfigurations = dto.SportsConfigurations != null && dto.SportsConfigurations.Any()
                ? JsonSerializer.Serialize(dto.SportsConfigurations, JsonOptions)
                : null;
            draft.Speakers = dto.Speakers != null && dto.Speakers.Any()
                ? JsonSerializer.Serialize(dto.Speakers, JsonOptions)
                : null;
            draft.ProgramItems = dto.ProgramItems != null && dto.ProgramItems.Any()
                ? JsonSerializer.Serialize(dto.ProgramItems, JsonOptions)
                : null;

            _auditService.SetAuditFieldsForCreate(draft);
            draft.CreatedBy = userId;
            draft.UpdatedBy = userId;

            await _repository.AddAsync(draft);
            return _mapper.Map<ActivityDraftResponseDto>(draft);
        }

        public async Task<ActivityDraftResponseDto> UpdateAsync(UpdateActivityDraftDto dto, int userId)
        {
            var draft = await _repository.GetByIdAndUserIdAsync(dto.Id, userId);
            if (draft == null)
                throw new NotFoundException(ErrorMessages.ActivityDraft.NotFound);

            _mapper.Map(dto, draft);

            // Serialize collections to JSON
            draft.Rules = dto.Rules != null && dto.Rules.Any()
                ? JsonSerializer.Serialize(dto.Rules, JsonOptions)
                : null;
            draft.SportsCategories = dto.SportsCategories != null && dto.SportsCategories.Any()
                ? JsonSerializer.Serialize(dto.SportsCategories, JsonOptions)
                : null;
            draft.SportsConfigurations = dto.SportsConfigurations != null && dto.SportsConfigurations.Any()
                ? JsonSerializer.Serialize(dto.SportsConfigurations, JsonOptions)
                : null;
            draft.Speakers = dto.Speakers != null && dto.Speakers.Any()
                ? JsonSerializer.Serialize(dto.Speakers, JsonOptions)
                : null;
            draft.ProgramItems = dto.ProgramItems != null && dto.ProgramItems.Any()
                ? JsonSerializer.Serialize(dto.ProgramItems, JsonOptions)
                : null;

            _auditService.SetAuditFieldsForUpdate(draft);
            draft.UpdatedBy = userId;

            await _repository.UpdateAsync(draft);
            return _mapper.Map<ActivityDraftResponseDto>(draft);
        }

        public async Task<ActivityDraftResponseDto> GetByIdAsync(int id, int userId)
        {
            var draft = await _repository.GetByIdAndUserIdAsync(id, userId);
            if (draft == null)
                throw new NotFoundException(ErrorMessages.ActivityDraft.NotFound);

            var dto = _mapper.Map<ActivityDraftResponseDto>(draft);
            
            // Deserialize collections from JSON
            dto.Rules = !string.IsNullOrEmpty(draft.Rules)
                ? JsonSerializer.Deserialize<List<string>>(draft.Rules, JsonOptions)
                : new List<string>();
            dto.SportsCategories = !string.IsNullOrEmpty(draft.SportsCategories)
                ? JsonSerializer.Deserialize<List<string>>(draft.SportsCategories, JsonOptions)
                : new List<string>();
            dto.SportsConfigurations = !string.IsNullOrEmpty(draft.SportsConfigurations)
                ? JsonSerializer.Deserialize<List<ActivitySportConfigDto>>(draft.SportsConfigurations, JsonOptions)
                : new List<ActivitySportConfigDto>();
            dto.Speakers = !string.IsNullOrEmpty(draft.Speakers)
                ? JsonSerializer.Deserialize<List<ActivitySpeakerDto>>(draft.Speakers, JsonOptions)
                : new List<ActivitySpeakerDto>();
            dto.ProgramItems = !string.IsNullOrEmpty(draft.ProgramItems)
                ? JsonSerializer.Deserialize<List<ActivityProgramDto>>(draft.ProgramItems, JsonOptions)
                : new List<ActivityProgramDto>();

            return dto;
        }

        public async Task<PaginationResponseDto<ActivityDraftListItemDto>> GetAllByUserIdAsync(
            PaginationRequestDto paginationRequest, int userId)
        {
            var query = _repository.GetQueryable()
                .Where(d => d.CreatedBy == userId);

            var pagedResult = await _paginationService.GetPagedResultAsync(query, paginationRequest);

            return new PaginationResponseDto<ActivityDraftListItemDto>
            {
                Data = _mapper.Map<IEnumerable<ActivityDraftListItemDto>>(pagedResult.Data),
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var draft = await _repository.GetByIdAndUserIdAsync(id, userId);
            if (draft == null)
                throw new NotFoundException(ErrorMessages.ActivityDraft.NotFound);

            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<CreateActivityDto> ConvertDraftToActivityDtoAsync(int draftId, int userId)
        {
            var draft = await _repository.GetByIdAndUserIdAsync(draftId, userId);
            if (draft == null)
                throw new NotFoundException(ErrorMessages.ActivityDraft.NotFound);

            // Ensure required fields have default values
            var defaultDate = DateTime.UtcNow;
            var defaultEndDate = defaultDate.AddDays(1);
            
            var dto = new CreateActivityDto
            {
                Title = draft.Title ?? "Hoạt động mới",
                Description = draft.Description ?? "",
                Category = draft.Category,
                SubType = draft.SubType,
                Location = draft.Location ?? "Chưa xác định",
                Organizer = draft.Organizer ?? "Chưa xác định",
                ThumbnailUrl = draft.ThumbnailUrl ?? "",
                StartDate = draft.StartDate ?? defaultDate,
                EndDate = draft.EndDate ?? defaultEndDate,
                RegisterDate = draft.RegisterDate ?? defaultDate,
                EndRegisterDate = draft.EndRegisterDate ?? defaultDate,
                MaxParticipants = draft.MaxParticipants,
                CompetitionType = draft.CompetitionType,
                Theme = draft.Theme,
                Genre = draft.Genre,
                PaperSize = draft.PaperSize,
                DrawingMedium = draft.DrawingMedium,
                TimeLimit = draft.TimeLimit,
                SubmissionFormat = draft.SubmissionFormat,
                ProblemText = draft.ProblemText,
                ProblemFileUrl = draft.ProblemFileUrl,
                SubmissionDeadline = draft.SubmissionDeadline,
                OnlyTeacherCanRegister = draft.OnlyTeacherCanRegister ?? false,
            };

            // Deserialize JSON collections
            if (!string.IsNullOrEmpty(draft.Rules))
            {
                dto.Rules = JsonSerializer.Deserialize<List<string>>(draft.Rules, JsonOptions) ?? new List<string>();
            }
            if (!string.IsNullOrEmpty(draft.SportsCategories))
            {
                dto.SportsCategories = JsonSerializer.Deserialize<List<string>>(draft.SportsCategories, JsonOptions) ?? new List<string>();
            }
            if (!string.IsNullOrEmpty(draft.SportsConfigurations))
            {
                dto.SportsConfigurations = JsonSerializer.Deserialize<List<ActivitySportConfigDto>>(draft.SportsConfigurations, JsonOptions) ?? new List<ActivitySportConfigDto>();
            }
            if (!string.IsNullOrEmpty(draft.Speakers))
            {
                dto.Speakers = JsonSerializer.Deserialize<List<ActivitySpeakerDto>>(draft.Speakers, JsonOptions) ?? new List<ActivitySpeakerDto>();
            }
            if (!string.IsNullOrEmpty(draft.ProgramItems))
            {
                dto.ProgramItems = JsonSerializer.Deserialize<List<ActivityProgramDto>>(draft.ProgramItems, JsonOptions) ?? new List<ActivityProgramDto>();
            }
            if (!string.IsNullOrEmpty(draft.GradingSettings))
            {
                try
                {
                    dto.GradingSettings = JsonSerializer.Deserialize<GradingSettingsDto>(draft.GradingSettings, JsonOptions);
                }
                catch
                {
                    // Ignore deserialization errors
                }
            }
            if (!string.IsNullOrEmpty(draft.RegistrationSettings))
            {
                try
                {
                    dto.RegistrationSettings = JsonSerializer.Deserialize<ActivityRegistrationSettingsDto>(draft.RegistrationSettings, JsonOptions);
                }
                catch
                {
                    // Ignore deserialization errors
                }
            }
            if (!string.IsNullOrEmpty(draft.StarPointRewards))
            {
                try
                {
                    dto.StarPointRewards = JsonSerializer.Deserialize<StarPointRewardsDto>(draft.StarPointRewards, JsonOptions);
                }
                catch
                {
                    // Ignore deserialization errors
                }
            }

            return dto;
        }
    }
}

