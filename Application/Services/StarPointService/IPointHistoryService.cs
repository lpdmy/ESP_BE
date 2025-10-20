using EduShpere.Application.DTOs.StarPointDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.Services.StarPointService
{
    public interface IPointHistoryService
    {
        Task<List<PointHistoryDto>> GetUserHistoryAsync();
        Task<PointHistoryDto> CreateHistoryAsync(CreatePointHistoryDto dto);
        Task<UserPointsDto> GetCurrentUserPoints(int userId);
    }
}
