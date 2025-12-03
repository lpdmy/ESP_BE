# Edit Class Information by Admin
Function trigger:
Navigation path: Admin Dashboard → "Lớp học" (Classes) → Click edit icon on a class card.
Timing demand: On demand (whenever an admin wants to update class information).
Function description:
Actors/Roles: Admin.
Purpose: Allow admins to update class information including class name, grade, academic year, description, and schedule.
Screen Layout:
Figure X: Edit Class Information Screen
The edit class interface consists of a modal dialog displayed when admin clicks edit icon:
Modal Dialog: White modal dialog titled "Chỉnh sửa lớp học" (Edit Class) centered on screen.
Modal Description: Text "Cập nhật thông tin lớp học. Điền thông tin bắt buộc bên dưới." (Update class information. Fill in the required information below.).
Class Information Section: Four input fields:
Grade Dropdown: Label "Khối *" (Grade *) with current grade selected (e.g., "Khối 10" - Grade 10).
Class Name Input: Label "Tên lớp *" (Class Name *) with current class name (e.g., "A1").
Academic Year Dropdown: Label "Năm học *" (Academic Year *) with current academic year selected (e.g., "2025-2026").
Description Input: Label "Mô tả" (Description) with current description (e.g., "Chuyên Toán" - Math Major).
Schedule Section: Label "Lịch học (Giờ bận)" (Schedule (Busy hours)) with clock icon:
Days of Week Checkboxes: Label "Chọn các ngày trong tuần *" (Select days of the week *) with checkboxes for Monday through Sunday. Some days are pre-checked.
Start Time Input: Label "Giờ bắt đầu *" (Start time *) with time picker (e.g., "07:00 AM").
End Time Input: Label "Giờ kết thúc *" (End time *) with time picker (e.g., "05:00 PM").
Add Schedule Button: Blue button with plus icon and text "+ Thêm lịch học" (Add schedule).
Set Schedule List: Section "Lịch học đã thiết lập:" (Set Schedule:) displaying existing schedules as cards with time range and days, each with a delete icon.
Instructions Section: Yellow lightbulb icon with two bullet points explaining how to add schedule.
Action Buttons: Cancel Button (White) "Hủy" (Cancel) and Update Button (Blue) "Cập nhật lớp học" (Update Class).
Data processing:
Admin navigates to Admin Dashboard and selects "Lớp học" (Classes) section.
Admin clicks edit icon on a class card.
System opens edit class modal dialog, pre-filled with existing class data.
Admin modifies class information fields (grade, class name, academic year, description) or schedule.
Admin can add new schedule or delete existing schedules.
Admin clicks "Cập nhật lớp học" (Update Class) button.
System validates that all required fields are filled, class name is unique for the grade and academic year (if changed), schedule is valid, and user is admin.
If validation passes, system updates Class record and schedules, displays success message, closes modal, and refreshes class list.
Function details:
Data: ClassId, Grade, ClassName, AcademicYear, Description, Schedules (DaysOfWeek, StartTime, EndTime).
Validation: Grade is required. Class name is required and must be unique for the grade and academic year (if changed). Academic year is required. At least one schedule must exist. Start time must be before end time. User must be authenticated and have Admin role.
Authentication: User must be authenticated and have Admin role.
Business rules: Only admins can edit class information. Class name must be unique within the same grade and academic year. At least one schedule must be configured. Multiple schedules can be configured for the same class.
Normal case: Admin modifies class information and schedules => System validates input => Class is updated => Success message displayed => Modal closes => Class list is refreshed.
Abnormal case: User is not authenticated => Redirect to login page. User is not admin => Display error message. Class name already exists => Display error message "Tên lớp đã tồn tại" (Class name already exists). Invalid schedule => Display validation error. Network error => Display error message "Có lỗi xảy ra khi cập nhật lớp học" (Error occurred while updating class).

