Namespace DPC.Components.UI
    Public Class PopUpMenuSales
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub NavigateToNewInvoice(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigate to New Invoice", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        Private Sub NavigateToNewInvoiceV2(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigate to New Invoice V2 - Mobile", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        Private Sub NavigateToManageInvoices(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigate to Manage Invoices", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        Private Sub NavigateToNewQuote(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigate to New Quote", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        Private Sub NavigateToManageQuote(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigate to Manage Quote", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        Private Sub NavigateToNewSubscription(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigate to New Subscription", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        Private Sub NavigateToSubscriptions(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigate to Subscriptions", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

    End Class
End Namespace
