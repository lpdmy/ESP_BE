# 3.X.X Cancel Join Request

Function trigger:
Navigation path: Club detail page → Click "Hủy yêu cầu" (Cancel request) button.
Timing demand: On demand (whenever a user wants to cancel their pending join request to a club).
Function description:
Actors/Roles: All authenticated users (Teacher, Student) who have submitted a pending join request.
Purpose: Allow users to cancel their pending join request to a club before it is approved or rejected by the club manager.
Screen Layout:
Figure X: Cancel Join Request Screen
The cancel join request interface consists of a single button component:
Cancel Request Button:
Red button with white X icon and text "Hủy yêu cầu" (Cancel request).
Positioned in the club information section on the left side of the club detail page.
Displayed only when user has a pending join request for the club.
Button has red background (bg-red-500) with hover effect (hover:bg-red-600).
Replaces the "Tham gia câu lạc bộ" (Join club) button when a pending request exists.
Data processing:
User navigates to a club detail page.
System checks if user has a pending join request for the club and displays "Hủy yêu cầu" button if request exists.
User clicks "Hủy yêu cầu" button.
System validates that the join request exists, belongs to the current user, and is in "Pending" status.
If validation passes, system finds the join request by ClubId and UserId and deletes it from the database.
System displays success message: "Hủy yêu cầu tham gia câu lạc bộ thành công" (Successfully cancelled join request).
System refreshes the club detail page and updates the UI (button changes back to "Tham gia câu lạc bộ").
The join request is removed from the club manager's pending requests list.
Function details:
Data: ClubId, UserId, JoinRequestId.
Validation: Join request must exist. Join request must belong to the current user. Join request must be in "Pending" status. User must be authenticated.
Authentication: JWT token required.
Business rules: Only the user who created the join request can cancel it. Only pending requests can be cancelled. Once cancelled, the join request is permanently deleted. After cancellation, user can submit a new join request if desired. Cancelled requests are removed from club manager's view immediately.
Normal case: User has pending join request => User clicks cancel button => System validates request => Join request is deleted => Success message displayed => UI updates to show "Tham gia câu lạc bộ" button => Request removed from club manager's list.
Abnormal case: User is not authenticated => Redirect to login page. Join request not found or already processed => Display error message. Network error => Display error message "Có lỗi xảy ra khi hủy yêu cầu" (Error occurred while cancelling request).
