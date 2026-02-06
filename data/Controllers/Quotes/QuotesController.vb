Imports System.Collections.ObjectModel
Imports System.Data
Imports System.IO
Imports System.ServiceModel
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Namespace DPC.Data.Controllers
    Public Class QuotesController

        ''' Gets all quotes from the database with a limit
        Public Shared Function GetQuotes(limit As Integer) As ObservableCollection(Of QuotesModel)
            Dim quotes As New ObservableCollection(Of QuotesModel)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "
                        SELECT * FROM quotes 
                        ORDER BY QuoteNumber DESC
                        LIMIT @limit"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@limit", limit)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim itemList = ParseOrderItems(reader("OrderItems").ToString())
                                quotes.Add(New QuotesModel() With {
                                    .QuoteNumber = reader("QuoteNumber").ToString(),
                                    .Reference = If(reader("ReferenceNo") Is DBNull.Value, "-", reader("ReferenceNo").ToString()),
                                    .QuoteDate = If(reader("QuoteDate") Is DBNull.Value, "-", reader.GetDateTime("QuoteDate").ToString("MMM d, yyyy")),
                                    .Validity = If(reader("QuoteValidity") Is DBNull.Value, "-", reader.GetDateTime("QuoteValidity").ToString("MMM d, yyyy")),
                                    .Tax = reader("Tax").ToString(),
                                    .Discount = reader("Discount").ToString(),
                                    .ClientID = reader("ClientID").ToString(),
                                    .ClientName = reader("ClientName").ToString(),
                                    .WarehouseID = reader("WarehouseID").ToString(),
                                    .WarehouseName = reader("WarehouseName").ToString(),
                                    .OrderItems = itemList,
                                    .QuoteNote = If(reader("QuoteNote") Is DBNull.Value, String.Empty, reader("QuoteNote").ToString()),
                                    .TotalTax = If(reader("TotalTax") Is DBNull.Value, 0, reader("TotalTax")),
                                    .TotalDiscount = If(reader("TotalDiscount") Is DBNull.Value, 0, reader("TotalDiscount")),
                                    .TotalPrice = If(reader("TotalPrice") Is DBNull.Value, 0, reader("TotalPrice"))
                                })
                            End While
                        End Using
                    End Using
                End Using

            Catch ex As Exception
                Debug.WriteLine("Error in GetQuotes: " & ex.Message)
            End Try

            Return quotes
        End Function


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


        ''' Searches quotes based on search criteria
        Public Shared Function SearchQuotes(searchText As String, limit As Integer) As ObservableCollection(Of QuotesModel)
            Dim quotes As New ObservableCollection(Of QuotesModel)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "
                        SELECT * FROM quotes
                        WHERE QuoteNumber LIKE @searchText 
                            OR ReferenceNo LIKE @searchText
                            OR ClientName LIKE @searchText
                            OR WarehouseName LIKE @searchText
                        ORDER BY QuoteNumber DESC
                        LIMIT @limit"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@searchText", "%" & searchText & "%")
                        cmd.Parameters.AddWithValue("@limit", limit)

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim itemList = ParseOrderItems(reader("OrderItems").ToString())
                                quotes.Add(New QuotesModel() With {
                                    .QuoteNumber = reader("QuoteNumber").ToString(),
                                    .Reference = If(reader("ReferenceNo") Is DBNull.Value, "-", reader("ReferenceNo").ToString()),
                                    .QuoteDate = If(reader("QuoteDate") Is DBNull.Value, "-", reader.GetDateTime("QuoteDate").ToString("MMM d, yyyy")),
                                    .Validity = If(reader("QuoteValidity") Is DBNull.Value, "-", reader.GetDateTime("QuoteValidity").ToString("MMM d, yyyy")),
                                    .Tax = reader("Tax").ToString(),
                                    .Discount = ("Discount").ToString(),
                                    .ClientID = reader("ClientID").ToString(),
                                    .ClientName = reader("ClientName").ToString(),
                                    .WarehouseID = reader("WarehouseID").ToString(),
                                    .WarehouseName = reader("WarehouseName").ToString(),
                                    .OrderItems = itemList,
                                    .QuoteNote = If(reader("QuoteNote") Is DBNull.Value, String.Empty, reader("QuoteNote").ToString()),
                                    .TotalTax = If(reader("TotalTax") Is DBNull.Value, 0, reader("TotalTax")),
                                    .TotalDiscount = If(reader("TotalDiscount") Is DBNull.Value, 0, reader("TotalDiscount")),
                                    .TotalPrice = If(reader("TotalPrice") Is DBNull.Value, 0, reader("TotalPrice"))
                                })
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                Debug.WriteLine("Error in SearchQuotes: " & ex.Message)
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


        ''' Helper function to parse order items from JSON
        Private Shared Function ParseOrderItems(jsonItems As String) As String
            Try
                If String.IsNullOrWhiteSpace(jsonItems) Then Return "-"
                Dim list As List(Of Dictionary(Of String, Object)) =
                    JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, Object)))(jsonItems)
                Dim itemList As String = ""
                For Each item In list
                    itemList &= item("Quantity") & "pc/s " & item("ProductName") & ", "
                Next
                Return If(String.IsNullOrWhiteSpace(itemList), "-", itemList.TrimEnd(", ".ToCharArray()))
            Catch
                Return "-"
            End Try
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


        Public Shared Function GetQuoteByNumber(quoteNumber As String) As QuotesModel
            Dim quote As New QuotesModel()
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT * FROM quotes WHERE QuoteNumber = @quoteNumber LIMIT 1"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@quoteNumber", quoteNumber)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            If reader.Read() Then
                                quote.QuoteNumber = reader("QuoteNumber").ToString()
                                quote.Reference = If(reader("ReferenceNo") Is DBNull.Value, "", reader("ReferenceNo").ToString())
                                quote.QuoteDate = If(reader("QuoteDate") Is DBNull.Value, "", reader.GetDateTime("QuoteDate").ToString("yyyy-MM-dd"))
                                quote.Validity = If(reader("QuoteValidity") Is DBNull.Value, "", reader.GetDateTime("QuoteValidity").ToString("yyyy-MM-dd"))
                                quote.Tax = If(reader("Tax") Is DBNull.Value, "", reader("Tax").ToString())
                                quote.Discount = If(reader("Discount") Is DBNull.Value, "", reader("Discount").ToString())
                                quote.ClientID = If(reader("ClientID") Is DBNull.Value, "", reader("ClientID").ToString())
                                quote.ClientName = If(reader("ClientName") Is DBNull.Value, "", reader("ClientName").ToString())
                                quote.WarehouseID = If(reader("WarehouseID") Is DBNull.Value, "", reader("WarehouseID").ToString())
                                quote.WarehouseName = If(reader("WarehouseName") Is DBNull.Value, "", reader("WarehouseName").ToString())
                                quote.OrderItems = If(reader("OrderItems") Is DBNull.Value, "", reader("OrderItems").ToString())
                                quote.QuoteNote = If(reader("QuoteNote") Is DBNull.Value, "", reader("QuoteNote").ToString())
                                quote.TotalTax = If(reader("TotalTax") Is DBNull.Value, 0, reader("TotalTax"))
                                quote.TotalDiscount = If(reader("TotalDiscount") Is DBNull.Value, 0, reader("TotalDiscount"))
                                quote.TotalPrice = If(reader("TotalPrice") Is DBNull.Value, 0, reader("TotalPrice"))
                            End If
                        End Using
                    End Using
                End Using

            Catch ex As Exception
                Debug.WriteLine("Error in GetQuoteByNumber: " & ex.Message)
            End Try
            Return quote
        End Function


        ''' Updates an existing quote in the database
        Public Shared Function UpdateQuote(QuoteNumber As String,
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
                           paymentTerms As String) As Boolean

            Try
                ' Step 1: Verify quote exists BEFORE attempting update
                If Not QuoteNumberExists(QuoteNumber) Then
                    MessageBox.Show("Quote Number '" & QuoteNumber & "' does not exist. Cannot update.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return False
                End If

                ' Step 2: Build UPDATE query - REMOVED DateUpdated = NOW()
                Dim updateQuery As String = "UPDATE quotes SET " &
                                            "ReferenceNo = @ReferenceNo, " &
                                            "QuoteDate = @QuoteDate, " &
                                            "QuoteValidity = @QuoteValidity, " &
                                            "Tax = @Tax, " &
                                            "Discount = @Discount, " &
                                            "ClientID = @ClientID, " &
                                            "ClientName = @ClientName, " &
                                            "WarehouseID = @WarehouseID, " &
                                            "WarehouseName = @WarehouseName, " &
                                            "OrderItems = @OrderItems, " &
                                            "QuoteNote = @QuoteNote, " &
                                            "TotalTax = @TotalTax, " &
                                            "TotalDiscount = @TotalDiscount, " &
                                            "TotalPrice = @TotalPrice, " &
                                            "Username = @Username, " &
                                            "ApprovedBy = @ApprovedBy, " &
                                            "PaymentTerms = @PaymentTerms " &
                                            "WHERE QuoteNumber = @QuoteNumber"

                ' Step 3: Execute update with transaction
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using transaction As MySqlTransaction = conn.BeginTransaction()

                        Try
                            Using updateCmd As New MySqlCommand(updateQuery, conn, transaction)
                                ' Add all parameters
                                updateCmd.Parameters.AddWithValue("@QuoteNumber", QuoteNumber)
                                updateCmd.Parameters.AddWithValue("@ReferenceNo", If(String.IsNullOrEmpty(ReferenceNo), "", ReferenceNo))
                                updateCmd.Parameters.AddWithValue("@QuoteDate", QuoteDate)
                                updateCmd.Parameters.AddWithValue("@QuoteValidity", QuoteValidity)
                                updateCmd.Parameters.AddWithValue("@Tax", If(String.IsNullOrEmpty(Tax), "0", Tax))
                                updateCmd.Parameters.AddWithValue("@Discount", If(String.IsNullOrEmpty(Discount), "0", Discount))
                                updateCmd.Parameters.AddWithValue("@ClientID", ClientID)
                                updateCmd.Parameters.AddWithValue("@ClientName", ClientName)
                                updateCmd.Parameters.AddWithValue("@WarehouseID", WarehouseID)
                                updateCmd.Parameters.AddWithValue("@WarehouseName", WarehouseName)
                                updateCmd.Parameters.AddWithValue("@OrderItems", OrderItems)
                                updateCmd.Parameters.AddWithValue("@QuoteNote", If(String.IsNullOrEmpty(QuoteNote), "", QuoteNote))
                                updateCmd.Parameters.AddWithValue("@TotalTax", TotalTax)
                                updateCmd.Parameters.AddWithValue("@TotalDiscount", TotalDiscount)
                                updateCmd.Parameters.AddWithValue("@TotalPrice", TotalPrice)
                                updateCmd.Parameters.AddWithValue("@Username", Username)
                                updateCmd.Parameters.AddWithValue("@ApprovedBy", If(String.IsNullOrEmpty(ApprovedBy), "", ApprovedBy))
                                updateCmd.Parameters.AddWithValue("@PaymentTerms", If(String.IsNullOrEmpty(paymentTerms), "", paymentTerms))

                                ' Execute and get rows affected
                                Dim rowsAffected As Integer = updateCmd.ExecuteNonQuery()
                                If rowsAffected > 0 Then
                                    transaction.Commit()
                                    Debug.WriteLine($"UpdateQuote: Successfully updated {rowsAffected} row(s) for QuoteNumber {QuoteNumber}")
                                    MessageBox.Show($"Quote {QuoteNumber} has been successfully updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                                    Return True
                                Else
                                    transaction.Rollback()
                                    MessageBox.Show("No rows were updated. Quote number may have changed.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Warning)
                                    Return False
                                End If
                            End Using

                        Catch ex As Exception
                            transaction.Rollback()
                            Debug.WriteLine("UpdateQuote Error: " & ex.Message & vbCrLf & ex.StackTrace)
                            MessageBox.Show("Failed to update the quote: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                            Return False
                        End Try

                    End Using
                End Using

            Catch ex As Exception
                Debug.WriteLine("UpdateQuote Unexpected Error: " & ex.Message & vbCrLf & ex.StackTrace)
                MessageBox.Show("Unexpected error during update: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try

        End Function


        Public Shared Function SearchProductsByNameAllWarehouses(searchText As String) As ObservableCollection(Of ProductDataModel)
            Dim products As New ObservableCollection(Of ProductDataModel)

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String = "
                                    SELECT DISTINCT p.productID, p.productName, p.measurementUnit
                                    FROM product p
                                    LEFT JOIN productnovariation pnv ON p.productID = pnv.productID
                                    LEFT JOIN productvariationstock pvs ON p.productID = pvs.productID
                                    WHERE p.productName LIKE @searchText
                                      AND (pnv.productID IS NOT NULL OR pvs.productID IS NOT NULL)
                                    ORDER BY p.productName
                                    LIMIT 20"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@searchText", "%" & searchText & "%")

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
                Debug.WriteLine("Error in SearchProductsByNameAllWarehouses: " & ex.Message)
            End Try

            Return products
        End Function

        ''' <summary>
        ''' Overloaded version - Gets product details across ALL warehouses (first match)
        ''' Used by NewQuoteGovernment which doesn't have a specific warehouse selected
        ''' </summary>
        Public Shared Function GetProductDetailsByProductIDAllWarehouses(productID As String) As ObservableCollection(Of ProductDataModel)
            Dim productDetails As New ObservableCollection(Of ProductDataModel)

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Check productnovariation first
                    Dim pnvQuery As String = "
            SELECT p.productID, p.productName, p.measurementUnit, 
                   pnv.buyingPrice, pnv.sellingPrice, pnv.defaultTax, pnv.stockUnit
            FROM productnovariation pnv
            INNER JOIN product p ON p.productID = pnv.productID
            WHERE pnv.productID = @productID
            LIMIT 1"

                    Using cmdPNV As New MySqlCommand(pnvQuery, conn)
                        cmdPNV.Parameters.AddWithValue("@productID", productID)

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
            WHERE pvs.productID = @productID
            LIMIT 1"

                    Using cmdPVS As New MySqlCommand(pvsQuery, conn)
                        cmdPVS.Parameters.AddWithValue("@productID", productID)

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

                    Debug.WriteLine("Product not found in PNV or PVS (All Warehouses search).")
                End Using
            Catch ex As Exception
                Debug.WriteLine("Error in GetProductDetailsByProductIDAllWarehouses: " & ex.Message)
            End Try

            Return productDetails
        End Function

    End Class
End Namespace