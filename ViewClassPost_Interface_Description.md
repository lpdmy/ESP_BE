# View Class Post by Teacher and Student

Function trigger:
Navigation path: "Lớp học của tôi" (My Class) → Select class → "Bài đăng của giáo viên" (Teacher's Posts) tab.
Timing demand: On demand (whenever a teacher or student wants to view class posts).
Function description:
Actors/Roles: Teacher, Student.
Purpose: Allow teachers and students to view posts shared by the homeroom teacher in their class.
Screen Layout:
Figure X: View Class Post Screen
The class posts interface consists of:
Page Header: Class name "Lớp [ClassName]" (Class [ClassName]) with academic year "Năm học [Year]" (Academic year [Year]).
Tabs: Two tabs "Bài đăng của giáo viên" (Teacher's Posts) and "Danh sách học sinh" (Student List). "Bài đăng của giáo viên" tab is currently selected.
Create Post Button: Orange button "+ Tạo bài viết" (Create Post) visible for teachers on the right side of tabs.
Posts List: List of posts displayed as cards. Each post card shows:
Author Information: Profile icon, author name, and timestamp (e.g., "7 giờ trước" - 7 hours ago).
Post Title: Post title displayed prominently.
Post Content: Content preview or full content.
Engagement: Heart icon with like count.
Action Menu: Three-dot menu icon (for post owner) with options "Chỉnh sửa" (Edit) and "Xóa" (Delete).
Empty State: Message "Chưa có bài viết nào" (No posts yet) and "Hãy tạo bài viết để chia sẻ với lớp học của bạn." (Create a post to share with your class.) when no posts exist.
Right Sidebar: Class information card, schedule card, and quick statistics card.
Data processing:
User navigates to "Lớp học của tôi" (My Class) and selects a class.
User clicks "Bài đăng của giáo viên" (Teacher's Posts) tab.
System fetches class posts from database for the selected class.
System displays posts in a list, sorted by creation date (newest first).
User can view post details, like posts, or manage posts (if teacher and post owner).
Function details:
Data: ClassId, PostId, TeacherId, Title, Content, CreatedAt, LikeCount, AttachmentUrls.
Validation: Class must exist. User must be authenticated. User must be a member of the class (teacher or student).
Authentication: JWT token required.
Business rules: Only authenticated teachers and students who are members of the class can view class posts. Posts are displayed in chronological order (newest first). Teachers can create, edit, and delete their own posts. Students can only view posts.
Normal case: User navigates to class posts tab => System fetches posts => Posts are displayed in list => User can view post details.
Abnormal case: User is not authenticated => Redirect to login page. User is not a member of the class => Display error message "Bạn không có quyền xem bài viết của lớp này" (You don't have permission to view posts of this class). Class not found => Display error message. Network error => Display error message "Có lỗi xảy ra khi tải bài viết" (Error occurred while loading posts).
