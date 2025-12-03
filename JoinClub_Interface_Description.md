# 3.X.X Join Club

Function trigger:
Navigation path: Club detail page → Click "Tham gia câu lạc bộ" (Join club) button.
Timing demand: On demand (whenever a user wants to join a club).
Function description:
Actors/Roles: All authenticated users (Teacher, Student).
Purpose: Allow users to submit a join request to become a member of a club by providing their reason for joining and experience.
Screen Layout:
Figure X: Join Club Screen
The join club interface consists of two main components:
Join Club Button:
Orange button with person icon and text "Tham gia câu lạc bộ" (Join club).
Positioned in the club information section on the left side of the club detail page.
Highlighted with red rectangular outline when visible.
Displayed only when user is not already a member and has not submitted a join request.
Join Club Modal:
Modal dialog appears centered on the screen when user clicks the join button.
Modal Header:
Title: "Tham gia Câu lạc bộ" (Join Club) displayed prominently at the top.
Description: "Vui lòng chia sẻ lý do bạn muốn tham gia và kinh nghiệm liên quan." (Please share your reason for joining and related experience.) displayed below the title.
Form Fields:
Reason to Join Field:
Label: "Lý do tham gia" (Reason to join) displayed above the field.
Textarea input with border and rounded corners.
Placeholder text: "Tôi muốn tham gia vì..." (I want to join because...).
3 rows height.
Experience Field:
Label: "Kinh nghiệm" (Experience) displayed above the field.
Textarea input with border and rounded corners.
Placeholder text: "Tôi đã từng tham gia..." (I have participated in...).
3 rows height.
Action Buttons:
Cancel Button: Gray button with text "Huỷ" (Cancel) positioned on the left.
Submit Button: Orange button with text "Gửi yêu cầu" (Send request) positioned on the right.
Data processing:
User navigates to a club detail page.
System checks if user is already a member or has a pending join request and displays "Tham gia câu lạc bộ" button if user is not a member.
User clicks "Tham gia câu lạc bộ" button and system opens the join club modal dialog.
User enters reason for joining and experience in the form fields.
User clicks "Gửi yêu cầu" (Send request) button.
System validates that both reason and experience fields are not empty.
If validation passes, system creates a new ClubJoinRequest record with ClubId, UserId, Status "Pending", ReasonToJoin, Experience, and CreatedAt timestamp.
System saves the join request to the database.
System displays success message: "Gửi yêu cầu tham gia câu lạc bộ thành công" (Successfully sent join request).
System closes the modal and updates the UI (button changes to "Hủy yêu cầu").
Club manager receives the join request and can approve or reject it later.
Function details:
Data: ClubId, UserId, ReasonToJoin, Experience, Status (Pending), CreatedAt.
Validation: Club must exist. User must be authenticated. User must not already be a member of the club. User must not have a pending join request for the same club. ReasonToJoin field must not be empty. Experience field must not be empty.
Authentication: JWT token required.
Business rules: Only authenticated users can submit join requests. A user can only have one active join request per club. Join request status starts as "Pending" and requires club manager approval. Once approved, user becomes a club member with "Member" role. Users cannot join a club they are already members of. Join request is created with current timestamp.
Normal case: User is authenticated and not a member => User clicks join button => User fills form => User submits request => Join request is created with "Pending" status => Success message displayed => Modal closes => Button changes to "Hủy yêu cầu".
Abnormal case: User is not authenticated => Redirect to login page. User is already a member or has pending request => Join button is not displayed or shows "Hủy yêu cầu". Empty reason or experience => Display validation error. Network error => Display error message "Có lỗi xảy ra khi gửi yêu cầu" (Error occurred while sending request).
