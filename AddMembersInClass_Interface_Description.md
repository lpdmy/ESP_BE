# Add Members in Class by Admin
Function trigger:
Navigation path: Admin Dashboard → "Lớp học" (Classes) → Click "Xem chi tiết" (View details) on a class card → Click "+ Thêm học sinh" (Add student) button.
Timing demand: On demand (whenever an admin wants to add a student to a class).
Function description:
Actors/Roles: Admin.
Purpose: Allow admins to add students to a class by entering their email address.
Screen Layout:
Figure X: Add Members in Class Screen
The add student interface consists of a modal dialog displayed when admin clicks "+ Thêm học sinh":
Modal Dialog: White modal dialog titled "Thêm học sinh vào lớp" (Add student to class) centered on screen.
Modal Subtitle: Text "Thêm học sinh vào lớp [ClassName]. Nhập email học sinh để thêm vào lớp." (Add students to class [ClassName]. Enter student email to add to class.).
Email Input Field: Required input field with label "@ Email học sinh *" (Student email *) and placeholder "Nhập email học sinh (ví dụ: student@email.com)" (Enter student email (e.g., student@email.com)).
Instruction Text: Two lines of text below input field explaining that student must be registered in system and cannot be in multiple classes in same academic year.
Action Buttons: Cancel Button (Gray) with text "Hủy" (Cancel) and Add Button (Blue) with plus icon and text "+ Thêm học sinh" (Add student).
Data processing:
Admin navigates to class detail page and clicks "+ Thêm học sinh" (Add student) button.
System opens add student modal dialog.
Admin enters student email in the input field.
Admin clicks "+ Thêm học sinh" (Add student) button.
System validates that email is not empty, email format is valid, student exists in system, student is not already in another class in the same academic year, and user is admin.
If validation passes, system adds student to the class by creating ClassStudent record, updates class student count, displays success message, closes modal, and refreshes student list.
Function details:
Data: ClassId, StudentEmail, AcademicYear, StudentId.
Validation: Email is required and must be in valid format. Student must exist in system. Student must not be in another class in the same academic year. Class must exist. User must be authenticated and have Admin role.
Authentication: User must be authenticated and have Admin role.
Business rules: Only admins can add students to classes. Students must be registered in the system. Students cannot be in multiple classes in the same academic year. Student is added immediately upon successful validation.
Normal case: Admin enters valid student email => System validates student exists and not in another class => Student is added to class => Success message displayed => Modal closes => Student list is refreshed.
Abnormal case: User is not authenticated => Redirect to login page. User is not admin => Display error message. Student not found => Display error message "Học sinh không tồn tại" (Student not found). Student already in another class => Display error message "Học sinh đã có trong lớp khác" (Student is already in another class). Invalid email format => Display validation error. Network error => Display error message "Có lỗi xảy ra khi thêm học sinh" (Error occurred while adding student).

