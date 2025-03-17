Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Components

Namespace DPC.Views.Stocks.Suppliers.ManageSuppliers
    Public Class ManageSuppliers
        Inherits Window

        Public Sub New()
            InitializeComponent()

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Child = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Child = topNav

            LoadData()
        End Sub

        ' Load Data Using SupplierController
        Public Sub LoadData()
            dataGrid.ItemsSource = SupplierController.GetSuppliers()
        End Sub
    End Class
End Namespace
