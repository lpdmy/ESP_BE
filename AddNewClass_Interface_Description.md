# Add New Class by Admin
Function trigger:
Navigation path: Admin Dashboard → "Lớp học" (Classes) → Click "+ Thêm lớp mới" (Add new class) button.
Timing demand: On demand (whenever an admin wants to create a new class).
Function description:
Actors/Roles: Admin.
Purpose: Allow admins to create new classes with class information and schedule.
Screen Layout:
Figure X: Add New Class Screen
The add new class interface consists of a modal dialog displayed when admin clicks "+ Thêm lớp mới":
Modal Dialog: White modal dialog titled "Thêm lớp học mới" (Add new class) centered on screen.
Modal Description: Text "Tạo lớp học mới. Điền thông tin bắt buộc bên dưới." (Create a new class. Fill in the required information below.).
Class Information Section: Three required input fields:
Grade Dropdown: Label "Khối *" (Grade *) with placeholder "Chọn khối" (Select grade).
Class Name Input: Label "Tên lớp *" (Class Name *) with placeholder "Nhập tên lớp (VD: A1)" (Enter class name (e.g., A1)).
Academic Year Dropdown: Label "Năm học *" (Academic Year *) with default value "2025-2026".
Schedule Section: Label "Lịch học (Giờ bận)" (Schedule (Busy hours)) with clock icon:
Days of Week Checkboxes: Label "Chọn các ngày trong tuần *" (Select days of the week *) with checkboxes for Monday through Sunday.
Start Time Input: Label "Giờ bắt đầu *" (Start time *) with time picker (default "07:00 AM").
End Time Input: Label "Giờ kết thúc *" (End time *) with time picker (default "05:00 PM").
Add Schedule Button: Blue button with plus icon and text "Thêm lịch học" (Add schedule).
Empty State Message: Text "Chưa có lịch học. Thêm lịch học ở trên." (No schedule yet. Add schedule above.).
Instructions Section: Yellow lightbulb icon with two bullet points explaining how to add schedule.
Action Buttons: Cancel Button (White with blue text) "Hủy" (Cancel) and Create Button (Blue) "Tạo lớp học" (Create class).
Data processing:
Admin navigates to Admin Dashboard and selects "Lớp học" (Classes) section.
Admin clicks "+ Thêm lớp mới" (Add new class) button.
System opens add new class modal dialog.
Admin fills in required fields: selects grade, enters class name, selects academic year.
Admin selects days of week, sets start time and end time, then clicks "Thêm lịch học" (Add schedule) button.
Admin clicks "Tạo lớp học" (Create class) button.
System validates that all required fields are filled, class name is unique for the grade and academic year, schedule is valid, and user is admin.
If validation passes, system creates new Class record with schedule, displays success message, closes modal, and refreshes class list.
Function details:
Data: Grade, ClassName, AcademicYear, DaysOfWeek, StartTime, EndTime.
Validation: Grade is required. Class name is required and must be unique for the grade and academic year. Academic year is required. At least one day of week must be selected. Start time and end time are required. Start time must be before end time. User must be authenticated and have Admin role.
Authentication: User must be authenticated and have Admin role.
Business rules: Only admins can create classes. Class name must be unique within the same grade and academic year. Schedule must have at least one day selected. Multiple days can be selected for the same schedule.
Normal case: Admin fills in all required fields and adds schedule => System validates input => Class is created => Success message displayed => Modal closes => Class list is refreshed.
Abnormal case: User is not authenticated => Redirect to login page. User is not admin => Display error message. Class name already exists => Display error message "Tên lớp đã tồn tại" (Class name already exists). Invalid schedule => Display validation error. Network error => Display error message "Có lỗi xảy ra khi tạo lớp học" (Error occurred while creating class).

