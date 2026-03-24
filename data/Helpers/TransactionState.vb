Imports DPC.DPC.Data

Public Module TransactionState
    Public Property ActiveRecord As New Models.UniversalTransactionModel()
    Public Property IsEditMode As Boolean = False

    Public Sub ResetRecord()
        ActiveRecord = New Models.UniversalTransactionModel()
        IsEditMode = False
    End Sub
End Module