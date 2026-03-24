Imports System.Windows
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Helpers ' Ensure this matches your namespace for DB connection

Namespace DPC.Components.ConfirmationModals
    Public Class NotificationModal
        Inherits Window

        Public Class NotificationItem
            Public Property Title As String
            Public Property Message As String
            Public Property CreatedAt As String
        End Class

        Public Sub New(employeeID As String)
            InitializeComponent()

            ' Load role and notifications
            LoadUserRole(employeeID)
            LoadNotifications()
        End Sub

        Private Sub LoadUserRole(empID As String)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String = "SELECT ur.RoleName FROM employee e " &
                                          "INNER JOIN userroles ur ON e.UserRoleID = ur.RoleID " &
                                          "WHERE e.EmployeeID = @EmpID"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@EmpID", empID)

                        Dim result = cmd.ExecuteScalar()

                        If result IsNot Nothing Then
                            txtUserRole.Text = "Role: " & result.ToString()
                        Else
                            txtUserRole.Text = "Role: Unknown"
                        End If
                    End Using
                End Using
            Catch ex As Exception
                txtUserRole.Text = "Role: Error loading role"
            End Try
        End Sub

        Private Sub LoadNotifications()
            Dim notifications As New List(Of NotificationItem)()

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Fetch the 15 most recent logins by ALL users (including yourself), joining tables to get the RoleName
                    Dim query As String = "SELECT elh.employeeName, elh.loggedInTime, ur.RoleName " &
                                          "FROM employeeloginhistory elh " &
                                          "INNER JOIN employee e ON elh.employeeID = e.EmployeeID " &
                                          "INNER JOIN userroles ur ON e.UserRoleID = ur.RoleID " &
                                          "ORDER BY elh.loggedInTime DESC LIMIT 15"

                    Using cmd As New MySqlCommand(query, conn)

                        Dim reader = cmd.ExecuteReader()
                        While reader.Read()
                            ' Note: Add .AddHours(8) at the end of Convert.ToDateTime if your DB is in UTC and you need PHT
                            Dim dateValue As DateTime = Convert.ToDateTime(reader("loggedInTime"))
                            Dim empName As String = reader("employeeName").ToString()
                            Dim roleName As String = reader("RoleName").ToString()

                            notifications.Add(New NotificationItem With {
                                .Title = "Team Login",
                                .Message = $"{empName} ({roleName}) has logged into the system.",
                                .CreatedAt = dateValue.ToString("MMM dd, yyyy - hh:mm tt")
                            })
                        End While
                    End Using
                End Using
            Catch ex As Exception
                ' Optional: Add error logging here
            End Try

            NotificationList.ItemsSource = notifications
        End Sub

        Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
            Me.Close()
        End Sub
    End Class
End Namespace