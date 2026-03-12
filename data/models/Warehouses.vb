Namespace DPC.Data.Model
    Public Class Warehouses
        Public Property ID As Integer
        Public Property Name As String
        Public Property Description As String

        ' Use numeric types to match how the controller reads the columns
        Public Property TotalProducts As Integer
        Public Property StockQuantity As Integer

        ' Add Worth — nullable Decimal in case the value can be NULL or is not available
        Public Property Worth As Decimal?
    End Class
End Namespace
