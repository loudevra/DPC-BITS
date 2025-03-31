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
            ' Create a new window to host the AddAccount UserControl
            Dim addAccountWindow As New Window With {
                .Title = "Add New Account",
                .Content = New AddAccount(),
                .SizeToContent = SizeToContent.WidthAndHeight,
                .WindowStartupLocation = WindowStartupLocation.CenterOwner,
                .Owner = Application.Current.MainWindow ' Set the owner to the main window
            }

            addAccountWindow.ShowDialog()

            ' Reload the data after adding a new account
            LoadAccounts()
        End Sub

    End Class
End Namespace
