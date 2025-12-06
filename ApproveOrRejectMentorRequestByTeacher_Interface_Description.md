# 3.2.X Approve or Reject Mentor Request by Teacher

Function trigger:
Navigation path: "Club List" > "Lời mời cố vấn" button (for Teachers) or via notification link > Mentor Invitation Modal > "Chấp nhận" or "Từ chối" button.
Timing demand: On demand (whenever a teacher receives a mentor invitation from a club and wants to respond).
Function description:
Actors/Roles: Teacher (Giáo viên).
Purpose: Allow teachers to view mentor invitations from clubs and approve or reject them to become club mentors.
Screen Layout:
Figure X: Mentor Invitation Modal
Modal overlay: Semi-transparent background covering the entire screen.
Modal container: White rounded card centered on screen with shadow and border.
Close button: X icon button in top-right corner of modal.
Modal header:
Title: "Lời mời cố vấn từ câu lạc bộ" displayed prominently.
Description: "Các CLB đã gửi lời mời bạn làm cố vấn. Bạn có thể chấp nhận hoặc từ chối."
Invitations list section:
Scrollable list area with max height constraint (350px).
Empty state: "Hiện tại bạn chưa có lời mời nào." message when no invitations exist.
Invitation items: Each invitation displays:
Club information:
Club avatar: Circular profile image with fallback initial letter.
Club name: Club name displayed in bold.
Invitation date: "Mời vào ngày [date]" text below club name.
Action buttons:
Approve button: Green button with checkmark icon and "Chấp nhận" text.
Reject button: Red outlined button with X icon and "Từ chối" text.
Footer:
Close button: "Đóng" button to dismiss modal.
Data processing:
Teacher navigates to Club List page and clicks "Lời mời cố vấn" button.
System displays Mentor Invitation Modal and queries database for pending mentor invitations.
System displays list of pending mentor invitations with club information.
Teacher reviews invitation details and clicks "Chấp nhận" or "Từ chối" button.
If teacher clicks "Chấp nhận", system validates invitation exists, belongs to teacher, status is "Pending", and teacher is not already a mentor of another club.
If validation passes, system updates ClubJoinRequest status to "Approved", assigns teacher as club mentor, creates ClubMember record with Role "Mentor", and displays success message.
If teacher clicks "Từ chối", system validates invitation exists and belongs to teacher, updates ClubJoinRequest status to "Rejected", and displays success message.
Function details:
Data: Invitation ID (ClubJoinRequest ID), club ID, teacher user ID.
Validation: Invitation must exist, invitation must belong to the teacher, invitation status must be "Pending", teacher must not be mentor of another club (for approval), user must be authenticated and have Teacher role.
Authentication: User must be authenticated and have Teacher role.
Business rules: Teacher can only be mentor of one club at a time. Only pending invitations can be approved or rejected. Once approved, teacher becomes club mentor. Once rejected, invitation status is updated. Approved invitations create ClubMember relationship with Role "Mentor".
Normal case: Valid pending invitation exists and teacher not already mentor => Teacher clicks "Chấp nhận" => Invitation status updated to "Approved", teacher assigned as club mentor, ClubMember created, success message displayed, invitation removed from list. Or teacher clicks "Từ chối" => Invitation status updated to "Rejected", success message displayed, invitation removed from list.
Abnormal case: Invitation not found or already processed => Display error message. Teacher already mentor of another club => Display "Bạn đã là cố vấn của câu lạc bộ khác" error. Network error => Display error message.
