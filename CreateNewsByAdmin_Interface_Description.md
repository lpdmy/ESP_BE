# 3.2.X Create News by Admin

Function trigger:
Navigation path: "Admin Dashboard" > "Thông báo hệ thống" > "+ Tạo thông báo" button.
Timing demand: On demand (whenever an admin wants to create a new system announcement).
Function description:
Actors/Roles: Admin.
Purpose: Allow admin to create new system announcements with title, content, type, priority, expiration date, and file attachments.
Screen Layout:
Figure X: Create System Notification Modal
Modal container: White rounded card centered on screen with shadow.
Modal header: Title "Tạo thông báo hệ thống" displayed prominently.
Form fields section:
Title field: Required text input with label "Tiêu đề _" and placeholder "Nhập tiêu đề thông báo...".
Notification type field: Required dropdown select with label "Loại thông báo _" and options (Lịch thi, Khẩn cấp, Nghỉ học, Thông báo chung).
Content field: Required rich text editor with label "Nội dung \*" and placeholder "Nhập nội dung thông báo...". Toolbar includes formatting options (Bold, Italic, Underline, H1, H2, H3).
Urgent notification toggle: Switch control with label "Thông báo khẩn cấp" and warning triangle icon.
Expiration date field: Optional datetime input with label "Ngày hết hạn" and calendar icon.
File attachment section: Drag and drop area with dashed border, upload icon, and instructions "Chọn file hoặc kéo thả vào đây". Shows file type hints "PDF, Word, Excel, JPG, PNG (tối đa 10MB)".
Action buttons: Cancel button (White) labeled "Hủy" and Create notification button (Blue) labeled "Tạo thông báo".
Data processing:
Admin navigates to System Notifications page and clicks "+ Tạo thông báo" button.
System displays Create System Notification Modal.
Admin fills in required fields: title, notification type, and content.
Admin optionally toggles urgent notification switch, selects expiration date, and uploads files.
System validates file types and file size, displays selected files.
Admin clicks "Tạo thông báo" button.
System validates title, content, announcement type, and expiry date.
If validation passes, system uploads files to storage, creates Post record with IsSystemAnnouncement = true, Status = Published, sets AnnouncementType, IsUrgent, ExpiryDate, and file attachments, displays success message, closes modal, and refreshes notifications list.
Function details:
Data: Title, content, announcement type, is urgent flag, expiry date (optional), files (optional), user ID.
Validation: Title is required and max 500 characters, content is required, announcement type is required and must be valid, expiry date must not be in the past if provided, file types must be PDF, Word, Excel, JPG, or PNG, file size must not exceed 10MB per file, user must be authenticated and have Admin role.
Authentication: User must be authenticated and have Admin role.
Business rules: When announcement type is "urgent", isUrgent is automatically set to true. System announcements are created with Status = Published and PrivacyLevel = Public. Files are uploaded to storage before announcement creation. Announcement is visible to all users immediately after creation.
Normal case: Valid form data provided => Files uploaded successfully => System announcement created => Success message displayed => Modal closes => Announcement appears in notifications list.
Abnormal case: Title or content missing => Display validation error. Invalid announcement type or expiry date in past => Display error message. File size exceeds limit or invalid file type => Display file error. Network error => Display error message.
