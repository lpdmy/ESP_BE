using EduShpere.Domain.Models;

namespace EduShpere.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly ICurrentUserService _currentUserService;

        public AuditService(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public void SetAuditFieldsForCreate(BaseEntity entity, int? userId = null)
        {
            var currentUserId = userId ?? GetCurrentUserId();
            var now = DateTime.UtcNow;

            entity.CreatedAt = now;
            entity.CreatedBy = currentUserId;
            entity.UpdatedAt = now;
            entity.UpdatedBy = currentUserId;
            entity.IsDeleted = false;
        }

        public void SetAuditFieldsForUpdate(BaseEntity entity, int? userId = null)
        {
            var currentUserId = userId ?? GetCurrentUserId();
            var now = DateTime.UtcNow;

            entity.UpdatedAt = now;
            entity.UpdatedBy = currentUserId;
        }

        public void SetAuditFieldsForDelete(BaseEntity entity, int? userId = null)
        {
            var currentUserId = userId ?? GetCurrentUserId();
            var now = DateTime.UtcNow;

            entity.UpdatedAt = now;
            entity.UpdatedBy = currentUserId;
            entity.IsDeleted = true;
        }

        private int? GetCurrentUserId()
        {
            try
            {
                return _currentUserService.GetCurrentUserId();
            }
            catch
            {
                return null; // Return null if user is not authenticated
            }
        }
    }
}
