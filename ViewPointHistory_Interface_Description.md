# View Point History by Student
Function trigger:
Navigation path: "Đổi thưởng" (Redeem Rewards) page → Click "Lịch sử điểm" (Point History) card or link.
Timing demand: On demand (whenever a student wants to view their point transaction history).
Function description:
Actors/Roles: Student.
Purpose: Allow students to view their complete point transaction history including points received and points redeemed.
Screen Layout:
Figure X: View Point History Screen
The point history interface consists of:
Header Section: Title "Lịch sử điểm" (Point History) with subtitle "Theo dõi toàn bộ giao dịch tích lũy và đổi điểm" (Track all accumulated and redeemed point transactions).
Current Total Points: Orange rectangular box displaying "Tổng điểm hiện tại" (Current total points) and the student's current point balance.
Filter Tabs: Three tabs for filtering transactions: "Tất cả" (All), "Nhận điểm" (Points Received), "Đổi điểm" (Points Redeemed).
Transaction List: List of point transactions displayed with transaction icon, description, date, and point amount (positive for received, negative for redeemed).
Data processing:
Student navigates to "Đổi thưởng" (Redeem Rewards) page and clicks "Lịch sử điểm" (Point History) card or link.
System fetches all point transactions for the student from database.
System displays transactions in chronological order (newest first) with transaction details.
Student can filter transactions by selecting filter tabs: "Tất cả" (All), "Nhận điểm" (Points Received), or "Đổi điểm" (Points Redeemed).
System filters and displays transactions based on selected filter.
Function details:
Data: TransactionId, TransactionType (Received/Redeemed), Description, PointAmount, TransactionDate, CurrentTotalPoints.
Validation: User must be authenticated. Student must exist.
Authentication: JWT token required.
Business rules: Only students can view their own point history. Transactions are displayed in chronological order (newest first). Filtering is performed on client side or server side. Point amounts are displayed as positive for received points and negative for redeemed points.
Normal case: Student navigates to point history page => System fetches transactions => Transactions displayed in list => Student can filter by type => Student can view all transaction details.
Abnormal case: User is not authenticated => Redirect to login page. No transactions found => Display empty state message. Network error => Display error message "Có lỗi xảy ra khi tải lịch sử điểm" (Error occurred while loading point history).

