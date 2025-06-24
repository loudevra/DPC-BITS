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

        Public Shared Sub AddLogOutHistory(loginHistoryID As Integer)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim LoggedOutTimeForEmployeeQuery As String = "UPDATE employeeloginhistory SET loggedOutTime = @loggedOutTime WHERE loginHistoryID = @loginHistoryID"

                    Using LogTimeCmd As New MySqlCommand(LoggedOutTimeForEmployeeQuery, conn)
                        LogTimeCmd.Parameters.AddWithValue("@loggedOutTime", DateTime.Now())
                        LogTimeCmd.Parameters.AddWithValue("@loginHistoryID", loginHistoryID)

                        LogTimeCmd.ExecuteNonQuery()
                        'Console.WriteLine("Login history inserted successfully.")
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error inserting login history: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
    End Class
End Namespace