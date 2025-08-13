using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class Conversation
{
    [Key]
    public int Id { get; set; }

    public bool IsGroup { get; set; }

    [StringLength(255)]
    public string? Title { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [InverseProperty("Conversation")]
    public virtual ICollection<ConversationMember> ConversationMembers { get; set; } = new List<ConversationMember>();
}
