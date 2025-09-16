# 🚀 EduShpere Backend Setup Guide

## 📋 Prerequisites

Trước khi bắt đầu, đảm bảo bạn đã cài đặt:

- **.NET 8.0 SDK** hoặc mới hơn
- **SQL Server** (LocalDB, Express, hoặc Full)
- **Visual Studio 2022** hoặc **VS Code** với C# extension
- **Git** để clone repository
- **Postman** hoặc **Insomnia** để test API

## 🔧 Installation Steps

### 1. Clone Repository

```bash
git clone <repository-url>
cd ESP_BE
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Database Setup

#### Option A: Using Entity Framework Migrations

```bash
# Tạo migration mới (nếu cần)
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project EduShpere

# Cập nhật database
dotnet ef database update --project Infrastructure --startup-project EduShpere
```

#### Option B: Using SQL Scripts

1. Mở SQL Server Management Studio
2. Tạo database mới tên `EduShpereDB`
3. Chạy script trong `ESP_DB/migrations/001_init_database.sql`

### 4. Configuration

#### Update Connection String

Mở file `EduShpere/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnectionString": "Server=(localdb)\\mssqllocaldb;Database=EduShpereDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

#### Update App Settings

```json
{
  "AppSetting": {
    "SecretKey": "YourSuperSecretKeyHereAtLeast32Characters",
    "FrontEndUrl": "http://localhost:3000"
  },
  "Gmail": {
    "Email": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "GoogleOAuth": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  }
}
```

### 5. Run Application

```bash
dotnet run --project EduShpere
```

Application sẽ chạy tại: `https://localhost:7000` (HTTPS) hoặc `http://localhost:5000` (HTTP)

## 🧪 Testing Setup

### 1. Swagger UI

Mở browser và truy cập: `https://localhost:7000/swagger`

### 2. Postman Collection

Import file `EduShpere.postman_collection.json` vào Postman.

### 3. Test Data

Sử dụng các file test trong thư mục `test_*.json`:

```bash
# Test login
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d @test_login.json

# Test create user
curl -X POST "https://localhost:7000/api/auth/create-user" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <admin-token>" \
  -d @test_create_user.json
```

## 🔐 Initial Admin Setup

### 1. Tạo Admin User

Chạy script SQL để tạo admin user:

```sql
INSERT INTO Users (Username, Email, FirstName, LastName, Role, Password, CreatedAt, IsDeleted)
VALUES ('admin', 'admin@edushpere.com', 'Admin', 'User', 0, 'hashed-password', GETUTCDATE(), 0);
```

### 2. Hoặc sử dụng API

```bash
# Tạo admin user qua API (cần token admin)
curl -X POST "https://localhost:7000/api/auth/create-user" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <admin-token>" \
  -d '{
    "username": "admin",
    "email": "admin@edushpere.com",
    "firstName": "Admin",
    "lastName": "User",
    "role": 0
  }'
```

## 🛠️ Development Workflow

### 1. Tạo Feature mới

```bash
# Tạo branch mới
git checkout -b feature/new-feature

# Tạo migration (nếu có thay đổi database)
dotnet ef migrations add AddNewFeature --project Infrastructure --startup-project EduShpere

# Cập nhật database
dotnet ef database update --project Infrastructure --startup-project EduShpere
```

### 2. Code Changes

1. Tạo Entity mới kế thừa từ `BaseEntity`
2. Tạo DTOs với validation
3. Tạo Repository interface và implementation
4. Tạo Service với `IAuditService`
5. Tạo Controller với authorization
6. Tạo AutoMapper profile
7. Viết unit tests

### 3. Testing

```bash
# Chạy unit tests
dotnet test

# Chạy với coverage
dotnet test --collect:"XPlat Code Coverage"
```

### 4. Build và Deploy

```bash
# Build release
dotnet build --configuration Release

# Publish
dotnet publish --configuration Release --output ./publish
```

## 🐛 Troubleshooting

### Common Issues

#### 1. Database Connection Error

```
A network-related or instance-specific error occurred while establishing a connection to SQL Server
```

**Solution:**
- Kiểm tra SQL Server đang chạy
- Kiểm tra connection string
- Kiểm tra firewall settings

#### 2. Migration Error

```
The migration 'xxx' has already been applied to the database
```

**Solution:**
```bash
# Xem migration history
dotnet ef migrations list --project Infrastructure --startup-project EduShpere

# Rollback migration
dotnet ef database update PreviousMigrationName --project Infrastructure --startup-project EduShpere
```

#### 3. JWT Token Error

```
The token is invalid or expired
```

**Solution:**
- Kiểm tra `SecretKey` trong appsettings.json
- Đảm bảo token chưa hết hạn
- Kiểm tra format Authorization header

#### 4. CORS Error

```
Access to fetch at 'https://localhost:7000/api/...' from origin 'http://localhost:3000' has been blocked by CORS policy
```

**Solution:**
- Kiểm tra CORS configuration trong Program.cs
- Đảm bảo frontend URL được thêm vào allowed origins

### Debug Mode

```bash
# Chạy với debug logging
dotnet run --project EduShpere --environment Development --verbosity detailed
```

## 📁 Project Structure

```
ESP_BE/
├── Application/                 # Business Logic
│   ├── DTOs/                   # Data Transfer Objects
│   ├── Mappings/               # AutoMapper Profiles
│   └── Services/               # Business Services
├── Domain/                     # Domain Models
│   ├── Models/                 # Entity Models
│   └── Enum/                   # Enums
├── Infrastructure/             # Data Access
│   ├── DbContext/              # EF Context
│   ├── Repositories/           # Data Repositories
│   └── Services/               # Infrastructure Services
├── Shared/                     # Shared Components
│   ├── Constants/              # Constants
│   └── Exceptions/             # Custom Exceptions
├── EduShpere/                 # API Layer
│   ├── Controllers/            # API Controllers
│   ├── Middlewares/            # Custom Middlewares
│   └── Program.cs              # Entry Point
├── DEVELOPMENT_GUIDE.md        # Development Guidelines
├── API_REFERENCE.md            # API Documentation
└── SETUP_GUIDE.md              # This file
```

## 🔧 Environment Variables

### Development

```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:7000;http://localhost:5000
```

### Production

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://api.edushpere.com
```

## 📊 Monitoring

### Health Checks

```bash
# Health check endpoint
curl https://localhost:7000/health
```

### Logging

Logs được ghi vào:
- Console (Development)
- File (Production)
- Application Insights (Azure)

## 🚀 Deployment

### Docker

```bash
# Build Docker image
docker build -t edushpere-backend .

# Run container
docker run -p 5000:80 edushpere-backend
```

### Azure

```bash
# Deploy to Azure App Service
az webapp deployment source config-zip --resource-group myResourceGroup --name myAppName --src myapp.zip
```

## 📞 Support

Nếu gặp vấn đề:

1. Kiểm tra [Troubleshooting](#-troubleshooting) section
2. Xem logs trong console hoặc file
3. Kiểm tra [API Reference](API_REFERENCE.md)
4. Liên hệ team qua Slack: #edushpere-backend

---

**Happy Coding! 🎉**
