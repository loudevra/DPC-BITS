Namespace DPC.Data.Model
    Public Class PurchaseOrderModel
        Public Property InvoiceNumber As String
        Public Property OrderDate As String
        Public Property DueDate As String
        Public Property Tax As String
        Public Property Discount As String
        Public Property SupplierID As String
        Public Property SupplierName As String
        Public Property WarehouseID As String
        Public Property WarehouseName As String
        Public Property OrderItems As String
        Public Property OrderNote As String

        Public Property TotalTax As String
        Public Property TotalDiscount As String
        Public Property TotalPrice As String

    End Class
End Namespace
