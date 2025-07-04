Namespace DPC.Data.Models
    Public Class UpdatedClient
        Public Property ClientID As Long
        Public Property ClientGroupID As Integer
        Public Property Name As String
        Public Property Company As String
        Public Property Phone As String
        Public Property Email As String
        Public Property BillingAddress As UpdatedClientBillingAddress
        Public Property ShippingAddress As UpdatedClientBillingAddress
        Public Property CustomerGroup As String
        Public Property Language As String
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime
    End Class

    Public Class UpdatedClientBillingAddress
        Public Property CompanyName As String
        Public Property Address As String
        Public Property City As String
        Public Property Region As String
        Public Property Country As String
        Public Property ZipCode As String
    End Class
End Namespace
