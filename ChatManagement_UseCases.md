# Chat Management - Use Cases

## Danh sách các Use Case của tính năng Chat Management

### 1. **View Chat Rooms by User**
   - Xem danh sách tất cả các phòng chat của user
   - Hiển thị thông tin: Tên người tham gia, Avatar, Tin nhắn cuối cùng, Thời gian cập nhật, Số tin nhắn chưa đọc
   - Sắp xếp theo thời gian cập nhật (mới nhất trước)
   - Hỗ trợ real-time update khi có tin nhắn mới

### 2. **View Chat Messages by User**
   - Xem danh sách tin nhắn trong một phòng chat
   - Hỗ trợ pagination (limit = 50 messages mặc định)
   - Hiển thị thông tin: Nội dung, Người gửi, Thời gian, Loại tin nhắn (text, image, file)
   - Tự động scroll xuống tin nhắn mới nhất
   - Load thêm tin nhắn cũ khi scroll lên

### 3. **Create Chat Room by User**
   - Tạo phòng chat mới với một hoặc nhiều người
   - Kiểm tra phòng chat đã tồn tại (nếu 2 người) → trả về phòng cũ
   - Tự động tạo room ID
   - Thêm participants vào phòng chat
   - Navigate đến phòng chat sau khi tạo

### 4. **Send Message by User (via API)**
   - Gửi tin nhắn qua REST API
   - Lưu tin nhắn vào database (MongoDB)
   - Tự động set SenderId từ token
   - Tự động set Timestamp
   - Tạo notification cho người nhận
   - Trả về tin nhắn đã lưu

### 5. **Send Message by User (via SignalR)**
   - Gửi tin nhắn real-time qua SignalR Hub
   - Broadcast tin nhắn đến tất cả users trong room
   - Không lưu vào database (chỉ broadcast)
   - Hỗ trợ các loại tin nhắn: text, image, file

### 6. **Receive Message (Real-time)**
   - Nhận tin nhắn real-time qua SignalR
   - Tự động cập nhật UI khi có tin nhắn mới
   - Hiển thị tin nhắn trong danh sách
   - Tự động scroll đến tin nhắn mới
   - Cập nhật unread count

### 7. **Mark Messages as Read by User**
   - Đánh dấu tất cả tin nhắn trong phòng là đã đọc
   - Reset unread count về 0
   - Cập nhật trạng thái trong database
   - Cập nhật UI (xóa badge unread)

### 8. **Join Chat Room (SignalR)**
   - User tham gia vào SignalR group của phòng chat
   - Nhận tin nhắn real-time từ phòng đó
   - Cập nhật connection status

### 9. **Leave Chat Room (SignalR)**
   - User rời khỏi SignalR group của phòng chat
   - Ngừng nhận tin nhắn real-time từ phòng đó
   - Giải phóng connection

### 10. **View Unread Count**
   - Hiển thị số tin nhắn chưa đọc cho mỗi phòng chat
   - Cập nhật real-time khi có tin nhắn mới
   - Hiển thị badge trên danh sách phòng chat

### 11. **Search Chat Rooms**
   - Tìm kiếm phòng chat theo tên người tham gia
   - Filter phòng chat đang hiển thị
   - Real-time search khi user nhập

### 12. **View Chat User Info**
   - Xem thông tin người đang chat (tên, avatar)
   - Hiển thị trạng thái online/offline (nếu có)
   - Hiển thị trong header của chat detail

### 13. **Create Chat Room from Profile**
   - Tạo phòng chat từ trang profile của user khác
   - Click nút "Nhắn tin" trên profile
   - Tự động tạo room với 2 participants
   - Navigate đến phòng chat mới

### 14. **Notification on New Message**
   - Tạo notification khi nhận tin nhắn mới
   - Hiển thị thông báo: "X đã gửi cho bạn một tin nhắn"
   - Link đến phòng chat tương ứng
   - Hiển thị avatar người gửi

### 15. **Auto-scroll to Latest Message**
   - Tự động scroll xuống tin nhắn mới nhất khi:
     - Load messages lần đầu
     - Nhận tin nhắn mới
     - Gửi tin nhắn mới
   - Smooth scroll animation

### 16. **Load More Messages (Pagination)**
   - Load thêm tin nhắn cũ khi scroll lên đầu
   - Hỗ trợ infinite scroll
   - Giữ nguyên vị trí scroll sau khi load

### 17. **Emoji Picker**
   - Chọn và gửi emoji trong tin nhắn
   - Hiển thị emoji picker khi click icon
   - Insert emoji vào input field

### 18. **File Upload (Future)**
   - Upload file/ảnh trong chat
   - Gửi file như một loại tin nhắn
   - Hiển thị preview file

### 19. **Connection Management**
   - Đảm bảo SignalR connection được thiết lập
   - Tự động reconnect khi mất kết nối
   - Quản lý connection lifecycle

### 20. **Chat Room Ordering**
   - Sắp xếp phòng chat theo thời gian cập nhật
   - Phòng có tin nhắn mới nhất hiển thị đầu tiên
   - Tự động cập nhật thứ tự khi có tin nhắn mới




