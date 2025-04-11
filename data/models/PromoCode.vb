Namespace DPC.Data.Models
    Public Class PromoCode
        Public Property ID As Integer
        Public Property Code As String
        Public Property Amount As Decimal
        Public Property Quantity As Integer
        Public Property ValidUntil As Date
        Public Property IsLinked As Boolean
        Public Property Account As String
        Public Property Note As String
        Public Property CreatedAt As Date
    End Class
End Namespace
