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

    Public Class Transaction
        Public Property Code As String
        Public Property Contact As String
        Public Property AccountID As String
        Public Property Amount As Decimal
        Public Property TransactionDate As String
        Public Property Type As String
        Public Property Category As String
        Public Property Method As String
        Public Property Note As String
        Public Property TransactionTo As String
    End Class
End Namespace
