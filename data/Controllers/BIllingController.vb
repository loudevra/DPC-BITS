Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Namespace DPC.Data.Controllers
    Public Class BillingController

        ''' Gets billing statements from the database with a limit and filter
        Public Shared Function GetBillingStatements(limit As Integer, billingType As String) As ObservableCollection(Of BillingModel)
            Dim statements As New ObservableCollection(Of BillingModel)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Debug Print 1: Check Connection
                    Debug.WriteLine("MySQL: Connection opened successfully.")

                    Dim query As String = "SELECT * FROM walkinbilling ORDER BY dateAdded DESC LIMIT @limit"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@limit", limit)

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            Dim rowCount As Integer = 0

                            While reader.Read()
                                rowCount += 1
                                ' Debug Print 2: Print the Billing Number of each row found
                                Debug.WriteLine($"MySQL Row {rowCount}: Found Billing # {reader("billingNumber")}")

                                statements.Add(MapReaderToModel(reader))
                            End While

                            ' Debug Print 3: Final Count
                            Debug.WriteLine($"MySQL Total: {statements.Count} records sent to DataGrid.")
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                ' Debug Print 4: Catch Errors (Missing columns, etc.)
                Debug.WriteLine("MySQL ERROR: " & ex.Message)
                MessageBox.Show("Database Error: " & ex.Message)
            End Try
            Return statements
        End Function

        ''' Searches billing statements based on text criteria
        Public Shared Function SearchBillingStatements(searchText As String, limit As Integer, billingType As String) As ObservableCollection(Of BillingModel)
            Dim statements As New ObservableCollection(Of BillingModel)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String = $"
                        SELECT * FROM walkinbilling 
                        WHERE (billingNumber LIKE @search OR DRNo LIKE @search OR clientID LIKE @search OR companyRep LIKE @search)
                        ORDER BY dateAdded DESC LIMIT @limit"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@search", "%" & searchText & "%")
                        cmd.Parameters.AddWithValue("@limit", limit)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                statements.Add(MapReaderToModel(reader))
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                Debug.WriteLine("Error in SearchBillingStatements: " & ex.Message)
            End Try
            Return statements
        End Function

        ''' Inserts a new Billing Statement into the walkinbilling table
        Public Shared Function InsertBillingStatement(bm As BillingModel) As Boolean
            Try
                Dim query As String = "INSERT INTO walkinbilling (billingNumber, billingDate, DRNo, clientID, companyRep, salesRep, preparedBy, approvedBy, paymentTerms, orderItems, warehouseID, base64img, taxProperty, discountProperty, totalTax, totalDiscount, totalAmount, billingNote, bankDetails, accName, accNo, remarks, dateAdded) " &
                                     "VALUES (@billingNumber, @billingDate, @DRNo, @clientID, @companyRep, @salesRep, @preparedBy, @approvedBy, @paymentTerms, @orderItems, @warehouseID, @base64img, @taxProperty, @discountProperty, @totalTax, @totalDiscount, @totalAmount, @billingNote, @bankDetails, @accName, @accNo, @remarks, NOW())"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@billingNumber", bm.BillingNumber)
                        cmd.Parameters.AddWithValue("@billingDate", bm.BillingDate)
                        cmd.Parameters.AddWithValue("@DRNo", bm.DRNo)
                        cmd.Parameters.AddWithValue("@clientID", bm.ClientID)
                        cmd.Parameters.AddWithValue("@companyRep", bm.CompanyRep)
                        cmd.Parameters.AddWithValue("@salesRep", bm.SalesRep)
                        cmd.Parameters.AddWithValue("@preparedBy", bm.PreparedBy)
                        cmd.Parameters.AddWithValue("@approvedBy", bm.ApprovedBy)
                        cmd.Parameters.AddWithValue("@paymentTerms", bm.PaymentTerms)
                        cmd.Parameters.AddWithValue("@orderItems", bm.OrderItems)
                        cmd.Parameters.AddWithValue("@warehouseID", bm.WarehouseID)
                        cmd.Parameters.AddWithValue("@base64img", bm.Base64img)
                        cmd.Parameters.AddWithValue("@taxProperty", bm.TaxProperty)
                        cmd.Parameters.AddWithValue("@discountProperty", bm.DiscountProperty)
                        cmd.Parameters.AddWithValue("@totalTax", bm.TotalTax)
                        cmd.Parameters.AddWithValue("@totalDiscount", bm.TotalDiscount)
                        cmd.Parameters.AddWithValue("@totalAmount", bm.TotalAmount)
                        cmd.Parameters.AddWithValue("@billingNote", bm.BillingNote)
                        cmd.Parameters.AddWithValue("@bankDetails", bm.BankDetails)
                        cmd.Parameters.AddWithValue("@accName", bm.AccName)
                        cmd.Parameters.AddWithValue("@accNo", bm.AccNo)
                        cmd.Parameters.AddWithValue("@remarks", bm.Remarks)

                        Return cmd.ExecuteNonQuery() > 0
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Failed to save billing statement: " & ex.Message)
                Return False
            End Try
        End Function

        ''' Generates a unique Billing ID
        Public Shared Function GenerateBillingID(isGov As Boolean) As String
            Dim prefix As String = If(isGov, "GB-", "B-")
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy")

            Dim nextID As Integer = 1
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT MAX(CAST(RIGHT(billingNumber, 4) AS UNSIGNED)) FROM walkinbilling WHERE billingNumber LIKE @prefix"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@prefix", prefix & datePart & "-%")
                        Dim result = cmd.ExecuteScalar()
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            nextID = Convert.ToInt32(result) + 1
                        End If
                    End Using
                End Using
            Catch ex As Exception
                Debug.WriteLine("Error generating Billing ID: " & ex.Message)
            End Try

            Return $"{prefix}{datePart}-{nextID:D4}"
        End Function

        ''' Helper to map SQL reader to BillingModel
        Private Shared Function MapReaderToModel(reader As MySqlDataReader) As BillingModel
            Return New BillingModel() With {
                .BillingNumber = reader("billingNumber").ToString(),
                .BillingDate = If(reader("billingDate") Is DBNull.Value, "-", Convert.ToDateTime(reader("billingDate")).ToString("MMM d, yyyy")),
                .DRNo = reader("DRNo").ToString(),
                .ClientID = reader("clientID").ToString(),
                .CompanyRep = reader("companyRep").ToString(),
                .SalesRep = reader("salesRep").ToString(),
                .TotalAmount = ParseCurrencyToDecimal(reader("totalAmount").ToString()),
                .OrderItems = reader("orderItems").ToString(),
                .DateAdded = If(reader("dateAdded") Is DBNull.Value, DateTime.MinValue, reader.GetDateTime("dateAdded"))
            }
        End Function

        Private Shared Function ParseCurrencyToDecimal(value As String) As Decimal
            If String.IsNullOrWhiteSpace(value) OrElse value = "-" Then Return 0D

            ' Remove the currency symbol and any commas
            Dim cleanValue As String = value.Replace("₱", "").Replace(",", "").Trim()

            Dim result As Decimal
            If Decimal.TryParse(cleanValue, result) Then
                Return result
            Else
                Return 0D
            End If
        End Function
    End Class
End Namespace