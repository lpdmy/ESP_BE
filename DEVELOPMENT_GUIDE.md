# 📚 EduShpere Backend Development Guide

## 🎯 Tổng quan

Tài liệu này hướng dẫn cách phát triển backend cho dự án EduShpere, bao gồm các quy tắc, patterns và best practices đã được thiết lập.

## 📁 Cấu trúc Project

```
ESP_BE/
├── Application/           # Business Logic Layer
│   ├── DTOs/             # Data Transfer Objects
│   ├── Mappings/         # AutoMapper Profiles
│   └── Services/         # Business Services
├── Domain/               # Domain Layer
│   ├── Models/           # Entity Models
│   └── Enum/             # Enums
├── Infrastructure/       # Data Access Layer
│   ├── DbContext/        # Entity Framework Context
│   ├── Repositories/     # Data Access Repositories
│   └── Services/         # Infrastructure Services
├── Shared/               # Shared Components
│   ├── Constants/        # Constants & Error Messages
│   └── Exceptions/       # Custom Exceptions
└── EduShpere/           # Presentation Layer
    ├── Controllers/      # API Controllers
    ├── Middlewares/      # Custom Middlewares
    └── Program.cs        # Application Entry Point
```

## 🔐 Authentication & Authorization

### Role-Based Authorization

Hệ thống sử dụng 4 roles chính:

```csharp
public enum UserRole : byte
{
    Admin = 0,      // Quyền cao nhất - quản lý toàn bộ hệ thống
    Staff = 1,      // Quyền quản lý với permissions cụ thể (quản lý nhân sự, hoạt động, v.v.)
    Teacher = 2,    // Quyền quản lý hoạt động
    Student = 4     // Quyền cơ bản - tham gia hoạt động
}
```

### Cách sử dụng Authorization

```csharp
[Authorize(Roles = "Admin")]                           // Chỉ Admin
[Authorize(Roles = "Staff,Admin")]                     // Staff và Admin
[Authorize(Roles = "Teacher,Admin")]                   // Teacher và Admin
[Authorize(Roles = "Staff,Teacher,Admin")]             // Staff, Teacher và Admin
[Authorize(Roles = "Student,Teacher,Admin,Staff")]      // Tất cả authenticated users
[Authorize]                                             // Bất kỳ user nào đã đăng nhập
```

**Lưu ý về Staff role:**
- Staff có quyền hạn dựa trên **permissions** được gán, không phải full quyền như Admin
- Staff chỉ có thể truy cập các endpoints mà họ có permission tương ứng
- Sử dụng `requiredPermissions` trong frontend để kiểm tra quyền cụ thể

### Quyền hạn theo Role

| Endpoint | Admin | Staff | Teacher | Student | Mô tả |
|----------|-------|-------|---------|---------|-------|
| User Management | ✅ | ⚠️* | ❌ | ❌ | Quản lý users, import, tạo user (*Staff cần permission MANAGE_USER) |
| Staff Management | ✅ | ⚠️* | ❌ | ❌ | Quản lý nhân viên (*Staff cần permission MANAGE_STAFF) |
| Profile Management | ✅ | ❌ | ❌ | ✅ | Quản lý profiles (Admin: tất cả, Student: của mình) |
| Activity Management | ✅ | ⚠️* | ✅ | ❌ | Tạo, cập nhật hoạt động (*Staff cần permission MANAGE_ACTIVITIES) |
| Activity Participation | ✅ | ✅ | ✅ | ✅ | Tham gia hoạt động |
| Club Management | ✅ | ⚠️* | ❌ | ❌ | Quản lý câu lạc bộ (*Staff cần permission MANAGE_CLUBS) |
| Class Management | ✅ | ⚠️* | ❌ | ❌ | Quản lý lớp học (*Staff cần permission MANAGE_CLASSES) |
| Rewards Management | ✅ | ⚠️* | ❌ | ❌ | Quản lý điểm thưởng (*Staff cần permission MANAGE_REWARDS) |
| Moderation | ✅ | ⚠️* | ❌ | ❌ | Kiểm duyệt nội dung (*Staff cần permission MODERATE_CONTENT) |
| File Upload | ✅ | ✅ | ✅ | ✅ | Upload files/ảnh |

