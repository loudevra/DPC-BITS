Imports System.Windows.Controls.Primitives
Imports DocumentFormat.OpenXml.Bibliography
Imports DocumentFormat.OpenXml.InkML
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.Stocks.ProductCategories
Imports DPC.DPC.Views.Stocks.Suppliers.ManageBrands
Imports Google.Protobuf.Collections
Imports MySql.Data.MySqlClient


Namespace DPC.Components.Forms
    Public Class EditProductCategory
        Private productcategoryNameTextBoxes As New List(Of TextBox)()
        Private productcategoryDescriptionTextBoxes As New List(Of TextBox)()
        Private productcategoryPanels As New List(Of StackPanel)()
        Public Event ProductCategoryEdit()
        Public productCategoriesLoad As ProductCategories

        Public Event EditProductCategoryAndSubcategory As EventHandler
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

        End Sub

        Public Sub CreateSubCategoryPanel(Optional subcategoryName As String = "", Optional subcategoryDescription As String = "")
            ' Create subcategory panel
            Dim subcategoryPanel As New StackPanel() With {.Name = "SubCategoryPanel"}
            Dim subcategoryLabelPanel As New StackPanel() With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
            Dim subcategoryLabel As New TextBlock() With {.Text = "Sub-Category Name:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)}
            subcategoryLabelPanel.Children.Add(subcategoryLabel)

            Dim categoryBorder As New Border() With {.Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style), .Margin = New Thickness(0, 0, 0, 15)}
            Dim txtName As New TextBox() With {.Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style), .Name = "TxtSubCatName", .Text = subcategoryName}
            productcategoryNameTextBoxes.Add(txtName)
            categoryBorder.Child = txtName

            subcategoryPanel.Children.Add(subcategoryLabelPanel)
            subcategoryPanel.Children.Add(categoryBorder)

            ' Create description panel
            Dim descriptionPanel As New StackPanel() With {.Name = "DescriptionPanel"}
            Dim descriptionLabelPanel As New StackPanel() With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
            Dim descriptionLabel As New TextBlock() With {.Text = "Description:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)}
            descriptionLabelPanel.Children.Add(descriptionLabel)

            Dim descriptionBorder As New Border() With {.Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style), .Margin = New Thickness(0, 0, 0, 15)}
            Dim txtDescription As New TextBox() With {.Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style), .Name = "TxtSubDescription", .Text = subcategoryDescription}
            productcategoryDescriptionTextBoxes.Add(txtDescription)
            descriptionBorder.Child = txtDescription

            descriptionPanel.Children.Add(descriptionLabelPanel)
            descriptionPanel.Children.Add(descriptionBorder)

            ' Store panels for later removal
            productcategoryPanels.Add(subcategoryPanel)
            productcategoryPanels.Add(descriptionPanel)

            ' Add panels to UI
            MainContent.Children.Add(subcategoryPanel)
            MainContent.Children.Add(descriptionPanel)
        End Sub

        Private Sub RemoveCategoryPanel()
            If productcategoryPanels.Count >= 2 Then
                ' Remove last added panels from UI and list
                Dim lastDescriptionPanel As StackPanel = productcategoryPanels(productcategoryPanels.Count - 1)
                Dim lastSubcategoryPanel As StackPanel = productcategoryPanels(productcategoryPanels.Count - 2)

                MainContent.Children.Remove(lastDescriptionPanel)
                MainContent.Children.Remove(lastSubcategoryPanel)

                productcategoryPanels.RemoveAt(productcategoryPanels.Count - 1)
                productcategoryPanels.RemoveAt(productcategoryPanels.Count - 1)
            End If
        End Sub

        ' UPDATE IN THE DATABASE FUNCTION PART
        Public Sub UpdateCategory(categoryID As Integer?, categoryName As String, categoryDescription As String)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    ' Update
                    conn.Open()

                    If categoryID.HasValue AndAlso categoryID.Value > 0 Then
                        ' Check if the Category is Existed Already
                        Dim existQuery As String = "SELECT COUNT(*) FROM category WHERE categoryID = @categoryID"
                        Using existCMD As New MySqlCommand(existQuery, conn)
                            existCMD.Parameters.AddWithValue("@categoryID", categoryID)
                            Dim result As Object = existCMD.ExecuteScalar()
                            Dim count As Integer = If(result IsNot DBNull.Value, Convert.ToInt32(result), 0)
                            If count = 0 Then
                                MessageBox.Show("Category not found.")
                                Return
                            End If
                        End Using

                        ' copy sub
                        ' Check for duplicate brand name (excluding current brand)
                        Dim checkQuery As String = "SELECT COUNT(*) FROM category WHERE categoryName = @categoryName AND categoryID <> @categoryID"
                        Using checkCmd As New MySqlCommand(checkQuery, conn)
                            checkCmd.Parameters.AddWithValue("@categoryName", categoryName)
                            checkCmd.Parameters.AddWithValue("@categoryID", categoryID)
                            Dim result As Object = checkCmd.ExecuteScalar()
                            Dim count As Integer = If(result IsNot DBNull.Value, Convert.ToInt32(result), 0)
                            If count > 0 Then
                                MessageBox.Show("Category name already exists.")
                                Return
                            End If
                        End Using

                        ' Update category
                        Dim updateQuery As String = "UPDATE category SET categoryName = @categoryName, categoryDescription = @categoryDescription, dateModified = NOW() WHERE categoryID = @categoryID"
                        Using updateCmd As New MySqlCommand(updateQuery, conn)
                            updateCmd.Parameters.AddWithValue("@categoryName", categoryName)
                            updateCmd.Parameters.AddWithValue("@categoryID", categoryID)
                            updateCmd.Parameters.AddWithValue("@categoryDescription", categoryDescription)
                            Dim rowsAffected As Integer = updateCmd.ExecuteNonQuery()

                            If rowsAffected > 0 Then
                                MessageBox.Show("Category updated successfully!")
                                ' Raise the event to refresh DataGrid
                                RaiseEvent EditProductCategoryAndSubcategory(Me, EventArgs.Empty)
                            Else
                                MessageBox.Show("No changes were made.")
                            End If
                        End Using
                    End If
                End Using
            Catch ex As Exception
                Console.WriteLine($"An error occurred: {ex.Message}")
            End Try
        End Sub

        Public Sub UpdateSubCategories(categoryID As Integer, subcategories As List(Of Subcategory))
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    ' Check if the subcategory is Existed Already
                    Dim existSubCategoryQuery As String = "SELECT 1 FROM subcategory WHERE subcategoryName = @subcategoryName AND categoryID = @categoryID"
                    ' CHeck for duplication of subcategory name
                    Dim checkDuplicateSubCategoryQuery As String = "SELECT COUNT(*) FROM subcategory WHERE subcategoryName = @subcategoryName AND subcategoryID <> @subcategoryID"
                    ' Update the current subcategory based on the list 
                    Dim updateSubCategoryQuery As String = "UPDATE subcategory SET subcategoryName = @subcategoryName , dateModified = NOW() WHERE subcategoryID = @subcategoryID"

                    Dim missingSubcategories As New List(Of Subcategory)

                    For Each subcategory As Subcategory In subcategories
                        ' Check if the subcategory name is empty
                        If String.IsNullOrWhiteSpace(subcategory.subcategoryName) Then
                            MessageBox.Show("Subcategory name cannot be empty.")
                            Continue For
                        End If

                        ' Check if this subcategory already exists in the database using ExecuteReader.
                        Dim exists As Boolean = False
                        Using existSubCategoryCmd As New MySqlCommand(existSubCategoryQuery, conn)
                            ' Pass the subcategory name and the proper categoryID
                            existSubCategoryCmd.Parameters.AddWithValue("@subcategoryName", subcategory.subcategoryName)
                            existSubCategoryCmd.Parameters.AddWithValue("@categoryID", categoryID)

                            ' Use ExecuteReader to determine if any rows are returned.
                            Using reader As MySqlDataReader = existSubCategoryCmd.ExecuteReader()
                                exists = reader.HasRows
                            End Using

                            Console.WriteLine($"Exists Result = {exists}")
                        End Using
                    Next

                    ' Step 3: Continue to update existing subcategories
                    For Each subcategory As Subcategory In subcategories
                        ' Skip if newly inserted (they don’t need updating)
                        If missingSubcategories.Contains(subcategory) Then Continue For

                        ' Check for duplication
                        Using dupeSubCategoryCmd As New MySqlCommand(checkDuplicateSubCategoryQuery, conn)
                            dupeSubCategoryCmd.Parameters.AddWithValue("@subcategoryName", subcategory.subcategoryName)
                            dupeSubCategoryCmd.Parameters.AddWithValue("@subcategoryID", subcategory.subcategoryID)
                            Dim result As Object = dupeSubCategoryCmd.ExecuteScalar()
                            Dim count As Integer = If(result IsNot DBNull.Value, Convert.ToInt32(result), 0)
                            If count > 0 Then
                                MessageBox.Show("Sub-Category name already exists.")
                                Continue For
                            End If
                        End Using

                        ' Update existing subcategory
                        Using cmd As New MySqlCommand(updateSubCategoryQuery, conn)
                            cmd.Parameters.AddWithValue("@subcategoryID", subcategory.subcategoryID)
                            cmd.Parameters.AddWithValue("@subcategoryName", subcategory.subcategoryName)
                            cmd.ExecuteNonQuery()
                        End Using

                        MessageBox.Show($"Successfully Updated {subcategory.subcategoryName}")

                    Next
                    RaiseEvent EditProductCategoryAndSubcategory(Me, EventArgs.Empty)
                End Using
            Catch ex As Exception
                Console.WriteLine($"An error occurred: {ex.Message}")
            End Try
        End Sub

        ' Getting categoryID 
        Public _categoryID As Integer
        Public _subcategories As List(Of Subcategory)
        Private Sub btnUpdateCategory_Click(sender As Object, e As RoutedEventArgs)
            Dim categoryName As String = TxtCategoryName.Text
            Dim categoryDescription As String = TxtCategoryDescription.Text

            Dim categoryID As Integer = _categoryID
            If Me.Tag IsNot Nothing AndAlso TypeOf Me.Tag Is Integer Then
                categoryID = DirectCast(Me.Tag, Integer)
            End If

            ' Build updated subcategories list after passing the data
            Dim subcategories As New List(Of Subcategory)()

            For i As Integer = 0 To productcategoryNameTextBoxes.Count - 1
                Dim subcategoryName As String = productcategoryNameTextBoxes(i).Text.Trim()

                ' Skip empty or whitespace-only subcategory names
                If Not String.IsNullOrWhiteSpace(subcategoryName) Then
                    ' Safely get subcategoryID from _subcategories if it exists
                    Dim subcategoryID As Integer = 0
                    If _subcategories IsNot Nothing AndAlso i < _subcategories.Count Then
                        subcategoryID = _subcategories(i).subcategoryID
                    End If

                    ' Create and add the subcategory
                    Dim subcategory As New Subcategory With {
            .categoryID = categoryID,
            .subcategoryID = subcategoryID,
            .subcategoryName = subcategoryName
        }

                    subcategories.Add(subcategory)
                End If
            Next

            ' Call the update methods
            UpdateCategory(categoryID, categoryName, categoryDescription)
            UpdateSubCategories(categoryID, subcategories)
            ' productCategoriesLoad.LoadData()
            'removeTheLastSubcategory()
        End Sub

        Public Sub UpdateCategoryWithSubCategoryInfo(categoryID As Integer, subcategoriesMain As List(Of Subcategory))
            _categoryID = categoryID
            _subcategories = subcategoriesMain
        End Sub
    End Class
End Namespace