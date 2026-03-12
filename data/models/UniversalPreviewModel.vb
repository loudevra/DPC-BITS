Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model

Namespace DPC.Data.Models
    Public Class UniversalPreviewModel
        ' Header Information
        Public Property DocumentTitle As String = "DOCUMENT"
        Public Property DocumentNumber As String = ""
        Public Property DateLabel As String = "Date:"
        Public Property DocumentDate As String = ""
        Public Property ValidityLabel As String = "Valid Until:"
        Public Property DocumentValidity As String = ""
        Public Property BackButtonLabel As String = "Back"

        ' Client Information
        Public Property ClientName As String = ""
        Public Property ClientAddress As String = ""
        Public Property ClientContact As String = ""
        Public Property ClientEmail As String = ""

        ' Tables & Items
        Public Property Items As New ObservableCollection(Of OrderItems)

        ' Financials
        Public Property SubtotalLabel As String = ""
        Public Property Subtotal As String = "₱ 0.00"
        Public Property VatLabel As String = "VAT 12%"
        Public Property VatValue As String = "₱ 0.00"
        Public Property DeliveryMobilizationLabel As String = ""
        Public Property InstallationFee As String = "₱ 0.00"
        Public Property DeliveryFee As String = "₱ 0.00"
        Public Property TotalCost As String = "₱ 0.00"

        ' Footer/Terms
        Public Property WarrantyText As String = ""
        Public Property Notes As String = ""
        Public Property Remarks As String = ""
        Public Property PreparedBy As String = ""
        Public Property ApprovedBy As String = ""
        Public Property PaymentTerms As String = ""
        Public Property SignatureImageBase64 As String = ""
        Public Property HasSignature As Boolean = False
        Public Property IsCustomTerm As Boolean = False

        ' Custom Toggle (Internal Logic)
        Public Property ShowImages As Boolean = True

        ' State Information
        Public Property IsEditMode As Boolean = False
        Public Property PrintPath As String = ""
        Public Property CreatePath As String = ""
    End Class
End Namespace