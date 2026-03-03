Module DeliveryDetails
    Public DRNumber As String = ""
    Public DRReferenceInvoice As String = ""
    Public DRDate As String = ""

    ' Client Information
    Public DRClientDetails As String = ""
    Public DRClientName As String = ""

    ' Logistic Details
    Public DRDeliveryNotes As String = ""
    Public DRShippingMethod

    Public DRApprovedBy As String = ""
    Public DRPaymentTerm As String = ""

    Public DRDeliveryStatus As String = ""

    ' Product Data (Cached from the Billing Table)
    Public Property DRDeliveryItems As List(Of Dictionary(Of String, String))



    Public Sub ClearDeliveryDetails()
        ' Reset Strings to Empty
        DRNumber = ""
        DRReferenceInvoice = ""
        DRDate = ""
        DRClientDetails = ""
        DRClientName = ""
        DRDeliveryNotes = ""
        DRApprovedBy = ""
        DRPaymentTerm = ""

        ' Reset Variants/Objects
        DRShippingMethod = Nothing

        ' Re-initialize the List to wipe any cached product data
        If DRDeliveryItems IsNot Nothing Then
            DRDeliveryItems.Clear()
        Else
            DRDeliveryItems = New List(Of Dictionary(Of String, String))()
        End If
    End Sub

End Module
