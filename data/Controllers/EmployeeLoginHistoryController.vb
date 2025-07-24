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
    End Class
End Namespace