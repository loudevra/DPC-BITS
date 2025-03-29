Imports System.Windows

Namespace DPC.Views.Stocks.ProductCategories
    ''' <summary>
    ''' Interaction logic for ProductCategories.xaml
    ''' </summary>
    Public Class ProductCategories
        Inherits Window

        Public Sub New()
            InitializeComponent()

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Child = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Child = topNav

        End Sub
    End Class
End Namespace
