# 👨‍🎓 **Student Sample Data - Hướng dẫn sử dụng**

## 📋 **Tổng quan**
Bộ dữ liệu mẫu cho việc tạo student users trong hệ thống EduShpere, bao gồm các trường hợp khác nhau từ cơ bản đến nâng cao.

## 📁 **Files**

### 1. **`student_sample_data.json`**
- **Mô tả**: Dữ liệu mẫu chi tiết với 5 student examples
- **Nội dung**: 
  - Student cơ bản (năm 1)
  - Student đầy đủ thông tin (năm 2) 
  - Student năm cuối (năm 4)
  - Student nữ (năm 1)
  - Student có kinh nghiệm (năm 3)
  - Role mapping và class group samples
  - Usage notes chi tiết

### 2. **`simple_student_samples.json`**
- **Mô tả**: Dữ liệu mẫu đơn giản với 3 student examples
- **Nội dung**:
  - Student cơ bản
  - Student nâng cao
  - Student sắp tốt nghiệp

## 🎯 **Cách sử dụng**

### **Option 1: Copy từ file JSON**
1. Mở file `student_sample_data.json` hoặc `simple_student_samples.json`
2. Copy JSON object của student muốn tạo
3. Paste vào Postman request body
4. Modify thông tin nếu cần
5. Send request

### **Option 2: Sử dụng trong Postman**
1. Import Postman collection
2. Chọn **Create Student User (Admin)**
3. Replace request body với JSON mẫu
4. Send request

### **Option 3: Sử dụng trong code**
```csharp
var studentData = new CreateUserDto
{
    Username = "student001",
    Email = "student001@example.com",
    FirstName = "Nguyễn Văn",
    LastName = "An",
    PhoneNumber = "+84901234567",
    Role = UserRole.Student,
    StudentNumber = "SV2024001",
    EnrollmentYear = 2024,
    Bio = "Sinh viên năm 1 chuyên ngành Công nghệ thông tin",
    ExtraJson = "{\"hobbies\":[\"Lập trình\",\"Đọc sách\"],\"skills\":[\"C#\",\"JavaScript\"]}",
    AvatarUrl = "https://example.com/avatars/student001.jpg",
    BirthDate = new DateTime(2005, 3, 15),
    ClassGroupId = 1
};
```

## 📊 **Field Descriptions**

### **Required Fields**
- `username`: Tên đăng nhập (unique)
- `email`: Email (unique)
- `firstName`: Tên
- `lastName`: Họ
- `role`: 1 = Student, 2 = Teacher, 3 = Admin, 4 = Other
- `enrollmentYear`: Năm nhập học

### **Optional Fields**
- `phoneNumber`: Số điện thoại (format: +84xxxxxxxxx)
- `studentNumber`: Mã số sinh viên (format: SV + năm + số)
- `bio`: Tiểu sử, mô tả về sinh viên
- `extraJson`: Thông tin bổ sung dạng JSON string
- `avatarUrl`: URL ảnh đại diện
- `birthDate`: Ngày sinh (format: YYYY-MM-DD)
- `classGroupId`: ID lớp học (phải tồn tại trong hệ thống)

## 🎨 **Sample Scenarios**

### **1. Student Mới (Năm 1)**
```json
{
  "username": "student001",
  "email": "student001@example.com",
  "firstName": "Nguyễn Văn",
  "lastName": "An",
  "phoneNumber": "+84901234567",
  "role": 1,
  "studentNumber": "SV2024001",
  "enrollmentYear": 2024,
  "bio": "Sinh viên năm 1 chuyên ngành Công nghệ thông tin",
  "extraJson": "{\"hobbies\":[\"Lập trình\",\"Đọc sách\"],\"skills\":[\"C#\",\"JavaScript\"]}",
  "avatarUrl": "https://example.com/avatars/student001.jpg",
  "birthDate": "2005-03-15",
  "classGroupId": 1
}
```

