using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.CollectionDto;

namespace EduShpere.Application.DTOs
{
    public class CollectionResponseDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [StringLength(255)]
        public string? Name { get; set; }
        public DateTime? CreatedAt { get; set; } 
        public List<PostIteamDto> CollectionItems { get; set; } = new List<PostIteamDto>();
    }
}
