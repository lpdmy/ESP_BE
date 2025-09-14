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
            public const string InvalidPassword = "Mật khẩu không chính xác";
        }

        public struct Password
        {
            public const string PasswordTooShort = "Mật khẩu phải có ít nhất 8 ký tự.";
            public const string PasswordWhiteSpace = "Mật khẩu không được chứa khoảng trắng";
            public const string PasswordMismatch = "Mật khẩu xác nhận không khớp.";
        }

        public struct Generic
        {
            public const string UnknownError = "Có lỗi xảy ra. Vui lòng thử lại.";
        }

        public struct Upload
        {
            public const string UploadFailed = "Upload file thất bại. Vui lòng thử lại.";
        }
        public struct UserProfile
        {
            public const string UserNotFound = "Người dùng không tồn tại.";
            public const string ProfileAlreadyExists = "Hồ sơ sinh viên đã tồn tại cho người dùng này.";
            public const string InvalidStudentNumber = "Mã sinh viên không hợp lệ hoặc đã tồn tại.";
            public const string EnrollmentYearInvalid = "Năm nhập học không hợp lệ.";
            public const string CreateFailed = "Tạo hồ sơ sinh viên thất bại. Vui lòng thử lại.";
            public const string UpdateFailed = "Cập nhật hồ sơ sinh viên thất bại. Vui lòng thử lại.";

            public const string StudentNumberRequired = "Mã sinh viên là bắt buộc.";
            public const string EnrollmentYearRequired = "Năm nhập học là bắt buộc.";

        public struct Activity
        {
            public const string ActivityNotFound = "Hoạt động không tồn tại.";
            public const string ActivityAlreadyExists = "Hoạt động đã tồn tại.";
            public const string StartDayAfterEndDay = "Ngày bắt đầu phải trước ngày kết thúc.";
        }
        

        public struct ActivityParticipant
        {
            public const string AlreadyJoined = "Người dùng đã tham gia hoạt động này.";
            public const string NotFound = "Người dùng chưa tham gia hoạt động này.";

        }

    }
}
