Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Models
Imports System.Windows.Controls.Primitives


Namespace DPC.Components.Forms
    Public Class AddSubcategory
        Private subcategoryNameTextBoxes As New List(Of TextBox)()
        Private subcategoryDescriptionTextBoxes As New List(Of TextBox)()
        Private subcategoryPanels As New List(Of StackPanel)()
        Public Event SubCategoryAdded As EventHandler

        Public Sub New()
            InitializeComponent()

            ProductCategoryController.GetProductCategory(ComboBoxCategory)

            CreateSubCategoryPanel()

            ' Add event handler for the close button
            AddHandler BtnClose.Click, AddressOf BtnClose_Click
        End Sub

        ' Event handler for the close button
        Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
            ' Get the parent Popup if it exists
            Dim parent = TryCast(Me.Parent, Popup)
            If parent IsNot Nothing Then
                ' Close the popup
                parent.IsOpen = False
            Else
                ' Try to find another parent container to close
                Dim parentWindow = Window.GetWindow(Me)
                If parentWindow IsNot Nothing Then
                    parentWindow.Close()
                End If
            End If
        End Sub

        Private Sub CreateSubCategoryPanel()
            ' Create subcategory panel
            Dim subcategoryPanel As New StackPanel() With {.Name = "SubCategoryPanel"}
            Dim subcategoryLabelPanel As New StackPanel() With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
            Dim subcategoryLabel As New TextBlock() With {.Text = "Sub-Category Name:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)}
            subcategoryLabelPanel.Children.Add(subcategoryLabel)

            Dim categoryBorder As New Border() With {.Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style), .Margin = New Thickness(0, 0, 0, 15)}
            Dim txtName As New TextBox() With {.Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style), .Name = "TxtName"}
            subcategoryNameTextBoxes.Add(txtName)
            categoryBorder.Child = txtName

            subcategoryPanel.Children.Add(subcategoryLabelPanel)
            subcategoryPanel.Children.Add(categoryBorder)

            ' Create description panel
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

            ' Store panels for later removal
            subcategoryPanels.Add(subcategoryPanel)
            subcategoryPanels.Add(descriptionPanel)

            ' Add panels to UI
            MainContent.Children.Add(subcategoryPanel)
            MainContent.Children.Add(descriptionPanel)
        End Sub

        Private Sub RemoveCategoryPanel()
            If subcategoryPanels.Count >= 2 Then
                ' Remove last added panels from UI and list
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
            ' Get selected category ID
            Dim selectedItem As ComboBoxItem = TryCast(ComboBoxCategory.SelectedItem, ComboBoxItem)

            If selectedItem Is Nothing Then
                'MessageBox.Show("Please select a category.")
                Exit Sub
            End If

            Dim selectedCategoryID As Integer = Convert.ToInt32(selectedItem.Tag)

            ' Collect all subcategory names from dynamically created textboxes
            Dim subcategories As New List(Of Subcategory)()

            For i As Integer = 0 To subcategoryNameTextBoxes.Count - 1
                Dim subcategoryName As String = subcategoryNameTextBoxes(i).Text.Trim()
                If Not String.IsNullOrWhiteSpace(subcategoryName) Then
                    Dim subcategory As New Subcategory With {
                .categoryID = selectedCategoryID,
                .subcategoryName = subcategoryName
            }
                    subcategories.Add(subcategory)
                End If
            Next

            ' Insert into database
            If ProductCategoryController.InsertSubcategories(selectedCategoryID, subcategories) Then
                'MessageBox.Show("All subcategories added successfully!")

                ' Notify that a new category has been added
                RaiseEvent SubCategoryAdded(Me, EventArgs.Empty)

                ' Close the popup after successful addition
                BtnClose_Click(Me, New RoutedEventArgs())
            Else
                'MessageBox.Show("Failed to add subcategories.")
            End If
        End Sub

    End Class
End Namespace