**Ghi chú:** Staff role hoạt động dựa trên hệ thống permissions. Mỗi staff member có thể được gán các permissions cụ thể để truy cập các chức năng tương ứng.

## 🏗️ Entity Development

### BaseEntity Pattern

Tất cả entities phải kế thừa từ `BaseEntity`:

```csharp
public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
}
```

### Tạo Entity mới

```csharp
using EduShpere.Domain.Models;

public class MyEntity : BaseEntity
{
    [Key]
    public int Id { get; set; }
    
    [StringLength(255)]
    public string Name { get; set; }
    
    // Các properties khác...
    // KHÔNG cần thêm CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted
}
```

## 🔧 Service Development

### AuditService Integration

Tất cả services phải sử dụng `IAuditService` để tự động quản lý audit fields:

```csharp
public class MyService : IMyService
{
    private readonly IMyRepository _repository;
    private readonly IAuditService _auditService;
    
    public MyService(IMyRepository repository, IAuditService auditService)
    {
        _repository = repository;
        _auditService = auditService;
    }
    
    public async Task<MyEntity> CreateAsync(CreateMyEntityDto dto)
    {
        var entity = _mapper.Map<MyEntity>(dto);
        
        // Tự động set audit fields
        _auditService.SetAuditFieldsForCreate(entity);
        
        await _repository.AddAsync(entity);
        return entity;
    }
    
    public async Task<MyEntity> UpdateAsync(int id, UpdateMyEntityDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("Entity not found");
            
        _mapper.Map(dto, entity);
        
        // Tự động set audit fields
        _auditService.SetAuditFieldsForUpdate(entity);
        
        await _repository.UpdateAsync(entity);
        return entity;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return false;
            
        // Soft delete - set IsDeleted = true
        _auditService.SetAuditFieldsForDelete(entity);
        
        await _repository.UpdateAsync(entity);
        return true;
    }
}
```

### Service Registration

Đăng ký service trong `Program.cs`:

```csharp
builder.Services.AddScoped<IMyService, MyService>();
```

## 🎮 Controller Development

### Controller Structure

```csharp
using EduShpere.Middlewares;
using Microsoft.AspNetCore.Authorization;

namespace EduShpere.Controllers
{
    [CustomModelValidationFilter]
    public class MyController : BaseController
    {
        private readonly IMyService _service;
        
        public MyController(IMyService service)
        {
            _service = service;
        }
        
        [HttpGet]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetAll()
        {
            // Implementation
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateMyEntityDto dto)
        {
            // Implementation
        }
    }
}
```

### Response Format

Sử dụng `ResponseDto<T>` cho tất cả API responses:

```csharp
// Success Response
return Ok(new ResponseDto<MyEntityDto>(data, "Operation successful"));

// Error Response
return BadRequest(new ResponseDto<string>(null, "Error message", 400));
return NotFound(new ResponseDto<string>(null, "Not found", 404));
```

### Validation

Sử dụng `CustomModelValidationFilter` để xử lý validation:

```csharp
[CustomModelValidationFilter]
public class MyController : BaseController
{
    // Validation sẽ được xử lý tự động
    // Chỉ cần sử dụng Data Annotations trong DTOs
}
```

## 📝 DTO Development

### DTO Naming Convention

```csharp
// Create DTOs
public class CreateMyEntityDto
{
    [Required(ErrorMessage = ErrorMessages.Validation.NameRequired)]
    [StringLength(255, ErrorMessage = ErrorMessages.Validation.NameTooLong)]
    public string Name { get; set; }
}

// Update DTOs
public class UpdateMyEntityDto
{
    public int Id { get; set; }
    
    [StringLength(255, ErrorMessage = ErrorMessages.Validation.NameTooLong)]
    public string? Name { get; set; }
}

// Response DTOs
public class MyEntityDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    // ... other properties
}
```

### Validation Attributes

