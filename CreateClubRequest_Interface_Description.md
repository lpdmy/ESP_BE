# 3.2.X Create Club Request

Function trigger:
Navigation path: "Club List" > "Tạo CLB mới" button or direct navigation to "Create Club" page.
Timing demand: On demand (whenever a student wants to create a new club).
Function description:
Actors/Roles: Student (Sinh viên).
Purpose: Allow students to submit a request to create a new club by filling out club information form and submitting for admin approval.
Screen Layout:
Figure X: Create Club Request Screen
Page header:
Title: "Tạo câu lạc bộ" displayed in large orange gradient text.
Subtitle: "Tạo một cộng đồng học tập và chia sẻ kiến thức" displayed below title.
Main form section (left column):
Basic information section:
Section title: "Thông tin cơ bản" with description "Điền thông tin chi tiết về câu lạc bộ của bạn".
Club name field: Required text input with placeholder "Ví dụ: CLB Lập trình, CLB Toán nâng cao...".
Short description field: Required text input with placeholder "Mô tả ngắn gọn về CLB (tối đa 100 ký tự)" and max length 100 characters.
Detailed description field: Required textarea with placeholder "Mô tả chi tiết về mục tiêu, hoạt động, lợi ích khi tham gia...".
Category field: Required dropdown select with placeholder "Chọn danh mục" showing categories (Công nghệ, Học tập, Nghệ thuật, Ngôn ngữ, Khoa học, Thể thao, Âm nhạc, Nhiếp ảnh, Khác).
Images section:
Section title: "Hình ảnh".
Avatar image upload: Required dashed border upload area with upload icon, "Kéo thả ảnh vào đây hoặc click để chọn" text, "Chọn ảnh" button, and "PNG, JPG tối đa 5MB" file size hint. Shows preview when image selected with remove button.
Cover image upload: Optional dashed border upload area with "Ảnh bìa cho trang chi tiết (tùy chọn)" text and "Chọn ảnh bìa" button. Shows preview when image selected with remove button.
Participation requirements section:
Section title: "Yêu cầu tham gia" with target icon.
Knowledge/skill requirements field: Optional textarea with placeholder "Ví dụ: Có kiến thức cơ bản về lập trình, đam mê học hỏi công nghệ mới...".
Contact information section:
Section title: "Thông tin liên hệ".
Contact email field: Required email input with placeholder "contact@example.com" and email format validation.
Phone number field: Optional phone input with placeholder "0123456789" and phone format validation (10 digits starting with 0).
Terms and conditions:
Checkbox: "Tôi đồng ý với các điều khoản và quy định của trường, cam kết tổ chức các hoạt động tích cực và có ích \*" (required).
Action buttons:
Save draft button: White button with gray border labeled "Lưu nháp".
Create club button: Orange gradient button labeled "Tạo câu lạc bộ" (shows loading spinner when submitting).
Sidebar section (right column):
Preview card:
Title: "Xem trước".
Avatar preview: Gray placeholder or uploaded avatar image.
Club name: Displays entered club name or "Tên CLB" placeholder.
Short description: Displays entered short description or placeholder text.
Category badge: Displays selected category or "Danh mục" placeholder.
Suggestions card:
Title: "Gợi ý" with lightbulb icon.
Tips: Three bullet points with icons covering club name, detailed description, and activity schedule suggestions.
Regulations card:
Title: "Quy định" with info icon.
Rules: Three bullet points listing school regulations and requirements.
Data processing:
Student navigates to Create Club page via "Tạo CLB mới" button or direct URL.
System displays create club form with all required and optional fields.
Student fills in basic information (club name, descriptions, category), uploads avatar image (required) and optionally cover image.
System validates image file type and size, uploads images to storage and stores URLs.
Student optionally fills in participation requirements, enters contact email and phone number.
System validates email format and phone number format if provided.
Student checks terms and conditions checkbox and clicks "Tạo câu lạc bộ" button.
System validates all required fields are filled and checks if user has submitted a club creation request within the last 7 days.
If validation passes, system creates ClubCreationRequest with Status "Pending", stores all form data, and displays success message.
Request is submitted for admin review and approval.
Function details:
Data: Club name, short description, detailed description, category ID, avatar URL, cover URL (optional), requirements (optional), contact email, contact phone (optional), user ID.
Validation: Club name is required, short description is required and max 100 characters, detailed description is required, category is required, avatar image is required (PNG/JPG, max 5MB), email format is validated, phone format is validated if provided (10 digits starting with 0), terms checkbox must be checked, user must not have submitted a request within last 7 days.
Authentication: User must be authenticated and have Student role.
Business rules: Only students can create club requests. User can only submit one club creation request per 7 days. Request status is set to "Pending" upon creation. Request must be approved by admin before club is created. All required fields must be provided.
Normal case: Valid form data and no recent request => Club creation request created with Status = "Pending", success message displayed, request submitted for admin approval.
Abnormal case: Recent request exists (within 7 days) => Display "Bạn đã gửi yêu cầu tạo câu lạc bộ trong 7 ngày qua" error. Required fields missing or invalid format => Display validation errors. Network error => Display error message.
