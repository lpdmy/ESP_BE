# 3.X.X Update Club Post

Function trigger:
Navigation path: Club detail page → Post card → Click three-dot menu → Select "Chỉnh sửa bài đăng" (Edit post).
Timing demand: On demand (whenever a student wants to edit their own post).
Function description:
Actors/Roles: Student (post owner).
Purpose: Allow students to edit their own club posts, including text content, media attachments, hashtags, and privacy settings.
Screen Layout:
Figure X: Update Club Post Screen
The edit post interface is displayed as a modal dialog centered on the screen:
Modal Header:
Title: "Chỉnh sửa bài đăng" (Edit Post) displayed prominently at the top center.
Close Button: X icon positioned at the top right corner to close the modal.
Content Input Area:
Title Input Field: Large text input field for post title (pre-filled with existing title).
Body Input Field: Large textarea for post content (pre-filled with existing content).
Hashtag Section: Field with # icon showing existing hashtags with remove option and placeholder "Thêm #hashtag" (Add #hashtag).
Privacy Selector: Dropdown selector showing current privacy level (e.g., "Công khai" (Public), "Nội bộ" (Internal)).
Attached Media Section: "Ảnh/Video đã chọn" (Selected Photo/Video) showing thumbnails of existing media with remove option.
Media Attachment Options:
Row of buttons: "Ảnh/Video" (Upload Photo/Video), "Biểu cảm" (Emotion/Emoji), "GIF".
Update Button:
Orange button labeled "Cập nhật" (Update) positioned at the bottom right corner of the modal.
Data processing:
User navigates to club detail page and views their own post.
User clicks the three-dot menu icon on the post card and selects "Chỉnh sửa bài đăng" (Edit post).
System opens the edit post modal dialog and pre-fills all existing post data (title, content, hashtags, privacy level, media).
User modifies post content, title, hashtags, privacy level, or media attachments as needed.
User clicks "Cập nhật" (Update) button.
System validates that post body is not empty and user is the post owner.
If new media files are attached, system uploads media files to storage service and receives file URLs.
System updates the Post record with new data and saves to database.
System displays success message: "Cập nhật bài viết thành công" (Successfully updated post).
System closes the modal and refreshes the club posts list.
Function details:
Data: PostId, Title (optional), Body (required), PrivacyLevel (0=Public, 1=Internal), Hashtags (optional), AttachmentUrls (optional), UpdatedAt.
Validation: Post must exist. Post must belong to the current user. Body must not be empty. Title must not exceed 500 characters. User must be authenticated and a student.
Authentication: JWT token required.
Business rules: Only students can edit their own posts. Post body is required, title is optional. Privacy level can be changed. Media attachments can be added, removed, or replaced. Hashtags can be added or removed. Post is updated with current timestamp.
Normal case: User is post owner => User clicks edit option => Modal opens with existing data => User modifies content => User clicks update => Post is updated => Success message displayed => Modal closes => Post appears updated in club feed.
Abnormal case: User is not authenticated => Redirect to login page. Post not found or user is not post owner => Display error message "Bạn không có quyền chỉnh sửa bài đăng này" (You do not have permission to edit this post). Empty body => Display validation error "Vui lòng nhập nội dung bài viết" (Please enter post content). Network error => Display error message "Có lỗi xảy ra khi cập nhật bài đăng" (Error occurred while updating post).
