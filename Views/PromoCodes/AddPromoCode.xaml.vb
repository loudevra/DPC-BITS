Imports System
Imports System.Windows
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.PromoCodes
    Public Class AddPromoCode
        Public Sub New()
            InitializeComponent()

            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar

            ' Add TopNavBar to TopNavBarContainer
            Dim topNavBar As New TopNavBar()
            TopNavBarContainer.Child = topNavBar
        End Sub

        Private Sub BtnAddPromoCode_Click(sender As Object, e As RoutedEventArgs)
            Dim promoCode As New PromoCode() With {
                .Code = txtCode.Text,
                .Amount = Decimal.Parse(txtAmount.Text),
                .Quantity = Integer.Parse(txtQuantity.Text),
                .ValidUntil = If(dpValidUntil.SelectedDate, DateTime.Now),
                .IsLinked = chkLinkToAccount.IsChecked.GetValueOrDefault(False),
                .Account = If(chkLinkToAccount.IsChecked.GetValueOrDefault(False) AndAlso cmbAccount.SelectedItem IsNot Nothing, cmbAccount.SelectedItem.ToString(), String.Empty),
                .Note = txtNote.Text
            }

            If String.IsNullOrEmpty(promoCode.Code) OrElse promoCode.Amount <= 0 OrElse promoCode.Quantity <= 0 Then
                MessageBox.Show("Please enter valid promo details.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            If PromoCodeController.AddPromoCode(promoCode) Then
                MessageBox.Show("Promo code added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                Dim ManagePromoCode As New ManagePromoCodes()
                ManagePromoCode.Show()
                Me.Close()
            Else
                MessageBox.Show("Failed to add promo code.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub

    End Class
End Namespace
