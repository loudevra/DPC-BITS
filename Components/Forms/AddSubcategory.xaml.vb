Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Models

Namespace DPC.Components.Forms
    Public Class AddSubcategory
        Private subcategoryNameTextBoxes As New List(Of TextBox)()
        Private subcategoryDescriptionTextBoxes As New List(Of TextBox)()
        Public Event SubCategoryAdded As EventHandler
        Public Sub New()
            InitializeComponent()
            CreateSubCategoryPanel()
        End Sub

        ' Function to load categories into the ComboBox
        Private Sub LoadCategories()
            ProductCategoryController.GetProductCategory(ComboBoxCategory)

        End Sub

        Private Sub CreateSubCategoryPanel()
            Dim subcategoryPanel As New StackPanel() With {.Name = "SubCategoryPanel"}
            Dim subcategoryLabelPanel As New StackPanel() With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
            Dim subcategoryLabel As New TextBlock() With {.Text = "Category Name:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)}
            subcategoryLabelPanel.Children.Add(subcategoryLabel)

            Dim categoryBorder As New Border() With {.Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style), .Margin = New Thickness(0, 0, 0, 15)}
            Dim txtName As New TextBox() With {.Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style), .Name = "TxtName"}
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
            subcategoryDescriptionTextBoxes.Add(txtDescription)
            descriptionBorder.Child = txtDescription

            descriptionPanel.Children.Add(descriptionLabelPanel)
            descriptionPanel.Children.Add(descriptionBorder)

            MainContent.Children.Add(subcategoryPanel)
            MainContent.Children.Add(descriptionPanel)
        End Sub

        Private Sub RemoveCategoryPanel()
            If MainContent.Children.Count > 0 Then
                MainContent.Children.RemoveAt(MainContent.Children.Count - 1)
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
    End Class
End Namespace
