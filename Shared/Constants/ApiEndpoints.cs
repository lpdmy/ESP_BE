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
            public const string UploadFile = "api/upload/file";
        }
        public static class UserProfile
        {
            public const string StudentProfileUrl = "api/user/student-profile";
            public const string MyProfile = "api/userprofile/my-profile";
            public const string AllProfiles = "api/userprofile/all";
            public const string ProfileById = "api/userprofile/{id}";
            public const string ProfileByUserId = "api/userprofile/user/{id}";
            public const string CreateProfile = "api/userprofile";
            public const string UpdateProfile = "api/userprofile/{id}";
            public const string DeleteProfile = "api/userprofile/{id}";
            public const string CheckProfileExists = "api/userprofile/{id}/exists";

            // Teacher Profile endpoints
            public const string MyTeacherProfile = "api/userprofile/my-teacher-profile";
            public const string AllTeacherProfiles = "api/userprofile/teachers";
            public const string TeacherProfileById = "api/userprofile/teachers/{id}";
            public const string TeacherProfileByUserId = "api/userprofile/teachers/user/{id}";
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
            public const string GetPostsByClassGroup = "api/post/classgroup/{Id}";
            public const string Like = "api/post/like";
            public const string ClubPending = "api/post/club/pending/{clubid}";
            public const string Club = "api/post/club/{clubid}";
            public const string ApprovePost = "api/post/approve/{id}";
            public const string RejectPost = "api/post/reject/{id}";

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
            public const string Reject = "api/club-creation-request/reject";
        }
        public static class Club
        {
            public const string Clubs = "api/club";
            public const string GetClubById = "api/club/{id}";
            public const string GetClubByUser = "api/club/user";
            public const string Categories = "api/club/categories";
            public const string Members = "api/club/members";
            public const string RemoveMember = "api/club/members/remove/{id}";
            public const string SearchUsers = "api/club/search-users";
        }
        public static class ClubJoinRequest
        {
            public const string JoinRequest = "api/join-request";
            public const string JoinRequestId = "api/join-request/{id}";
            public const string ApproveJoinRequest = "api/join-request/approve/{id}";
            public const string RejectJoinRequest = "api/join-request/reject/{id}";
            public const string InviteMentor = "api/join-request/invite-mentor";
            public const string JoinRequestByClub = "api/join-request/club/{id}";
            public const string JoinRequestByUser = "api/join-request/user";
        }
        public static class ClubMember
        {
            public const string ClubMembers = "api/club-member";
            public const string GetClubMemberByUser = "api/club-member/user";
            public const string OutClub = "api/club-member/{id}";
            public const string KickClub = "api/club-member";
            public const string ChangeRole = "api/club-member/change-role";
        }
        public static class Comment
        {
            public const string Comments = "api/comment";
            public const string GetCommentsByPost = "api/comment/post/{postId}";
            public const string GetCommentsByComment = "api/comment/comment/{id}";
            public const string DeleteComment = "api/comment/{id}";
        }
        public static class ClassGroup
        {
            public const string ClassGroups = "api/classgroup";
            public const string GetClassGroupById = "api/classgroup/{id}";
            public const string GetClassGroupDetail = "api/classgroup/{id}/detail";
            public const string GetByName = "api/classgroup/by-name";
            public const string Dashboard = "api/classgroup/dashboard";
            public const string ByGrade = "api/classgroup/by-grade/{grade}";
            public const string WithoutGrade = "api/classgroup/without-grade";
            public const string ByAcademicYear = "api/classgroup/by-academic-year/{academicYearId}";
            public const string WithoutAcademicYear = "api/classgroup/without-academic-year";
            public const string Deleted = "api/classgroup/deleted";
            public const string Filter = "api/classgroup/filter";
            public const string CheckNameExists = "api/classgroup/check-name-exists";
            public const string Create = "api/classgroup";
            public const string Update = "api/classgroup/{id}";
            public const string Delete = "api/classgroup/{id}";
            
            // Student management endpoints
            public const string GetStudents = "api/classgroup/{id}/students";
            public const string AddStudent = "api/classgroup/{id}/students";
            public const string RemoveStudent = "api/classgroup/{id}/students/{studentId}";
            
            // Homeroom Teacher management endpoints
            public const string AssignHomeroomTeacher = "api/classgroup/{id}/homeroom-teacher";
            public const string RemoveHomeroomTeacher = "api/classgroup/{id}/homeroom-teacher";
            public const string GetHomeroomTeacher = "api/classgroup/{id}/homeroom-teacher";
            
            // Academic Year endpoints
            public const string GetAcademicYears = "api/classgroup/academic-years";
            public const string GetAcademicYearCurrent = "api/classgroup/academic-years-current";
            
            // Current Class endpoints
            public const string GetCurrentClass = "api/classgroup/current-class";
        }

        public static class StarPoint
        {
            public const string GetAllRules = "api/admin/rules";
            public const string UpdateRulePoints = "api/admin/rules/{actionType}/points";

            public const string GetAllRewards = "api/admin/rewards";    
            public const string GetRewardById = "api/admin/rewards/{id}";  
            public const string CreateReward = "api/admin/rewards"; 
            public const string UpdateReward = "api/admin/rewards/{id}"; 
            public const string DeleteReward = "api/admin/rewards/{id}";   

            public const string RedeemReward = "api/rewards/redeem";
            public const string GetPointHistory = "api/points/history";
            public const string CreatePointHistory = "api/points/history";
            public const string GetCurrentUserPoints = "api/points/current";

            public const string GetAllRedemptionsAdmin = "api/redeems/admin";
            public const string GetMyRedemptions = "api/redeems/me";
            public const string PickupRedemption = "api/redeems/{id}/pickup";
        }

        public static class Notification
        {
            public const string Notifications = "api/notification";
            public const string GetByUser = "api/notification/user";
            public const string MarkAsRead = "api/notification/{id}/read";
            public const string AddNotification = "api/notification";
            public const string AddTestNotification = "api/notification/test";
        }

        public static class Chat
        {
            public const string Rooms = "api/chat/rooms";
            public const string Messages = "api/chat/messages";
            public const string MarkAsRead = "api/chat/messages/{roomId}/read";
            public const string GetMessages = "api/chat/messages/{roomId}";
        }
    }
}