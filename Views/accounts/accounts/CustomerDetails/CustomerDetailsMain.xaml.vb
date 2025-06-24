Namespace DPC.Views.Accounts.Accounts
    Public Class CustomerDetailsMain
        Inherits UserControl ' ✅ Ensure this is inheriting from UserControl (if not already in designer)

        Public Sub New()
            InitializeComponent()

            ' ✅ Automatically load Personal Information on view open
            customerdetailsmaincontent.Content = New CustomerDetailsPersonalInformation()
        End Sub

        ' ✅ These must have Handles if defined in XAML
        Private Sub BtnInvoice_Click(sender As Object, e As RoutedEventArgs) Handles BtnInvoice.Click
            customerdetailsmaincontent.Content = New CustomerDetailsInvoices()
        End Sub

        Private Sub BtnInfo_Click(sender As Object, e As RoutedEventArgs) Handles BtnInfo.Click
            customerdetailsmaincontent.Content = New CustomerDetailsPersonalInformation()
        End Sub

        Private Sub BtnTransactions_Click(sender As Object, e As RoutedEventArgs) Handles BtnTransactions.Click
            customerdetailsmaincontent.Content = New CustomerDetailsTransaction()
        End Sub

        Private Sub BtnQuotes_Click(sender As Object, e As RoutedEventArgs) Handles BtnQuotes.Click
            customerdetailsmaincontent.Content = New CustomerDetailsQuotes()
        End Sub

        Private Sub BtnSubscriptions_Click(sender As Object, e As RoutedEventArgs) Handles BtnSubscriptions.Click
            customerdetailsmaincontent.Content = New CustomerDetailsSubscriptions()
        End Sub

        Private Sub BtnProjects_Click(sender As Object, e As RoutedEventArgs) Handles BtnProjects.Click
            customerdetailsmaincontent.Content = New CustomerDetailsProjects()
        End Sub

        Private Sub BtnNotes_Click(sender As Object, e As RoutedEventArgs) Handles BtnNotes.Click
            customerdetailsmaincontent.Content = New CustomerDetailsAccountStatement()
        End Sub

        Private Sub BtnDocuments_Click(sender As Object, e As RoutedEventArgs) Handles BtnDocuments.Click
            customerdetailsmaincontent.Content = New CustomerDetailsAccountStatement()
        End Sub

        Private Sub BtnAccountStatement_Click(sender As Object, e As RoutedEventArgs) Handles BtnAccountStatement.Click
            customerdetailsmaincontent.Content = New CustomerDetailsAccountStatement()
        End Sub

        Private Sub DetailsWallet_Click(sender As Object, e As RoutedEventArgs) Handles CustomerDetailsWallet.Click
            customerdetailsmaincontent.Content = New CustomerDetailsWallet()
        End Sub
    End Class
End Namespace