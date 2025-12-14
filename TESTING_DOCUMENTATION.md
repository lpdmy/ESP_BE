# Testing Documentation - EduShpere System

## 2.1 Testing Types

### Table 1. Type of Tests

| Type of Tests | Test Level |
|---------------|-----------|
| | **Unit** | **System** | **Acceptance** |
| **Unit Testing** | X | | |
| **Integration Testing** | | X | |
| **Functional Testing** | X | X | X |
| **System Testing** | | X | |
| **Performance Testing** | | X | |
| **Security Testing** | | X | X |
| **Regression Testing** | X | X | |
| **Acceptance Testing** | | | X |

### Table 2. Testing Types

| Level | Objective | Technique | Completion Criteria |
|-------|-----------|-----------|-------------------|
| **Unit Testing** | Verify that individual components, functions, and methods operate correctly in isolation. | Automated unit tests using testing frameworks (xUnit, NUnit) | All critical units have test coverage > 80%, all unit tests pass, and code quality metrics are met. |
| **Integration Testing** | Verify that different system components (database, APIs, external services) work together correctly. | API testing, database integration testing, third-party service testing | All integration points function correctly, data flows properly between components, and external services are properly integrated. |
| **Functional Testing** | Validate that all UI and backend features operate as intended under various scenarios. | Manual and automated tests | All features and workflows meet functional requirements. |
| **System Testing** | Verify the complete system integration and end-to-end functionality across all modules and components. | Integration testing, end-to-end testing, automated test suites | All system components work together correctly, and complete user workflows execute successfully. |
| **Performance Testing** | Validate system performance under expected load, including response times, throughput, and resource utilization. | Load testing, stress testing, concurrent request testing, performance profiling | System meets performance requirements (response time < 3s, handles 100+ concurrent users, no memory leaks). |
| **Security Testing** | Ensure the application properly enforces access controls, protects sensitive data, and prevents unauthorized access. | Penetration testing, vulnerability scanning, manual security audits, automated security tools | All security vulnerabilities are identified and resolved, access controls are properly enforced, and sensitive data is protected. |
| **Regression Testing** | Ensure that new changes or bug fixes do not break existing functionality. | Automated regression test suites, smoke testing | All existing features continue to work after changes, no new bugs introduced, and test suite passes. |
| **Acceptance Testing** | Validate that the software system meets business requirements and user expectations before production deployment. | Mentor review, faculty surveys and feedback, business process validation | Mentors and faculty members approve the system, business requirements are met, and the application is ready for production. |

## 2.2 Testing Schedule

### Table 3. Testing Milestones

| Milestone Task | Start Date | End Date |
|----------------|------------|----------|
| Init Test Plan | 07/09/2024 | 07/09/2024 |
| Define test case for sprint 1 | 07/09/2024 | 14/09/2024 |
| Execute test for sprint 1 | 14/09/2024 | 21/09/2024 |
| Define test case for sprint 2 | 21/09/2024 | 28/09/2024 |
| Execute test for sprint 2 | 28/09/2024 | 05/10/2024 |
| Define test case for sprint 3 | 05/10/2024 | 12/10/2024 |
| Execute test for sprint 3 | 12/10/2024 | 19/10/2024 |
| Define test case for sprint 4 | 19/10/2024 | 26/10/2024 |
| Execute test for sprint 4 | 26/10/2024 | 02/11/2024 |
| Define test case for sprint 5 | 02/11/2024 | 09/11/2024 |
| Execute test for sprint 5 | 09/11/2024 | 16/11/2024 |
| Define test case for sprint 6 | 16/11/2024 | 23/11/2024 |
| Execute test for sprint 6 | 23/11/2024 | 30/11/2024 |
| Define test case for sprint 7 | 30/11/2024 | 07/12/2024 |
| Execute test for sprint 7 | 07/12/2024 | 15/12/2024 |

## 2.3 Supporting Tools

### Table 4. Testing Environments

