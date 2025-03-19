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

            If CategoryComboBox.SelectedItem IsNot Nothing Then
                CategoryComboBox_SelectionChanged(CategoryComboBox, Nothing)
            End If
        End Sub


        Private Sub CategoryComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CategoryComboBox.SelectionChanged
            Dim selectedCategory As String = TryCast(CategoryComboBox.SelectedItem, ComboBoxItem)?.Content?.ToString()
            If Not String.IsNullOrEmpty(selectedCategory) Then
                ProductController.GetProductSubcategory(selectedCategory, SubCategoryComboBox)
            Else
                SubCategoryComboBox.Items.Clear()
            End If
        End Sub
    End Class
End Namespace
