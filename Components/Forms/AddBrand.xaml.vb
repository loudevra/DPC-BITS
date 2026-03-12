' AddBrand.xaml.vb
Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input

Namespace DPC.Components.Forms
    Public Class AddBrand
        Public Event BrandAdded()
        Private ProductController As New ProductController()
        Private categoryItems As New List(Of ComboBoxItem)()
        Private subcategoryItems As New List(Of ComboBoxItem)()

        Public Sub New()
            InitializeComponent()
            ProductController.GetProductCategory(CmbCategory)
            SnapshotCategoryItems()

            CmbCategory.IsEditable = True
            CmbCategory.IsTextSearchEnabled = False
            CmbCategory.StaysOpenOnEdit = True

            CmbSubCategory.IsEditable = True
            CmbSubCategory.IsTextSearchEnabled = False
            CmbSubCategory.StaysOpenOnEdit = True

            AddHandler CmbCategory.PreviewTextInput, AddressOf CmbCategory_Filter
            AddHandler CmbSubCategory.PreviewTextInput, AddressOf CmbSubCategory_Filter
        End Sub

        ' Walks up the logical tree to find the Popup hosting this AddBrand form
        Private Function FindParentPopup() As Popup
            Dim current As DependencyObject = Me
            While current IsNot Nothing
                Dim p = TryCast(current, Popup)
                If p IsNot Nothing Then Return p
                current = LogicalTreeHelper.GetParent(current)
            End While
            Return Nothing
        End Function

        Private Sub SnapshotCategoryItems()
            categoryItems.Clear()
            For Each item As ComboBoxItem In CmbCategory.Items
                categoryItems.Add(item)
            Next
        End Sub

        Private Sub SnapshotSubcategoryItems()
            subcategoryItems.Clear()
            For Each item As ComboBoxItem In CmbSubCategory.Items
                subcategoryItems.Add(item)
            Next
        End Sub

        Private Sub CmbCategory_Filter(sender As Object, e As TextCompositionEventArgs)
            CmbCategory.IsDropDownOpen = True
            Dim tb = TryCast(CmbCategory.Template.FindName("PART_EditableTextBox", CmbCategory), TextBox)
            Dim filterText = If(tb IsNot Nothing, tb.Text & e.Text, e.Text).ToUpperInvariant()

            CmbCategory.Items.Clear()
            For Each item In categoryItems
                If item.Content.ToString().ToUpperInvariant().Contains(filterText) Then
                    CmbCategory.Items.Add(item)
                End If
            Next
        End Sub

        Private Sub CmbSubCategory_Filter(sender As Object, e As TextCompositionEventArgs)
            CmbSubCategory.IsDropDownOpen = True
            Dim tb = TryCast(CmbSubCategory.Template.FindName("PART_EditableTextBox", CmbSubCategory), TextBox)
            Dim filterText = If(tb IsNot Nothing, tb.Text & e.Text, e.Text).ToUpperInvariant()

            CmbSubCategory.Items.Clear()
            For Each item In subcategoryItems
                If item.Content.ToString().ToUpperInvariant().Contains(filterText) Then
                    CmbSubCategory.Items.Add(item)
                End If
            Next
        End Sub

        Private Sub CmbCategory_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CmbCategory.SelectionChanged
            Dim selectedCategory As String = TryCast(CmbCategory.SelectedItem, ComboBoxItem)?.Content?.ToString()
            If Not String.IsNullOrEmpty(selectedCategory) Then
                ProductController.GetProductSubcategory(selectedCategory, CmbSubCategory, SubCategoryLabel, New StackPanel())
                SnapshotSubcategoryItems()
            Else
                CmbSubCategory.Items.Clear()
                subcategoryItems.Clear()
            End If
        End Sub

        Private Sub BtnAddBrand(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrWhiteSpace(TxtBrand.Text) Then
                MessageBox.Show("Please enter a brand name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            If CmbCategory.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a category.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim selectedItem As ComboBoxItem = TryCast(CmbCategory.SelectedItem, ComboBoxItem)
            If selectedItem Is Nothing Then Return
            Dim categoryID As Integer = Convert.ToInt32(selectedItem.Tag)

            Dim subCategoryID As Integer = 0
            Dim selectedSubItem As ComboBoxItem = TryCast(CmbSubCategory.SelectedItem, ComboBoxItem)
            If selectedSubItem IsNot Nothing AndAlso selectedSubItem.Tag IsNot Nothing Then
                subCategoryID = Convert.ToInt32(selectedSubItem.Tag)
            End If

            BrandController.InsertBrand(TxtBrand.Text, categoryID, subCategoryID)
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

        Private Sub BtnAddCategory_Click(sender As Object, e As RoutedEventArgs)
            Dim addNewCategory As New DPC.Components.Forms.AddCategory()

            Dim win As New Window() With {
                .Content = addNewCategory,
                .SizeToContent = SizeToContent.WidthAndHeight,
                .WindowStyle = WindowStyle.None,
                .AllowsTransparency = True,
                .Background = Brushes.Transparent,
                .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                .ResizeMode = ResizeMode.NoResize,
                .ShowInTaskbar = False,
                .Topmost = True
            }

            AddHandler addNewCategory.CategoryAdded, Sub(s As Object, ev As EventArgs)
                                                         ProductController.GetProductCategory(CmbCategory)
                                                         SnapshotCategoryItems()
                                                         win.Close()
                                                     End Sub

            ' Pin the AddBrand popup so it does not close when focus shifts to the new Window
            Dim parentPopup As Popup = FindParentPopup()
            If parentPopup IsNot Nothing Then parentPopup.StaysOpen = True

            win.ShowDialog()

            ' Restore normal behavior after the category window is fully closed
            If parentPopup IsNot Nothing Then parentPopup.StaysOpen = False
        End Sub

        Private Sub BtnAddSubcategory_Click(sender As Object, e As RoutedEventArgs)
            Dim addNewSubCategory As New DPC.Components.Forms.AddSubcategory()

            Dim win As New Window() With {
                .Content = addNewSubCategory,
                .SizeToContent = SizeToContent.WidthAndHeight,
                .WindowStyle = WindowStyle.None,
                .AllowsTransparency = True,
                .Background = Brushes.Transparent,
                .WindowStartupLocation = WindowStartupLocation.CenterScreen,
                .ResizeMode = ResizeMode.NoResize,
                .ShowInTaskbar = False,
                .Topmost = True
            }

            AddHandler win.Closed, Sub(s As Object, ev As EventArgs)
                                       Dim selectedCategory As String = TryCast(CmbCategory.SelectedItem, ComboBoxItem)?.Content?.ToString()
                                       ProductController.GetProductSubcategory(If(selectedCategory, String.Empty), CmbSubCategory, SubCategoryLabel, New StackPanel())
                                       SnapshotSubcategoryItems()
                                   End Sub

            ' Pin the AddBrand popup so it does not close when focus shifts to the new Window
            Dim parentPopup As Popup = FindParentPopup()
            If parentPopup IsNot Nothing Then parentPopup.StaysOpen = True

            win.ShowDialog()

            ' Restore normal behavior after the subcategory window is fully closed
            If parentPopup IsNot Nothing Then parentPopup.StaysOpen = False
        End Sub

    End Class
End Namespace