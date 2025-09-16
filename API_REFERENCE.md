# 🔌 EduShpere API Reference

## 📋 Tổng quan

Tài liệu này mô tả chi tiết các API endpoints của EduShpere Backend, bao gồm request/response format, authentication requirements và error handling.

## 🔐 Authentication

### JWT Token

Tất cả protected endpoints yêu cầu JWT token trong header:

```
Authorization: Bearer <your-jwt-token>
```

### Token Format

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token"
}
```

## 📊 Response Format

### Success Response

```json
{
  "data": <actual-data>,
  "message": "Operation successful",
  "statusCode": 200
}
```

### Error Response

```json
{
  "data": null,
  "message": "Error message",
  "statusCode": 400
}
```

### Validation Error Response

```json
{
  "data": null,
  "message": "Field validation error message",
  "statusCode": 400
}
```

## 👥 Authentication APIs

### POST /api/auth/login

Đăng nhập vào hệ thống.

**Request:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "data": {
    "accessToken": "string",
    "refreshToken": "string"
  },
  "message": "Đăng nhập thành công",
  "statusCode": 200
}
```

### GET /api/auth/GetMe

Lấy thông tin user hiện tại.

**Headers:** `Authorization: Bearer <token>`

**Response:**
```json
{
  "data": {
    "id": 1,
    "username": "string",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "role": "Student"
  },
  "message": null,
  "statusCode": 200
}
```

### POST /api/auth/create-user

Tạo user mới (Admin only).

**Headers:** `Authorization: Bearer <admin-token>`

**Request:**
```json
{
  "username": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "role": "Student",
  "phoneNumber": "string",
  "birthDate": "2024-01-01",
  "studentNumber": "string",
  "enrollmentYear": 2024,
  "bio": "string",
  "avatarUrl": "string"
}
```

**Response:**
```json
{
  "data": true,
  "message": "Tạo người dùng thành công",
  "statusCode": 200
}
```

### POST /api/auth/create-user-dev

Tạo user mới và trả về token (Development only - Admin only).

**Headers:** `Authorization: Bearer <admin-token>`

**Request:**
```json
{
  "username": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "role": "Student",
  "phoneNumber": "string",
  "birthDate": "2024-01-01",
  "studentNumber": "string",
  "enrollmentYear": 2024,
  "bio": "string",
  "avatarUrl": "string"
}
```

**Response:**
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-encoded-refresh-token"
  },
  "message": "Tạo người dùng thành công - Development Mode",
  "statusCode": 200
}
```

**Note:** Endpoint này chỉ dành cho development để test nhanh. Trong production, sử dụng `/api/auth/create-user` để gửi email OTL.

### GET /api/users

Lấy danh sách users với pagination (Admin only).

**Headers:** `Authorization: Bearer <admin-token>`

**Query Parameters:**
- `pageNumber` (int): Số trang (default: 1, min: 1)
- `pageSize` (int): Số items per page (default: 10, min: 1, max: 100)
- `search` (string, optional): Tìm kiếm trong username, email, firstName, lastName
- `sortBy` (string, optional): Sắp xếp theo trường (firstName, lastName, email, createdAt, etc.)
- `sortDescending` (bool): Sắp xếp giảm dần (default: false)

**Request Examples:**
```bash
# Basic pagination
GET /api/users?pageNumber=1&pageSize=10

# With search
GET /api/users?pageNumber=1&pageSize=10&search=john

# With sorting
GET /api/users?pageNumber=1&pageSize=10&sortBy=firstName&sortDescending=true

