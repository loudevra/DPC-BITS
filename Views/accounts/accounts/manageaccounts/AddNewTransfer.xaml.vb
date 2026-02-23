Namespace DPC.Views.Accounts.Accounts.ManageAccounts
    Public Class AddNewTransfer
        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            AddHandler TxtFromAccount.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler TxtToAccount.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler TxtAmount.TextChanged, AddressOf TxtToUpper_TextChanged
        End Sub

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
    End Class

End Namespace
