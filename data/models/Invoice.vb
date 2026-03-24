Namespace DPC.Data.Model
    Public Class Checker
        Public Property SalesRep As String
        Public Property CheckedBy As String
        Public Property ApprovedBy As String
    End Class

    Public Class OrderItems
        Public Property Quantity As String        ' ✅ Changed from Integer to String
        Public Property Description As String
        Public Property UnitPrice As String
        Public Property LinePrice As String
        Public Property ProductImage As BitmapImage
        Public Property ProductDescription As String
        Public Property ProductDescriptionVisibility As Visibility
        Public Property IsCategoryHeader As Boolean
        Public Property IsSubtotalRow As Boolean
    End Class
End Namespace