# Combined
GET /api/users?pageNumber=2&pageSize=20&search=admin&sortBy=createdAt&sortDescending=true
```

**Response:**
```json
{
  "data": {
    "data": [
      {
        "id": 1,
        "username": "john_doe",
        "email": "john@example.com",
        "firstName": "John",
        "lastName": "Doe",
        "role": "Student",
        "phoneNumber": "0123456789",
        "birthdate": "2000-01-01T00:00:00Z",
        "avatarUrl": "https://example.com/avatar.jpg",
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "totalCount": 150,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 15,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "message": "Lấy danh sách người dùng thành công",
  "statusCode": 200
}
```

### PUT /api/users

Cập nhật thông tin user (Admin only).

**Headers:** `Authorization: Bearer <admin-token>`

**Request:**
```json
{
  "id": 1,
  "username": "string",
  "email": "string",
  "firstName": "string",
  "lastName": "string",
  "phoneNumber": "string",
  "birthdate": "2024-01-01",
  "address": "string",
  "avatarUrl": "string",
  "role": "Student"
}
```

### POST /api/auth/change-password

Đổi mật khẩu (Authenticated users).

**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "oldPassword": "string",
  "newPassword": "string",
  "confirmPassword": "string"
}
```

### POST /api/auth/forgot-password

Quên mật khẩu.

**Request:**
```json
{
  "email": "string"
}
```

## 👤 User Profile APIs

### GET /api/user-profile/my-profile

Lấy profile của user hiện tại.

**Headers:** `Authorization: Bearer <token>`

**Response:**
```json
{
  "data": {
    "id": 1,
    "userId": 1,
    "studentNumber": "string",
    "enrollmentYear": 2024,
    "bio": "string",
    "extraJson": "string",
    "avatarUrl": "string",
    "birthDate": "2024-01-01",
    "phoneNumber": "string",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  },
  "message": "Lấy thông tin profile thành công",
  "statusCode": 200
}
```

### PUT /api/user-profile/my-profile

Cập nhật thông tin cá nhân.

**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "phoneNumber": "string",
  "birthDate": "2024-01-01",
  "avatarUrl": "string",
  "bio": "string",
  "extraJson": "string"
}
```

### GET /api/user-profile/all-profiles

Lấy tất cả student profiles (Admin only).

**Headers:** `Authorization: Bearer <admin-token>`

### GET /api/user-profile/{id}

Lấy profile theo ID (Admin only).

**Headers:** `Authorization: Bearer <admin-token>`

### POST /api/user-profile/create-profile

Tạo profile mới (Admin only).

**Headers:** `Authorization: Bearer <admin-token>`

**Request:**
```json
{
  "userId": 1,
  "studentNumber": "string",
  "enrollmentYear": 2024,
  "bio": "string",
  "extraJson": "string",
  "avatarUrl": "string",
  "birthDate": "2024-01-01",
  "phoneNumber": "string"
}
```

### PUT /api/user-profile/update-profile

Cập nhật thông tin sinh viên (Admin only).

**Headers:** `Authorization: Bearer <admin-token>`

**Request:**
```json
{
  "id": 1,
  "userId": 1,
  "studentNumber": "string",
  "enrollmentYear": 2024,
  "bio": "string",
  "extraJson": "string",
  "avatarUrl": "string",
  "birthDate": "2024-01-01",
  "phoneNumber": "string"
}
```

### DELETE /api/user-profile/delete-profile/{id}

Xóa profile (Admin only).

**Headers:** `Authorization: Bearer <admin-token>`

## 🎯 Activity APIs

### GET /api/activities

Lấy danh sách hoạt động.

**Headers:** `Authorization: Bearer <token>`

**Query Parameters:**
- `pageNumber` (int): Số trang
- `pageSize` (int): Số items per page
- `search` (string, optional): Tìm kiếm

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "title": "string",
      "description": "string",
      "startDate": "2024-01-01T00:00:00Z",
      "endDate": "2024-01-02T00:00:00Z",
      "location": "string",
      "maxParticipants": 100,
      "currentParticipants": 50,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "message": "Lấy danh sách thành công",
  "statusCode": 200
}
```

### GET /api/activities/{id}

Lấy chi tiết hoạt động.

**Headers:** `Authorization: Bearer <token>`

### POST /api/activities

Tạo hoạt động mới (Teacher/Admin only).

**Headers:** `Authorization: Bearer <teacher-or-admin-token>`

**Request:**
```json
{
  "title": "string",
  "description": "string",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-02T00:00:00Z",
  "location": "string",
  "maxParticipants": 100
}
```

### PUT /api/activities

Cập nhật hoạt động (Teacher/Admin only).

**Headers:** `Authorization: Bearer <teacher-or-admin-token>`

## 👥 Activity Participant APIs

### POST /api/activity-participants

Tham gia hoạt động.

**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "activityId": 1,
  "userId": 1
}
```

### DELETE /api/activity-participants/{participationId}

Rời khỏi hoạt động (Teacher/Admin only).

**Headers:** `Authorization: Bearer <teacher-or-admin-token>`

## 📁 Upload APIs

### POST /api/upload

Upload file/ảnh.

**Headers:** `Authorization: Bearer <token>`

**Request:** `multipart/form-data`
- `file`: File to upload

**Response:**
```json
{
  "data": "https://cloudinary-url.com/image.jpg",
  "message": null,
  "statusCode": 200
}
```

## 🚨 Error Codes

| Status Code | Description |
|-------------|-------------|
| 200 | Success |
| 400 | Bad Request - Validation error or invalid data |
| 401 | Unauthorized - Invalid or missing token |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource not found |
| 500 | Internal Server Error - Server error |

## 📄 Pagination System

### Tổng quan

Hệ thống pagination được áp dụng cho tất cả endpoints trả về danh sách dữ liệu lớn để tối ưu performance và trải nghiệm người dùng.

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `pageNumber` | int | No | 1 | Số trang (min: 1) |
| `pageSize` | int | No | 10 | Số items per page (min: 1, max: 100) |
| `search` | string | No | - | Tìm kiếm trong các trường string |
| `sortBy` | string | No | - | Sắp xếp theo trường |
| `sortDescending` | bool | No | false | Sắp xếp giảm dần |

### Response Format

```json
{
  "data": {
    "data": [...],           // Array of items
    "totalCount": 150,       // Total number of items
    "pageNumber": 1,         // Current page number
    "pageSize": 10,          // Items per page
    "totalPages": 15,        // Total number of pages
    "hasPreviousPage": false, // Has previous page
    "hasNextPage": true      // Has next page
  },
  "message": "Success message",
  "statusCode": 200
}
```

### Usage Examples

#### Basic Pagination
```bash
GET /api/users?pageNumber=1&pageSize=10
```

#### With Search
```bash
GET /api/users?pageNumber=1&pageSize=10&search=john
```

#### With Sorting
```bash
GET /api/users?pageNumber=1&pageSize=10&sortBy=firstName&sortDescending=true
```

#### Combined
```bash
GET /api/users?pageNumber=2&pageSize=20&search=admin&sortBy=createdAt&sortDescending=true
```

### Supported Endpoints

- `GET /api/users` - User management
- `GET /api/activities` - Activity management (coming soon)
- `GET /api/user-profile/all-profiles` - Profile management (coming soon)

## 📝 Validation Rules

### Common Validation

- **Email**: Must be valid email format
- **Phone**: Must be valid phone number format
- **URL**: Must be valid URL format
- **String Length**: Check individual field limits
- **Required Fields**: Cannot be null or empty

### Pagination Validation

- **pageNumber**: Must be >= 1
- **pageSize**: Must be between 1 and 100
- **search**: Optional string search
- **sortBy**: Must be a valid entity property name
- **sortDescending**: Boolean value

### Password Requirements

- Minimum 6 characters
- Maximum 20 characters
- At least 1 uppercase letter
- At least 1 special character (!@#$%^&*)
- No spaces or Vietnamese characters

### Role-Based Access

- **Admin**: Full access to all endpoints
- **Teacher**: Can manage activities and participants
- **Student**: Can view activities and manage own profile

## 🔧 Testing

### Postman Collection

Import the provided Postman collection for easy API testing.

### cURL Examples

```bash
# Login
curl -X POST "https://api.edushpere.com/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"test@example.com","password":"password123"}'

# Get Profile
curl -X GET "https://api.edushpere.com/api/user-profile/my-profile" \
  -H "Authorization: Bearer <your-token>"

# Create Activity
curl -X POST "https://api.edushpere.com/api/activities" \
  -H "Authorization: Bearer <teacher-token>" \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Activity","description":"Test Description"}'
```

## 📞 Support

For API support and questions:

- **Documentation**: [Link to full docs]
- **Support Email**: support@edushpere.com
- **Status Page**: [Link to status page]

---

**Note**: This API reference is automatically generated and updated with each deployment.
