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
                ' Query to check for duplicate billingNumber
                Dim findclientIDquery As String = "SELECT 
    ClientID,
FROM client
WHERE Name = @clientName

UNION

SELECT 
    ClientID,
FROM clientcorporational
WHERE Company = @clientName

ORDER BY Name ASC
LIMIT 10;"
                Dim findwarehouseIDquery As String = "SELECT warehouseID FROM warehouse WHERE warehouseName = @warehouseName"
                Dim checkDuplicateQuery As String = "SELECT COUNT(*) FROM walkinbilling WHERE billingNumber = @billingNumber"

                ' Query to insert billing
                Dim addQuery As String =
                    "INSERT INTO walkinbilling (billingNumber, billingDate, DRNo, clientID, companyRep, salesRep, preparedBy, approvedBy, paymentTerms, orderItems, warehouseID, base64img, taxProperty, discountProperty, totalTax, totalDiscount, totalAmount, billingNote, bankDetails, accName, accNo, remarks, dateAdded) VALUES (@billnum, @billdate, @DRNo, @clientID ,@companyRep, @salesRep ,@preparedBy, @approvedBy, @paymentTerms, @orderItems, @warehouseID, @base64img, @taxProperty, @discountProperty, @totalTax, @totalDiscount, @totalAmount, @billingNote, @bankDetails, @accName, @accNo, @remarks, NOW())"

                Dim clientID As String = ""
                Dim warehouseID As String = ""

                ' Find clientID based on clientName
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using findClientCmd As New MySqlCommand(findclientIDquery, conn)
                        findClientCmd.Parameters.AddWithValue("@clientName", clientName)
                        Dim result As Object = findClientCmd.ExecuteScalar()
                        If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                            clientID = result.ToString()
                        Else
                            MessageBox.Show("Client not found. Please add the client first.")
                            Return False
                        End If
                    End Using

                    ' Find warehouseID based on warehouse name
                    Using findWarehouseCmd As New MySqlCommand(findwarehouseIDquery, conn)
                        findWarehouseCmd.Parameters.AddWithValue("@warehouseName", warehouseName)
                        Dim result As Object = findWarehouseCmd.ExecuteScalar()
                        If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                            warehouseID = result.ToString()
                        Else
                            MessageBox.Show("Warehouse not found. Please add the warehouse first.")
                            Return False
                        End If
                    End Using
                End Using

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Check for duplicate billingNumber
                    Using checkCmd As New MySqlCommand(checkDuplicateQuery, conn)
                        checkCmd.Parameters.AddWithValue("@billingNumber", billingNumber)
                        Dim count As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())
                        If count > 0 Then
                            MessageBox.Show("Billing Number already exists. Please use a different number.")
                            Return False
                        End If
                    End Using

                    ' Insert billing if no duplicate
                    Using transaction As MySqlTransaction = conn.BeginTransaction()
                        Try
                            Using addbillingCmd As New MySqlCommand(addQuery, conn, transaction)
                                addbillingCmd.Parameters.AddWithValue("@billnum", billingNumber)
                                addbillingCmd.Parameters.AddWithValue("@billdate", billingDate)
                                addbillingCmd.Parameters.AddWithValue("@DRNo", DRNo)
                                addbillingCmd.Parameters.AddWithValue("@clientID", clientID)
                                addbillingCmd.Parameters.AddWithValue("@companyRep", companyRep)
                                addbillingCmd.Parameters.AddWithValue("@salesRep", salesRep)
                                addbillingCmd.Parameters.AddWithValue("@preparedBy", preparedBy)
                                addbillingCmd.Parameters.AddWithValue("@approvedBy", approvedBy)
                                addbillingCmd.Parameters.AddWithValue("@paymentTerms", paymentTerms)
                                addbillingCmd.Parameters.AddWithValue("@orderItems", OrderItems)
                                addbillingCmd.Parameters.AddWithValue("@base64img", base64Image)
                                addbillingCmd.Parameters.AddWithValue("@warehouseID", warehouseID)
                                addbillingCmd.Parameters.AddWithValue("@taxProperty", tax)
                                addbillingCmd.Parameters.AddWithValue("@discountProperty", discount)
                                addbillingCmd.Parameters.AddWithValue("@totalTax", totalTax)
                                addbillingCmd.Parameters.AddWithValue("@totalDiscount", totalDiscount)
                                addbillingCmd.Parameters.AddWithValue("@totalAmount", totalAmount)
                                addbillingCmd.Parameters.AddWithValue("@billingNote", billingNote)
                                addbillingCmd.Parameters.AddWithValue("@bankDetails", bankDetails)
                                addbillingCmd.Parameters.AddWithValue("@accName", accountName)
                                addbillingCmd.Parameters.AddWithValue("@accNo", accountNumber)
                                addbillingCmd.Parameters.AddWithValue("@remarks", remarks)

                                addbillingCmd.ExecuteNonQuery()
                                transaction.Commit()
                                MessageBox.Show($"Successfully Added the Billing With Number {billingNumber}")
                                Return True
                            End Using
                        Catch ex As Exception
                            transaction.Rollback()
                            MessageBox.Show("Failed to insert the data - " & ex.Message)
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
