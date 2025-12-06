# 3.2.X Approve Club Post

Function trigger:
Navigation path: "Club Management" > "Bài đăng" tab > "Duyệt bài" button on pending post card.
Timing demand: On demand (whenever a club manager wants to approve a pending post).
Function description:
Actors/Roles: Club Manager (Club President or Club Mentor).
Purpose: Allow club managers to review and approve posts submitted by club members that are pending approval.
Screen Layout:
Figure X: Pending Posts Management Screen
Navigation tabs: Five horizontal tabs at top (Bài đăng, Thành viên, Yêu cầu tham gia, Hoạt động, Thông tin CLB). "Bài đăng" tab is active.
Section header: "Bài đăng đang chờ duyệt (N)" where N is the count of pending posts.
Post cards container: Scrollable list of post cards displayed vertically.
Post card layout:
Author information section:
Avatar: Circular profile image with gradient background (orange to yellow) or user avatar image.
Author name: Full name displayed in bold.
Role and timestamp: Class group name or "Sinh viên" followed by bullet separator and formatted date-time.
Status badge: Yellow pill-shaped badge with "Chờ duyệt" text positioned on the right side of author info.
Post content section:
Title: Post title displayed in bold large text (if present).
Body: Post content text with preserved line breaks and formatting.
Hashtags: Blue clickable hashtag links displayed below content if present.
Media section (if attachments exist):
Image/Video display: Centered media with max height constraint.
Media type badges: Overlay badges for GIF or VIDEO types.
Navigation arrows: Previous/Next buttons for multiple media items (visible on hover).
Action buttons section:
Approve button: Orange button with checkmark icon and "Duyệt bài" text.
Reject button: Grey button with X icon and "Từ chối" text.
Empty state: "Không có bài đăng nào đang chờ duyệt." message when no pending posts exist.
Data processing:
Club manager navigates to Club Management page and selects "Bài đăng" tab.
System queries database for posts with Status = "Pending" and ClubId matching current club, then displays list of pending posts.
Club manager reviews post content, media, and author information.
Club manager clicks "Duyệt bài" button on a post card.
System validates post exists, belongs to the club, and user has permission to approve posts (must be club manager).
If validation passes, system updates post status from "Pending" to "Approved", refreshes pending posts list, displays success message, and approved post becomes visible in club feed.
Function details:
Data: Post ID, club ID, user role.
Validation: Post must exist, post must belong to the club, post status must be "Pending", user must be club manager (president or mentor), user must be authenticated.
Authentication: User must be authenticated and have Club Manager role (President or Mentor) for the specific club.
Business rules: Only posts with Status = "Pending" can be approved. Approved posts become visible to all club members. Post status changes from "Pending" to "Approved". Post must belong to the club being managed.
Normal case: Valid pending post exists and user has permission => Post status updated to "Approved", post removed from pending list, success message displayed, approved post becomes visible in club feed.
Abnormal case: Post not found or already approved => Display error message. User lacks permission => Display authorization error. Network error => Display "Duyệt bài đăng không thành công" error message.
