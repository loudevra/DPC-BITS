Imports System.Collections.ObjectModel
Imports DocumentFormat.OpenXml.Office2010.Excel
Imports DPC.DPC.Data.Controllers.Misc
Imports DPC.DPC.Data.Helpers.ViewLoader
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Misc.CashAdvance
    Public Class ManageCashAdvanceRequests

        Public editcashadvance As EditCashAdvanceRequestController

        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            LoadData()
            'editcashadvance = New EditCashAdvanceRequestController(Me)

        End Sub

        '' Cache variables to store data for the edit and print preview views
        Public Sub LoadData()
            ' Create an instance of the controller
            Dim controller As New DPC.Data.Controllers.Misc.ManageCashAdvanceController()
            Dim requestList As ObservableCollection(Of CashAdvanceRetrieval) = controller.GetRequests()

            dataGrid.ItemsSource = requestList
        End Sub

        Private Sub NavigateToEdit(sender As Object, e As RoutedEventArgs)
            Dim btn As Button = TryCast(sender, Button)
            GetAllData()
            DynamicView.NavigateToView("editcashadvancerequest", Me)
        End Sub


        Private Sub NavigateToPrintPreview(sender As Object, e As RoutedEventArgs)
            Dim btn As Button = TryCast(sender, Button)
            GetAllData()
            DynamicView.NavigateToView("previewprintcashadvancerequestform", Me)
        End Sub


        '' Retrieves all data for the selected cash advance request
        Private Sub GetAllData()
            Dim selectedRequest As CashAdvanceRetrieval = TryCast(dataGrid.SelectedItem, CashAdvanceRetrieval)
            Dim query As String = "SELECT employeeID, caDate, Supervisor, Rate, RequestInfo, requestedBy, requestDate, approvedBy, approvalDate, Remarks FROM cashadvance WHERE caRef = @ID"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@ID", selectedRequest.CashAdvanceID)

                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then

                            ' Cache variables to store data for the edit and print preview views
                            cacheCAREmployeeID = reader("employeeID").ToString()
                            cacheCARCADate = Convert.ToDateTime(reader("caDate")).ToString("MMM dd, yyyy")
                            cacheCARSupervisor = reader("Supervisor").ToString()
                            cacheCARRate = reader("Rate").ToString()
                            cacheCARRequestInfo = reader("RequestInfo").ToString()
                            cacheCARrequestedBy = reader("requestedBy").ToString()
                            cacheCARrequestDate = Convert.ToDateTime(reader("requestDate")).ToString("MMM dd, yyyy")
                            cacheCARApprovedBy = If(reader("approvedBy") IsNot DBNull.Value, reader("approvedBy").ToString(), String.Empty)
                            cacheCARApprovalDate = If(reader("approvalDate") IsNot DBNull.Value, Convert.ToDateTime(reader("approvalDate")).ToString("MMM dd, yyyy"), Date.Today)
                            cacheCARRemarks = If(reader("remarks") IsNot DBNull.Value, reader("remarks").ToString(), String.Empty)
                            cacheCARCAID = selectedRequest.CashAdvanceID
                            cacheCAREmployeeName = selectedRequest.EmployeeName
                            cacheCARJobTitle = selectedRequest.JobTitle
                            cacheCAREmployeeID = selectedRequest.EmployeeID
                            cacheCARDepartment = selectedRequest.Department
                            cacheCARTotalAmount = selectedRequest.TotalAmount
                        End If
                    End Using
                Catch ex As Exception
                    MessageBox.Show("May Error")
                End Try
            End Using
        End Sub
    End Class

End Namespace

