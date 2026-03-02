Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.PromoCodes
    Public Class AddPromoCode
        Public Property StartDate As New CalendarController.SingleCalendar()
        Public Sub New()
            InitializeComponent()
            StartDate.SelectedDate = Date.Today
            DataContext = Me

            AddHandler txtCode.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtQuantity.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtNote.TextChanged, AddressOf TxtToUpper_TextChanged
        End Sub

        ' Ensure typed text is converted to uppercase while preserving caret position
        Private Sub TxtToUpper_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return

            Dim originalSelectionStart = tb.SelectionStart
            Dim originalSelectionLength = tb.SelectionLength
            Dim originalText = tb.Text

            Dim upperText = originalText.ToUpperInvariant()
            If Not String.Equals(originalText, upperText, StringComparison.Ordinal) Then
                RemoveHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
                tb.Text = upperText
                tb.SelectionStart = Math.Min(originalSelectionStart, tb.Text.Length)
                tb.SelectionLength = originalSelectionLength
                AddHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
            End If
        End Sub

        Private Sub BtnAddPromoCode_Click(sender As Object, e As RoutedEventArgs)
            Dim promoCode As New PromoCode() With {
                .Code = txtCode.Text,
                .Amount = Decimal.Parse(txtAmount.Text),
                .Quantity = Integer.Parse(txtQuantity.Text),
                .ValidUntil = If(StartDatePicker.SelectedDate, DateTime.Now),
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

                ' Me.Close()
            Else
                MessageBox.Show("Failed to add promo code.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End If
        End Sub

        Private Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            StartDatePicker.IsDropDownOpen = True
        End Sub
    End Class
End Namespace
