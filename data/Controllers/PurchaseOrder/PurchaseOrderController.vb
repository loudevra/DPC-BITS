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
    Public Class PurchaseOrderController

        Public Shared Function SearchProductsBySupplier(supplierID As String, searchText As String) As ObservableCollection(Of ProductDataModel)
            Dim products As New ObservableCollection(Of ProductDataModel)

            ' In a real implementation, this would query the database
            ' Example implementation:
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Query only the product table and join with productnovariation to get buying price
                    ' This ensures we only get unique products that belong to the selected supplier
                    Dim query As String = "
                    SELECT DISTINCT p.productID, p.productName, 
                           COALESCE(pnv.buyingPrice, 0) AS buyingPrice, 
                           COALESCE(pnv.defaultTax, 0) AS defaultTax,
                           COALESCE(pnv.stockUnit, 0) AS stockUnits,
                           p.measurementUnit, p.supplierID
                    FROM product p
                    LEFT JOIN productnovariation pnv ON p.productID = pnv.productID
                    WHERE p.supplierID = @supplierID 
                    AND p.productName LIKE @searchText
                    GROUP BY p.productID
                    ORDER BY p.productName
                    LIMIT 20"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@supplierID", supplierID)
                        cmd.Parameters.AddWithValue("@searchText", "%" & searchText & "%")

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                products.Add(New ProductDataModel() With {
                                        .ProductID = reader("productID").ToString(),
                                        .ProductName = reader("productName").ToString(),
                                        .BuyingPrice = Convert.ToDecimal(reader("buyingPrice")),
                                        .DefaultTax = Convert.ToDecimal(reader("defaultTax")),
                                        .StockUnits = Convert.ToInt32(reader("stockUnits")),
                                        .MeasurementUnit = If(reader("measurementUnit") Is DBNull.Value, String.Empty, reader("measurementUnit").ToString()),
                                        .SupplierID = reader("supplierID").ToString()
                                    })
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                ' Log error
                Console.WriteLine("Error in SearchProductsBySupplier: " & ex.Message)
            End Try

            Return products
        End Function

        Public Shared Function GenerateInvoice() As String
            Dim prefix As String = "30"
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy") ' MMDDYYYY format
            Dim counter As Integer = GetNextINvoiceCounter(datePart)

            ' Format counter to be 4 digits (e.g., 0001, 0025, 0150)
            Dim counterPart As String = counter.ToString("D4")

            ' Concatenate to get full ProductCode
            Return prefix & datePart & counterPart
        End Function

        Public Shared Function GetNextINvoiceCounter(datePart As String) As Integer

            'will be creating a new table soon
            Dim query As String = "SELECT MAX(CAST(SUBSTRING(InvoiceNumber, 11, 4) AS UNSIGNED)) FROM purchaseorders " &
                  "WHERE InvoiceNumber LIKE '30" & datePart & "%'"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Dim result As Object = cmd.ExecuteScalar()

                        ' If no previous records exist for today, start with 0001
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            Return Convert.ToInt32(result) + 1
                        Else
                            Return 1
                        End If
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error generating Product Code: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return 1
                End Try
            End Using
        End Function

        Public Shared Function InsertPurchaseOrder(InvoiceNumber As String, OrderDate As String, DueDate As String,
                                              Tax As String, Discount As String, SupplierID As String,
                                              SupplierName As String, WarehouseID As Integer, WarehouseName As String,
                                              OrderItems As String, TotalPrice As String, TotalTax As String, TotalDiscount As String, OrderNote As String) As Boolean



            Return InsertPurchaseOrderToDatabase(InvoiceNumber, OrderDate, DueDate,
                                              Tax, Discount, SupplierID,
                                              SupplierName, WarehouseID, WarehouseName,
                                              OrderItems, TotalPrice, TotalTax, TotalDiscount, OrderNote)
        End Function

        Public Shared Function InsertPurchaseOrderToDatabase(InvoiceNumber As String, OrderDate As String, DueDate As String,
                                              Tax As String, Discount As String, SupplierID As String,
                                              SupplierName As String, WarehouseID As Integer, WarehouseName As String,
                                              OrderItems As String, TotalPrice As String, TotalTax As String, TotalDiscount As String, OrderNote As String) As Boolean

            Dim query As String = "INSERT INTO purchaseorders (InvoiceNumber, OrderDate, DueDate, Tax, Discount, SupplierID, SupplierName, WarehouseID, WarehouseName,
                                    OrderItems, TotalTax, TotalDiscount, TotalPrice, OrderNote) VALUES (" & InvoiceNumber & ", '" & OrderDate & "', '" & DueDate & "', '" & Tax & "', '" & Discount & "', " & SupplierID & ", '" &
                                    SupplierName & "', " & WarehouseID & ", '" & WarehouseName & "', '" & OrderItems & "', " & TotalTax & ", " & TotalDiscount & ", " & TotalPrice & ", '" & OrderNote & "')"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()

                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)

                        cmd.ExecuteNonQuery()

                    End Using


                Catch ex As Exception
                    MessageBox.Show($"Error: {ex.Message}")
                    Return False
                End Try

            End Using

            Return True
        End Function
    End Class
End Namespace

