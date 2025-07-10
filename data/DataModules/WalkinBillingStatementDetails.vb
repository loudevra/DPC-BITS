Module WalkinBillingStatementDetails
    Public BLNumberCache As String
    Public BLClientIDCache As String
    Public BLClientDetailsCache As String ' Add to render when editing
    Public BLSubTotalCache As String ' Subtotal cache for the cost estimate
    Public BLTaxProperty As String
    Public BLDiscountProperty As String
    Public BLWarehouseIDCache As String
    Public BLWarehouseNameCache As String
    Public BLDateCache As String ' Render when editing
    Public BLReferenceNumber As String ' Reference number for the billing need to render when editing
    Public BLTotalTaxValueCache As String
    Public BLTotalDiscountValueCache As String
    Public BLTaxValueCache As String
    Public BLTotalAmountCache As String
    Public BLnoteTxt, BLremarksTxt, BLpaymentTerms As String ' Notes, remarks, and payment terms need to render when editing
    Public BLItemsCache As List(Of Dictionary(Of String, String)) ' Already Cached
    Public BLsignature, BLisCustomTerm As Boolean
    Public BLImageCache As String
    Public BLPathCache As String
    Public BLClientName, BLAddress, BLCity, BLRegion, BLCountry, BLPhone, BLEmail As String ' Client details need to render when editing
    Public BLTerm1, BLTerm2, BLTerm3, BLTerm4, BLTerm5, BLTerm6, BLTerm7, BLTerm8, BLTerm9, BLTerm10, BLTerm11, BLTerm12 As String
    Public BLTotalCostDelivery, BLDeliveryCost As Decimal
    Public BLApproved As String
    Public BLInstallation As String
    Public BLSalesRep As String
    Public BLCompleteAddress As String
    Public BLDRNo As String
    Public BLClientContact As String
    Public BLCompanyRep As String
    Public BLBankDetails As String
    Public BLAccountName As String
    Public BLAccountNumber As String


    Public Sub ClearAllBLCache()
        BLAccountNumber = ""
        BLAccountName = ""
        BLBankDetails = ""
        BLCompanyRep = ""
        BLClientContact = ""
        BLNumberCache = ""
        BLClientIDCache = ""
        BLClientDetailsCache = ""
        BLDateCache = ""
        BLReferenceNumber = ""
        BLTaxProperty = ""
        BLDiscountProperty = ""
        BLTaxValueCache = ""
        BLTotalTaxValueCache = ""
        BLTotalDiscountValueCache = ""
        BLTotalAmountCache = ""
        BLSalesRep = ""
        BLCompleteAddress = ""
        BLDRNo = ""


        BLnoteTxt = ""
        BLremarksTxt = ""
        BLpaymentTerms = ""

        If BLItemsCache IsNot Nothing Then
            BLItemsCache.Clear()
        Else
            BLItemsCache = New List(Of Dictionary(Of String, String))()
        End If

        BLsignature = False
        BLisCustomTerm = False

        BLImageCache = ""
        BLPathCache = ""

        BLClientName = ""
        BLAddress = ""
        BLCity = ""
        BLRegion = ""
        BLCountry = ""
        BLPhone = ""
        BLEmail = ""

        BLWarehouseIDCache = ""
        BLWarehouseNameCache = ""

        BLTerm1 = ""
        BLTerm2 = ""
        BLTerm3 = ""
        BLTerm4 = ""
        BLTerm5 = ""
        BLTerm6 = ""
        BLTerm7 = ""
        BLTerm8 = ""
        BLTerm9 = ""
        BLTerm10 = ""
        BLTerm11 = ""
        BLTerm12 = ""

        BLTotalCostDelivery = 0D
        BLDeliveryCost = 0D

        BLApproved = ""
        BLInstallation = ""
    End Sub
End Module
