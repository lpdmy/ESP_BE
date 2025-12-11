using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs.StarPointDto
{
    public class PointHistoryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } 
        public string ActionType { get; set; }
        public int Points { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
