# 3.X.X Search Club Members

Function trigger:
Navigation path: Club detail page → Right sidebar "Thành viên" (Members) section → Enter search term in search bar.
Timing demand: On demand (whenever a user wants to search for specific club members).
Function description:
Actors/Roles: Teacher, Student.
Purpose: Allow users to search and filter club members by name to quickly find specific members.
Screen Layout:
Figure X: Search Club Members Screen
The search interface is displayed within the members section in the right sidebar:
Search Bar: Input field with placeholder text "Tìm kiếm thành viên..." (Search members...) positioned below the section header.
Members List: Scrollable list displaying filtered members with avatar, name, and role badge.
Data processing:
User navigates to club detail page and views the members section.
User enters search term in the search bar.
System filters members list in real-time by matching search term against member names (case-insensitive).
System updates displayed members list to show only matching results.
User can clear search to view all members again.
Function details:
Data: ClubId, SearchTerm, MemberFullName, MemberAvatarUrl, MemberRole.
Validation: Club must exist. User must be authenticated. Search term can be empty.
Authentication: JWT token required.
Business rules: Teachers and students can search club members. Search filtering is performed on client side in real-time. Search is case-insensitive and matches against full name. Empty search term displays all members.
Normal case: User enters search term => System filters members list => Matching members are displayed => User can view member details.
Abnormal case: User is not authenticated => Redirect to login page. Club not found => Display error message. No results found => Display empty list.
