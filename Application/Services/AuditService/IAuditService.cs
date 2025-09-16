using EduShpere.Domain.Models;

namespace EduShpere.Application.Services
{
    public interface IAuditService
    {
        void SetAuditFieldsForCreate(BaseEntity entity, int? userId = null);
        void SetAuditFieldsForUpdate(BaseEntity entity, int? userId = null);
        void SetAuditFieldsForDelete(BaseEntity entity, int? userId = null);
    }
}
