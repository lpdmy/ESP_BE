
namespace EduShpere.Shared.Constants
{
    public static class EmailTemplate
    {
        public static string GetResetLink(string userEmail)
        {
            return $"https://he-thong-truongxyz.edu.vn/reset-password?email={userEmail}";
        }

        public static string GetInvitationEmail(string userFullName, string userEmail)
        {
            var resetLink = GetResetLink(userEmail);
            return $@"
<html>
  <head>
    <style>
      body {{
        font-family: Arial, sans-serif;
        background-color: #f4f4f4;
        margin: 0;
        padding: 0;
      }}
      .container {{
        background-color: #ffffff;
        max-width: 600px;
        margin: 30px auto;
        padding: 20px;
        border-radius: 8px;
        box-shadow: 0 2px 6px rgba(0,0,0,0.1);
      }}
      h1 {{
        color: #2c3e50;
      }}
      p {{
        font-size: 15px;
        color: #555555;
        line-height: 1.5;
      }}
      .btn {{
        display: inline-block;
        margin-top: 20px;
        padding: 12px 20px;
        background-color: #0078d7;
        color: #ffffff !important;
        text-decoration: none;
        border-radius: 5px;
        font-weight: bold;
      }}
      .footer {{
        margin-top: 30px;
        font-size: 12px;
        color: #999999;
        text-align: center;
      }}
    </style>
  </head>
  <body>
  <div class='container'>
    <h1>Tài khoản truy cập hệ thống Trường XYZ</h1>
    <p>Xin chào {userFullName},</p>
    <p>Nhà trường đã cấp cho bạn một tài khoản để đăng nhập vào hệ thống trực tuyến.</p>
    <p>Bạn có thể truy cập bằng email này và mật khẩu mặc định do nhà trường cung cấp.</p>
    <p><strong>Tên đăng nhập:</strong> {userEmail}</p>
    <p>Để bắt đầu, vui lòng bấm vào liên kết sau:</p>
    <a class='btn' href='{resetLink}'>Đổi mật khẩu và đăng nhập hệ thống</a>
    <div class='footer'>
      © 2025 Trường XYZ. Mọi quyền được bảo lưu.
    </div>
  </div>
</body>
</html>";
        }

    }
}

