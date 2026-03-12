
Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports DocumentFormat.OpenXml.Bibliography
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Misc.OverTime

    Public Class OverTimeRequestForm
        Inherits UserControl

        Private employeeNames As New List(Of String)

        Public Sub New()
            InitializeComponent()
            LoadEmployeeNames()
        End Sub

        ' ==========================================
        ' DATABASE: LOAD EMPLOYEE NAMES
        ' ==========================================
        Private Sub LoadEmployeeNames()
            Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString()
            Try
                Using conn As New MySqlConnection(connStr)
                    conn.Open()
                    Dim cmd As New MySqlCommand("SELECT Name FROM employee", conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            employeeNames.Add(reader("Name").ToString())
                        End While
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error loading employees: " & ex.Message)
            End Try
        End Sub

        ' ==========================================
        ' AUTOCOMPLETE LOGIC
        ' ==========================================
        Private Sub AutoCompleteTextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim input = AutoCompleteTextBox.Text.Trim().ToLower()
            Dim filtered = employeeNames.Where(Function(name) name.ToLower().Contains(input)).ToList()

            If String.IsNullOrWhiteSpace(AutoCompleteTextBox.Text) Then
                JobTitle.Text = ""
                EmployeeID.Text = ""
                hourlyRate.Text = ""
            End If

            If filtered.Any() AndAlso Not String.IsNullOrWhiteSpace(input) Then
                SuggestionListBox.ItemsSource = filtered
                AutoCompletePopup.IsOpen = True
            Else
                AutoCompletePopup.IsOpen = False
            End If
        End Sub

        Private Sub FetchEmployeeDetails(name As String)
            Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString()
            Try
                Using conn As New MySqlConnection(connStr)
                    conn.Open()
                    Dim cmd As New MySqlCommand("SELECT employeeID, JobTitle, Department FROM employee WHERE Name = @name", conn)
                    cmd.Parameters.AddWithValue("@name", name)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            EmployeeID.Text = reader("employeeID").ToString()
                            JobTitle.Text = reader("JobTitle").ToString()
                            hourlyRate.Text = reader("Department").ToString()
                        End If
                    End Using
                End Using
            Catch ex As Exception
            End Try
        End Sub

        ' ==========================================
        ' AUTOCOMPLETE CLICK & KEYBOARD HANDLERS
        ' ==========================================
        Private Sub SuggestionListBox_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            If SuggestionListBox.SelectedItem IsNot Nothing Then
                AutoCompleteTextBox.Text = SuggestionListBox.SelectedItem.ToString()
                AutoCompletePopup.IsOpen = False
                FetchEmployeeDetails(AutoCompleteTextBox.Text)
            End If
        End Sub

        Private Sub AutoCompleteTextBox_PreviewKeyDown(sender As Object, e As KeyEventArgs)
            If e.Key = Key.Down AndAlso AutoCompletePopup.IsOpen Then
                SuggestionListBox.Focus()
            End If
        End Sub

        ' ==========================================
        ' CALENDAR DROPDOWN HANDLERS
        ' ==========================================
        Private Sub OvertimeDate_Click(sender As Object, e As RoutedEventArgs)
            dtOvertimeDate.IsDropDownOpen = True
        End Sub

        Private Sub CashAdvanceDate_Click(sender As Object, e As RoutedEventArgs)
            CashAdvanceDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub RequestDate_Click(sender As Object, e As RoutedEventArgs)
            RequestDate.IsDropDownOpen = True
        End Sub

        Private Sub ApprovalDate_Click(sender As Object, e As RoutedEventArgs)
            ApprovalDate.IsDropDownOpen = True
        End Sub

        ' ==========================================
        ' DATE SYNC LOGIC
        ' ==========================================
        Private Sub MainDate_Changed(sender As Object, e As SelectionChangedEventArgs)
            If CashAdvanceDatePicker.SelectedDate.HasValue Then
                dtOvertimeDate.SelectedDate = CashAdvanceDatePicker.SelectedDate
                RequestDate.SelectedDate = CashAdvanceDatePicker.SelectedDate
            End If
        End Sub

        ' ==========================================
        ' SUBMIT BUTTON
        ' ==========================================
        Private Sub BtnSubmit_Click(sender As Object, e As RoutedEventArgs) Handles BtnSubmit.Click
            Dim rawDate As DateTime? = dtOvertimeDate.SelectedDate
            Dim totalHours As String = txtHours.Text

            If rawDate Is Nothing OrElse String.IsNullOrWhiteSpace(totalHours) Then
                MessageBox.Show("Please select a date and ensure Start/End times are valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim newRecord As New OvertimeRequestModel With {
                .OvertimeID = "OT-" & (ManageTimeoutRequests.GlobalOvertimeList.Count + 1).ToString("D3"),
                .EmployeeName = AutoCompleteTextBox.Text,
                .JobTitle = JobTitle.Text,
                .Department = hourlyRate.Text,
                .TotalHours = totalHours,
                .RequestDate = rawDate.Value.ToString("MMM dd, yyyy"),
                .Status = "Pending",
                .EmployeeID = EmployeeID.Text,
                .Supervisor = SupervisorName.Text,
                .StartTime = txtStartTime.Text,
                .EndTime = txtEndTime.Text,
                .Reason = txtReason.Text,
                .Remarks = approverRemarks.Text,
                .RequestedBy = TxtRequestedBy.Text
            }

            Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString()
            Try
                Using conn As New MySqlConnection(connStr)
                    conn.Open()
                    Dim cmd As New MySqlCommand(
                        "INSERT INTO overtime_requests 
                        (OvertimeID, EmployeeName, EmployeeID, JobTitle, Department, Supervisor,
                         StartTime, EndTime, TotalHours, Reason, Remarks, RequestedBy, RequestDate, Status)
                        VALUES
                        (@OvertimeID, @EmployeeName, @EmployeeID, @JobTitle, @Department, @Supervisor,
                         @StartTime, @EndTime, @TotalHours, @Reason, @Remarks, @RequestedBy, @RequestDate, @Status)", conn)

                    cmd.Parameters.AddWithValue("@OvertimeID", newRecord.OvertimeID)
                    cmd.Parameters.AddWithValue("@EmployeeName", newRecord.EmployeeName)
                    cmd.Parameters.AddWithValue("@EmployeeID", newRecord.EmployeeID)
                    cmd.Parameters.AddWithValue("@JobTitle", newRecord.JobTitle)
                    cmd.Parameters.AddWithValue("@Department", newRecord.Department)
                    cmd.Parameters.AddWithValue("@Supervisor", newRecord.Supervisor)
                    cmd.Parameters.AddWithValue("@StartTime", newRecord.StartTime)
                    cmd.Parameters.AddWithValue("@EndTime", newRecord.EndTime)
                    cmd.Parameters.AddWithValue("@TotalHours", newRecord.TotalHours)
                    cmd.Parameters.AddWithValue("@Reason", newRecord.Reason)
                    cmd.Parameters.AddWithValue("@Remarks", newRecord.Remarks)
                    cmd.Parameters.AddWithValue("@RequestedBy", newRecord.RequestedBy)
                    cmd.Parameters.AddWithValue("@RequestDate", newRecord.RequestDate)
                    cmd.Parameters.AddWithValue("@Status", newRecord.Status)

                    cmd.ExecuteNonQuery()
                End Using

                MessageBox.Show("Request Submitted Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                DPC.Data.Helpers.ViewLoader.DynamicView.NavigateToView("manageovertimerequests", Me)

            Catch ex As Exception
                MessageBox.Show("Error saving request: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' ==========================================
        ' TIME CALCULATION LOGIC
        ' ==========================================
        Private Sub Time_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtStartTime.TextChanged, txtEndTime.TextChanged
            CalculateHours()
        End Sub

        Private Sub CalculateHours()
            Dim startT As DateTime
            Dim endT As DateTime

            Dim startInput = txtStartTime.Text.Trim()
            Dim endInput = txtEndTime.Text.Trim()

            ' If no AM/PM typed, assume start is AM and end is PM
            If Not startInput.ToUpper().Contains("AM") AndAlso Not startInput.ToUpper().Contains("PM") Then
                startInput = startInput & " AM"
            End If
            If Not endInput.ToUpper().Contains("AM") AndAlso Not endInput.ToUpper().Contains("PM") Then
                endInput = endInput & " PM"
            End If

            If DateTime.TryParse(startInput, startT) AndAlso DateTime.TryParse(endInput, endT) Then
                Dim duration As TimeSpan = endT - startT

                If duration.TotalMinutes < 0 Then
                    duration = duration.Add(TimeSpan.FromDays(1))
                End If

                txtHours.Text = Math.Round(duration.TotalHours, 2).ToString()
            Else
                txtHours.Text = ""
            End If
        End Sub

    End Class
End Namespace