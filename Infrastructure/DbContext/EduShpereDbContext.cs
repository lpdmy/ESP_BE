using System;
using System.Collections.Generic;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduShpere.Infrastructure;

public partial class EduShpereDbContext : DbContext
{
    public EduShpereDbContext()
    {
    }

    public EduShpereDbContext(DbContextOptions<EduShpereDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<ActivityParticipant> ActivityParticipants { get; set; }

    public virtual DbSet<ActivityReward> ActivityRewards { get; set; }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<ClassGroup> ClassGroups { get; set; }

    public virtual DbSet<ClassGroupMember> ClassGroupMembers { get; set; }

    public virtual DbSet<Club> Clubs { get; set; }

    public virtual DbSet<ClubJoinRequest> ClubJoinRequests { get; set; }

    public virtual DbSet<ClubMember> ClubMembers { get; set; }

    public virtual DbSet<CollectionItem> CollectionItems { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<CommentLike> CommentLikes { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<ConversationMember> ConversationMembers { get; set; }

    public virtual DbSet<FavoriteCollection> FavoriteCollections { get; set; }

    public virtual DbSet<Follow> Follows { get; set; }

    public virtual DbSet<Friend> Friends { get; set; }

    public virtual DbSet<FriendRequest> FriendRequests { get; set; }

    public virtual DbSet<PointTransaction> PointTransactions { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostInterest> PostInterests { get; set; }

    public virtual DbSet<PostLike> PostLikes { get; set; }

    public virtual DbSet<PostReport> PostReports { get; set; }

    public virtual DbSet<Reward> Rewards { get; set; }

    public virtual DbSet<RewardRedemptionLog> RewardRedemptionLogs { get; set; }

    public virtual DbSet<School> Schools { get; set; }

    public virtual DbSet<StudentProfile> StudentProfiles { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<SubmissionVote> SubmissionVotes { get; set; }

    public virtual DbSet<TeacherProfile> TeacherProfiles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserBlock> UserBlocks { get; set; }

    public virtual DbSet<UserPoint> UserPoints { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnectionString");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Activiti__3214EC073E220058");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Club).WithMany(p => p.Activities).HasConstraintName("FK__Activitie__ClubI__6BE40491");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Activities)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Activitie__Creat__6AEFE058");
        });

        modelBuilder.Entity<ActivityParticipant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Activity__3214EC0707BE3B6F");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Activity).WithMany(p => p.ActivityParticipants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ActivityP__Activ__6DCC4D03");