| Purpose | Tool | Provider | Version |
|---------|------|----------|---------|
| Development Environment | https://edusphere-dev.netlify.app/ | Netlify | Latest |
| Backend API Development | http://localhost:5000/ | Local Development | Latest |
| User Acceptance Testing Environment | https://edusphere-uat.netlify.app/ | Netlify | Latest |
| Production Environment | https://edusphere.netlify.app/ | Netlify | Latest |
| Database Development | SQL Server (Local/Cloud) | Microsoft Azure / Local | Latest |
| NoSQL Database Development | MongoDB Atlas | MongoDB Inc. | Latest |

### Table 5. Supporting Tools

| Purpose | Tool | Vendor/In-house | Version |
|---------|------|-----------------|---------|
| Run unit test | NUnit | Unit test open-source tool | 3.14.0 |
| Unit testing framework (.NET) | xUnit | Open-source | Latest |
| Mocking framework (.NET) | Moq | Open-source | Latest |
| Integration testing (.NET) | Microsoft.AspNetCore.Mvc.Testing | Microsoft | 8.0.19 |
| API testing | Postman | Postman Inc. | Latest |
| API documentation and testing | Swagger (Swashbuckle) | Open-source | 6.6.2 |
| Database testing | Entity Framework Core In-Memory | Microsoft | 8.0.19 |
| Performance testing | .NET Performance Profiler | Microsoft | 8.0.0 |
| Code coverage | Coverlet | Open-source | Latest |

## Acceptance Testing

### Purpose
Acceptance Testing is conducted to validate that the software system meets the business requirements and user expectations. It ensures the application is ready for production deployment and satisfies all stakeholder needs.

### Testing Approach

#### 1. User Story Validation
- **Class Management**: Verify that administrators can create, update, delete, and view class groups with all required information (name, grade, academic year, schedules)
- **Student Management**: Validate that admins can add/remove students from classes, and students can view their current class information
- **Homeroom Teacher Assignment**: Confirm that admins can assign and remove homeroom teachers, and teachers can view their assigned classes
- **Activity Management**: Test that activities can be created, managed, and students can participate in activities
- **Dashboard Functionality**: Verify that dashboard displays accurate statistics (total classes, students, teachers) and filters correctly by academic year

#### 2. Business Process Validation
- **Academic Year Workflow**: Test the complete flow from creating academic year → assigning classes → enrolling students → assigning teachers
- **Activity Registration Flow**: Validate student registration process, jury assignment, and submission workflow
- **Notification System**: Verify that users receive appropriate notifications for important events (class assignments, activity updates, etc.)
- **Search and Filter**: Test search functionality across different entities (classes, students, activities) with various filters

#### 3. User Experience Validation
- **Responsive Design**: Test application on different screen sizes and devices
- **Performance**: Verify page load times are acceptable (< 3 seconds for most pages)
- **Error Handling**: Confirm user-friendly error messages are displayed for invalid operations
- **Navigation**: Validate intuitive navigation flow between different sections

#### 4. Stakeholder Review and Surveys
- **Mentor Review**: Gather feedback from project mentors on system architecture, code quality, and overall implementation
- **Faculty Surveys**: Collect feedback from teachers and academic staff on usability, feature completeness, and educational value
- **Student Feedback**: Gather input from students on user experience, accessibility, and functionality
- **Technical Review**: Verify system maintainability, logging, and monitoring capabilities through mentor evaluation

#### 5. Data Integrity Validation
- **Data Consistency**: Verify that related data remains consistent (e.g., student count matches actual enrollments)
- **Soft Delete**: Test that deleted records are properly marked and hidden from normal views
- **Data Validation**: Confirm all required fields are validated and business rules are enforced

#### 6. Integration Points
- **Database**: Verify all database operations complete successfully and maintain referential integrity
- **External Services**: Test integration with email service, file upload service (Cloudinary), and any third-party APIs
- **Authentication**: Validate JWT token generation, validation, and refresh mechanisms

#### 7. Acceptance Criteria Checklist
- ✅ All critical user stories are implemented and functional
- ✅ System performance meets specified requirements (response time, concurrent users)
- ✅ All business rules are correctly implemented
- ✅ User interface is intuitive and accessible
- ✅ Data is accurately stored and retrieved
- ✅ Error handling provides meaningful feedback
- ✅ System documentation is complete and accurate

---

## Security Testing

### Purpose
Security Testing ensures that the application properly enforces access controls, protects sensitive data, and prevents unauthorized access to system resources.

