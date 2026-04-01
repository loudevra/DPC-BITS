Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input

Namespace DPC.Views.Misc.EmployeeLeave

    Public Class EditEmployeeLeave
        Inherits UserControl

        ' Stores the record being edited passed from the DataGrid
        Public Shared TargetEditRecord As EmployeeLeaveModel

        Public Sub New()
            InitializeComponent()
            LoadRecordData()
        End Sub

        ' 1. Load ALL existing data into the form
        Private Sub LoadRecordData()
            If TargetEditRecord IsNot Nothing Then
                AutoCompleteTextBox.Text = TargetEditRecord.EmployeeName
                EmployeeID.Text = TargetEditRecord.EmployeeID
                EmployeeEmail.Text = TargetEditRecord.EmployeeEmail
                WorkPhone.Text = TargetEditRecord.WorkPhone
                PersonalPhone.Text = TargetEditRecord.PersonalPhone
                Department.Text = TargetEditRecord.Department
                SupervisorName.Text = TargetEditRecord.SupervisorName
                txtHours.Text = TargetEditRecord.HoursRequested

                ' Load Dates
                Dim tDate As DateTime
                If DateTime.TryParse(TargetEditRecord.RequestDate, tDate) Then
                    TodayDatePicker.SelectedDate = tDate
                End If

                Dim supDate As DateTime
                If DateTime.TryParse(TargetEditRecord.SupervisorDate, supDate) Then
                    SupervisorDate.SelectedDate = supDate
                End If

                Dim sDate As DateTime
                If DateTime.TryParse(TargetEditRecord.StartDate, sDate) Then
                    StartDatePicker.SelectedDate = sDate
                End If

                Dim eDate As DateTime
                If DateTime.TryParse(TargetEditRecord.EndDate, eDate) Then
                    EndDatePicker.SelectedDate = eDate
                End If

                Dim aDate As DateTime
                If DateTime.TryParse(TargetEditRecord.ApprovalDate, aDate) Then
                    ApprovalDate.SelectedDate = aDate
                End If

                ' Load Leave Code
                For Each item As ComboBoxItem In CbLeaveCode.Items
                    If item.Content.ToString() = TargetEditRecord.LeaveCode Then
                        CbLeaveCode.SelectedItem = item
                        Exit For
                    End If
                Next

                ' Load Approver
                For Each item As ComboBoxItem In CbApprover.Items
                    If item.Content.ToString() = TargetEditRecord.Approver Then
                        CbApprover.SelectedItem = item
                        Exit For
                    End If
                Next
            End If
        End Sub

        ' ==========================================
        ' CALENDAR CLICK HANDLERS
        ' ==========================================
        Private Sub TodayDate_Click(sender As Object, e As RoutedEventArgs)
            TodayDatePicker.DisplayDateStart = DateTime.Today
            TodayDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            ' If there's already an older date saved, use that as the minimum so it doesn't crash. 
            ' Otherwise, restrict to today.
            Dim minDate As DateTime = DateTime.Today
            If StartDatePicker.SelectedDate.HasValue AndAlso StartDatePicker.SelectedDate.Value < DateTime.Today Then
                minDate = StartDatePicker.SelectedDate.Value
            End If

            StartDatePicker.DisplayDateStart = minDate
            StartDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub EndDate_Click(sender As Object, e As RoutedEventArgs)
            EndDatePicker.DisplayDateStart = DateTime.Today
            EndDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub SupervisorDate_Click(sender As Object, e As RoutedEventArgs)
            SupervisorDate.DisplayDateStart = DateTime.Today
            SupervisorDate.IsDropDownOpen = True
        End Sub

        Private Sub ApprovalDate_Click(sender As Object, e As RoutedEventArgs)
            ApprovalDate.DisplayDateStart = DateTime.Today
            ApprovalDate.IsDropDownOpen = True
        End Sub
        ' ==========================================
        ' SAVE / APPROVE / REJECT LOGIC
        ' ==========================================
        Private Sub RejectRequest_Click(sender As Object, e As RoutedEventArgs)
            ProcessUpdate("Rejected")
        End Sub

        Private Sub ApproveRequest_Click(sender As Object, e As RoutedEventArgs)
            ProcessUpdate("Approved")
        End Sub

        Private Sub SaveRequest_Click(sender As Object, e As RoutedEventArgs)
            ProcessUpdate("Pending") ' Or whichever status you want as default when just saving
        End Sub

        Private Sub ProcessUpdate(newStatus As String)
            If TargetEditRecord IsNot Nothing Then

                ' Update ALL properties in the model
                TargetEditRecord.EmployeeName = AutoCompleteTextBox.Text
                TargetEditRecord.EmployeeID = EmployeeID.Text
                TargetEditRecord.EmployeeEmail = EmployeeEmail.Text
                TargetEditRecord.WorkPhone = WorkPhone.Text
                TargetEditRecord.PersonalPhone = PersonalPhone.Text
                TargetEditRecord.Department = Department.Text
                TargetEditRecord.SupervisorName = SupervisorName.Text
                TargetEditRecord.HoursRequested = txtHours.Text
                TargetEditRecord.LeaveCode = If(CbLeaveCode.SelectedItem IsNot Nothing, CType(CbLeaveCode.SelectedItem, ComboBoxItem).Content.ToString(), "")

                If TodayDatePicker.SelectedDate.HasValue Then
                    TargetEditRecord.RequestDate = TodayDatePicker.SelectedDate.Value.ToString("MMM dd, yyyy")
                End If

                If SupervisorDate.SelectedDate.HasValue Then
                    TargetEditRecord.SupervisorDate = SupervisorDate.SelectedDate.Value.ToString("MMM dd, yyyy")
                End If

                If StartDatePicker.SelectedDate.HasValue Then
                    TargetEditRecord.StartDate = StartDatePicker.SelectedDate.Value.ToString("MMM dd, yyyy")
                End If

                If EndDatePicker.SelectedDate.HasValue Then
                    TargetEditRecord.EndDate = EndDatePicker.SelectedDate.Value.ToString("MMM dd, yyyy")
                End If

                TargetEditRecord.Approver = If(CbApprover.SelectedItem IsNot Nothing, CType(CbApprover.SelectedItem, ComboBoxItem).Content.ToString(), "")

                If ApprovalDate.SelectedDate.HasValue Then
                    TargetEditRecord.ApprovalDate = ApprovalDate.SelectedDate.Value.ToString("MMM dd, yyyy")
                End If

                TargetEditRecord.Status = newStatus

                ' Overwrite the list index with our updated record
                Dim index = ManageEmployeeLeaveRequests.GlobalLeaveList.IndexOf(TargetEditRecord)
                If index >= 0 Then
                    ManageEmployeeLeaveRequests.GlobalLeaveList(index) = TargetEditRecord
                End If
            End If

            MessageBox.Show($"Leave Request {newStatus} Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

            ' Navigate back to the main datagrid view
            DPC.Data.Helpers.ViewLoader.DynamicView.NavigateToView("manageemployeeleaverequests", Me)
        End Sub

        ' ==========================================
        ' NUMBER RESTRICTION LOGIC
        ' ==========================================
        Private Sub NumericOnly_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            Dim regex As New System.Text.RegularExpressions.Regex("[^0-9]+")
            e.Handled = regex.IsMatch(e.Text)
        End Sub

    End Class
End Namespace