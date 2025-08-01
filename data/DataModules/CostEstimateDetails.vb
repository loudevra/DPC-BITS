﻿Module CostEstimateDetails
    Public CEQuoteNumberCache As String
    Public CEClientIDCache As String
    Public CEClientDetailsCache As String ' Add to render when editing
    Public CESubTotalCache As String ' Subtotal cache for the cost estimate
    Public CETaxProperty As String
    Public CEDiscountProperty As String
    Public CEWarehouseIDCache As String
    Public CEWarehouseNameCache As String
    Public CEQuoteDateCache As String ' Render when editing
    Public CEReferenceNumber As String ' Reference number for the quote need to render when editing
    Public CEQuoteValidityDateCache As String ' Render when editing (Note : Will be rerplaced with newer variable)
    Public CETotalTaxValueCache As String
    Public CETotalDiscountValueCache As String
    Public CETaxValueCache As String
    Public CETotalAmountCache As String
    Public CEpaperNote As String = "Bank Details : " & vbCrLf &
                                    "Acc. Name: " & vbCrLf &
                                    "Acc. No:"
    Public CEnoteTxt, CEremarksTxt, CEpaymentTerms As String ' Notes, remarks, and payment terms need to render when editing
    Public CEQuoteItemsCache As List(Of Dictionary(Of String, String)) ' Already Cached
    Public CEsignature, CEisCustomTerm As Boolean
    Public CEImageCache As String
    Public CEPathCache As String
    Public CEClientName, CEAddress, CECity, CERegion, CECountry, CEPhone, CEEmail As String ' Client details need to render when editing
    ' Default Value for the terms
    Public CETerm1 As String = "50% upon ordering"
    Public CETerm2 As String = "30% upon delivery"
    Public CETerm3 As String = "20% upon completion"
    Public CETerm4 As String = "Progress billing remaining balance (for bulk orders)"
    Public CETerm5 As String = "Order will process upon clearance of cheque"
    Public CETerm6 As String = "Any additional orders will be charged seperately"
    Public CETerm7 As String = "No return after installation"
    Public CETerm8 As String = ""
    Public CETerm9 As String = "Additional delivery charges"
    Public CETerm10 As String = "Please indicate lead time for installation"
    Public CETerm11 As String = "We will have a site inscpetions prior to installation"
    Public CETerm12 As String = ""
    Public CETerm13 As String = "" ' Before Terms
    Public CETerm14 As String = "" ' During Terms
    Public CETerm15 As String = "" ' After Terms
    Public CETotalCostDelivery, CEDeliveryCost As Decimal
    Public CEApproved As String
    Public CEInstallation As String
    Public CEtaxSelection As Boolean
    Public CEType As Integer = 0 ' default value for extra safety
    Public CEValidUntilDate As String ' default value of extra safety
    Public CECompanyName As String = "" ' Company name for the cost estimate
    Public CESubtotalExInc As String = "Subtotal Vat Ex." ' Default Value if doesnt work
    Public CEWarranty As String = "Dream PC Build and IT Solutions Inc. offers 1 year warranty for this cost estimate.&#x0a;This warranty covers manufacturer defects and hardware malfunctions under normal usage. Terms and conditions apply."
    Public CEDeliveryMobilization = "Not Selected" ' Default Value if doesnt work
    Public CERepresentative As String = "" ' Representative for the cost estimate
    Public CECNIndetifier As String ' Client Identifier for the cost estimate

    Public Sub ClearAllCECache()
        CEQuoteNumberCache = ""
        CEClientIDCache = ""
        CEClientDetailsCache = ""
        CEQuoteDateCache = ""
        CEReferenceNumber = ""
        CEQuoteValidityDateCache = ""
        CETaxProperty = ""
        CEDiscountProperty = ""
        CETaxValueCache = ""
        CETotalTaxValueCache = ""
        CETotalDiscountValueCache = ""
        CETotalAmountCache = ""

        CEnoteTxt = ""
        CEremarksTxt = ""
        CEpaymentTerms = ""

        If CEQuoteItemsCache IsNot Nothing Then
            CEQuoteItemsCache.Clear()
        Else
            CEQuoteItemsCache = New List(Of Dictionary(Of String, String))()
        End If

        CEsignature = False
        CEisCustomTerm = False

        CEImageCache = ""
        CEPathCache = ""

        CEClientName = ""
        CEAddress = ""
        CECity = ""
        CERegion = ""
        CECountry = ""
        CEPhone = ""
        CEEmail = ""

        CEWarehouseIDCache = ""
        CEWarehouseNameCache = ""

        CETerm1 = ""
        CETerm2 = ""
        CETerm3 = ""
        CETerm4 = ""
        CETerm5 = ""
        CETerm6 = ""
        CETerm7 = ""
        CETerm8 = ""
        CETerm9 = ""
        CETerm10 = ""
        CETerm11 = ""
        CETerm12 = ""

        CETotalCostDelivery = 0D
        CEDeliveryCost = 0D

        CEApproved = ""
        CEInstallation = ""
        CEValidUntilDate = ""
    End Sub
End Module