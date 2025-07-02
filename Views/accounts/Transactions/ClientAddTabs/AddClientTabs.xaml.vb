Namespace DPC.Views.Accounts.Transactions.ClientAddTabs
    Public Class AddClientTabs

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub AddClientTabs_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If Not IsLoaded Then Return

            AddClientMainContent.Children.Clear()

            Dim selectedTab = CType(sender, TabControl).SelectedIndex

            Select Case selectedTab
                Case 0
                    AddClientMainContent.Children.Add(New AddClientPersonalInfo())
                Case 1
                    AddClientMainContent.Children.Add(New AddClientBillingAddress())
                Case 2
                    AddClientMainContent.Children.Add(New AddClientShippingAddress())
                Case 3
                    AddClientMainContent.Children.Add(New AddClientOtherSettings())
            End Select
        End Sub

        Private Sub AddClient_Loaded(sender As Object, e As RoutedEventArgs)
            AddClientMainContent.Children.Add(New AddClientPersonalInfo())
        End Sub
    End Class
End Namespace