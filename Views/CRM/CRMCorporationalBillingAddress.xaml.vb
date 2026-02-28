Imports System.Windows.Markup
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMCorporationalBillingAddress
        Inherits UserControl

        ' =========================================================
        ' 1. INTERNAL MEMORY (Keeps data alive across tabs)
        ' =========================================================
        Private Shared _savedAddress As String = ""
        Private Shared _savedCity As String = ""
        Private Shared _savedRegion As String = ""
        Private Shared _savedCountry As String = ""
        Private Shared _savedZipCode As String = ""

        Public Sub New()
            InitializeComponent()

            ' 2. RESTORE DATA
            ' Load data from memory immediately
            txtAddress.Text = _savedAddress
            txtCity.Text = _savedCity
            txtRegion.Text = _savedRegion
            txtCountry.Text = _savedCountry
            txtZipCode.Text = _savedZipCode

            ' 3. AUTO-SAVE HANDLERS
            ' Use TextChanged to capture every keystroke
            AddHandler txtAddress.TextChanged, AddressOf SaveToMemory
            AddHandler txtCity.TextChanged, AddressOf SaveToMemory
            AddHandler txtRegion.TextChanged, AddressOf SaveToMemory
            AddHandler txtCountry.TextChanged, AddressOf SaveToMemory
            AddHandler txtZipCode.TextChanged, AddressOf SaveToMemory

            ' 4. FORMATTING (Uppercase)
            AddHandler txtAddress.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtCity.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtRegion.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtCountry.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtZipCode.TextChanged, AddressOf TxtToUpper_TextChanged
        End Sub

        ' --- MEMORY MANAGEMENT ---
        Private Sub SaveToMemory(sender As Object, e As RoutedEventArgs)
            ' 1. Save to Local Memory
            _savedAddress = txtAddress.Text
            _savedCity = txtCity.Text
            _savedRegion = txtRegion.Text
            _savedCountry = txtCountry.Text
            _savedZipCode = txtZipCode.Text

            ' 2. Save to Global Model (For the Add Button)
            CorporationalClientDetails.BillAddress = txtAddress.Text
            CorporationalClientDetails.BillCity = txtCity.Text
            CorporationalClientDetails.BillRegion = txtRegion.Text
            CorporationalClientDetails.BillCountry = txtCountry.Text
            CorporationalClientDetails.BillZipCode = txtZipCode.Text
        End Sub

        ' --- FORMATTING ---
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

        ' --- ADD CLIENT BUTTON ---
        Private Sub AddClient(sender As Object, e As RoutedEventArgs)
            ' Check Required Fields (Global Model Check)
            If String.IsNullOrEmpty(CorporationalClientDetails.CompanyName) OrElse
               String.IsNullOrEmpty(CorporationalClientDetails.Representative) OrElse
               String.IsNullOrEmpty(CorporationalClientDetails.Phone) OrElse
               String.IsNullOrEmpty(CorporationalClientDetails.Email) OrElse
               String.IsNullOrEmpty(CorporationalClientDetails.BillAddress) Then

                MessageBox.Show("Please fill in all required fields (Personal, Billing, etc.) before adding.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            Dim client As New ClientCorporational With {
                .ClientGroupID = CorporationalClientDetails.ClientGroupID,
                .Company = CorporationalClientDetails.CompanyName,
                .Representative = CorporationalClientDetails.Representative,
                .Phone = CorporationalClientDetails.Phone,
                .Landline = CorporationalClientDetails.Landline,
                .Email = CorporationalClientDetails.Email,
                .BillingAddress = $"{CorporationalClientDetails.BillAddress}, {CorporationalClientDetails.BillCity}, {CorporationalClientDetails.BillRegion}, {CorporationalClientDetails.BillCountry}, {CorporationalClientDetails.BillZipCode}",
                .ShippingAddress = $"{CorporationalClientDetails.Address}, {CorporationalClientDetails.City}, {CorporationalClientDetails.Region}, {CorporationalClientDetails.Country}, {CorporationalClientDetails.ZipCode}",
                .CustomerGroup = CorporationalClientDetails.CustomerGroup,
                .ClientLanguage = CorporationalClientDetails.CustomerLanguage,
                .ClientType = "Corporational",
                .TinID = CorporationalClientDetails.TinID
            }

            Dim success As Boolean = ClientController.CreateClientCorporational(client)

            If success Then
                MessageBox.Show("Client added successfully.")
                ClearCache()
            End If
        End Sub

        Private Sub ClearCache()
            ' Clear Local Memory
            _savedAddress = ""
            _savedCity = ""
            _savedRegion = ""
            _savedCountry = ""
            _savedZipCode = ""

            ' Clear UI
            txtAddress.Text = ""
            txtCity.Text = ""
            txtRegion.Text = ""
            txtCountry.Text = ""
            txtZipCode.Text = ""

            ' Clear Global Model
            CorporationalClientDetails.Representative = Nothing
            CorporationalClientDetails.TinID = Nothing
            CorporationalClientDetails.CompanyName = Nothing
            CorporationalClientDetails.Phone = Nothing
            CorporationalClientDetails.Landline = Nothing
            CorporationalClientDetails.Email = Nothing
            CorporationalClientDetails.BillAddress = Nothing
            CorporationalClientDetails.BillCity = Nothing
            CorporationalClientDetails.BillRegion = Nothing
            CorporationalClientDetails.BillCountry = Nothing
            CorporationalClientDetails.BillZipCode = Nothing
            CorporationalClientDetails.ClientGroupID = Nothing
            CorporationalClientDetails.CustomerGroup = Nothing
            CorporationalClientDetails.CustomerLanguage = Nothing
            CorporationalClientDetails.Address = Nothing
            CorporationalClientDetails.City = Nothing
            CorporationalClientDetails.Region = Nothing
            CorporationalClientDetails.Country = Nothing
            CorporationalClientDetails.ZipCode = Nothing
            CorporationalClientDetails.SameAsBilling = Nothing
        End Sub
    End Class
End Namespace