# 3.X.X Mark Read News by Student

Function trigger:
Navigation path: School News page → Click on an announcement card or view announcement detail modal → Click "Đã xem" (Mark as read) button, or automatically when viewing announcement detail.
Timing demand: On demand (whenever a user views an announcement and wants to mark it as read) or automatically when viewing announcement details.
Function description:
Actors/Roles: All authenticated users (Admin, Teacher, Student).
Purpose: Allow users to mark system announcements/news as viewed/read, tracking which announcements have been seen by each user to provide better user experience and notification management.
Screen Layout:
Figure X: Mark Read News Screen
The mark read news interface consists of multiple components:
Announcement Card:
Announcement card displayed on the School News page with title, content preview, type badge, urgent indicator, and creation date.
When announcement is unread: Card may have visual indicator (e.g., bold text, unread badge, different background color).
When announcement is read: Card appears with normal styling, no unread indicators.
Announcement Detail Modal:
Modal dialog appears when user clicks on an announcement card.
Modal Header:
Title: Announcement title displayed prominently at the top.
Metadata: Posted time and date, announcement type tag (e.g., "Lịch thi" - Exam schedule).
Subtitle: Additional subtitle or category information.
Modal Content:
Full announcement content including text, images, tables, and formatted information.
Attached Files Section:
Section titled "File đính kèm (X):" (Attached files (X)) where X is the number of files.
File items displayed with file icon, file name, and download icon.
Action Buttons:
"Đã xem" (Mark as read) Button: Orange/primary button positioned at bottom-left of modal.
"Đóng" (Close) Button: Secondary button with red border positioned at bottom-right of modal.
When announcement is already read: "Đã xem" button may be disabled or show different state.
Data processing:
User navigates to School News page or views an announcement detail.
System fetches list of public announcements from the database and checks localStorage for viewed announcements.
System displays announcements with visual indicators for unread items.
User clicks on an announcement card or views announcement detail modal.
System extracts current userId from JWT token and checks if announcement is already marked as viewed.
If announcement is not yet viewed, user clicks "Đã xem" button or viewing detail automatically triggers mark as viewed.
System sends POST request to `/api/systemannouncement/{id}/mark-viewed` with announcement ID.
Backend receives request, validates user authentication and announcement validity, then marks announcement as viewed for the user.
Backend returns success response.
Frontend receives success response, saves to localStorage, updates UI state, and removes unread indicators from announcement card.
Function details:
Data: AnnouncementId, UserId, ViewedAt (timestamp), ViewedAnnouncements (localStorage structure: { userId: { announcementId: true } }).
Validation: Announcement must exist. Announcement must be published and not deleted. User must be authenticated. AnnouncementId must be valid integer. UserId must be valid integer.
Authentication: JWT token required.
Business rules: All authenticated users can mark announcements as viewed. Marking as viewed is user-specific (each user has their own viewed status). Viewing announcement detail may automatically mark it as viewed. Users can manually mark announcements as viewed using "Đã xem" button. Viewed status is persisted both in database (backend) and localStorage (frontend for quick access). If announcement is already marked as viewed, the operation is idempotent (no error, just returns success). Viewed status helps filter unread announcements for users. System tracks viewed status per user to provide personalized experience.
Normal case: User views announcement detail => System checks if already viewed => User clicks "Đã xem" or viewing automatically triggers mark => POST request sent to backend => Backend marks as viewed => Success response returned => Frontend saves to localStorage => UI updates to show announcement as read => Unread indicators removed.
Abnormal case: User is not authenticated => Redirect to login page. Announcement not found or invalid ID => Display error message. Network error => Display error message "Lỗi khi đánh dấu đã xem" (Error marking as viewed).
