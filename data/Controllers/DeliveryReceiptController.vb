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
    Public Class DeliveryReceiptController
        Public Shared Function InsertDeliveryReceipt(DRNumber As String,
                                            ReferenceInvoice As String,
                                            DRDate As String,
                                            ClientName As String,
                                            ClientDetails As String,
                                            DeliveryNotes As String,
                                            ShippingMethod As String,
                                            DeliveryStatus As String,
                                            ApprovedBy As String,
                                            PaymentTerm As String,
                                            OrderItems As String,
                                            Username As String) As Boolean
            Try
                Dim checkDuplicateQuery As String = "SELECT COUNT(*) FROM deliveryreceipts WHERE DRNumber = @DRNumber"

                Dim addQuery As String = "INSERT INTO deliveryreceipts (DRNumber, ReferenceInvoice, DRDate, ClientName, ClientDetails, DeliveryNotes, ShippingMethod, DeliveryStatus, ApprovedBy, PaymentTerm, OrderItems, Username, DateAdded) " &
                                         "VALUES (@DRNumber, @ReferenceInvoice, @DRDate, @ClientName, @ClientDetails, @DeliveryNotes, @ShippingMethod, @DeliveryStatus, @ApprovedBy, @PaymentTerm, @OrderItems, @Username, NOW())"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' 1. Check for duplicate DRNumber
                    Using checkCmd As New MySqlCommand(checkDuplicateQuery, conn)
                        checkCmd.Parameters.AddWithValue("@DRNumber", DRNumber)
                        Dim count As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())
                        If count > 0 Then
                            MessageBox.Show("Delivery Receipt Number already exists. Please use a different number.", "Duplicate Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                            Return False
                        End If
                    End Using

                    ' 2. Insert DR using a Transaction
                    Using transaction As MySqlTransaction = conn.BeginTransaction()
                        Try
                            Using addCmd As New MySqlCommand(addQuery, conn, transaction)
                                addCmd.Parameters.AddWithValue("@DRNumber", DRNumber)
                                addCmd.Parameters.AddWithValue("@ReferenceInvoice", ReferenceInvoice)
                                addCmd.Parameters.AddWithValue("@DRDate", DRDate)
                                addCmd.Parameters.AddWithValue("@ClientName", ClientName)
                                addCmd.Parameters.AddWithValue("@ClientDetails", ClientDetails)
                                addCmd.Parameters.AddWithValue("@DeliveryNotes", DeliveryNotes)
                                addCmd.Parameters.AddWithValue("@ShippingMethod", If(ShippingMethod Is Nothing, "", ShippingMethod))
                                addCmd.Parameters.AddWithValue("@DeliveryStatus", DeliveryStatus)
                                addCmd.Parameters.AddWithValue("@ApprovedBy", ApprovedBy)
                                addCmd.Parameters.AddWithValue("@PaymentTerm", PaymentTerm)
                                addCmd.Parameters.AddWithValue("@OrderItems", OrderItems)
                                addCmd.Parameters.AddWithValue("@Username", Username)

                                addCmd.ExecuteNonQuery()
                                transaction.Commit()

                                MessageBox.Show($"Successfully Added the Delivery Receipt With Number {DRNumber}", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                                Return True
                            End Using
                        Catch ex As Exception
                            transaction.Rollback()
                            MessageBox.Show("Failed to insert the delivery data - " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                            Return False
                        End Try
                    End Using
                End Using

            Catch ex As Exception
                MessageBox.Show("Unexpected error - " & ex.Message, "System Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function

        Public Shared Function GetDeliveryReceipts(limit As Integer) As ObservableCollection(Of DeliveryReceiptModel)
            Dim receipts As New ObservableCollection(Of DeliveryReceiptModel)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "
                SELECT 
                    DRNumber, ReferenceInvoice, DRDate, ClientName, 
                    ClientDetails, DeliveryNotes, ShippingMethod, 
                    DeliveryStatus, ApprovedBy, PaymentTerm, 
                    OrderItems, Username, DateAdded 
                FROM deliveryreceipts 
                ORDER BY DateAdded DESC 
                LIMIT @limit"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@limit", limit)

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                receipts.Add(New DeliveryReceiptModel() With {
                            .DRNumber = reader("DRNumber").ToString(),
                            .ReferenceInvoice = reader("ReferenceInvoice").ToString(),
                            .DRDate = If(reader("DRDate") Is DBNull.Value, "-", reader("DRDate").ToString()),
                            .ClientName = reader("ClientName").ToString(),
                            .ClientDetails = If(reader("ClientDetails") Is DBNull.Value, String.Empty, reader("ClientDetails").ToString()),
                            .DeliveryNotes = If(reader("DeliveryNotes") Is DBNull.Value, String.Empty, reader("DeliveryNotes").ToString()),
                            .ShippingMethod = reader("ShippingMethod").ToString(),
                            .DeliveryStatus = reader("DeliveryStatus").ToString(),
                            .ApprovedBy = If(reader("ApprovedBy") Is DBNull.Value, "-", reader("ApprovedBy").ToString()),
                            .PaymentTerm = If(reader("PaymentTerm") Is DBNull.Value, "-", reader("PaymentTerm").ToString()),
                            .OrderItems = reader("OrderItems").ToString(),
                            .Username = reader("Username").ToString(),
                            .DateAdded = If(reader("DateAdded") Is DBNull.Value, DateTime.MinValue, Convert.ToDateTime(reader("DateAdded")))
                        })
                            End While
                        End Using
                    End Using
                End Using

            Catch ex As Exception
                Debug.WriteLine("Error in GetDeliveryReceipts: " & ex.Message)
            End Try

            Return receipts
        End Function

        Public Shared Function SearchDeliveryReceipts()
            Return New ObservableCollection(Of DeliveryReceiptModel)()
        End Function

        Public Shared Function GetAccumulatedDeliveryTotals(invoiceNo As String) As Dictionary(Of String, Integer)
            Dim totals As New Dictionary(Of String, Integer)
            Dim query As String = "SELECT OrderItems FROM deliveryreceipts WHERE ReferenceInvoice = @inv"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                conn.Open()
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@inv", invoiceNo)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            ' Parse each DR's JSON list
                            Dim items = Newtonsoft.Json.JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(reader("OrderItems").ToString())
                            For Each itm In items
                                Dim name = itm("ProductName")
                                Dim qty = CInt(itm("Quantity"))

                                If totals.ContainsKey(name) Then
                                    totals(name) += qty
                                Else
                                    totals(name) = qty
                                End If
                            Next
                        End While
                    End Using
                End Using
            End Using
            Return totals
        End Function
    End Class
End Namespace