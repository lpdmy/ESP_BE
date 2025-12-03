# Update News by Admin
Function trigger:
Navigation path: Admin Dashboard → "Thông báo" (Notifications) → Click edit icon (pencil) on a notification row.
Timing demand: On demand (whenever an admin wants to update an existing system announcement).
Function description:
Actors/Roles: Admin.
Purpose: Allow admins to update existing system announcements including title, content, expiration date, and file attachments.
Screen Layout:
Figure X: Update News Screen
The update news interface is displayed as a modal dialog centered on the screen:
Modal Dialog: White modal dialog titled "Nội dung và hình thức đánh giá:" (Content and assessment form:) or similar.
Form Fields:
Title Field: Input field labeled "Tiêu đề" (Title) pre-filled with existing title.
Content Field: Large textarea for announcement content pre-filled with existing content.
Expiration Date Field: Optional datetime input labeled "Ngày hết hạn (tùy chọn)" (Expiration date (optional)) with calendar icon.
File Attachment Section: Dashed border upload area labeled "Tệp đính kèm" (Attached file) with text "Chọn file hoặc kéo thả vào đây" (Select file or drag and drop here) and file type hints "PDF, Word, Excel, JPG, PNG (tối đa 10MB)".
Action Buttons: Cancel Button (Gray) with text "Hủy" (Cancel) and Update Button (Blue) with text "Cập nhật thông báo" (Update notification).
Data processing:
Admin navigates to Admin Dashboard and selects "Thông báo" (Notifications) section.
Admin clicks edit icon (pencil) on a notification row.
System opens update modal dialog and pre-fills all existing notification data (title, content, expiration date, attachments).
Admin modifies notification content, title, expiration date, or file attachments as needed.
Admin clicks "Cập nhật thông báo" (Update notification) button.
System validates required fields: title must not be empty, content must not be empty.
If new files are attached, system uploads files to storage service and receives file URLs.
System updates the notification record in database with new data, updates UpdatedAt timestamp, saves file URLs if new files were uploaded, displays success message, and closes modal.
Function details:
Data: NotificationId, Title, Content, ExpirationDate (optional), AttachmentUrls (optional), UpdatedAt.
Validation: Title is required. Content is required. Expiration date must not be in the past if provided. File types must be PDF, Word, Excel, JPG, or PNG. File size must not exceed 10MB per file. User must be authenticated and have Admin role.
Authentication: User must be authenticated and have Admin role.
Business rules: Only admins can update notifications. All required fields must be filled. Files are uploaded to storage before saving notification. If no new file is uploaded, existing file URLs are retained. UpdatedAt timestamp is automatically set to current time.
Normal case: Admin clicks edit icon => Modal opens with existing data => Admin updates fields => Admin uploads files if needed => Admin clicks update => Validation passes => Notification is updated => Success message displayed => Modal closes.
Abnormal case: User is not authenticated => Redirect to login page. User is not admin => Display error message. Notification not found => Display error message. Required field empty => Display validation error. File upload failure or network error => Display error message.

