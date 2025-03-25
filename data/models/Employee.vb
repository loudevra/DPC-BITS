Namespace DPC.Data.Model
    Public Class Employee
        Public Property EmployeeID As String ' Changed from Integer to String
        Public Property Username As String
        Public Property Email As String
        Public Property Password As String
        Public Property UserRoleID As Integer
        Public Property BusinessLocationID As Integer
        Public Property Name As String
        Public Property StreetAddress As String
        Public Property City As String
        Public Property Region As String
        Public Property Country As String
        Public Property PostalCode As String
        Public Property Phone As String
        Public Property Salary As Decimal
        Public Property SalesCommission As Decimal
        Public Property Department As String
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime
        ' 🔹 Add RoleName and LocationName
        Public Property RoleName As String
        Public Property LocationName As String
    End Class
End Namespace
