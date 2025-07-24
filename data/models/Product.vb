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

        Public Property ProductCode As String
        Public Property ProductDescription As String
        Public Property MeasurementUnit As String
        Public Property CreatedAt As DateTime
        Public Property ModifiedAt As DateTime
    End Class

    ' Helper class to store image data
    Public Class ImageData
        Public Property Base64String As String
        Public Property FileExtension As String
        Public Property FileName As String
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

    Public Class ProductVariationData
        ' Identifier for this variation combination
        Public Property CombinationID As String

        ' Variation details (e.g. "Red, Large" or just "Red" for single variation)
        Public Property CombinationName As String

        ' Array of individual variation options that make up this combination
        Public Property VariationOptions As String()


        Public Property WarehouseId As Integer
        Public Property SelectedWarehouseIndex As Integer

        ' Properties that can be different per variation
        Public Property RetailPrice As Decimal
        Public Property PurchaseOrder As Decimal
        Public Property DefaultTax As Decimal
        Public Property DiscountRate As Decimal
        Public Property StockUnits As Integer
        Public Property AlertQuantity As Integer

        ' Serial numbers specific to this variation
        Public Property SerialNumbers As List(Of String)

        ' Whether this variation uses serial numbers
        Public Property IncludeSerialNumbers As Boolean

        Public Property MarkupValue As Decimal
        Public Property IsPercentageMarkup As Boolean

        ' Common constructor
        Public Sub New()
            SerialNumbers = New List(Of String)()
            IncludeSerialNumbers = True
        End Sub

        ' Constructor with combination name
        Public Sub New(combinationName As String)
            Me.New()
            Me.CombinationName = combinationName
            Me.VariationOptions = combinationName.Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)

            ' Trim whitespace from options
            For i As Integer = 0 To VariationOptions.Length - 1
                VariationOptions(i) = VariationOptions(i).Trim()
            Next

            ' Generate a unique ID for this combination
            Me.CombinationID = GenerateCombinationID(combinationName)
        End Sub

        ' Generate a unique ID based on the combination name
        Private Function GenerateCombinationID(combinationName As String) As String
            ' Simple hash of the combination name
            Dim sanitizedName As String = combinationName.Replace(" ", "").Replace(",", "_").ToLower()
            Return "var_" & sanitizedName
        End Function



        ' Constructor
    End Class

    'for products in the purchase order and stock return
    Public Class ProductDataModel
        Public Property ProductID As String
        Public Property ProductName As String
        Public Property BuyingPrice As Decimal
        Public Property SellingPrice As Decimal
        Public Property DefaultTax As Decimal
        Public Property StockUnits As Integer
        Public Property MeasurementUnit As String
        Public Property SupplierID As String
    End Class
End Namespace