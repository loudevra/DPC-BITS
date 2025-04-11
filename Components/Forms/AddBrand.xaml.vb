Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient

Namespace DPC.Components.Forms
    Public Class AddBrand
        Public Event BrandAdded()

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub BtnAddBrand()
            BrandController.InsertBrand(TxtBrand.Text)
            RaiseEvent BrandAdded() ' Notify that a brand was added
        End Sub


    End Class
End Namespace
