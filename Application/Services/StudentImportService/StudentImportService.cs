using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Application.Services;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Infrastructure.Security;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Application.Services
{
    public interface IStudentImportService
    {
        Task<ImportStudentsResponseDto> ImportStudentsAsync(ImportStudentsRequestDto request);
        Task<ValidateStudentsResponseDto> ValidateStudentsAsync(ValidateStudentsRequestDto request);
        Task<byte[]> GenerateTemplateAsync();
    }

    public class StudentImportService : IStudentImportService
    {
        private readonly IUserRepository _userRepository;
        private readonly IStudentProfileRepository _studentProfileRepository;
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;

        // Required fields for student import
        private readonly string[] _requiredFields = { "studentId", "firstName", "lastName", "email", "phone", "dateOfBirth", "enrollmentYear", "grade" };

        public StudentImportService(
            IUserRepository userRepository,
            IStudentProfileRepository studentProfileRepository,
            IAuditService auditService,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _studentProfileRepository = studentProfileRepository;
            _auditService = auditService;
            _mapper = mapper;
        }

        public async Task<ImportStudentsResponseDto> ImportStudentsAsync(ImportStudentsRequestDto request)
        {
            var response = new ImportStudentsResponseDto
            {
                Total = request.Students.Count
            };

            var errors = new List<ImportErrorDto>();
            var successItems = new List<ImportSuccessDto>();

            // Step 1: Validate all students first
            var validStudents = new List<(int Index, CreateStudentDto StudentDto, Dictionary<string, string> OriginalData)>();
            
            for (int i = 0; i < request.Students.Count; i++)
            {
                var studentData = request.Students[i];
                
                // Validate required fields
                var validationErrors = ValidateStudentData(studentData, request.Mapping);
                if (validationErrors.Any())
                {
                    errors.Add(new ImportErrorDto
                    {
                        Row = studentData,
                        Errors = validationErrors
                    });
                    continue;
                }

                // Map student data
                var studentDto = MapStudentData(studentData, request.Mapping);
                validStudents.Add((i, studentDto, studentData));
            }

            if (!validStudents.Any())
            {
                response.Success = 0;
                response.Failed = errors.Count;
                response.Errors = errors;
                return response;
            }

            // Step 2: Batch check for existing students
            var existingStudents = await BatchCheckExistingStudentsAsync(validStudents.Select(v => v.StudentDto).ToList());

            // Step 3: Process students based on import mode
            if (request.ImportMode == "insert")
            {
                // Filter out existing students for insert mode
                var newStudents = validStudents.Where(v => !existingStudents.ContainsKey(v.StudentDto.StudentId) && 
                                                          !existingStudents.ContainsKey(v.StudentDto.Email)).ToList();
                
                if (newStudents.Any())
                {
                    var bulkResult = await BulkCreateStudentsAsync(newStudents.Select(v => v.StudentDto).ToList());
                    successItems.AddRange(bulkResult);
                }
            }
            else if (request.ImportMode == "upsert")
            {
                // Separate new and existing students
                var newStudents = validStudents.Where(v => !existingStudents.ContainsKey(v.StudentDto.StudentId) && 
                                                          !existingStudents.ContainsKey(v.StudentDto.Email)).ToList();
                var existingStudentsToUpdate = validStudents.Where(v => existingStudents.ContainsKey(v.StudentDto.StudentId) || 
                                                                       existingStudents.ContainsKey(v.StudentDto.Email)).ToList();

                // Bulk create new students
                if (newStudents.Any())
                {
                    var bulkResult = await BulkCreateStudentsAsync(newStudents.Select(v => v.StudentDto).ToList());
                    successItems.AddRange(bulkResult);
                }

                // Bulk update existing students
                if (existingStudentsToUpdate.Any())
                {
                    var bulkUpdateResult = await BulkUpdateExistingStudentsAsync(existingStudentsToUpdate.Select(v => v.StudentDto).ToList());
                    successItems.AddRange(bulkUpdateResult);
                }
            }

            response.Success = successItems.Count;
            response.Failed = errors.Count;
            response.Errors = errors;
            response.SuccessItems = successItems;

            return response;
        }

        public async Task<ValidateStudentsResponseDto> ValidateStudentsAsync(ValidateStudentsRequestDto request)
        {
            var response = new ValidateStudentsResponseDto();
            var errors = new List<ValidationErrorDto>();

            foreach (var (studentData, index) in request.Students.Select((s, i) => (s, i)))
            {
                var validationErrors = ValidateStudentData(studentData, request.Mapping);
                if (validationErrors.Any())
                {
                    errors.Add(new ValidationErrorDto
                    {
                        RowIndex = index + 1,
                        Row = studentData,
                        Errors = validationErrors
                    });
                }
            }

            response.Valid = !errors.Any();
            response.Errors = errors;
            response.ValidCount = request.Students.Count - errors.Count;
            response.InvalidCount = errors.Count;

            return response;
        }

        public async Task<byte[]> GenerateTemplateAsync()
        {
            var headers = new[]
            {
                "Mã học sinh",
                "Họ",
                "Tên",
                "Email",
                "Số điện thoại",
                "Ngày sinh",
                "Giới tính",
                "Địa chỉ",
                "Lớp",
                "Khối",
                "Tên phụ huynh",
                "SĐT phụ huynh",
                "Email phụ huynh",
                "Ghi chú"
            };

            var sampleData = new[]
            {
                new[] { "HS001", "Nguyễn", "Văn A", "nguyenvana@example.com", "0123456789", "2010-01-15", "Nam", "123 Đường ABC, Quận 1", "10A1", "10", "Nguyễn Văn B", "0987654321", "nguyenvanb@example.com", "Học sinh giỏi" },
                new[] { "HS002", "Trần", "Thị B", "tranthib@example.com", "0123456788", "2010-03-20", "Nữ", "456 Đường XYZ, Quận 2", "10A2", "10", "Trần Thị C", "0987654322", "tranthic@example.com", "Thích môn Toán" }
            };

            var csvContent = new List<string>
            {
                string.Join(",", headers)
            };

            foreach (var row in sampleData)
            {
                csvContent.Add(string.Join(",", row.Select(field => $"\"{field}\"")));
            }

            var csvString = string.Join("\n", csvContent);
            return System.Text.Encoding.UTF8.GetBytes(csvString);
        }

        private List<string> ValidateStudentData(Dictionary<string, string> studentData, Dictionary<string, string> mapping)
        {
            var errors = new List<string>();

            // Check required fields are mapped
            foreach (var requiredField in _requiredFields)
            {
                var csvHeader = mapping.FirstOrDefault(m => m.Value == requiredField).Key;
                if (string.IsNullOrEmpty(csvHeader))
                {
                    errors.Add($"{ErrorMessages.StudentImport.FieldNotMapped} '{GetFieldDisplayName(requiredField)}'");
                    continue;
                }

                var value = studentData.GetValueOrDefault(csvHeader, "").Trim();
                if (string.IsNullOrEmpty(value))
                {
                    errors.Add($"{ErrorMessages.StudentImport.FieldEmpty} '{GetFieldDisplayName(requiredField)}'");
                }
            }

            // Validate specific field formats
            ValidateFieldFormats(studentData, mapping, errors);

            return errors;
        }

        private void ValidateFieldFormats(Dictionary<string, string> studentData, Dictionary<string, string> mapping, List<string> errors)
        {
            foreach (var kvp in mapping)
            {
                var csvHeader = kvp.Key;
                var fieldId = kvp.Value;
                var value = studentData.GetValueOrDefault(csvHeader, "").Trim();

                if (string.IsNullOrEmpty(value)) continue;

                switch (fieldId)
                {
                    case "email":
                        if (!IsValidEmail(value))
                            errors.Add($"{ErrorMessages.StudentImport.InvalidEmailFormat}: {value}");
                        break;
                    case "phone":
                        if (!IsValidPhone(value))
                            errors.Add($"{ErrorMessages.StudentImport.InvalidPhoneFormat}: {value}");
                        break;
                    case "studentId":
                        if (!IsValidStudentId(value))
                            errors.Add($"{ErrorMessages.StudentImport.InvalidStudentIdFormat}: {value}");
                        break;
                    case "dateOfBirth":
                        if (!IsValidDate(value))
                            errors.Add($"{ErrorMessages.StudentImport.InvalidDateFormat}: {value}");
                        break;
                    case "enrollmentYear":
                        if (!short.TryParse(value, out var year) || year < 2000 || year > 2030)
                            errors.Add($"Năm nhập học không hợp lệ: {value}");
                        break;
                }
            }
        }

        private async Task<Dictionary<string, string>> BatchCheckExistingStudentsAsync(List<CreateStudentDto> students)
        {
            var existingStudents = new Dictionary<string, string>();
            
            if (!students.Any()) return existingStudents;

            var emails = students.Select(s => s.Email).ToList();
            var studentIds = students.Select(s => s.StudentId).ToList();

            // Single query to check all emails and student IDs
            var existingUsers = await _userRepository.GetQueryable()
                .Where(u => emails.Contains(u.Email) || 
                           studentIds.Contains(u.Username) ||
                           (u.StudentProfile != null && studentIds.Contains(u.StudentProfile.StudentNumber)))
                .Select(u => new { u.Email, u.Username, StudentNumber = u.StudentProfile != null ? u.StudentProfile.StudentNumber : null })
                .ToListAsync();

            foreach (var user in existingUsers)
            {
                if (!string.IsNullOrEmpty(user.Email) && !existingStudents.ContainsKey(user.Email))
                    existingStudents[user.Email] = user.Email;
                
                if (!string.IsNullOrEmpty(user.Username) && !existingStudents.ContainsKey(user.Username))
                    existingStudents[user.Username] = user.Username;
                    
                if (!string.IsNullOrEmpty(user.StudentNumber) && !existingStudents.ContainsKey(user.StudentNumber))
                    existingStudents[user.StudentNumber] = user.StudentNumber;
            }

            return existingStudents;
        }

        private async Task<List<ImportSuccessDto>> BulkCreateStudentsAsync(List<CreateStudentDto> students)
        {
            var successItems = new List<ImportSuccessDto>();
            
            if (!students.Any()) return successItems;

            try
            {
                // Create users first
                var users = students.Select(studentDto => new User
                {
                    Email = studentDto.Email,
                    FirstName = studentDto.FirstName,
                    LastName = studentDto.LastName,
                    PhoneNumber = studentDto.Phone,
                    Birthdate = studentDto.DateOfBirth,
                    Role = Domain.UserRole.Student,
                    Status = Domain.Enum.UserStatus.Active,
                    Username = studentDto.StudentId,
                    Password = PasswordHelper.HashPassword(new User(), "123") // Default password
                }).ToList();

                // Set audit fields for all users
                foreach (var user in users)
                {
                    _auditService.SetAuditFieldsForCreate(user);
                }

                // Bulk insert users
                await _userRepository.AddRangeAsync(users);

                // Create student profiles
                var studentProfiles = users.Zip(students, (user, studentDto) => new StudentProfile
                {
                    UserId = user.Id,
                    StudentNumber = studentDto.StudentId,
                    EnrollmentYear = studentDto.EnrollmentYear,
                    Bio = $"{studentDto.Grade}" + (!string.IsNullOrEmpty(studentDto.Class) ? $" - {studentDto.Class}" : "")
                }).ToList();

                // Set audit fields for all profiles
                foreach (var profile in studentProfiles)
                {
                    _auditService.SetAuditFieldsForCreate(profile);
                }

                // Bulk insert student profiles
                await _studentProfileRepository.AddRangeAsync(studentProfiles);

                // Create success items
                successItems = users.Zip(students, (user, studentDto) => new ImportSuccessDto
                {
                    UserId = user.Id,
                    StudentId = studentDto.StudentId,
                    FullName = $"{studentDto.LastName} {studentDto.FirstName}",
                    Email = studentDto.Email
                }).ToList();
            }
            catch (Exception ex)
            {
                // If bulk insert fails, fall back to individual inserts
                foreach (var studentDto in students)
                {
                    try
                    {
                        var result = await CreateNewStudentAsync(studentDto);
                        if (result.Success && result.UserId > 0)
                        {
                            successItems.Add(new ImportSuccessDto
                            {
                                UserId = result.UserId,
                                StudentId = studentDto.StudentId,
                                FullName = $"{studentDto.LastName} {studentDto.FirstName}",
                                Email = studentDto.Email
                            });
                        }
                    }
                    catch
                    {
                        // Skip failed individual inserts
                        continue;
                    }
                }
            }

            return successItems;
        }

        private async Task<List<ImportSuccessDto>> BulkUpdateExistingStudentsAsync(List<CreateStudentDto> students)
        {
            var successItems = new List<ImportSuccessDto>();
            
            if (!students.Any()) return successItems;

            try
            {
                var emails = students.Select(s => s.Email).ToList();
                var studentIds = students.Select(s => s.StudentId).ToList();

                // Single query to get all existing users that need to be updated
                var existingUsers = await _userRepository.GetQueryable()
                    .Include(u => u.StudentProfile)
                    .Where(u => emails.Contains(u.Email) || 
                               studentIds.Contains(u.Username) ||
                               (u.StudentProfile != null && studentIds.Contains(u.StudentProfile.StudentNumber)))
                    .ToListAsync();

                // Create a lookup dictionary for quick access
                var userLookup = new Dictionary<string, User>();
                foreach (var user in existingUsers)
                {
                    if (!string.IsNullOrEmpty(user.Email) && !userLookup.ContainsKey(user.Email))
                        userLookup[user.Email] = user;
                    
                    if (!string.IsNullOrEmpty(user.Username) && !userLookup.ContainsKey(user.Username))
                        userLookup[user.Username] = user;
                        
                    if (user.StudentProfile != null && !string.IsNullOrEmpty(user.StudentProfile.StudentNumber) && !userLookup.ContainsKey(user.StudentProfile.StudentNumber))
                        userLookup[user.StudentProfile.StudentNumber] = user;
                }

                var usersToUpdate = new List<User>();
                var profilesToUpdate = new List<StudentProfile>();

                // Prepare all updates
                foreach (var studentDto in students)
                {
                    User existingUser = null;
                    
                    // Try to find user by email first, then by studentId
                    if (userLookup.TryGetValue(studentDto.Email, out existingUser) ||
                        userLookup.TryGetValue(studentDto.StudentId, out existingUser))
                    {
                        // Update user info
                        existingUser.FirstName = studentDto.FirstName;
                        existingUser.LastName = studentDto.LastName;
                        existingUser.PhoneNumber = studentDto.Phone;
                        existingUser.Birthdate = studentDto.DateOfBirth;
                        _auditService.SetAuditFieldsForUpdate(existingUser);
                        
                        usersToUpdate.Add(existingUser);

                        // Update student profile if exists
                        if (existingUser.StudentProfile != null)
                        {
                            existingUser.StudentProfile.EnrollmentYear = studentDto.EnrollmentYear;
                            existingUser.StudentProfile.Bio = $"{studentDto.Grade}" + (!string.IsNullOrEmpty(studentDto.Class) ? $" - {studentDto.Class}" : "");
                            _auditService.SetAuditFieldsForUpdate(existingUser.StudentProfile);
                            
                            profilesToUpdate.Add(existingUser.StudentProfile);
                        }

                        successItems.Add(new ImportSuccessDto
                        {
                            UserId = existingUser.Id,
                            StudentId = studentDto.StudentId,
                            FullName = $"{studentDto.LastName} {studentDto.FirstName}",
                            Email = studentDto.Email
                        });
                    }
                }

                // Bulk update users
                if (usersToUpdate.Any())
                {
                    await _userRepository.UpdateRangeAsync(usersToUpdate);
                }

                // Bulk update student profiles
                if (profilesToUpdate.Any())
                {
                    await _studentProfileRepository.UpdateRangeAsync(profilesToUpdate);
                }
            }
            catch (Exception ex)
            {
                // If bulk update fails, fall back to individual updates
                foreach (var studentDto in students)
                {
                    try
                    {
                        var existingUser = await _userRepository.GetQueryable()
                            .Include(u => u.StudentProfile)
                            .Where(u => u.Email == studentDto.Email || 
                                       u.Username == studentDto.StudentId ||
                                       (u.StudentProfile != null && u.StudentProfile.StudentNumber == studentDto.StudentId))
                            .FirstOrDefaultAsync();

                        if (existingUser != null)
                        {
                            var updateResult = await UpdateExistingStudentAsync(existingUser, studentDto);
                            if (updateResult.Success && updateResult.UserId > 0)
                            {
                                successItems.Add(new ImportSuccessDto
                                {
                                    UserId = updateResult.UserId,
                                    StudentId = studentDto.StudentId,
                                    FullName = $"{studentDto.LastName} {studentDto.FirstName}",
                                    Email = studentDto.Email
                                });
                            }
                        }
                    }
                    catch
                    {
                        // Skip failed individual updates
                        continue;
                    }
                }
            }

            return successItems;
        }

        private async Task<(bool Success, int UserId, List<string> Errors)> ProcessStudentAsync(CreateStudentDto studentDto, string importMode)
        {
            var errors = new List<string>();

            try
            {
                // Check if student already exists by studentId or email
                var existingUser = await _userRepository.GetQueryable()
                    .Where(u => u.Email == studentDto.Email || 
                               u.Username == studentDto.StudentId ||
                               (u.StudentProfile != null && u.StudentProfile.StudentNumber == studentDto.StudentId))
                    .FirstOrDefaultAsync();

                switch (importMode)
                {
                    case "insert":
                        if (existingUser != null)
                        {
                            // Trong mode "insert", học sinh đã tồn tại không phải là lỗi, chỉ bỏ qua
                            // Trả về success = true nhưng không tạo user mới
                            return (true, 0, new List<string>());
                        }
                        return await CreateNewStudentAsync(studentDto);
                    
                    case "upsert":
                        if (existingUser != null)
                        {
                            return await UpdateExistingStudentAsync(existingUser, studentDto);
                        }
                        return await CreateNewStudentAsync(studentDto);
                    
                    default:
                        errors.Add("Chế độ import không hợp lệ");
                        return (false, 0, errors);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Lỗi xử lý: {ex.Message}");
                return (false, 0, errors);
            }
        }

        private async Task<(bool Success, int UserId, List<string> Errors)> CreateNewStudentAsync(CreateStudentDto studentDto)
        {
            try
            {
                // Check if email already exists
                if (await _userRepository.FindUserByEmail(studentDto.Email))
                {
                    return (false, 0, new List<string> { ErrorMessages.StudentImport.StudentAlreadyExists });
                }

                // Create user
                var user = new User
                {
                    Email = studentDto.Email,
                    FirstName = studentDto.FirstName,
                    LastName = studentDto.LastName,
                    PhoneNumber = studentDto.Phone,
                    Birthdate = studentDto.DateOfBirth,
                    Role = Domain.UserRole.Student,
                    Status = Domain.Enum.UserStatus.Active,
                    Username = studentDto.StudentId, // Use studentId as username
                    Password = PasswordHelper.HashPassword(new User(), "123") // Default password
                };

                _auditService.SetAuditFieldsForCreate(user);
                await _userRepository.AddAsync(user);

                // Create student profile using the same method as AuthService
                var studentProfile = new StudentProfile
                {
                    UserId = user.Id,
                    StudentNumber = studentDto.StudentId,
                    EnrollmentYear = studentDto.EnrollmentYear,
                    Bio = $"{studentDto.Grade}" + (!string.IsNullOrEmpty(studentDto.Class) ? $" - {studentDto.Class}" : "")
                };

                // Use CreateStudentProfileAsync to match AuthService pattern
                await _studentProfileRepository.CreateStudentProfileAsync(
                    studentProfile, 
                    studentDto.DateOfBirth, 
                    studentDto.Phone, 
                    null, // AvatarUrl
                    null  // ClassGroupId
                );

                return (true, user.Id, new List<string>());
            }
            catch (Exception ex)
            {
                return (false, 0, new List<string> { $"{ErrorMessages.StudentImport.CreateStudentFailed}: {ex.Message}" });
            }
        }

        private async Task<(bool Success, int UserId, List<string> Errors)> UpdateExistingStudentAsync(User existingUser, CreateStudentDto studentDto)
        {
            try
            {
                // Update user info
                existingUser.FirstName = studentDto.FirstName;
                existingUser.LastName = studentDto.LastName;
                existingUser.PhoneNumber = studentDto.Phone;
                existingUser.Birthdate = studentDto.DateOfBirth;

                _auditService.SetAuditFieldsForUpdate(existingUser);
                await _userRepository.UpdateAsync(existingUser);

                // Update student profile
                if (existingUser.StudentProfile != null)
                {
                    existingUser.StudentProfile.EnrollmentYear = studentDto.EnrollmentYear;
                    existingUser.StudentProfile.Bio = $"{studentDto.Grade}" + (!string.IsNullOrEmpty(studentDto.Class) ? $" - {studentDto.Class}" : "");
                    _auditService.SetAuditFieldsForUpdate(existingUser.StudentProfile);
                    await _studentProfileRepository.UpdateAsync(existingUser.StudentProfile);
                }

                return (true, existingUser.Id, new List<string>());
            }
            catch (Exception ex)
            {
                return (false, 0, new List<string> { $"Lỗi cập nhật học sinh: {ex.Message}" });
            }
        }

        private CreateStudentDto MapStudentData(Dictionary<string, string> studentData, Dictionary<string, string> mapping)
        {
            var studentDto = new CreateStudentDto();

            foreach (var kvp in mapping)
            {
                var csvHeader = kvp.Key;
                var fieldId = kvp.Value;
                var value = studentData.GetValueOrDefault(csvHeader, "").Trim();

                switch (fieldId)
                {
                    case "studentId":
                        studentDto.StudentId = value;
                        break;
                    case "firstName":
                        studentDto.FirstName = value;
                        break;
                    case "lastName":
                        studentDto.LastName = value;
                        break;
                    case "email":
                        studentDto.Email = value;
                        break;
                    case "phone":
                        studentDto.Phone = value;
                        break;
                    case "dateOfBirth":
                        if (DateTime.TryParse(value, out var date))
                            studentDto.DateOfBirth = date;
                        break;
                    case "enrollmentYear":
                        if (short.TryParse(value, out var year))
                            studentDto.EnrollmentYear = year;
                        break;
                    case "grade":
                        studentDto.Grade = value;
                        break;
                    case "class":
                        studentDto.Class = value;
                        break;
                }
            }

            return studentDto;
        }

        private string GetFieldDisplayName(string fieldId)
        {
            return fieldId switch
            {
                "studentId" => "Mã học sinh",
                "firstName" => "Tên",
                "lastName" => "Họ",
                "email" => "Email",
                "phone" => "Số điện thoại",
                "dateOfBirth" => "Ngày sinh",
                "gender" => "Giới tính",
                "address" => "Địa chỉ",
                "class" => "Lớp",
                "grade" => "Khối",
                "parentName" => "Tên phụ huynh",
                "parentPhone" => "SĐT phụ huynh",
                "parentEmail" => "Email phụ huynh",
                "notes" => "Ghi chú",
                _ => fieldId
            };
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(phone.Replace(" ", ""), @"^[\+]?[0-9\s\-\(\)]{10,}$");
        }

        private bool IsValidStudentId(string studentId)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(studentId, @"^[A-Za-z0-9]{3,}$");
        }

        private bool IsValidDate(string date)
        {
            return DateTime.TryParse(date, out _);
        }

        private class CreateStudentDto
        {
            public string StudentId { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public DateTime DateOfBirth { get; set; }
            public short EnrollmentYear { get; set; }
            public string Grade { get; set; } = string.Empty;
            public string? Class { get; set; }
        }
    }
}
