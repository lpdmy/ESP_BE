using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class Friend
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int FriendId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("FriendId")]
    [InverseProperty("FriendFriendNavigations")]
    public virtual User FriendNavigation { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("FriendUsers")]
    public virtual User User { get; set; } = null!;
}
