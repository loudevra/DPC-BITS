Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model

Namespace DPC.Data.Controllers
    Public Class WarehouseController
        Public Shared Reload As Boolean

        ' Function to Fetch Business Locations from the Database
        Public Shared Function GetBusinessLocations() As ObservableCollection(Of KeyValuePair(Of Integer, String))
            Dim locations As New ObservableCollection(Of KeyValuePair(Of Integer, String))()
            Try
                ' Use connection from SplashScreen
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT locationID, locationName FROM businesslocation"
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                locations.Add(New KeyValuePair(Of Integer, String)(reader.GetInt32(0), reader.GetString(1)))
                            End While
                        End Using
                    End Using
                End Using ' ✅ Connection automatically returned to the pool
            Catch ex As Exception
                MessageBox.Show($"Error fetching business locations: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
            Return locations
        End Function

        ' Function to Add a Warehouse to the Database
        Public Shared Function AddWarehouse(name As String, description As String, businessLocationID As Integer) As Boolean
            Try
                ' Use connection from SplashScreen
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "INSERT INTO warehouse (warehouseName, description, businessLocationID) VALUES (@name, @description, @businessLocationID)"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@name", name)
                        cmd.Parameters.AddWithValue("@description", description)
                        cmd.Parameters.AddWithValue("@businessLocationID", If(businessLocationID = 0, DBNull.Value, businessLocationID))
                        Dim result As Integer = cmd.ExecuteNonQuery()
                        Return result > 0

                    End Using
                End Using ' ✅ Connection automatically returned to the pool
            Catch ex As Exception
                MessageBox.Show($"Error adding warehouse: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function

        ' Function to Fetch Warehouses Data
        Public Shared Function GetWarehouses() As ObservableCollection(Of Warehouses)
            Dim warehouseList As New ObservableCollection(Of Warehouses)()
            Try
                ' Use connection from SplashScreen
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT warehouseID, warehouseName, description FROM warehouse;"
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                warehouseList.Add(New Warehouses With {
                                    .ID = reader.GetInt32("warehouseID"),
                                    .Name = reader.GetString("warehouseName"),
                                    .Description = reader.GetString("description")})
                            End While
                        End Using
                    End Using
                End Using ' ✅ Connection automatically returned to the pool
            Catch ex As Exception
                MessageBox.Show("Error fetching Warehouses: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
            Return warehouseList
        End Function
    End Class
End Namespace