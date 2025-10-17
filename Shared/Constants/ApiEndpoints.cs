namespace EduShpere.Shared.Constants
{
    public static class ApiEndpoints
    {
        public static class Auth
        {
            public const string Login = "api/auth/login";
            public const string GetMe = "api/auth/GetMe";
            public const string ImportFile = "api/auth/ImportFile";
            public const string Test = "api/auth/Test";
            public const string TestInvitation = "api/auth/TestInvitation";
            public const string CreateUser = "api/auth/create-user";
            public const string UserUrl = "api/users";
            public const string OneTimeLogin = "api/auth/one-time-login";
            public const string ChangePasswordOtl = "api/auth/change-password-otl";
            public const string ChangePassword = "api/auth/change-password";
            public const string ForgotPassword = "api/auth/forgot-password";
        }

        public static class User
        {
            public const string Users = "api/users";
            public const string GetUserById = "api/users/{id}";
            public const string Statistics = "api/user/statistics";
        }

        public static class Upload
        {
            public const string UploadUrl = "api/upload";
        }
        public static class UserProfile
        {
            public const string StudentProfileUrl = "api/user/student-profile";
            public const string MyProfile = "api/userprofile/my-profile";
            public const string AllProfiles = "api/userprofile/all";
            public const string ProfileById = "api/userprofile/{id}";
            public const string CreateProfile = "api/userprofile";
            public const string UpdateProfile = "api/userprofile/{id}";
            public const string DeleteProfile = "api/userprofile/{id}";
            public const string CheckProfileExists = "api/userprofile/{id}/exists";

            // Teacher Profile endpoints
            public const string MyTeacherProfile = "api/userprofile/my-teacher-profile";
            public const string AllTeacherProfiles = "api/userprofile/teachers";
            public const string TeacherProfileById = "api/userprofile/teachers/{id}";
            public const string CreateTeacherProfile = "api/userprofile/teachers";
            public const string UpdateTeacherProfile = "api/userprofile/teachers/{id}";
            public const string DeleteTeacherProfile = "api/userprofile/teachers/{id}";
            public const string CheckTeacherProfileExists = "api/userprofile/teachers/{id}/exists";
        }

        public static class Activity
        {
            public const string Activities = "api/activity";
            public const string GetActivityById = "api/activity/{id}";

        }


        public static class ActivityParticipant
        {
            public const string ActivityParticipantRoute = "api/activityparticipant";
            public const string GetActivityParticipantById = "api/activityparticipant/{id}";
        }
        public static class Post
        {
            public const string Posts = "api/post";
            public const string GetPostById = "api/post/{id}";
            public const string GetPostByUser = "api/post/user";
            public const string Like = "api/post/like";
            public const string ClubPending = "api/post/club/pending/{clubid}";
            public const string Club = "api/post/club/{clubid}";
            public const string ApprovePost = "api/post/approve/{id}";

        }
        public static class Collection
        {
            public const string Collections = "api/collection";
            public const string GetCollectionById = "api/collection/{id}";
            public const string GetCollectionByUser = "api/collection/user";
            public const string AddCollectionIteam = "api/collection/add-collection-iteam";
        }
        public static class ClubCreationRequest
        {
            public const string Create = "api/club-creation-request";
            public const string GetAll = "api/club-creation-request";
            public const string GetAllByUser = "api/club-creation-request/user";
            public const string Approve = "api/club-creation-request/approve/{id}";
        }
        public static class Club
        {
            public const string Clubs = "api/club";
            public const string GetClubById = "api/club/{id}";
            public const string GetClubByUser = "api/club/user";
            public const string Categories = "api/club/categories";
            public const string Members = "api/club/members";
            public const string RemoveMember = "api/club/members/remove/{id}";
        }
        public static class ClubJoinRequest
        {
            public const string JoinRequest = "api/join-request";
            public const string JoinRequestId = "api/join-request/{id}";
            public const string ApproveJoinRequest = "api/join-request/approve/{id}";
            public const string RejectJoinRequest = "api/join-request/reject/{id}";
            public const string InviteMentor = "api/join-request/invite-mentor";
            public const string JoinRequestByClub = "api/join-request/club/{id}";
        }
        public static class ClubMember
        {
            public const string ClubMembers = "api/club-member";
            public const string GetClubMemberByUser = "api/club-member/user";
            public const string OutClub = "api/club-member/{id}";
        }
    }
}