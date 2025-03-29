Imports System.Data
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Views.HRM.Employees


Namespace DPC.Views.PromoCodes
    Partial Public Class ManagePromoCodes
        Inherits Window

        Public Sub New()
            InitializeComponent()

            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar

            ' Add TopNavBar to TopNavBarContainer
            Dim topNavBar As New TopNavBar()
            TopNavBarContainer.Child = topNavBar

            LoadPromoCodes()
        End Sub


        Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
            Dim addPromoCode As New AddPromoCode()
            addPromoCode.Show()
            Me.Close()
        End Sub

        Private Sub LoadPromoCodes()
            Dim promoCodes As DataTable = PromoCodeController.FetchPromoCodes()

            If promoCodes IsNot Nothing Then
                dgPromoCodes.ItemsSource = promoCodes.DefaultView
            End If
        End Sub
    End Class
End Namespace
