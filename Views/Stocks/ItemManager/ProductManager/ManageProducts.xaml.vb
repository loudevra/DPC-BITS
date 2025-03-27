Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model
Imports MaterialDesignThemes.Wpf.Theme

Namespace DPC.Views.Stocks.ItemManager.ProductManager
    Public Class ManageProducts
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
            ProductController.LoadProductData(dataGrid)
        End Sub
    End Class
End Namespace


