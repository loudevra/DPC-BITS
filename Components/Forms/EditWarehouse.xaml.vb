
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Views.Stocks.Warehouses
Imports MySql.Data.MySqlClient

Namespace DPC.Components.Forms
    Public Class EditWarehouse
        Public Event WarehouseUpdated()
        Public WarehouseID
        Public WarehouseNameOld
        Public Warehouses As DPC.Views.Stocks.Warehouses.Warehouses
        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()
            ' Add any initialization after the InitializeComponent() call.
        End Sub

        Public Sub UpdateWarehouse(ID As Integer, Name As String)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    ' Check for duplicate name
                    Dim checkQuery As String = "SELECT COUNT(*) FROM warehouse WHERE warehouseName = @name AND warehouseID <> @id"
                    Using checkCmd As New MySqlCommand(checkQuery, conn)
                        checkCmd.Parameters.AddWithValue("@name", Name)
                        checkCmd.Parameters.AddWithValue("@id", ID)
                        Dim count As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())
                        If count > 0 Then
                            MessageBox.Show("A warehouse with the same name already exists.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Warning)
                        Else
                            Dim updateQuery As String = "UPDATE warehouse SET warehouseName = @name WHERE warehouseID = @id"
                            Using cmd As New MySqlCommand(updateQuery, conn)
                                cmd.Parameters.AddWithValue("@name", Name)
                                cmd.Parameters.AddWithValue("@id", ID)
                                Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                                MessageBox.Show("Updated warehouse ID " & ID & " from '" & WarehouseNameOld & "'" & vbCrLf & "to '" & Name & "' successfully.")
                            End Using
                        End If
                    End Using
                End Using

            Catch ex As Exception
                MessageBox.Show("Error updating warehouse: " & ex.Message, "Update Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub RefreshDataGrid()
            Warehouses.dataGrid.ItemsSource = Nothing
            Warehouses.InitializeControls()
        End Sub

        Private Sub SaveWarehouse_Click(sender As Object, e As RoutedEventArgs)
            ' Get the brand name from your UI (adjust the control name as needed)
            Dim OldWarehouseName As String = WarehouseNameOld.Trim()
            Dim NewWarehouseName As String = TxtWarehouseName.Text.Trim()

            If NewWarehouseName = OldWarehouseName Then
                MessageBox.Show("Old and New Warehouse name are same.")
                Return
            ElseIf String.IsNullOrWhiteSpace(NewWarehouseName) Then
                MessageBox.Show("Please enter a warehouse name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            Else
                ' Call your UpdateWarehouse method
                UpdateWarehouse(WarehouseID, NewWarehouseName)
                ' Refreshes datagrid after update
                RefreshDataGrid()
            End If


        End Sub
    End Class

End Namespace
