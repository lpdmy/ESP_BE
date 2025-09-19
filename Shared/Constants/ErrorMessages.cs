namespace EduShpere.Shared.Constants
{
    public static class ErrorMessages
    {
        public struct Auth
        {
            public const string InvalidCredentials = "Email or Password is not correct.";
            public const string InvalidToken = "Token không hợp lệ hoặc đã hết hạn.";
            public const string UserNotFound = "Người dùng không tồn tại.";
            public const string EmailAlreadyExists = "Email đã tồn tại trong hệ thống.";
            public const string InvalidFile = "File không hợp lệ.";
            public const string InvalidPassword = "Mật khẩu không chính xác";
            public const string EmailContainsSpacesOrVietnamese = "Email không được chứa dấu cách hoặc ký tự tiếng Việt";
            public const string UsernameContainsSpacesOrVietnamese = "ID không được chứa dấu cách hoặc ký tự tiếng Việt";
            public const string UsernameAlreadyExists = "ID đã tồn tại trong hệ thống.";
            public const string PasswordContainsSpacesOrVietnamese = "Mật khẩu không được chứa dấu cách hoặc ký tự tiếng Việt";
            public const string OldPasswordContainsSpacesOrVietnamese = "Mật khẩu cũ không được chứa dấu cách hoặc ký tự tiếng Việt";
            public const string NewPasswordContainsSpacesOrVietnamese = "Mật khẩu mới không được chứa dấu cách hoặc ký tự tiếng Việt";
            public const string ConfirmPasswordContainsSpacesOrVietnamese = "Mật khẩu xác nhận không được chứa dấu cách hoặc ký tự tiếng Việt";
        }

        public struct Password
        {
            public const string PasswordTooShort = "Mật khẩu phải có ít nhất 6 ký tự.";
            public const string PasswordTooLong = "Mật khẩu không được vượt quá 20 ký tự.";
            public const string PasswordWhiteSpace = "Mật khẩu không được chứa khoảng trắng";
            public const string PasswordMismatch = "Mật khẩu xác nhận không khớp.";
            public const string PasswordMustContainUppercase = "Mật khẩu phải chứa ít nhất 1 chữ cái viết hoa.";
            public const string PasswordMustContainSpecialChar = "Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt (!@#$%^&*).";
            public const string PasswordInvalidLength = "Mật khẩu phải có từ 6 đến 20 ký tự.";
        }

        public struct Validation
        {
            public const string EmailRequired = "Email là bắt buộc";
            public const string EmailInvalidFormat = "Email không đúng định dạng";
            public const string PasswordRequired = "Mật khẩu là bắt buộc";
            public const string OldPasswordRequired = "Mật khẩu cũ là bắt buộc";
            public const string NewPasswordRequired = "Mật khẩu mới là bắt buộc";
            public const string ConfirmPasswordRequired = "Xác nhận mật khẩu là bắt buộc";
            public const string UsernameRequired = "Username là bắt buộc";
            public const string FirstNameRequired = "Tên là bắt buộc";
            public const string LastNameRequired = "Họ là bắt buộc";
            public const string RoleRequired = "Vai trò là bắt buộc";
            public const string UserIdRequired = "User ID là bắt buộc";
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
            public const string ProfileNotFound = "Không tìm thấy profile.";
            public const string ProfileAlreadyExists = "Người dùng đã có profile. Không thể tạo mới.";
            public const string InvalidStudentNumber = "Mã sinh viên không hợp lệ hoặc đã tồn tại.";
            public const string EnrollmentYearInvalid = "Năm nhập học không hợp lệ.";
            public const string CreateFailed = "Tạo hồ sơ sinh viên thất bại. Vui lòng thử lại.";
            public const string UpdateFailed = "Cập nhật hồ sơ sinh viên thất bại. Vui lòng thử lại.";
            public const string DeleteFailed = "Xóa hồ sơ sinh viên thất bại. Vui lòng thử lại.";
            public const string GetFailed = "Lấy thông tin hồ sơ sinh viên thất bại. Vui lòng thử lại.";
            public const string InvalidId = "ID không hợp lệ.";
            public const string ProfileNotExists = "Profile không tồn tại.";
            public const string ProfileExists = "Profile tồn tại.";
            public const string ValidationFailed = "Dữ liệu không hợp lệ.";
            public const string StudentNumberRequired = "Mã sinh viên là bắt buộc.";
            public const string EnrollmentYearRequired = "Năm nhập học là bắt buộc.";
            public const string StudentNumberTooLong = "Mã số sinh viên không được vượt quá 100 ký tự.";
            public const string BioTooLong = "Tiểu sử không được vượt quá 1000 ký tự.";
            public const string InvalidAvatarUrl = "URL avatar phải là một URL hợp lệ.";
            public const string AvatarUrlTooLong = "URL avatar không được vượt quá 500 ký tự.";
            public const string InvalidPhoneNumber = "Số điện thoại phải là số điện thoại hợp lệ.";
            public const string PhoneNumberTooLong = "Số điện thoại không được vượt quá 20 ký tự.";
            public const string InvalidEnrollmentYear = "Năm nhập học phải từ 2000 đến 2030.";
            public const string SubjectSpecialtiesTooLong = "Chuyên môn giảng dạy không được vượt quá 500 ký tự.";
            public const string TitleTooLong = "Chức danh không được vượt quá 100 ký tự.";
            public const string TeacherCodeTooLong = "Mã giảng viên không được vượt quá 100 ký tự.";
            public const string DepartmentTooLong = "Khoa/Bộ môn không được vượt quá 100 ký tự.";
            public const string PositionTooLong = "Chức vụ không được vượt quá 100 ký tự.";
        }
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
