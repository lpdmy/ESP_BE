-- Create & use DB
IF DB_ID(N'EduShpere') IS NULL
BEGIN
  CREATE DATABASE EduShpere;
END
GO
USE EduShpere;
GO

-- =========================
-- Tables
-- =========================
CREATE TABLE [Schools] (
  [SchoolId] bigint PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(200) NOT NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [Users] (
  [Id] int IDENTITY(1,1) PRIMARY KEY,
  [SchoolId] bigint NULL,
  [Username] nvarchar(255) NULL,
  [Firstname] nvarchar(255) NULL,
  [Lastname] nvarchar(255) NULL,
  [Role] tinyint NULL,
  [AvatarUrl] nvarchar(1000) NULL,
  [CreatedAtLegacy] datetime NULL,
  [Email] nvarchar(320) NULL,
  [PhoneNumber] nvarchar(50) NULL,
  [Birthdate] datetime2 NULL,
  [Address] nvarchar(500) NULL,
  [Password] nvarchar(255) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [StudentProfiles] (
  [Id] int IDENTITY(1,1) PRIMARY KEY,
  [UserId] int UNIQUE NOT NULL,
  [StudentNumber] nvarchar(100) NULL,
  [EnrollmentYear] smallint NULL,
  [Bio] nvarchar(1000) NULL,
  [ExtraJson] nvarchar(max) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [TeacherProfiles] (
  [Id] int IDENTITY(1,1) PRIMARY KEY,
  [UserId] int UNIQUE NOT NULL,
  [SubjectSpecialties] nvarchar(500) NULL,
  [Title] nvarchar(100) NULL,
  [ExtraJson] nvarchar(max) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [Follows] (
  [FollowingUserId] int NOT NULL,
  [FollowedUserId] int NOT NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion,
  CONSTRAINT PK_Follows PRIMARY KEY ([FollowingUserId], [FollowedUserId])
);
GO

CREATE TABLE [FriendRequests] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [FromUserId] int NOT NULL,
  [ToUserId] int NOT NULL,
  [Status] nvarchar(20) NULL,
  [RespondedAt] datetime2 NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [UserBlocks] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [BlockerId] int NOT NULL,
  [BlockedId] int NOT NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [Friends] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [UserId] int NOT NULL,
  [FriendId] int NOT NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [ClassGroups] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(255) NULL,
  [Description] nvarchar(max) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [ClassGroupMembers] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [ClassGroupId] int NOT NULL,
  [UserId] int NOT NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [Clubs] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(255) NULL,
  [Description] nvarchar(max) NULL,
  [AvatarUrl] nvarchar(1000) NULL,
  [CoverUrl] nvarchar(1000) NULL,
  [CreatedByUserId] int NOT NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [ClubMembers] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [ClubId] int NOT NULL,
  [UserId] int NOT NULL,
  [Role] nvarchar(50) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [ClubJoinRequests] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [ClubId] int NOT NULL,
  [UserId] int NOT NULL,
  [Status] nvarchar(20) NULL,
  [RespondedAt] datetime2 NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [Posts] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [Title] nvarchar(500) NULL,
  [Body] nvarchar(max) NULL,
  [UserId] int NOT NULL,
  [Type] nvarchar(30) NULL,
  [ClassGroupId] int NULL,
  [ClubId] int NULL,
  [PrivacyLevel] nvarchar(20) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [Attachments] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [PostId] int NULL,
  [CommentId] int NULL,
  [FileUrl] nvarchar(1000) NULL,
  [FileType] nvarchar(100) NULL,
  [Mime] nvarchar(100) NULL,
  [Width] int NULL,
  [Height] int NULL,
  [ModerationStatus] nvarchar(30) NULL,
  [AiFlagsJson] nvarchar(max) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [PostLikes] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [PostId] int NOT NULL,
  [UserId] int NOT NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [Comments] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [PostId] int NULL,
  [ParentCommentId] int NULL,
  [UserId] int NOT NULL,
  [Content] nvarchar(max) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [CommentLikes] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [CommentId] int NOT NULL,
  [UserId] int NOT NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [PostInterests] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [PostId] int NOT NULL,
  [UserId] int NOT NULL,
  [InterestStatus] nvarchar(20) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [FavoriteCollections] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [UserId] int NOT NULL,
  [Name] nvarchar(255) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [CollectionItems] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [CollectionId] int NOT NULL,
  [PostId] int NOT NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [Activities] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [Title] nvarchar(500) NULL,
  [Description] nvarchar(max) NULL,
  [StartDate] datetime2 NULL,
  [EndDate] datetime2 NULL,
  [CreatedByUserId] int NOT NULL,
  [Scope] nvarchar(20) NULL,
  [ClubId] int NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [ActivityRewards] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [ActivityId] int NOT NULL,
  [Rank] nvarchar(50) NULL,
  [StarPoints] int NOT NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [ActivityParticipants] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [ActivityId] int NOT NULL,
  [UserId] int NOT NULL,
  [Status] nvarchar(50) NULL,
  [JoinedAt] datetime2 NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [Submissions] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [ActivityId] int NOT NULL,
  [UserId] int NOT NULL,
  [FileUrl] nvarchar(1000) NULL,
  [Score] decimal(9,2) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [SubmissionVotes] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [SubmissionId] int NOT NULL,
  [UserId] int NOT NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [UserPoints] (
  [UserId] int PRIMARY KEY,
  [Balance] int NOT NULL DEFAULT (0),
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [PointTransactions] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [UserId] int NOT NULL,
  [Change] int NOT NULL,
  [Reason] nvarchar(255) NULL,
  [SourceType] nvarchar(50) NULL,
  [SourceId] int NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [Rewards] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [Title] nvarchar(255) NULL,
  [Description] nvarchar(max) NULL,
  [PointsCost] int NULL,
  [RewardType] nvarchar(50) NULL,
  [Metadata] nvarchar(max) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [RewardRedemptionLogs] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [UserId] int NOT NULL,
  [RewardId] int NOT NULL,
  [PointsSpent] int NOT NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [Conversations] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [IsGroup] bit NOT NULL DEFAULT (0),
  [Title] nvarchar(255) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [ConversationMembers] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [ConversationId] int NOT NULL,
  [UserId] int NOT NULL,
  [JoinedAt] datetime2 NULL,
  [LastReadAt] datetime2 NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

CREATE TABLE [PostReports] (
  [Id] int PRIMARY KEY IDENTITY(1, 1),
  [PostId] int NOT NULL,
  [ReporterId] int NOT NULL,
  [Reason] nvarchar(255) NULL,
  [Status] nvarchar(20) NULL,
  [CreatedAt] datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
  [CreatedBy] int NULL,
  [UpdatedAt] datetime2 NULL,
  [UpdatedBy] int NULL,
  [IsDeleted] bit NOT NULL DEFAULT (0),
  [RowVersion] rowversion
);
GO

-- =========================
-- Indexes
-- =========================
CREATE UNIQUE INDEX [UX_FriendRequests_Pair] ON [FriendRequests] ([FromUserId], [ToUserId]);
GO
CREATE UNIQUE INDEX [UX_UserBlocks_Pair] ON [UserBlocks] ([BlockerId], [BlockedId]);
GO
CREATE UNIQUE INDEX [ClassGroupMembers_index_2] ON [ClassGroupMembers] ([ClassGroupId], [UserId]);
GO
CREATE UNIQUE INDEX [ClubMembers_index_3] ON [ClubMembers] ([ClubId], [UserId]);
GO
CREATE UNIQUE INDEX [ClubJoinRequests_index_4] ON [ClubJoinRequests] ([ClubId], [UserId]);
GO
CREATE INDEX [IX_Posts_User_CreatedAt] ON [Posts] ([UserId], [CreatedAt]);
GO
CREATE UNIQUE INDEX [PostLikes_index_6] ON [PostLikes] ([PostId], [UserId]);
GO
CREATE UNIQUE INDEX [CommentLikes_index_7] ON [CommentLikes] ([CommentId], [UserId]);
GO
CREATE UNIQUE INDEX [PostInterests_index_8] ON [PostInterests] ([PostId], [UserId]);
GO
CREATE UNIQUE INDEX [CollectionItems_index_9] ON [CollectionItems] ([CollectionId], [PostId]);
GO
CREATE UNIQUE INDEX [ActivityParticipants_index_10] ON [ActivityParticipants] ([ActivityId], [UserId]);
GO
CREATE UNIQUE INDEX [SubmissionVotes_index_11] ON [SubmissionVotes] ([SubmissionId], [UserId]);
GO
CREATE INDEX [PointTransactions_index_12] ON [PointTransactions] ([UserId], [CreatedAt]);
GO
CREATE UNIQUE INDEX [ConversationMembers_index_13] ON [ConversationMembers] ([ConversationId], [UserId]);
GO

-- =========================
-- Extended properties
-- =========================
EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'rowversion/byte[]',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Schools',
@level2type = N'Column', @level2name = 'RowVersion';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '0 Student, 1 Teacher, 2 Admin',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Users',
@level2type = N'Column', @level2name = 'Role';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'legacy created_at if any',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Users',
@level2type = N'Column', @level2name = 'CreatedAtLegacy';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'rowversion/byte[]',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Users',
@level2type = N'Column', @level2name = 'RowVersion';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '<SchoolName>10XXXXX',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'StudentProfiles',
@level2type = N'Column', @level2name = 'StudentNumber';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'ThS/TS/...',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'TeacherProfiles',
@level2type = N'Column', @level2name = 'Title';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'pending/accepted/rejected',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'FriendRequests',
@level2type = N'Column', @level2name = 'Status';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'president/vice_president/member',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ClubMembers',
@level2type = N'Column', @level2name = 'Role';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'pending/accepted/rejected',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ClubJoinRequests',
@level2type = N'Column', @level2name = 'Status';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'school/general/class/club',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Posts',
@level2type = N'Column', @level2name = 'Type';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'private/friends/public/draft',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Posts',
@level2type = N'Column', @level2name = 'PrivacyLevel';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Pending/Approved/Rejected',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Attachments',
@level2type = N'Column', @level2name = 'ModerationStatus';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'interested/not_interested',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PostInterests',
@level2type = N'Column', @level2name = 'InterestStatus';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'club/public',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Activities',
@level2type = N'Column', @level2name = 'Scope';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '1st/2nd/3rd/special',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ActivityRewards',
@level2type = N'Column', @level2name = 'Rank';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'activity/admin_edit/purchase',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PointTransactions',
@level2type = N'Column', @level2name = 'SourceType';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'avatar/comment/...',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Rewards',
@level2type = N'Column', @level2name = 'RewardType';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'pending/resolved/rejected',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PostReports',
@level2type = N'Column', @level2name = 'Status';
GO

-- =========================
-- Foreign Keys
-- =========================
ALTER TABLE [Users] ADD FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([SchoolId]);
GO
ALTER TABLE [StudentProfiles] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [TeacherProfiles] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [Follows] ADD FOREIGN KEY ([FollowingUserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [Follows] ADD FOREIGN KEY ([FollowedUserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [FriendRequests] ADD FOREIGN KEY ([FromUserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [FriendRequests] ADD FOREIGN KEY ([ToUserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [UserBlocks] ADD FOREIGN KEY ([BlockerId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [UserBlocks] ADD FOREIGN KEY ([BlockedId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [Friends] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [Friends] ADD FOREIGN KEY ([FriendId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [ClassGroupMembers] ADD FOREIGN KEY ([ClassGroupId]) REFERENCES [ClassGroups] ([Id]);
GO
ALTER TABLE [ClassGroupMembers] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [Clubs] ADD FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [ClubMembers] ADD FOREIGN KEY ([ClubId]) REFERENCES [Clubs] ([Id]);
GO
ALTER TABLE [ClubMembers] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [ClubJoinRequests] ADD FOREIGN KEY ([ClubId]) REFERENCES [Clubs] ([Id]);
GO
ALTER TABLE [ClubJoinRequests] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [Posts] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [Posts] ADD FOREIGN KEY ([ClassGroupId]) REFERENCES [ClassGroups] ([Id]);
GO
ALTER TABLE [Posts] ADD FOREIGN KEY ([ClubId]) REFERENCES [Clubs] ([Id]);
GO
ALTER TABLE [Attachments] ADD FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]);
GO
ALTER TABLE [Attachments] ADD FOREIGN KEY ([CommentId]) REFERENCES [Comments] ([Id]);
GO
ALTER TABLE [PostLikes] ADD FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]);
GO
ALTER TABLE [PostLikes] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [Comments] ADD FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]);
GO
ALTER TABLE [Comments] ADD FOREIGN KEY ([ParentCommentId]) REFERENCES [Comments] ([Id]);
GO
ALTER TABLE [Comments] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [CommentLikes] ADD FOREIGN KEY ([CommentId]) REFERENCES [Comments] ([Id]);
GO
ALTER TABLE [CommentLikes] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [PostInterests] ADD FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]);
GO
ALTER TABLE [PostInterests] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [FavoriteCollections] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [CollectionItems] ADD FOREIGN KEY ([CollectionId]) REFERENCES [FavoriteCollections] ([Id]);
GO
ALTER TABLE [CollectionItems] ADD FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]);
GO
ALTER TABLE [Activities] ADD FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [Activities] ADD FOREIGN KEY ([ClubId]) REFERENCES [Clubs] ([Id]);
GO
ALTER TABLE [ActivityRewards] ADD FOREIGN KEY ([ActivityId]) REFERENCES [Activities] ([Id]);
GO
ALTER TABLE [ActivityParticipants] ADD FOREIGN KEY ([ActivityId]) REFERENCES [Activities] ([Id]);
GO
ALTER TABLE [ActivityParticipants] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [Submissions] ADD FOREIGN KEY ([ActivityId]) REFERENCES [Activities] ([Id]);
GO
ALTER TABLE [Submissions] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [SubmissionVotes] ADD FOREIGN KEY ([SubmissionId]) REFERENCES [Submissions] ([Id]);
GO
ALTER TABLE [SubmissionVotes] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [UserPoints] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [PointTransactions] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [RewardRedemptionLogs] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [RewardRedemptionLogs] ADD FOREIGN KEY ([RewardId]) REFERENCES [Rewards] ([Id]);
GO
ALTER TABLE [ConversationMembers] ADD FOREIGN KEY ([ConversationId]) REFERENCES [Conversations] ([Id]);
GO
ALTER TABLE [ConversationMembers] ADD FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO
ALTER TABLE [PostReports] ADD FOREIGN KEY ([PostId]) REFERENCES [Posts] ([Id]);
GO
ALTER TABLE [PostReports] ADD FOREIGN KEY ([ReporterId]) REFERENCES [Users] ([Id]);
GO
