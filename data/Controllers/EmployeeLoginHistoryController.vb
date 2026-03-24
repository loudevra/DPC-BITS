Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models

Namespace DPC.Data.Controllers
    Public Class EmployeeLoginHistoryController
        Public Shared Sub AddLoginHistory(EmployeeID As String, EmployeeName As String, EmployeeEmail As String, LoggedInTime As DateTime)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim LoggedInTimeForEmployeeQuery As String = "INSERT INTO employeeloginhistory (employeeID, employeeName, employeeEmail, loggedInTime) VALUES (@employeeID, @employeeName, @employeeEmail, @loggedintime)"

                    Using LogTimeCmd As New MySqlCommand(LoggedInTimeForEmployeeQuery, conn)
                        LogTimeCmd.Parameters.AddWithValue("@employeeID", EmployeeID)
                        LogTimeCmd.Parameters.AddWithValue("@employeeName", EmployeeName)
                        LogTimeCmd.Parameters.AddWithValue("@employeeEmail", EmployeeEmail)
                        LogTimeCmd.Parameters.AddWithValue("@loggedintime", LoggedInTime)

                        LogTimeCmd.ExecuteNonQuery()
                        'Console.WriteLine("Login history inserted successfully.")

                        Dim cacheLoginHistory As Integer = LogTimeCmd.LastInsertedId
                        CacheLogInHistoryID = cacheLoginHistory

                        Console.WriteLine($"Employee ID = {CacheLogInHistoryID}")
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error inserting login history: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Public Shared Sub AddLogOutHistory(loginHistoryID As String)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim LoggedOutTimeForEmployeeQuery As String = "UPDATE employeeloginhistory SET loggedOutTime = @loggedOutTime WHERE loginHistoryID = @loginHistoryID"

                    Using LogTimeCmd As New MySqlCommand(LoggedOutTimeForEmployeeQuery, conn)
                        LogTimeCmd.Parameters.AddWithValue("@loggedOutTime", DateTime.Now())
                        LogTimeCmd.Parameters.AddWithValue("@loginHistoryID", loginHistoryID)

                        LogTimeCmd.ExecuteNonQuery()
                        'Console.WriteLine("Login history inserted successfully.")
                        DeleteAuthUserStatus(CacheLogInHistoryID)
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error inserting login history: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Public Shared Sub AddAuthUserStatus(loginHistoryID As String, employeeID As Long, employeeName As String)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim AuthUserStatusQuery As String = "INSERT INTO auth_users (id, employee_id, full_name) VALUES (@LoginHistoryID, @EmployeeID, @EmployeeName)"
                    Using LogTimeCmd As New MySqlCommand(AuthUserStatusQuery, conn)
                        LogTimeCmd.Parameters.AddWithValue("@loginHistoryID", loginHistoryID)
                        LogTimeCmd.Parameters.AddWithValue("@EmployeeID", employeeID)
                        LogTimeCmd.Parameters.AddWithValue("@EmployeeName", employeeName)

                        LogTimeCmd.ExecuteNonQuery()
                        'Console.WriteLine("Login history inserted successfully.")
                    End Using
                End Using
            Catch ex As Exception

            End Try
        End Sub

        Public Shared Sub DeleteAuthUserStatus(loginHistoryID As String)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim AuthUserStatusQuery As String = "DELETE FROM auth_users WHERE id = @LoginHistoryID"
                    Using LogTimeCmd As New MySqlCommand(AuthUserStatusQuery, conn)
                        LogTimeCmd.Parameters.AddWithValue("@LoginHistoryID", loginHistoryID)
                        LogTimeCmd.ExecuteNonQuery()
                        'Console.WriteLine("Login history inserted successfully.")
                    End Using
                End Using
            Catch ex As Exception

            End Try
        End Sub

        Public Shared Function GetUnreadNotifications(employeeID As String) As List(Of String)
            Dim notifications As New List(Of String)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT employeeName, loggedInTime FROM employeeloginhistory 
                                   WHERE employeeID = @employeeID AND is_read = 0 
                                   ORDER BY loggedInTime DESC"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@employeeID", employeeID)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim name As String = reader("employeeName").ToString()
                                Dim time As DateTime = Convert.ToDateTime(reader("loggedInTime"))
                                notifications.Add($"{name} logged in on {time.ToString("MMM dd, yyyy hh:mm tt")}")
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error fetching notifications: " & ex.Message)
            End Try
            Return notifications
        End Function

        Public Shared Sub MarkAllAsRead(employeeID As String)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "UPDATE employeeloginhistory SET is_read = 1 
                                   WHERE employeeID = @employeeID AND is_read = 0"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@employeeID", employeeID)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error marking notifications as read: " & ex.Message)
            End Try
        End Sub

        Public Shared Function GetUnreadCount(employeeID As Long) As Integer
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT COUNT(*) FROM employeeloginhistory 
                                   WHERE employeeID = @employeeID AND is_read = 0"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@employeeID", employeeID)
                        Return Convert.ToInt32(cmd.ExecuteScalar())
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("ERROR: " & ex.Message)
                Return 0
            End Try
        End Function
    End Class
End Namespace