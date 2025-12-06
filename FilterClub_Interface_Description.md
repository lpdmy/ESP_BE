# 3.X.X Filter Club

Function trigger:
Navigation path: Club page → Click on a category filter button (e.g., "Công nghệ", "Học tập", "Nghệ thuật", "Khoa học", etc.).
Timing demand: On demand (whenever a user wants to filter clubs by category).
Function description:
Actors/Roles: All authenticated users (Admin, Teacher, Student).
Purpose: Allow users to filter and view clubs by specific categories to find clubs that match their interests.
Screen Layout:
Figure X: Filter Club Screen
The filter interface is displayed on the Club page with the following components:
Category Filter Buttons:
Row of filter buttons displayed below the search bar.
Buttons include: "Tất cả" (All), "Công nghệ" (Technology), "Học tập" (Study), "Nghệ thuật" (Arts), "Ngôn ngữ" (Language), "Khoa học" (Science), "Thể thao" (Sports), "Âm nhạc" (Music), "Nhiếp ảnh" (Photography), "Khác" (Other).
Each button has oval shape with light orange border and light text when unselected.
Selected category button has solid orange background with white text (e.g., "Khoa học" button highlighted in orange).
Unselected category buttons have light orange border with light gray text.
Filtered Club Result Cards:
Each filtered club is displayed in a white card with rounded corners and subtle shadow, arranged in a grid layout.
Each card contains:
Club Image/Banner: Cover image displayed at the top of the card with category-related illustrations (e.g., science club shows atom model, beakers, flasks, test tubes, microscope, magnifying glass, gears on blue background).
Category Badge: Small orange badge positioned at top right corner of the image showing category name in white text (e.g., "Khoa học").
Club Title: Bold dark text displayed below the image (e.g., "CLB Khoa học").
Club Description: Brief description text in lighter gray below the title, truncated with ellipsis if too long (e.g., "Câu lạc bộ Khoa học FPT là nơi hội tụ những bạn trẻ yêu thích khám...").
Member Count: Icon depicting two overlapping human figures followed by number and text "X thành viên" (X members) in light gray text (e.g., "3 thành viên").
View Details Button: Prominent horizontally stretched button with gradient orange-to-yellow background and white text "Xem chi tiết" (View details) at the bottom of the card, with rounded corners.
Data processing:
User navigates to the Club page.
System fetches all club categories from the database and displays them as filter buttons.
System fetches all clubs from the database with pagination.
User clicks on a category filter button (e.g., "Khoa học", "Công nghệ", etc.).
System filters clubs by matching the selected category name with club's category.
If "Tất cả" (All) is selected, system displays all clubs without category filtering.
System displays filtered results in a grid layout showing only clubs that match the selected category.
Selected category button is highlighted with orange background and white text.
Other category buttons remain unselected with light orange border.
System resets to page 1 when category filter changes.
User can click on pagination buttons to navigate through filtered result pages.
If no clubs found for selected category, system displays message "Không tìm thấy CLB nào phù hợp" (No clubs found matching criteria).
Function details:
Data: CategoryName, CategoryId, ClubId, ClubName, ClubDescription, ClubCategory, MemberCount, PageNumber, PageSize.
Validation: Category must exist if selected. PageNumber must be greater than 0. PageSize must be greater than 0.
Authentication: JWT token required.
Business rules: Only one category can be selected at a time. "Tất cả" category shows all clubs without filtering. Category filter is applied on the client side after fetching data from server. Filtering matches club's category name with selected category. Results are paginated. When category changes, page resets to page 1. Selected category button is visually highlighted.
Normal case: User clicks on a category filter button => System filters clubs by category => Filtered results are displayed in grid layout => Selected category button is highlighted => User can navigate pages => User can click "Xem chi tiết" to view club details.
Abnormal case: User is not authenticated => Redirect to login page. Network error => Display error message "Lỗi khi tải CLUB. Thử lại sau." (Error loading clubs. Please try again later.). No clubs found for selected category => Display message "Không tìm thấy CLB nào phù hợp" (No clubs found matching criteria).
