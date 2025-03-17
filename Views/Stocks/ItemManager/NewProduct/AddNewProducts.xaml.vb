Imports DPC.DPC.Components.Navigation

Namespace DPC.Views.Stocks.ItemManager.NewProduct
    Public Class AddNewProducts
        Inherits Window

        Public Sub New()
            InitializeComponent()

            InitializeComponent()

            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar

            ' Add TopNavBar to TopNavBarContainer
            Dim topNavBar As New TopNavBar()
            TopNavBarContainer.Child = topNavBar

        End Sub

        Private Sub btnAddRow_Click(sender As Object, e As RoutedEventArgs)
            ' Open the second form
            Dim secondForm As New AddNewProductSecondForm()
            secondForm.Show()
        End Sub
    End Class
End Namespace
