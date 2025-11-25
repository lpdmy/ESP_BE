namespace EduShpere.Shared.Constants
{
    public static class ErrorMessages
    {
        public struct Auth
        {
            public const string InvalidCredentials = "Email hoặc Password không tồn tại";
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
            public const string ExistByEmail = "Email đã được sử dụng";
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
            
            public const string FieldRequired = "Trường này là bắt buộc";
            public const string IdRequired = "ID là bắt buộc";
            public const string NameTooLong = "Tên không được vượt quá 255 ký tự";
            public const string DescriptionTooLong = "Mô tả không được vượt quá 500 ký tự";
            
            // System Announcement validation
            public const string TitleRequired = "Tiêu đề là bắt buộc";
            public const string TitleTooLong = "Tiêu đề không được vượt quá 500 ký tự";
            public const string ContentRequired = "Nội dung là bắt buộc";
            public const string AnnouncementTypeRequired = "Loại thông báo là bắt buộc";
            public const string AnnouncementTypeTooLong = "Loại thông báo không được vượt quá 50 ký tự";
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
            public const string StartDayAfterEndDayRegister = "Ngày bắt đầu đăng ký phải trước ngày kết thúc đăng ký.";
            public const string EndDayRegisterAfterStarDay = "Ngày kết thúc đăng ký phải trước Ngày bắt đầu sự kiện.";
            public const string MaxParticipantGreaterThanZero = "số lượng người đăng ký phải lớn hơn 0.";
        }
        

        public struct ActivityParticipant
        {
            public const string AlreadyJoined = "Người dùng đã tham gia hoạt động này.";
            public const string NotFound = "Người dùng chưa tham gia hoạt động này.";

        }
        public struct Post
        {
            public const string PostIsFlaged = "Bài viết đã bị vi phạm nội dung.";
            public const string PostError = "Bài viết đã bị lỗi khi tạo.";
            public const string PostNotFound = "Bài viết không tồn tại.";
            public const string ListNotFound = "Không tìm thấy bài viết nào.";
        }

        public struct SystemAnnouncement
        {
            public const string NotFound = "Thông báo hệ thống không tồn tại.";
            public const string AlreadyExists = "Thông báo hệ thống đã tồn tại.";
            public const string CreateFailed = "Tạo thông báo hệ thống thất bại. Vui lòng thử lại.";
            public const string UpdateFailed = "Cập nhật thông báo hệ thống thất bại. Vui lòng thử lại.";
            public const string DeleteFailed = "Xóa thông báo hệ thống thất bại. Vui lòng thử lại.";
            public const string GetFailed = "Lấy thông tin thông báo hệ thống thất bại. Vui lòng thử lại.";
            public const string InvalidId = "ID thông báo hệ thống không hợp lệ.";
            public const string ValidationFailed = "Dữ liệu thông báo hệ thống không hợp lệ.";
            public const string InvalidAnnouncementType = "Loại thông báo không hợp lệ.";
            public const string ExpiryDateInPast = "Ngày hết hạn không được là ngày trong quá khứ.";
            public const string FileUploadFailed = "Upload file đính kèm thất bại.";
            public const string InvalidFileType = "Loại file không được hỗ trợ.";
            public const string FileTooLarge = "File quá lớn. Kích thước tối đa là 10MB.";
        }
        public struct ClubCreationRequest
        {
            public const string RecentRequestExists = "Bạn chỉ có thể gửi đơn tạo CLB 1 lần trong vòng 7 ngày.";
            public const string RequestNotFound = "Yêu cầu tạo câu lạc bộ không tồn tại.";
            public const string AlreadyApproved = "Yêu cầu đã được duyệt";
            public const string NotNullReason = "Lý do không được để trống khi từ chối yêu cầu.";
        }
        public struct Club
        {
            public const string ClubNotFound = "Câu lạc bộ không tồn tại.";
        }
        public struct ClubJoinRequest
        {
            public const string RequestNotFound = "Yêu cầu tham gia câu lạc bộ không tồn tại.";
            public const string AlreadyMember = "Bạn đã là thành viên của câu lạc bộ này.";
            public const string AlreadyPendingRequest = "Bạn đã có yêu cầu tham gia đang chờ xử lý cho câu lạc bộ này.";
            public const string AlreadyApproved = "Yêu cầu đã được duyệt";
            public const string NotTeacher = "Người được mời phải là giáo viên.";
            public const string AlreadyMentor = "Người được mời đã là cố vấn của một câu lạc bộ rồi.";
            public const string AlreadyInvite = "Câu lạc bộ đã mời giáo viên làm cố vấn";
        }
        public struct ClubMember
        {
            public const string NotMember = "Người dùng không phải là thành viên của câu lạc bộ này.";
            public const string CannotRemovePresident = "Không thể xóa chủ nhiệm câu lạc bộ.";
            public const string CannotRemoveMentor = "Không thể xóa cố vấn câu lạc bộ.";
            public const string CannotChangeRoleOfPresident = "Không thể thay đổi vai trò của chủ nhiệm câu lạc bộ.";
            public const string CannotChangeRoleOfMentor = "Không thể thay đổi vai trò của cố vấn câu lạc bộ.";
            public const string InvalidRole = "Vai trò không hợp lệ.";
            public const string CannotChangeRoleForTeacher = "Không thể chuyển vai trò cho giáo viên.";
        }
        public struct Comment
        {
           public const string CommentNotFound = "Bình luận không tồn tại.";
        }

        public struct ClassGroup
        {
            public const string NotFound = "Lớp học không tồn tại.";
            public const string NameAlreadyExists = "Tên lớp học đã tồn tại trong năm bắt đầu này.";
            public const string InvalidData = "Dữ liệu lớp học không hợp lệ.";
            public const string CreateFailed = "Tạo lớp học thất bại. Vui lòng thử lại.";
            public const string UpdateFailed = "Cập nhật lớp học thất bại. Vui lòng thử lại.";
            public const string DeleteFailed = "Xóa lớp học thất bại. Vui lòng thử lại.";
            public const string GetFailed = "Lấy thông tin lớp học thất bại. Vui lòng thử lại.";
            public const string InvalidId = "ID lớp học không hợp lệ.";
            public const string ValidationFailed = "Dữ liệu không hợp lệ.";
            public const string NameRequired = "Tên lớp học là bắt buộc.";
            public const string InitialGradeRequired = "Khối ban đầu là bắt buộc.";
            public const string StartYearRequired = "Năm bắt đầu là bắt buộc.";
            public const string NameTooLong = "Tên lớp học không được vượt quá 10 ký tự.";
            public const string DescriptionTooLong = "Mô tả không được vượt quá 500 ký tự.";
            public const string RoomNumberTooLong = "Số phòng học không được vượt quá 50 ký tự.";
            public const string MaxStudentsInvalid = "Số học sinh tối đa phải từ 1 đến 100.";
            public const string InitialGradeInvalid = "Khối ban đầu phải từ 10 đến 12.";
            public const string StartYearInvalid = "Năm bắt đầu phải từ 2020 đến 2030.";
            public const string HomeroomTeacherNotFound = "Giáo viên chủ nhiệm không tồn tại.";
            public const string SchoolNotFound = "Trường học không tồn tại.";
            public const string CannotDeleteWithStudents = "Không thể xóa lớp học có học sinh.";
            public const string CannotDeleteWithPosts = "Không thể xóa lớp học có bài viết.";
        }

        public struct StudentImport
        {
            // --- Nhóm: Kiểm tra danh sách và mapping ---
            public const string StudentsRequired = "Danh sách học sinh không được để trống.";
            public const string MappingRequired = "Cấu hình mapping trường không được để trống.";
            public const string HeadersRequired = "Danh sách tiêu đề (headers) không được để trống.";
            public const string FieldNotMapped = "Một hoặc nhiều trường bắt buộc chưa được mapping.";

            // --- Nhóm: Kiểm tra dữ liệu cơ bản ---
            public const string FieldEmpty = "Trường bắt buộc không được để trống.";
            public const string InvalidImportMode = "Chế độ import không hợp lệ. Vui lòng chọn insert, upsert hoặc skip.";

            // --- Nhóm: Kiểm tra thông tin học sinh ---
            public const string StudentIdRequired = "Mã học sinh là bắt buộc.";
            public const string InvalidStudentIdFormat = "Định dạng mã học sinh không hợp lệ.";
            public const string FirstNameRequired = "Tên học sinh là bắt buộc.";
            public const string LastNameRequired = "Họ học sinh là bắt buộc.";
            public const string EmailRequired = "Email là bắt buộc.";
            public const string InvalidEmailFormat = "Định dạng email không hợp lệ.";
            public const string InvalidPhoneFormat = "Định dạng số điện thoại không hợp lệ.";
            public const string InvalidDateFormat = "Định dạng ngày sinh không hợp lệ.";

            // --- Nhóm: Kiểm tra tồn tại / trùng lặp ---
            public const string StudentAlreadyExists = "Học sinh đã tồn tại trong hệ thống.";
            public const string StudentNotFound = "Không tìm thấy học sinh trong hệ thống.";

            // --- Nhóm: Kết quả xử lý ---
            public const string ImportFailed = "Quá trình import học sinh thất bại.";
            public const string ValidationFailed = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            public const string CreateStudentFailed = "Không thể tạo mới học sinh.";
            public const string UpdateStudentFailed = "Không thể cập nhật học sinh.";
            public const string TemplateGenerationFailed = "Không thể tạo file template import.";
        }
        public struct Staff
        {
            public const string StaffNotFound = "Nhân viên không tồn tại.";
        }
        public struct Jury
        {
            public const string JuryNotFound = "Ban giám khảo không tồn tại.";
            public const string JuryNotEnough = "Số lượng giám khảo không đủ để thực hiện chấm điểm.";
        }
        public struct Moderation
        {
            public const string ContentViolation = "Nội dung vi phạm môi trường học đường.";
        }
        public struct Submission
        {
            public const string ListSubmissionNotFound = "danh sách bài nộp không tồn tại.";
        }
        public struct Assignment { 
            public const string AssignMentNotFound = "Phân công không tồn tại.";
        }

    }
}
