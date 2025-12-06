# Redeem Rewards by Student
Function trigger:
Navigation path: "Đổi thưởng" (Redeem Rewards) page → Click "Đổi ngay" (Redeem now) button on a reward item.
Timing demand: On demand (whenever a student wants to redeem a reward using their accumulated points).
Function description:
Actors/Roles: Student.
Purpose: Allow students to redeem reward items from the shop using their accumulated star points.
Screen Layout:
Figure X: Redeem Rewards Screen
The redeem reward interface consists of a confirmation modal dialog displayed when user clicks "Đổi ngay":
Modal Dialog: White modal dialog titled "Xác nhận đổi thưởng" (Confirm Reward Redemption) centered on screen.
Reward Information: Reward image and name displayed in the modal.
Points Breakdown:
"Điểm cần thiết:" (Points required:) showing required points in red.
"Điểm hiện tại:" (Current points:) showing student's current points.
"Điểm còn lại:" (Remaining points:) showing remaining points after redemption in green.
Action Buttons: Cancel Button (Gray) with text "Hủy" (Cancel) and Confirm Button (Orange) with text "Xác nhận đổi" (Confirm Redemption).
Data processing:
Student navigates to "Đổi thưởng" (Redeem Rewards) page and clicks "Đổi ngay" button on a reward item.
System opens confirmation modal dialog displaying reward information and points breakdown.
Student reviews points breakdown and can click "Hủy" (Cancel) to close modal or click "Xác nhận đổi" (Confirm Redemption) to proceed.
If student confirms, system validates that student has enough points, reward is in stock, and student is authenticated.
If validation passes, system creates RewardRedemption record, deducts points from student's account, decreases reward stock, displays success message, and closes modal.
Function details:
Data: RewardId, StudentId, PointCost, CurrentPoints, RemainingPoints, Stock.
Validation: Student must be authenticated. Student must have enough points. Reward must be in stock. Reward must exist.
Authentication: JWT token required.
Business rules: Only students can redeem rewards. Points are deducted immediately upon redemption. Stock is decreased by 1 for each redemption. Student cannot redeem if they don't have enough points or if reward is out of stock.
Normal case: Student clicks redeem button => Confirmation modal opens => Student reviews points breakdown => Student confirms redemption => Points deducted => Reward stock decreased => Success message displayed => Modal closes.
Abnormal case: User is not authenticated => Redirect to login page. Not enough points => Display error message "Không đủ điểm" (Not enough points). Reward out of stock => Display error message "Hết hàng" (Out of stock). Network error => Display error message "Có lỗi xảy ra khi đổi thưởng" (Error occurred while redeeming reward).

