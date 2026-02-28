Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Project
    Partial Public Class AddProject1
        Inherits UserControl

        ' ViewModels for the custom date pickers
        Private startDateViewModel As New CalendarController.SingleCalendar()
        Private dueDateViewModel As New CalendarController.SingleCalendar()

        Public Sub New()
            InitializeComponent()
            SetupDatePickers() ' Initialize the date picker contexts
        End Sub

        ' =========================================================
        ' SECTION 1: TEXT HANDLING (Uppercase Project Name)
        ' =========================================================

        ' Ensure project name is Uppercase while preserving caret position
        Private Sub TxtToUpper_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtName.TextChanged
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return

            Dim originalSelectionStart = tb.SelectionStart
            Dim originalSelectionLength = tb.SelectionLength
            Dim originalText = tb.Text

            Dim upperText = originalText.ToUpperInvariant()

            ' Only update if there is a change to avoid infinite loops
            If Not String.Equals(originalText, upperText, StringComparison.Ordinal) Then
                RemoveHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
                tb.Text = upperText
                tb.SelectionStart = Math.Min(originalSelectionStart, tb.Text.Length)
                tb.SelectionLength = originalSelectionLength
                AddHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
            End If
        End Sub

        ' =========================================================
        ' SECTION 2: DATE PICKER LOGIC
        ' =========================================================

        ' Setup bindings between DatePickers, Buttons, and ViewModels
        Public Sub SetupDatePickers()
            startDateViewModel.SelectedDate = Nothing
            dueDateViewModel.SelectedDate = Nothing

            ' Bind the DataContexts for the hidden pickers and visible buttons
            StartDatePicker.DataContext = startDateViewModel
            StartDateButton.DataContext = startDateViewModel

            DueDatePicker.DataContext = dueDateViewModel
            DueDateButton.DataContext = dueDateViewModel
        End Sub

        ' Open the hidden DatePicker dropdown when the custom button is clicked
        Private Sub StartDateButton_Click(sender As Object, e As RoutedEventArgs) Handles StartDateButton.Click
            StartDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub DueDateButton_Click(sender As Object, e As RoutedEventArgs) Handles DueDateButton.Click
            DueDatePicker.IsDropDownOpen = True
        End Sub

        ' Sync the ViewModel when the Start Date changes
        Private Sub StartDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles StartDatePicker.SelectedDateChanged
            Dim dp = TryCast(sender, DatePicker)
            If dp IsNot Nothing AndAlso dp.DataContext IsNot Nothing Then
                Dim vm = TryCast(dp.DataContext, CalendarController.SingleCalendar)
                If vm IsNot Nothing Then
                    vm.SelectedDate = dp.SelectedDate
                    ' Force the button binding to update its text
                    Dim be = BindingOperations.GetBindingExpression(StartDateButton, Button.DataContextProperty)
                    If be IsNot Nothing Then be.UpdateTarget()
                End If
            End If
        End Sub

        ' Sync the ViewModel when the Due Date changes
        Private Sub DueDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles DueDatePicker.SelectedDateChanged
            Dim dp = TryCast(sender, DatePicker)
            If dp IsNot Nothing AndAlso dp.DataContext IsNot Nothing Then
                Dim vm = TryCast(dp.DataContext, CalendarController.SingleCalendar)
                If vm IsNot Nothing Then
                    vm.SelectedDate = dp.SelectedDate
                    ' Force the button binding to update its text
                    Dim be = BindingOperations.GetBindingExpression(DueDateButton, Button.DataContextProperty)
                    If be IsNot Nothing Then be.UpdateTarget()
                End If
            End If
        End Sub

        ' =========================================================
        ' SECTION 3: SUBMIT LOGIC
        ' =========================================================

        Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
            ' Logic for adding the project goes here
            ' Example: 
            ' Dim projectName = txtName.Text
            ' Dim customer = txtCustomer.Text
            ' ... Save to database ...

            ' Navigate away after saving (Optional)
            ' ViewLoader.DynamicView.NavigateToView("projectlist", Me)
        End Sub

        Private Sub cmbStatus_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles cmbStatus.SelectionChanged

        End Sub
    End Class
End Namespace