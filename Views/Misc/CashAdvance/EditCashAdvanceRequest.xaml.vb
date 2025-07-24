Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Controllers.Misc
Imports DPC.DPC.Data.Helpers.ViewLoader
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Misc.CashAdvance
    Public Class EditCashAdvanceRequest

        Private employeeNames As New List(Of String)
        Public editcashadvance As EditCashAdvanceRequestController

        Public Property CashAdvanceDate As New CalendarController.SingleCalendar()
        Public Property ApprovalDate As New CalendarController.SingleCalendar()
        Public Property RequestDate As New CalendarController.SingleCalendar()

        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()
            ' Add any initialization after the InitializeComponent() call.
            LoadEmployeeNames()
            DataContext = Me
            editcashadvance = New EditCashAdvanceRequestController(Me)
            ApprovalDate.SelectedDate = Date.Today
            ApprovalDatePicker.DisplayDateStart = Date.Today
            RequestDatePicker.DisplayDateStart = RequestDate.SelectedDate
        End Sub

        '
        ' Cache variables for the current request
        Private Sub InitializeContent()
            AutoCompleteTextBox.Text = cacheCAREmployeeName
            AutoCompletePopup.IsOpen = False

            JobTitle.Text = cacheCARJobTitle
            EmployeeID.Text = cacheCAREmployeeID
            Department.Text = cacheCARDepartment

            CashAdvanceDate.SelectedDate = cacheCARCADate
            CashAdvanceDatePicker.DisplayDateStart = CashAdvanceDate.SelectedDate

            SupervisorName.Text = cacheCARSupervisor
            hourlyRate.Text = cacheCARRate

            ' Load the request amounts from the cached JSON data
            Dim requestInfo As List(Of EditCashAdvanceJsonToList) = editcashadvance.ConvertJSONToList(cacheCARRequestInfo)
            For Each request As EditCashAdvanceJsonToList In requestInfo
                editcashadvance.AddAmountRequestRow(request.Amount, request.DateRequested, request.Reason)
            Next

            TbTotalRequestAmount.Text = cacheCARTotalAmount
            approverRemarks.Text = cacheCARRemarks
            TxtRequestedBy.Text = cacheCARrequestedBy

            RequestDate.SelectedDate = cacheCARrequestDate
            RequestDatePicker.DisplayDateStart = RequestDate.SelectedDate


            ' ' Set the approver combobox selection based on the cached value
            For Each Approver As ComboBoxItem In CbApprover.Items
                If Approver.Content.ToString() = cacheCARApprovedBy Then
                    CbApprover.SelectedValue = Approver
                    Exit For
                End If
            Next

            ApprovalDate.SelectedDate = cacheCARApprovalDate
            ApprovalDatePicker.DisplayDateStart = ApprovalDate.SelectedDate

        End Sub

        ' ' Handles the Loaded event of the CashAdvanceNewRequest control
        Public Sub CashAdvanceNewRequest_Loaded(sender As Object, e As RoutedEventArgs)
            InitializeContent()
        End Sub

        Private Sub RequestDate_Click(sender As Object, e As RoutedEventArgs)
            RequestDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub CashAdvanceDate_Click(sender As Object, e As RoutedEventArgs)
            CashAdvanceDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub ApprovalDate_Click(sender As Object, e As RoutedEventArgs)
            ApprovalDatePicker.IsDropDownOpen = True
        End Sub

        ' ' Handles the Click event of the BtnAddRow button
        Private Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs) Handles BtnAddRow.Click
            editcashadvance.AddAmountRequestRow()
        End Sub

        ' ' Handles the Click event of the BtnRemoveRow button
        Private Sub hourlyRate_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles hourlyRate.PreviewTextInput
            Dim pattern As String = "^₱?\d{0,9}(\.\d{0,2})?$"
            editcashadvance.HandlePreviewInputRegex(sender, e, pattern)
        End Sub

        ' ' Handles the TextChanged event of the hourlyRate TextBox
        Private Sub hourlyRate_TextChanged(sender As Object, e As TextChangedEventArgs) Handles hourlyRate.TextChanged
            editcashadvance.RequestAmountFormatting(sender, e, hourlyRate)
        End Sub

        ' ' Loads employee names from the database into the employeeNames list
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
                MessageBox.Show("Error loading employee names: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' ' Handles the TextChanged event of the AutoCompleteTextBox
        Private Sub AutoCompleteTextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim input = AutoCompleteTextBox.Text.Trim().ToLower()
            Dim filtered = employeeNames.Where(Function(name) name.ToLower().Contains(input)).ToList()

            If String.IsNullOrWhiteSpace(AutoCompleteTextBox.Text) Then
                JobTitle.Text = ""
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

        ' ' Handles the PreviewMouseLeftButtonDown event of the SuggestionListBox
        Private Sub SuggestionListBox_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            Dim item = ItemsControl.ContainerFromElement(SuggestionListBox, e.OriginalSource)
            If item IsNot Nothing Then
                Dim selected = CType(item, ListBoxItem).Content.ToString()
                AutoCompleteTextBox.Text = selected
                editcashadvance.FindEmployeeInfo(selected, EmployeeID, Department, JobTitle)
                AutoCompletePopup.IsOpen = False
            End If
        End Sub

        ' ' Handles the PreviewKeyDown event of the AutoCompleteTextBox
        Private Sub AutoCompleteTextBox_PreviewKeyDown(sender As Object, e As System.Windows.Input.KeyEventArgs)
            If e.Key = Key.Down Then
                SuggestionListBox.Focus()
                If SuggestionListBox.Items.Count > 0 Then
                    SuggestionListBox.SelectedIndex = 0
                End If
            End If

            If e.Key = Key.Enter AndAlso SuggestionListBox.Items.Count > 0 Then
                Dim selected = SuggestionListBox.Items(0).ToString()
                AutoCompleteTextBox.Text = selected
                editcashadvance.FindEmployeeInfo(selected, EmployeeID, Department, JobTitle)
                AutoCompletePopup.IsOpen = False
                e.Handled = True
            End If
        End Sub

        ' ' Handles the PreviewKeyDown event of the SuggestionListBox
        Private Sub SuggestionListBox_PreviewKeyDown(sender As Object, e As System.Windows.Input.KeyEventArgs)
            If e.Key = Key.Enter AndAlso SuggestionListBox.SelectedItem IsNot Nothing Then
                Dim selected = SuggestionListBox.SelectedItem.ToString()
                AutoCompleteTextBox.Text = selected
                editcashadvance.FindEmployeeInfo(selected, EmployeeID, Department, JobTitle)
                AutoCompletePopup.IsOpen = False
            End If
        End Sub

        ' ' Handles the Click event of the BtnSaveRequest button
        Private Sub SaveRequest(sender As Object, e As RoutedEventArgs)
            HandleUpdate(2)
        End Sub
        '        ' Handles the Click event of the BtnApproveRequest button
        Private Sub ApproveRequest(sender As Object, e As RoutedEventArgs)
            HandleUpdate(1)
        End Sub

        ' ' Handles the Click event of the BtnRejectRequest button
        Private Sub RejectRequest(sender As Object, e As RoutedEventArgs)
            HandleUpdate(0)
        End Sub

        ' ' Handles the update of the cash advance request based on the approval status
        Private Sub HandleUpdate(IsApproved As Integer)
            If editcashadvance.UpdatetoDB(cacheCARCAID, EmployeeID, AutoCompleteTextBox, CashAdvanceDate, SupervisorName, hourlyRate, TbTotalRequestAmount, approverRemarks, TxtRequestedBy, RequestDate, CbApprover, ApprovalDate, IsApproved) Then
                DynamicView.NavigateToView("managecashadvancerequests", Me)
                ClearInputs()
            Else
                MessageBox.Show("Action has been cancelled.")
            End If
        End Sub

        ' ' Clears all input fields in the form
        Public Sub ClearInputs()
            AutoCompleteTextBox.Text = ""
            SPRequestAmount.Children.Clear()
            CashAdvanceDate.SelectedDate = Date.Today
            RequestDate.SelectedDate = Date.Today
            ApprovalDate.SelectedDate = Date.Today
            CbApprover.SelectedIndex = -1
            TbTotalRequestAmount.Text = "₱ 0.00"
            TxtRequestedBy.Text = ""
            EmployeeID.Text = ""
            Department.Text = ""
            JobTitle.Text = ""
            SupervisorName.Text = ""
            hourlyRate.Text = ""
            approverRemarks.Text = ""
            AutoCompletePopup.IsOpen = False

        End Sub


        ' ' Handles the Click event of the BtnBack button
        Private Sub NavigateBackToManage(sender As Object, e As RoutedEventArgs)
            DynamicView.NavigateToView("managecashadvancerequests", Me)
            ClearInputs()
        End Sub
    End Class

End Namespace
