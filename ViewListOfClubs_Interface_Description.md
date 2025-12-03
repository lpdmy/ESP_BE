# 3.X.X View List of Clubs

Function trigger:
Navigation path: Navigation menu → "Câu lạc bộ" (Clubs) or direct URL navigation to club listing page.
Timing demand: On demand (whenever a user wants to browse available clubs).
Function description:
Actors/Roles: All authenticated users (Admin, Teacher, Student).
Purpose: Allow users to browse and discover available clubs in a grid layout with search and filter capabilities.
Screen Layout:
Figure X: View List of Clubs Screen
The club listing page is organized into multiple sections:
Page Header:
Large orange title "Câu lạc bộ" (Clubs) displayed at the top.
Subtitle in dark gray: "Khám phá và tham gia các câu lạc bộ phù hợp với sở thích của bạn" (Discover and join clubs that match your interests).
Create Club Button: Orange button with plus icon and text "+ Tạo CLB mới" (Create new club) positioned at top right (visible for Students only).
Search and Filter Section:
Search Bar:
White rectangular input field with magnifying glass icon on the left.
Placeholder text: "Tìm kiếm CLB...." (Search CLB....).
Category Filter Buttons:
Row of rounded buttons displayed below search bar.
Buttons include: "Tất cả" (All), "Công nghệ" (Technology), "Học tập" (Study), "Nghệ thuật" (Arts), "Ngôn ngữ" (Language), "Khoa học" (Science), "Thể thao" (Sports), "Âm nhạc" (Music), "Nhiếp ảnh" (Photography), "Khác" (Other).
Selected category button has orange background with white text.
Unselected category buttons have gray background with dark text.
Club Cards Grid:
Grid layout displaying club cards (1 column on mobile, 2 columns on tablet, 3 columns on desktop).
Each club card contains:
Club Image: Rectangular image at the top (h-48) showing club avatar or cover image.
Category Badge: Small orange pill-shaped badge positioned at top-right corner of image showing category name (e.g., "Khác", "Nghệ thuật", "Khoa học").
Club Name: Bold text displayed below image showing club name (e.g., "CLB Thiên nhiên", "CLB Khoa học").
Short Description: Truncated description text in gray below club name, limited to 2 lines with ellipsis.
Member Count: Users icon followed by number and text "X thành viên" (X members) displayed at bottom.
View Details Button: Orange button with text "Xem chi tiết" (View details) at the bottom of card, spanning full width.
Empty State:
If no clubs found: Centered message "Không tìm thấy CLB nào phù hợp" (No clubs found matching criteria) displayed in gray text.
Pagination:
Page number buttons displayed at bottom center when total count exceeds page size.
Current page button has orange background with white number.
Other page buttons have transparent background with hover effect.
Data processing:
User navigates to club listing page.
System fetches all club categories from database and displays them as filter buttons.
System fetches clubs from database with pagination and checks user's membership status for each club.
System displays clubs in grid layout with club cards.
If user enters search term or selects category filter, system filters clubs accordingly.
User can click pagination buttons to navigate through pages.
Function details:
Data: ClubId, ClubName, ClubShortDescription, ClubDescription, CategoryName, AvatarUrl, MemberCount, IsMember, IsPresident, PageNumber, PageSize, TotalCount.
Validation: PageNumber must be greater than 0. PageSize must be greater than 0. Search term can be empty. Category must exist if selected.
Authentication: JWT token required.
Business rules: All authenticated users can view club list. Clubs are displayed in paginated grid layout. Search and category filtering are performed on client side after fetching data. Default page size is 6 clubs per page. User's membership status is checked for each club. Only active (non-deleted) clubs are displayed. Results are sorted by creation date (newest first). Pagination resets to page 1 when search term or category filter changes.
Normal case: User navigates to club listing page => System fetches clubs and categories => Clubs are displayed in grid layout => User can search, filter, and navigate pages => User can click "Xem chi tiết" to view club details.
Abnormal case: User is not authenticated => Redirect to login page. Network error => Display error message "Lỗi khi tải CLUB. Thử lại sau." (Error loading clubs. Please try again later.). No clubs found => Display empty state message.
