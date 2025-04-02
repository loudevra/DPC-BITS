Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Model
Imports DPC.DPC.DPC.Data.Model

Namespace DPC.Data.Controllers
    Public Class ProductCategoryController
        Public Shared Function InsertCategory(newCategory As ProductCategory) As Boolean
            ' Ensure the category name is not empty
            If String.IsNullOrWhiteSpace(newCategory.categoryName) Then
                MessageBox.Show("Category name cannot be empty.")
                Return False
            End If

            Try
                ' Use connection from SplashScreen
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Check for duplicate category
                    Dim checkQuery As String = "SELECT COUNT(*) FROM Category WHERE CategoryName = @CategoryName"
                    Using checkCmd As New MySqlCommand(checkQuery, conn)
                        checkCmd.Parameters.AddWithValue("@CategoryName", newCategory.categoryName)

                        Dim count As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())
                        If count > 0 Then
                            MessageBox.Show("Category already exists.")
                            Return False
                        End If
                    End Using

                    ' Insert the new category
                    Dim query As String = "INSERT INTO Category (CategoryName, categoryDescription, DateCreated, DateModified) " &
                                          "VALUES (@CategoryName, @categoryDescription, NOW(), NOW())"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@CategoryName", newCategory.categoryName)
                        cmd.Parameters.AddWithValue("@categoryDescription", newCategory.categoryDescription)

                        cmd.ExecuteNonQuery()
                    End Using

                    MessageBox.Show("Category added successfully!")
                    Return True
                End Using ' ✅ Connection automatically returned to the pool
            Catch ex As Exception
                MessageBox.Show($"An error occurred: {ex.Message}")
                Return False
            End Try
        End Function

        ' New method to fetch all categories with subcategories
        Public Shared Function GetAllCategoriesWithSubcategories() As List(Of ProductCategory)
            Dim categories As New List(Of ProductCategory)()
            Dim connectionString As String = SplashScreen.GetDatabaseConnection().ConnectionString

            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                ' Query to fetch categories along with subcategories (only relevant columns)
                Dim query As String = "SELECT c.categoryID, c.categoryName, c.categoryDescription, " &
                                      "s.subcategoryID, s.subcategoryName " &
                                      "FROM Category c " &
                                      "LEFT JOIN Subcategory s ON c.categoryID = s.categoryID"

                Dim cmd As New MySqlCommand(query, conn)

                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        ' Check if the category already exists in the list
                        Dim existingCategory As ProductCategory = categories.FirstOrDefault(Function(cat) cat.categoryID = reader.GetInt32("categoryID"))

                        If existingCategory Is Nothing Then
                            ' If the category is not found, create a new category
                            existingCategory = New ProductCategory With {
                                .categoryID = reader.GetInt32("categoryID"),
                                .categoryName = reader.GetString("categoryName"),
                                .categoryDescription = reader.GetString("categoryDescription"),
                                .subcategories = New List(Of Subcategory)() ' Initialize subcategory list
                            }
                            categories.Add(existingCategory)
                        End If

                        ' Add the subcategory to the category
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
    End Class
End Namespace
