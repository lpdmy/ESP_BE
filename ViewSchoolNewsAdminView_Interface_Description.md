# 3.2.X View School News (Admin View)

Function trigger:
Navigation path: "Admin Dashboard" > "Thông báo hệ thống" menu item.
Timing demand: On demand (whenever an admin wants to view and manage system announcements).
Function description:
Actors/Roles: Admin.
Purpose: Allow admin to view all system announcements in a table format with statistics, filter, and manage them (edit, hide/show, delete).
Screen Layout:
Figure X: System Notifications Management Dashboard
Page header:
Title: "Thông báo hệ thống" displayed in large bold text.
Subtitle: "Quản lý tất cả thông báo hệ thống" displayed below title.
Create notification button: Blue button with plus icon and "Tạo thông báo" text positioned at top right.
Statistics cards section:
Four summary cards displayed in a grid:
Total notifications card: Shows total count with document icon and blue color.
Urgent card: Shows count of urgent notifications with warning triangle icon and red color.
Currently displayed card: Shows count of visible notifications with eye icon and green color.
Hidden card: Shows count of hidden notifications with crossed-out eye icon and gray color.
Notifications table:
Table container: White card with rounded corners and overflow handling.
Table header row: Gray background with column headers:
Title column: "Tiêu đề" header.
Type column: "Loại" header.
Priority column: "Ưu tiên" header.
Status column: "Trạng thái" header.
Creation date column: "Ngày tạo" header.
Expiry date column: "Hết hạn" header.
Actions column: "Thao tác" header.
Table body rows: Each row displays:
Title cell: Announcement title in bold text.
Type cell: Badge showing type (Lịch thi, Khẩn cấp, Nghỉ học, Thông báo chung) with color coding.
Priority cell: Badge showing "Khẩn cấp" (red) or "Bình thường" (gray).
Status cell: Badge showing "Đang hiển thị" (green) or "Đã ẩn" (gray).
Creation date cell: Formatted date in Vietnamese locale.
Expiry date cell: Formatted date or "Không giới hạn" text.
Actions cell: Three icon buttons:
Edit button: Pencil icon button (opens edit modal).
Hide/Show button: Eye or crossed-out eye icon button (toggles visibility).
Delete button: Trash icon button (deletes announcement).
Row hover effect: Background color changes on hover.
Data processing:
The admin navigates to Admin Dashboard and selects "Thông báo hệ thống" menu item.
System queries database for all SystemAnnouncement records with pagination, calculates statistics (total count, urgent count, visible count, hidden count), and displays statistics cards with calculated values and icons.
System displays table with all announcements sorted by creation date (newest first).
Each row shows announcement details: title, type, priority, status, dates, and action buttons.
The admin can view announcement details in the table, click edit button to modify an announcement, click hide/show button to toggle announcement visibility, click delete button to remove an announcement, or click "Tạo thông báo" button to create a new announcement.
System refreshes table data after any action (create, edit, delete, toggle visibility).
Function details:
Data: Announcement ID, title, announcement type, priority (urgent/normal), visibility status, creation date, expiry date, pagination parameters.
Validation: User must be authenticated and have Admin role, pagination parameters must be valid.
Authentication: User must be authenticated and have Admin role.
Business rules: All announcements are displayed regardless of visibility status. Announcements are sorted by creation date descending by default. Statistics are calculated in real-time from current data. Only admins can view all announcements including hidden ones. Table supports pagination for large datasets.
Normal case: Admin navigates to page => System loads all announcements => Statistics calculated and displayed => Table populated with announcement data => Admin can view, edit, hide/show, or delete announcements.
Abnormal case: No announcements exist => Table displays empty state. Network error => Display error message "Lấy danh sách thông báo hệ thống thất bại". Unauthorized access => Redirect to login or display access denied message.
