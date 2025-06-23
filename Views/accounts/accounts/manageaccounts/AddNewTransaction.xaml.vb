
Imports DPC.DPC.Data.Controllers


Namespace DPC.Views.Accounts.Accounts.ManageAccounts
    Public Class AddNewTransaction
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
            SetupControllerReferences() ' Ensures viewmodel is connected
        End Sub


        ' Setup DataContext and ViewModel bindings
        Private Sub SetupControllerReferences()
            Dim calendarViewModel As New CalendarController.SingleCalendar()
            calendarViewModel.SelectedDate = Nothing

            ' Set DataContext for the DatePicker and Button
            SingleDatePicker.DataContext = calendarViewModel
            DateButton.DataContext = calendarViewModel
        End Sub

        ' Open the calendar popup
        Private Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            SingleDatePicker.IsDropDownOpen = True
        End Sub

        ' Respond to date selection and update the view model
        Private Sub SingleDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles SingleDatePicker.SelectedDateChanged
            Dim datePicker As DatePicker = TryCast(sender, DatePicker)
            If datePicker IsNot Nothing AndAlso datePicker.DataContext IsNot Nothing Then
                Dim calendarViewModel As CalendarController.SingleCalendar = TryCast(datePicker.DataContext, CalendarController.SingleCalendar)
                If calendarViewModel IsNot Nothing Then
                    calendarViewModel.SelectedDate = datePicker.SelectedDate

                    ' Optional: Force UI to refresh FormattedDate if needed
                    BindingOperations.GetBindingExpression(DateButton, Button.DataContextProperty)?.UpdateTarget()
                End If
            End If
        End Sub

    End Class
End Namespace
