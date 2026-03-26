Namespace DPC.Data.Models
    Public Class DeliveryReceiptModel
        ' Primary Identifiers
        Public Property DRNumber As String
        Public Property ReferenceInvoice As String
        Public Property DocumentReference As String

        ' Client Information
        Public Property ClientName As String
        Public Property ClientDetails As String

        ' Logistics & Status
        Public Property DRDate As String
        Public Property ShippingMethod As String
        Public Property DeliveryStatus As String

        ' Approval & Responsibility
        Public Property ApprovedBy As String
        Public Property PaymentTerm As String
        Public Property Username As String

        ' Data & Items
        Public Property OrderItems As String
        Public Property DeliveryNotes As String

        ' Audit Trail & UI Helpers
        Public Property DateAdded As DateTime
        Public Property DateAddedDisplay As String
        Public Property IsFullyDelivered As Boolean = False
    End Class
End Namespace