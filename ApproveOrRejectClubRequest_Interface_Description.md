# 3.X.X Approve or Reject Club Request

Function trigger:
Navigation path: Admin Dashboard → "Câu lạc bộ" (Clubs) → "Duyệt bài tạo Câu Lạc Bộ" (Approve Club Creation Posts) tab → Click "Xem chi tiết" (View Details) → Click "Phê duyệt" (Approve) or "Từ chối" (Reject) button.
Timing demand: On demand (whenever an admin wants to approve or reject a club creation request).
Function description:
Actors/Roles: Admin.
Purpose: Allow admins to review and make decisions on pending club creation requests submitted by students.
Screen Layout:
Figure X: Approve or Reject Club Request Screen
The club request detail modal is displayed when admin clicks "Xem chi tiết" (View Details):
Modal Header:
Title: "Chi tiết yêu cầu tạo CLB" (Club creation request details) displayed prominently.
Close Button: X icon positioned at the top right corner to close the modal.
Club Information Section:
Club Name: "Tên CLB" (Club Name) with club name value.
Field: "Lĩnh vực" (Field) with category name.
Request Date: "Ngày gửi yêu cầu" (Request submission date) with formatted date.
Description: "Mô tả" (Description) with full club description text.
Contact Information Section:
Contact Email: "Email liên hệ" (Contact Email) with email address.
Phone Number: "Số điện thoại" (Phone Number) with phone number.
Rejection Reason Section (for rejection):
Text input field: "Lý do từ chối (Admin)" (Reason for rejection (Admin)) with placeholder "Nhập lý do từ chối tại đây..." (Enter reason for rejection here...).
Action Buttons:
Close Button: White button with dark border labeled "Đóng" (Close).
Reject Button: Red button with X icon and "Từ chối" (Reject) text.
Approve Button: Blue button with checkmark icon and "Phê duyệt" (Approve) text.
Data processing:
Admin navigates to Admin Dashboard and selects "Câu lạc bộ" (Clubs) section.
Admin clicks on "Duyệt bài tạo Câu Lạc Bộ" (Approve Club Creation Posts) tab.
System fetches all pending club creation requests from database and displays them in a table.
Admin clicks "Xem chi tiết" (View Details) button on a club request.
System opens modal dialog displaying full club request details including club information and contact information.
Admin reviews club request details.
Admin clicks either "Phê duyệt" (Approve) or "Từ chối" (Reject) button.
If "Phê duyệt" is clicked, system validates request exists and is in "Pending" status, updates ClubCreationRequest status to "Approved", creates new Club record with all club information, creates ClubMember record with requester as President, and displays success message.
If "Từ chối" is clicked, admin can optionally enter rejection reason, system validates request exists, updates ClubCreationRequest status to "Rejected", saves rejection reason if provided, and displays success message.
System refreshes club requests list to reflect updated status.
Function details:
Data: ClubCreationRequestId, ClubName, CategoryId, Description, ContactEmail, ContactPhone, AvatarUrl, CoverUrl, Status (Pending/Approved/Rejected), RejectionReason (optional).
Validation: Request must exist. Request must be in "Pending" status. User must be authenticated and have Admin role.
Authentication: User must be authenticated and have Admin role.
Business rules: Only admins can approve or reject club creation requests. Once approved, a new club is created with the requester as president. Once rejected, request status is updated and club is not created. Rejection reason is optional but recommended.
Normal case: Admin reviews club request details => Admin clicks approve or reject => Request status is updated => Club is created (if approved) => Success message displayed => Request list is refreshed.
Abnormal case: User is not authenticated => Redirect to login page. Request not found or already processed => Display error message. User is not admin => Display error message. Network error => Display error message "Có lỗi xảy ra khi xử lý yêu cầu" (Error occurred while processing request).