            entity.HasOne(d => d.User).WithMany(p => p.ActivityParticipants)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ActivityP__UserI__6EC0713C");
        });

        modelBuilder.Entity<ActivityReward>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Activity__3214EC07030871D1");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Activity).WithMany(p => p.ActivityRewards)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ActivityR__Activ__6CD828CA");
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attachme__3214EC07C75D80D4");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Comment).WithMany(p => p.Attachments).HasConstraintName("FK__Attachmen__Comme__5E8A0973");

            entity.HasOne(d => d.Post).WithMany(p => p.Attachments).HasConstraintName("FK__Attachmen__PostI__5D95E53A");
        });

        modelBuilder.Entity<ClassGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClassGro__3214EC07D8340F99");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<ClassGroupMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClassGro__3214EC079279792A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.ClassGroup).WithMany(p => p.ClassGroupMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClassGrou__Class__540C7B00");

            entity.HasOne(d => d.User).WithMany(p => p.ClassGroupMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClassGrou__UserI__55009F39");
        });

        modelBuilder.Entity<Club>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clubs__3214EC07E20E6A9B");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Clubs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Clubs__CreatedBy__55F4C372");
        });

        modelBuilder.Entity<ClubJoinRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClubJoin__3214EC07FD2A1A89");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Club).WithMany(p => p.ClubJoinRequests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClubJoinR__ClubI__58D1301D");

            entity.HasOne(d => d.User).WithMany(p => p.ClubJoinRequests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClubJoinR__UserI__59C55456");
        });

        modelBuilder.Entity<ClubMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClubMemb__3214EC07E85B00FA");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Club).WithMany(p => p.ClubMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClubMembe__ClubI__56E8E7AB");

            entity.HasOne(d => d.User).WithMany(p => p.ClubMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ClubMembe__UserI__57DD0BE4");
        });

        modelBuilder.Entity<CollectionItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Collecti__3214EC079AA0869A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Collection).WithMany(p => p.CollectionItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Collectio__Colle__690797E6");

            entity.HasOne(d => d.Post).WithMany(p => p.CollectionItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Collectio__PostI__69FBBC1F");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comments__3214EC075E749776");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment).HasConstraintName("FK__Comments__Parent__625A9A57");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments).HasConstraintName("FK__Comments__PostId__6166761E");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comments__UserId__634EBE90");
        });

        modelBuilder.Entity<CommentLike>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CommentL__3214EC077975CA6F");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Comment).WithMany(p => p.CommentLikes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CommentLi__Comme__6442E2C9");

            entity.HasOne(d => d.User).WithMany(p => p.CommentLikes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CommentLi__UserI__65370702");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Conversa__3214EC0732D38B6C");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<ConversationMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Conversa__3214EC07B903B47E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Conversation).WithMany(p => p.ConversationMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Conversat__Conve__7755B73D");

            entity.HasOne(d => d.User).WithMany(p => p.ConversationMembers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Conversat__UserI__7849DB76");
        });

        modelBuilder.Entity<FavoriteCollection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Favorite__3214EC07D06C8AE1");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.User).WithMany(p => p.FavoriteCollections)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FavoriteC__UserI__681373AD");
        });

        modelBuilder.Entity<Follow>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.FollowedUser).WithMany(p => p.FollowFollowedUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Follows__Followe__4D5F7D71");

            entity.HasOne(d => d.FollowingUser).WithMany(p => p.FollowFollowingUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Follows__Followi__4C6B5938");
        });

        modelBuilder.Entity<Friend>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Friends__3214EC07E8A63F67");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.FriendNavigation).WithMany(p => p.FriendFriendNavigations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Friends__FriendI__531856C7");

            entity.HasOne(d => d.User).WithMany(p => p.FriendUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Friends__UserId__5224328E");
        });

        modelBuilder.Entity<FriendRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FriendRe__3214EC07C6DD5A4D");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.FromUser).WithMany(p => p.FriendRequestFromUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FriendReq__FromU__4E53A1AA");

            entity.HasOne(d => d.ToUser).WithMany(p => p.FriendRequestToUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FriendReq__ToUse__4F47C5E3");
        });

        modelBuilder.Entity<PointTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PointTra__3214EC075A86985D");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.User).WithMany(p => p.PointTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PointTran__UserI__74794A92");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Posts__3214EC07EE391665");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.ClassGroup).WithMany(p => p.Posts).HasConstraintName("FK__Posts__ClassGrou__5BAD9CC8");

            entity.HasOne(d => d.Club).WithMany(p => p.Posts).HasConstraintName("FK__Posts__ClubId__5CA1C101");

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Posts__UserId__5AB9788F");
        });

        modelBuilder.Entity<PostInterest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PostInte__3214EC0728500A76");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Post).WithMany(p => p.PostInterests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostInter__PostI__662B2B3B");

            entity.HasOne(d => d.User).WithMany(p => p.PostInterests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostInter__UserI__671F4F74");
        });

        modelBuilder.Entity<PostLike>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PostLike__3214EC0747CA159F");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Post).WithMany(p => p.PostLikes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostLikes__PostI__5F7E2DAC");

            entity.HasOne(d => d.User).WithMany(p => p.PostLikes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostLikes__UserI__607251E5");
        });

        modelBuilder.Entity<PostReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PostRepo__3214EC07BC04AE86");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Post).WithMany(p => p.PostReports)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostRepor__PostI__793DFFAF");

            entity.HasOne(d => d.Reporter).WithMany(p => p.PostReports)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostRepor__Repor__7A3223E8");
        });

        modelBuilder.Entity<Reward>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Rewards__3214EC07A02DC01E");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<RewardRedemptionLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RewardRe__3214EC0726FAC239");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Reward).WithMany(p => p.RewardRedemptionLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RewardRed__Rewar__76619304");

            entity.HasOne(d => d.User).WithMany(p => p.RewardRedemptionLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RewardRed__UserI__756D6ECB");
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.SchoolId).HasName("PK__Schools__3DA4675B2FE61093");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<StudentProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentP__3214EC077AC05576");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.User).WithOne(p => p.StudentProfile)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentPr__UserI__4A8310C6");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Submissi__3214EC07D1DBDF8C");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Activity).WithMany(p => p.Submissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Submissio__Activ__6FB49575");

            entity.HasOne(d => d.User).WithMany(p => p.Submissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Submissio__UserI__70A8B9AE");
        });

        modelBuilder.Entity<SubmissionVote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Submissi__3214EC070C68EA79");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Submission).WithMany(p => p.SubmissionVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Submissio__Submi__719CDDE7");

            entity.HasOne(d => d.User).WithMany(p => p.SubmissionVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Submissio__UserI__72910220");
        });

        modelBuilder.Entity<TeacherProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TeacherP__3214EC073F32419D");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.User).WithOne(p => p.TeacherProfile)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TeacherPr__UserI__4B7734FF");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07A3A6D7CF");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.School).WithMany(p => p.Users).HasConstraintName("FK__Users__SchoolId__498EEC8D");
        });

        modelBuilder.Entity<UserBlock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserBloc__3214EC07D9DB2867");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Blocked).WithMany(p => p.UserBlockBlockeds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserBlock__Block__51300E55");

            entity.HasOne(d => d.Blocker).WithMany(p => p.UserBlockBlockers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserBlock__Block__503BEA1C");
        });

        modelBuilder.Entity<UserPoint>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserPoin__1788CC4C981D0001");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.User).WithOne(p => p.UserPoint)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserPoint__UserI__73852659");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
