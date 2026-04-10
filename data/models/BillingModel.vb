Namespace DPC.Data.Models
    Public Class BillingModel
        ' Primary Identifiers
        Public Property BillingNumber As String
        Public Property DRNo As String ' Maps to your DRNo column

        ' Client & Representative Information
        Public Property ClientID As String
        Public Property ClientName As String
        Public Property CompanyRep As String ' Maps to companyRep
        Public Property SalesRep As String ' Maps to salesRep

        ' Approval & Responsibility
        Public Property PreparedBy As String
        Public Property ApprovedBy As String

        ' Date & Terms
        Public Property BillingDate As String
        Public Property PaymentTerms As String

        ' Financial Details
        Public Property DeliveryFee As String
        Public Property InstallationFee As String
        Public Property TaxProperty As String
        Public Property DiscountProperty As String
        Public Property TotalTax As String
        Public Property TotalDiscount As String
        Public Property TotalAmount As String

        ' Bank & Account Details
        Public Property BankDetails As String
        Public Property AccName As String
        Public Property AccNo As String

        ' Items, Logistics & Images
        Public Property OrderItems As String
        Public Property WarehouseName As String
        Public Property WarehouseID As Integer
        Public Property Base64img As String ' For the signature/stamp

        ' Notes & Audit Trail
        Public Property BillingNote As String
        Public Property Remarks As String
        Public Property DateAdded As DateTime

        ' Created By Information
        Public Property CreatedBy As String
    End Class
End Namespace
''''''''''