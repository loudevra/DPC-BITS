Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Sales.Subscriptions
    Public Class NewSubscriptionInvoice
        Public Property SubscriptionDate As New CalendarController.SingleCalendar()
        Public Sub New()
            SubscriptionDate.SelectedDate = Date.Today

            DataContext = Me

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

        End Sub
        Private Sub SubscriptionDate_Click(sender As Object, e As RoutedEventArgs)
            SubscriptionDatePicker.IsDropDownOpen = True
        End Sub
    End Class
End Namespace

