# 3.5.1 Add Posts to Album

Function trigger:
Navigation path: From any post card → Click three-dot menu → Select "Lưu vào bộ sưu tập" (Save to collection).
Timing demand: On demand (whenever a user wants to save a post to their collection/album).
Function description:
Actors/Roles: All authenticated users (Teacher, Student).
Purpose: Allow users to organize and save posts into their personal albums (collections) for easy access and management.
Screen Layout:
Figure X: Add Post to Album Screen
When user clicks "Lưu vào bộ sưu tập", a modal dialog appears centered on the screen.
Modal Header:
Figure X: Favorite Album Modal
Title: "Chọn album" (Select album) displayed prominently at the top.
Data processing:
User clicks the three-dot menu icon on a post card and selects "Lưu vào bộ sưu tập" from the dropdown menu.
System opens the album selection modal, fetches all collections/albums belonging to the current user from the database, and displays the list of existing albums with their preview images and post counts.
User can select an existing album by clicking on it, click "Tạo album mới" to create a new album (redirects to album creation flow), or click "Không thêm vào album" to cancel.
If user selects an existing album, system validates that the post is not already in the selected album (optional duplicate check), creates a new CollectionItem record linking the post to the selected collection, saves the relationship to the database, displays a success message "thêm mục vào bộ sưu tập thành công" (Successfully added item to collection), closes the modal and updates the UI.
Function details:
Data: PostId, CollectionId, UserId, CollectionItem record with timestamp.
Validation: Post must exist and not be deleted. Collection must exist and belong to the current user. Post must not already be in the selected collection (optional duplicate prevention).
Authentication: JWT token required.
Business rules: Only authenticated users can save posts to collections. Users can only save posts to their own collections. A post can be saved to multiple collections. CollectionItem is created with current timestamp and user ID.
Normal case: User is authenticated and selects an existing album => Post is successfully added to collection => Success message displayed => Modal closes.
Abnormal case: User is not authenticated => Redirect to login page. Post not found or collection not found => Display error message. Network error => Display error message "Có lỗi xảy ra khi lưu bài đăng" (Error occurred while saving post).
