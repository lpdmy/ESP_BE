# Filter Class by Academic Year
Function trigger:
Navigation path: Admin Dashboard → "Lớp học" (Classes) → Select academic year from "Năm học:" (Academic year:) dropdown.
Timing demand: On demand (whenever an admin wants to filter classes by academic year).
Function description:
Actors/Roles: Admin.
Purpose: Allow admins to filter and view classes by academic year to manage classes for different school years.
Screen Layout:
Figure X: Filter Class by Academic Year Screen
The filter interface is displayed within the Classes management page:
Academic Year Dropdown: Dropdown menu labeled "Năm học:" (Academic year:) displaying list of available academic years (e.g., "2020-2021", "2021-2022", "2025-2026", "2030-2031"). Selected year is highlighted.
Statistics Section: Statistics cards showing "Tổng số lớp" (Total classes), "Tổng học sinh" (Total students), and "Giáo viên chủ nhiệm" (Homeroom teachers) for the selected academic year.
Class List: List of classes organized by grade level (e.g., "Khối 10" - Grade 10) displaying classes for the selected academic year.
Data processing:
Admin navigates to Admin Dashboard and selects "Lớp học" (Classes) section.
System fetches all available academic years from database.
Admin selects academic year from "Năm học:" dropdown menu.
System fetches all classes for the selected academic year from database.
System calculates statistics (total classes, total students, homeroom teachers) for the selected academic year.
System displays classes organized by grade level and updates statistics cards.
Function details:
Data: AcademicYear, ClassId, ClassName, StudentCount, HomeroomTeacherName, GradeLevel, TotalClasses, TotalStudents, TotalTeachers.
Validation: Academic year must be valid. User must be authenticated and have Admin role.
Authentication: User must be authenticated and have Admin role.
Business rules: Only admins can filter classes by academic year. Statistics are calculated in real-time for the selected academic year. Classes are displayed organized by grade level. Default academic year is the current year.
Normal case: Admin selects academic year from dropdown => System fetches classes for selected year => Statistics calculated and displayed => Classes displayed organized by grade level.
Abnormal case: User is not authenticated => Redirect to login page. User is not admin => Display error message. Invalid academic year => Display error message. Network error => Display error message "Có lỗi xảy ra khi tải danh sách lớp học" (Error occurred while loading class list).

