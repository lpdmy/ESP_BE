# 3.X.X Change Member Role

Function trigger:
Navigation path: Club Management page → "Thành viên" (Members) tab → Click three-dot menu on a member card → Select "Chuyển chức vụ" (Change role).
Timing demand: On demand (whenever a club manager wants to transfer presidency to another member).
Function description:
Actors/Roles: Club Manager (Club President).
Purpose: Allow club managers to transfer their presidency role to another member, changing the member's role to President and the current president's role to Member.
Screen Layout:
Figure X: Change Member Role Screen
The change role interface consists of a confirmation modal dialog displayed when user clicks "Chuyển chức vụ":
Modal Dialog: White background with rounded corners and shadow, centered on the screen.
Modal Header: Title "Xác Nhận Chuyển Quyền" (Confirm Transfer Authority) displayed at the top.
Confirmation Message: Text "Bạn có chắc muốn chuyển quyền Chủ nhiệm cho [Member Name] không?" (Are you sure you want to transfer President role to [Member Name]?). Member name is displayed in bold.
Action Buttons: Cancel Button (Gray) with text "Hủy" (Cancel) and Confirm Button (Orange) with text "Xác nhận" (Confirm).
Data processing:
Club manager navigates to Club Management page and clicks on "Thành viên" (Members) tab.
System displays list of club members.
Club manager clicks the three-dot menu icon on a member card and selects "Chuyển chức vụ" (Change role) option.
System opens the confirmation modal dialog and displays confirmation message with the selected member's name.
Club manager can click "Hủy" (Cancel) to close the modal or click "Xác nhận" (Confirm) to proceed.
If user clicks "Xác nhận", system validates that the current user is the club manager, target member exists and is a member of the club, target member is not a Teacher, and target member currently has "Member" role.
If validation passes, system updates target member's role to "President", updates current president's role to "Member", updates club's PresidentUserId, saves changes to database, displays success message, closes modal, and redirects to club detail page.
Function details:
Data: CurrentUserId (club manager), TargetUserId (member to promote), ClubId, NewRole (President), PreviousRole (Member).
Validation: Current user must be the club manager. Target member must exist in the club. Target member must have "Member" role. Target member must not be a Teacher. Club must exist. User must be authenticated.
Authentication: JWT token required.
Business rules: Only club managers can transfer their role. Managers can only transfer to regular members (not to Teachers or Mentors). Role transfer is permanent. When role is transferred, current president becomes a regular member. Club's PresidentUserId is updated to reflect the new president. Only one president can exist per club at a time.
Normal case: Club manager clicks change role option => Confirmation modal opens => Manager confirms transfer => Target member becomes President => Current president becomes Member => Success message displayed => Redirect to club page.
Abnormal case: User is not authenticated => Redirect to login page. User is not club president or target member not found => Display error message. Target member is Teacher or already President => Display error message. Network error => Display error message "Có lỗi xảy ra khi chuyển chức vụ" (Error occurred while changing role).
