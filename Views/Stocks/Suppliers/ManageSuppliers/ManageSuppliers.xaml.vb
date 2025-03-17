Imports System.ComponentModel
Imports System.Windows
Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports Org.BouncyCastle.Asn1.X509.SigI

Namespace DPC.Views.Stocks.Suppliers.ManageSuppliers

    Public Class ManageSuppliers
        Inherits Window

        Public Sub New()
            InitializeComponent()
            LoadData()
        End Sub


        Public Sub LoadData()
            Dim connectionString As String = "server=localhost;userid=root;password=;database=dpc;"
            Dim query As String = "SELECT supplierid, representative,
                                           CONCAT(officeAddress, ', ', city, ', ', region, ', ', country, ', ', postalcode) AS address, 
                                           email, phoneNumber 
                                    From supplier;"


            Dim dataList As New ObservableCollection(Of PersonData)

            Try
                Using conn As New MySqlConnection(connectionString)
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                dataList.Add(New PersonData With {
                                    .ID = reader.GetInt32("supplierid"),
                                    .Name = reader.GetString("representative"),
                                    .Address = reader.GetString("address"),
                                    .Email = reader.GetString("email"),
                                    .Phone = reader.GetInt32("phoneNumber")
                                })
                            End While
                        End Using
                    End Using
                End Using

                ' Bind Data to DataGrid
                dataGrid.ItemsSource = dataList

            Catch ex As Exception
                MessageBox.Show("Error: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
    End Class

    Public Class PersonData
        Public Property ID As Integer
        Public Property Name As String
        Public Property Address As String
        Public Property Email As String
        Public Property Phone As Integer
    End Class

    ' ViewModel for Date Range Picker

End Namespace
