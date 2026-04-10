Imports System.Collections.ObjectModel
Imports System.Data
Imports System.IO
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.HRM.Employees.Employees.EmployeesProfile.EmployeesProfileControls
Imports MongoDB.Driver
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

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

                ' UPDATED: Added CreatedBy field
                Dim addQuery As String = "INSERT INTO deliveryreceipts (DRNumber, ReferenceInvoice, DRDate, ClientName, ClientDetails, DeliveryNotes, ShippingMethod, DeliveryStatus, ApprovedBy, PaymentTerm, OrderItems, Username, CreatedBy, DateAdded) " &
                                         "VALUES (@DRNumber, @ReferenceInvoice, @DRDate, @ClientName, @ClientDetails, @DeliveryNotes, @ShippingMethod, @DeliveryStatus, @ApprovedBy, @PaymentTerm, @OrderItems, @Username, @CreatedBy, NOW())"

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
                                ' ADDED: Set the current user as the creator
                                addCmd.Parameters.AddWithValue("@CreatedBy", CacheOnEmployeeID)

                                addCmd.ExecuteNonQuery()
                                transaction.Commit()

                                Dim billingUpdated = UpdateRelatedBilling(DRNumber, ReferenceInvoice)

                                If billingUpdated Then
                                    MessageBox.Show($"Successfully Added the Delivery Receipt With Number {DRNumber}", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                                Else
                                    MessageBox.Show($"Added {DRNumber}, but failed to link to Billing record.", "Partial Success", MessageBoxButton.OK, MessageBoxImage.Information)
                                End If
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

        ''' <summary>
        ''' Gets delivery receipts with user-based filtering
        ''' </summary>
        Public Shared Function GetDeliveryReceipts(limit As Integer, currentUserID As String, isAdmin As Boolean) As ObservableCollection(Of UniversalTransactionModel)
            Dim receipts As New ObservableCollection(Of UniversalTransactionModel)

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String
                    If isAdmin Then
                        ' Admin sees all records
                        query = "SELECT * FROM deliveryreceipts ORDER BY dateAdded DESC LIMIT @Limit"
                    Else
                        ' Regular users see only their own records
                        query = "SELECT * FROM deliveryreceipts WHERE CreatedBy = @CreatedBy ORDER BY dateAdded DESC LIMIT @Limit"
                    End If

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@Limit", limit)
                        If Not isAdmin Then
                            cmd.Parameters.AddWithValue("@CreatedBy", currentUserID)
                        End If

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                receipts.Add(MapReaderToUniversalModel(reader))
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show($"Error loading delivery receipts: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try

            Return receipts
        End Function

        ''' <summary>
        ''' Search delivery receipts with user-based filtering
        ''' </summary>
        Public Shared Function SearchDeliveryReceipts(searchTerm As String, limit As Integer, currentUserID As String, isAdmin As Boolean) As ObservableCollection(Of UniversalTransactionModel)
            Dim results As New ObservableCollection(Of UniversalTransactionModel)

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String
                    If isAdmin Then
                        ' Admin searches all records
                        query = "SELECT * FROM deliveryreceipts WHERE " &
                       "(DRNumber LIKE @SearchTerm OR ReferenceInvoice LIKE @SearchTerm OR ClientName LIKE @SearchTerm) " &
                       "ORDER BY dateAdded DESC LIMIT @Limit"
                    Else
                        ' Regular users search only their own records
                        query = "SELECT * FROM deliveryreceipts WHERE CreatedBy = @CreatedBy AND " &
                       "(DRNumber LIKE @SearchTerm OR ReferenceInvoice LIKE @SearchTerm OR ClientName LIKE @SearchTerm) " &
                       "ORDER BY dateAdded DESC LIMIT @Limit"
                    End If

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@SearchTerm", "%" & searchTerm & "%")
                        cmd.Parameters.AddWithValue("@Limit", limit)
                        If Not isAdmin Then
                            cmd.Parameters.AddWithValue("@CreatedBy", currentUserID)
                        End If

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                results.Add(MapReaderToUniversalModel(reader))
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show($"Error searching delivery receipts: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try

            Return results
        End Function

        Private Shared Function MapReaderToUniversalModel(reader As MySqlDataReader) As UniversalTransactionModel
            Return New UniversalTransactionModel With {
        .DocumentNumber = If(reader("DRNumber") IsNot DBNull.Value, reader("DRNumber").ToString(), ""),
        .DocumentReference = If(reader("ReferenceInvoice") IsNot DBNull.Value, reader("ReferenceInvoice").ToString(), ""),
        .DocumentDate = If(reader("DRDate") IsNot DBNull.Value, reader("DRDate").ToString(), "-"),
        .ClientName = If(reader("ClientName") IsNot DBNull.Value, reader("ClientName").ToString(), ""),
        .ClientDetails = If(reader("ClientDetails") IsNot DBNull.Value, reader("ClientDetails").ToString(), ""),
        .ShippingMethod = If(reader("ShippingMethod") IsNot DBNull.Value, reader("ShippingMethod").ToString(), ""),
        .PreparedBy = If(reader("Username") IsNot DBNull.Value, reader("Username").ToString(), ""),
        .ApprovedBy = If(reader("ApprovedBy") IsNot DBNull.Value, reader("ApprovedBy").ToString(), ""),
        .PaymentTerm = If(reader("PaymentTerm") IsNot DBNull.Value, reader("PaymentTerm").ToString(), ""),
        .Notes = If(reader("DeliveryNotes") IsNot DBNull.Value, reader("DeliveryNotes").ToString(), ""),
        .RawItemsJson = If(reader("OrderItems") IsNot DBNull.Value, reader("OrderItems").ToString(), "[]"),
        .DateAdded = If(reader("dateAdded") IsNot DBNull.Value, reader.GetDateTime("dateAdded").ToString("MMM d, yyyy"), ""),
        .CreatedBy = If(reader("CreatedBy") IsNot DBNull.Value, reader("CreatedBy").ToString(), "")
    }
        End Function

        ''' <summary>
        ''' Gets a single delivery receipt by DR Number for editing
        ''' </summary>
        Public Shared Function GetDeliveryReceiptByDRNumber(drNumber As String) As UniversalTransactionModel
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String = "SELECT * FROM deliveryreceipts WHERE DRNumber = @DRNumber LIMIT 1"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@DRNumber", drNumber)

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            If reader.Read() Then
                                Dim receipt = MapReaderToUniversalModel(reader)

                                ' Parse the OrderItems JSON into the ObservableCollection
                                If Not String.IsNullOrEmpty(receipt.RawItemsJson) Then
                                    Try
                                        Dim itemsList = JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(receipt.RawItemsJson)

                                        If itemsList IsNot Nothing Then
                                            receipt.OrderItems.Clear()

                                            For Each dict In itemsList
                                                Dim newItem As New OrderItems()

                                                newItem.ProductName = If(dict.ContainsKey("ProductName"), dict("ProductName"), "")
                                                newItem.Quantity = If(dict.ContainsKey("Quantity"), dict("Quantity"), "0")
                                                newItem.Description = If(dict.ContainsKey("Description"), dict("Description"), "")
                                                newItem.ProductDescription = If(dict.ContainsKey("ProductDescription"), dict("ProductDescription"), "")
                                                newItem.UnitPrice = If(dict.ContainsKey("UnitPrice"), dict("UnitPrice"),
                                                                    If(dict.ContainsKey("Rate"), dict("Rate"), "0.00"))
                                                newItem.LinePrice = If(dict.ContainsKey("LinePrice"), dict("LinePrice"),
                                                                    If(dict.ContainsKey("Amount"), dict("Amount"), "0.00"))

                                                Dim isHeaderVal As Boolean = False
                                                If dict.ContainsKey("IsHeaderRow") Then
                                                    Boolean.TryParse(dict("IsHeaderRow").ToString(), isHeaderVal)
                                                End If
                                                newItem.IsHeaderRow = isHeaderVal
                                                newItem.IsCategoryHeader = isHeaderVal

                                                Dim isSubtotalVal As Boolean = False
                                                If dict.ContainsKey("IsSubtotalRow") Then
                                                    Boolean.TryParse(dict("IsSubtotalRow").ToString(), isSubtotalVal)
                                                ElseIf dict.ContainsKey("IsSubotalRow") Then
                                                    Boolean.TryParse(dict("IsSubotalRow").ToString(), isSubtotalVal)
                                                End If
                                                newItem.IsSubtotalRow = isSubtotalVal

                                                newItem.ProductDescriptionVisibility = If(String.IsNullOrWhiteSpace(newItem.ProductDescription),
                                                             Visibility.Collapsed, Visibility.Visible)

                                                receipt.OrderItems.Add(newItem)
                                            Next
                                        End If
                                    Catch jsonEx As Exception
                                        Debug.WriteLine($"Error parsing delivery items JSON: {jsonEx.Message}")
                                    End Try
                                End If

                                Return receipt
                            End If
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                Debug.WriteLine($"Error in GetDeliveryReceiptByDRNumber: {ex.Message}")
                MessageBox.Show($"Error retrieving delivery receipt: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try

            Return Nothing
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
                            Dim items = Newtonsoft.Json.JsonConvert.DeserializeObject(Of List(Of Dictionary(Of String, String)))(reader("OrderItems").ToString())
                            For Each itm In items
                                Dim name = itm("ProductName")
                                Dim qty = 0
                                Integer.TryParse(itm("Quantity"), qty)

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

        Public Shared Function UpdateRelatedBilling(deliveryNo As String, invoiceNo As String) As Boolean
            Try
                Dim query As String = "UPDATE walkinbilling SET " &
                             "DRNo = IF(DRNo IS NULL OR DRNo = '', @dr, CONCAT(DRNo, ', ', @dr)) " &
                             "WHERE billingNumber = @inv"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@dr", deliveryNo)
                        cmd.Parameters.AddWithValue("@inv", invoiceNo)

                        Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                        Return rowsAffected > 0
                    End Using
                End Using
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Shared Function GetLatestDRFromDatabase(invoiceNumber As String) As String
            Dim latestDR As String = ""
            Dim query As String = "SELECT DRNumber FROM deliveryreceipts " &
                                 "WHERE ReferenceInvoice = @inv " &
                                 "ORDER BY DRNumber DESC LIMIT 1"

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@inv", invoiceNumber)
                        Dim result = cmd.ExecuteScalar()
                        If result IsNot Nothing Then
                            latestDR = result.ToString()
                        End If
                    End Using
                End Using
            Catch ex As Exception
                Debug.WriteLine("Error fetching latest DR: " & ex.Message)
            End Try

            Return latestDR
        End Function
    End Class
End Namespace