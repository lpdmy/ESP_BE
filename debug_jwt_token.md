# 🔍 JWT Token Debug Guide

## Vấn đề đã fix

**Nguyên nhân 403 Forbidden:**
- JWT token chỉ có `UserRole` claim với giá trị số (0, 2, 4)
- Authorization attributes cần `ClaimTypes.Role` với giá trị string ("Admin", "Teacher", "Student")

## Giải pháp

Đã thêm `ClaimTypes.Role` claim vào JWT token generation:

```csharp
// Convert role number to role name
var roleName = appUser.Role switch
{
    UserRole.Admin => "Admin",
    UserRole.Teacher => "Teacher", 
    UserRole.Student => "Student",
    _ => "Student"
};

// Add both claims
new Claim("UserRole", appUser.Role.ToString()),        // Số: 0, 2, 4
new Claim(ClaimTypes.Role, roleName)                   // String: "Admin", "Teacher", "Student"
```

## Test JWT Token

### 1. Tạo user và lấy token

```bash
curl -X POST "https://localhost:7000/api/auth/create-user-dev" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <admin-token>" \
  -d @test_create_user_dev.json
```

### 2. Decode JWT Token

Sử dụng [jwt.io](https://jwt.io) để decode token và kiểm tra claims:

**Expected Claims:**
```json
{
  "Email": "user@example.com",
  "FullName": "Admin",
  "Id": "123",
  "UserName": "admin001",
  "UserRole": "0",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Admin"
}
```

### 3. Test Authorization

```bash
# Test với token mới
curl -X GET "https://localhost:7000/api/users" \
  -H "Authorization: Bearer <new-token-from-create-user-dev>"
```

## Debug Steps

1. **Tạo user mới** với `/api/auth/create-user-dev`
2. **Copy token** từ response
3. **Decode token** tại jwt.io để xem claims
4. **Test endpoint** với token mới
5. **Kiểm tra logs** nếu vẫn có lỗi

## Common Issues

- **Token expired**: Kiểm tra `Expires` claim
- **Wrong role**: Kiểm tra `role` claim có đúng "Admin" không
- **Missing claims**: Kiểm tra có đủ claims không
- **Secret key**: Đảm bảo secret key giống nhau giữa tạo và verify token
