
Imports System.Text.RegularExpressions
Imports System.Threading
Imports DPC.DPC.Data.Controllers
Imports MahApps.Metro.Controls
Imports MaterialDesignThemes.Wpf
Imports MySql.Data.MySqlClient
Imports NuGet.ContentModel
Imports OfficeOpenXml.Drawing.Controls
Imports Org.BouncyCastle.Asn1.Ocsp
Imports DPC.DPC.Data.Controllers.Misc
Imports System.Windows.Input
Imports System.Windows.MessageBox

Namespace DPC.Views.Misc.CashAdvance
    Public Class CashAdvanceNewRequest

        '        ' Interaction logic for CashAdvanceNewRequest.xaml
        Private employeeNames As New List(Of String)

        '        ' Controller for handling cash advance requests
        Public newcashadvance As NewCashAdvanceController


        '        ' Properties for binding to the view
        Public Property CashAdvanceDate As New CalendarController.SingleCalendar()
        Public Property ApprovalDate As New CalendarController.SingleCalendar()
        Public Property RequestDate As New CalendarController.SingleCalendar()

        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()
            LoadEmployeeNames()
            DataContext = Me
            newcashadvance = New NewCashAdvanceController(Me)
            CashAdvanceDate.SelectedDate = Date.Today
            CashAdvanceDatePicker.DisplayDateStart = Date.Today
            ApprovalDate.SelectedDate = Date.Today
            ApprovalDatePicker.DisplayDateStart = Date.Today
            RequestDate.SelectedDate = Date.Today
            RequestDatePicker.DisplayDateStart = Date.Today
        End Sub

        Public Sub CashAdvanceNewRequest_Loaded(sender As Object, e As RoutedEventArgs)
            'adds the first row for the amount request
            newcashadvance.AddAmountRequestRow()
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

        Private Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs) Handles BtnAddRow.Click
            ' Adds a new row for the amount request
            newcashadvance.AddAmountRequestRow()
        End Sub

        Private Sub hourlyRate_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles hourlyRate.PreviewTextInput
            ' Handles the preview text input for the hourly rate TextBox
            Dim pattern As String = "^₱?\d{0,9}(\.\d{0,2})?$"
            newcashadvance.HandlePreviewInputRegex(sender, e, pattern)
        End Sub

        Private Sub hourlyRate_TextChanged(sender As Object, e As TextChangedEventArgs) Handles hourlyRate.TextChanged
            ' Formats the hourly rate input and updates the total request amount
            newcashadvance.RequestAmountFormatting(sender, e, hourlyRate)
        End Sub


        '        ' Loads employee names from the database for the autocomplete feature
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

        '        ' Handles the text changed event for the autocomplete TextBox
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


        '        ' Handles the mouse left button down event for the suggestion ListBox
        Private Sub SuggestionListBox_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            Dim item = ItemsControl.ContainerFromElement(SuggestionListBox, e.OriginalSource)
            If item IsNot Nothing Then
                Dim selected = CType(item, ListBoxItem).Content.ToString()
                AutoCompleteTextBox.Text = selected
                newcashadvance.FindEmployeeInfo(selected, EmployeeID, Department, JobTitle)
                AutoCompletePopup.IsOpen = False
            End If
        End Sub


        '        ' Handles the key down event for the autocomplete TextBox
        Private Sub AutoCompleteTextBox_PreviewKeyDown(sender As Object, e As System.Windows.Input.KeyEventArgs)
            If e.Key = Key.Down Then
                SuggestionListBox.Focus()
                If SuggestionListBox.Items.Count > 0 Then
                    SuggestionListBox.SelectedIndex = 0
                End If
            End If

            ' If the Enter key is pressed and there are suggestions, select the first suggestion
            If e.Key = Key.Enter AndAlso SuggestionListBox.Items.Count > 0 Then
                Dim selected = SuggestionListBox.Items(0).ToString()
                AutoCompleteTextBox.Text = selected
                newcashadvance.FindEmployeeInfo(selected, EmployeeID, Department, JobTitle)
                AutoCompletePopup.IsOpen = False
                e.Handled = True
            End If
        End Sub


        '        ' Handles the preview key down event for the suggestion ListBox
        Private Sub SuggestionListBox_PreviewKeyDown(sender As Object, e As System.Windows.Input.KeyEventArgs)
            If e.Key = Key.Enter AndAlso SuggestionListBox.SelectedItem IsNot Nothing Then
                Dim selected = SuggestionListBox.SelectedItem.ToString()
                AutoCompleteTextBox.Text = selected
                newcashadvance.FindEmployeeInfo(selected, EmployeeID, Department, JobTitle)
                AutoCompletePopup.IsOpen = False
            End If
        End Sub

        '        ' Handles the click event for the submit button
        Private Sub SubmitRequest(sender As Object, e As RoutedEventArgs) Handles BtnSubmit.Click
            If newcashadvance.HasAnyRowWithEmptyTextBox() Or String.IsNullOrWhiteSpace(AutoCompleteTextBox.Text) Or String.IsNullOrWhiteSpace(TxtRequestedBy.Text) Then
                MessageBox.Show("All inputs in the request must be filled out before submitting.", "Information", MessageBoxButton.OK, MessageBoxImage.Error)
            Else
                Dim result As MessageBoxResult = MessageBox.Show(
                    "Please note that this request will need approval. The 'Approved By' as well as 'Remarks' field will be ignored upon submission.",
                    "Information",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Information
                )

                If result = MessageBoxResult.OK Then

                    ' Get the values from the input fields
                    Dim EmployID As String = EmployeeID.Text.Trim().ToString()
                    Dim caDate As String = If(CashAdvanceDate.SelectedDate.HasValue,
                          CashAdvanceDate.SelectedDate.Value.ToString("yyyy-MM-dd"),
                          "")
                    Dim supervisor As String = If(String.IsNullOrWhiteSpace(SupervisorName.Text), "", SupervisorName.Text.Trim().ToString())
                    Dim rate As String = If(String.IsNullOrWhiteSpace(hourlyRate.Text), "", hourlyRate.Text.Trim().ToString())
                    Dim requestInfo As String = newcashadvance.GetEntryAsJson()
                    Dim requestTotal As String = TbTotalRequestAmount.Text.Trim().ToString()
                    Dim requestBy As String = TxtRequestedBy.Text.Trim().ToString()
                    Dim reqDate As String = If(RequestDate.SelectedDate.HasValue,
                           RequestDate.SelectedDate.Value.ToString("yyyy-MM-dd"),
                           "")

                    'Dim approvedBy As String
                    'CbApprover.SelectedIndex = 0

                    'Dim selectedItem = TryCast(CbApprover.SelectedItem, ComboBoxItem)
                    'If selectedItem IsNot Nothing Then
                    '    approvedBy = selectedItem.Content.ToString()
                    'Else
                    '    approvedBy = ""
                    'End If


                    ' Generate a unique reference for the cash advance request
                    Dim caRef As String = newcashadvance.GenerateCashAdvanceRef(EmployID)

                    MessageBox.Show(caRef)
                    'MessageBox.Show(requestInfo, "Request Information", MessageBoxButton.OK, MessageBoxImage.Information)
                    If newcashadvance.InsertRequestToDb(caRef, EmployID, caDate, supervisor, rate, requestInfo, requestTotal, requestBy, reqDate) Then
                        MessageBox.Show("Request submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                        ClearInputs()
                    End If
                Else
                    MessageBox.Show("Request Cancelled")
                End If
            End If
        End Sub


        '        ' Clears all input fields and resets the form
        Private Sub ClearInputs()
            AutoCompleteTextBox.Text = ""
            SPRequestAmount.Children.Clear()
            newcashadvance.RequestAmountRows.Clear()
            newcashadvance.AddAmountRequestRow()
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

    End Class


End Namespace
