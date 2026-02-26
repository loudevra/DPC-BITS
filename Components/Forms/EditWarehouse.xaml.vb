Imports DPC.DPC.Views.Stocks.Warehouses
Imports MySql.Data.MySqlClient

Namespace DPC.Components.Forms
    Public Class EditWarehouse
        Public Event WarehouseUpdated()
        Public WarehouseID As Integer
        Public WarehouseNameOld As String
        Public Warehouses As DPC.Views.Stocks.Warehouses.Warehouses

        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()
        End Sub

        ' Populate dialog fields before showing it
        Public Sub LoadWarehouseData(id As Integer, name As String, Optional totalProducts As Nullable(Of Integer) = Nothing, Optional stockQuantity As Nullable(Of Integer) = Nothing, Optional worth As Nullable(Of Decimal) = Nothing)
            WarehouseID = id
            WarehouseNameOld = If(name, String.Empty)

            ' Populate UI
            TxtWarehouseName.Text = WarehouseNameOld
            TxtTotalProducts.Text = If(totalProducts.HasValue, totalProducts.Value.ToString(), String.Empty)
            TxtStockQuantity.Text = If(stockQuantity.HasValue, stockQuantity.Value.ToString(), String.Empty)
            TxtWorth.Text = If(worth.HasValue, worth.Value.ToString("G"), String.Empty)

            ' Focus and select the name field
            TxtWarehouseName.Focus()
            TxtWarehouseName.SelectAll()
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
                                RaiseEvent WarehouseUpdated()
                            End Using
                        End If
                    End Using
                End Using

            Catch ex As Exception
                MessageBox.Show("Error updating warehouse: " & ex.Message, "Update Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub RefreshDataGrid()
            If Warehouses IsNot Nothing Then
                Warehouses.dataGrid.ItemsSource = Nothing
                Warehouses.InitializeControls()
            End If
        End Sub

        Private Sub UpdateWarehouse_Click(sender As Object, e As RoutedEventArgs)
            Dim OldWarehouseName As String = If(WarehouseNameOld, String.Empty).Trim()
            Dim NewWarehouseName As String = TxtWarehouseName.Text.Trim()

            If NewWarehouseName = OldWarehouseName Then
                MessageBox.Show("Old and New Warehouse name are same.")
                Return
            ElseIf String.IsNullOrWhiteSpace(NewWarehouseName) Then
                MessageBox.Show("Please enter a warehouse name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            Else
                UpdateWarehouse(WarehouseID, NewWarehouseName)
                RefreshDataGrid()
                ' Try to close open DialogHost if hosted in it; otherwise close the window.
                Try
                    MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(Nothing, Me)
                Catch
                    Try
                        Me.Close()
                    Catch
                    End Try
                End Try
            End If
        End Sub

        Private Sub ClosePopup(sender As Object, e As RoutedEventArgs)
            Try
                Me.Close()
            Catch
            End Try
        End Sub
    End Class
End Namespace
