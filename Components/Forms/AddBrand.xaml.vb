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
            ' Validate brand name
            If String.IsNullOrWhiteSpace(TxtBrand.Text) Then
                MessageBox.Show("Please enter a brand name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' Validate category selection
            If CmbCategory.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a category.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' Get categoryID from Tag (this is how GetProductCategory stores the ID)
            Dim selectedItem As ComboBoxItem = TryCast(CmbCategory.SelectedItem, ComboBoxItem)
            If selectedItem Is Nothing Then Return

            Dim categoryID As Integer = Convert.ToInt32(selectedItem.Tag)
            BrandController.InsertBrand(TxtBrand.Text, categoryID)
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