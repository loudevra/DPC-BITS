Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Components
Imports DPC.DPC.Views.Warehouse

Namespace DPC.Views.Stocks.Warehouses
    Public Class Warehouses
        Inherits Window

        Public Sub New()
            InitializeComponent()

            ' Load Sidebar
            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Child = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Child = topNav

            ' Load Data
            LoadData()
        End Sub

        ' Load Warehouse Data
        Public Sub LoadData()
            dataGrid.ItemsSource = WarehouseController.GetWarehouses()
        End Sub

        ' Event Handler for Add Warehouse Button Click
        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs)
            Dim addWarehousePopup As New AddWarehouse With {
        .Owner = Me ' Set parent window
    }
            addWarehousePopup.ShowDialog() ' Show as modal popup

            ' Check if reload flag is set and refresh the data
            If WarehouseController.Reload Then
                LoadData()
                WarehouseController.Reload = False ' Reset the flag after reloading
            End If
        End Sub
    End Class
End Namespace
