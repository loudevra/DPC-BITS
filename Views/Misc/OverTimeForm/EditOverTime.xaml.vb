Imports System.Windows
Imports System.Windows.Controls
Imports MySql.Data.MySqlClient


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

        ' Updated ProcessUpdate to use the specific status and save to MySQL
        Private Sub ProcessUpdate(newStatus As String)
            If TargetEditRecord IsNot Nothing Then
                ' 1. Update the local object with the new data
                TargetEditRecord.EmployeeName = AutoCompleteTextBox.Text
                TargetEditRecord.JobTitle = JobTitle.Text
                TargetEditRecord.Department = Department.Text
                TargetEditRecord.TotalHours = txtHours.Text
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

                ' 2. UPDATE THE MYSQL DATABASE
                Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString()
                Try
                    Using conn As New MySqlConnection(connStr)
                        conn.Open()

                        ' SQL Update Command matching the fields you load in ManageTimeoutRequests
                        Dim query As String = "UPDATE overtime_requests SET EmployeeName=@EmployeeName, JobTitle=@JobTitle, Department=@Department, TotalHours=@TotalHours, EmployeeID=@EmployeeID, Supervisor=@Supervisor, StartTime=@StartTime, EndTime=@EndTime, Reason=@Reason, Remarks=@Remarks, RequestedBy=@RequestedBy, RequestDate=@RequestDate, Status=@Status WHERE OvertimeID=@OvertimeID"

                        Using cmd As New MySqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@EmployeeName", TargetEditRecord.EmployeeName)
                            cmd.Parameters.AddWithValue("@JobTitle", TargetEditRecord.JobTitle)
                            cmd.Parameters.AddWithValue("@Department", TargetEditRecord.Department)
                            cmd.Parameters.AddWithValue("@TotalHours", TargetEditRecord.TotalHours)
                            cmd.Parameters.AddWithValue("@EmployeeID", TargetEditRecord.EmployeeID)
                            cmd.Parameters.AddWithValue("@Supervisor", TargetEditRecord.Supervisor)
                            cmd.Parameters.AddWithValue("@StartTime", TargetEditRecord.StartTime)
                            cmd.Parameters.AddWithValue("@EndTime", TargetEditRecord.EndTime)
                            cmd.Parameters.AddWithValue("@Reason", TargetEditRecord.Reason)
                            cmd.Parameters.AddWithValue("@Remarks", TargetEditRecord.Remarks)
                            cmd.Parameters.AddWithValue("@RequestedBy", TargetEditRecord.RequestedBy)
                            cmd.Parameters.AddWithValue("@RequestDate", TargetEditRecord.RequestDate)
                            cmd.Parameters.AddWithValue("@Status", TargetEditRecord.Status)
                            cmd.Parameters.AddWithValue("@OvertimeID", TargetEditRecord.OvertimeID)

                            cmd.ExecuteNonQuery()
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error updating database: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return ' Stop execution here so we don't navigate away if the DB update fails
                End Try

                ' 3. Update the shared list for the UI
                Dim index = ManageTimeoutRequests.GlobalOvertimeList.IndexOf(TargetEditRecord)
                If index >= 0 Then
                    ManageTimeoutRequests.GlobalOvertimeList.RemoveAt(index)
                    ManageTimeoutRequests.GlobalOvertimeList.Insert(index, TargetEditRecord)
                End If
            End If

            MessageBox.Show($"Request {newStatus} Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

            ' Navigate back to the main list
            DPC.Data.Helpers.ViewLoader.DynamicView.NavigateToView("manageovertimerequests", Me)
        End Sub



    End Class

End Namespace