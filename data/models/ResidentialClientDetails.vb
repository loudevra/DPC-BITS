Namespace DPC.Data.Models
    Public Class ResidentialClientDetails
        ' CRITICAL: "Shared" keeps the data alive even when you switch tabs.
        Public Shared Property ClientName As String
        Public Shared Property Phone As String
        Public Shared Property Email As String

        ' Address & Other Fields
        Public Shared Property Address As String
        Public Shared Property City As String
        Public Shared Property Region As String
        Public Shared Property Country As String
        Public Shared Property ZipCode As String
        Public Shared Property BillAddress As String
        Public Shared Property BillCity As String
        Public Shared Property BillRegion As String
        Public Shared Property BillCountry As String
        Public Shared Property BillZipCode As String
        Public Shared Property ClientGroupID As Integer?
        Public Shared Property CustomerGroup As String
        Public Shared Property CustomerLanguage As String
        Public Shared Property SameAsBilling As Boolean
    End Class
End Namespace