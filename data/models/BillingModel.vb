Namespace DPC.Data.Models
    Public Class BillingModel
        ' Primary Identifiers
        Public Property BillingNumber As String
        Public Property DRNo As String ' Maps to your DRNo column

        ' Client & Representative Information
        Public Property ClientID As String
        Public Property CompanyRep As String ' Maps to companyRep
        Public Property SalesRep As String ' Maps to salesRep

        ' Approval & Responsibility
        Public Property PreparedBy As String
        Public Property ApprovedBy As String

        ' Date & Terms
        Public Property BillingDate As String
        Public Property PaymentTerms As String

        ' Financial Details
        Public Property TaxProperty As String
        Public Property DiscountProperty As String
        Public Property TotalTax As Decimal
        Public Property TotalDiscount As Decimal
        Public Property TotalAmount As Decimal

        ' Bank & Account Details
        Public Property BankDetails As String
        Public Property AccName As String
        Public Property AccNo As String

        ' Items, Logistics & Images
        Public Property OrderItems As String
        Public Property WarehouseID As Integer
        Public Property Base64img As String ' For the signature/stamp

        ' Notes & Audit Trail
        Public Property BillingNote As String
        Public Property Remarks As String
        Public Property DateAdded As DateTime
    End Class
End Namespace