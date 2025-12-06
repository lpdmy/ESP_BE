# Remove Members in Class by Admin
Function trigger:
Navigation path: Admin Dashboard → "Lớp học" (Classes) → Click "Xem chi tiết" (View details) on a class card → Click "Xóa khỏi lớp" (Remove from class) button on a student row.
Timing demand: On demand (whenever an admin wants to remove a student from a class).
Function description:
Actors/Roles: Admin.
Purpose: Allow admins to remove students from a class.
Screen Layout:
Figure X: Remove Members in Class Screen
The remove student interface consists of:
Student List Table: Table displaying students with columns "Mã số học sinh" (Student ID), "Họ tên" (Full Name), "Email", "Ngày sinh" (Date of Birth), and "Thao tác" (Action). Each row has a red "Xóa khỏi lớp" (Remove from class) button with trash can icon in the "Thao tác" column.
Confirmation Modal Dialog: White modal dialog titled "Xác nhận xóa" (Confirm deletion) displayed when admin clicks "Xóa khỏi lớp":
Confirmation Message: Text "Bạn có chắc chắn muốn xóa học sinh "[StudentName]" khỏi lớp [ClassName]?" (Are you sure you want to delete student "[StudentName]" from class [ClassName]?).
Warning Box: Yellow warning box with warning icon stating "Hành động này không thể hoàn tác" (This action cannot be undone) and "Học sinh sẽ bị xóa khỏi lớp và cần được thêm lại nếu muốn quay lại." (The student will be removed from the class and needs to be re-added if you want them back.).
Action Buttons: Cancel Button (Plain) with text "Hủy" (Cancel) and Delete Button (Red) with trash can icon and text "Xóa học sinh" (Delete student).
Data processing:
Admin navigates to class detail page.
Admin clicks "Xóa khỏi lớp" (Remove from class) button on a student row in the student list table.
System displays confirmation modal dialog with student name and class name.
Admin reviews warning message and clicks "Xóa học sinh" (Delete student) button.
System validates that student exists in the class, class exists, and user is admin.
If validation passes, system removes student from the class by deleting ClassStudent record, updates class student count, displays success message, closes modal, and refreshes student list.
Function details:
Data: ClassId, StudentId, StudentName, ClassName.
Validation: Student must exist in the class. Class must exist. User must be authenticated and have Admin role.
Authentication: User must be authenticated and have Admin role.
Business rules: Only admins can remove students from classes. Removal is permanent and requires re-adding the student if they need to return to the class. Student count is updated after removal.
Normal case: Admin clicks remove button => System displays confirmation modal => Admin confirms deletion => Student is removed from class => Success message displayed => Modal closes => Student list is refreshed.
Abnormal case: User is not authenticated => Redirect to login page. User is not admin => Display error message. Student not found in class => Display error message "Học sinh không có trong lớp này" (Student is not in this class). Class not found => Display error message. Network error => Display error message "Có lỗi xảy ra khi xóa học sinh" (Error occurred while removing student).

