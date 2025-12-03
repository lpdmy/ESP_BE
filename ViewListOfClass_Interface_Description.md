# View List of Class by Admin
Function trigger:
Navigation path: Admin Dashboard → "Lớp học" (Classes) menu item.
Timing demand: On demand (whenever an admin wants to view the list of classes).
Function description:
Actors/Roles: Admin.
Purpose: Allow admins to view and manage all classes organized by grade level and academic year.
Screen Layout:
Figure X: View List of Class Screen
The class list interface consists of:
Page Header: Title "Lớp học" (Classes) with subtitle "Quản lý danh sách lớp học theo năm học và khối" (Manage class list by school year and grade).
Search and Filter Section: Search bar with placeholder "Tìm kiếm lớp học..." (Search for class...), Academic Year dropdown "Năm học:" (Academic year:), and "Thêm lớp mới" (Add new class) button.
Statistics Section: Three cards displaying "Tổng số lớp" (Total classes), "Tổng học sinh" (Total students), and "Giáo viên chủ nhiệm" (Homeroom teachers) for selected academic year.
Class List: Classes organized by grade level (e.g., "Khối 10" - Grade 10). Each grade section shows number of classes and total students. Class cards displayed in grid layout showing class name, student count, homeroom teacher name, and action buttons (edit, delete, view details).
Data processing:
Admin navigates to Admin Dashboard and selects "Lớp học" (Classes) section.
System fetches all classes from database for the selected academic year.
System calculates statistics (total classes, total students, homeroom teachers) for the selected academic year.
System displays classes organized by grade level in grid layout.
Admin can search classes, filter by academic year, view class details, edit classes, or delete classes.
Function details:
Data: AcademicYear, ClassId, ClassName, StudentCount, HomeroomTeacherName, GradeLevel, TotalClasses, TotalStudents, TotalTeachers.
Validation: Academic year must be valid. User must be authenticated and have Admin role.
Authentication: User must be authenticated and have Admin role.
Business rules: Only admins can view class list. Classes are displayed organized by grade level. Statistics are calculated in real-time for the selected academic year. Default academic year is the current year.
Normal case: Admin navigates to classes page => System fetches classes for selected academic year => Statistics calculated and displayed => Classes displayed organized by grade level => Admin can view, edit, or delete classes.
Abnormal case: User is not authenticated => Redirect to login page. User is not admin => Display error message. Network error => Display error message "Có lỗi xảy ra khi tải danh sách lớp học" (Error occurred while loading class list).

