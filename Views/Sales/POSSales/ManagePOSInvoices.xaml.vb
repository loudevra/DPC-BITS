Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Sales.POSSales
    Public Class ManagePOSInvoices
        Public Property StartDate As New CalendarController.SingleCalendar()
        Public Sub New()
            StartDate.SelectedDate = Date.Today

            DataContext = Me
            ' This call is required by the designer.
            InitializeComponent()
        End Sub
        Private Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            StartDatePicker.IsDropDownOpen = True
        End Sub
    End Class
End Namespace
