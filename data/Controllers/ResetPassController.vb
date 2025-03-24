Imports System
Imports System.Data
Imports DPC.DPC.Data.Helpers
Imports MySql.Data.MySqlClient

Namespace DPC.Data.Controllers
    Public Class ResetPassController
        ' Check if the email exists in the employee table
        Public Shared Function DoesEmailExist(email As String) As Boolean
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT COUNT(*) FROM employee WHERE email = @Email"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@Email", email)
                        Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
                    End Using
                End Using
            Catch ex As Exception
                Console.WriteLine("Error checking email existence: " & ex.Message)
                Return False
            End Try
        End Function

        ' Generate and store a verification code
        Public Shared Function GenerateAndStoreVerificationCode(email As String) As String
            Dim verificationCode As String = New Random().Next(100000, 999999).ToString()
            Dim expiration As DateTime = DateTime.Now.AddMinutes(5) ' Code expires in 5 minutes

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "INSERT INTO password_resets (email, verification_code, expires_at) 
                                           VALUES (@Email, @Code, @Expires)
                                           ON DUPLICATE KEY UPDATE verification_code = @Code, expires_at = @Expires"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@Email", email)
                        cmd.Parameters.AddWithValue("@Code", verificationCode)
                        cmd.Parameters.AddWithValue("@Expires", expiration)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using

                ' Send Email with the verification code
                EmailService.SendVerificationCode(email, verificationCode)
                Return verificationCode
            Catch ex As Exception
                Console.WriteLine("Error storing verification code: " & ex.Message)
                Return String.Empty
            End Try
        End Function

        ' Validate the verification code
        Public Shared Function ValidateVerificationCode(email As String, code As String) As Boolean
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT COUNT(*) FROM password_resets 
                                           WHERE email = @Email AND verification_code = @Code AND expires_at > NOW()"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@Email", email)
                        cmd.Parameters.AddWithValue("@Code", code)
                        Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
                    End Using
                End Using
            Catch ex As Exception
                Console.WriteLine("Error validating verification code: " & ex.Message)
                Return False
            End Try
        End Function

        ' Reset the password
        Public Shared Function ResetPassword(email As String, newPassword As String) As Boolean
            Dim hashedPassword As String = PBKDF2Hasher.HashPassword(newPassword)

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "UPDATE employee SET password = @Password WHERE email = @Email"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@Password", hashedPassword)
                        cmd.Parameters.AddWithValue("@Email", email)

                        ' If password was updated, delete the verification code
                        If cmd.ExecuteNonQuery() > 0 Then
                            Dim deleteQuery As String = "DELETE FROM password_resets WHERE email = @Email"
                            Using deleteCmd As New MySqlCommand(deleteQuery, conn)
                                deleteCmd.Parameters.AddWithValue("@Email", email)
                                deleteCmd.ExecuteNonQuery()
                            End Using
                            Return True
                        End If
                    End Using
                End Using
            Catch ex As Exception
                Console.WriteLine("Error resetting password: " & ex.Message)
            End Try

            Return False
        End Function
    End Class
End Namespace
