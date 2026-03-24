Namespace DPC.Views.CRM
    Public Class CRMCorporationalTabStructured

        Public Sub New()
            InitializeComponent()
            ClearCache()
        End Sub

        Private Sub CorporationalTabs_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If Not IsLoaded Then Return

            CorporationalMainContent.Children.Clear()

            Dim selectedTab = CType(sender, TabControl).SelectedIndex

            Select Case selectedTab
                Case 0
                    CorporationalMainContent.Children.Add(New CRMCorporationalPersonalInfo())
                Case 1
                    CorporationalMainContent.Children.Add(New CRMCorporationalBillingAddress())
                Case 2
                    CorporationalMainContent.Children.Add(New CRMCorporationalShippingAddress())
                Case 3
                    CorporationalMainContent.Children.Add(New CRMCorporationalOtherSettings())
            End Select

        End Sub

        Private Sub CorporationalTabStructured_Loaded(sender As Object, e As RoutedEventArgs)
            CorporationalMainContent.Children.Add(New CRMCorporationalPersonalInfo())
        End Sub

        Private Sub ClearCache()
            CorporationalClientDetails.Representative = Nothing
            CorporationalClientDetails.TinID = Nothing
            CorporationalClientDetails.CompanyName = Nothing
            CorporationalClientDetails.Phone = Nothing
            CorporationalClientDetails.Landline = Nothing
            CorporationalClientDetails.Email = Nothing
            CorporationalClientDetails.BillAddress = Nothing
            CorporationalClientDetails.BillCity = Nothing
            CorporationalClientDetails.BillRegion = Nothing
            CorporationalClientDetails.BillCountry = Nothing
            CorporationalClientDetails.BillZipCode = Nothing
            CorporationalClientDetails.ClientGroupID = Nothing
            CorporationalClientDetails.CustomerGroup = Nothing
            CorporationalClientDetails.CustomerLanguage = Nothing
            CorporationalClientDetails.Address = Nothing
            CorporationalClientDetails.City = Nothing
            CorporationalClientDetails.Region = Nothing
            CorporationalClientDetails.Country = Nothing
            CorporationalClientDetails.ZipCode = Nothing
            CorporationalClientDetails.SameAsBilling = Nothing
        End Sub
    End Class

    Module CorporationalClientDetails
        Public ClientGroupID As Integer
        Public CompanyName As String
        Public Representative As String
        Public Phone As String
        Public Landline As String
        Public Email As String
        Public BillAddress As String
        Public BillCity As String
        Public BillRegion As String
        Public BillCountry As String
        Public BillZipCode As String
        Public Address As String
        Public City As String
        Public Region As String
        Public Country As String
        Public ZipCode As String
        Public TinID As String
        Public CustomerGroup As String
        Public CustomerLanguage As String
        Public CustomerType As String
        Public SameAsBilling As Boolean = False
    End Module
End Namespace

