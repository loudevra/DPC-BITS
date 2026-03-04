Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports DocumentFormat.OpenXml.Bibliography
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Misc.OverTime

    Public Class OverTimeRequestForm
        Inherits UserControl

        ' List to store names for autocomplete
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
        Private Sub AutoCompleteTextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim input = AutoCompleteTextBox.Text.Trim().ToLower()
            Dim filtered = employeeNames.Where(Function(name) name.ToLower().Contains(input)).ToList()

            If String.IsNullOrWhiteSpace(AutoCompleteTextBox.Text) Then
                JobTitle.Text = ""
                EmployeeID.Text = ""
                hourlyRate.Text = "" ' <-- Updated from hourlyRate
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
                            hourlyRate.Text = reader("Department").ToString() ' <-- Updated from hourlyRate
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
                ' Put the clicked name into the textbox
                AutoCompleteTextBox.Text = SuggestionListBox.SelectedItem.ToString()
                AutoCompletePopup.IsOpen = False

                ' Automatically fetch their job title and department
                FetchEmployeeDetails(AutoCompleteTextBox.Text)
            End If
        End Sub

        Private Sub AutoCompleteTextBox_PreviewKeyDown(sender As Object, e As KeyEventArgs)
            ' Allows the user to use the down arrow key to select a name from the list
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
            ' When they pick the main date, auto-fill the other date pickers to match
            If CashAdvanceDatePicker.SelectedDate.HasValue Then
                dtOvertimeDate.SelectedDate = CashAdvanceDatePicker.SelectedDate
                RequestDate.SelectedDate = CashAdvanceDatePicker.SelectedDate
            End If
        End Sub

        Private Sub BtnSubmit_Click(sender As Object, e As RoutedEventArgs) Handles BtnSubmit.Click
            Dim rawDate As DateTime? = dtOvertimeDate.SelectedDate
            Dim totalHours As String = txtHours.Text

            If rawDate Is Nothing OrElse String.IsNullOrWhiteSpace(totalHours) Then
                MessageBox.Show("Please select a date and ensure Start/End times are valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' 1. Package ALL the form data using the matched x:Names
            Dim newRecord As New OvertimeRequestModel With {
        .OvertimeID = "OT-" & (ManageTimeoutRequests.GlobalOvertimeList.Count + 1).ToString("D3"),
        .EmployeeName = AutoCompleteTextBox.Text,
        .JobTitle = JobTitle.Text,
        .Department = hourlyRate.Text, ' <-- Updated
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

            ' 2. Add to Shared List
            ManageTimeoutRequests.GlobalOvertimeList.Add(newRecord)

            MessageBox.Show("Request Submitted Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
            DPC.Data.Helpers.ViewLoader.DynamicView.NavigateToView("manageovertimerequests", Me)
        End Sub
        ' ==========================================
        ' TIME CALCULATION LOGIC
        ' ==========================================
        ' 1. This event triggers automatically whenever the Start Time or End Time text changes
        Private Sub Time_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtStartTime.TextChanged, txtEndTime.TextChanged
            CalculateHours()
        End Sub

        ' 2. This does the math to find the difference between the two times
        Private Sub CalculateHours()
            Dim startT As DateTime
            Dim endT As DateTime

            ' Only calculate if both textboxes have valid time formats (like "8:00 AM" and "5:00 PM")
            If DateTime.TryParse(txtStartTime.Text, startT) AndAlso DateTime.TryParse(txtEndTime.Text, endT) Then

                Dim duration As TimeSpan = endT - startT

                ' If the end time crosses midnight (e.g., 10:00 PM to 2:00 AM), add 24 hours to the math
                If duration.TotalMinutes < 0 Then
                    duration = duration.Add(TimeSpan.FromDays(1))
                End If

                ' Update the Total Hours textbox, rounded to 2 decimal places
                txtHours.Text = Math.Round(duration.TotalHours, 2).ToString()
            Else
                ' If the times are blank or invalid, leave the hours blank
                txtHours.Text = ""
            End If
        End Sub


    End Class
End Namespace