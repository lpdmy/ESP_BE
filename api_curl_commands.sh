#!/bin/bash

# EduShpere API cURL Commands
# Base URL - thay đổi theo môi trường của bạn
BASE_URL="https://localhost:7000"

# JWT Token - sẽ được cập nhật sau khi login
JWT_TOKEN=""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🚀 EduShpere API cURL Commands${NC}"
echo "=================================="

# Function to make API calls
make_request() {
    local method=$1
    local url=$2
    local data=$3
    local description=$4
    
    echo -e "\n${YELLOW}📋 $description${NC}"
    echo "Method: $method"
    echo "URL: $url"
    
    if [ -n "$data" ]; then
        echo "Data: $data"
        curl -X $method \
             -H "Content-Type: application/json" \
             -H "Authorization: Bearer $JWT_TOKEN" \
             -d "$data" \
             "$url" | jq .
    else
        curl -X $method \
             -H "Authorization: Bearer $JWT_TOKEN" \
             "$url" | jq .
    fi
    
    echo -e "\n${GREEN}✅ Request completed${NC}"
    echo "----------------------------------"
}

# 1. Authentication
echo -e "\n${BLUE}🔐 AUTHENTICATION${NC}"

# Login
echo -e "\n${YELLOW}📋 Login${NC}"
LOGIN_RESPONSE=$(curl -s -X POST \
    -H "Content-Type: application/json" \
    -d '{
        "username": "student001",
        "password": "Student123!"
    }' \
    "$BASE_URL/api/auth/login")

echo "$LOGIN_RESPONSE" | jq .

# Extract JWT token
JWT_TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.data.accessToken')
echo -e "\n${GREEN}🔑 JWT Token: $JWT_TOKEN${NC}"

# Get Me
make_request "GET" "$BASE_URL/api/auth/me" "" "Get Current User Info"

# Test endpoint
make_request "GET" "$BASE_URL/api/auth/test" "" "Test API Health"

# 2. Student Profile
echo -e "\n${BLUE}👤 STUDENT PROFILE${NC}"

# Get My Profile
make_request "GET" "$BASE_URL/api/user-profile/my-profile" "" "Get My Student Profile"

# Update My Profile
make_request "PUT" "$BASE_URL/api/user-profile/my-profile" '{
    "bio": "Sinh viên năm 1 chuyên ngành Công nghệ thông tin, đam mê lập trình",
    "avatarUrl": "https://example.com/avatars/student001_new.jpg",
    "birthDate": "2005-01-01T00:00:00Z",
    "phoneNumber": "+84901234567"
}' "Update My Student Profile"

# Get All Student Profiles (Admin only)
make_request "GET" "$BASE_URL/api/user-profile/profiles" "" "Get All Student Profiles (Admin)"

# Create Student Profile (Admin only)
make_request "POST" "$BASE_URL/api/user-profile/profiles" '{
    "userId": 2,
    "studentCode": "SV002",
    "enrollmentYear": 2024,
    "major": "Toán học",
    "class": "MATH01",
    "bio": "Sinh viên năm 1 chuyên ngành Toán học",
    "avatarUrl": "https://example.com/avatars/student002.jpg",
    "birthDate": "2005-02-01T00:00:00Z",
    "phoneNumber": "+84901234568",
    "extraJson": "{\"hobbies\":[\"Toán học\",\"Thống kê\"],\"skills\":[\"Python\",\"R\"]}"
}' "Create Student Profile (Admin)"

# 3. Teacher Profile
echo -e "\n${BLUE}👨‍🏫 TEACHER PROFILE${NC}"

# Get My Teacher Profile
make_request "GET" "$BASE_URL/api/user-profile/my-teacher-profile" "" "Get My Teacher Profile"

# Update My Teacher Profile
make_request "PUT" "$BASE_URL/api/user-profile/my-teacher-profile" '{
    "bio": "Giảng viên có 10 năm kinh nghiệm giảng dạy môn Lập trình, chuyên gia về C# và .NET",
    "avatarUrl": "https://example.com/avatars/teacher001_updated.jpg",
    "birthDate": "1985-03-15T00:00:00Z",
    "phoneNumber": "+84901234569"
}' "Update My Teacher Profile"

