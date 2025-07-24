Imports System.Collections.ObjectModel
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports DPC.DPC.Data.Model
Imports MailKit.Search
Imports MySql.Data.MySqlClient
Imports ZXing

Namespace DPC.Data.Controllers.Stocks
    Public Class StocksLabelController

        Public Shared Function GetWarehouse() As List(Of KeyValuePair(Of Integer, String))
            Dim warehouse As New List(Of KeyValuePair(Of Integer, String))
            Dim query As String = "SELECT warehouseID, warehouseName
                                    FROM warehouse;"

            ' Always get a new connection from the pool
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                warehouse.Add(New KeyValuePair(Of Integer, String)(reader.GetInt32("warehouseID"), reader.GetString("warehouseName")))

                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error fetching Warehouses: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using ' ✅ Connection is automatically returned to the pool

            Return warehouse
        End Function

        Public Shared Function SearchProducts(searchText As String, warehouseID As Integer) As ObservableCollection(Of Product)
            Dim products As New ObservableCollection(Of Product)
            Dim query As String = "
        SELECT p.productID,
            p.productName,
            p.productCode,
            pnv.sellingPrice
        FROM product p
        INNER JOIN productnovariation pnv ON p.productID = pnv.productID
        WHERE pnv.warehouseID = " & warehouseID & "
           AND (
                p.productName LIKE @searchText 
                OR p.productID LIKE @searchText 
                OR p.productCode LIKE @searchText
            )

        UNION

        SELECT p.productID,
            p.productName,
            p.productCode,
            pvs.sellingPrice
        FROM product p
        INNER JOIN productvariationstock pvs ON p.productID = pvs.productID
        WHERE pvs.warehouseID = " & warehouseID & "
           AND (
                p.productName LIKE @searchText 
                OR p.productID LIKE @searchText 
                OR p.productCode LIKE @searchText
            )

        ORDER BY productName ASC
        LIMIT 10; 
        "
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@searchText", "%" & searchText & "%")
                        'cmd.Parameters.AddWithValue("@warehouseID", "%" & warehouseID & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim product As New Product With {
                            .ProductName = reader("productName"),
                            .ProductID = reader("productID"),
                            .ProductCode = reader("productCode"),
                            .RetailPrice = reader("sellingPrice")
                        }
                                products.Add(product)
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error searching products: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
            Return products
        End Function

        Public Shared Function GetSerialNumber(productID As String) As List(Of String)

            Dim serialNumbers As New List(Of String)
            Dim query As String = "
                SELECT * 
                FROM serialnumberproduct
                WHERE ProductID = '" & productID & "'
            "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                serialNumbers.Add(reader("SerialNumber"))
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error searching products: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Return serialNumbers
        End Function

        Public Shared Function GenerateBarcode(code As String, width As Double, height As Double, barcodeType As Integer) As BitmapImage
            Dim writer As New BarcodeWriter()
            Select Case barcodeType
                Case 0 ' EAN-13
                    writer.Format = BarcodeFormat.EAN_13
                Case 1 ' Code 128
                    writer.Format = BarcodeFormat.CODE_128
                Case 2 ' CODE 39
                    writer.Format = BarcodeFormat.CODE_39
                Case 3 ' EAN-8
                    writer.Format = BarcodeFormat.EAN_8
                Case 4 ' UPC-A
                    writer.Format = BarcodeFormat.UPC_A
                Case 5 ' UPC-E
                    writer.Format = BarcodeFormat.UPC_E
            End Select
            writer.Options = New ZXing.Common.EncodingOptions With {
        .Width = width,
        .Height = height,
        .PureBarcode = True
    }

            Try
                Using bitmap As Bitmap = writer.Write(code)
                    Using stream As New MemoryStream()
                        bitmap.Save(stream, ImageFormat.Png)
                        stream.Position = 0
                        Dim image As New BitmapImage()
                        image.BeginInit()
                        image.CacheOption = BitmapCacheOption.OnLoad
                        image.StreamSource = stream
                        image.EndInit()
                        image.Freeze()
                        Return image
                    End Using
                End Using
            Catch ex As Exception
            End Try
            Return Nothing
        End Function
    End Class
End Namespace


