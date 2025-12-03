# 3.X.X View Club Details

Function trigger:
Navigation path: Club listing page → Click "Xem chi tiết" (View details) button on a club card, or direct URL navigation to club detail page.
Timing demand: On demand (whenever a user wants to view detailed information about a club).
Function description:
Actors/Roles: Teacher, Student.
Purpose: Allow users to view comprehensive information about a club including description, category, members, posts, and activities.
Screen Layout:
Figure X: View Club Details Screen
The club detail page is organized into multiple sections:
Club Banner Section:
Large rectangular banner image displayed at the top spanning full width.
Cover image with club-related illustrations or design.
Overlay with dark semi-transparent background for text readability.
Category Tags:
Two pill-shaped tags positioned at bottom-left of banner:
Orange tag with text "Câu Lạc Bộ" (Club).
Light gray tag with category name (e.g., "Nghệ thuật", "Khoa học").
Club Name:
Large white text displayed prominently below the tags showing club name (e.g., "CLB Mỹ Thuật FPT", "CLB Khoa học").
Action Icons:
Share icon and three-dot menu icon positioned at top-right corner of banner.
Left Panel - Club Information:
Card titled "Thông tin câu lạc bộ" (Club information) with orange info icon.
Club Logo:
Circular image displayed at center (w-24 h-24) with orange border.
Description Section:
Label "Mô tả" (Description) displayed above.
Text content showing club description, truncated to 160 characters initially.
"Xem thêm" (See more) link to expand full description.
"Thu gọn" (Collapse) link to collapse when expanded.
Category Section:
Label "Danh mục" (Category) displayed above.
Orange badge showing category name (e.g., "Nghệ thuật", "Khoa học").
Action Buttons:
Join Club Button: Orange button with person icon and "Tham gia câu lạc bộ" (Join club) text, displayed when user is not a member.
Cancel Request Button: Red button with X icon and "Hủy yêu cầu" (Cancel request) text, displayed when user has pending join request.
Center Panel - Posts and Activities:
Navigation Tabs:
Two tabs: "Bài đăng" (Posts) and "Hoạt động" (Activities).
Active tab is highlighted.
Post Creation Section:
Text input field with user avatar on the left.
Placeholder text: "Bạn đang nghĩ gì, [Name]?" (What are you thinking, [Name]?).
Action buttons below input: "Ảnh/Video" (Photo/Video), "Cảm xúc" (Feeling), "GIF".
Posts List:
List of club posts displayed as cards.
If no posts: Message "Hiện tại chưa có bài đăng nào trong câu lạc bộ." (Currently no posts in the club.) with icon.
Right Panel - Members:
Section titled "Thành viên (X)" (Members (X)) with people icon, where X is member count.
Search Bar:
Input field with placeholder "Tìm kiếm thành viên..." (Search members...).
Member List:
List of members displayed with:
Circular avatar with user's initial letter.
Member name.
Member role badge (e.g., "Chủ nhiệm" (President), "Thành viên" (Member), "Cố vấn" (Advisor)).
Data processing:
User navigates to club detail page via "Xem chi tiết" button or direct URL.
System extracts club ID from URL parameters and fetches club details from database including club basic information, members list with roles, user's membership status, and club posts.
System checks if current user is a member, president, or has pending join request.
System displays club banner with cover image, category tags, and club name.
System displays club information panel with logo, description, and category.
System displays appropriate action button based on user status: "Tham gia câu lạc bộ" if not a member, "Hủy yêu cầu" if has pending request, or member-specific options if is a member.
System displays posts tab with post creation input and list of posts.
System displays members panel with search functionality and member list.
User can interact with various elements: expand description, search members, view posts, create new posts (if member), join club or cancel request.
Function details:
Data: ClubId, ClubName, ClubDescription, CategoryName, AvatarUrl, CoverUrl, MemberCount, Members list with roles, Posts list, UserMembershipStatus, IsPresident, IsRequestToJoin.
Validation: Club must exist. ClubId must be valid. User must be authenticated.
Authentication: JWT token required.
Business rules: Teachers and students can view club details. Description is truncated to 160 characters by default with expand/collapse option. Member list is searchable by name. Posts are displayed only to members (or all users depending on club settings). Action buttons are displayed based on user's membership status. Club information is read-only for non-presidents.
Normal case: User navigates to club detail page => System fetches club data => Club information is displayed => User can view description, members, and posts => User can interact with appropriate action buttons based on membership status.
Abnormal case: User is not authenticated => Redirect to login page. Club not found or invalid ID => Display error message. Network error => Display error message "Lỗi khi tải thông tin câu lạc bộ" (Error loading club information).
