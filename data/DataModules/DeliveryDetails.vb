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

    ' Product Data (Cached from the Billing Table)
    Public Property DRDeliveryItems As List(Of Dictionary(Of String, String))
End Module
