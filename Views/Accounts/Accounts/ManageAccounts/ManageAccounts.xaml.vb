Imports System.Collections.ObjectModel
Imports System.Data
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.Accounts.Accounts.NewAccount
Imports MaterialDesignThemes.Wpf

Namespace DPC.Views.Accounts.Accounts.ManageAccounts
    Partial Public Class ManageAccounts
        Inherits Window
        Public Property AccountsList As ObservableCollection(Of Account)

        Public Sub New()
            InitializeComponent()

            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar

            ' Add TopNavBar to TopNavBarContainer
            Dim topNavBar As New TopNavBar()
            TopNavBarContainer.Child = topNavBar

            ' Load Account Data
            LoadAccounts()
        End Sub

        ' Load Accounts Data
        Public Sub LoadAccounts()
            Try
                AccountsDataGrid.ItemsSource = AccountController.GetAllAccounts()
            Catch ex As Exception
                MessageBox.Show("Error loading accounts: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Event Handler for Add Account Button Click
        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs)
            ' Load the NewAccount UserControl
            Dim newAccountControl As New AddAccount
            newAccountControl.ShowDialog()

            ' Attach an event handler to reload the data after adding the account
            AddHandler newAccountControl.AccountAdded, AddressOf OnAccountAdded
        End Sub

        ' Handle the AccountAdded event to refresh the accounts list
        Private Sub OnAccountAdded()
            LoadAccounts()
            AccountController.Reload = False ' Reset the flag after reloading
        End Sub



    End Class
End Namespace
