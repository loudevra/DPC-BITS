Namespace DPC.Views.CRM
    Public Class CRMNewResidentialClient

        Public Sub New()
            InitializeComponent()
        End Sub


        Private Sub ResidentialTabs_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If Not IsLoaded Then Return

            ResidentialMainContent.Children.Clear()

            Dim selectedTab = CType(sender, TabControl).SelectedIndex

            Select Case selectedTab
                Case 0
                    ResidentialMainContent.Children.Add(New CRMResidentialClientPersonalInfo())
                Case 1
                    ResidentialMainContent.Children.Add(New CRMResidentialClientBillingAddress())
                Case 2
                    ResidentialMainContent.Children.Add(New CRMResidentialClientShippingAddress())
                Case 3
                    ResidentialMainContent.Children.Add(New CRMResidentialClientOtherSettings())
            End Select
        End Sub

        Private Sub CRMNewResidentialClient_Loaded(sender As Object, e As RoutedEventArgs)
            ResidentialMainContent.Children.Add(New CRMResidentialClientPersonalInfo())
        End Sub
    End Class

    Module ResidentialClientDetails
        Public ClientName As String
        Public Phone As String
        Public Email As String
        Public BillAddress As String
        Public BillCity As String
        Public BillRegion As String
        Public BillCountry As String
        Public BillZipCode As String
        Public ClientGroupID As Integer
        Public CustomerGroup As String
        Public CustomerLanguage As String
        Public Address As String
        Public City As String
        Public Region As String
        Public Country As String
        Public ZipCode As String
        Public SameAsBilling As Boolean = False
    End Module
End Namespace