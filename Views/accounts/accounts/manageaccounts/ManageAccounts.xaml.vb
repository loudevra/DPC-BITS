Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Accounts.Accounts.ManageAccounts
    Partial Public Class ManageAccounts
        Inherits UserControl

        ' Event to notify parent container when an account is added
        Public Event AccountAdded As EventHandler

        Public Sub New()
            InitializeComponent()
            ' Load Accounts data into DataGrid
            LoadAccounts()
        End Sub

        Private Sub LoadAccounts()
            Try
                Dim accounts = AccountController.GetAllAccounts()
                AccountsDataGrid.ItemsSource = accounts
            Catch ex As Exception
                MessageBox.Show("Error loading accounts: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub AddAccount_Click(sender As Object, e As RoutedEventArgs)
            Dim addAccountWindow As New AddAccount()

            Dim parentWindow As Window = Window.GetWindow(Me)
            ' Subscribe to the event to reload data after adding a category
            AddHandler addAccountWindow.AccountAdded, AddressOf OnAccountAdd
            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, addAccountWindow, "windowcenter", -50, 0, True, parentWindow)

        End Sub

        Private Sub OnAccountAdd(sender As Object, e As EventArgs)
            LoadAccounts() ' Reloads the accounts in the main view
            RaiseEvent AccountAdded(Me, New EventArgs()) ' Notify parent that account was added
        End Sub
    End Class
End Namespace