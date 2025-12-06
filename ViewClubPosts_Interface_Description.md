# 3.X.X View Club Posts

Function trigger:
Navigation path: Club detail page → "Bài đăng" (Posts) tab in the main content area.
Timing demand: On demand (whenever a user wants to view club posts).
Function description:
Actors/Roles: Teacher, Student.
Purpose: Allow users to view posts published in a club, including text content, media attachments, hashtags, and interaction counts.
Screen Layout:
Figure X: View Club Posts Screen
The posts section is displayed in the center panel of the club detail page:
Navigation Tabs: Two tabs "Bài đăng" (Posts) and "Hoạt động" (Activities). "Bài đăng" tab is active.
Post Creation Section: Text input field with user avatar and placeholder "Bạn đang nghĩ gì, [Name]?" (What are you thinking, [Name]?). Action buttons below: "Ảnh/Video" (Photo/Video), "Cảm xúc" (Feeling), "GIF".
Posts List: List of club posts displayed as cards. Each post shows author information (avatar, name, role, timestamp), privacy level (Công khai/Public or Nội bộ/Internal), post content (title, body, hashtags), media attachments if any, and interaction counts (likes, comments).
Empty State: Message "Hiện tại chưa có bài đăng nào trong câu lạc bộ." (Currently no posts in the club.) with icon when no posts exist.
Data processing:
User navigates to club detail page and clicks on "Bài đăng" (Posts) tab.
System fetches club posts from database including post content, author information, media attachments, and interaction counts.
System displays posts list in chronological order (newest first).
User can scroll through posts, view post details, like posts, and comment on posts.
If user is a club member, user can create new posts using the post creation section.
Function details:
Data: ClubId, PostId, PostTitle, PostBody, AuthorId, AuthorName, AuthorRole, PrivacyLevel, Hashtags, AttachmentUrls, LikeCount, CommentCount, CreatedAt.
Validation: Club must exist. User must be authenticated.
Authentication: JWT token required.
Business rules: Teachers and students can view club posts. Posts are displayed in chronological order (newest first). Privacy level "Công khai" (Public) makes post visible to everyone. Privacy level "Nội bộ" (Internal) makes post visible only to club members. Only club members can create new posts.
Normal case: User navigates to club detail page => User clicks "Bài đăng" tab => System fetches and displays club posts => User can view, like, and comment on posts.
Abnormal case: User is not authenticated => Redirect to login page. Club not found => Display error message. Network error => Display error message "Lỗi khi tải bài đăng" (Error loading posts).
