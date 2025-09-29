using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Application.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly ICollectionRepository _repo;
        private readonly ICollectionIteamRepository _iteamRepo;
        private readonly IMapper _mapper;
        public CollectionService(ICollectionRepository repo , IMapper mapper, ICollectionIteamRepository iteamRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _iteamRepo = iteamRepo;
        }
        public async Task<PaginationResponseDto<CollectionResponseDto>> GetAllCollectionByUserAsync(
    User user,
    PaginationRequestDto paginationRequest)
        {
            var query = _repo.GetByUserIdQuery(user);
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<CollectionResponseDto>>(data);

            return new PaginationResponseDto<CollectionResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }

        public async Task<CollectionResponseDto?> CreateCollectionResponse(User user,CreateCollectionDto dto)
        {
            var collection = new FavoriteCollection
            {
                Name = dto.Name,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repo.AddAsync(collection);
            return _mapper.Map<FavoriteCollection,CollectionResponseDto>(collection);
        }
        public async Task<bool> DeleteCollection(int id)
        {
            var collection = await _repo.GetByIdAsync(id);
            if (collection == null || collection.IsDeleted) 
                return false;
            await _repo.DeleteSoft(id);
            return true;
        }
        public async Task<bool> UpdateCollection( UpdateCollectionDto dto)
        {
            var collection = await _repo.GetByIdAsync(dto.id);
            if (collection == null || collection.IsDeleted) return false;
            collection.Name = dto.Name;
            collection.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(collection);
            return true;
        }
        public async Task<CollectionItem?> AddCollectionIteam(AddCollectionIteamDto dto,User user)
        {
            var collection = new CollectionItem
            {
                PostId = dto.PostId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id,
                IsDeleted = false,
                CollectionId = dto.CollectionId
            };
            await _iteamRepo.AddAsync(collection);
            return collection;
        }
    }
}
