Imports DPC.DPC.Data

Public Module TransactionState
    Public Property ActiveRecord As New Models.TransactionModel()

    Public Sub ResetState()
        ActiveRecord = New Models.TransactionModel()
    End Sub
End Module