using EduShpere.Application.DTOs.CommonDto;

namespace EduShpere.Application.Services
{
    public interface IPaginationService
    {
        Task<PaginationResponseDto<T>> GetPagedResultAsync<T>(
            IQueryable<T> query,
            PaginationRequestDto paginationRequest) where T : class;
    }
}
