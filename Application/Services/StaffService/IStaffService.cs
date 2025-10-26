using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs;

namespace EduShpere.Application.Services
{
    public interface IStaffService
    {
        Task<PaginationResponseDto<UserResponseDto>> GetAllAsync(
    PaginationRequestDto paginationRequest);
        Task<UserResponseDto?> GetByIdAsync(int id);
        Task<UserResponseDto> UpdateStaff(UpdateStaffDto dto);
        Task DeleteSoft(int id);
        Task RecoveryStaff(int id);
    }
}