```csharp
[Required(ErrorMessage = ErrorMessages.Validation.FieldRequired)]
[StringLength(255, ErrorMessage = ErrorMessages.Validation.FieldTooLong)]
[EmailAddress(ErrorMessage = ErrorMessages.Validation.EmailInvalidFormat)]
[Url(ErrorMessage = ErrorMessages.Validation.UrlInvalidFormat)]
[Phone(ErrorMessage = ErrorMessages.Validation.PhoneInvalidFormat)]
[Range(1, 100, ErrorMessage = ErrorMessages.Validation.RangeInvalid)]
```

## 🗄️ Repository Development

### Repository Pattern

```csharp
public interface IMyRepository : IGenericRepository<MyEntity>
{
    Task<MyEntity?> GetByNameAsync(string name);
    Task<IEnumerable<MyEntity>> GetActiveAsync();
}

public class MyRepository : GenericRepository<MyEntity>, IMyRepository
{
    public MyRepository(EduShpereDbContext context) : base(context)
    {
    }
    
    public async Task<MyEntity?> GetByNameAsync(string name)
    {
        return await _context.MyEntities
            .FirstOrDefaultAsync(x => x.Name == name && !x.IsDeleted);
    }
    
    public async Task<IEnumerable<MyEntity>> GetActiveAsync()
    {
        return await _context.MyEntities
            .Where(x => !x.IsDeleted)
            .ToListAsync();
    }
}
```

## 🔄 AutoMapper Configuration

### Profile Creation

```csharp
public class MyEntityProfile : Profile
{
    public MyEntityProfile()
    {
        CreateMap<CreateMyEntityDto, MyEntity>();
        CreateMap<UpdateMyEntityDto, MyEntity>();
        CreateMap<MyEntity, MyEntityDto>();
    }
}
```

### Registration

```csharp
builder.Services.AddAutoMapper(typeof(MyEntityProfile).Assembly);
```

## 🚨 Error Handling

### Custom Exceptions

```csharp
// Sử dụng các exception có sẵn
throw new BadRequestException(ErrorMessages.MyEntity.InvalidData);
throw new NotFoundException(ErrorMessages.MyEntity.NotFound);
throw new UnauthorizedException(ErrorMessages.Auth.Unauthorized);
throw new ForbiddenException(ErrorMessages.Auth.Forbidden);
```

### Error Messages

Thêm error messages vào `ErrorMessages.cs`:

```csharp
public struct MyEntity
{
    public const string NotFound = "Entity không tồn tại.";
    public const string InvalidData = "Dữ liệu không hợp lệ.";
    public const string AlreadyExists = "Entity đã tồn tại.";
}
```

## 📄 Pagination System

### Tổng quan

Hệ thống pagination được thiết kế để xử lý dữ liệu lớn một cách hiệu quả với các tính năng:
- **Pagination**: Phân trang dữ liệu
- **Search**: Tìm kiếm tự động trên các trường string
- **Sorting**: Sắp xếp theo các trường khác nhau
- **Validation**: Kiểm tra đầu vào

### Cách sử dụng

#### 1. DTOs cho Pagination

```csharp
// Request DTO
public class PaginationRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

// Response DTO
public class PaginationResponseDto<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

#### 2. Repository Pattern

```csharp
// Repository cần implement method GetQueryable()
public interface IMyEntityRepository
{
    IQueryable<MyEntity> GetQueryable();
    // ... other methods
}

public class MyEntityRepository : BaseRepository<MyEntity>, IMyEntityRepository
{
    public IQueryable<MyEntity> GetQueryable()
    {
        return _dbSet.Where(e => !e.IsDeleted);
    }
}
```

#### 3. Service Implementation

```csharp
public class MyEntityService : IMyEntityService
{
    private readonly IMyEntityRepository _repository;
    private readonly IPaginationService _paginationService;
    private readonly IMapper _mapper;

    public MyEntityService(
        IMyEntityRepository repository,
        IPaginationService paginationService,
        IMapper mapper)
    {
        _repository = repository;
        _paginationService = paginationService;
        _mapper = mapper;
    }

