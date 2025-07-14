Imports System
Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports System.Security.Cryptography
Imports System.Text
Imports Microsoft.IdentityModel.Tokens
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Helpers ' Import PBKDF2Hasher
Imports Microsoft.AspNetCore.Cryptography.KeyDerivation
Imports System.Windows
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Components.Navigation
Imports System.Collections.ObjectModel ' Required for MessageBox.Show()

Namespace DPC.Data.Controllers
    Public Class AuthController
        ' Secret key for JWT (Change this for production, For Testing Purposes)
        Private Shared ReadOnly JWT_SECRET As String = EnvLoader.GetEnv("JWT_TOKEN")

        ' Token expiration settings
        Private Const ACCESS_TOKEN_EXPIRY As Integer = 15 ' Minutes
        Private Const REFRESH_TOKEN_EXPIRY As Integer = 7 * 24 * 60 ' Minutes (7 days)

        ' Sign-in function
        ' Adding an employee name and employee email to render the value when loggedin successfully in sidebar
        ' Adding an log whenever the user login it will add to the logs
        Public Shared Function SignIn(username As String, password As String) As (String, String)
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim query As String = "SELECT EmployeeID, Password, UserRoleID, Name, Email FROM employee WHERE Username = @Username"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@Username", username)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            If reader.Read() Then
                                Dim storedHashedPassword As String = reader("Password").ToString()
                                Dim employeeID As String = reader("EmployeeID").ToString()
                                Dim roleID As String = reader("UserRoleID").ToString()
                                Dim Name As String = reader("Name").ToString()
                                Dim Email As String = reader("Email").ToString()

                                ' Verify password using PBKDF2Hasher
                                If PBKDF2Hasher.VerifyPassword(password, storedHashedPassword) Then
                                    ' Pass the value to the object
                                    Dim UserLogs As New Sidebar()

                                    CacheOnLoggedInEmail = Email
                                    CacheOnLoggedInName = Name
                                    CacheOnEmployeeID = employeeID

                                    Dim userRole As String = GetUserRole(roleID)
                                    Dim accessToken As String = GenerateJwtToken(employeeID, username, userRole, ACCESS_TOKEN_EXPIRY)
                                    Dim refreshToken As String = GenerateJwtToken(employeeID, username, userRole, REFRESH_TOKEN_EXPIRY, True)

                                    ' Store refresh token in the database
                                    StoreRefreshToken(employeeID, refreshToken)

                                    Return (accessToken, refreshToken)
                                End If
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error during authentication: " & ex.Message, "Authentication Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
            Return (Nothing, Nothing)
        End Function

        ' Generate JWT token
        Private Shared Function GenerateJwtToken(userID As String, username As String, role As String, expiryMinutes As Integer, Optional isRefresh As Boolean = False) As String
            Dim key As New SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT_SECRET))
            Dim credentials As New SigningCredentials(key, SecurityAlgorithms.HmacSha256)

            Dim claims As New List(Of Claim) From {
                New Claim(JwtRegisteredClaimNames.Sub, userID),
                New Claim(JwtRegisteredClaimNames.UniqueName, username),
                New Claim("role", role),
                New Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }

            If isRefresh Then
                claims.Add(New Claim("RefreshToken", "true"))
            End If

            Dim token As New JwtSecurityToken(
                issuer:="DPC",
                audience:="DPCUsers",
                claims:=claims,
                expires:=DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials:=credentials
            )

            Return New JwtSecurityTokenHandler().WriteToken(token)
        End Function

        ' Get user role
        Private Shared Function GetUserRole(roleID As String) As String
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim query As String = "SELECT RoleName FROM userroles WHERE RoleID = @RoleID"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@RoleID", roleID)
                        Dim result As Object = cmd.ExecuteScalar()
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            Return result.ToString()
                        End If
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error fetching user role: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
            Return "User"
        End Function

        ' Store refresh token in database
        Private Shared Sub StoreRefreshToken(userID As String, refreshToken As String)
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim query As String = "UPDATE employee SET RefreshToken = @RefreshToken WHERE EmployeeID = @EmployeeID"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@RefreshToken", refreshToken)
                        cmd.Parameters.AddWithValue("@EmployeeID", userID)
                        cmd.ExecuteNonQuery()
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error storing refresh token: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub

        ' Refresh access token
        Public Shared Function RefreshAccessToken(refreshToken As String) As String
            Dim handler As New JwtSecurityTokenHandler()
            Try
                Dim validationParameters As New TokenValidationParameters() With {
                    .ValidateIssuer = True,
                    .ValidateAudience = True,
                    .ValidIssuer = "DPC",
                    .ValidAudience = "DPCUsers",
                    .ValidateLifetime = True,
                    .ValidateIssuerSigningKey = True,
                    .IssuerSigningKey = New SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT_SECRET))
                }

                Dim principal As ClaimsPrincipal = handler.ValidateToken(refreshToken, validationParameters, Nothing)
                Dim userID As String = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                Dim username As String = principal.Identity.Name
                Dim role As String = principal.FindFirst("role")?.Value

                ' Generate new access token
                Return GenerateJwtToken(userID, username, role, ACCESS_TOKEN_EXPIRY)
            Catch ex As Exception
                Return Nothing
            End Try
        End Function
    End Class
End Namespace