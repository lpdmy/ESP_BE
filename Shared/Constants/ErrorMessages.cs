namespace EduShpere.Shared.Constants
{
    public static class ErrorMessages
    {
        public struct Auth
        {
            public const string InvalidCredentials = "Username or Password is not correct.";
            public const string InvalidToken = "Token không hợp lệ hoặc đã hết hạn.";
            public const string UserNotFound = "Người dùng không tồn tại.";
            public const string EmailAlreadyExists = "Email đã tồn tại trong hệ thống.";
            public const string InvalidFile = "File không hợp lệ.";
        }

        public struct Password
        {
            public const string PasswordTooShort = "Mật khẩu phải có ít nhất 8 ký tự.";
            public const string PasswordMismatch = "Mật khẩu xác nhận không khớp.";
        }

        public struct Generic
        {
            public const string UnknownError = "Có lỗi xảy ra. Vui lòng thử lại.";
        }


        public struct Activity
        {
            public const string ActivityNotFound = "Hoạt động không tồn tại.";
            public const string ActivityAlreadyExists = "Hoạt động đã tồn tại.";
        }

        public struct ActivityParticipant
        {
            public const string AlreadyJoined = "Người dùng đã tham gia hoạt động này.";
            public const string NotFound = "Người dùng chưa tham gia hoạt động này.";
        }

    }
}