As part of the security testing process, comprehensive security assessments were conducted by security experts and IT administrators to identify vulnerabilities, validate access control mechanisms, and ensure compliance with security standards. This step involved systematic testing of authentication mechanisms, authorization policies, data protection measures, and API security configurations. The testing process included penetration testing, vulnerability scanning, and manual security audits to verify that all security controls are properly implemented and functioning as intended. This rigorous security validation ensured that the application meets security requirements and protects sensitive user data before final deployment.

### Testing Scope

#### 1. Authentication Testing

##### 1.1 Login Security
- **Valid Credentials**: Verify users can login with correct username/password
- **Invalid Credentials**: Test system rejects incorrect credentials and provides appropriate error messages
- **Password Security**: 
  - Verify password requirements (minimum length, complexity)
  - Test password hashing (passwords should not be stored in plain text)
  - Validate password reset functionality
- **Session Management**:
  - Test JWT token expiration
  - Verify token refresh mechanism
  - Test logout functionality invalidates tokens
- **Account Lockout**: Verify system locks accounts after multiple failed login attempts

##### 1.2 Token Security
- **JWT Token Validation**: Verify tokens are properly signed and validated
- **Token Tampering**: Test that modified tokens are rejected
- **Expired Tokens**: Verify expired tokens cannot be used to access protected resources
- **Token Storage**: Ensure tokens are stored securely (not in localStorage for sensitive operations)

#### 2. Authorization Testing (Role-Based Access Control)

##### 2.1 Admin Role Permissions
- **Full System Access**: Verify Admin can access all features
- **User Management**: 
  - Create, update, delete users (Admin, Teacher, Student)
  - Assign roles to users
  - Manage user permissions
- **Class Management**:
  - Create, update, delete classes
  - Assign homeroom teachers
  - Add/remove students from classes
- **Activity Management**:
  - Create, update, delete activities
  - Assign juries to activities
  - View all submissions
- **System Configuration**:
  - Manage academic years
  - Configure system settings
  - Access audit logs

##### 2.2 Teacher Role Permissions
- **Class Access**: 
  - View assigned classes only
  - View students in assigned classes
  - Cannot access other teachers' classes
- **Activity Management**:
  - Create activities (if permitted)
  - View activities related to their classes
  - Cannot delete activities created by others
- **Student Information**:
  - View student profiles in their classes
  - Cannot modify student information
  - Cannot access students from other classes
- **Restricted Actions**:
  - Cannot create/delete classes
  - Cannot assign homeroom teachers
  - Cannot manage system users

##### 2.3 Student Role Permissions
- **Class Information**:
  - View their own class information
  - View homeroom teacher information
  - Cannot view other students' classes
- **Activity Participation**:
  - Register for activities
  - Submit work for activities
  - View their own submissions
  - Cannot view other students' submissions
- **Restricted Actions**:
  - Cannot create/modify classes
  - Cannot access admin/teacher features
  - Cannot view other students' personal information

##### 2.4 Unauthorized Access Testing
- **Direct URL Access**: Attempt to access protected endpoints without authentication
- **Role Escalation**: Test that users cannot access features beyond their role
- **Cross-User Data Access**: Verify users cannot access data belonging to other users
- **API Endpoint Protection**: Test all API endpoints require proper authorization

#### 3. Data Security Testing

##### 3.1 Input Validation
- **SQL Injection**: Test all input fields for SQL injection vulnerabilities
- **XSS (Cross-Site Scripting)**: Verify user inputs are properly sanitized
- **Command Injection**: Test for command injection in file uploads and system commands
- **Path Traversal**: Verify file access is restricted to authorized paths

##### 3.2 Data Protection
- **Sensitive Data Encryption**: 
  - Verify passwords are hashed (not plain text)
  - Test encryption of sensitive personal information
- **Data Transmission**: 
  - Verify HTTPS is enforced for all communications
  - Test that sensitive data is not exposed in URLs or logs
- **Data Storage**: 
  - Verify database connections use encrypted connections
  - Test that backup data is properly secured

