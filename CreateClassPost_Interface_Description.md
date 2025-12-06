# Create Class Post by Teacher
Function trigger:
Navigation path: "Lớp học của tôi" (My Class) → Select class → Click "+ Tạo bài viết" (Create Post) button.
Timing demand: On demand (whenever a teacher wants to create a post for their class).
Function description:
Actors/Roles: Teacher.
Purpose: Allow teachers to create posts to share information, announcements, or updates with their class.
Screen Layout:
Figure X: Create Class Post Screen
The create post interface consists of a modal dialog displayed when teacher clicks "+ Tạo bài viết":
Modal Dialog: White modal dialog titled "Tạo bài viết mới" (Create new post) centered on screen.
Title Input Field: Optional input field with label "Tiêu đề (tùy chọn)" (Title (optional)) and character counter showing "X/500 ký tự" (X/500 characters).
Content Text Area: Required text area with label "Nội dung *" (Content *) for the main body of the post.
File Attachment Section: Section labeled "Tệp đính kèm" (Attached file) with "Chọn file" (Choose file) button and upload icon.
Action Buttons: Cancel Button (Gray) with text "Hủy" (Cancel) and Create Button (Orange) with paper plane icon and text "Tạo bài viết" (Create post).
Data processing:
Teacher navigates to "Lớp học của tôi" (My Class) and selects a class.
Teacher clicks "+ Tạo bài viết" (Create Post) button.
System opens create post modal dialog.
Teacher enters optional title and required content, optionally attaches files.
Teacher clicks "Tạo bài viết" (Create post) button.
System validates that content is not empty, title does not exceed 500 characters, user is teacher, and teacher is homeroom teacher of the class.
If validation passes, system creates new ClassPost record, uploads attached files if any, displays success message, closes modal, and refreshes class posts list.
Function details:
Data: ClassId, TeacherId, Title (optional), Content (required), AttachmentUrls (optional).
Validation: Content is required and must not be empty. Title is optional but must not exceed 500 characters if provided. User must be authenticated and have Teacher role. Teacher must be homeroom teacher of the class. File size and type must be valid if files are attached.
Authentication: User must be authenticated and have Teacher role.
Business rules: Only teachers who are homeroom teachers of the class can create posts. Posts are visible to all students in the class. Attached files are optional.
Normal case: Teacher fills in content and optionally title and files => System validates input => Post is created => Success message displayed => Modal closes => Post appears in class posts list.
Abnormal case: User is not authenticated => Redirect to login page. User is not teacher or not homeroom teacher => Display error message "Bạn không có quyền tạo bài viết cho lớp này" (You don't have permission to create posts for this class). Content is empty => Display validation error. Title exceeds 500 characters => Display validation error. Network error => Display error message "Có lỗi xảy ra khi tạo bài viết" (Error occurred while creating post).

