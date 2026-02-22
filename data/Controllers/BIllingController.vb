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
    Public Class BIllingController
        Public Shared Function GetBilling(limit As Integer, billingType As String) As ObservableCollection(Of BillingModel)
            Dim billings As New ObservableCollection(Of BillingModel)
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Logic to filter based on the billing number prefix
                    Dim filterClause As String
                    If billingType.Equals("Government", StringComparison.OrdinalIgnoreCase) Then
                        filterClause = "WHERE billingNumber LIKE 'GPCE-%'"
                    Else
                        filterClause = "WHERE billingNumber NOT LIKE 'GPCE-%'"
                    End If

                    ' Querying the walkinbilling table specifically
                    Dim query As String = $"
                SELECT 
                    `billingNumber`, `billingDate`, `DRNo`, `clientID`, `companyRep`, 
                    `salesRep`, `preparedBy`, `approvedBy`, `paymentTerms`, `orderItems`, 
                    `warehouseID`, `base64img`, `taxProperty`, `discountProperty`, 
                    `totalTax`, `totalDiscount`, `totalAmount`, `billingNote`, 
                    `bankDetails`, `accName`, `accNo`, `remarks`, `dateAdded`
                FROM `walkinbilling` 
                {filterClause}
                ORDER BY dateAdded DESC 
                LIMIT @limit"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@limit", limit)

                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                billings.Add(New BillingModel() With {
                                    .BillingNumber = reader("billingNumber").ToString(),
                                    .BillingDate = If(reader("billingDate") Is DBNull.Value, "-", reader.GetDateTime("billingDate").ToString("MMM d, yyyy")),
                                    .DRNo = If(reader("DRNo") Is DBNull.Value, "-", reader("DRNo").ToString()),
                                    .ClientID = reader("clientID").ToString(),
                                    .CompanyRep = reader("companyRep").ToString(),
                                    .SalesRep = reader("salesRep").ToString(),
                                    .PreparedBy = reader("preparedBy").ToString(),
                                    .ApprovedBy = reader("approvedBy").ToString(),
                                    .PaymentTerms = reader("paymentTerms").ToString(),
                                    .OrderItems = reader("orderItems").ToString(),
                                    .WarehouseID = reader("warehouseID").ToString(),
                                    .Base64Img = reader("base64img").ToString(),
                                    .TaxProperty = reader("taxProperty").ToString(),
                                    .DiscountProperty = reader("discountProperty").ToString(),
                                    .TotalTax = If(reader("totalTax") Is DBNull.Value, 0, Convert.ToDecimal(reader("totalTax"))),
                                    .TotalDiscount = If(reader("totalDiscount") Is DBNull.Value, 0, Convert.ToDecimal(reader("totalDiscount"))),
                                    .TotalAmount = If(reader("totalAmount") Is DBNull.Value, 0, Convert.ToDecimal(reader("totalAmount"))),
                                    .BillingNote = reader("billingNote").ToString(),
                                    .Remarks = reader("remarks").ToString()
                                })
                            End While
                        End Using
                    End Using
                End Using

            Catch ex As Exception
                Debug.WriteLine("Error in GetWalkInBilling: " & ex.Message)
            End Try

            Return billings
        End Function
    End Class
End Namespace