# Delete Class Post by Teacher

Function trigger:
Navigation path: "Lớp học của tôi" (My Class) → Select class → Click three-dot menu on a post → Select "Xóa" (Delete).
Timing demand: On demand (whenever a teacher wants to delete their own post).
Function description:
Actors/Roles: Teacher.
Purpose: Allow teachers to delete their own posts in their class.
Screen Layout:
Figure X: Delete Class Post Screen
The delete post interface consists of a confirmation modal dialog displayed when teacher clicks "Xóa" (Delete):
Modal Dialog: White modal dialog titled "Xóa bài viết" (Delete Post) centered on screen with trash can icon on the left and close button (X) on the right.
Confirmation Message: Text "Bạn có chắc chắn muốn xóa bài viết này không?" (Are you sure you want to delete this post?) followed by the post title in quotes.
Action Buttons: Cancel Button (Plain) with text "Hủy" (Cancel) and Delete Button (Red with white text and red border) with text "Xóa bài viết" (Delete Post).
Data processing:
Teacher navigates to "Lớp học của tôi" (My Class) and selects a class.
Teacher clicks three-dot menu on their post and selects "Xóa" (Delete).
System displays confirmation modal dialog with post title.
Teacher clicks "Xóa bài viết" (Delete Post) button.
System validates that post exists, user is teacher, and teacher is the post owner.
If validation passes, system deletes ClassPost record from database, displays success message, closes modal, and refreshes class posts list.
Function details:
Data: PostId, ClassId, TeacherId.
Validation: Post must exist. User must be authenticated and have Teacher role. Teacher must be the post owner.
Authentication: User must be authenticated and have Teacher role.
Business rules: Only post owners can delete their posts. Deletion is permanent and cannot be undone.
Normal case: Teacher confirms deletion => System validates post ownership => Post is deleted => Success message displayed => Modal closes => Post is removed from class posts list.
Abnormal case: User is not authenticated => Redirect to login page. User is not teacher or not post owner => Display error message "Bạn không có quyền xóa bài viết này" (You don't have permission to delete this post). Post not found => Display error message "Bài viết không tồn tại" (Post not found). Network error => Display error message "Có lỗi xảy ra khi xóa bài viết" (Error occurred while deleting post).
