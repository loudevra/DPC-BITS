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
End Namespace