# Get All Teacher Profiles (Admin only)
make_request "GET" "$BASE_URL/api/user-profile/teacher-profiles" "" "Get All Teacher Profiles (Admin)"

# Create Teacher Profile (Admin only)
make_request "POST" "$BASE_URL/api/user-profile/teacher-profiles" '{
    "userId": 4,
    "teacherCode": "GV002",
    "department": "Toán học",
    "position": "Trưởng khoa",
    "bio": "Giảng viên có 15 năm kinh nghiệm giảng dạy môn Toán học",
    "avatarUrl": "https://example.com/avatars/teacher002.jpg",
    "birthDate": "1980-05-20T00:00:00Z",
    "phoneNumber": "+84901234570",
    "extraJson": "{\"specialties\":[\"Toán học\",\"Thống kê\"],\"education\":\"Tiến sĩ Toán học\",\"experience\":\"15 năm\"}"
}' "Create Teacher Profile (Admin)"

# 4. Activities
echo -e "\n${BLUE}🎯 ACTIVITIES${NC}"

# Get All Activities
make_request "GET" "$BASE_URL/api/activities?pageNumber=1&pageSize=10&search=programming" "" "Get All Activities"

# Get Activity by ID
make_request "GET" "$BASE_URL/api/activities/1" "" "Get Activity by ID"

# Create Activity
make_request "POST" "$BASE_URL/api/activities" '{
    "title": "Seminar Machine Learning",
    "description": "Seminar về Machine Learning và ứng dụng thực tế",
    "startDate": "2024-03-01T14:00:00Z",
    "endDate": "2024-03-01T16:00:00Z",
    "location": "Phòng B201",
    "maxParticipants": 30,
    "activityType": "Seminar"
}' "Create Activity"

# Update Activity
make_request "PUT" "$BASE_URL/api/activities" '{
    "id": 1,
    "title": "Workshop Lập trình C# - Nâng cao",
    "description": "Workshop học lập trình C# từ cơ bản đến nâng cao, tập trung vào ASP.NET Core",
    "startDate": "2024-02-01T09:00:00Z",
    "endDate": "2024-02-01T17:00:00Z",
    "location": "Phòng A101",
    "maxParticipants": 60,
    "activityType": "Workshop"
}' "Update Activity"

# 5. Activity Participants
echo -e "\n${BLUE}👥 ACTIVITY PARTICIPANTS${NC}"

# Add Participant
make_request "POST" "$BASE_URL/api/activity-participants" '{
    "activityId": 1,
    "userId": 1,
    "status": "Registered"
}' "Add Activity Participant"

# Remove Participant
make_request "DELETE" "$BASE_URL/api/activity-participants?participationId=1" "" "Remove Activity Participant"

# 6. File Upload
echo -e "\n${BLUE}📁 FILE UPLOAD${NC}"

echo -e "\n${YELLOW}📋 Upload File${NC}"
echo "Method: POST"
echo "URL: $BASE_URL/api/upload"
echo "Note: This requires a file upload. Use Postman or similar tool for file uploads."
echo "Example: curl -X POST -F 'file=@/path/to/your/file.jpg' '$BASE_URL/api/upload'"

# 7. Error Examples
echo -e "\n${BLUE}❌ ERROR EXAMPLES${NC}"

# Unauthorized request
echo -e "\n${YELLOW}📋 Unauthorized Request (No Token)${NC}"
curl -s -X GET "$BASE_URL/api/user-profile/my-profile" | jq .

# Invalid data
echo -e "\n${YELLOW}📋 Invalid Data (Missing Required Fields)${NC}"
curl -s -X POST \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $JWT_TOKEN" \
    -d '{
        "username": "",
        "password": ""
    }' \
    "$BASE_URL/api/auth/login" | jq .

echo -e "\n${GREEN}🎉 All API calls completed!${NC}"
echo "=================================="
echo "Note: Some requests may fail due to authorization or data validation."
echo "Make sure to update the JWT_TOKEN variable after successful login."

