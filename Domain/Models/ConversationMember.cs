using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("ConversationId", "UserId", Name = "ConversationMembers_index_13", IsUnique = true)]
public partial class ConversationMember
{
    [Key]
    public int Id { get; set; }

    public int ConversationId { get; set; }

    public int UserId { get; set; }

    public DateTime? JoinedAt { get; set; }

    public DateTime? LastReadAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ConversationId")]
    [InverseProperty("ConversationMembers")]
    public virtual Conversation Conversation { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("ConversationMembers")]
    public virtual User User { get; set; } = null!;
}
