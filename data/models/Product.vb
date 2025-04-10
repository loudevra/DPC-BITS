Imports System.Buffers.Text
Imports System.Security.Cryptography

Namespace DPC.Data.Model
    Public Class Product

        Public Property ProductImage As String
        Public Property ProductID As String
        Public Property ProductName As String
        Public Property CategoryID As String
        Public Property SubCategoryID As String
        Public Property WarehouseID As String
        Public Property RetailPrice As Decimal
        Public Property PurchaseOrder As Decimal
        Public Property DefaultTax As Decimal
        Public Property DiscountRate As Decimal
        Public Property StockUnits As Integer
        Public Property AlertQuantity As Integer
        Public Property ProductDescription As String
        Public Property MeasurementUnit As String
        Public Property CreatedAt As DateTime
        Public Property ModifiedAt As DateTime
    End Class

    Public Class ProductVariation

    End Class
End Namespace