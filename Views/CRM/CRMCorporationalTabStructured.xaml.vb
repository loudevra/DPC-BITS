Namespace DPC.Views.CRM
    Public Class CRMCorporationalTabStructured

        Public Sub New()
            InitializeComponent()
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
    End Class
End Namespace

