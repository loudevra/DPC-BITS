Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Views.Accounts.Accounts.CustomerDetails.CustomerDetailsCards1

Namespace DPC.Views.Accounts.Accounts.CustomerDetails

    Public Class CustomerDetailsMain
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub BtnInvoice_Click(sender As Object, e As RoutedEventArgs) Handles BtnInvoice.Click
            ' Load the Invoice view inside the right panel
            customerdetailsmaincontent.Content = New CustomerDetailsInvoices()
        End Sub

        Private Sub BtnInfo_Click(sender As Object, e As RoutedEventArgs) Handles BtnInfo.Click
            customerdetailsmaincontent.Content = New CustomerDetailsAccountStatement()
        End Sub

        Private Sub BtnTransactions_Click(sender As Object, e As RoutedEventArgs)
            customerdetailsmaincontent.Content = New CustomerDetailsTransaction()
        End Sub

        Private Sub BtnQuotes_Click(sender As Object, e As RoutedEventArgs)
            ' Load the Quotes view inside the right panel
            customerdetailsmaincontent.Content = New CustomerDetailsQuotes()
        End Sub

        Private Sub BtnSubscriptions_Click(sender As Object, e As RoutedEventArgs)
            customerdetailsmaincontent.Content = New CustomerDetailsSubscriptions()
        End Sub

        Private Sub BtnProjects_Click(sender As Object, e As RoutedEventArgs)
            customerdetailsmaincontent.Content = New CustomerDetailsProjects()
        End Sub

        Private Sub BtnNotes_Click(sender As Object, e As RoutedEventArgs)
            customerdetailsmaincontent.Content = New CustomerDetailsAccountStatement()
        End Sub

        Private Sub BtnDocuments_Click(sender As Object, e As RoutedEventArgs)
            customerdetailsmaincontent.Content = New CustomerDetailsAccountStatement()
        End Sub

        Private Sub BtnAccountStatement_Click(sender As Object, e As RoutedEventArgs)
            customerdetailsmaincontent.Content = New CustomerDetailsAccountStatement()
        End Sub


        Private Sub DetailsWallet_Click(sender As Object, e As RoutedEventArgs)
            customerdetailsmaincontent.Content = New CustomerDetailsWallet()
        End Sub
    End Class

End Namespace
