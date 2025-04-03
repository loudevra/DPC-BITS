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

        ' Function to get email template path dynamically
        Private Shared Function GetEmailTemplatePath(filename As String) As String
            ' Get the base directory of the running application
            Dim basePath As String = AppDomain.CurrentDomain.BaseDirectory

            ' Check multiple possible locations
            Dim possiblePaths As String() = {
                 Path.Combine(basePath, "Assets", "mails", filename), ' Normal path
                 Path.Combine(basePath, "..", "..", "Assets", "mails", filename), ' For bin/debug
                 Path.Combine(basePath, "..", "..", "..", "Assets", "mails", filename) ' For deeper structures
            }

            ' Return the first valid path found
            For Each path In possiblePaths
                If File.Exists(path) Then
                    Return path
                End If
            Next

            ' Log error if not found
            Console.WriteLine("🚨 ERROR: Email template not found: " & filename)
            Return String.Empty
        End Function



        ' Send Verification Code Email
        Public Shared Sub SendVerificationCode(recipientEmail As String, username As String, verificationCode As String)
            Try
                ' Get email template path
                Dim templatePath As String = GetEmailTemplatePath("password_reset_code.html")
                If String.IsNullOrEmpty(templatePath) Then Exit Sub

                ' Read and customize the HTML template
                Dim htmlBody As String = File.ReadAllText(templatePath)
                htmlBody = htmlBody.Replace("{{USERNAME}}", username).Replace("{{VERIFICATION_CODE}}", verificationCode)

                ' Create Email Message
                Dim message As New MimeMessage()
                message.From.Add(New MailboxAddress("Dream PC Support", SenderEmail))
                message.To.Add(New MailboxAddress(username, recipientEmail))
                message.Subject = "Your Password Reset Code"
                message.Body = New BodyBuilder With {.HtmlBody = htmlBody}.ToMessageBody()

                ' Send email using MailKit
                Using smtpClient As New SmtpClient()
                    smtpClient.Connect(SmtpServer, SmtpPort, SecureSocketOptions.StartTls)
                    smtpClient.Authenticate(SenderEmail, SenderPassword)
                    smtpClient.Send(message)
                    smtpClient.Disconnect(True)
                End Using
            Catch ex As Exception
            End Try
        End Sub

        ' Send Password Reset Confirmation Email
        Public Shared Function SendPasswordResetConfirmation(toEmail As String, username As String, resetDate As DateTime) As Boolean
            Try
                ' Get email template path
                Dim templatePath As String = GetEmailTemplatePath("password_reset_success.html")
                If String.IsNullOrEmpty(templatePath) Then Return False

                ' Read and customize the HTML template
                Dim htmlBody As String = File.ReadAllText(templatePath)
                htmlBody = htmlBody.Replace("{{USERNAME}}", username).Replace("{{RESET_DATE}}", resetDate.ToString("dddd, MMMM dd yyyy, hh:mm:ss tt"))

                ' Create Email Message
                Dim message As New MimeMessage()
                message.From.Add(New MailboxAddress("Dream PC Support", SenderEmail))
                message.To.Add(New MailboxAddress(username, toEmail))
                message.Subject = "Your Password Has Been Changed"
                message.Body = New BodyBuilder With {.HtmlBody = htmlBody}.ToMessageBody()

                ' Send email using MailKit
                Using smtpClient As New SmtpClient()
                    smtpClient.Connect(SmtpServer, SmtpPort, SecureSocketOptions.StartTls)
                    smtpClient.Authenticate(SenderEmail, SenderPassword)
                    smtpClient.Send(message)
                    smtpClient.Disconnect(True)
                End Using
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function
    End Class
End Namespace
