using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs;

namespace EduShpere.Application.Services
{
    public interface IJuryService
    {
        Task<List<JuryActivityResponseDto>> GetAllJuryByActivityIdAsync(int activityId, string? search);
        Task<List<JuryActivityResponseDto>> CreateJury(CreateJuryDto dto);
        Task DeletedJury(int id);
    }
}
