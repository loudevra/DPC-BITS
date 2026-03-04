Imports System.Windows
Imports System.Windows.Controls

Namespace DPC.Views.Misc.OverTime

    Public Class EditOverTime
        Inherits UserControl

        ' Stores the record being edited
        Public Shared TargetEditRecord As OvertimeRequestModel

        Public Sub New()
            InitializeComponent()
            LoadRecordData()
        End Sub

        ' 1. Load existing data into the form
        Private Sub LoadRecordData()
            If TargetEditRecord IsNot Nothing Then
                ' Existing fields
                AutoCompleteTextBox.Text = TargetEditRecord.EmployeeName
                JobTitle.Text = TargetEditRecord.JobTitle
                Department.Text = TargetEditRecord.Department
                txtHours.Text = TargetEditRecord.TotalHours

                ' --- NEW FIELDS TO FETCH ---
                EmployeeID.Text = TargetEditRecord.EmployeeID
                SupervisorName.Text = TargetEditRecord.Supervisor
                txtStartTime.Text = TargetEditRecord.StartTime
                txtEndTime.Text = TargetEditRecord.EndTime
                txtReason.Text = TargetEditRecord.Reason
                approverRemarks.Text = TargetEditRecord.Remarks
                TxtRequestedBy.Text = TargetEditRecord.RequestedBy

                ' Load Dates
                Dim parsedDate As DateTime
                If DateTime.TryParse(TargetEditRecord.RequestDate, parsedDate) Then
                    ' Sets the date for all date pickers on the form
                    dtOvertimeDate.SelectedDate = parsedDate
                    CashAdvanceDatePicker.SelectedDate = parsedDate
                    RequestDate.SelectedDate = parsedDate
                End If
            End If
        End Sub

        ' ==========================================
        ' CALENDAR CLICK HANDLERS
        ' ==========================================
        ' These methods manually open the dropdown when the button is clicked

        Private Sub OvertimeDate_Click(sender As Object, e As RoutedEventArgs)
            dtOvertimeDate.IsDropDownOpen = True
        End Sub

        Private Sub CashAdvanceDate_Click(sender As Object, e As RoutedEventArgs)
            CashAdvanceDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub RequestDate_Click(sender As Object, e As RoutedEventArgs)
            RequestDate.IsDropDownOpen = True
        End Sub

        ' Add this handler to your EditOverTime class
        Private Sub ApprovalDate_Click(sender As Object, e As RoutedEventArgs)
            ' This manually opens the calendar dropdown when you click the button
            If ApprovalDate IsNot Nothing Then
                ApprovalDate.IsDropDownOpen = True
            End If
        End Sub

        ' Add these handlers to your EditOverTime class
        Private Sub RejectRequest_Click(sender As Object, e As RoutedEventArgs)
            ProcessUpdate("Rejected")
        End Sub

        Private Sub ApproveRequest_Click(sender As Object, e As RoutedEventArgs)
            ProcessUpdate("Approved")
        End Sub

        Private Sub SaveRequest_Click(sender As Object, e As RoutedEventArgs)
            ProcessUpdate("Pending") ' Or whatever status you use for a standard save
        End Sub

        ' Updated ProcessUpdate to use the specific status
        Private Sub ProcessUpdate(newStatus As String)
            If TargetEditRecord IsNot Nothing Then
                ' Save existing fields
                TargetEditRecord.EmployeeName = AutoCompleteTextBox.Text
                TargetEditRecord.JobTitle = JobTitle.Text
                TargetEditRecord.Department = Department.Text
                TargetEditRecord.TotalHours = txtHours.Text

                ' --- SAVE NEW EDITED FIELDS ---
                TargetEditRecord.EmployeeID = EmployeeID.Text
                TargetEditRecord.Supervisor = SupervisorName.Text
                TargetEditRecord.StartTime = txtStartTime.Text
                TargetEditRecord.EndTime = txtEndTime.Text
                TargetEditRecord.Reason = txtReason.Text
                TargetEditRecord.Remarks = approverRemarks.Text
                TargetEditRecord.RequestedBy = TxtRequestedBy.Text

                If dtOvertimeDate.SelectedDate.HasValue Then
                    TargetEditRecord.RequestDate = dtOvertimeDate.SelectedDate.Value.ToString("MMM dd, yyyy")
                End If

                TargetEditRecord.Status = newStatus

                ' Update the shared list
                Dim index = ManageTimeoutRequests.GlobalOvertimeList.IndexOf(TargetEditRecord)
                If index >= 0 Then
                    ManageTimeoutRequests.GlobalOvertimeList(index) = TargetEditRecord
                End If
            End If

            MessageBox.Show($"Request {newStatus} Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

            ' Use the correct navigation name to avoid the "View Not Found" error
            DPC.Data.Helpers.ViewLoader.DynamicView.NavigateToView("manageovertimerequests", Me)
        End Sub


    End Class
End Namespace