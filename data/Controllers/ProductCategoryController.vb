Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Models

Namespace DPC.Data.Controllers
    Public Class ProductCategoryController

        'Function to insert categories
        Public Shared Function InsertCategory(newCategory As ProductCategory) As Boolean
            If String.IsNullOrWhiteSpace(newCategory.categoryName) Then
                MessageBox.Show("Category name cannot be empty.")
                Return False
            End If

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim checkQuery As String = "SELECT COUNT(*) FROM category WHERE CategoryName = @CategoryName"
                    Using checkCmd As New MySqlCommand(checkQuery, conn)
                        checkCmd.Parameters.AddWithValue("@CategoryName", newCategory.categoryName)
                        Dim count As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())
                        If count > 0 Then
                            MessageBox.Show("Category already exists.")
                            Return False
                        End If
                    End Using

                    Dim query As String = "INSERT INTO category (CategoryName, categoryDescription, DateCreated, DateModified) " &
                                          "VALUES (@CategoryName, @categoryDescription, NOW(), NOW())"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@CategoryName", newCategory.categoryName)
                        cmd.Parameters.AddWithValue("@categoryDescription", newCategory.categoryDescription)
                        cmd.ExecuteNonQuery()
                    End Using

                    MessageBox.Show("Category added successfully!")
                    Return True
                End Using
            Catch ex As Exception
                MessageBox.Show($"An error occurred: {ex.Message}")
                Return False
            End Try
        End Function

        'Function to insert sub-categories
        Public Shared Function InsertSubcategories(categoryID As Integer, subcategories As List(Of Subcategory)) As Boolean
            If subcategories.Count = 0 Then
                MessageBox.Show("Please add at least one subcategory.")
                Return False
            End If

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    For Each subcategory As Subcategory In subcategories
                        If String.IsNullOrWhiteSpace(subcategory.subcategoryName) Then
                            MessageBox.Show("Subcategory name cannot be empty.")
                            Continue For
                        End If

                        ' Check for duplicate directly in DB before inserting
                        Dim checkQuery As String = "SELECT COUNT(*) FROM subcategory WHERE categoryID = @categoryID AND subcategoryName = @subcategoryName"
                        Using checkCmd As New MySqlCommand(checkQuery, conn)
                            checkCmd.Parameters.AddWithValue("@categoryID", categoryID)
                            checkCmd.Parameters.AddWithValue("@subcategoryName", subcategory.subcategoryName)
                            Dim count As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())
                            If count > 0 Then
                                MessageBox.Show($"Duplicate detected! ""{subcategory.subcategoryName}"" is already registered under this category.")
                                Return False
                            End If
                        End Using

                        Dim query As String = "INSERT INTO subcategory (categoryID, subcategoryName, dateCreated, dateModified) VALUES (@categoryID, @subcategoryName, NOW(), NOW())"
                        Using cmd As New MySqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@categoryID", categoryID)
                            cmd.Parameters.AddWithValue("@subcategoryName", subcategory.subcategoryName)
                            cmd.ExecuteNonQuery()
                        End Using
                    Next

                    MessageBox.Show("Subcategories added successfully!")
                    Return True
                End Using
            Catch ex As Exception
                MessageBox.Show($"An error occurred: {ex.Message}")
                Return False
            End Try
        End Function

        'Function to check existing subcategory names for a given category
        Public Shared Function GetExistingSubcategoryNames(categoryID As Integer) As List(Of String)
            Dim names As New List(Of String)()

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String = "SELECT subcategoryName FROM subcategory WHERE categoryID = @categoryID"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@categoryID", categoryID)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                names.Add(reader.GetString("subcategoryName").ToUpperInvariant().Trim())
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show($"Error fetching subcategories: {ex.Message}")
            End Try

            Return names
        End Function

        'Function to fetch all category data and add to grid
        Public Shared Function GetAllCategoriesWithSubcategories() As List(Of ProductCategory)
            Dim categories As New List(Of ProductCategory)()
            Dim connectionString As String = SplashScreen.GetDatabaseConnection().ConnectionString

            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                Dim query As String = "SELECT c.categoryID, c.categoryName, c.categoryDescription, " &
                                      "s.subcategoryID, s.subcategoryName " &
                                      "FROM category c " &
                                      "LEFT JOIN subcategory s ON c.categoryID = s.categoryID"

                Dim cmd As New MySqlCommand(query, conn)

                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim existingCategory As ProductCategory = categories.FirstOrDefault(Function(cat) cat.categoryID = reader.GetInt32("categoryID"))

                        If existingCategory Is Nothing Then
                            existingCategory = New ProductCategory With {
                                .categoryID = reader.GetInt32("categoryID"),
                                .categoryName = reader.GetString("categoryName"),
                                .categoryDescription = reader.GetString("categoryDescription"),
                                .subcategories = New List(Of Subcategory)()
                            }
                            categories.Add(existingCategory)
                        End If

                        If Not reader.IsDBNull(reader.GetOrdinal("subcategoryID")) Then
                            Dim subcategory As New Subcategory With {
                                .subcategoryID = reader.GetInt32("subcategoryID"),
                                .categoryID = reader.GetInt32("categoryID"),
                                .subcategoryName = reader.GetString("subcategoryName")
                            }
                            existingCategory.subcategories.Add(subcategory)
                        End If
                    End While
                End Using
            End Using

            Return categories
        End Function

        'Function to fetch all category and add into a combobox
        Public Shared Sub GetProductCategory(comboBox As ComboBox)
            Dim query As String = "
                SELECT categoryID, categoryName
                FROM category
                ORDER BY categoryName ASC;
            "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            comboBox.Items.Clear()
                            While reader.Read()
                                Dim categoryName As String = reader("categoryName").ToString()
                                Dim categoryId As Integer = Convert.ToInt32(reader("categoryID"))
                                Dim item As New ComboBoxItem With {
                                    .Content = categoryName,
                                    .Tag = categoryId
                                }
                                comboBox.Items.Add(item)
                            End While
                            comboBox.SelectedIndex = 0
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}")
                End Try
            End Using
        End Sub

    End Class
End Namespace
