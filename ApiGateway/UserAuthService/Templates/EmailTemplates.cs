namespace UserAuthService.Templates
{
    public enum EmailType
    {
        Welcome = 0,
        OTP = 1,
        Notification = 2,
        ApplicationStatus = 3,
        LoginDetail = 4,
    }
    public class EmailTemplate
    {
        public string GetHTMLTemplate(EmailType emailType)
        {
            switch (emailType)
            {
                case EmailType.Welcome:
                    return WelcomeEmail();
                case EmailType.OTP:
                    return OTPMessage();
                case EmailType.Notification:
                    return NotificationMessage();
                case EmailType.LoginDetail:
                    return LoginDetailsEmail();
                default:
                    return "";
            }


        }
        private string WelcomeEmail()
        {
            return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome to Engage!</title>
</head>
<body style=""width:100%;height:100%;padding:0;margin:0;background-color:#f7f9fc;font-family:'Segoe UI',Arial,sans-serif;"">
    <div style=""width:100%;max-width:600px;margin:0 auto;background-color:#ffffff;padding:40px;box-shadow:0 4px 6px rgba(0,0,0,0.1);border-radius:8px;"">
        <div style=""text-align:center;margin-bottom:30px;"">
            <img src=""[LOGO_URL]"" alt=""Engage Logo"" style=""width:180px;"">
        </div>
        <div style=""padding:20px;"">
            <h1 style=""font-size:28px;color:#2d3748;margin-bottom:24px;text-align:center;"">Welcome to Your Learning Journey! 🎓</h1>
            <p style=""font-size:16px;color:#4a5568;line-height:1.6;margin-bottom:20px;"">We're thrilled to have you join our community of learners and educators. At Engage, we believe in making education accessible, engaging, and empowering.</p>
            <div style=""background-color:#f8fafc;padding:24px;border-radius:6px;margin:30px 0;"">
                <h2 style=""font-size:20px;color:#2d3748;margin-bottom:16px;"">What's Next?</h2>
                <ul style=""color:#4a5568;line-height:1.8;padding-left:20px;"">
                    <li>Complete your profile</li>
                    <li>Explore available courses</li>
                    <li>Connect with other learners</li>
                    <li>Track your progress</li>
                </ul>
            </div>
            <p style=""font-size:16px;color:#4a5568;line-height:1.6;"">Need help getting started? Our support team is here for you!</p>
            <div style=""text-align:center;margin-top:30px;"">
                <a href=""[PLATFORM_URL]"" style=""display:inline-block;background-color:#4f46e5;color:#ffffff;padding:12px 24px;text-decoration:none;border-radius:6px;font-weight:600;"">Get Started Now</a>
            </div>
        </div>
        <div style=""margin-top:40px;padding-top:20px;border-top:1px solid #e2e8f0;text-align:center;font-size:14px;color:#718096;"">
            <p>Questions? Contact us at <a href=""mailto:support@engage.com"" style=""color:#4f46e5;text-decoration:none;"">support@engage.com</a></p>
            <p style=""margin:8px 0;"">© 2024 Engage. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string OTPMessage()
        {
            return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Your Engage Verification Code</title>
</head>
<body style=""width:100%;height:100%;padding:0;margin:0;background-color:#f7f9fc;font-family:'Segoe UI',Arial,sans-serif;"">
    <div style=""width:100%;max-width:600px;margin:0 auto;background-color:#ffffff;padding:40px;box-shadow:0 4px 6px rgba(0,0,0,0.1);border-radius:8px;"">
        <div style=""text-align:center;margin-bottom:30px;"">
            <img src=""[LOGO_URL]"" alt=""Engage Logo"" style=""width:180px;"">
        </div>
        <div style=""padding:20px;"">
            <h1 style=""font-size:28px;color:#2d3748;margin-bottom:24px;text-align:center;"">Verify Your Account</h1>
            <p style=""font-size:16px;color:#4a5568;line-height:1.6;margin-bottom:20px;text-align:center;"">To ensure the security of your account, please use the verification code below:</p>
            
            <div style=""background-color:#f8fafc;padding:24px;border-radius:6px;margin:30px 0;text-align:center;"">
                <p style=""font-size:32px;font-weight:bold;color:#4f46e5;letter-spacing:4px;margin:0;"">[Your OTP Here]</p>
                <p style=""font-size:14px;color:#718096;margin-top:12px;"">This code will expire in 10 minutes</p>
            </div>

            <div style=""background-color:#fef2f2;padding:20px;border-radius:6px;margin:20px 0;"">
                <p style=""color:#991b1b;margin:0;font-size:14px;"">🔐 Never share this code with anyone. Our team will never ask for your verification code.</p>
            </div>

            <p style=""font-size:16px;color:#4a5568;line-height:1.6;text-align:center;"">If you didn't request this code, please ignore this email or contact our support team if you have concerns.</p>
        </div>
        <div style=""margin-top:40px;padding-top:20px;border-top:1px solid #e2e8f0;text-align:center;font-size:14px;color:#718096;"">
            <p>Need help? Contact us at <a href=""mailto:support@engage.com"" style=""color:#4f46e5;text-decoration:none;"">support@engage.com</a></p>
            <p style=""margin:8px 0;"">© 2024 Engage. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string NotificationMessage()
        {
            return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Engage Notification</title>
</head>
<body style=""width:100%;height:100%;padding:0;margin:0;background-color:#f7f9fc;font-family:'Segoe UI',Arial,sans-serif;"">
    <div style=""width:100%;max-width:600px;margin:0 auto;background-color:#ffffff;padding:40px;box-shadow:0 4px 6px rgba(0,0,0,0.1);border-radius:8px;"">
        <div style=""text-align:center;margin-bottom:30px;"">
            <img src=""[LOGO_URL]"" alt=""Engage Logo"" style=""width:180px;"">
        </div>
        <div style=""padding:20px;"">
            <h1 style=""font-size:24px;color:#2d3748;margin-bottom:24px;"">Important Update</h1>
            
            <div style=""background-color:#f8fafc;padding:24px;border-radius:6px;margin:20px 0;"">
                <p style=""font-size:16px;color:#4a5568;line-height:1.6;margin:0;"">[Content]</p>
            </div>

            <div style=""text-align:center;margin-top:30px;"">
                <a href=""[ACTION_URL]"" style=""display:inline-block;background-color:#4f46e5;color:#ffffff;padding:12px 24px;text-decoration:none;border-radius:6px;font-weight:600;"">View Details</a>
            </div>

            <p style=""font-size:14px;color:#718096;line-height:1.6;margin-top:20px;text-align:center;"">This is an automated message. Please do not reply to this email.</p>
        </div>
        <div style=""margin-top:40px;padding-top:20px;border-top:1px solid #e2e8f0;text-align:center;font-size:14px;color:#718096;"">
            <p>Questions? Contact us at <a href=""mailto:support@engage.com"" style=""color:#4f46e5;text-decoration:none;"">support@engage.com</a></p>
            <p style=""margin:8px 0;"">© 2024 Engage. All rights reserved.</p>
            <p style=""margin:8px 0;font-size:12px;"">
                <a href=""[UNSUBSCRIBE_URL]"" style=""color:#718096;text-decoration:underline;"">Update notification preferences</a>
            </p>
        </div>
    </div>
</body>
</html>";
        }

        private string LoginDetailsEmail()
        {
            return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Your Engage Account Details</title>
</head>
<body style=""width:100%;height:100%;padding:0;margin:0;background-color:#f7f9fc;font-family:'Segoe UI',Arial,sans-serif;"">
    <div style=""width:100%;max-width:600px;margin:0 auto;background-color:#ffffff;padding:40px;box-shadow:0 4px 6px rgba(0,0,0,0.1);border-radius:8px;"">
        <div style=""padding:20px;"">
            <h1 style=""font-size:24px;color:#2d3748;margin-bottom:24px;"">Welcome to Engage, [Name]!</h1>
            <p style=""font-size:16px;color:#4a5568;line-height:1.6;"">Your account has been created successfully. Here are your login credentials:</p>
            
            <div style=""background-color:#f8fafc;padding:24px;border-radius:6px;margin:30px 0;"">
                <p style=""margin:8px 0;font-size:16px;color:#4a5568;""><strong>Role:</strong> [UserRole]</p>
                <p style=""margin:8px 0;font-size:16px;color:#4a5568;""><strong>Email:</strong> [UserEmail]</p>
                <p style=""margin:8px 0;font-size:16px;color:#4a5568;""><strong>Temporary Password:</strong> [UserPassword]</p>
            </div>

            <div style=""background-color:#fef2f2;padding:20px;border-radius:6px;margin:20px 0;"">
                <p style=""color:#991b1b;margin:0;font-size:14px;"">🔐 For security reasons, please change your password upon first login.</p>
            </div>

            <div style=""text-align:center;margin-top:30px;"">
                <a href=""[LOGIN_URL]"" style=""display:inline-block;background-color:#4f46e5;color:#ffffff;padding:12px 24px;text-decoration:none;border-radius:6px;font-weight:600;"">Login to Your Account</a>
            </div>

            <p style=""font-size:16px;color:#4a5568;line-height:1.6;margin-top:30px;"">If you have any questions or need assistance, our support team is here to help!</p>
        </div>
        <div style=""margin-top:40px;padding-top:20px;border-top:1px solid #e2e8f0;text-align:center;font-size:14px;color:#718096;"">
            <p>Questions? Contact us at <a href=""mailto:support@engage.com"" style=""color:#4f46e5;text-decoration:none;"">support@engage.com</a></p>
            <p style=""margin:8px 0;"">© 2024 Engage. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

    }
}
