Namespace DPC.Data.Models
    Public Class Client
        Public Property ClientID As Integer
        Public Property ClientGroupID As Integer
        Public Property Name As String
        Public Property Company As String
        Public Property Phone As String
        Public Property Email As String
        Public Property BillingAddress As String()
        Public Property ShippingAddress As String()
        Public Property CustomerGroup As String
        Public Property Language As String
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime
    End Class
End Namespace
