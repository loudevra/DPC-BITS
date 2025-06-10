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


        ' This is where you code a function

        Private Sub btnIncrease(sender As Object, e As RoutedEventArgs)
            AdjustCategoryNumber(1)
            CreateSubCategoryPanel()
        End Sub

        Public Sub AdjustCategoryNumber(change As Integer)
            Dim currentValue As Integer
            If Integer.TryParse(SubCategoryNumber.Text, currentValue) Then
                currentValue += change
                SubCategoryNumber.Text = currentValue.ToString()
            End If
        End Sub

        Private Sub btnDecrease(sender As Object, e As RoutedEventArgs)
            If Integer.TryParse(SubCategoryNumber.Text, Nothing) AndAlso CInt(SubCategoryNumber.Text) > 1 Then
                AdjustCategoryNumber(-1)
                RemoveCategoryPanel()
                RemoveCategoryTextBoxes()
                ' Enlist
                enlistTheLastSubcategory()
            End If
        End Sub

        Private Sub RemoveCategoryTextBoxes()
            If productcategoryNameTextBoxes.Count > 0 Then productcategoryNameTextBoxes.RemoveAt(productcategoryNameTextBoxes.Count - 1)
            If productcategoryDescriptionTextBoxes.Count > 0 Then productcategoryDescriptionTextBoxes.RemoveAt(productcategoryDescriptionTextBoxes.Count - 1)
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

        Public Sub LoadSubCategoryPanel(subcategories As List(Of Subcategory))
            ' Clear previous UI elements to prevent duplication
            MainContent.Children.Clear()

            ' Loop through each subcategory in the list
            For Each subcategory As Subcategory In subcategories
                Dim subcategoryPanel As New StackPanel()

                Dim lblSubCategory As New TextBlock With {
            .Text = $"Subcategory {subcategory.subcategoryID}:",
            .FontSize = 14,
            .FontWeight = FontWeights.SemiBold
        }

                Dim txtSubCategoryName As New TextBox With {
            .Text = subcategory.subcategoryName,
            .Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style)
        }

                subcategoryPanel.Children.Add(lblSubCategory)
                subcategoryPanel.Children.Add(txtSubCategoryName)

                MainContent.Children.Add(subcategoryPanel)
            Next
        End Sub

        Public Sub PrintProductSubCategory(productCategory As ProductCategory)
            ' Loop through subcategories
            If productCategory.subcategories IsNot Nothing Then
                For Each subcategory In productCategory.subcategories
                    Console.WriteLine($"Subcategory ID: {subcategory.subcategoryID}, Name: {subcategory.subcategoryName}")
                Next
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

                        ' If it does NOT exist, collect it in our list for later insertion.
                        If Not exists Then
                            missingSubcategories.Add(subcategory)
                        End If
                    Next

                    ' Step 2: Check if all missing subcategories are valid before inserting.
                    ' (In this case, it ensures all have a non-empty name.)
                    If missingSubcategories.All(Function(s) Not String.IsNullOrWhiteSpace(s.subcategoryName)) Then
                        For Each subcategory As Subcategory In missingSubcategories
                            Try
                                addSubcategory(categoryID, subcategory.subcategoryName)
                                Console.WriteLine($"Inserted: {subcategory.subcategoryName}")
                            Catch ex As Exception
                                Console.WriteLine("Insert failed: " & ex.Message)
                            End Try
                        Next
                    Else
                        MessageBox.Show("One or more subcategories are invalid. Insert canceled.")
                    End If

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
            removeTheLastSubcategory()
        End Sub

        Public Sub UpdateCategoryWithSubCategoryInfo(categoryID As Integer, subcategoriesMain As List(Of Subcategory))
            _categoryID = categoryID
            _subcategories = subcategoriesMain
        End Sub

        Private enlistDeletion As New List(Of Subcategory)

        ' Method to add the last subcategory to the list
        Private Sub enlistTheLastSubcategory()
            If _subcategories.Count > 0 Then
                For i As Integer = _subcategories.Count - 1 To 0 Step -1
                    Dim current As Subcategory = _subcategories(i)

                    ' Check if it's not already enlisted
                    Dim alreadyEnlisted = enlistDeletion.Any(Function(s) s.subcategoryID = current.subcategoryID)

                    If Not alreadyEnlisted Then
                        enlistDeletion.Add(New Subcategory With {
                            .subcategoryID = current.subcategoryID,
                            .subcategoryName = current.subcategoryName
                        })
                        Exit For ' Enlist only one per call
                    End If
                Next

                Console.WriteLine("Current items in enlistDeletion:")
                For Each item As Subcategory In enlistDeletion
                    Console.WriteLine($"ID: {item.subcategoryID}, Name: {item.subcategoryName}")
                Next
            End If
        End Sub

        ' This will automatically delete the subcategory
        Private Sub removeTheLastSubcategory()
            If _subcategories.Count > 0 Then
                Dim lastSubCategoryID As Integer = _subcategories(_subcategories.Count - 1).subcategoryID
                Dim lastSubCategoryName As String = _subcategories(_subcategories.Count - 1).subcategoryName
                Console.WriteLine("Removing SubcategoryID: " & lastSubCategoryID)
                Try
                    Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                        conn.Open()

                        Dim deleteSubCategoryQuery As String = "DELETE FROM subcategory WHERE subcategoryID = @subcategoryID"
                        For Each item As Subcategory In enlistDeletion
                            Using deleteCmd As New MySqlCommand(deleteSubCategoryQuery, conn)
                                deleteCmd.Parameters.AddWithValue("@subcategoryID", item.subcategoryID)
                                deleteCmd.ExecuteNonQuery()

                                Console.WriteLine($"Successfully Deleted the Subcategory '{item.subcategoryName}' (ID: {item.subcategoryID})")
                            End Using
                        Next
                        enlistDeletion.Clear()
                        Console.WriteLine("Finished deleting all enlisted subcategories.")
                        RaiseEvent EditProductCategoryAndSubcategory(Me, EventArgs.Empty)
                    End Using
                Catch ex As Exception
                    Console.WriteLine("Error deleting subcategory: " & ex.Message)
                End Try
            End If
        End Sub

        Private Sub addSubcategory(categoryID As Integer, subcategoryName As String)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim insertSubCategoryQuery As String = "INSERT INTO subcategory (categoryID, subcategoryName, dateCreated, dateModified) VALUES (@categoryID, @subcategoryName, NOW(), NOW())"
                    Using insertCmd As New MySqlCommand(insertSubCategoryQuery, conn)
                        insertCmd.Parameters.AddWithValue("@categoryID", categoryID)
                        insertCmd.Parameters.AddWithValue("@subcategoryName", subcategoryName)
                        insertCmd.ExecuteNonQuery()

                        Console.WriteLine($"Successfully Added the Subcategory '{subcategoryName}'")
                    End Using
                End Using
            Catch ex As Exception
                Console.WriteLine("Inserting subcategory failed : " & ex.Message)
            End Try
        End Sub
    End Class
End Namespace