##### 3.3 Data Access Controls
- **Row-Level Security**: Verify users can only access data they are authorized to view
- **Soft Delete Protection**: Test that deleted data is not accessible to unauthorized users
- **Audit Trail**: Verify all sensitive operations are logged with user information

#### 4. API Security Testing

##### 4.1 Endpoint Protection
- **Public Endpoints**: Verify only intended endpoints are publicly accessible
- **Protected Endpoints**: Test all protected endpoints require valid authentication
- **Role-Based Endpoints**: Verify endpoints enforce role-based access control
- **Rate Limiting**: Test API rate limiting to prevent abuse

##### 4.2 Request Validation
- **Input Sanitization**: Verify all inputs are validated and sanitized
- **File Upload Security**: 
  - Test file type validation
  - Verify file size limits
  - Test for malicious file uploads
- **Request Size Limits**: Verify request size limits prevent DoS attacks

##### 4.3 CORS Configuration
- **Allowed Origins**: Verify CORS is properly configured for frontend domains
- **Cross-Origin Requests**: Test that unauthorized origins are rejected
- **Credentials Handling**: Verify credentials are handled securely in cross-origin requests

#### 5. Session Security Testing

##### 5.1 Session Management
- **Session Timeout**: Verify sessions expire after inactivity
- **Concurrent Sessions**: Test behavior with multiple concurrent sessions
- **Session Fixation**: Verify session tokens are regenerated after login

##### 5.2 Cookie Security
- **HttpOnly Flag**: Verify cookies have HttpOnly flag set
- **Secure Flag**: Verify cookies use Secure flag in HTTPS
- **SameSite Attribute**: Test SameSite cookie attribute prevents CSRF

#### 6. Security Headers Testing
- **Content Security Policy (CSP)**: Verify CSP headers are set
- **X-Frame-Options**: Test X-Frame-Options prevents clickjacking
- **X-Content-Type-Options**: Verify X-Content-Type-Options prevents MIME sniffing
- **Strict-Transport-Security**: Test HSTS header enforces HTTPS

#### 7. Security Test Cases

##### Test Case 1: Unauthorized Access to Admin Endpoints
```
Given: A Student user is logged in
When: Student attempts to access /api/admin/users
Then: System returns 403 Forbidden
```

##### Test Case 2: Cross-User Data Access
```
Given: Teacher A is logged in
When: Teacher A attempts to access /api/classgroup/{id}/students where id belongs to Teacher B's class
Then: System returns 403 Forbidden or 404 Not Found
```

##### Test Case 3: SQL Injection Prevention
```
Given: User is on search page
When: User enters: ' OR '1'='1
Then: System sanitizes input and returns appropriate results (not all records)
```

##### Test Case 4: JWT Token Validation
```
Given: User has expired JWT token
When: User attempts to access protected endpoint
Then: System returns 401 Unauthorized
```

##### Test Case 5: Role-Based Feature Access
```
Given: Student user is logged in
When: Student attempts to create a new class via POST /api/classgroup
Then: System returns 403 Forbidden
```

#### 8. Security Checklist
- ✅ All endpoints require proper authentication
- ✅ Role-based access control is enforced
- ✅ User inputs are validated and sanitized
- ✅ SQL injection vulnerabilities are prevented
- ✅ XSS attacks are prevented
- ✅ Sensitive data is encrypted
- ✅ HTTPS is enforced
- ✅ Session management is secure
- ✅ CORS is properly configured
- ✅ Security headers are set
- ✅ Audit logging is implemented
- ✅ Password policies are enforced
- ✅ Account lockout is implemented
- ✅ File uploads are validated
- ✅ API rate limiting is configured

---

## Testing Tools and Methods

### Automated Testing
- **Unit Tests**: xUnit, NUnit for backend testing
- **Integration Tests**: Test API endpoints with test database
- **Security Scanning**: OWASP ZAP, Burp Suite for security testing

### Manual Testing
- **Exploratory Testing**: Manual exploration of features
- **User Acceptance Testing**: Stakeholder interviews and feedback
- **Security Penetration Testing**: Manual security testing by security experts

### Performance Testing
- **Load Testing**: Test system under expected load
- **Stress Testing**: Test system beyond normal capacity
- **Concurrent Request Testing**: Verify thread pool and connection limits handle concurrent requests




