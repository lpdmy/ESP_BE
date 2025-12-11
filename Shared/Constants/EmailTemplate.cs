namespace EduShpere.Shared.Constants
{
    public static class EmailTemplate
    {
        public static string GetInvitationEmail(string userFullName, string resetLink)
        {
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
    <h1>Tài khoản truy cập hệ thống EduSphere Trường THPT FPT</h1>
    <p>Xin chào {userFullName},</p>
    <p>Nhà trường đã cấp cho bạn một tài khoản để đăng nhập vào hệ thống trực tuyến.</p>
    <p>Bạn có thể truy cập bằng email này và mật khẩu mặc định do nhà trường cung cấp.</p>
    <p>Để bắt đầu, vui lòng bấm vào liên kết sau:</p>
    <a class='btn' href='{resetLink}'>Đổi mật khẩu và đăng nhập hệ thống</a>
    <div class='footer'>
      © 2025 Trường THPT FPT. Mọi quyền được bảo lưu.
    </div>
  </div>
</body>
</html>";
        }

        public static string GetResetPasswordEmail(string userFullName, string resetLink)
        {
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
        background-color: #e67e22;
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
    <h1>Yêu cầu đặt lại mật khẩu - EduSphere</h1>
    <p>Xin chào {userFullName},</p>
    <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
    <p>Nếu bạn thực hiện yêu cầu này, vui lòng bấm vào liên kết sau để đặt lại mật khẩu:</p>
    <a class='btn' href='{resetLink}'>Đặt lại mật khẩu</a>
    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này. Tài khoản của bạn sẽ vẫn an toàn.</p>
    <div class='footer'>
      © 2025 Trường THPT FPT. Mọi quyền được bảo lưu.
    </div>
  </div>
</body>
</html>";
        }
    }
}
