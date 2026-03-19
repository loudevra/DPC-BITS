Namespace DPC.Data.Models
    Public Class TransactionModel
        ' Primary Identifiers
        Public Property CostEstimateNumber As String
        Public Property BillingNumber
        Public Property DeliveryNumber As String

        ' Client & Representative Information
        Public Property ClientID As String
        Public Property ClientName As String
        Public Property CompanyRep As String
        Public Property SalesRep As String

        ' Approval & Responsibility
        Public Property PreparedBy As String
        Public Property ApprovedBy As String

        ' Date & Terms
        Public Property DocumentDate As String
        Public Property ValidityDate
        Public Property PaymentTerms As String

        ' Financial Details
        Public Property TaxProperty As String
        Public Property DiscountProperty As String
        Public Property FeeProperty As String
        Public Property InstallationProperty As String
        Public Property TotalFee As String
        Public Property TotalInstallation As String
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

        ' Notes & Audit Trail
        Public Property DocumentNote As String
        Public Property Remarks As String
        Public Property DateAdded As DateTime
    End Class
End Namespace