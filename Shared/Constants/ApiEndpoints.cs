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
        }

        public static class ClassGroup
        {
            public const string ClassGroups = "api/classgroup";
            public const string GetClassGroupById = "api/classgroup/{id}";
            public const string GetByName = "api/classgroup/by-name";
            public const string Dashboard = "api/classgroup/dashboard";
            public const string ByGrade = "api/classgroup/by-grade/{grade}";
            public const string WithoutGrade = "api/classgroup/without-grade";
            public const string ByStartYear = "api/classgroup/by-start-year/{startYear}";
            public const string WithoutStartYear = "api/classgroup/without-start-year";
            public const string Deleted = "api/classgroup/deleted";
            public const string Filter = "api/classgroup/filter";
            public const string CheckNameExists = "api/classgroup/check-name-exists";
            public const string Create = "api/classgroup";
            public const string Update = "api/classgroup/{id}";
            public const string Delete = "api/classgroup/{id}";
        }
    }
}