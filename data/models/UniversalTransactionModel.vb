Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model

Namespace DPC.Data.Models
    Public Class UniversalTransactionModel
        ' --- 1. HEADER & IDENTIFIERS ---
        Public Property DocumentTitle As String = "DOCUMENT"
        Public Property DocumentNumber As String = ""
        Public Property DocumentNumberLabel As String = "No:"
        Public Property DocumentReference As String = ""

        ' --- 2. DATES & VALIDITY ---
        Public Property DateAdded As String = ""
        Public Property DateLabel As String = "Date:"
        Public Property DocumentDate As String = DateTime.Now.ToString("MMMM dd, yyyy")
        Public Property ValidityLabel As String = "Valid Until:"
        Public Property DocumentValidity As String = ""
        Public Property ShowValidity As Boolean = True

        ' --- 3. CLIENT & PERSONNEL ---
        Public Property ClientId As String = ""
        Public Property ClientName As String = ""
        Public Property ClientDetails As String = ""
        Public Property ClientAddress As String = ""
        Public Property ClientContact As String = ""
        Public Property ClientEmail As String = ""
        Public Property CompanyRep As String = ""
        Public Property SalesRep As String = ""
        Public Property PreparedBy As String = ""
        Public Property ApprovedBy As String = ""
        Public Property CreatedBy As String = ""

        ' --- 4. WAREHOUSE & LOGISTICS ---
        Public Property WarehouseName As String = ""
        Public Property WarehouseID As String = ""
        Public Property ShippingMethod As String = ""
        Public Property Status As String = ""

        ' --- 5. TABLES & ITEMS ---
        Public Property OrderItems As New ObservableCollection(Of OrderItems)
        Public Property RawItemsJson As String = ""

        ' --- 6. FINANCIALS (CURRENCY FORMATTED) ---
        Public Property PaymentTerm As String = ""
        Public Property SubtotalLabel As String = ""
        Public Property Subtotal As String = "₱ 0.00"

        ' TAX
        Public Property VatLabel As String = "VAT 12%"
        Public Property VatValue As String = "₱ 0.00"
        Public Property VatType As String = ""

        ' DISCOUNT
        Public Property DiscountSelection As String = ""
        Public Property DiscountValue As String = "₱ 0.00"
        Public Property DiscountPercent As String = "0%"

        ' FEES
        Public Property DeliveryMobilizationLabel As String = "Delivery Fee" ' 
        Public Property FeeValue As String = "₱ 0.00" ' 
        Public Property InstallationFee As String = "₱ 0.00"

        ' TOTAL
        Public Property TotalCost As String = "₱ 0.00"

        ' --- 7. FOOTER, BANK & TERMS ---
        Public Property PaymentTerms As String = ""
        Public Property BankDetails As String = ""
        Public Property AccName As String = ""
        Public Property AccNo As String = ""
        Public Property Notes As String = ""
        Public Property Remarks As String = ""
        Public Property WarrantyText As String = ""
        Public Property SignatureImageBase64 As String = ""
        Public Property HasSignature As Boolean = False

        ' --- 8. STATE & UI CONTROL ---
        Public Property IsFullyDelivered As Boolean = False
        Public Property IsEditMode As Boolean = False
        Public Property ShowImages As Boolean = True
        Public Property PrintPath As String = ""
        Public Property CreatePath As String = ""
        Public Property BackButtonLabel As String = "Back"
        Public Property EditLabel As String = ""
        Public Property EditButtonLabel As String = ""
    End Class
End Namespace