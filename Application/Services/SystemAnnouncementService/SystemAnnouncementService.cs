using AutoMapper;
using EduShpere.Application;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.SystemAnnouncementDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Infrastructure.Services;
using EduShpere.Shared.Constants;
using EduShpere.Shared;
using EduShpere.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Application.Services.SystemAnnouncementService;

public class SystemAnnouncementService : ISystemAnnouncementService
{
    private readonly IPostRepository _postRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAuditService _auditService;
    private readonly IPaginationService _paginationService;
    private readonly IMapper _mapper;
    private readonly ICloudinaryService _cloudinaryService;

    public SystemAnnouncementService(
        IPostRepository postRepository,
        IAttachmentRepository attachmentRepository,
        IAuditService auditService,
        IPaginationService paginationService,
        IMapper mapper,
        ICloudinaryService cloudinaryService)
    {
        _postRepository = postRepository;
        _attachmentRepository = attachmentRepository;
        _auditService = auditService;
        _paginationService = paginationService;
        _mapper = mapper;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<PaginationResponseDto<SystemAnnouncementListItemDto>> GetAllAsync(PaginationRequestDto paginationRequest)
    {
        try
        {
            var query = _postRepository.GetQueryable()
                .Where(p => p.IsSystemAnnouncement && !p.IsDeleted);

            var pagedResult = await _paginationService.GetPagedResultAsync(query, paginationRequest);

            var announcements = _mapper.Map<IEnumerable<SystemAnnouncementListItemDto>>(pagedResult.Data);

            return new PaginationResponseDto<SystemAnnouncementListItemDto>
            {
                Data = announcements,
                TotalCount = pagedResult.TotalCount,
                PageNumber = pagedResult.PageNumber,
                PageSize = pagedResult.PageSize
            };
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ErrorMessages.SystemAnnouncement.GetFailed);
        }
    }

    public async Task<SystemAnnouncementDetailDto?> GetByIdAsync(int id)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(id);
            
            if (post == null || !post.IsSystemAnnouncement || post.IsDeleted)
                return null;

