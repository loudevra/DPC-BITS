Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Linq
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Misc.EmployeeLeave

    Public Class EmployeeLeaveRequestForm
        Inherits UserControl

        ' List to store names for autocomplete
        Private employeeNames As New List(Of String)

        Public Sub New()
            InitializeComponent()
            LoadEmployeeNames()

            ' Set Today's date by default
            If TodayDatePicker IsNot Nothing Then
                TodayDatePicker.SelectedDate = DateTime.Now
            End If
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
                EmployeeID.Text = ""
                Department.Text = ""
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

                    Dim cmd As New MySqlCommand("SELECT EmployeeID, Department FROM employee WHERE Name = @name", conn)
                    cmd.Parameters.AddWithValue("@name", name.Trim())

                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            EmployeeID.Text = reader("EmployeeID").ToString()
                            Department.Text = reader("Department").ToString()
                        End If
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Database Error: " & ex.Message, "Error Fetching Details")
            End Try
        End Sub

        Private Sub SuggestionListBox_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            Dim item = ItemsControl.ContainerFromElement(SuggestionListBox, CType(e.OriginalSource, DependencyObject))

            If item IsNot Nothing Then
                Dim selected As String = CType(item, ListBoxItem).Content.ToString()
                AutoCompleteTextBox.Text = selected
                FetchEmployeeDetails(selected)
                AutoCompletePopup.IsOpen = False
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
        Private Sub TodayDate_Click(sender As Object, e As RoutedEventArgs)
            TodayDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            StartDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub EndDate_Click(sender As Object, e As RoutedEventArgs)
            EndDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub SupervisorDate_Click(sender As Object, e As RoutedEventArgs)
            SupervisorDate.IsDropDownOpen = True
        End Sub

        Private Sub ApprovalDate_Click(sender As Object, e As RoutedEventArgs)
            ApprovalDate.IsDropDownOpen = True
        End Sub

        ' ==========================================
        ' SUBMIT LOGIC
        ' ==========================================
        Private Sub BtnSubmit_Click(sender As Object, e As RoutedEventArgs) Handles BtnSubmit.Click

            If String.IsNullOrWhiteSpace(AutoCompleteTextBox.Text) OrElse StartDatePicker.SelectedDate Is Nothing OrElse EndDatePicker.SelectedDate Is Nothing Then
                MessageBox.Show("Please ensure Employee Name and Leave Dates are filled out.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim newRecord As New EmployeeLeaveModel With {
                .LeaveID = "LV-" & (ManageEmployeeLeaveRequests.GlobalLeaveList.Count + 1).ToString("D3"),
                .EmployeeName = AutoCompleteTextBox.Text,
                .EmployeeID = EmployeeID.Text,
                .EmployeeEmail = EmployeeEmail.Text,
                .WorkPhone = WorkPhone.Text,
                .PersonalPhone = PersonalPhone.Text,
                .Department = Department.Text,
                .SupervisorName = SupervisorName.Text,
                .RequestDate = If(TodayDatePicker.SelectedDate.HasValue, TodayDatePicker.SelectedDate.Value.ToString("MMM dd, yyyy"), ""),
                .SupervisorDate = If(SupervisorDate.SelectedDate.HasValue, SupervisorDate.SelectedDate.Value.ToString("MMM dd, yyyy"), ""),
                .StartDate = If(StartDatePicker.SelectedDate.HasValue, StartDatePicker.SelectedDate.Value.ToString("MMM dd, yyyy"), ""),
                .EndDate = If(EndDatePicker.SelectedDate.HasValue, EndDatePicker.SelectedDate.Value.ToString("MMM dd, yyyy"), ""),
                .HoursRequested = txtHours.Text,
                .LeaveCode = If(CbLeaveCode.SelectedItem IsNot Nothing, CType(CbLeaveCode.SelectedItem, ComboBoxItem).Content.ToString(), ""),
                .Status = "Pending",
                .Approver = If(CbApprover.Text = "None" OrElse String.IsNullOrWhiteSpace(CbApprover.Text), "", CbApprover.Text),
                .ApprovalDate = If(ApprovalDate.SelectedDate.HasValue, ApprovalDate.SelectedDate.Value.ToString("MMM dd, yyyy"), "")
            }

            ManageEmployeeLeaveRequests.GlobalLeaveList.Add(newRecord)
            MessageBox.Show("Leave Request Submitted Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
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