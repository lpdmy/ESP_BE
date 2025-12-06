# 3.X.X Approve or Reject Join Request by Club Manager

Function trigger:
Navigation path: Club Management page → "Yêu cầu tham gia" (Join requests) tab → Click "Chấp nhận" (Accept) or "Từ chối" (Reject) button on a join request card.
Timing demand: On demand (whenever a club manager wants to approve or reject a join request).
Function description:
Actors/Roles: Club Manager (President of the club).
Purpose: Allow club managers to review and make decisions on pending join requests from users who want to join their club.
Screen Layout:
Figure X: Approve or Reject Join Request Screen
The join requests section displays within the Club Management page under the "Yêu cầu tham gia" (Join requests) tab.
Section Title:
Title: "Yêu cầu tham gia đang chờ duyệt (X)" (Join requests pending approval (X)) where X is the number of pending requests, displayed at the top of the section.
Join Request Cards:
Each join request is displayed in a white card with rounded corners and subtle shadow, arranged in a grid layout (side-by-side).
Each card contains:
User Avatar:
Circular avatar with user's initial letter in white on orange background (e.g., "G" for "Giáp Hà Văn", "E" for "Em Lưu Văn").
User Information:
User name displayed in bold (e.g., "Giáp Hà Văn", "Em Lưu Văn").
Student ID displayed below name: "MSSV: HS053" or "MSSV: HS097".
Status Tag:
Yellow rectangular tag with rounded corners positioned to the right of the name.
Text: "Chờ duyệt" (Pending approval).
Request Details:
Request Timestamp: "Gửi yêu cầu: HH:mm:ss dd/MM/yyyy" (Request sent: time and date).
Reason for joining: Labeled "Lý do tham gia:" followed by the reason text (e.g., "Yêu thích lĩnh vực hoạt động của CLB.", "Muốn học hỏi").
Experience: Labeled "Kinh nghiệm:" followed by experience text (e.g., "Chưa có kinh nghiệm nhưng rất nhiệt huyết.", "chưa từng có").
Action Buttons:
Two buttons displayed at the bottom of each card:
Accept Button: Orange button with checkmark icon and "Chấp nhận" (Accept) text.
Reject Button: Gray button with X icon and "Từ chối" (Reject) text.
Data processing:
Club manager navigates to Club Management page and clicks on "Yêu cầu tham gia" (Join requests) tab.
System fetches all pending join requests for the club from the database and displays the list.
Club manager reviews join request details and clicks either "Chấp nhận" (Accept) or "Từ chối" (Reject) button.
If "Chấp nhận" is clicked, system validates request exists and is in "Pending" status, updates request status to "Approved", creates ClubMember record with Role "Member", saves changes, displays success message, and refreshes join requests list.
If "Từ chối" is clicked, system validates request exists, updates request status to "Rejected", saves changes, displays success message, and refreshes join requests list.
Function details:
Data: JoinRequestId, ClubId, UserId, Status (Pending/Approved/Rejected), ClubMember record with Role.
Validation: Request must exist. Request must be in "Pending" status for approval. User must be the club manager of the club.
Authentication: JWT token required.
Business rules: Only club managers can approve or reject join requests. Once approved, user becomes a club member with "Member" role. Once rejected, request status is updated but user does not become a member. Approved requests create a ClubMember relationship.
Normal case: Club manager is authenticated and clicks approve/reject => Request status is updated => ClubMember is created (if approved) => Success message displayed => Join requests list is refreshed.
Abnormal case: User is not authenticated => Redirect to login page. Request not found or already approved => Display error message. User is not club manager => Display error message. Network error => Display error message "Có lỗi xảy ra khi xử lý yêu cầu" (Error occurred while processing request).
