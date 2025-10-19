using EduShpere.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs.StarPointDto
{
    public class CreatePointHistoryDto
    {
        public PointActionType ActionType { get; set; }
        public int Points { get; set; }
        public string Description { get; set; }
    }
}
