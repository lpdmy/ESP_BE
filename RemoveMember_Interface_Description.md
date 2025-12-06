# 3.X.X Remove Member

Function trigger:
Navigation path: Club Management page → "Thành viên" (Members) tab → Click three-dot menu on a member card → Select "Xóa thành viên" (Delete member).
Timing demand: On demand (whenever a club manager wants to remove a member from the club).
Function description:
Actors/Roles: Club Manager (Club President).
Purpose: Allow club managers to remove members from their club, permanently deleting the membership relationship.
Screen Layout:
Figure X: Remove Member Screen
The remove member interface consists of a dropdown menu displayed on member cards:
Three-dot Menu Icon: Three horizontal dots icon positioned on the far right of each member card. Only visible for members with role "Member" (not for President or Mentor).
Dropdown Menu: White background with rounded corners and subtle shadow, positioned below the three-dot icon.
Menu Option: "Xóa thành viên" (Delete member) with red text and trash can icon on the left. Hover effect: Background changes to red-50.
Data processing:
Club manager navigates to Club Management page and clicks on "Thành viên" (Members) tab.
System displays list of club members.
Club manager clicks the three-dot menu icon on a member card and selects "Xóa thành viên" (Delete member) option.
System validates that the user is the club manager, target member exists and is a member of the club, and target member is not the president.
If validation passes, system finds and deletes the ClubMember record from the database, displays success message, and refreshes the members list.
Function details:
Data: UserId (member to be removed), ClubId, CurrentUserId (club manager).
Validation: User must be the club manager. Target member must exist in the club. Target member must not be the president. User must be authenticated.
Authentication: JWT token required.
Business rules: Only club managers can remove members. Presidents cannot be removed. Removal is permanent and cannot be undone. Removed members lose all club-related permissions immediately.
Normal case: Club manager clicks remove option => System validates permissions => Member is removed from database => Success message displayed => Members list is refreshed.
Abnormal case: User is not authenticated => Redirect to login page. User is not club president or member not found => Display error message. Attempting to remove president => Display error message "Không thể xóa chủ nhiệm" (Cannot remove president). Network error => Display error message "Có lỗi xảy ra khi xóa thành viên" (Error occurred while removing member).