    public async Task<PaginationResponseDto<MyEntityDto>> GetAllAsync(PaginationRequestDto paginationRequest)
    {
        var query = _repository.GetQueryable();
        
        // Thêm filter nếu cần
        // query = query.Where(e => e.Status == EntityStatus.Active);

        var pagedResult = await _paginationService.GetPagedResultAsync(query, paginationRequest);

        return new PaginationResponseDto<MyEntityDto>
        {
            Data = _mapper.Map<IEnumerable<MyEntityDto>>(pagedResult.Data),
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };
    }
}
```

#### 4. Controller Implementation

```csharp
[ApiController]
[CustomModelValidationFilter]
public class MyEntityController : BaseController
{
    private readonly IMyEntityService _service;

    [HttpGet("api/my-entities")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequestDto paginationRequest)
    {
        var result = await _service.GetAllAsync(paginationRequest);
        return Ok(new ResponseDto<PaginationResponseDto<MyEntityDto>>(result, "Lấy danh sách thành công"));
    }
}
```

### API Usage Examples

#### Request Examples

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

#### Response Example

```json
{
  "data": {
    "data": [
      {
        "id": 1,
        "firstName": "John",
        "lastName": "Doe",
        "email": "john@example.com"
      }
    ],
    "totalCount": 150,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 15,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "message": "Lấy danh sách thành công",
  "statusCode": 200
}
```

### Tính năng nâng cao

#### 1. Custom Search Fields

```csharp
// Trong service, có thể customize search logic
public async Task<PaginationResponseDto<MyEntityDto>> SearchAsync(
    PaginationRequestDto paginationRequest, 
    string? customFilter = null)
{
    var query = _repository.GetQueryable();
    
    if (!string.IsNullOrEmpty(customFilter))
    {
        query = query.Where(e => e.CustomField.Contains(customFilter));
    }

    var pagedResult = await _paginationService.GetPagedResultAsync(query, paginationRequest);
    // ... rest of implementation
}
```

#### 2. Multiple Sort Fields

```csharp
// Có thể extend PaginationService để hỗ trợ multiple sort
public class AdvancedPaginationService : IPaginationService
{
    public async Task<PaginationResponseDto<T>> GetPagedResultWithMultiSortAsync<T>(
        IQueryable<T> query,
        PaginationRequestDto paginationRequest,
        List<SortField> sortFields) where T : class
    {
        // Implementation for multiple sort fields
    }
}
```

### Best Practices

1. **Always use GetQueryable()** trong repository để tận dụng IQueryable
2. **Filter IsDeleted** trong GetQueryable() method
3. **Use AutoMapper** để map entities sang DTOs
4. **Validate pagination parameters** với Data Annotations
5. **Set reasonable page size limits** (1-100)
6. **Use consistent response format** với ResponseDto<T>

## 🧪 Testing Guidelines

### Unit Testing

```csharp
[Test]
public async Task CreateAsync_ValidData_ReturnsEntity()
{
    // Arrange
    var dto = new CreateMyEntityDto { Name = "Test" };
    var entity = new MyEntity { Name = "Test" };
    
    _repository.Setup(x => x.AddAsync(It.IsAny<MyEntity>()))
               .ReturnsAsync(entity);
    
    // Act
    var result = await _service.CreateAsync(dto);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test", result.Name);
}
```

## 📋 Code Review Checklist

### ✅ Before Submitting Code

- [ ] Entity kế thừa từ `BaseEntity`
- [ ] Service sử dụng `IAuditService`
- [ ] Controller có `[CustomModelValidationFilter]`
- [ ] Authorization được set đúng role
- [ ] DTOs có validation attributes
- [ ] Error messages được định nghĩa trong `ErrorMessages.cs`
- [ ] Response format sử dụng `ResponseDto<T>`
- [ ] Repository methods filter `IsDeleted = false`
- [ ] Repository có method `GetQueryable()` cho pagination
- [ ] Pagination sử dụng `IPaginationService` cho large datasets
- [ ] AutoMapper profile được tạo và đăng ký
- [ ] Unit tests được viết cho business logic

### 🚫 Common Mistakes to Avoid

- ❌ Không sử dụng `BaseEntity` cho entities
- ❌ Set audit fields thủ công thay vì dùng `AuditService`
- ❌ Quên thêm authorization cho endpoints
- ❌ Không sử dụng `CustomModelValidationFilter`
- ❌ Hardcode error messages thay vì dùng constants
- ❌ Không filter `IsDeleted` trong queries
- ❌ Quên đăng ký services trong DI container

## 🧪 Development Endpoints

### POST /api/auth/create-user-dev

Endpoint đặc biệt dành cho development để tạo user và nhận token ngay lập tức.

**Mục đích:**
- Test nhanh mà không cần gửi email OTL
- Tạo user và nhận token trong một request
- Chỉ dành cho môi trường development

**Sử dụng:**
```bash
# Tạo user và nhận token
curl -X POST "https://localhost:7000/api/auth/create-user-dev" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <admin-token>" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "firstName": "Test",
    "lastName": "User",
    "role": 4
  }'
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

