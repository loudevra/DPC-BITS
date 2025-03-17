Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model

Namespace DPC.Data.Controllers
    Public Class SupplierController
        ' Fetch Supplier Data Using Existing DB Connection
        Public Shared Function GetSuppliers() As ObservableCollection(Of Supplier)
            Dim supplierList As New ObservableCollection(Of Supplier)()
            Dim query As String = "SELECT supplierid, representative,
                                           CONCAT(officeAddress, ', ', city, ', ', region, ', ', country, ', ', postalcode) AS address, 
                                           email, phoneNumber 
                                    FROM supplier;"

            Try
                Using cmd As New MySqlCommand(query, SplashScreen.DBConnection)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            supplierList.Add(New Supplier With {
                                .ID = reader.GetInt32("supplierid"),
                                .Name = reader.GetString("representative"),
                                .Address = reader.GetString("address"),
                                .Email = reader.GetString("email"),
                                .Phone = reader.GetInt32("phoneNumber")
                            })
                        End While
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error fetching suppliers: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try

            Return supplierList
        End Function
    End Class
End Namespace
