# 3.X.X View Club Members

Function trigger:
Navigation path: Club detail page → Right sidebar "Thành viên" (Members) section.
Timing demand: On demand (whenever a user wants to view the list of club members).
Function description:
Actors/Roles: Teacher, Student.
Purpose: Allow users to view and search through the list of club members, including their names, roles, and avatars.
Screen Layout:
Figure X: View Club Members Screen
The club members section is displayed in the right sidebar of the club detail page:
Section title "Thành viên (X)" (Members (X)) with people icon, where X is the total number of members.
Search Bar: Input field with placeholder text "Tìm kiếm thành viên..." (Search members...).
Members List: Scrollable list displaying each member with circular avatar, full name, and role badge (Chủ nhiệm, Thành viên, Cố vấn).
Data processing:
User navigates to club detail page.
System fetches club details including members list from database.
System displays members section in right sidebar with member count and scrollable list.
If user enters search term, system filters members list in real-time by matching search term against member names.
User can click on a member item to navigate to member's profile page.
Function details:
Data: ClubId, MemberId, MemberFullName, MemberAvatarUrl, MemberRole, MemberCount, SearchTerm.
Validation: Club must exist. User must be authenticated.
Authentication: JWT token required.
Business rules: Teachers and students can view club members list. Search filtering is performed on client side in real-time. Clicking on a member navigates to their profile page.
Normal case: User navigates to club detail page => System fetches club members => Members list is displayed => User can search and view members.
Abnormal case: User is not authenticated => Redirect to login page. Club not found => Display error message. Network error => Display error message.
