# 3.X.X Leave Club by Student

Function trigger:
Navigation path: Club detail page → Click three-dot menu icon in the top-right corner of the club banner → Select "Rời câu lạc bộ" (Leave club) option.
Timing demand: On demand (whenever a club member wants to leave the club voluntarily).
Function description:
Actors/Roles: Club members (Student, Teacher) who are currently members of a club. Club president cannot leave the club (must transfer presidency first).
Purpose: Allow club members to voluntarily leave a club, permanently removing their membership and all associated permissions.
Screen Layout:
Figure X: Leave Club Screen
Dropdown Menu:
White background with rounded corners and subtle shadow.
Positioned below the three-dot icon in the top-right corner of the club banner.
Menu options include "Quản lý câu lạc bộ" (Manage club) with gear icon (visible only to club president), "Báo cáo" (Report), "Chia sẻ" (Share), and "Rời câu lạc bộ" (Leave club) with arrow icon pointing out of a box, displayed in red text, indicating a destructive action.
Confirmation Modal:
Modal dialog appears centered on the screen when user clicks "Rời câu lạc bộ".
Title "Xác nhận rời câu lạc bộ" (Confirm leaving the club) displayed prominently at the top.
Close icon (X) positioned at the top-right corner to dismiss the modal.
Warning message: "Bạn có chắc chắn muốn rời khỏi câu lạc bộ này? Hành động này không thể hoàn tác." (Are you sure you want to leave this club? This action cannot be undone.)
"Hủy" (Cancel) button: White button with black border, positioned on the left.
"Xác nhận rời" (Confirm leaving) button: Red button indicating a destructive action, positioned on the right.
Data processing:
User clicks "Rời câu lạc bộ" option from the dropdown menu.
System displays confirmation modal with warning message.
User clicks "Xác nhận rời" button to confirm.
System validates that the user is authenticated, is a current member of the club, and is not the club president.
If validation passes, system finds and deletes the ClubMember record from the database.
System displays success message "Rời câu lạc bộ thành công" (Successfully left the club).
System closes the modal and updates the UI to show non-member view.
Function details:
Data: ClubId, UserId (from JWT token).
Validation: User must be authenticated. User must be a current member of the club. User must not be the club president. Club must exist.
Authentication: JWT token required.
Business rules: Only club members can leave the club. Club president cannot leave without transferring presidency. Leaving is permanent and cannot be undone. Member loses all club-related permissions immediately.
Normal case: User clicks leave option => Confirmation modal appears => User confirms => Membership is removed => Success message displayed => UI updates to show non-member view.
Abnormal case: User is not authenticated => Redirect to login page. User is club president => Display error message "Chủ nhiệm không thể rời câu lạc bộ. Vui lòng chuyển quyền chủ nhiệm trước." (President cannot leave the club. Please transfer presidency first.). Network error => Display error message "Có lỗi xảy ra khi rời câu lạc bộ" (Error occurred while leaving the club).
