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

    ' Represents a single variation (e.g., "Color", "Size")
    Public Class ProductVariation
        Public Property VariationName As String              ' Name of the variation (e.g., "Color")
        Public Property EnableImage As Boolean               ' Whether image is used for this variation
        Public Property Options As List(Of VariationOption)  ' List of options (e.g., "Red", "Blue")
    End Class

    ' Represents a single option within a variation (e.g., "Red" for "Color")
    Public Class VariationOption
        Public Property OptionName As String
        Public Property ImageBase64 As String      ' Base64 encoded image data
        Public Property ImageFileName As String    ' Original filename
        Public Property ImageFileExtension As String ' File extension (jpg, png, etc.)
    End Class
End Namespace