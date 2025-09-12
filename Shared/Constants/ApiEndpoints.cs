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
            public const string CreateUser = "api/auth/create-user";
            public const string GetAllUsers = "api/auth/get-all-users";
            public const string UpdateUser = "api/auth/update-user";
            public const string OneTimeLogin = "api/auth/one-time-login";
            public const string ChangePasswordOtl = "api/auth/change-password-otl";
        }

        public static class User
        {
            public const string GetAllUsers = "api/user/get-all";
            public const string GetUserById = "api/user/get-by-id";
            public const string UpdateUser = "api/user/update";
            public const string DeleteUser = "api/user/delete";
        }

        public static class Course
        {
            public const string GetAllCourses = "api/course/get-all";
            public const string CreateCourse = "api/course/create";
            public const string UpdateCourse = "api/course/update";
            public const string DeleteCourse = "api/course/delete";
        }
    }
}
