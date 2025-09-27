using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Services
{
    public interface ICollectionService
    {
        Task<bool> UpdateCollection(UpdateCollectionDto dto);
        Task<bool> DeleteCollection(int id);
        Task<CollectionResponseDto?> CreateCollectionResponse(User user, CreateCollectionDto dto);
        Task<PaginationResponseDto<CollectionResponseDto>> GetAllCollectionByUserAsync(
    User user,
    PaginationRequestDto paginationRequest);
        Task<CollectionItem?> AddCollectionIteam(AddCollectionIteamDto dto, User user);
    }
}
