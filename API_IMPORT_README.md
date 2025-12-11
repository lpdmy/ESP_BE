# 📚 **EduShpere API Import Guide**

## 🎯 **Tổng quan**
Bộ sưu tập đầy đủ các file để import và test EduShpere API, bao gồm:
- **Markdown documentation** với mô tả chi tiết tất cả endpoints
- **JSON examples** với request/response mẫu
- **Postman collection** để import vào Postman
- **cURL commands** để test từ command line

## 📁 **Danh sách files**

### 1. **`api_endpoints_import.md`**
- **Mô tả**: Tài liệu đầy đủ về tất cả API endpoints
- **Nội dung**: 
  - Danh sách tất cả endpoints với method, URL, authorization
  - Mô tả chi tiết từng endpoint
  - DTOs và data structures
  - Authorization roles và permissions

### 2. **`api_endpoints_examples.json`**
- **Mô tả**: JSON examples cho tất cả endpoints
- **Nội dung**:
  - Request/Response mẫu cho từng endpoint
  - Data examples với các trường hợp khác nhau
  - Error response examples
  - Common response formats

### 3. **`EduShpere_API_Collection.postman_collection.json`**
- **Mô tả**: Postman collection để import vào Postman
- **Nội dung**:
  - Tất cả endpoints được tổ chức theo nhóm
  - Pre-configured headers và authentication
  - Environment variables (base_url, jwt_token)
  - Sample request bodies

### 4. **`api_curl_commands.sh`**
- **Mô tả**: Shell script với cURL commands để test API
- **Nội dung**:
  - Tất cả endpoints dưới dạng cURL commands
  - Colorized output cho dễ đọc
  - JWT token extraction và usage
  - Error handling examples

## 🚀 **Cách sử dụng**

### **Option 1: Sử dụng Postman (Khuyến nghị)**
1. Mở Postman
2. Click **Import** → **Upload Files**
3. Chọn file `EduShpere_API_Collection.postman_collection.json`
4. Set environment variables:
   - `base_url`: `https://localhost:7000` (hoặc URL server của bạn)
   - `jwt_token`: Token từ login response
5. Chạy các requests theo thứ tự

### **Option 2: Sử dụng cURL**
1. Mở terminal/command prompt
2. Chạy script:
   ```bash
   chmod +x api_curl_commands.sh
   ./api_curl_commands.sh
   ```
3. Hoặc copy từng command từ script

### **Option 3: Sử dụng tài liệu Markdown**
1. Mở `api_endpoints_import.md`
2. Copy request examples
3. Sử dụng với tool yêu thích (Postman, Insomnia, etc.)

## 🔧 **Cấu hình**

### **Environment Variables**
- `base_url`: URL của API server (mặc định: `https://localhost:7000`)
- `jwt_token`: JWT token từ login response

### **Authentication**
1. **Login** để lấy JWT token
2. **Set token** vào header: `Authorization: Bearer {token}`
3. **Use token** cho các protected endpoints

## 📋 **Endpoints Categories**

### **🔐 Authentication (12 endpoints)**
- Login, Get Me, Create User, Change Password, etc.

### **👤 Student Profile (8 endpoints)**
- Get/Update My Profile, CRUD operations (Admin)

### **👨‍🏫 Teacher Profile (8 endpoints)**
- Get/Update My Profile, CRUD operations (Admin)

### **🎯 Activities (4 endpoints)**
- Get All, Get by ID, Create, Update

### **👥 Activity Participants (2 endpoints)**
- Add Participant, Remove Participant

### **📁 File Upload (1 endpoint)**
- Upload file to Cloudinary

## 🔑 **Authorization Roles**

### **Student**
- Có thể truy cập: My Profile, Activities, Activity Participants
- Không thể: Admin functions, Create/Update Activities

### **Teacher**
- Có thể truy cập: My Profile, My Teacher Profile, Activities, Activity Participants
- Có thể: Create/Update Activities
- Không thể: Admin functions

### **Admin**
- Có thể truy cập: Tất cả endpoints
- Có thể: CRUD operations cho Users, Profiles, Activities

## 📝 **Request/Response Format**

### **Standard Response Format**
```json
{
  "data": "Response data or null",
  "message": "Success or error message",
  "statusCode": 200
}
```

### **Pagination Response**
```json
{
  "data": [...],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

### **Error Response**
```json
{
  "data": null,
  "message": "Error message",
  "statusCode": 400
}
```

## 🧪 **Testing Workflow**

### **1. Basic Flow**
1. **Login** → Get JWT token
2. **Get Me** → Verify authentication
3. **Get My Profile** → Check profile data
4. **Update Profile** → Test update functionality

### **2. Admin Flow**
1. **Login as Admin** → Get admin token
2. **Get All Users** → Check user management
3. **Create User** → Test user creation
4. **Get All Profiles** → Check profile management

### **3. Activity Flow**
1. **Login** → Get token
2. **Get All Activities** → Check activities
3. **Create Activity** → Test activity creation
4. **Add Participant** → Test participation

## ⚠️ **Lưu ý quan trọng**

### **Security**
- JWT token có thời hạn, cần refresh khi hết hạn
- Một số endpoints chỉ dành cho Admin
- File upload cần validation

### **Data Validation**
- Tất cả input đều được validate
- Required fields phải có giá trị
- String length limits được áp dụng

### **Error Handling**
- 400: Bad Request (validation errors)
- 401: Unauthorized (missing/invalid token)
- 403: Forbidden (insufficient permissions)
- 404: Not Found (resource not found)
- 500: Internal Server Error

## 🔄 **Updates**

### **Version 1.0.0**
- Initial release với tất cả endpoints
- Postman collection hoàn chỉnh
- cURL commands với error handling
- Comprehensive documentation

## 📞 **Support**

Nếu có vấn đề hoặc cần hỗ trợ:
1. Kiểm tra logs của API server
2. Verify JWT token validity
3. Check network connectivity
4. Review request/response format

---

**Happy Testing! 🎉**

