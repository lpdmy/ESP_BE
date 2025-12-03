# Delete News by Admin
Function trigger:
Navigation path: Admin Dashboard → "Thông báo" (Notifications) → Click delete icon (trash can) on a notification row.
Timing demand: On demand (whenever an admin wants to delete a system announcement).
Function description:
Actors/Roles: Admin.
Purpose: Allow admins to permanently delete system announcements from the system.
Screen Layout:
Figure X: Delete News Screen
The delete confirmation interface is displayed as a modal dialog centered on the screen:
Modal Dialog: White modal dialog with red warning triangle icon and title "Xác nhận xóa thông báo" (Confirm notification deletion) in red text.
Confirmation Message: Text "Bạn có chắc chắn muốn xóa thông báo này không? Hành động này không thể hoàn tác." (Are you sure you want to delete this notification? This action cannot be undone.).
Notification Title: Displays "Tiêu đề:" (Title:) followed by the notification title being deleted.
Action Buttons: Cancel Button (White) with text "Hủy" (Cancel) and Delete Button (Red) with trash can icon and text "Xóa vĩnh viễn" (Delete permanently).
Data processing:
Admin navigates to Admin Dashboard and selects "Thông báo" (Notifications) section.
Admin clicks delete icon (trash can) on a notification row.
System opens delete confirmation modal dialog and displays notification title.
Admin can click "Hủy" (Cancel) to close the modal or click "Xóa vĩnh viễn" (Delete permanently) to confirm deletion.
If admin confirms, system validates that notification exists and user is admin.
If validation passes, system deletes the notification record from database, displays success message, closes modal, and refreshes notifications list.
Function details:
Data: NotificationId, Title.
Validation: Notification must exist. User must be authenticated and have Admin role.
Authentication: User must be authenticated and have Admin role.
Business rules: Only admins can delete notifications. Deletion is permanent and cannot be undone. Deleted notifications are removed from all views immediately.
Normal case: Admin clicks delete icon => Confirmation modal opens => Admin confirms deletion => Notification is deleted => Success message displayed => Modal closes => Notifications list is refreshed.
Abnormal case: User is not authenticated => Redirect to login page. User is not admin => Display error message. Notification not found => Display error message. Network error => Display error message "Có lỗi xảy ra khi xóa thông báo" (Error occurred while deleting notification).