**Lưu ý:** 
- Chỉ sử dụng trong development
- Trong production, sử dụng `/api/auth/create-user` để gửi email OTL
- User được tạo với password mặc định "123"

## 🔧 Development Tools

### Required Extensions (VS Code)

- C# Dev Kit
- .NET Extension Pack
- Entity Framework Core Tools
- AutoMapper Extension

### Useful Commands

```bash
# Add new migration
dotnet ef migrations add MigrationName --project Infrastructure --startup-project EduShpere

# Update database
dotnet ef database update --project Infrastructure --startup-project EduShpere

# Build project
dotnet build

# Run project
dotnet run --project EduShpere
```

## 📞 Support

Nếu có thắc mắc hoặc cần hỗ trợ, vui lòng liên hệ:

- **Technical Lead**: [Tên người phụ trách]
- **Email**: [email]
- **Slack**: #edushpere-backend

## 🤖 AI Tournament Schedule Generation System

### Tổng quan

Hệ thống AI tạo lịch thi đấu sử dụng kết hợp Machine Learning (ML.NET) và OR-Tools để tối ưu hóa lịch thi đấu cho hội thao, tự động tránh conflicts với lịch học và các hoạt động khác.

### Kiến trúc hệ thống

```
Frontend (AISchedule.jsx)
    ↓
ActivityController.GenerateTournamentSchedule()
    ↓
TournamentScheduleService.GenerateScheduleAsync()
    ↓
ESP.AIService.TournamentScheduleService.GenerateSchedule()
    ├── TournamentScheduleMLService.GenerateSlotsWithScores()  // Tạo slots với ML scores
    └── ORToolsScheduler.OptimizeSchedule()                    // Tối ưu hóa với OR-Tools
```

### Flow xử lý chính

#### 1. Frontend Request (AISchedule.jsx)

```jsx
// User nhập thông tin:
- sportId, classGroupIds (ít nhất 2 lớp)
- startDate, endDate
- matchDuration, preferredStartTime, preferredEndTime
- availableLocations, maxMatchesPerDay
- tournamentFormat (SingleElimination, RoundRobin, DoubleElimination)
- userNotes (ghi chú cho AI)

// Gọi API:
POST /api/activity/{id}/generate-schedule
```

#### 2. Backend Validation (TournamentScheduleService.cs)

**Validation quan trọng:**
- ✅ StartDate >= Today (không cho phép ngày quá khứ)
- ✅ StartDate <= EndDate
- ✅ Chỉ lấy lớp thuộc niên khóa hiện tại (filter theo AcademicYearId)
- ✅ Cần ít nhất 2 lớp để tạo lịch

**Lưu ý:** Backend tự động filter `ClassGroupIds` để chỉ giữ các lớp thuộc niên khóa hiện tại. Nếu có lớp không thuộc niên khóa, sẽ throw BadRequestException.

#### 3. ML Service - Generate Slots (TournamentScheduleMLService.cs)

**Bước 1: Tạo slots từ khoảng thời gian**
- Tạo tất cả slots có thể từ `StartDate` đến `EndDate`
- Mỗi slot có: MatchDate, StartTime, EndTime, Location
- Tính ML Score cho mỗi slot dựa trên:
  - Historical data (nếu có model đã train)
  - UserNotes (NLP analysis: ưu tiên buổi sáng/chiều, tránh giờ cao điểm, etc.)

