# Search Class by Admin
Function trigger:
Navigation path: Admin Dashboard → "Lớp học" (Classes) → Enter search term in search bar.
Timing demand: On demand (whenever an admin wants to search for specific classes).
Function description:
Actors/Roles: Admin.
Purpose: Allow admins to search for classes by name or description to quickly find specific classes.
Screen Layout:
Figure X: Search Class Screen
The search interface is displayed within the Classes management page:
Search Bar: Input field with magnifying glass icon and placeholder text "Tìm kiếm lớp học..." (Search for classes...) positioned at the top.
Academic Year Filter: Dropdown menu labeled "Năm học:" (Academic year:) displaying current academic year (e.g., "2025-2026").
Class List: List of classes displayed in cards organized by grade level (e.g., "Khối 10" - Grade 10). Each class card shows class name, student count, homeroom teacher name, and action buttons.
Data processing:
Admin navigates to Admin Dashboard and selects "Lớp học" (Classes) section.
System fetches all classes from database for the selected academic year.
Admin enters search term in the search bar.
System filters classes by matching search term against class name or description (case-insensitive).
System displays filtered results in the class list organized by grade level.
Admin can select different academic year from dropdown to filter by year.
Function details:
Data: SearchTerm, AcademicYear, ClassId, ClassName, StudentCount, HomeroomTeacherName, GradeLevel.
Validation: Search term can be empty. Academic year must be valid. User must be authenticated and have Admin role.
Authentication: User must be authenticated and have Admin role.
Business rules: Only admins can search classes. Search is case-insensitive and matches against class name or description. Results are organized by grade level. Empty search term displays all classes for selected academic year.
Normal case: Admin enters search term => System filters classes => Filtered results are displayed => Admin can view class details.
Abnormal case: User is not authenticated => Redirect to login page. User is not admin => Display error message. Network error => Display error message "Có lỗi xảy ra khi tìm kiếm lớp học" (Error occurred while searching classes).

