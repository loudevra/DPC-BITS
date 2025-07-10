Namespace DPC.Data.Models
    Public Class Client
        Public Property ClientID As Long
        Public Property ClientGroupID As Integer
        Public Property Name As String
        Public Property Company As String
        Public Property Phone As String
        Public Property Email As String
        Public Property BillingAddress As String
        Public Property ShippingAddress As String
        Public Property CustomerGroup As String
        Public Property ClientLanguage As String
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime
        Public Property ClientType As String
    End Class

    Public Class ClientCorporational
        Public Property ClientID As Long
        Public Property ClientGroupID As Integer
        Public Property Company As String
        Public Property Representative As String
        Public Property Phone As String
        Public Property Landline As String
        Public Property Email As String
        Public Property BillingAddress As String
        Public Property ShippingAddress As String
        Public Property CustomerGroup As String
        Public Property ClientLanguage As String
        Public Property TinID As String
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime
        Public Property ClientType As String
    End Class
End Namespace
