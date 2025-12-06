# View Class Details by Admin

Function trigger:
Navigation path: Admin Dashboard → "Lớp học" (Classes) → Click "Xem chi tiết" (View details) button on a class card.
Timing demand: On demand (whenever an admin wants to view detailed information about a specific class).
Function description:
Actors/Roles: Admin.
Purpose: Allow admins to view detailed information about a class including class information, schedule, and student list.
Screen Layout:
Figure X: View Class Details Screen
The class details interface consists of:
Page Header: "Quay lại" (Go back) link with left arrow icon at the top.
Class Header: Class name "Lớp [ClassName]" (Class [ClassName]) with grade level "Khối [Grade]" (Grade [Grade]) and subject/specialization displayed prominently.
Summary Cards: Two cards displaying quick statistics:
Student Count Card: Person icon with number and label "Số học sinh" (Number of students).
Homeroom Teacher Card: Graduation cap icon with teacher name and label "Giáo viên chủ nhiêm" (Homeroom teacher), with view and delete icons on the right.
Class Information Section: Card titled "Thông tin lớp học" (Class information) displaying class subject/specialization.
Schedule Section: Card titled "Lịch học" (Schedule) displaying class time (e.g., "07:00 - 11:00") and days of week (e.g., "Thứ 2, Thứ 3, Thứ 4, Thứ 5, Thứ 6" - Monday through Friday).
Student List Section: Card titled "Danh sách học sinh ([Count])" (List of students ([Count])):
Add Student Button: Blue button "+ Thêm học sinh" (Add student) on the right side of the section title.
Student Table: Table with columns "Mã số học sinh" (Student ID), "Họ tên" (Full name), "Email", "Ngày sinh" (Date of birth), and "Thao tác" (Actions). Each row shows student information with a delete action "Xóa khỏi lớp" (Remove from class).
Data processing:
Admin navigates to Admin Dashboard and selects "Lớp học" (Classes) section.
Admin clicks "Xem chi tiết" (View details) button on a class card.
System fetches class details from database including class information, schedule, homeroom teacher, and student list.
System displays class details page with all information organized in sections.
Admin can view class information, schedule, and student list, add students, remove students, or edit class information.
Function details:
Data: ClassId, ClassName, Grade, Subject, AcademicYear, Schedule, HomeroomTeacherId, StudentId, StudentName, StudentEmail, StudentDateOfBirth.
Validation: Class must exist. User must be authenticated and have Admin role.
Authentication: User must be authenticated and have Admin role.
Business rules: Only admins can view class details. All class information, schedule, and student list are displayed. Admin can perform actions like adding or removing students from this page.
Normal case: Admin clicks "Xem chi tiết" on a class card => System fetches class details => Class details page is displayed with all information => Admin can view and manage class.
Abnormal case: User is not authenticated => Redirect to login page. User is not admin => Display error message. Class not found => Display error message "Lớp học không tồn tại" (Class not found). Network error => Display error message "Có lỗi xảy ra khi tải thông tin lớp học" (Error occurred while loading class information).
