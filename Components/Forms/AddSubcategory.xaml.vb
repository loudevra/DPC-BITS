Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Models
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input

Namespace DPC.Components.Forms
    Public Class AddSubcategory
        Private subcategoryNameTextBoxes As New List(Of TextBox)()
        Private subcategoryDescriptionTextBoxes As New List(Of TextBox)()
        Private subcategoryPanels As New List(Of StackPanel)()
        Private categoryItems As New List(Of ComboBoxItem)()
        Public Event SubCategoryAdded As EventHandler

        Public Sub New()
            InitializeComponent()

            ProductCategoryController.GetProductCategory(ComboBoxCategory)
            SnapshotCategoryItems()

            ComboBoxCategory.IsEditable = True
            ComboBoxCategory.IsTextSearchEnabled = False
            ComboBoxCategory.StaysOpenOnEdit = True

            AddHandler ComboBoxCategory.PreviewTextInput, AddressOf CmbCategory_Filter

            CreateSubCategoryPanel()

            AddHandler BtnClose.Click, AddressOf BtnClose_Click
        End Sub

        Private Sub SnapshotCategoryItems()
            categoryItems.Clear()
            For Each item As ComboBoxItem In ComboBoxCategory.Items
                categoryItems.Add(item)
            Next
        End Sub

        Private Sub CmbCategory_Filter(sender As Object, e As TextCompositionEventArgs)
            ComboBoxCategory.IsDropDownOpen = True
            Dim tb = TryCast(ComboBoxCategory.Template.FindName("PART_EditableTextBox", ComboBoxCategory), TextBox)
            Dim filterText = If(tb IsNot Nothing, tb.Text & e.Text, e.Text).ToUpperInvariant()

            ComboBoxCategory.Items.Clear()
            For Each item In categoryItems
                If item.Content.ToString().ToUpperInvariant().Contains(filterText) Then
                    ComboBoxCategory.Items.Add(item)
                End If
            Next
        End Sub

        Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
            Dim parent = TryCast(Me.Parent, Popup)
            If parent IsNot Nothing Then
                parent.IsOpen = False
            Else
                Dim parentWindow = Window.GetWindow(Me)
                If parentWindow IsNot Nothing Then
                    parentWindow.Close()
                End If
            End If
        End Sub

        Private Sub CreateSubCategoryPanel()
            Dim subcategoryPanel As New StackPanel() With {.Name = "SubCategoryPanel"}
            Dim subcategoryLabelPanel As New StackPanel() With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
            Dim subcategoryLabel As New TextBlock() With {.Text = "Sub-Category Name:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)}
            subcategoryLabelPanel.Children.Add(subcategoryLabel)

            Dim categoryBorder As New Border() With {.Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style), .Margin = New Thickness(0, 0, 0, 15)}
            Dim txtName As New TextBox() With {.Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style), .Name = "TxtName"}
            AddHandler txtName.TextChanged, AddressOf TxtToUpper_TextChanged
            subcategoryNameTextBoxes.Add(txtName)
            categoryBorder.Child = txtName

            subcategoryPanel.Children.Add(subcategoryLabelPanel)
            subcategoryPanel.Children.Add(categoryBorder)

            Dim descriptionPanel As New StackPanel() With {.Name = "DescriptionPanel"}
            Dim descriptionLabelPanel As New StackPanel() With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
            Dim descriptionLabel As New TextBlock() With {.Text = "Description:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)}
            descriptionLabelPanel.Children.Add(descriptionLabel)

            Dim descriptionBorder As New Border() With {.Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style), .Margin = New Thickness(0, 0, 0, 15)}
            Dim txtDescription As New TextBox() With {.Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style), .Name = "TxtDescription"}
            AddHandler txtDescription.TextChanged, AddressOf TxtToUpper_TextChanged
            subcategoryDescriptionTextBoxes.Add(txtDescription)
            descriptionBorder.Child = txtDescription

            descriptionPanel.Children.Add(descriptionLabelPanel)
            descriptionPanel.Children.Add(descriptionBorder)

            subcategoryPanels.Add(subcategoryPanel)
            subcategoryPanels.Add(descriptionPanel)

            MainContent.Children.Add(subcategoryPanel)
            MainContent.Children.Add(descriptionPanel)
        End Sub

        Private Sub TxtToUpper_TextChanged(sender As Object, e As Windows.Controls.TextChangedEventArgs)
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return

            Dim originalSelectionStart = tb.SelectionStart
            Dim originalSelectionLength = tb.SelectionLength
            Dim originalText = tb.Text

            Dim upperText = originalText.ToUpperInvariant()
            If Not String.Equals(originalText, upperText, StringComparison.Ordinal) Then
                tb.Text = upperText
                Dim newSelectionStart = Math.Min(originalSelectionStart, tb.Text.Length)
                tb.SelectionStart = newSelectionStart
                tb.SelectionLength = originalSelectionLength
            End If
        End Sub

        Private Sub RemoveCategoryPanel()
            If subcategoryPanels.Count >= 2 Then
                Dim lastDescriptionPanel As StackPanel = subcategoryPanels(subcategoryPanels.Count - 1)
                Dim lastSubcategoryPanel As StackPanel = subcategoryPanels(subcategoryPanels.Count - 2)

                MainContent.Children.Remove(lastDescriptionPanel)
                MainContent.Children.Remove(lastSubcategoryPanel)

                subcategoryPanels.RemoveAt(subcategoryPanels.Count - 1)
                subcategoryPanels.RemoveAt(subcategoryPanels.Count - 1)
            End If
        End Sub

        Private Sub RemoveCategoryTextBoxes()
            If subcategoryNameTextBoxes.Count > 0 Then subcategoryNameTextBoxes.RemoveAt(subcategoryNameTextBoxes.Count - 1)
            If subcategoryDescriptionTextBoxes.Count > 0 Then subcategoryDescriptionTextBoxes.RemoveAt(subcategoryDescriptionTextBoxes.Count - 1)
        End Sub

        Private Sub IncreaseBtn(sender As Object, e As RoutedEventArgs)
            AdjustCategoryNumber(1)
            CreateSubCategoryPanel()
        End Sub

        Private Sub AdjustCategoryNumber(change As Integer)
            Dim currentValue As Integer
            If Integer.TryParse(SubCategoryNumber.Text, currentValue) Then
                currentValue += change
                SubCategoryNumber.Text = currentValue.ToString()
            End If
        End Sub

        Private Sub DecreaseBtn(sender As Object, e As RoutedEventArgs)
            If Integer.TryParse(SubCategoryNumber.Text, Nothing) AndAlso CInt(SubCategoryNumber.Text) > 1 Then
                AdjustCategoryNumber(-1)
                RemoveCategoryPanel()
                RemoveCategoryTextBoxes()
            End If
        End Sub

        Private Sub InsertBtn(sender As Object, e As RoutedEventArgs)
            Dim selectedItem As ComboBoxItem = TryCast(ComboBoxCategory.SelectedItem, ComboBoxItem)

            If selectedItem Is Nothing Then
                Exit Sub
            End If

            Dim selectedCategoryID As Integer = Convert.ToInt32(selectedItem.Tag)

            Dim existingNames As List(Of String) = ProductCategoryController.GetExistingSubcategoryNames(selectedCategoryID)

            Dim subcategories As New List(Of Subcategory)()
            Dim duplicatesFound As New List(Of String)()

            For i As Integer = 0 To subcategoryNameTextBoxes.Count - 1
                Dim subcategoryName As String = subcategoryNameTextBoxes(i).Text.Trim().ToUpperInvariant()

                If String.IsNullOrWhiteSpace(subcategoryName) Then Continue For

                If existingNames.Contains(subcategoryName) Then
                    duplicatesFound.Add(subcategoryName)
                    Continue For
                End If

                Dim subcategory As New Subcategory With {
                    .categoryID = selectedCategoryID,
                    .subcategoryName = subcategoryName
                }
                subcategories.Add(subcategory)
            Next

            If duplicatesFound.Count > 0 Then
                MessageBox.Show($"Duplicate detected! ""{String.Join(", ", duplicatesFound)}"" is already registered under this category.")
                Exit Sub
            End If

            If subcategories.Count = 0 Then Exit Sub

            If ProductCategoryController.InsertSubcategories(selectedCategoryID, subcategories) Then
                RaiseEvent SubCategoryAdded(Me, EventArgs.Empty)
                BtnClose_Click(Me, New RoutedEventArgs())
            End If
        End Sub

    End Class
End Namespace