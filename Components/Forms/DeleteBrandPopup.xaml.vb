Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Views.Stocks.Suppliers.ManageBrands


Namespace DPC.Components.Forms
    Public Class DeleteBrandPopup
        Public brandID As String
        Public manageBrands As ManageBrands


        Public Sub New()
            InitializeComponent()
        End Sub


        Private Sub ConfirmDeletion(brandId As String)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim updateQuery As String = "DELETE FROM brand WHERE BrandID = @BrandID"
                    Using deleteCmd As New MySqlCommand(updateQuery, conn)
                        deleteCmd.Parameters.AddWithValue("@BrandID", brandId)
                        Dim rowsAffected As Integer = deleteCmd.ExecuteNonQuery()

                        If rowsAffected > 0 Then
                            MessageBox.Show("Brand Deleted successfully!")

                            ' Close the dialog/usercontrol
                            CloseDeleteDialog()
                        Else
                            MessageBox.Show("No changes were made.")
                        End If
                    End Using
                End Using

            Catch ex As Exception
                MessageBox.Show($"An error occurred: {ex.Message}")
            End Try
        End Sub

        ' Helper method to close the edit dialog
        Private Sub CloseDeleteDialog()
            Try
                ' If this is a UserControl that needs to be hidden
                Me.Visibility = Visibility.Collapsed
            Catch ex As Exception
                ' Handle any closing errors
            End Try
        End Sub

        Private Sub RefreshDataGrid()
            manageBrands.dataGrid.ItemsSource = Nothing
            manageBrands.InitializeControls()
        End Sub

        Private Sub ConfirmDeletion_Click(sender As Object, e As RoutedEventArgs)
            ConfirmDeletion(brandID)

            ' Refreshes datagrid after update or add
            RefreshDataGrid()
        End Sub
    End Class
End Namespace
