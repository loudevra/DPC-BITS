Imports System.Collections.ObjectModel
Imports System.Linq ' <--- THIS FIXES THE SEARCH BAR ERROR!
Imports System.Windows
Imports System.Windows.Controls
Imports System.IO
Imports Microsoft.Win32

Namespace DPC.Views.Misc.EmployeeLeave

    ' 1. Data Model (Now includes RequestDate and SupervisorDate)
    Public Class EmployeeLeaveModel
        Public Property LeaveID As String
        Public Property EmployeeName As String
        Public Property EmployeeID As String
        Public Property EmployeeEmail As String
        Public Property WorkPhone As String
        Public Property PersonalPhone As String
        Public Property Department As String
        Public Property SupervisorName As String

        Public Property RequestDate As String
        Public Property SupervisorDate As String

        Public Property StartDate As String
        Public Property EndDate As String
        Public Property HoursRequested As String
        Public Property LeaveCode As String
        Public Property Status As String
        Public Property Approver As String
        Public Property ApprovalDate As String
    End Class

    Public Class ManageEmployeeLeaveRequests
        Inherits UserControl

        ' 2. Shared Master List
        Public Shared GlobalLeaveList As New ObservableCollection(Of EmployeeLeaveModel)()

        Public Sub New()
            InitializeComponent()

            If GlobalLeaveList.Count = 0 Then
                GlobalLeaveList.Add(New EmployeeLeaveModel With {
                    .LeaveID = "LV-001",
                    .EmployeeName = "Jane Doe",
                    .EmployeeID = "EMP-012",
                    .EmployeeEmail = "jane.doe@company.com",
                    .WorkPhone = "555-0192",
                    .PersonalPhone = "555-9876",
                    .Department = "Human Resources",
                    .SupervisorName = "John Smith",
                    .RequestDate = "Mar 05, 2026",
                    .SupervisorDate = "Mar 05, 2026",
                    .StartDate = "Mar 10, 2026",
                    .EndDate = "Mar 12, 2026",
                    .HoursRequested = "24",
                    .LeaveCode = "Vacation (VC)",
                    .Status = "Pending"
                })
            End If

            dataGrid.ItemsSource = GlobalLeaveList
        End Sub

        Private Sub NavigateToNewRequest(sender As Object, e As RoutedEventArgs)
            DPC.Data.Helpers.ViewLoader.DynamicView.NavigateToView("employeeleaverequestform", Me)
        End Sub

        ' ==========================================
        ' DELETE BUTTON LOGIC
        ' ==========================================
        Private Sub DeleteRequest_Click(sender As Object, e As RoutedEventArgs)
            Dim btn As Button = TryCast(sender, Button)
            If btn Is Nothing Then Return

            Dim itemToDelete As EmployeeLeaveModel = TryCast(btn.DataContext, EmployeeLeaveModel)

            If itemToDelete IsNot Nothing Then
                Dim result = MessageBox.Show($"Are you sure you want to delete leave request {itemToDelete.LeaveID}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning)

                If result = MessageBoxResult.Yes Then
                    GlobalLeaveList.Remove(itemToDelete)
                End If
            End If
        End Sub

        ' ==========================================
        ' EDIT BUTTON LOGIC
        ' ==========================================
        Private Sub NavigateToEdit(sender As Object, e As RoutedEventArgs)
            Dim selectedRecord As EmployeeLeaveModel = CType(CType(sender, Button).DataContext, EmployeeLeaveModel)

            If selectedRecord IsNot Nothing Then
                ' Pass it to the shared Edit variable and navigate!
                DPC.Views.Misc.EmployeeLeave.EditEmployeeLeave.TargetEditRecord = selectedRecord
                DPC.Data.Helpers.ViewLoader.DynamicView.NavigateToView("editemployeeleave", Me)
            End If
        End Sub

        ' ==========================================
        ' PRINT BUTTON LOGIC
        ' ==========================================
        Private Sub NavigateToPreviewPrint(sender As Object, e As RoutedEventArgs)
            Dim btn As Button = TryCast(sender, Button)
            If btn Is Nothing Then Return

            Dim selectedRecord As EmployeeLeaveModel = TryCast(btn.DataContext, EmployeeLeaveModel)

            If selectedRecord IsNot Nothing Then
                ' Pass it to the shared Edit variable and navigate!
                DPC.Views.Misc.EmployeeLeave.PreviewPrintEmployeeLeave.TargetPrintRecord = selectedRecord
                DPC.Data.Helpers.ViewLoader.DynamicView.NavigateToView("previewprintemployeeleave", Me)
            End If
        End Sub

        ' ==========================================
        ' SEARCH FILTER LOGIC
        ' ==========================================
        Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim query As String = txtSearch.Text.ToLower()

            If String.IsNullOrWhiteSpace(query) Then
                dataGrid.ItemsSource = GlobalLeaveList
            Else
                Dim filteredList = GlobalLeaveList.Where(Function(item) _
                    (item.EmployeeName IsNot Nothing AndAlso item.EmployeeName.ToLower().Contains(query)) OrElse
                    (item.LeaveID IsNot Nothing AndAlso item.LeaveID.ToLower().Contains(query)) OrElse
                    (item.Department IsNot Nothing AndAlso item.Department.ToLower().Contains(query)) OrElse
                    (item.Status IsNot Nothing AndAlso item.Status.ToLower().Contains(query))
                ).ToList()

                dataGrid.ItemsSource = filteredList
            End If
        End Sub
        ' ---------------------------------------------------------------
        '  EXPORT TO EXCEL (CSV Format)
        ' ---------------------------------------------------------------
        Private Sub BtnExportExcel_Click(sender As Object, e As RoutedEventArgs) Handles btnExportExcel.Click

            ' --- TEST POPUP: If you don't see this, the code isn't building! ---
            MessageBox.Show("Excel Button Clicked!", "System Test")

            Try
                ' 1. Check if there is data in the grid
                If dataGrid.Items.Count = 0 Then
                    MessageBox.Show("No data to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                ' 2. Open the Save File Dialog
                Dim saveFileDialog As New SaveFileDialog()
                saveFileDialog.Filter = "CSV (Excel Compatible) (*.csv)|*.csv"
                saveFileDialog.FileName = "EmployeeLeave_Export_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv"
                saveFileDialog.Title = "Export Leave Requests to Excel"

                ' 3. If the user clicks "Save"
                If saveFileDialog.ShowDialog() = True Then

                    ' 4. Create and write to the file
                    Using writer As New StreamWriter(saveFileDialog.FileName)
                        ' Write the Header Row matching your DataGrid columns
                        writer.WriteLine("Ref #,Employee,Department,Start Date,End Date,Code,Hrs,Status")

                        ' 5. Loop through the current items in the DataGrid and write them
                        For Each obj In dataGrid.Items
                            Dim item As EmployeeLeaveModel = TryCast(obj, EmployeeLeaveModel)

                            If item IsNot Nothing Then
                                ' Wrap text in double quotes to prevent commas from breaking the columns
                                Dim ref = If(item.LeaveID, "").Replace("""", """""")
                                Dim empName = If(item.EmployeeName, "").Replace("""", """""")
                                Dim dept = If(item.Department, "").Replace("""", """""")
                                Dim startDate = If(item.StartDate, "").Replace("""", """""")
                                Dim endDate = If(item.EndDate, "").Replace("""", """""")
                                Dim code = If(item.LeaveCode, "").Replace("""", """""")
                                Dim hrs = If(item.HoursRequested, "").Replace("""", """""")
                                Dim status = If(item.Status, "").Replace("""", """""")

                                writer.WriteLine($"""{ref}"",""{empName}"",""{dept}"",""{startDate}"",""{endDate}"",""{code}"",""{hrs}"",""{status}""")
                            End If
                        Next
                    End Using

                    MessageBox.Show("Leave request data successfully exported!", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If

            Catch ex As Exception
                MessageBox.Show($"An error occurred while exporting: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub



    End Class
End Namespace