Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports System.IO
Imports DPC.DPC.Data.Controllers
Imports Newtonsoft.Json

Namespace DPC.Data.Controllers
    Public Class QuotesController

        Public Shared Function GetOrders(limit As Integer) As ObservableCollection(Of QuotesModel)
            Dim quotes As New ObservableCollection(Of QuotesModel)

            ' In a real implementation, this would query the database
            ' Example implementation:
            Try


                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()

                    conn.Open()

                    ' Query only the product table and join with productnovariation to get buying price
                    ' This ensures we only get unique products that belong to the selected supplier
                    Dim query As String = "
                    SELECT * FROM quotes 
                    ORDER BY QuoteNumber ASC 
                    LIMIT " & limit
                    Using cmd As New MySqlCommand(query, conn)

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()

                                Dim list As List(Of Dictionary(Of String, Object)) =
    JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, Object)))(reader("OrderItems"))

                                Dim ItemList As String = Nothing

                                For Each item In list
                                    ItemList = ItemList & item("Quantity") & "pc/s " & item("ProductName") & ", "
                                Next

                                ItemList = ItemList.TrimEnd(", ".ToCharArray())

                                quotes.Add(New QuotesModel() With {
                                        .QuoteNumber = reader("QuoteNumber").ToString(),
                                        .Reference = reader("ReferenceNo").ToString(),
                                        .QuoteDate = reader.GetDateTime("QuoteDate").ToString("MMMM d, yyyy"),
                                        .Validity = reader.GetDateTime("QuoteValidity").ToString("MMMM d, yyyy"),
                                        .Tax = reader("Tax").ToString(),
                                        .Discount = reader("Discount").ToString(),
                                        .ClientID = reader("ClientID").ToString(),
                                        .ClientName = reader("ClientName").ToString(),
                                        .WarehouseID = reader("WarehouseID").ToString(),
                                        .WarehouseName = reader("WarehouseName").ToString(),
                                        .OrderItems = ItemList,
                                        .QuoteNote = If(reader("QuoteNote") Is DBNull.Value, String.Empty, reader("QuoteNote").ToString()),
                                        .TotalTax = reader("TotalTax"),
                                        .TotalDiscount = reader("TotalDiscount"),
                                        .TotalPrice = reader("TotalPrice")
                                    })
                            End While
                        End Using
                    End Using

                End Using

            Catch ex As Exception

            End Try

            Return quotes
        End Function



        Public Shared Function GetOrdersSearch(_searchText As String, limit As Integer) As ObservableCollection(Of QuotesModel)
            Dim quotes As New ObservableCollection(Of QuotesModel)

            ' In a real implementation, this would query the database
            ' Example implementation:
            Try


                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()

                    conn.Open()

                    ' Query only the product table and join with productnovariation to get buying price
                    ' This ensures we only get unique products that belong to the selected supplier
                    Dim query As String = "
        SELECT *
        FROM quotes
        WHERE QuoteNumber LIKE @searchText 
            OR QuoteDate LIKE @searchText
            Or QuoteValidity LIKE @searchText
        ORDER BY QuoteNumber ASC
        LIMIT " & limit
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@searchText", "%" & _searchText & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()

                                Dim list As List(Of Dictionary(Of String, Object)) =
    JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, Object)))(reader("OrderItems"))

                                Dim ItemList As String = Nothing

                                For Each item In list
                                    ItemList = ItemList & item("Quantity") & "pc/s " & item("ProductName") & ", "
                                Next

                                ItemList = ItemList.TrimEnd(", ".ToCharArray())

                                quotes.Add(New QuotesModel() With {
                                        .QuoteNumber = reader("QuoteNumber").ToString(),
                                        .Reference = reader("ReferenceNo").ToString(),
                                        .QuoteDate = reader.GetDateTime("QuoteDate").ToString("MMMM d, yyyy"),
                                        .Validity = reader.GetDateTime("QuoteValidity").ToString("MMMM d, yyyy"),
                                        .Tax = reader("Tax").ToString(),
                                        .Discount = reader("Discount").ToString(),
                                        .ClientID = reader("ClientID").ToString(),
                                        .ClientName = reader("ClientName").ToString(),
                                        .WarehouseID = reader("WarehouseID").ToString(),
                                        .WarehouseName = reader("WarehouseName").ToString(),
                                        .OrderItems = ItemList,
                                        .QuoteNote = If(reader("QuoteNote") Is DBNull.Value, String.Empty, reader("QuoteNote").ToString()),
                                        .TotalTax = reader("TotalTax"),
                                        .TotalDiscount = reader("TotalDiscount"),
                                        .TotalPrice = reader("TotalPrice")
                                    })
                            End While
                        End Using
                    End Using

                End Using

            Catch ex As Exception

            End Try

            Return quotes

        End Function


        ' Search for the recent QuoteID then add 1 and check if it exists
        Public Shared Function GenerateQuoteID(Optional CeType As Integer = 0) As String
            Dim prefix As String

            ' Add switch case what is stored in cache CECostEstimate
            Select Case CeType
                Case 0
                    prefix = "GPCE-"
                Case 1
                    prefix = "BCCE-"
                Case 2
                    prefix = "HHCE-"
                Case 3
                    prefix = "WICE-"
                Case Else
                    ' default value of everything is wrong
                    prefix = "CE-"
            End Select

            Dim datePart As String = DateTime.Now.ToString("MMddyyyy") ' MMDDYYYY format
            Dim counter As Integer = GetNextQuoteID(datePart, prefix)

            Dim counterPart As String = counter.ToString("D4") ' e.g., 0001
            Return prefix & datePart & "-" & counterPart
        End Function


        Public Shared Function GetNextQuoteID(datePart As String, CEPrefix As String) As Integer
            Dim nextQuoteID As Integer = 1
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String = "SELECT MAX(CAST(RIGHT(QuoteNumber, 4) AS UNSIGNED)) FROM quotes " &
                                  "WHERE QuoteNumber LIKE @prefix"

                    Using cmd As New MySqlCommand(query, conn)
                        ' Use parameter to safely insert prefix like 'QUO06182025%'
                        cmd.Parameters.AddWithValue("@prefix", CEPrefix & datePart & "-%")

                        Dim result = cmd.ExecuteScalar()
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            nextQuoteID = Convert.ToInt32(result) + 1
                        End If
                    End Using
                End Using
            Catch ex As Exception
                Console.WriteLine("Error in GetNextQuoteID: " & ex.Message)
            End Try

            Return nextQuoteID
        End Function

        Public Shared Function SearchProductsByName(searchText As String, warehouseID As Integer) As ObservableCollection(Of ProductDataModel)
            Dim products As New ObservableCollection(Of ProductDataModel)

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String = "
                                            SELECT DISTINCT p.productID, p.productName, p.measurementUnit
                                            FROM product p
                                            LEFT JOIN productnovariation pnv ON p.productID = pnv.productID AND pnv.warehouseID = @warehouseID
                                            LEFT JOIN productvariationstock pvs ON p.productID = pvs.productID AND pvs.warehouseID = @warehouseID
                                            WHERE p.productName LIKE @searchText
                                              AND (pnv.productID IS NOT NULL OR pvs.productID IS NOT NULL)
                                            ORDER BY p.productName
                                            LIMIT 20"


                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@searchText", "%" & searchText & "%")
                        cmd.Parameters.AddWithValue("@warehouseID", warehouseID)

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                products.Add(New ProductDataModel() With {
                            .ProductID = reader("productID").ToString(),
                            .ProductName = reader("productName").ToString(),
                            .MeasurementUnit = If(reader("measurementUnit") Is DBNull.Value, String.Empty, reader("measurementUnit").ToString())
                        })
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                Debug.WriteLine("Error in SearchProductsByName: " & ex.Message)
            End Try

            Return products
        End Function

        Public Shared Function QuoteNumberExists(quoteNumber As String) As Boolean
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String = "SELECT COUNT(*) FROM quotes WHERE QuoteNumber = @quoteNumber"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@quoteNumber", quoteNumber)
                        Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                        Return count > 0
                    End Using
                End Using
            Catch ex As Exception
                Console.WriteLine("Error in QuoteNumberExists: " & ex.Message)
                Return False
            End Try
        End Function

        ' PNV AND PVS is ProductNoVariation table and ProductVariationStock
        Public Shared Function GetProductDetailsByProductID(productID As String, warehouseID As Integer) As ObservableCollection(Of ProductDataModel)
            Dim productDetails As New ObservableCollection(Of ProductDataModel)

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Check productnovariation
                    Dim pnvQuery As String = "
            SELECT p.productID, p.productName, p.measurementUnit, 
                   pnv.buyingPrice, pnv.sellingPrice, pnv.defaultTax, pnv.stockUnit
            FROM productnovariation pnv
            INNER JOIN product p ON p.productID = pnv.productID
            WHERE pnv.productID = @productID AND pnv.warehouseID = @warehouseID
            LIMIT 1"

                    Using cmdPNV As New MySqlCommand(pnvQuery, conn)
                        cmdPNV.Parameters.AddWithValue("@productID", productID)
                        cmdPNV.Parameters.AddWithValue("@warehouseID", warehouseID)

                        Using reader As MySqlDataReader = cmdPNV.ExecuteReader()
                            If reader.Read() Then
                                productDetails.Add(New ProductDataModel With {
                            .ProductID = reader("productID").ToString(),
                            .ProductName = reader("productName").ToString(),
                            .MeasurementUnit = reader("measurementUnit").ToString(),
                            .BuyingPrice = Convert.ToDecimal(reader("buyingPrice")),
                            .SellingPrice = Convert.ToDecimal(reader("sellingPrice")),
                            .DefaultTax = Convert.ToDecimal(reader("defaultTax")),
                            .StockUnits = Convert.ToInt32(reader("stockUnit"))
                        })
                                Return productDetails
                            End If
                        End Using
                    End Using

                    ' Check productvariationstock if not found in productnovariation
                    Dim pvsQuery As String = "
            SELECT p.productID, p.productName, p.measurementUnit, 
                   pvs.buyingPrice, pvs.sellingPrice, pvs.defaultTax, pvs.stockUnit
            FROM productvariationstock pvs
            INNER JOIN product p ON p.productID = pvs.productID
            WHERE pvs.productID = @productID AND pvs.warehouseID = @warehouseID
            LIMIT 1"

                    Using cmdPVS As New MySqlCommand(pvsQuery, conn)
                        cmdPVS.Parameters.AddWithValue("@productID", productID)
                        cmdPVS.Parameters.AddWithValue("@warehouseID", warehouseID)

                        Using reader As MySqlDataReader = cmdPVS.ExecuteReader()
                            If reader.Read() Then
                                productDetails.Add(New ProductDataModel With {
                            .ProductID = reader("productID").ToString(),
                            .ProductName = reader("productName").ToString(),
                            .MeasurementUnit = reader("measurementUnit").ToString(),
                            .BuyingPrice = Convert.ToDecimal(reader("buyingPrice")),
                            .SellingPrice = Convert.ToDecimal(reader("sellingPrice")),
                            .DefaultTax = Convert.ToDecimal(reader("defaultTax")),
                            .StockUnits = Convert.ToInt32(reader("stockUnit"))
                        })
                                Return productDetails
                            End If
                        End Using
                    End Using

                    Debug.WriteLine("Product not found in PNV or PVS.")
                End Using
            Catch ex As Exception
                Debug.WriteLine("Error in GetProductDetailsByProductID: " & ex.Message)
            End Try

            Return productDetails
        End Function

        Public Shared Function InsertQuote(QuoteNumber As String,
                                   ReferenceNo As String,
                                   QuoteDate As DateTime,
                                   QuoteValidity As String,
                                   Tax As String,
                                   Discount As String,
                                   ClientID As String,
                                   ClientName As String,
                                   WarehouseID As String,
                                   WarehouseName As String,
                                   OrderItems As String,
                                   QuoteNote As String,
                                   TotalTax As String,
                                   TotalDiscount As String,
                                   TotalPrice As String,
                                   Username As String,
                                   ApprovedBy As String,
                                   PaymentTerms As String) As Boolean
            Try
                ' Query to check for duplicate QuoteNumber
                Dim checkDuplicateQuery As String = "SELECT COUNT(*) FROM quotes WHERE QuoteNumber = @QuoteNumber"
                ' Query to insert quote
                Dim addQuery As String = "INSERT INTO quotes (QuoteNumber, ReferenceNo, QuoteDate, QuoteValidity, Tax, Discount, ClientID, ClientName, WarehouseID, WarehouseName, OrderItems, QuoteNote, TotalTax, TotalDiscount, TotalPrice, Username, ApprovedBy, PaymentTerms, DateAdded) VALUES (@QuoteNumber, @ReferenceNo, @QuoteDate, @QuoteValidity, @Tax, @Discount, @ClientID, @ClientName, @WarehouseID, @WarehouseName, @OrderItems, @QuoteNote, @TotalTax, @TotalDiscount, @TotalPrice, @Username, @ApprovedBy, @PaymentTerms, NOW())"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Check for duplicate QuoteNumber
                    Using checkCmd As New MySqlCommand(checkDuplicateQuery, conn)
                        checkCmd.Parameters.AddWithValue("@QuoteNumber", QuoteNumber)
                        Dim count As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())
                        If count > 0 Then
                            MessageBox.Show("Quote Number already exists. Please use a different number.")
                            Return False
                        End If
                    End Using

                    ' Insert quote if no duplicate
                    Using transaction As MySqlTransaction = conn.BeginTransaction()
                        Try
                            Using addQuoteCmd As New MySqlCommand(addQuery, conn, transaction)
                                addQuoteCmd.Parameters.AddWithValue("@QuoteNumber", QuoteNumber)
                                addQuoteCmd.Parameters.AddWithValue("@ReferenceNo", ReferenceNo)
                                addQuoteCmd.Parameters.AddWithValue("@QuoteDate", QuoteDate)
                                addQuoteCmd.Parameters.AddWithValue("@QuoteValidity", QuoteValidity)
                                addQuoteCmd.Parameters.AddWithValue("@Tax", Tax)
                                addQuoteCmd.Parameters.AddWithValue("@Discount", Discount)
                                addQuoteCmd.Parameters.AddWithValue("@ClientID", ClientID)
                                addQuoteCmd.Parameters.AddWithValue("@ClientName", ClientName)
                                addQuoteCmd.Parameters.AddWithValue("@WarehouseID", WarehouseID)
                                addQuoteCmd.Parameters.AddWithValue("@WarehouseName", WarehouseName)
                                addQuoteCmd.Parameters.AddWithValue("@OrderItems", OrderItems)
                                addQuoteCmd.Parameters.AddWithValue("@QuoteNote", QuoteNote)
                                addQuoteCmd.Parameters.AddWithValue("@TotalTax", TotalTax)
                                addQuoteCmd.Parameters.AddWithValue("@TotalDiscount", TotalDiscount)
                                addQuoteCmd.Parameters.AddWithValue("@TotalPrice", TotalPrice)
                                addQuoteCmd.Parameters.AddWithValue("@Username", Username)
                                addQuoteCmd.Parameters.AddWithValue("@ApprovedBy", ApprovedBy)
                                addQuoteCmd.Parameters.AddWithValue("@PaymentTerms", PaymentTerms)

                                addQuoteCmd.ExecuteNonQuery()
                                transaction.Commit()
                                MessageBox.Show($"Successfully Added the Quote With Number {QuoteNumber}")
                            End Using
                        Catch ex As Exception
                            transaction.Rollback()
                            MessageBox.Show("Failed to insert the data - " & ex.Message)
                            Return False
                        End Try
                    End Using
                End Using

                Return True
            Catch ex As Exception
                MessageBox.Show("Unexpected error - " & ex.Message)
                Return False
            End Try
        End Function


    End Class
End Namespace