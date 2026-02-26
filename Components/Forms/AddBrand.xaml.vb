Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient
Imports System.Windows.Controls

Namespace DPC.Components.Forms
    Public Class AddBrand
        Public Event BrandAdded()

        Private ProductController As New ProductController()

        Public Sub New()
            InitializeComponent()
            ProductController.GetProductCategory(CmbCategory)
        End Sub

        Private Sub BtnAddBrand(sender As Object, e As RoutedEventArgs)
            BrandController.InsertBrand(TxtBrand.Text)
            RaiseEvent BrandAdded()
        End Sub

        Private Sub TxtBrand_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return
            Dim original = tb.Text
            Dim upper = original.ToUpperInvariant()
            If original = upper Then Return
            Dim selStart = tb.SelectionStart
            Dim selLength = tb.SelectionLength
            RemoveHandler tb.TextChanged, AddressOf TxtBrand_TextChanged
            tb.Text = upper
            tb.SelectionStart = Math.Min(selStart, tb.Text.Length)
            tb.SelectionLength = selLength
            AddHandler tb.TextChanged, AddressOf TxtBrand_TextChanged
        End Sub

    End Class
End Namespace