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
                                            ApprovedBy As String,
                                            PaymentTerm As String,
                                            OrderItems As String,
                                            Username As String) As Boolean
            Try
                Dim checkDuplicateQuery As String = "SELECT COUNT(*) FROM deliveryreceipts WHERE DRNumber = @DRNumber"

                Dim addQuery As String = "INSERT INTO deliveryreceipts (DRNumber, ReferenceInvoice, DRDate, ClientName, ClientDetails, DeliveryNotes, ShippingMethod, ApprovedBy, PaymentTerm, OrderItems, Username, DateAdded) " &
                                         "VALUES (@DRNumber, @ReferenceInvoice, @DRDate, @ClientName, @ClientDetails, @DeliveryNotes, @ShippingMethod, @ApprovedBy, @PaymentTerm, @OrderItems, @Username, NOW())"

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
    End Class
End Namespace