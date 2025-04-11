Namespace DPC.Data.Models
    Public Enum AccountType
        Basic
        Assets
        Expenses
        Income
        Liabilities
        Equity
    End Enum

    Public Class Account
        Public Property AccountID As String
        Public Property AccountName As String
        Public Property AccountNo As String
        Public Property Name As String
        Public Property InitialBalance As Decimal
        Public Property Note As String
        Public Property BusinessLocation As Integer
        Public Property AccountType As AccountType
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime
    End Class
End Namespace