**Bước 2: Adjust ML Score theo UserNotes**
- Phân tích UserNotes bằng NLP (tokenize, sentiment analysis)
- Keywords: "sáng", "chiều", "cuối tuần", "tránh giờ cao điểm", etc.
- Điều chỉnh score với weighted combination:
  - Time preference: 40%
  - Day preference: 30%
  - Context score: 30%

**Vấn đề hiện tại:**
- ❌ ML model chưa được train đầy đủ (cần ít nhất 20 matches completed)
- ❌ Default score = 0.5 nếu không có model
- ⚠️ NLP analysis đơn giản, có thể miss các yêu cầu phức tạp

#### 4. OR-Tools Optimization (ORToolsScheduler.cs)

**Bước 1: Filter Available Slots**
- Loại bỏ slots conflict với:
  - Matches đã có (cùng ngày, overlap thời gian, cùng sân)
  - Activities khác của participants (cross-activity conflict)
  - Khung giờ cấm: 11:30 - 13:00 (nghỉ trưa)
  - Location không có trong AvailableLocations

**Lưu ý quan trọng:**
- ✅ KHÔNG filter slots dựa trên lịch học của TẤT CẢ lớp ở bước này
- ✅ Chỉ check conflict lịch học khi tạo match cụ thể (chỉ check 2 lớp tham gia match đó)
- ✅ Điều này cho phép linh hoạt: nếu lịch thi eo hẹp, vẫn có thể tạo match miễn là 2 lớp tham gia rảnh

**Bước 2: Heuristic Scoring**
- Ưu tiên slot mà nhiều lớp đều rảnh (80%+ lớp rảnh: +10 điểm, 50-80%: +5 điểm)
- Ưu tiên slot 11h-12h (giữa 2 buổi học): +15 điểm
- Ưu tiên phân bổ đều các sân (nếu có nhiều sân)
- Penalty cho cuối tuần (không cộng điểm)

**Bước 3: OR-Tools Solver**
- Variables: x[i] = 1 nếu slot i được chọn
- Constraints:
  1. Chọn ít nhất `actualMinSlots` slots (tối thiểu lý thuyết hoặc số slots có sẵn)
  2. Tối đa `MaxMatchesPerDay` matches mỗi ngày
  3. Không overlap thời gian (trừ khi có nhiều sân và slots ở sân khác nhau)
- Objective: Tối đa hóa tổng ML Score

**Vấn đề hiện tại:**
- ⚠️ Nếu không đủ slots, constraint có thể INFEASIBLE
- ⚠️ Logic thêm slots sau khi OR-Tools solve có thể không tối ưu
- ⚠️ MatchNumber được đánh lại theo thời gian, có thể làm rối NextMatchId mapping

#### 5. Generate Matches (ORToolsScheduler.GenerateMatches)

**Single Elimination Logic:**
- Tính số rounds: `ceil(log2(n))` với n = số lớp
- Chia nhánh nếu số lớp > 8 (chia thành 2 nhánh A và B)
- Tạo matches theo bracket tree:
  - Round 1: n/2 matches (hoặc n/2 + 1 nếu n lẻ)
  - Round 2: n/4 matches
  - ...
  - Chung kết: 1 match

**Xếp lịch thông minh:**
- Phân tích lịch học: xác định lớp nào học buổi sáng/chiều
- Ưu tiên xếp lớp buổi sáng đấu với nhau vào buổi chiều
- Ưu tiên xếp lớp buổi chiều đấu với nhau vào buổi sáng
- Check conflict lịch học khi gán slot cho match (chỉ check 2 lớp tham gia)

**Vấn đề hiện tại:**
- ❌ Logic chia nhánh phức tạp, có thể tạo ra matches không hợp lý
- ❌ Nếu không đủ slots cho tất cả rounds, matches sau sẽ không có slot
- ⚠️ NextMatchId mapping có thể sai nếu MatchNumber bị đánh lại

#### 6. Apply Schedule (TournamentScheduleService.ApplyScheduleAsync)

**Validation:**
- ✅ Tất cả matches phải có MatchDate, StartTime, EndTime
- ✅ Không có duplicate MatchNumber
- ✅ Không overlap thời gian (cùng ngày, cùng sân)
- ✅ Check conflicts:
  - Cross-activity conflicts (participants đã tham gia activity khác)
  - Class schedule conflicts (trùng với lịch học)

