# 3.X.X Update Club Information

Function trigger:
Navigation path: Club Management page → "Thông tin CLB" (Club Information) tab.
Timing demand: On demand (whenever a club manager wants to update club information).
Function description:
Actors/Roles: Club Manager (Club President).
Purpose: Allow club managers to update and modify their club's basic information, including name, description, category, images, and contact details.
Screen Layout:
Figure X: Update Club Information Screen
The update club information form is displayed within the Club Management page under the "Thông tin CLB" (Club Information) tab:
Basic Information Section:
Section title "Thông tin cơ bản" (Basic Information).
Form Fields:
Club Name Field: Required text input with label "Tên CLB _" and placeholder "VD: CLB Lập trình, CLB Toán nâng cao...".
Short Description Field: Required text input with label "Mô tả ngắn _" and placeholder "Tối đa 100 ký tự" (max 100 characters).
Detailed Description Field: Required textarea with label "Mô tả chi tiết _" and placeholder "Mô tả chi tiết về mục tiêu, hoạt động..".
Category Field: Required dropdown select with label "Danh mục _" and placeholder "Chọn danh mục".
Image Upload Sections:
Avatar Image Field: Required upload area with label "Ảnh đại diện _", dashed border, upload icon, and "Chọn ảnh" button.
Cover Image Field: Optional upload area with label "Ảnh bìa (tuỳ chọn)", dashed border, and "Chọn ảnh bìa" button.
Requirements Section: Optional textarea with label "Yêu cầu kiến thức/kỹ năng" and placeholder "VD: Có kiến thức cơ bản về lập trình...".
Contact Information Section:
Contact Email Field: Required email input with label "Email liên hệ _" and placeholder "contact@example.com".
Phone Number Field: Optional text input with label "Số điện thoại" and placeholder "0123456789".
Update Button: Large orange button labeled "Cập nhật" (Update) at the bottom of the form.
Data processing:
Club manager navigates to Club Management page and clicks on "Thông tin CLB" (Club Information) tab.
System fetches current club information from the database and populates form fields with existing club data.
Club manager reviews and modifies the information: updates club name, short description, detailed description, selects category, uploads new images if needed, updates requirements text, updates contact email and phone number.
If new images are selected, system uploads images to storage service and receives URLs.
Club manager clicks "Cập nhật" (Update) button.
System validates required fields: club name, short description (max 100 characters), detailed description, category, avatar image, and contact email format.
If validation passes, system updates club record in database with new information, saves image URLs if new images were uploaded, displays success message, and refreshes the club information display.
Function details:
Data: ClubId, ClubName, ShortDescription, Description, CategoryId, AvatarUrl, CoverUrl, Requirements, ContactEmail, ContactPhone, UpdatedAt.
Validation: Club name is required. Short description is required and must not exceed 100 characters. Detailed description is required. Category is required. Avatar image is required (existing or new). Contact email is required and must be in valid email format. Club must exist. User must be the club manager.
Authentication: JWT token required.
Business rules: Only club managers can update club information. All required fields must be filled. Short description is limited to 100 characters. Images are uploaded to storage before saving club information. If no new image is uploaded, existing image URL is retained.
Normal case: Club manager navigates to club information tab => Form is populated with current data => Manager updates fields => Manager uploads images if needed => Manager clicks update => Validation passes => Club information is updated => Success message displayed.
Abnormal case: User is not authenticated => Redirect to login page. User is not club president or club not found => Display error message. Required field empty or invalid format => Display validation error. Image upload failure or network error => Display error message.
