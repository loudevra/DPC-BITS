Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Models
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components.Dynamic  ' Added import for DynamicDialogs

Namespace DPC.Components.Forms
    Public Class AddCategory

        Private categoryNameTextBoxes As New List(Of TextBox)()
        Private categoryDescriptionTextBoxes As New List(Of TextBox)()
        Public Event CategoryAdded As EventHandler


        Public Sub New()
            InitializeComponent()
            CreateCategoryPanel()

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

        Private Sub InsertBtn(sender As Object, e As RoutedEventArgs)
            For i As Integer = 0 To categoryNameTextBoxes.Count - 1
                Dim categoryName As String = categoryNameTextBoxes(i).Text
                Dim categoryDescription As String = categoryDescriptionTextBoxes(i).Text

                Dim newCategory As New ProductCategory With {
                    .categoryName = categoryName,
                    .categoryDescription = categoryDescription
                }

                If String.IsNullOrWhiteSpace(newCategory.categoryName) OrElse String.IsNullOrWhiteSpace(categoryDescription) Then
                    ' Show validation error dialog
                    DynamicDialogs.ShowWarning(Me, "Please fill out both category name and description.")
                    Return
                End If

                If ProductCategoryController.InsertCategory(newCategory) Then
                    ' Show success dialog
                    DynamicDialogs.ShowSuccess(Me, "Category added successfully!")

                    ' Notify that a new category has been added
                    RaiseEvent CategoryAdded(Me, EventArgs.Empty)

                    ' Close the popup after successful addition
                    BtnClose_Click(Me, New RoutedEventArgs())
                Else
                    ' Show error dialog
                    DynamicDialogs.ShowError(Me, "Failed to add category.")
                End If
            Next
        End Sub

        Private Sub CreateCategoryPanel()
            Dim categoryPanel As New StackPanel() With {.Name = "CategoryPanel"}
            Dim categoryLabelPanel As New StackPanel() With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
            Dim categoryLabel As New TextBlock() With {.Text = "Category Name:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)}
            categoryLabelPanel.Children.Add(categoryLabel)

            Dim categoryBorder As New Border() With {.Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style), .Margin = New Thickness(0, 0, 0, 15)}
            Dim txtName As New TextBox() With {.Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style), .Name = "TxtName"}
            categoryNameTextBoxes.Add(txtName)
            categoryBorder.Child = txtName

            categoryPanel.Children.Add(categoryLabelPanel)
            categoryPanel.Children.Add(categoryBorder)

            Dim descriptionPanel As New StackPanel() With {.Name = "DescriptionPanel"}
            Dim descriptionLabelPanel As New StackPanel() With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
            Dim descriptionLabel As New TextBlock() With {.Text = "Description:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)}
            descriptionLabelPanel.Children.Add(descriptionLabel)

            Dim descriptionBorder As New Border() With {.Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style), .Margin = New Thickness(0, 0, 0, 15)}
            Dim txtDescription As New TextBox() With {.Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style), .Name = "TxtDescription"}
            categoryDescriptionTextBoxes.Add(txtDescription)
            descriptionBorder.Child = txtDescription

            descriptionPanel.Children.Add(descriptionLabelPanel)
            descriptionPanel.Children.Add(descriptionBorder)

            MainContent.Children.Add(categoryPanel)
            MainContent.Children.Add(descriptionPanel)
        End Sub

        Private Sub RemoveCategoryPanel()
            If MainContent.Children.Count > 0 Then
                MainContent.Children.RemoveAt(MainContent.Children.Count - 1)
            End If
        End Sub

        Private Sub RemoveCategoryTextBoxes()
            If categoryNameTextBoxes.Count > 0 Then categoryNameTextBoxes.RemoveAt(categoryNameTextBoxes.Count - 1)
            If categoryDescriptionTextBoxes.Count > 0 Then categoryDescriptionTextBoxes.RemoveAt(categoryDescriptionTextBoxes.Count - 1)
        End Sub

        Private Sub IncreaseBtn(sender As Object, e As RoutedEventArgs)
            AdjustCategoryNumber(1)
            CreateCategoryPanel()
        End Sub

        Private Sub AdjustCategoryNumber(change As Integer)
            Dim currentValue As Integer
            If Integer.TryParse(CategoryNumber.Text, currentValue) Then
                currentValue += change
                CategoryNumber.Text = currentValue.ToString()
            End If
        End Sub

        Private Sub DecreaseBtn(sender As Object, e As RoutedEventArgs)
            If Integer.TryParse(CategoryNumber.Text, Nothing) AndAlso CInt(CategoryNumber.Text) > 1 Then
                AdjustCategoryNumber(-1)
                RemoveCategoryPanel()
                RemoveCategoryTextBoxes()
            End If
        End Sub

    End Class
End Namespace