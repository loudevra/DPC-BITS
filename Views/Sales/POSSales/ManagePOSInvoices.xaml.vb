Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Sales.POSSales
    Public Class ManagePOSInvoices
        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()
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
                End If
            End If
        End Sub
    End Class
End Namespace
