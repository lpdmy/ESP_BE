# 📚 **EduShpere Postman Collection - Hướng dẫn sử dụng**

## 🎯 **Tổng quan**
Postman collection đã được cập nhật với:
- ✅ **Environment variable** `BASE_API_URL` thay vì `base_url`
- ✅ **Multiple login requests** cho Student, Teacher, Admin
- ✅ **User Management folder** với các request tạo user
- ✅ **Organized structure** với các folder rõ ràng

## 📁 **Files cần import**

### 1. **Collection File**
- `EduShpere_API_Collection.postman_collection.json`

### 2. **Environment File** (Tùy chọn)
- `EduShpere_Environment.postman_environment.json`

## 🚀 **Cách import vào Postman**

### **Bước 1: Import Collection**
1. Mở Postman
2. Click **Import** → **Upload Files**
3. Chọn file `EduShpere_API_Collection.postman_collection.json`
4. Click **Import**

### **Bước 2: Import Environment** (Tùy chọn)
1. Click **Import** → **Upload Files**
2. Chọn file `EduShpere_Environment.postman_environment.json`
3. Click **Import**

### **Bước 3: Set Environment**
1. Click vào dropdown **Environment** ở góc trên bên phải
2. Chọn **EduShpere Environment**
3. Hoặc tạo environment mới với variables:
   - `BASE_API_URL`: `https://localhost:7000` (hoặc URL server của bạn)
   - `jwt_token`: (sẽ được set sau khi login)

## 📋 **Cấu trúc Collection**

### **🔐 Authentication**
- **Login as Student** - Đăng nhập với tài khoản student
- **Login as Teacher** - Đăng nhập với tài khoản teacher  
- **Login as Admin** - Đăng nhập với tài khoản admin
- **Get Me** - Lấy thông tin user hiện tại
- **Get All Users (Admin)** - Lấy danh sách tất cả users
- **Change Password** - Đổi mật khẩu
- **Forgot Password** - Quên mật khẩu
- **Test Endpoint** - Test API health

### **👥 User Management**
- **Create Teacher User (Admin)** - Tạo user teacher cơ bản
- **Create Student User (Admin)** - Tạo user student cơ bản
- **Create Admin User (Admin)** - Tạo user admin
- **Create Teacher User with Full Info (Admin)** - Tạo user teacher đầy đủ thông tin
- **Create Student User with Full Info (Admin)** - Tạo user student đầy đủ thông tin

### **👤 Student Profile**
- **Get My Profile** - Lấy profile của user hiện tại
- **Update My Profile** - Cập nhật profile cá nhân
- **Get All Student Profiles (Admin)** - Lấy tất cả student profiles
- **Get Student Profile by ID (Admin)** - Lấy profile theo ID
- **Create Student Profile (Admin)** - Tạo student profile mới
- **Update Student Profile (Admin)** - Cập nhật student profile
- **Delete Student Profile (Admin)** - Xóa student profile
- **Check Student Profile Exists (Admin)** - Kiểm tra profile tồn tại

### **👨‍🏫 Teacher Profile**
- **Get My Teacher Profile** - Lấy teacher profile của user hiện tại
- **Update My Teacher Profile** - Cập nhật teacher profile cá nhân
- **Get All Teacher Profiles (Admin)** - Lấy tất cả teacher profiles
- **Get Teacher Profile by ID (Admin)** - Lấy teacher profile theo ID
- **Create Teacher Profile (Admin)** - Tạo teacher profile mới
- **Update Teacher Profile (Admin)** - Cập nhật teacher profile
- **Delete Teacher Profile (Admin)** - Xóa teacher profile
- **Check Teacher Profile Exists (Admin)** - Kiểm tra teacher profile tồn tại

### **🎯 Activities**
- **Get All Activities** - Lấy danh sách hoạt động
- **Get Activity by ID** - Lấy hoạt động theo ID
- **Create Activity** - Tạo hoạt động mới
- **Update Activity** - Cập nhật hoạt động

