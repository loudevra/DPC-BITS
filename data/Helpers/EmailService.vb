Imports System.IO
Imports MimeKit
Imports MailKit.Net.Smtp
Imports MailKit.Security

Namespace DPC.Data.Helpers
    Public Class EmailService
        ' Load SMTP Credentials from .env
        Private Shared ReadOnly SmtpServer As String = EnvLoader.GetEnv("EMAIL_HOST")
        Private Shared ReadOnly SmtpPort As Integer = Convert.ToInt32(EnvLoader.GetEnv("EMAIL_PORT"))
        Private Shared ReadOnly SenderEmail As String = EnvLoader.GetEnv("EMAIL_USER")
        Private Shared ReadOnly SenderPassword As String = EnvLoader.GetEnv("EMAIL_PASS")

        ' Debugging: Print SMTP settings
        Public Shared Sub DebugSMTPSettings()
            Console.WriteLine("📌 SMTP Debug Info:")
            Console.WriteLine("SMTP Server: " & SmtpServer)
            Console.WriteLine("SMTP Port: " & SmtpPort)
            Console.WriteLine("Sender Email: " & SenderEmail)
            Console.WriteLine("Sender Password: " & If(String.IsNullOrEmpty(SenderPassword), "❌ NOT SET", "✅ SET (Hidden)"))
        End Sub

        ' Function to get email template path
        Private Shared Function GetEmailTemplatePath(filename As String) As String
            Dim projectRoot As String = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName
            Return Path.Combine(projectRoot, "DPC", "Assets", "mails", filename)
        End Function

        ' Send Verification Code Email
        Public Shared Sub SendVerificationCode(recipientEmail As String, username As String, verificationCode As String)
            Try
                DebugSMTPSettings()

                ' Get email template path
                Dim templatePath As String = GetEmailTemplatePath("password_reset_code.html")
                If Not File.Exists(templatePath) Then
                    Console.WriteLine("🚨 ERROR: Email template not found: " & templatePath)
                    Exit Sub
                End If

                ' Read and customize the HTML template
                Dim htmlBody As String = File.ReadAllText(templatePath)
                htmlBody = htmlBody.Replace("{{USERNAME}}", username)
                htmlBody = htmlBody.Replace("{{VERIFICATION_CODE}}", verificationCode)

                ' Create Email Message
                Dim message As New MimeMessage()
                message.From.Add(New MailboxAddress("Dream PC Support", SenderEmail))
                message.To.Add(New MailboxAddress(username, recipientEmail))
                message.Subject = "Your Password Reset Code"

                ' Set Email Body
                Dim bodyBuilder As New BodyBuilder With {
                    .HtmlBody = htmlBody
                }
                message.Body = bodyBuilder.ToMessageBody()

                ' Send email using MailKit
                Using smtpClient As New SmtpClient()
                    smtpClient.Connect(SmtpServer, SmtpPort, SecureSocketOptions.StartTls)
                    smtpClient.Authenticate(SenderEmail, SenderPassword)
                    smtpClient.Send(message)
                    smtpClient.Disconnect(True)
                End Using

                Console.WriteLine("✅ Password reset code email sent successfully to " & recipientEmail)

            Catch ex As Exception
                Console.WriteLine("🚨 Email Sending Error: " & ex.Message)
            End Try
        End Sub

        ' Send Password Reset Confirmation Email
        Public Shared Function SendPasswordResetConfirmation(toEmail As String, username As String, resetDate As DateTime) As Boolean
            Try
                DebugSMTPSettings()

                ' Get email template path
                Dim templatePath As String = GetEmailTemplatePath("password_reset_success.html")
                If Not File.Exists(templatePath) Then
                    Console.WriteLine("🚨 ERROR: Email template not found: " & templatePath)
                    Return False
                End If

                ' Read and customize the HTML template
                Dim htmlBody As String = File.ReadAllText(templatePath)
                htmlBody = htmlBody.Replace("{{USERNAME}}", username)
                htmlBody = htmlBody.Replace("{{RESET_DATE}}", resetDate.ToString("dddd, MMMM dd yyyy, hh:mm:ss tt"))

                ' Create Email Message
                Dim message As New MimeMessage()
                message.From.Add(New MailboxAddress("Dream PC Support", SenderEmail))
                message.To.Add(New MailboxAddress(username, toEmail))
                message.Subject = "Your Password Has Been Changed"

                ' Set Email Body
                Dim bodyBuilder As New BodyBuilder With {
                    .HtmlBody = htmlBody
                }
                message.Body = bodyBuilder.ToMessageBody()

                ' Send email using MailKit
                Using smtpClient As New SmtpClient()
                    smtpClient.Connect(SmtpServer, SmtpPort, SecureSocketOptions.StartTls)
                    smtpClient.Authenticate(SenderEmail, SenderPassword)
                    smtpClient.Send(message)
                    smtpClient.Disconnect(True)
                End Using

                Console.WriteLine("✅ Password reset confirmation email sent successfully to " & toEmail)
                Return True

            Catch ex As Exception
                Console.WriteLine("🚨 Email Sending Error: " & ex.Message)
                Return False
            End Try
        End Function
    End Class
End Namespace
