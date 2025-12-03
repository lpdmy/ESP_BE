# 3.2.X Assign Teacher as Club Mentor

Function trigger:
Navigation path: "Club Management" > "Thành viên" tab > "Cố vấn" section > "+ Tìm cố vấn" button.
Timing demand: On demand (whenever a club manager wants to assign a teacher as club mentor).
Function description:
Actors/Roles: Club Manager (Club President).
Purpose: Allow club managers to search for and invite a teacher to become the club mentor (cố vấn).
Screen Layout:
Figure X: Find Mentor Modal
Modal container: White rounded card centered on screen with shadow.
Close button: X icon button in top-right corner of modal.
Modal header: Title "Tìm cố vấn (Mentor)" and description "Nhập tên cố vấn để tìm và mời vào câu lạc bộ."
Search section: Search input field with placeholder "Nhập tên cố vấn..." and orange search button with "Tìm" label.
Results section: Scrollable list area displaying teacher results. Each result shows avatar, full name, and email. Empty state message "Chưa có kết quả tìm kiếm nào." when no results.
Footer: "Đóng" button to dismiss modal.
Data processing:
Club manager navigates to Club Management page and selects "Thành viên" tab.
Club manager clicks "+ Tìm cố vấn" button in the "Cố vấn" section.
System displays Find Mentor Modal.
Club manager enters teacher name in search field and clicks "Tìm" button.
System validates search input is not empty and queries database for teachers matching search term with role = Teacher.
System displays list of matching teachers with avatar, name, and email.
Club manager clicks on a teacher from the results list.
System validates selected user is a Teacher role, teacher is not already a mentor of another club, and invitation does not already exist for this teacher and club.
If validations pass, system creates ClubJoinRequest with Status "Pending" and IsMentor = true, sends notification to selected teacher, closes modal, and displays success message.
If teacher accepts invitation, system updates ClubJoinRequest status to "Approved", assigns teacher as club mentor, and creates ClubMember record with Role "Mentor".
Function details:
Data: Search term (teacher name), selected teacher ID, club ID.
Validation: Search term must not be empty, selected user must have Teacher role, teacher must not be mentor of another club, invitation must not already exist for this teacher and club.
Authentication: User must be authenticated and have Club Manager role for the specific club.
Business rules: Only one mentor per club, teacher can only be mentor of one club at a time, invitation must be pending before approval.
Normal case: Valid teacher selected and not already mentor => Invitation sent successfully, teacher receives notification, teacher accepts => Teacher becomes club mentor.
Abnormal case: Teacher already mentor of another club => Display "Giáo viên đã là cố vấn của câu lạc bộ khác" error. Invitation already exists => Display "Đã gửi lời mời cho giáo viên này" error. Invalid user role => Display "Người dùng không phải là giáo viên" error. Network error => Display error message.