### **👥 Activity Participants**
- **Add Participant** - Thêm người tham gia
- **Remove Participant** - Xóa người tham gia

### **📁 File Upload**
- **Upload File** - Upload file lên Cloudinary

## 🔧 **Cách sử dụng**

### **1. Test API Health**
1. Chọn **Test Endpoint** trong folder Authentication
2. Click **Send**
3. Kiểm tra response có status 200

### **2. Login và lấy JWT Token**
1. Chọn **Login as Student/Teacher/Admin**
2. Click **Send**
3. Copy `accessToken` từ response
4. Set vào environment variable `jwt_token`

### **3. Test các endpoints khác**
1. Đảm bảo đã set `jwt_token` trong environment
2. Chọn endpoint muốn test
3. Click **Send**
4. Kiểm tra response

### **4. Tạo users mới**
1. Login với tài khoản Admin
2. Chọn **Create Teacher/Student User** trong folder User Management
3. Modify request body nếu cần
4. Click **Send**

## 🔑 **Environment Variables**

### **Required Variables**
- `BASE_API_URL`: URL của API server
- `jwt_token`: JWT token từ login response

### **Optional Variables** (nếu sử dụng environment file)
- `student_username`: Username cho student
- `student_password`: Password cho student
- `teacher_username`: Username cho teacher
- `teacher_password`: Password cho teacher
- `admin_username`: Username cho admin
- `admin_password`: Password cho admin

## 📝 **Sample Data**

### **Teacher User Example**
```json
{
  "username": "teacher001",
  "email": "teacher001@example.com",
  "password": "Teacher123!",
  "firstName": "Nguyễn Văn",
  "lastName": "Giáo",
  "role": "Teacher",
  "teacherCode": "GV001",
  "department": "Công nghệ thông tin",
  "position": "Giảng viên"
}
```

### **Student User Example**
```json
{
  "username": "student001",
  "email": "student001@example.com",
  "password": "Student123!",
  "firstName": "Trần Thị",
  "lastName": "Sinh viên",
  "role": "Student"
}
```

### **Admin User Example**
```json
{
  "username": "admin001",
  "email": "admin001@example.com",
  "password": "Admin123!",
  "firstName": "Lê Văn",
  "lastName": "Quản trị",
  "role": "Admin"
}
```

## ⚠️ **Lưu ý quan trọng**

### **Authorization**
- Một số endpoints yêu cầu Admin role
- Một số endpoints yêu cầu Teacher role
- Đảm bảo login với đúng role trước khi test

### **JWT Token**
- Token có thời hạn, cần refresh khi hết hạn
- Set token vào environment variable `jwt_token`
- Token được sử dụng tự động cho các protected endpoints

### **Environment**
- Thay đổi `BASE_API_URL` theo môi trường của bạn
- Development: `https://localhost:7000`
- Production: `https://your-api-domain.com`

## 🔄 **Workflow Testing**

### **1. Basic Flow**
1. **Test Endpoint** → Verify API health
2. **Login as Student** → Get JWT token
3. **Get Me** → Verify authentication
4. **Get My Profile** → Check profile data

### **2. Admin Flow**
1. **Login as Admin** → Get admin token
2. **Create Teacher User** → Create new teacher
3. **Get All Users** → Check user management
4. **Create Student Profile** → Test profile management

### **3. Teacher Flow**
1. **Login as Teacher** → Get teacher token
2. **Get My Teacher Profile** → Check teacher profile
3. **Create Activity** → Test activity creation
4. **Get All Activities** → Check activities

## 🎉 **Kết luận**

Postman collection đã được cập nhật đầy đủ với:
- ✅ Environment variables chuẩn
- ✅ Multiple user types và login options
- ✅ Organized folder structure
- ✅ Complete API coverage
- ✅ Ready-to-use sample data

Bây giờ bạn có thể dễ dàng test tất cả API endpoints của EduShpere! 🚀

