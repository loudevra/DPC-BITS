Module CostEstimateDetails
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
    Public CEQuoteValidityDateCache As String ' Render when editing
    Public CETotalTaxValueCache As String
    Public CETotalDiscountValueCache As String
    Public CETaxValueCache As String
    Public CETotalAmountCache As String
    Public CEnoteTxt, CEremarksTxt, CEpaymentTerms As String ' Notes, remarks, and payment terms need to render when editing
    Public CEQuoteItemsCache As List(Of Dictionary(Of String, String)) ' Already Cached
    Public CEsignature, CEisCustomTerm As Boolean
    Public CEImageCache As String
    Public CEPathCache As String
    Public CEClientName, CEAddress, CECity, CERegion, CECountry, CEPhone, CEEmail As String ' Client details need to render when editing
    Public CETerm1, CETerm2, CETerm3, CETerm4, CETerm5, CETerm6, CETerm7, CETerm8, CETerm9, CETerm10, CETerm11, CETerm12 As String
    Public CETotalCostDelivery, CEDeliveryCost As Decimal
    Public CEApproved As String
    Public CEInstallation As String

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
    End Sub
End Module
