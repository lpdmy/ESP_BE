# Mark Read Notices by Student
Function trigger:
Navigation path: "Thông báo hệ thống" (System Notifications) page → Click on an announcement card or view announcement detail modal → Click "Đã xem" (Mark as read) button, or automatically when viewing announcement detail.
Timing demand: On demand (whenever a student views an announcement and wants to mark it as read) or automatically when viewing announcement details.
Function description:
Actors/Roles: Student.
Purpose: Allow students to mark system announcements/news as viewed/read, tracking which announcements have been seen by each user.
Screen Layout:
Figure X: Mark Read Notices Screen
The mark read notices interface consists of:
Announcement Card: Announcement card displayed on the System Notifications page with title, content preview, type badge, urgent indicator, and creation date. When announcement is unread: Card may have visual indicator (bold text, unread badge). When announcement is read: Card appears with normal styling.
Announcement Detail Modal: Modal dialog appears when user clicks on an announcement card. Modal shows full announcement content, attached files section, and action buttons including "Đã xem" (Mark as read) button and "Đóng" (Close) button.
Data processing:
Student navigates to System Notifications page or views an announcement detail.
System fetches announcements from database and checks localStorage for viewed announcements.
System displays announcements with visual indicators for unread items.
Student clicks on an announcement card or views announcement detail modal.
If announcement is not yet viewed, student clicks "Đã xem" button or viewing detail automatically triggers mark as viewed.
System sends POST request to mark announcement as viewed.
Backend validates and marks announcement as viewed, then returns success response.
Frontend receives success response, saves to localStorage, and updates UI to show announcement as read.
Function details:
Data: AnnouncementId, UserId, ViewedAt (timestamp), ViewedAnnouncements (localStorage structure).
Validation: Announcement must exist. Announcement must be published and not deleted. User must be authenticated. User must be a student.
Authentication: JWT token required.
Business rules: Only students can mark announcements as viewed. Marking as viewed is user-specific. Viewing announcement detail may automatically mark it as viewed. Viewed status is persisted both in database and localStorage. If announcement is already marked as viewed, the operation is idempotent.
Normal case: Student views announcement detail => System checks if already viewed => Student clicks "Đã xem" or viewing automatically triggers mark => POST request sent => Backend marks as viewed => Success response returned => Frontend saves to localStorage => UI updates to show announcement as read.
Abnormal case: User is not authenticated => Redirect to login page. Announcement not found or invalid ID => Display error message. Network error => Display error message "Lỗi khi đánh dấu đã xem" (Error marking as viewed).

