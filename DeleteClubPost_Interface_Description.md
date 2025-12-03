# 3.X.X Delete Club Post

Function trigger:
Navigation path: Club detail page → Post card → Click three-dot menu → Select "Xóa bài đăng" (Delete post).
Timing demand: On demand (whenever a student wants to delete their own post).
Function description:
Actors/Roles: Student (post owner).
Purpose: Allow students to permanently delete their own posts from the club, removing them from the feed and database.
Screen Layout:
Figure X: Delete Club Post Screen
The delete confirmation interface is displayed as a modal dialog centered on the screen:
Modal Header:
Warning Icon: Red warning triangle icon in a red circular background positioned at the top left.
Title: "Xóa bài đăng" (Delete post) displayed in bold black text next to the warning icon.
Close Button: X icon positioned at the top right corner to close the modal.
Main Content:
Confirmation Message: Question text displayed in gray: "Bạn có chắc chắn muốn xóa bài đăng này không? Hành động này không thể hoàn tác." (Are you sure you want to delete this post? This action cannot be undone.)
Warning Box:
Rectangular box with light yellow background and yellow border.
Yellow warning triangle icon on the left.
Warning text: "Lưu ý: Bài đăng này sẽ bị xóa vĩnh viễn và không thể khôi phục." (Note: This post will be permanently deleted and cannot be recovered.)
Action Buttons:
Cancel Button: White button with light gray border and black text "Hủy" (Cancel) positioned on the left.
Delete Button: Red button with white trash can icon and white text "Xóa bài đăng" (Delete post) positioned on the right.
Background: Blurred backdrop with semi-transparent overlay showing the underlying page content.
Data processing:
User navigates to club detail page and views their own post.
User clicks the three-dot menu icon on the post card and selects "Xóa bài đăng" (Delete post).
System opens the delete confirmation modal dialog and displays warning message.
User can click "Hủy" (Cancel) to close the modal or click "Xóa bài đăng" (Delete post) to confirm.
If user confirms, system validates that the post exists and user is the post owner.
If validation passes, system performs soft delete, saves changes, displays success message, closes modal, and refreshes posts list.
Function details:
Data: PostId, UserId, ClubId, IsDeleted flag.
Validation: Post must exist. Post must not already be deleted. User must be the post owner. User must be authenticated and a student.
Authentication: JWT token required.
Business rules: Only students can delete their own posts. Deletion is permanent (soft delete with IsDeleted flag). Deleted posts are removed from public view immediately. Deletion action cannot be undone.
Normal case: User is post owner => User clicks delete option => User confirms deletion => Post is soft deleted => Success message displayed => Modal closes => Post removed from feed.
Abnormal case: User is not authenticated => Redirect to login page. Post not found or user lacks permission => Display error message. Network error => Display error message "Có lỗi xảy ra khi xóa bài đăng" (Error occurred while deleting post).
