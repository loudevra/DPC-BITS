Namespace DPC.Data.Model
    Public Class SupplierDataModel
        Public Property SupplierID As String
        Public Property SupplierName As String
        Public Property SupplierCompany As String
        Public Property SupplierPhone As String
        Public Property SupplierEmail As String
        Public Property OfficeAddress As String
        Public Property City As String
        Public Property Region As String
        Public Property Country As String
        Public Property PostalCode As String
        Public Property TinID As String
        Public Property BrandIDs As List(Of Integer)
        Public Property BrandNames As String
    End Class

    Public Class Brand
        Public Property ID As String
        Public Property Name As String
    End Class
End Namespace
