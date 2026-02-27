Namespace DPC.Data.Model
    Public Class BillingModel
        Public Property BillingNumber As String
        Public Property Reference As String
        Public Property BillingDate As String
        Public Property Validity As String
        Public Property Tax As String
        Public Property Discount As String
        Public Property ClientID As String
        Public Property ClientName As String
        Public Property WarehouseID As String
        Public Property WarehouseName As String
        Public Property OrderItems As String
        Public Property BillingNote As String
        Public Property TotalTax As Decimal
        Public Property TotalDiscount As Decimal
        Public Property TotalAmount As Decimal
    End Class
End Namespace