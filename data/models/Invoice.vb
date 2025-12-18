Namespace DPC.Data.Model
    Public Class Checker
        Public Property SalesRep As String
        Public Property CheckedBy As String
        Public Property ApprovedBy As String
    End Class

    Public Class OrderItems
        Public Property Quantity As Integer
        Public Property Description As String
        Public Property UnitPrice As String
        Public Property LinePrice As String
        Public Property ProductImage As BitmapImage
        Public Property ProductDescription As String  ' Add this
        Public Property ProductDescriptionVisibility As Visibility  ' Add this
    End Class

End Namespace
