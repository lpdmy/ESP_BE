# View Notices by Student and Teacher
Function trigger:
Navigation path: Main navigation → "Thông báo hệ thống" (System Notifications) menu item or click on an announcement card.
Timing demand: On demand (whenever a user wants to view system announcements).
Function description:
Actors/Roles: Student, Teacher.
Purpose: Allow students and teachers to view system announcements including exam schedules, urgent notices, holiday announcements, and general notifications.
Screen Layout:
Figure X: View Notices Screen
The notices interface consists of:
Announcement Cards: List of announcement cards displayed on the page. Each card shows title, content preview, type badge, urgent indicator, and creation date. Unread announcements may have visual indicators (bold text, unread badge).
Announcement Detail Modal: Modal dialog appears when user clicks on an announcement card.
Modal Header: Title, metadata (posted time, announcement type tag), and close button.
Modal Content: Full announcement content including text, images, tables, and formatted information.
Attached Files Section: Section titled "File đính kèm (X):" (Attached files (X)) with file items displayed with file icon, file name, and download icon.
Action Buttons: "Đã xem" (Mark as read) button and "Đóng" (Close) button.
Data processing:
User navigates to "Thông báo hệ thống" (System Notifications) page.
System fetches list of public announcements from database.
System checks localStorage for viewed announcements and displays announcements with visual indicators for unread items.
User clicks on an announcement card to view details.
System opens announcement detail modal and displays full content.
User can mark announcement as read by clicking "Đã xem" button or viewing detail automatically triggers mark as viewed.
System sends POST request to mark announcement as viewed, saves to localStorage, and updates UI to show announcement as read.
Function details:
Data: AnnouncementId, Title, Content, AnnouncementType, IsUrgent, ExpiryDate, AttachmentUrls, CreatedAt, ViewedAnnouncements (localStorage).
Validation: Announcement must exist. Announcement must be published and not deleted. User must be authenticated.
Authentication: JWT token required.
Business rules: Students and teachers can view all published announcements. Viewing announcement detail may automatically mark it as viewed. Viewed status is persisted both in database and localStorage. Unread announcements are highlighted with visual indicators.
Normal case: User navigates to notices page => System fetches announcements => Announcements displayed with unread indicators => User clicks on announcement => Modal opens with full content => User can mark as read => UI updates to show as read.
Abnormal case: User is not authenticated => Redirect to login page. Announcement not found => Display error message. Network error => Display error message "Lỗi khi tải thông báo" (Error loading notifications).

