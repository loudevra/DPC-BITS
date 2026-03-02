Namespace DPC.Data.Models
    Public Class DeliveryReceiptModel
        ' Primary Identifiers
        Public Property DRNumber As String ' Maps to DRNumber column
        Public Property ReferenceInvoice As String ' Maps to ReferenceInvoice

        ' Client Information
        Public Property ClientName As String
        Public Property ClientDetails As String

        ' Logistics & Status
        Public Property DRDate As String ' Stored as String for display formatting
        Public Property ShippingMethod As String
        Public Property DeliveryStatus As String ' ENUM: 'FULL DELIVERY' or 'PARTIAL DELIVERY'

        ' Approval & Responsibility
        Public Property ApprovedBy As String
        Public Property PaymentTerm As String
        Public Property Username As String ' Tracking who created the record

        ' Data & Items
        Public Property OrderItems As String ' JSON string of products and serials
        Public Property DeliveryNotes As String

        ' Audit Trail & UI Helpers
        Public Property DateAdded As DateTime
        Public Property DateAddedDisplay As String ' For formatted DataGrid display (MMM d, yyyy)

        ' Optional: Financial Carry-over (If your DR shows totals)
        Public Property TotalAmount As Decimal
    End Class
End Namespace