**Lưu ý:**
- Nếu có conflicts, trả về HTTP 409 với danh sách conflicts chi tiết
- Frontend cần hiển thị conflicts để user điều chỉnh

### Các vấn đề và Logic chưa hợp lý

#### 🔴 Vấn đề nghiêm trọng

1. **ML Model chưa được train**
   - Model cần ít nhất 20 matches completed để train
   - Nếu không có model, tất cả slots có score = 0.5 (không phân biệt)
   - **Giải pháp:** Cần train model định kỳ hoặc có default model tốt hơn

2. **Conflict checking không đầy đủ**
   - Chỉ check conflict lịch học khi tạo match, không check khi filter slots
   - Có thể tạo ra matches conflict với lịch học của một số lớp
   - **Giải pháp:** Cần check conflict lịch học sớm hơn trong quá trình filter slots

3. **NextMatchId mapping sai**
   - MatchNumber được đánh lại theo thời gian sau khi tạo matches
   - NextMatchId vẫn dùng MatchNumber cũ → mapping sai
   - **Giải pháp:** Cần map NextMatchId sau khi đánh lại MatchNumber

#### ⚠️ Vấn đề cần cải thiện

4. **Slot selection không tối ưu**
   - OR-Tools có thể chọn quá ít slots, không đủ cho tất cả rounds
   - Logic thêm slots sau khi solve không đảm bảo tối ưu
   - **Giải pháp:** Cần tính toán số slots cần thiết chính xác hơn

5. **Heuristic scoring chưa đủ thông minh**
   - Chỉ dựa trên lịch học cơ bản, chưa xét đến:
     - Sức khỏe học sinh (không nên xếp quá nhiều trận trong ngày)
     - Khoảng cách giữa các trận của cùng một lớp
     - Ưu tiên các slot có nhiều lớp rảnh
   - **Giải pháp:** Cải thiện heuristic với nhiều yếu tố hơn

6. **UserNotes parsing đơn giản**
   - NLP analysis chỉ dựa trên keywords, không hiểu ngữ cảnh
   - Có thể miss các yêu cầu phức tạp
   - **Giải pháp:** Cải thiện NLP hoặc dùng AI model tốt hơn

7. **Error handling chưa đầy đủ**
   - Nếu OR-Tools không tìm được solution, chỉ trả về error message
   - Không có fallback mechanism
   - **Giải pháp:** Cần có fallback (ví dụ: greedy algorithm) khi OR-Tools fail

### Best Practices

#### 1. Train ML Model định kỳ

```csharp
// Gọi API train model sau khi có đủ dữ liệu
POST /api/activity/train-schedule-model
[Authorize(Roles = "Admin")]
```

#### 2. Validate input đầy đủ

```csharp
// Frontend: Validate trước khi gửi request
- classGroupIds.length >= 2
- startDate <= endDate
- startDate >= today
- availableLocations không rỗng (nếu có)
```

#### 3. Handle conflicts properly

```csharp
// Backend: Trả về conflicts chi tiết
if (conflicts.Any())
{
    return StatusCode(409, new ResponseDto<ApplyTournamentScheduleResponseDto>
    {
        Data = response,
        Conflicts = conflicts
    });
}
```

#### 4. Test với nhiều scenarios

- Test với ít slots available
- Test với nhiều conflicts
- Test với lịch học phức tạp
- Test với nhiều sân

### Code Review Checklist

- [ ] ML model đã được train (hoặc có default model)
- [ ] Conflict checking đầy đủ (lịch học, activities khác)
- [ ] NextMatchId mapping đúng sau khi đánh lại MatchNumber
- [ ] Slot selection đủ cho tất cả rounds
- [ ] Error handling có fallback mechanism
- [ ] UserNotes được parse đúng
- [ ] Heuristic scoring hợp lý
- [ ] Test với edge cases

---

**Lưu ý**: Tài liệu này sẽ được cập nhật thường xuyên. Vui lòng kiểm tra phiên bản mới nhất trước khi bắt đầu development.
