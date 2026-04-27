Public Class StatementModel
    ' ── DB keys ──────────────────────────────────────────────────────────────
    Public Property SoaId As Integer
    Public Property ClientId As String

    ' ── Header fields ────────────────────────────────────────────────────────
    Public Property SOANo As String
    Public Property ClientName As String
    Public Property ClientDetails As String
    Public Property ProjectTitle As String
    Public Property StatementDate As String
    Public Property PONo As String
    Public Property SINo As String
    Public Property DRNo As String
    Public Property BSNo As String
    Public Property PODate As String
    Public Property DeliveryPeriod As String
    Public Property RequiredDate As String
    Public Property CompletionDate As String

    ' ── Amounts ──────────────────────────────────────────────────────────────
    Public Property ContractAmount As String
    Public Property Subtotal As String
    Public Property TotalPayment As String
    Public Property OutstandingBalance As String
    Public Property LiquidatedDamages As String
    Public Property NetAmountDue As String

    ' ── LD fields ────────────────────────────────────────────────────────────
    Public Property LDRate As String
    Public Property LDDaysDelayed As String
    Public Property LDPerDay As String

    ' ── Child rows ───────────────────────────────────────────────────────────
    Public Property LineItems As New List(Of LineItemModel)
    Public Property PaymentItems As New List(Of PaymentItemModel)
End Class

Public Class LineItemModel
    Public Property DateStr As String
    Public Property Description As String
    Public Property Qty As String
    Public Property Amount As String
    Public Property Payment As String
    Public Property Balance As String
End Class

Public Class PaymentItemModel
    Public Property DateStr As String
    Public Property Reference As String
    Public Property AmountPaid As String
End Class