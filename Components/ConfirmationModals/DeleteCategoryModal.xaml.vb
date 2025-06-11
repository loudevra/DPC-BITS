Imports MySql.Data.MySqlClient

Namespace DPC.Components.ConfirmationModals
    Public Class DeleteCategoryModal
        Public Event DeletedCategory As EventHandler

        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

        End Sub
        Private Sub Cancel_Click(sender As Object, e As RoutedEventArgs)
            Me.Visibility = Visibility.Collapsed ' Hide Modal
        End Sub

        Public _categoryID As Integer

        Public Sub getCategoryID(categoryID As Integer)
            _categoryID = categoryID
        End Sub

        Private Sub Confirm_Click(sender As Object, e As RoutedEventArgs)
            Try
                Dim deleteSerialNumberQuery As String = "DELETE FROM serialnumberproduct WHERE ProductID IN (SELECT productID FROM product WHERE categoryID = @categoryID)"
                Dim deleteProductQuery As String = "DELETE FROM product WHERE categoryID = @categoryID"
                Dim deleteSubcategoryQuery As String = "DELETE FROM subcategory WHERE categoryID = @categoryID"
                Dim deleteCategoryQuery As String = "DELETE FROM category WHERE categoryID = @categoryID"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Step 1: Delete from serialnumberproduct first (most dependent)
                    Using deleteSerialNumberCmd As New MySqlCommand(deleteSerialNumberQuery, conn)
                        deleteSerialNumberCmd.Parameters.AddWithValue("@categoryID", _categoryID)
                        deleteSerialNumberCmd.ExecuteNonQuery()
                    End Using

                    ' Step 2: Delete products related to this category
                    Using deleteProductCmd As New MySqlCommand(deleteProductQuery, conn)
                        deleteProductCmd.Parameters.AddWithValue("@categoryID", _categoryID)
                        deleteProductCmd.ExecuteNonQuery()
                    End Using

                    ' Step 3: Delete subcategories
                    Using deleteSubcategoryCmd As New MySqlCommand(deleteSubcategoryQuery, conn)
                        deleteSubcategoryCmd.Parameters.AddWithValue("@categoryID", _categoryID)
                        deleteSubcategoryCmd.ExecuteNonQuery()
                    End Using

                    ' Step 4: Delete the category itself
                    Using deleteCategoryCmd As New MySqlCommand(deleteCategoryQuery, conn)
                        deleteCategoryCmd.Parameters.AddWithValue("@categoryID", _categoryID)
                        deleteCategoryCmd.ExecuteNonQuery()
                    End Using

                    MessageBox.Show("Category and related data deleted successfully.")
                    RaiseEvent DeletedCategory(Me, EventArgs.Empty)
                End Using
            Catch ex As MySqlException
                ' Check if it's a foreign key error (error code 1451)
                If ex.Number = 1451 Then
                    MessageBox.Show("Cannot delete category because it is still linked to products, subcategories, or other items.")
                Else
                    MessageBox.Show("An error occurred: " & ex.Message)
                End If
            Catch ex As Exception
                MessageBox.Show($"An Error Occured While Deleting the Category : {ex.Message}")
                Console.WriteLine($"StackTrace : {ex.StackTrace}")
            End Try
        End Sub
    End Class
End Namespace
