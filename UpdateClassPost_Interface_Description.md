# Update Class Post by Teacher
Function trigger:
Navigation path: "Lớp học của tôi" (My Class) → Select class → Click three-dot menu on a post → Select "Chỉnh sửa" (Edit).
Timing demand: On demand (whenever a teacher wants to update their own post).
Function description:
Actors/Roles: Teacher.
Purpose: Allow teachers to update their own posts in their class.
Screen Layout:
Figure X: Update Class Post Screen
The update post interface consists of a modal dialog displayed when teacher clicks "Chỉnh sửa" (Edit):
Modal Dialog: White modal dialog titled "Chỉnh sửa bài viết" (Edit Post) centered on screen with close button (X) in top right corner.
Title Input Field: Optional input field with label "Tiêu đề (Tùy chọn)" (Title (Optional)) pre-filled with existing post title.
Content Text Area: Required text area with label "Nội dung bài viết *" (Post content *) pre-filled with existing post content.
File Attachment Section: Section labeled "Thêm tệp đính kèm mới" (Add new attachment) with "Chọn file" (Choose file) button. Note text "Các tệp đính kèm hiện tại sẽ được giữ nguyên" (Current attachments will be kept).
Action Buttons: Cancel Button (Plain) with text "Hủy" (Cancel) and Update Button (Orange) with paper plane icon and text "Cập nhật bài viết" (Update post).
Data processing:
Teacher navigates to "Lớp học của tôi" (My Class) and selects a class.
Teacher clicks three-dot menu on their post and selects "Chỉnh sửa" (Edit).
System opens update post modal dialog, pre-filled with existing post data.
Teacher modifies title, content, or adds new attachments.
Teacher clicks "Cập nhật bài viết" (Update post) button.
System validates that content is not empty, title does not exceed 500 characters if provided, user is teacher, and teacher is the post owner.
If validation passes, system updates ClassPost record, uploads new attached files if any, displays success message, closes modal, and refreshes class posts list.
Function details:
Data: PostId, ClassId, TeacherId, Title (optional), Content (required), NewAttachmentUrls (optional).
Validation: Content is required and must not be empty. Title is optional but must not exceed 500 characters if provided. User must be authenticated and have Teacher role. Teacher must be the post owner. File size and type must be valid if new files are attached.
Authentication: User must be authenticated and have Teacher role.
Business rules: Only post owners can update their posts. Existing attachments are preserved. New attachments can be added. Post content is required.
Normal case: Teacher updates post content => System validates input => Post is updated => Success message displayed => Modal closes => Updated post appears in class posts list.
Abnormal case: User is not authenticated => Redirect to login page. User is not teacher or not post owner => Display error message "Bạn không có quyền chỉnh sửa bài viết này" (You don't have permission to edit this post). Content is empty => Display validation error. Title exceeds 500 characters => Display validation error. Network error => Display error message "Có lỗi xảy ra khi cập nhật bài viết" (Error occurred while updating post).

