# View Shop Items by Student

Function trigger:
Navigation path: Main navigation → "Đổi thưởng" (Redeem Rewards) menu item.
Timing demand: On demand (whenever a student wants to view available rewards in the shop).
Function description:
Actors/Roles: Student.
Purpose: Allow students to view all available reward items in the shop that can be redeemed using accumulated star points.
Screen Layout:
Figure X: View Shop Items Screen
The shop interface consists of:
Header Section: Title "Đổi thưởng" (Redeem Rewards) with subtitle and current points display.
Summary Cards: Point History, Member Rank, and Redeemed this month cards.
Category Filter Tabs: "Tất cả" (All), "Voucher", "Sách" (Books), "Điện tử" (Electronics).
Rewards Grid: Grid layout displaying reward items with image, name, description, point cost, stock availability, and "Đổi ngay" (Redeem now) button.
Data processing:
Student navigates to "Đổi thưởng" (Redeem Rewards) page.
System fetches all available rewards from database.
System displays rewards in grid layout with reward image, name, point cost, and available stock.
Student can filter rewards by category using category tabs.
For each reward, system checks if student has enough points and if reward is in stock, then enables/disables "Đổi ngay" button accordingly.
Function details:
Data: RewardId, Name, PointCost, Stock, Category, ImageUrl, Claimed.
Validation: User must be authenticated. Rewards must exist in database.
Authentication: JWT token required.
Business rules: All students can view shop items. Stock availability is calculated as (Stock - Claimed). Rewards can be filtered by category. Button state depends on student's current points vs reward cost and available stock.
Normal case: Student navigates to shop page => System fetches rewards => Rewards displayed in grid => Student can filter by category => Student can see point cost and stock for each reward.
Abnormal case: User is not authenticated => Redirect to login page. No rewards available => Display empty state message. Network error => Display error message.
