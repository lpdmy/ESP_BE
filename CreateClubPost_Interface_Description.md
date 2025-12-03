# 3.X.X Create Club Post

Function trigger:
Navigation path: Club detail page → Click on post creation input field "Bạn đang nghĩ gì, [Name]?" (What are you thinking, [Name]?) or click "Ảnh/Video" (Photo/Video) button.
Timing demand: On demand (whenever a club member wants to create a new post in the club).
Function description:
Actors/Roles: Student (club members).
Purpose: Allow students who are club members to create and publish posts within their club, including text content, media attachments, hashtags, and privacy settings.
Screen Layout:
Figure X: Create Club Post Screen
The create post interface is displayed as a modal dialog centered on the screen:
Modal Header:
Title: "Tạo Bài Đăng Mới" (Create New Post) displayed prominently at the top center.
Close Button: X icon positioned at the top right corner to close the modal.
User Information Section:
User Avatar: Circular avatar icon displayed on the left showing user's profile picture or initial.
User Name: User's full name displayed next to avatar (e.g., "Anh Nguyen").
Privacy Selector: Dropdown selector on the right showing:
Globe icon followed by privacy level text (e.g., "Công khai" (Public), "Nội bộ" (Internal)).
Dropdown arrow indicating the selector can be opened.
Content Input Area:
Title Input Field: Large text input field for post title (e.g., "Lịch họp câu lạc bộ" (Club meeting schedule)).
Body Input Field: Large textarea for post content below title field.
Text can span multiple lines.
Spell check indicators (red wavy underlines) may appear for potential errors.
Additional Options:
Add to Album Option: Button labeled "Thêm vào album" (Add to album) with folder icon and plus sign.
Hashtag Input: Field with # icon for entering hashtags (e.g., "#quantrong" (important)).
Media Attachment Options:
Row of buttons at the bottom of content area:
"Tải lên Ảnh/Video" (Upload Photo/Video) with image icon.
"Biểu cảm" (Emotion/Emoji) with smiley face icon.
"GIF" with gift box icon.
Post Button:
Orange button labeled "Đăng" (Post) positioned at the bottom right corner of the modal.
Used to submit and publish the new post.
Data processing:
User navigates to club detail page and clicks on post creation input field or media attachment button.
System opens the create post modal dialog and displays user information and default privacy setting.
User enters post title (optional) and post content (required).
User can optionally select privacy level, add hashtags, upload media, or add post to album.
User clicks "Đăng" (Post) button.
System validates that post body is not empty.
If media files are attached, system uploads media files to storage service and receives file URLs.
System creates a new Post record with all post data and saves to database.
System displays success message and closes the modal.
If club requires approval, post status is set to "Pending" and requires club manager approval.
Function details:
Data: Title (optional), Body (required), ClubId, UserId, PrivacyLevel (0=Public, 1=Internal), Status (0=Pending, 1=Approved), Hashtags (optional), AttachmentUrls (optional), CreatedAt.
Validation: Body must not be empty. Title must not exceed 500 characters. Club must exist. User must be a student and a member of the club. PrivacyLevel must be valid (0 or 1). Media files must be valid image or video formats.
Authentication: JWT token required.
Business rules: Only students who are club members can create posts. Post body is required, title is optional. Posts by club president are auto-approved. Posts by regular members may require approval depending on club settings. Privacy level "Công khai" (Public) makes post visible to everyone. Privacy level "Nội bộ" (Internal) makes post visible only to club members. Media attachments are uploaded to storage before post creation.
Normal case: User is club member => User opens create post modal => User enters content => User optionally adds media and hashtags => User selects privacy => User clicks "Đăng" => Post is created => Success message displayed => Modal closes => Post appears in club feed.
Abnormal case: User is not authenticated => Redirect to login page. User is not club member => Display error message "Bạn không phải thành viên của câu lạc bộ này" (You are not a member of this club). Empty body => Display validation error "Vui lòng nhập nội dung bài viết" (Please enter post content). Network error => Display error message "Có lỗi xảy ra khi đăng bài" (Error occurred while posting).
