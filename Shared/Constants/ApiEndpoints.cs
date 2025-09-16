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
            public const string GetAllUsers = "api/user/get-all";
            public const string GetUserById = "api/user/get-by-id";
            public const string UpdateUser = "api/user/update";
            public const string DeleteUser = "api/user/delete";
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

    }
}