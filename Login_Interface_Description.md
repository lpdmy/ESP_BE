# 3.2.1 Login

Function trigger:
Navigation path: "Login".
Timing demand: On demand (whenever a user wants to access the system).
Function description:
Actors/Roles: All users (Admin, Teacher, Student).
Purpose: Allow users to authenticate and access the system with their credentials.
Screen Layout:
Figure 27: Login Screen
The login screen features a clean and modern design with a light cream-colored background and subtle orange circular decorative elements in the corners. The layout consists of:
Header Section:
EduSphere Logo: Orange graduation cap icon followed by "EduSphere" text in matching orange color, positioned at the top center.
Tagline: "Nền tảng kết nối học sinh THPT FPT School" (Platform connecting FPT High School students) displayed in dark gray text below the logo.
Introduction Link: "Xem giới thiệu" (View introduction) with an orange arrow icon pointing right, positioned below the tagline.
Login Form Panel:
The main login form is presented within a slightly darker cream-colored rectangular panel with rounded corners, centered on the screen.
Form Title: "Đăng nhập" (Login) displayed prominently in large, bold, dark gray font at the top of the panel.
Welcome Message: "Chào mừng bạn quay trở lại!" (Welcome back!) displayed below the title.
Email field (labeled 1):
Label: "Email" displayed above the input field.
Input field: Horizontal rectangle with rounded corners, outlined in red when focused/selected.
Icon: Envelope icon positioned on the left side inside the field.
Placeholder text: "your.email@fpt.edu.vn".
Password field (labeled 2):
Label: "Mật khẩu" (Password) displayed above the input field.
Input field: Horizontal rectangle with rounded corners, outlined in red when focused/selected.
Icon: Padlock icon positioned on the left side inside the field.
Placeholder text: "Nhập mật khẩu" (Enter password).
Toggle button: Eye icon on the right side to show/hide password visibility.
Remember Me and Forgot Password (labeled 3):
Remember Me checkbox: Checkbox (outlined in red when selected) with label "Ghi nhớ đăng nhập" (Remember login), positioned on the left.
Forgot Password link: "Quên mật khẩu?" (Forgot password?) link in lighter gray color, positioned on the right, aligned with the checkbox.
Login button (labeled 4):
Large, prominent "Đăng nhập" (Login) button at the bottom of the form.
Design: Horizontal rectangle with rounded corners, featuring an orange gradient from left to right (orange to darker orange/yellow).
Outlined in red when focused/selected.
Footer Section:
Copyright notice: "© 2025 EduSphere - Nền tảng học tập THPT FPT School" (© 2025 EduSphere - FPT High School learning platform) displayed in small, dark gray text at the bottom center of the page.
Data processing:
The user enters email and password in the login form.
System validates input format and required fields.
System verifies credentials against the database.
If credentials are valid, the system generates JWT tokens and redirects users based on role: Admin → User Management dashboard, Teacher/Student → Home dashboard.
If credentials are invalid, the system displays an "Email or Password is not correct" error message.
Function details:
Data: Email (required), Password (required), Remember me (optional checkbox).
Validation: Email format validation (must match @fpt.edu.vn domain pattern), required fields validation, password strength validation.
Authentication: JWT token generation upon successful login, role-based redirection after authentication.
Business rules: Account must be active, credentials must match database records, email must be in valid format.
Normal case: Valid credentials exist → Login successful, redirect to appropriate dashboard based on user role.
Abnormal case: Invalid credentials or inactive account => Display "Email or Password is not correct" error message. Empty required fields or invalid email format => Display validation error messages.
