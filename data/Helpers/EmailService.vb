Imports System.Net
Imports System.Net.Mail

Namespace DPC.Data.Helpers
    Public Class EmailService
        ' Load SMTP Credentials from .env
        Private Shared ReadOnly SmtpServer As String = EnvLoader.GetEnv("EMAIL_HOST")
        Private Shared ReadOnly SmtpPort As Integer = Convert.ToInt32(EnvLoader.GetEnv("EMAIL_PORT"))
        Private Shared ReadOnly SenderEmail As String = EnvLoader.GetEnv("EMAIL_USER")
        Private Shared ReadOnly SenderPassword As String = EnvLoader.GetEnv("EMAIL_PASS")

        ' Send verification email
        Public Shared Sub SendVerificationCode(recipientEmail As String, verificationCode As String)
            Try
                Using smtp As New SmtpClient(SmtpServer, SmtpPort)
                    smtp.Credentials = New NetworkCredential(SenderEmail, SenderPassword)
                    smtp.EnableSsl = True

                    Dim mail As New MailMessage With {
                        .From = New MailAddress(SenderEmail, "DPC Support")
                    }
                    mail.To.Add(recipientEmail)
                    mail.Subject = "Password Reset Verification Code"
                    mail.Body = $"Your password reset code is: {verificationCode}" & vbCrLf &
                                "This code is valid for 5 minutes. If you did not request this, please ignore this email."
                    mail.IsBodyHtml = False

                    smtp.Send(mail)
                End Using
            Catch ex As Exception
                Console.WriteLine("Email sending failed: " & ex.Message)
            End Try
        End Sub
    End Class
End Namespace
