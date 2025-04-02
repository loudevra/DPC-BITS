Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Models

Namespace DPC.Components.Forms
    Public Class AddSubcategory

        Public Event SubCategoryAdded As EventHandler
        Public Sub New()
            InitializeComponent()
            LoadCategories()
        End Sub

        ' Function to load categories into the ComboBox
        Private Sub LoadCategories()
            ProductCategoryController.GetProductCategory(ComboBoxCategory)
        End Sub
    End Class
End Namespace
