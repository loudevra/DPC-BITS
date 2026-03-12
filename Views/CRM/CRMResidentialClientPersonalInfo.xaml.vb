Imports System.Windows.Markup
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMResidentialClientPersonalInfo
        Inherits UserControl

        ' =========================================================
        ' 1. INTERNAL MEMORY
        ' These "Shared" variables keep your data alive when you switch tabs.
        ' =========================================================
        Private Shared _cachedName As String = ""
        Private Shared _cachedPhone As String = ""
        Private Shared _cachedEmail As String = ""

        Public Sub New()
            InitializeComponent()

            ' =========================================================
            ' 2. RESTORE DATA
            ' Immediately put the saved text back into the boxes.
            ' =========================================================
            If Not String.IsNullOrEmpty(_cachedName) Then txtName.Text = _cachedName
            If Not String.IsNullOrEmpty(_cachedPhone) Then txtPhone.Text = _cachedPhone
            If Not String.IsNullOrEmpty(_cachedEmail) Then txtEmail.Text = _cachedEmail

            ' =========================================================
            ' 3. ENABLE AUTO-SAVE
            ' Watch for typing and save to memory instantly.
            ' =========================================================
            AddHandler txtName.TextChanged, AddressOf SaveToMemory
            AddHandler txtPhone.TextChanged, AddressOf SaveToMemory
            AddHandler txtEmail.TextChanged, AddressOf SaveToMemory

            ' =========================================================
            ' 4. FORMATTING
            ' =========================================================
            AddHandler txtName.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtPhone.TextChanged, AddressOf TxtToUpper_TextChanged
        End Sub

        ' This method saves your text to the Shared variables every time you type.
        Private Sub SaveToMemory(sender As Object, e As TextChangedEventArgs)
            _cachedName = txtName.Text
            _cachedPhone = txtPhone.Text
            _cachedEmail = txtEmail.Text
        End Sub

        ' --- UPPERCASE FORMATTING LOGIC ---
        Private Sub TxtToUpper_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return

            Dim originalSelectionStart = tb.SelectionStart
            Dim originalText = tb.Text
            Dim upperText = originalText.ToUpperInvariant()

            If Not String.Equals(originalText, upperText, StringComparison.Ordinal) Then
                RemoveHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
                tb.Text = upperText
                tb.SelectionStart = Math.Min(originalSelectionStart, tb.Text.Length)
                AddHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
            End If
        End Sub

        ' --- NUMBER VALIDATION LOGIC ---
        Private Sub txtInput_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            Dim pattern As String = "^[0-9!@#$%^&*()_\-+=\.,:;?/ ]$"
            If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, pattern) Then
                e.Handled = True
            End If
        End Sub

        ' --- SUBMIT BUTTON LOGIC ---
        Private Sub AddClient(sender As Object, e As RoutedEventArgs)
            ' Check required fields
            If String.IsNullOrEmpty(txtName.Text) OrElse
               String.IsNullOrEmpty(txtPhone.Text) OrElse
               String.IsNullOrEmpty(txtEmail.Text) Then
                MessageBox.Show("Please fill in all required fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Create the Client object (Make sure to use commas correctly!)
            Dim client As New Client With {
                .Name = txtName.Text,
                .Phone = txtPhone.Text,
                .Email = txtEmail.Text,
                .ClientType = "Residential",
                .BillingAddress = "",
                .ShippingAddress = ""
            }

            Dim success As Boolean = ClientController.CreateClient(client)

            If success Then
                MessageBox.Show("Client added successfully.")

                ' Clear the memory and the text boxes ONLY after success
                _cachedName = ""
                _cachedPhone = ""
                _cachedEmail = ""

                txtName.Text = ""
                txtPhone.Text = ""
                txtEmail.Text = ""
            End If
        End Sub

    End Class
End Namespace