Imports DPC.DPC.Data

Public Module TransactionState
    Public Property ActiveRecord As New Models.UniversalTransactionModel()

    Public Sub ResetRecord()
        ActiveRecord = New Models.UniversalTransactionModel()
    End Sub
End Module