            return _mapper.Map<SystemAnnouncementDetailDto>(post);
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ErrorMessages.SystemAnnouncement.GetFailed);
        }
    }

    public async Task<SystemAnnouncementDetailDto> CreateAsync(CreateSystemAnnouncementDto dto, int userId)
    {
        try
        {
            // Validate announcement type
            var validTypes = new[] { "exam", "urgent", "holiday", "general" };
            if (!validTypes.Contains(dto.AnnouncementType))
                throw new BadRequestException(ErrorMessages.SystemAnnouncement.InvalidAnnouncementType);

            // Validate expiry date
            if (dto.ExpiryDate.HasValue && dto.ExpiryDate.Value < DateTime.UtcNow)
                throw new BadRequestException(ErrorMessages.SystemAnnouncement.ExpiryDateInPast);

            var post = new Post
            {
                Title = dto.Title,
                Body = dto.Content,
                UserId = userId,
                PrivacyLevel = Domain.Enum.PostVisibility.Public,
                Status = Domain.Enum.PostStatus.Published,
                IsSystemAnnouncement = true,
                IsUrgent = dto.IsUrgent,
                ExpiryDate = dto.ExpiryDate,
                AnnouncementType = dto.AnnouncementType
            };

            _auditService.SetAuditFieldsForCreate(post);
            await _postRepository.AddAsync(post);

            // Handle file uploads
            if (dto.Files != null && dto.Files.Any())
            {
                await UploadAttachmentsAsync(post.Id, dto.Files);
            }

            return _mapper.Map<SystemAnnouncementDetailDto>(post);
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ErrorMessages.SystemAnnouncement.CreateFailed);
        }
    }

    public async Task<SystemAnnouncementDetailDto> UpdateAsync(UpdateSystemAnnouncementDto dto, int userId)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(dto.Id);
            
            if (post == null || !post.IsSystemAnnouncement || post.IsDeleted)
                throw new NotFoundException(ErrorMessages.SystemAnnouncement.NotFound);

            // Validate announcement type
            var validTypes = new[] { "exam", "urgent", "holiday", "general" };
            if (!validTypes.Contains(dto.AnnouncementType))
                throw new BadRequestException(ErrorMessages.SystemAnnouncement.InvalidAnnouncementType);

            // Validate expiry date
            if (dto.ExpiryDate.HasValue && dto.ExpiryDate.Value < DateTime.UtcNow)
                throw new BadRequestException(ErrorMessages.SystemAnnouncement.ExpiryDateInPast);

            post.Title = dto.Title;
            post.Body = dto.Content;
            post.IsUrgent = dto.IsUrgent;
            post.ExpiryDate = dto.ExpiryDate;
            post.AnnouncementType = dto.AnnouncementType;
            post.Status = dto.IsVisible ? Domain.Enum.PostStatus.Published : Domain.Enum.PostStatus.Pending;

            _auditService.SetAuditFieldsForUpdate(post);
            await _postRepository.UpdateAsync(post);

            // Handle file uploads
            if (dto.Files != null && dto.Files.Any())
            {
                await UploadAttachmentsAsync(post.Id, dto.Files);
            }

            return _mapper.Map<SystemAnnouncementDetailDto>(post);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ErrorMessages.SystemAnnouncement.UpdateFailed);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(id);
            
            if (post == null || !post.IsSystemAnnouncement || post.IsDeleted)
                return false;

            _auditService.SetAuditFieldsForDelete(post);
            await _postRepository.UpdateAsync(post);
            
            return true;
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ErrorMessages.SystemAnnouncement.DeleteFailed);
        }
    }

    public async Task<bool> ToggleVisibilityAsync(int id)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(id);
            
            if (post == null || !post.IsSystemAnnouncement || post.IsDeleted)
                return false;

            post.Status = post.Status == Domain.Enum.PostStatus.Published 
                ? Domain.Enum.PostStatus.Pending 
                : Domain.Enum.PostStatus.Published;

            _auditService.SetAuditFieldsForUpdate(post);
            await _postRepository.UpdateAsync(post);
            
            return true;
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ErrorMessages.SystemAnnouncement.UpdateFailed);
        }
    }

    public async Task<List<SystemAnnouncementDto>> GetPublicAnnouncementsAsync()
    {
        try
        {
            var publicAnnouncements = await _postRepository.GetQueryable()
                .Include(p => p.Attachments)
                .Where(p => p.IsSystemAnnouncement && 
                        !p.IsDeleted && 
                        p.Status == Domain.Enum.PostStatus.Published &&
                        (!p.ExpiryDate.HasValue || p.ExpiryDate.Value > DateTime.UtcNow))
                .OrderByDescending(p => p.IsUrgent)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<SystemAnnouncementDto>>(publicAnnouncements);
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ErrorMessages.SystemAnnouncement.GetFailed);
        }
    }

    public async Task<bool> MarkAsViewedAsync(int announcementId, int userId)
    {
        try
        {
            // TODO: Implement user viewed tracking if needed
            // For now, just return true
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private async Task UploadAttachmentsAsync(int postId, List<IFormFile> files)
    {
        foreach (var file in files)
        {
            try
            {
                // Validate file type
                var allowedTypes = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                
                if (!allowedTypes.Contains(fileExtension))
                    throw new BadRequestException(ErrorMessages.SystemAnnouncement.InvalidFileType);

                // Validate file size (10MB max)
                if (file.Length > 10 * 1024 * 1024)
                    throw new BadRequestException(ErrorMessages.SystemAnnouncement.FileTooLarge);

                // Upload to Cloudinary
                var uploadResult = await _cloudinaryService.UploadFileAsync(file);
                
                if (uploadResult == null)
                    throw new BadRequestException(ErrorMessages.SystemAnnouncement.FileUploadFailed);

                // Save attachment record
                var attachment = new Attachment
                {
                    PostId = postId,
                    FileUrl = uploadResult,
                    FileType = fileExtension,
                    Mime = file.ContentType,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                await _attachmentRepository.AddAsync(attachment);
            }
            catch (BadRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ErrorMessages.SystemAnnouncement.FileUploadFailed);
            }
        }
    }
}
