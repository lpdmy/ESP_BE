# 🎓 EduShpere Backend

Backend API cho hệ thống quản lý hoạt động giáo dục EduShpere.

## 📚 Documentation

- **[Setup Guide](SETUP_GUIDE.md)** - Hướng dẫn cài đặt và chạy project
- **[Development Guide](DEVELOPMENT_GUIDE.md)** - Hướng dẫn phát triển và coding standards
- **[API Reference](API_REFERENCE.md)** - Tài liệu API endpoints chi tiết

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 hoặc VS Code

### Installation

```bash
# Clone repository
git clone <repository-url>
cd ESP_BE

# Restore dependencies
dotnet restore

# Setup database
dotnet ef database update --project Infrastructure --startup-project EduShpere

# Run application
dotnet run --project EduShpere
```

Application sẽ chạy tại: `https://localhost:7000`

## 🏗️ Architecture

### Clean Architecture Pattern

```
┌─────────────────┐
│   Controllers   │ ← Presentation Layer
├─────────────────┤
│    Services     │ ← Business Logic Layer
├─────────────────┤
│   Repositories  │ ← Data Access Layer
├─────────────────┤
│   Database      │ ← Infrastructure
└─────────────────┘
```

### Key Features

- ✅ **JWT Authentication** với role-based authorization
- ✅ **Auto Audit Fields** (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
- ✅ **Custom Validation** với error handling
- ✅ **Soft Delete** pattern
- ✅ **AutoMapper** cho object mapping
- ✅ **Entity Framework Core** với migrations
- ✅ **Swagger UI** cho API documentation

## 🔐 Authentication & Authorization

### Roles

| Role | Value | Permissions |
|------|-------|-------------|
| Admin | 0 | Full system access |
| Teacher | 2 | Activity management |
| Student | 4 | Basic user access |

### API Security

- JWT Bearer token authentication
- Role-based endpoint protection
- Input validation với custom error messages
- CORS configuration cho frontend integration

## 📊 API Endpoints

### Authentication
- `POST /api/auth/login` - Đăng nhập
- `GET /api/auth/GetMe` - Lấy thông tin user hiện tại
- `POST /api/auth/create-user` - Tạo user (Admin only)
- `POST /api/auth/change-password` - Đổi mật khẩu

### User Management
- `GET /api/users` - Lấy danh sách users (Admin only)
- `PUT /api/users` - Cập nhật user (Admin only)

### Profile Management
- `GET /api/user-profile/my-profile` - Lấy profile cá nhân
- `PUT /api/user-profile/my-profile` - Cập nhật profile cá nhân
- `GET /api/user-profile/all-profiles` - Lấy tất cả profiles (Admin only)

### Activity Management
- `GET /api/activities` - Lấy danh sách hoạt động
- `POST /api/activities` - Tạo hoạt động (Teacher/Admin)
- `PUT /api/activities` - Cập nhật hoạt động (Teacher/Admin)

### File Upload
- `POST /api/upload` - Upload file/ảnh

## 🛠️ Development

### Code Standards

- Sử dụng **BaseEntity** cho tất cả models
- **AuditService** cho auto audit fields
- **CustomModelValidationFilter** cho validation
- **ResponseDto<T>** cho API responses
- **Role-based authorization** cho endpoints

### Project Structure

```
ESP_BE/
├── Application/          # Business Logic
│   ├── DTOs/            # Data Transfer Objects
│   ├── Services/        # Business Services
│   └── Mappings/        # AutoMapper Profiles
├── Domain/              # Domain Models
│   ├── Models/          # Entity Models
│   └── Enum/            # Enums
├── Infrastructure/      # Data Access
│   ├── DbContext/       # EF Context
│   └── Repositories/    # Data Repositories
├── Shared/              # Shared Components
│   ├── Constants/       # Constants
│   └── Exceptions/      # Custom Exceptions
└── EduShpere/          # API Layer
    ├── Controllers/     # API Controllers
    └── Middlewares/     # Custom Middlewares
```

## 🧪 Testing

### Unit Tests

```bash
dotnet test
```

### API Testing

- Swagger UI: `https://localhost:7000/swagger`
- Postman collection: `EduShpere.postman_collection.json`

## 🚀 Deployment

### Docker

```bash
docker build -t edushpere-backend .
docker run -p 5000:80 edushpere-backend
```

### Environment Variables

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnectionString="your-connection-string"
AppSetting__SecretKey="your-secret-key"
```

## 📈 Performance

### Database Optimization

- Entity Framework Core với query optimization
- Soft delete pattern để maintain data integrity
- Indexed columns cho performance

### Caching

- JWT token caching
- Database query result caching (future enhancement)

## 🔒 Security

### Data Protection

- Password hashing với secure algorithms
- JWT token với expiration
- Input validation và sanitization
- SQL injection prevention với EF Core

### Audit Trail

- Automatic audit fields tracking
- User action logging
- Data change history

## 📞 Support

- **Documentation**: Xem các file markdown trong project
- **Issues**: Tạo issue trên GitHub repository
- **Slack**: #edushpere-backend

## 🤝 Contributing

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Tạo Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Made with ❤️ by EduShpere Team**
