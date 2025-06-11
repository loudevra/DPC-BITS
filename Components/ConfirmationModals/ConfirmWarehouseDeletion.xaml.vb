
Imports DPC.DPC.Data.Model
Imports MySql.Data.MySqlClient

Namespace DPC.Components.ConfirmationModals

    Public Class ConfirmWarehouseDeletion

        Public warehouseID As Integer
        Public Warehouse As DPC.Views.Stocks.Warehouses.Warehouses
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

        End Sub

        Private Sub ConfirmDeletion(ID As Integer)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim updateQuery As String = "DELETE FROM warehouse WHERE warehouseID = @warehouseID"
                    Using deleteCmd As New MySqlCommand(updateQuery, conn)
                        deleteCmd.Parameters.AddWithValue("@warehouseID", ID)
                        Dim rowsAffected As Integer = deleteCmd.ExecuteNonQuery()

                        If rowsAffected > 0 Then
                            MessageBox.Show("Warehouse Deleted successfully!")
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

        Private Sub CloseDeleteDialog()
            Try
                ' If this is a UserControl that needs to be hidden
                Me.Visibility = Visibility.Collapsed
            Catch ex As Exception
                ' Handle any closing errors
            End Try
        End Sub

        Private Sub ConfirmDeletion_Click(sender As Object, e As RoutedEventArgs)
            ConfirmDeletion(warehouseID)

            ' Refreshes datagrid after update or add
            RefreshDataGrid()
        End Sub

        Private Sub RefreshDataGrid()
            Warehouse.dataGrid.ItemsSource = Nothing
            Warehouse.InitializeControls()
        End Sub



    End Class

End Namespace

