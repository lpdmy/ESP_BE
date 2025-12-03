# 3.X.X View List of Join Requests

Function trigger:
Navigation path: Club Management page → "Yêu cầu tham gia" (Join requests) tab.
Timing demand: On demand (whenever a club manager wants to view pending join requests).
Function description:
Actors/Roles: Club Manager (President of the club).
Purpose: Allow club managers to view and review all pending join requests from users who want to join their club.
Screen Layout:
Figure X: View List of Join Requests Screen
The join requests list is displayed within the Club Management page under the "Yêu cầu tham gia" (Join requests) tab:
Section Title: "Yêu cầu tham gia đang chờ duyệt (X)" (Join requests pending approval (X)) where X is the number of pending requests.
Join Request Cards Grid: Grid layout displaying join request cards in two columns.
Each card contains:
User Avatar: Circular avatar with user's initial letter in white on orange background.
User Information: User name in bold, Student ID below name, Request timestamp in small gray text.
Status Tag: Yellow pill-shaped badge with "Chờ duyệt" (Pending approval) text.
Request Details: Reason for joining and Experience text.
Action Buttons: "Xem chi tiết" (View details), "Chấp nhận" (Accept), and "Từ chối" (Reject) buttons.
Empty State: "Hiện tại chưa có yêu cầu nào" (Currently no requests) message when no requests found.
Data processing:
Club manager navigates to Club Management page and clicks on "Yêu cầu tham gia" (Join requests) tab.
System fetches all pending join requests for the club from the database and displays them in a grid layout.
Each card displays user information, request timestamp, reason for joining, experience, and status badge.
Club manager can review request details, click "Xem chi tiết" to view full information, or click "Chấp nhận" or "Từ chối" to make a decision.
System updates the request count in the section title based on the number of pending requests.
Function details:
Data: ClubId, JoinRequestId, UserId, UserFullName, StudentCode, AvatarUrl, ReasonToJoin, Experience, Status (Pending), CreatedAt, RequestCount.
Validation: Club must exist. User must be the club manager. Only pending requests are displayed. User must be authenticated.
Authentication: JWT token required.
Business rules: Only club managers can view join requests list. Only requests with "Pending" status are displayed. Requests are displayed in order of creation (newest first). Request count is updated in real-time. Club manager can take action on each request directly from the list view.
Normal case: Club manager navigates to join requests tab => System fetches pending requests => Requests are displayed in grid layout => Manager can review details and take action on each request.
Abnormal case: User is not authenticated => Redirect to login page. User is not club president => Display error message. Club not found => Display error message. No pending requests => Display empty state message. Network error => Display error message "Lỗi khi tải danh sách yêu cầu" (Error loading requests list).
