Imports System.Windows
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Components

Namespace DPC.Views.Stocks.ItemManager.NewProduct
    Public Class AddNewProducts
        Inherits Window

        Public Sub New()
            InitializeComponent()

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNav

            ProductController.GetProductCategory(CategoryComboBox)
        End Sub

        Private Sub btnAddRow_Click(sender As Object, e As RoutedEventArgs)
            ' Open the second form
            Dim secondForm As New AddNewProductSecondForm()
            secondForm.Show()
        End Sub

    End Class
End Namespace
