Namespace DPC.Data.Model
    Public Class QuotesModel
        Public Property QuoteNumber As String
        Public Property Reference As String
        Public Property QuoteDate As String
        Public Property Validity As String
        Public Property Tax As String
        Public Property Discount As String
        Public Property ClientID As String
        Public Property ClientName As String
        Public Property WarehouseID As String
        Public Property WarehouseName As String
        Public Property QuoteNote As String
        Public Property DeliveryFee As String
        Public Property InstallationFee As String
        Public Property TotalTax As String
        Public Property TotalDiscount As String
        Public Property TotalPrice As String
        Public Property ClientDetails As String = ""

        ' Government-Specific Fields (Safe to leave empty for Private)
        Public Property Subject As String = ""
        Public Property ProjectID As String = ""
        Public Property IsGovernmentQuote As Boolean = False

        ' This holds the JSON list (it can be categorized or flat, JSON doesn't care!)
        Public Property OrderItems As Object = Nothing


    End Class
End Namespace