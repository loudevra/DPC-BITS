Namespace DPC.DPC.Data.Model
    Public Class ProductCategory
        Public Property categoryID As Integer
        Public Property categoryName As String
        Public Property categoryDescription As String
        Public Property subcategories As List(Of Subcategory) ' Property to hold subcategories
    End Class

    Public Class Subcategory
        Public Property subcategoryID As Integer
        Public Property categoryID As Integer
        Public Property subcategoryName As String
    End Class
End Namespace
