Imports System.Collections.ObjectModel
Imports DocumentFormat.OpenXml.Office2010.Excel
Imports DPC.DPC.Data.Controllers.Misc
Imports DPC.DPC.Data.Helpers.ViewLoader
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient
Imports System.IO
Imports Microsoft.Win32

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
            ' Get the specific item bound to the row where the button was clicked
            Dim btn As Button = TryCast(sender, Button)
            Dim selectedRequest As CashAdvanceRetrieval = TryCast(btn.DataContext, CashAdvanceRetrieval)

            If selectedRequest IsNot Nothing Then
                ' Set the selected item manually so GetAllData picks it up
                dataGrid.SelectedItem = selectedRequest
                GetAllData()

                ' Navigate to the Print Preview page
                DynamicView.NavigateToView("previewprintcashadvancerequestform", Me)
            End If
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
        ' ---------------------------------------------------------------
        '  EXPORT TO EXCEL (CSV Format)
        ' ---------------------------------------------------------------
        Private Sub BtnExportExcel_Click(sender As Object, e As RoutedEventArgs)
            Try
                ' 1. Check if there is data in the grid
                If dataGrid.Items.Count = 0 Then
                    MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                ' 2. Open the Save File Dialog
                Dim saveFileDialog As New SaveFileDialog()
                saveFileDialog.Filter = "CSV (Excel Compatible) (*.csv)|*.csv"
                saveFileDialog.FileName = "CashAdvance_Export_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
                saveFileDialog.Title = "Export Cash Advance Requests to Excel"

                ' 3. If the user clicks "Save"
                If saveFileDialog.ShowDialog() = True Then

                    ' 4. Create and write to the file
                    Using writer As New StreamWriter(saveFileDialog.FileName)
                        ' Write the Header Row matching your DataGrid columns
                        writer.WriteLine("Ref #,Employee,Job Title,Department,Total Amount,Request Date,Status")

                        ' 5. Loop through the current items in the DataGrid and write them
                        For Each obj In dataGrid.Items
                            Dim item As CashAdvanceRetrieval = TryCast(obj, CashAdvanceRetrieval)

                            If item IsNot Nothing Then
                                ' Wrap text in double quotes to prevent commas from breaking the columns
                                Dim ref = If(item.CashAdvanceID, "").Replace("""", """""")
                                Dim empName = If(item.EmployeeName, "").Replace("""", """""")
                                Dim jobTitle = If(item.JobTitle, "").Replace("""", """""")
                                Dim dept = If(item.Department, "").Replace("""", """""")
                                Dim amount = If(item.TotalAmount, "").Replace("""", """""")
                                Dim reqDate = If(item.CArequestDate, "").Replace("""", """""")
                                Dim status = If(item.Status, "").Replace("""", """""")

                                writer.WriteLine($"""{ref}"",""{empName}"",""{jobTitle}"",""{dept}"",""{amount}"",""{reqDate}"",""{status}""")
                            End If
                        Next
                    End Using

                    MessageBox.Show("Cash advance data successfully exported!", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If

            Catch ex As Exception
                MessageBox.Show($"An error occurred while exporting: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
        ' ---------------------------------------------------------------
        '  DELETE FUNCTIONALITY
        ' ---------------------------------------------------------------
        Private Sub BtnDelete_Click(sender As Object, e As RoutedEventArgs)
            ' 1. Identify the selected row via the button's DataContext
            Dim btn As Button = TryCast(sender, Button)
            Dim selectedRequest As CashAdvanceRetrieval = TryCast(btn.DataContext, CashAdvanceRetrieval)

            If selectedRequest Is Nothing Then Return

            ' 2. Ask for user confirmation
            Dim result As MessageBoxResult = MessageBox.Show(
                $"Are you sure you want to delete Cash Advance Ref: {selectedRequest.CashAdvanceID}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning)

            If result = MessageBoxResult.Yes Then
                DeleteRecordFromDatabase(selectedRequest.CashAdvanceID)
            End If
        End Sub

        Private Sub DeleteRecordFromDatabase(refID As String)
            ' The query to remove the record
            Dim query As String = "DELETE FROM cashadvance WHERE caRef = @ID"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@ID", refID)

                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                    If rowsAffected > 0 Then
                        MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                        ' 3. Refresh the DataGrid to show current data
                        LoadData()
                    Else
                        MessageBox.Show("Record not found or already deleted.", "Notice", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                    End If

                Catch ex As Exception
                    MessageBox.Show($"Database Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Finally
                    conn.Close()
                End Try
            End Using
        End Sub
        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs) Handles btnAddNew.Click
            DynamicView.NavigateToView("addcashadvancerequest", Me)
        End Sub
    End Class

End Namespace

