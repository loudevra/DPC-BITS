Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports System.IO
Imports DPC.DPC.Data.Controllers

Namespace DPC.Data.Controllers
    Public Class WalkInController
        ' Search for the recent BillingID then add 1 and check if it exists
        Public Shared Function GenerateBillingID() As String
            Dim prefix As String = "BL-"
            Dim datePart As String = DateTime.Now.ToString("MM-dd-yyyy") ' MMDDYYYY format
            Dim counter As Integer = GetNextBillingID(datePart)

            Dim counterPart As String = counter.ToString("D4") ' e.g., 0001
            Return prefix & datePart & "-" & counterPart
        End Function

        Public Shared Function GetNextBillingID(datePart As String) As Integer
            Dim nextBillingID As Integer = 1
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String = "SELECT MAX(CAST(RIGHT(billingNumber, 4) AS UNSIGNED)) FROM walkinbilling " &
                                  "WHERE billingNumber LIKE @prefix"

                    Using cmd As New MySqlCommand(query, conn)
                        ' Use parameter to safely insert prefix like 'QUO06182025%'
                        cmd.Parameters.AddWithValue("@prefix", "BL-" & datePart & "-%")

                        Dim result = cmd.ExecuteScalar()
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            nextBillingID = Convert.ToInt32(result) + 1
                        End If
                    End Using
                End Using
            Catch ex As Exception
                Console.WriteLine("Error in GetNextBillingID: " & ex.Message)
            End Try

            Return nextBillingID
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

        Public Shared Function BillingNumberExists(billingNumber As String) As Boolean
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String = "SELECT COUNT(*) FROM walkinbilling WHERE billingNumber = @billingNumber"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@billingNumber", billingNumber)
                        Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                        Return count > 0
                    End Using
                End Using
            Catch ex As Exception
                Console.WriteLine("Error in BillingNumberExists: " & ex.Message)
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
                   pnv.buyingPrice, pnv.defaultTax, pnv.stockUnit
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
                   pvs.buyingPrice, pvs.defaultTax, pvs.stockUnit
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

        Public Shared Function InsertBilling(
                                    billingNumber As String,
                                    billingDate As DateTime,
                                    DRNo As String,
                                    clientName As String,
                                    companyRep As String,
                                    salesRep As String,
                                    preparedBy As String,
                                    approvedBy As String,
                                    paymentTerms As String,
                                    OrderItems As String,
                                    warehouseName As String,
                                    base64Image As String,
                                    tax As String,
                                    discount As String,
                                    totalTax As String,
                                    totalDiscount As String,
                                    totalAmount As String,
                                    billingNote As String,
                                    bankDetails As String,
                                    accountName As String,
                                    accountNumber As String,
                                    remarks As String) As Boolean
            Try

                Dim findclientIDquery As String = "SELECT ClientID FROM client WHERE Name = @clientName UNION SELECT ClientID FROM clientcorporational WHERE Company = @clientName"
                Dim findwarehouseIDquery As String = "SELECT warehouseID FROM warehouse WHERE warehouseName = @warehouseName"
                Dim checkDuplicateQuery As String = "SELECT COUNT(*) FROM walkinbilling WHERE billingNumber = @billingNumber"

                Dim insertQuery As String = "INSERT INTO walkinbilling (billingNumber, billingDate, DRNo, clientID, companyRep, salesRep, preparedBy, approvedBy, paymentTerms, orderItems, warehouseID, base64img, taxProperty, discountProperty, totalTax, totalDiscount, totalAmount, billingNote, bankDetails, accName, accNo, remarks, dateAdded) VALUES (@billnum, @billdate, @DRNo, @clientID ,@companyRep, @salesRep ,@preparedBy, @approvedBy, @paymentTerms, @orderItems, @warehouseID, @base64img, @taxProperty, @discountProperty, @totalTax, @totalDiscount, @totalAmount, @billingNote, @bankDetails, @accName, @accNo, @remarks, NOW())"


                Dim updateQuery As String = "UPDATE walkinbilling SET billingDate=@billdate, DRNo=@DRNo, clientID=@clientID, companyRep=@companyRep, salesRep=@salesRep, preparedBy=@preparedBy, approvedBy=@approvedBy, paymentTerms=@paymentTerms, orderItems=@orderItems, warehouseID=@warehouseID, base64img=@base64img, taxProperty=@taxProperty, discountProperty=@discountProperty, totalTax=@totalTax, totalDiscount=@totalDiscount, totalAmount=@totalAmount, billingNote=@billingNote, bankDetails=@bankDetails, accName=@accName, accNo=@accNo, remarks=@remarks WHERE billingNumber=@billnum"

                Dim clientID As String = ""
                Dim warehouseID As String = ""

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Using findCmd As New MySqlCommand(findclientIDquery, conn)
                        findCmd.Parameters.AddWithValue("@clientName", clientName)
                        Dim res = findCmd.ExecuteScalar()
                        If res IsNot Nothing Then clientID = res.ToString() Else MessageBox.Show("Client not found.") : Return False
                    End Using

                    Using findWhCmd As New MySqlCommand(findwarehouseIDquery, conn)
                        findWhCmd.Parameters.AddWithValue("@warehouseName", warehouseName)
                        Dim res = findWhCmd.ExecuteScalar()
                        If res IsNot Nothing Then warehouseID = res.ToString() Else MessageBox.Show("Warehouse not found.") : Return False
                    End Using

                    Dim isUpdate As Boolean = False
                    Using checkCmd As New MySqlCommand(checkDuplicateQuery, conn)
                        checkCmd.Parameters.AddWithValue("@billingNumber", billingNumber)
                        isUpdate = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0
                    End Using

                    Using transaction As MySqlTransaction = conn.BeginTransaction()
                        Try
                            Dim finalQuery As String = If(isUpdate, updateQuery, insertQuery)
                            Using cmd As New MySqlCommand(finalQuery, conn, transaction)
                                cmd.Parameters.AddWithValue("@billnum", billingNumber)
                                cmd.Parameters.AddWithValue("@billdate", billingDate)
                                cmd.Parameters.AddWithValue("@DRNo", DRNo)
                                cmd.Parameters.AddWithValue("@clientID", clientID)
                                cmd.Parameters.AddWithValue("@companyRep", companyRep)
                                cmd.Parameters.AddWithValue("@salesRep", salesRep)
                                cmd.Parameters.AddWithValue("@preparedBy", preparedBy)
                                cmd.Parameters.AddWithValue("@approvedBy", approvedBy)
                                cmd.Parameters.AddWithValue("@paymentTerms", paymentTerms)
                                cmd.Parameters.AddWithValue("@orderItems", OrderItems)
                                cmd.Parameters.AddWithValue("@base64img", base64Image)
                                cmd.Parameters.AddWithValue("@warehouseID", warehouseID)
                                cmd.Parameters.AddWithValue("@taxProperty", tax)
                                cmd.Parameters.AddWithValue("@discountProperty", discount)
                                cmd.Parameters.AddWithValue("@totalTax", totalTax)
                                cmd.Parameters.AddWithValue("@totalDiscount", totalDiscount)
                                cmd.Parameters.AddWithValue("@totalAmount", totalAmount)
                                cmd.Parameters.AddWithValue("@billingNote", billingNote)
                                cmd.Parameters.AddWithValue("@bankDetails", bankDetails)
                                cmd.Parameters.AddWithValue("@accName", accountName)
                                cmd.Parameters.AddWithValue("@accNo", accountNumber)
                                cmd.Parameters.AddWithValue("@remarks", remarks)

                                cmd.ExecuteNonQuery()
                            End Using

                            transaction.Commit()
                            Dim msg As String = If(isUpdate, "Updated", "Added")
                            MessageBox.Show($"Successfully {msg} the Billing With Number {billingNumber}")
                            Return True
                        Catch ex As Exception
                            transaction.Rollback()
                            MessageBox.Show("Database error - " & ex.Message)
                            Return False
                        End Try
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Unexpected error - " & ex.Message)
                Return False
            End Try
        End Function
    End Class
End Namespace