### **2. Student Có Kinh Nghiệm (Năm 3)**
```json
{
  "username": "student005",
  "email": "student005@example.com",
  "firstName": "Hoàng Văn",
  "lastName": "Em",
  "phoneNumber": "+84901234571",
  "role": 1,
  "studentNumber": "SV2022005",
  "enrollmentYear": 2022,
  "bio": "Sinh viên năm 3 chuyên ngành Kinh tế, có kinh nghiệm khởi nghiệp",
  "extraJson": "{\"hobbies\":[\"Khởi nghiệp\",\"Hoạt động xã hội\"],\"skills\":[\"Excel\",\"Power BI\",\"Marketing\"],\"startup\":\"Ứng dụng kết nối sinh viên\"}",
  "avatarUrl": "https://example.com/avatars/student005.jpg",
  "birthDate": "2003-06-10",
  "classGroupId": 4
}
```

### **3. Student Sắp Tốt Nghiệp (Năm 4)**
```json
{
  "username": "student003",
  "email": "student003@example.com",
  "firstName": "Lê Văn",
  "lastName": "Cường",
  "phoneNumber": "+84901234569",
  "role": 1,
  "studentNumber": "SV2021003",
  "enrollmentYear": 2021,
  "bio": "Sinh viên năm cuối chuyên ngành Kỹ thuật phần mềm, có kinh nghiệm thực tập",
  "extraJson": "{\"hobbies\":[\"Phát triển phần mềm\",\"Nghiên cứu AI\"],\"skills\":[\"Java\",\"Spring Boot\",\"React\",\"Docker\"],\"experience\":[\"Thực tập tại FPT Software\"],\"thesis\":\"Ứng dụng AI trong giáo dục\"}",
  "avatarUrl": "https://example.com/avatars/student003.jpg",
  "birthDate": "2002-11-08",
  "classGroupId": 3
}
```

## 🔧 **Customization Tips**

### **1. Thay đổi thông tin cơ bản**
- `username`: Đảm bảo unique trong hệ thống
- `email`: Đảm bảo unique và đúng format
- `studentNumber`: Format SV + năm + số thứ tự

### **2. Tùy chỉnh extraJson**
```json
{
  "hobbies": ["Lập trình", "Đọc sách", "Chơi game"],
  "skills": ["C#", "JavaScript", "Python"],
  "interests": ["Web Development", "Mobile App"],
  "achievements": ["Giải nhất cuộc thi", "Học bổng xuất sắc"],
  "projects": ["Website bán hàng", "Ứng dụng mobile"],
  "experience": ["Thực tập tại công ty X", "Freelance project Y"]
}
```

### **3. ClassGroupId Mapping**
- `1`: IT01 - Công nghệ thông tin năm 1
- `2`: MATH01 - Toán học năm 2
- `3`: SE01 - Kỹ thuật phần mềm năm 4
- `4`: ECON01 - Kinh tế năm 3

## ⚠️ **Lưu ý quan trọng**

### **Validation Rules**
- `username`: 3-50 ký tự, chỉ chữ cái, số, dấu gạch dưới
- `email`: Format email hợp lệ
- `phoneNumber`: Format +84xxxxxxxxx
- `birthDate`: Format YYYY-MM-DD
- `classGroupId`: Phải tồn tại trong hệ thống

### **Role Values**
- `1`: Student
- `2`: Teacher
- `3`: Admin
- `4`: Other

### **EnrollmentYear**
- Năm nhập học (ví dụ: 2024, 2023, 2022, 2021)
- Thường là năm hiện tại hoặc các năm trước

## 🎉 **Kết luận**

Bộ dữ liệu mẫu này cung cấp:
- ✅ **5 student examples** với các tình huống khác nhau
- ✅ **Field descriptions** chi tiết
- ✅ **Usage examples** cho Postman và code
- ✅ **Customization tips** để tùy chỉnh
- ✅ **Validation rules** để tránh lỗi

Bây giờ bạn có thể dễ dàng tạo student users với dữ liệu mẫu phong phú! 🚀


