Imports System.Collections.ObjectModel
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.IdentityModel.Protocols.WSTrust
Imports MySql.Data.MySqlClient

Namespace DPC.Data.Controllers.Misc
    Public Class ManageCashAdvanceController


        '        ' Retrieves all cash advance requests from the database
        Public Function GetRequests() As ObservableCollection(Of Models.CashAdvanceRetrieval)
            Dim requests As New ObservableCollection(Of Models.CashAdvanceRetrieval)()
            Dim query As String = "SELECT ca.caRef,
                                          ca.employeeID,
                                          e.Name, 
                                          ur.RoleName, 
                                          e.Department,
                                          ca.RequestTotal,
                                          ca.requestDate,
                                          ca.approvedBy
                                   FROM cashadvance ca 
                                   JOIN employee e ON ca.employeeID = e.EmployeeID 
                                   JOIN userroles ur ON e.UserRoleID = ur.RoleID"
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try

                    'Dim connStr As String = SplashScreen.GetDatabaseConnection.

                    conn.Open()
                    Dim cmd As New MySqlCommand(query, conn)
                    Dim reader As MySqlDataReader = cmd.ExecuteReader()

                    While reader.Read()
                        Dim status As String
                        Dim request As New Models.CashAdvanceRetrieval With {
                    .CashAdvanceID = reader("caRef").ToString(),
                    .EmployeeName = reader("Name").ToString(),
                    .JobTitle = reader("RoleName").ToString(),
                    .EmployeeID = reader("employeeID").ToString(),
                    .Department = reader("Department").ToString(),
                    .TotalAmount = reader("RequestTotal").ToString(),
                    .CArequestDate = Convert.ToDateTime(reader("requestDate")).ToString("MMM dd, yyyy")
                    }

                        ' Determine the status of the request based on the approvedBy field
                        status = reader("approvedBy").ToString()

                        If String.IsNullOrWhiteSpace(status) Then
                            request.Status = "Pending"
                        ElseIf status = "Rejected" Then
                            request.Status = "Rejected"
                        Else
                            request.Status = "Approved"
                        End If

                        requests.Add(request)
                    End While

                Catch ex As Exception
                    MessageBox.Show("Error fetching Requests: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
            Return requests
        End Function

    End Class

End Namespace

