Namespace DPC.Views.Stocks.ProductsLabel.StandardLabel
    ''' <summary>
    ''' Interaction logic for CustomLabel.xaml
    ''' </summary>

    Public Class StandardLabel
        Inherits Window

        Public Sub New()
            InitializeComponent()

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNav
        End Sub
    End Class
End Namespace