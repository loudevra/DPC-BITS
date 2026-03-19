Imports DPC.DPC.Data.Models

Public Module DeliveryState
    Public Property CurrentReceipt As DeliveryReceiptModel
    Public Property IsEditMode As Boolean = False

    Public Sub ClearDeliveryState()
        CurrentReceipt = Nothing
        IsEditMode = False
    End Sub
End Module