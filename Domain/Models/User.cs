using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class User
{
    [Key]
    public int Id { get; set; }

    public long? SchoolId { get; set; }

    [StringLength(255)]
    public string? Username { get; set; }

    [StringLength(255)]
    public string? FirstName { get; set; }

    [StringLength(255)]
    public string? LastName { get; set; }

    public UserRole? Role { get; set; }

    [StringLength(1000)]
    public string? AvatarUrl { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAtLegacy { get; set; }

    [StringLength(320)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? PhoneNumber { get; set; }

    public DateTime? Birthdate { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(255)]
    public string? Password { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    [InverseProperty("User")]
    public virtual ICollection<ActivityParticipant> ActivityParticipants { get; set; } = new List<ActivityParticipant>();

    [InverseProperty("User")]
    public virtual ICollection<ClassGroupMember> ClassGroupMembers { get; set; } = new List<ClassGroupMember>();

    [InverseProperty("User")]
    public virtual ICollection<ClubJoinRequest> ClubJoinRequests { get; set; } = new List<ClubJoinRequest>();

    [InverseProperty("User")]
    public virtual ICollection<ClubMember> ClubMembers { get; set; } = new List<ClubMember>();

    [InverseProperty("CreatedByUser")]
    public virtual ICollection<Club> Clubs { get; set; } = new List<Club>();

    [InverseProperty("User")]
    public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();

    [InverseProperty("User")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("User")]
    public virtual ICollection<ConversationMember> ConversationMembers { get; set; } = new List<ConversationMember>();

    [InverseProperty("User")]
    public virtual ICollection<FavoriteCollection> FavoriteCollections { get; set; } = new List<FavoriteCollection>();

    [InverseProperty("FollowedUser")]
    public virtual ICollection<Follow> FollowFollowedUsers { get; set; } = new List<Follow>();

    [InverseProperty("FollowingUser")]
    public virtual ICollection<Follow> FollowFollowingUsers { get; set; } = new List<Follow>();

    [InverseProperty("FriendNavigation")]
    public virtual ICollection<Friend> FriendFriendNavigations { get; set; } = new List<Friend>();

    [InverseProperty("FromUser")]
    public virtual ICollection<FriendRequest> FriendRequestFromUsers { get; set; } = new List<FriendRequest>();

    [InverseProperty("ToUser")]
    public virtual ICollection<FriendRequest> FriendRequestToUsers { get; set; } = new List<FriendRequest>();

    [InverseProperty("User")]
    public virtual ICollection<Friend> FriendUsers { get; set; } = new List<Friend>();

    [InverseProperty("User")]
    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();

    [InverseProperty("User")]
    public virtual ICollection<PostInterest> PostInterests { get; set; } = new List<PostInterest>();

    [InverseProperty("User")]
    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    [InverseProperty("Reporter")]
    public virtual ICollection<PostReport> PostReports { get; set; } = new List<PostReport>();

    [InverseProperty("User")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    [InverseProperty("User")]
    public virtual ICollection<RewardRedemptionLog> RewardRedemptionLogs { get; set; } = new List<RewardRedemptionLog>();

    [ForeignKey("SchoolId")]
    [InverseProperty("Users")]
    public virtual School? School { get; set; }

    [InverseProperty("User")]
    public virtual StudentProfile? StudentProfile { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<SubmissionVote> SubmissionVotes { get; set; } = new List<SubmissionVote>();

    [InverseProperty("User")]
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    [InverseProperty("User")]
    public virtual TeacherProfile? TeacherProfile { get; set; }

    [InverseProperty("Blocked")]
    public virtual ICollection<UserBlock> UserBlockBlockeds { get; set; } = new List<UserBlock>();

    [InverseProperty("Blocker")]
    public virtual ICollection<UserBlock> UserBlockBlockers { get; set; } = new List<UserBlock>();

    [InverseProperty("User")]
    public virtual UserPoint? UserPoint { get; set; }
}
