# 3.X.X Search Club

Function trigger:
Navigation path: Club page → Enter search term in search bar or select category filter.
Timing demand: On demand (whenever a user wants to search for clubs by name, description, or category).
Function description:
Actors/Roles: All authenticated users (Admin, Teacher, Student).
Purpose: Allow users to search and filter clubs by keywords or category to find clubs that match their interests.
Screen Layout:
Figure X: Search Club Screen
The search interface is displayed on the Club page with the following components:
Search Bar:
Search input field with magnifying glass icon on the left.
Placeholder text: "Tìm kiếm CLB...." (Search for clubs....).
Positioned at the top of the search and filter section.
Category Filter Buttons:
Row of filter buttons displayed below the search bar.
Buttons include: "Tất cả" (All) highlighted in orange when selected, "Công nghệ" (Technology), "Học tập" (Learning), "Nghệ thuật" (Arts), "Ngôn ngữ" (Language), "Khoa học" (Science), "Thể thao" (Sports), "Âm nhạc" (Music), "Nhiếp ảnh" (Photography), "Khác" (Other).
Selected category button has orange background with white text.
Unselected category buttons have gray background with dark text.
Club Result Cards:
Each club is displayed in a white card with rounded corners and subtle shadow, arranged in a grid layout.
Each card contains:
Club Image: Cover image displayed at the top of the card (e.g., scenic landscape with path and stream).
Category Badge: Small orange badge positioned at top right corner of the image showing category name (e.g., "Khác").
Club Name: Bold text displayed below the image (e.g., "CLB Thiên nhiên").
Short Description: Brief description text below the name (e.g., "CLB Thiên Nhiên là một câu lạc bộ dành cho những bạn trẻ yêu thích...").
Member Count: Users icon followed by number and text "X thành viên" (X members) displayed at the bottom.
View Details Button: Orange button with text "Xem chi tiết" (View details) at the bottom of the card.
Pagination:
Page number buttons displayed at the bottom center.
Current page button has orange background with white number.
Other page buttons have transparent background with hover effect.
Data processing:
User navigates to the Club page.
System fetches all club categories from the database and displays them as filter buttons.
System fetches all clubs from the database with pagination.
User can enter search term in the search bar, click on a category filter button, or combine both search term and category filter.
If search term is entered, system filters clubs by matching the search term against club name or description (case-insensitive).
If category is selected, system filters clubs by matching the selected category name.
If both search term and category are applied, system applies both filters to narrow down results.
System displays filtered results in a grid layout with club cards.
If no results found, system displays message "Không tìm thấy CLB nào phù hợp" (No clubs found matching criteria).
User can click on pagination buttons to navigate through result pages, and system resets to page 1 when search term or category filter changes.
Function details:
Data: SearchTerm (optional), CategoryName (optional), PageNumber, PageSize, ClubId, ClubName, ClubDescription, CategoryId, MemberCount.
Validation: Search term can be empty. Category must exist if selected. PageNumber must be greater than 0. PageSize must be greater than 0.
Authentication: JWT token required.
Business rules: Search is case-insensitive. Search matches club name or description. Category filter is applied when a category is selected. "Tất cả" category shows all clubs. Results are paginated. Empty search term returns all clubs (if no category selected) or all clubs in selected category. Filtering is performed on the client side after fetching data from server.
Normal case: User enters search term or selects category => System filters clubs => Results are displayed in grid layout => User can navigate pages => User can click "Xem chi tiết" to view club details.
Abnormal case: User is not authenticated => Redirect to login page. Network error => Display error message "Lỗi khi tải CLUB. Thử lại sau." (Error loading clubs. Please try again later.). No results found => Display message "Không tìm thấy CLB nào phù hợp" (No clubs found matching criteria).
