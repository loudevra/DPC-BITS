' CRMResidentialClientPersonalInfo.xaml.vb
Imports System.Windows.Markup
Imports DPC.Data.Helpers.ViewLoader
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers.ViewLoader
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMResidentialClientPersonalInfo
        Inherits UserControl

        ' =========================================================
        ' 1. INTERNAL MEMORY (Keeps data alive across tabs)
        ' =========================================================
        Private Shared _cachedName As String = ""
        Private Shared _cachedPhone As String = ""
        Private Shared _cachedEmail As String = ""

        ' Called by other tabs during ClearCache to wipe this tab's memory
        Public Shared Sub ResetMemory()
            _cachedName = ""
            _cachedPhone = ""
            _cachedEmail = ""
        End Sub

        Public Sub New()
            InitializeComponent()

            ' 2. RESTORE DATA
            If Not String.IsNullOrEmpty(_cachedName) Then txtName.Text = _cachedName
            If Not String.IsNullOrEmpty(_cachedPhone) Then txtPhone.Text = _cachedPhone
            If Not String.IsNullOrEmpty(_cachedEmail) Then txtEmail.Text = _cachedEmail

            ' 3. AUTO-SAVE HANDLERS
            AddHandler txtName.TextChanged, AddressOf SaveToMemory
            AddHandler txtPhone.TextChanged, AddressOf SaveToMemory
            AddHandler txtEmail.TextChanged, AddressOf SaveToMemory

            ' 4. FORMATTING (Uppercase)
            AddHandler txtName.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtPhone.TextChanged, AddressOf TxtToUpper_TextChanged
        End Sub

        ' --- MEMORY MANAGEMENT ---
        Private Sub SaveToMemory(sender As Object, e As TextChangedEventArgs)
            _cachedName = txtName.Text
            _cachedPhone = txtPhone.Text
            _cachedEmail = txtEmail.Text

            ResidentialClientDetails.ClientName = txtName.Text
            ResidentialClientDetails.Phone = txtPhone.Text
            ResidentialClientDetails.Email = txtEmail.Text
        End Sub

        ' --- FORMATTING ---
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

        ' --- NUMBER VALIDATION ---
        Private Sub txtInput_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            Dim pattern As String = "^[0-9!@#$%^&*()_\-+=\.,:;?/ ]$"
            If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, pattern) Then
                e.Handled = True
            End If
        End Sub

        ' --- ADD CLIENT BUTTON ---
        Private Sub AddClient(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrEmpty(ResidentialClientDetails.ClientName) OrElse
               String.IsNullOrEmpty(ResidentialClientDetails.Phone) OrElse
               String.IsNullOrEmpty(ResidentialClientDetails.Email) OrElse
               String.IsNullOrEmpty(ResidentialClientDetails.BillAddress) Then

                MessageBox.Show("Please fill in all required fields (Personal, Billing, etc.) before adding.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            Dim client As New Client With {
                .ClientGroupID = ResidentialClientDetails.ClientGroupID,
                .Name = ResidentialClientDetails.ClientName,
                .Phone = ResidentialClientDetails.Phone,
                .Email = ResidentialClientDetails.Email,
                .BillingAddress = $"{ResidentialClientDetails.BillAddress}, {ResidentialClientDetails.BillCity}, {ResidentialClientDetails.BillRegion}, {ResidentialClientDetails.BillCountry}, {ResidentialClientDetails.BillZipCode}",
                .ShippingAddress = $"{ResidentialClientDetails.Address}, {ResidentialClientDetails.City}, {ResidentialClientDetails.Region}, {ResidentialClientDetails.Country}, {ResidentialClientDetails.ZipCode}",
                .CustomerGroup = ResidentialClientDetails.CustomerGroup,
                .ClientLanguage = ResidentialClientDetails.CustomerLanguage,
                .ClientType = "Residential",
                .TinId = ""
            }

            Dim success As Boolean = ClientController.CreateClient(client)

            If success Then
                MessageBox.Show("Client added successfully.")
                ClearCache()
            End If
        End Sub

        Private Sub ClearCache()
            ' Step 1 - Reset ALL tabs' shared memory (so switching tabs won't restore old data)
            CRMResidentialClientPersonalInfo.ResetMemory()
            CRMResidentialClientBillingAddress.ResetMemory()
            CRMResidentialClientShippingAddress.ResetMemory()
            CRMResidentialClientOtherSettings.ResetMemory()

            ' Step 2 - Clear own UI fields
            txtName.Text = ""
            txtPhone.Text = ""
            txtEmail.Text = ""

            ' Step 3 - Clear Global Model (ALL fields)
            ResidentialClientDetails.ClientName = Nothing
            ResidentialClientDetails.Phone = Nothing
            ResidentialClientDetails.Email = Nothing
            ResidentialClientDetails.BillAddress = Nothing
            ResidentialClientDetails.BillCity = Nothing
            ResidentialClientDetails.BillRegion = Nothing
            ResidentialClientDetails.BillCountry = Nothing
            ResidentialClientDetails.BillZipCode = Nothing
            ResidentialClientDetails.Address = Nothing
            ResidentialClientDetails.City = Nothing
            ResidentialClientDetails.Region = Nothing
            ResidentialClientDetails.Country = Nothing
            ResidentialClientDetails.ZipCode = Nothing
            ResidentialClientDetails.ClientGroupID = 0
            ResidentialClientDetails.CustomerGroup = Nothing
            ResidentialClientDetails.CustomerLanguage = Nothing
            ResidentialClientDetails.SameAsBilling = Nothing
        End Sub

    End Class
End Namespace
