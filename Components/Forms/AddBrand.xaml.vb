Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient
Imports System.Windows.Controls

Namespace DPC.Components.Forms
    Public Class AddBrand
        Public Event BrandAdded()

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub BtnAddBrand(sender As Object, e As RoutedEventArgs)
            BrandController.InsertBrand(TxtBrand.Text)
            RaiseEvent BrandAdded() ' Notify that a brand was added
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
