Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Controllers

Namespace DPC.Views.Accounts.Accounts.ManageAccounts
    Partial Public Class ManageAccounts
        Inherits Window

        Public Sub New()
            InitializeComponent()

            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar

            ' Add TopNavBar to TopNavBarContainer
            Dim topNavBar As New TopNavBar()
            TopNavBarContainer.Child = topNavBar

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
            Dim addAccountWindow As New DPC.Views.Accounts.Accounts.ManageAccounts.AddAccount()

            ' Subscribe to the event to reload data after adding a category
            AddHandler addAccountWindow.AccountAdded, AddressOf OnAccountAdd

            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, addAccountWindow, "windowcenter", -50, 0, Me)
        End Sub

        Private Sub OnAccountAdd(sender As Object, e As EventArgs)
            LoadAccounts() ' Reloads the subcategories in the main view
        End Sub
    End Class
End Namespace
