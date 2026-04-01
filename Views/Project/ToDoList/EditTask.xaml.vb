Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports System.Windows.Data
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers '<-- Added missing import here!

Public Class EditTask
    Inherits UserControl

    Private startDateViewModel As New CalendarController.SingleCalendar()
    Private dueDateViewModel As New CalendarController.SingleCalendar()

    Public Sub New()
        InitializeComponent()
        SetupDatePickers()

        Dim statuses As New List(Of StatusItem) From {
            New StatusItem With {.Label = "Due", .Color = New SolidColorBrush(Color.FromRgb(199, 87, 87))},
            New StatusItem With {.Label = "Progress", .Color = New SolidColorBrush(Color.FromRgb(134, 188, 213))},
            New StatusItem With {.Label = "Done", .Color = New SolidColorBrush(Color.FromRgb(137, 172, 116))}
        }

        cmbStatus.ItemsSource = statuses
        AddHandler Me.Loaded, AddressOf EditTask_Loaded
    End Sub

    ' This automatically fills the form with the task you clicked "Edit" on!
    Private Sub EditTask_Loaded(sender As Object, e As RoutedEventArgs)
        Dim taskToEdit = GlobalTaskStore.TaskToEdit
        If taskToEdit IsNot Nothing Then
            txtName.Text = taskToEdit.Task

            ' Set correct status dropdown
            For Each item As StatusItem In cmbStatus.Items
                If item.Label = taskToEdit.Status Then
                    cmbStatus.SelectedItem = item
                    Exit For
                End If
            Next

            ' Load dates back into the calendars
            If Not String.IsNullOrWhiteSpace(taskToEdit.Start) AndAlso taskToEdit.Start <> "-" Then
                Dim sDate As DateTime
                If DateTime.TryParse(taskToEdit.Start, sDate) Then
                    StartDatePicker.SelectedDate = sDate
                End If
            End If

            If Not String.IsNullOrWhiteSpace(taskToEdit.DueDate) AndAlso taskToEdit.DueDate <> "-" Then
                Dim dDate As DateTime
                If DateTime.TryParse(taskToEdit.DueDate, dDate) Then
                    DueDatePicker.SelectedDate = dDate
                End If
            End If
        End If
    End Sub

    Public Sub SetupDatePickers()
        StartDatePicker.DataContext = startDateViewModel
        StartDateButton.DataContext = startDateViewModel
        DueDatePicker.DataContext = dueDateViewModel
        DueDateButton.DataContext = dueDateViewModel
    End Sub

    Private Sub BtnSaveChanges_Click(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrWhiteSpace(txtName.Text) Then
            MessageBox.Show("Task Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        Dim taskToUpdate = GlobalTaskStore.TaskToEdit
        If taskToUpdate IsNot Nothing Then
            ' 1. Update the values
            taskToUpdate.Task = txtName.Text
            Dim selectedStatus = TryCast(cmbStatus.SelectedItem, StatusItem)
            taskToUpdate.Status = If(selectedStatus IsNot Nothing, selectedStatus.Label, "Due")

            taskToUpdate.Start = If(StartDatePicker.SelectedDate.HasValue, StartDatePicker.SelectedDate.Value.ToString("MMM dd, yyyy"), "-")
            taskToUpdate.DueDate = If(DueDatePicker.SelectedDate.HasValue, DueDatePicker.SelectedDate.Value.ToString("MMM dd, yyyy"), "-")

            ' 2. Replace the item in the list so the ManageTask datagrid updates visually
            Dim idx = GlobalTaskStore.TaskList.IndexOf(taskToUpdate)
            If idx >= 0 Then
                GlobalTaskStore.TaskList(idx) = taskToUpdate
            End If

            MessageBox.Show("Task updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

            ' 3. Navigate back to the Manage Tasks view (using shortened, correct path)
            Try
                ViewLoader.DynamicView.NavigateToView("managetask", Me)
            Catch ex As Exception
            End Try
        End If
    End Sub

    Private Sub TxtToUpper_TextChanged(sender As Object, e As Controls.TextChangedEventArgs)
        Dim tb = TryCast(sender, TextBox)
        If tb Is Nothing Then Return

        Dim upperText = tb.Text.ToUpperInvariant()
        If Not String.Equals(tb.Text, upperText, StringComparison.Ordinal) Then
            RemoveHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
            Dim caret = tb.SelectionStart
            tb.Text = upperText
            tb.SelectionStart = Math.Min(caret, tb.Text.Length)
            AddHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
        End If
    End Sub

    ' =========================================================
    ' DATE PICKER CLICK HANDLERS (With Past-Date Restrictions)
    ' =========================================================
    Private Sub StartDateButton_Click(sender As Object, e As RoutedEventArgs) Handles StartDateButton.Click
        Dim minDate As DateTime = DateTime.Today
        If StartDatePicker.SelectedDate.HasValue AndAlso StartDatePicker.SelectedDate.Value < DateTime.Today Then
            minDate = StartDatePicker.SelectedDate.Value
        End If

        StartDatePicker.DisplayDateStart = minDate
        StartDatePicker.IsDropDownOpen = True
    End Sub

    Private Sub DueDateButton_Click(sender As Object, e As RoutedEventArgs) Handles DueDateButton.Click
        Dim minDate As DateTime = DateTime.Today
        If DueDatePicker.SelectedDate.HasValue AndAlso DueDatePicker.SelectedDate.Value < DateTime.Today Then
            minDate = DueDatePicker.SelectedDate.Value
        End If

        DueDatePicker.DisplayDateStart = minDate
        DueDatePicker.IsDropDownOpen = True
    End Sub

    Private Sub StartDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim dp = TryCast(sender, DatePicker)
        If dp IsNot Nothing AndAlso dp.DataContext IsNot Nothing Then
            Dim vm = TryCast(dp.DataContext, CalendarController.SingleCalendar)
            If vm IsNot Nothing Then
                vm.SelectedDate = dp.SelectedDate
                Dim be = BindingOperations.GetBindingExpression(StartDateButton, Button.DataContextProperty)
                If be IsNot Nothing Then be.UpdateTarget()
            End If
        End If
    End Sub

    Private Sub DueDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim dp = TryCast(sender, DatePicker)
        If dp IsNot Nothing AndAlso dp.DataContext IsNot Nothing Then
            Dim vm = TryCast(dp.DataContext, CalendarController.SingleCalendar)
            If vm IsNot Nothing Then
                vm.SelectedDate = dp.SelectedDate
                Dim be = BindingOperations.GetBindingExpression(DueDateButton, Button.DataContextProperty)
                If be IsNot Nothing Then be.UpdateTarget()
            End If
        End If
    End Sub
End Class