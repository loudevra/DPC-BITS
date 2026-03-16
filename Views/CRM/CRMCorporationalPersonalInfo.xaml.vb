' personal info
Imports System.Windows.Markup
Imports DocumentFormat.OpenXml.Wordprocessing
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMCorporationalPersonalInfo
        Inherits UserControl

        ' =========================================================
        ' 1. INTERNAL MEMORY (Keeps data alive across tabs)
        ' =========================================================
        Private Shared _savedCompany As String = ""
        Private Shared _savedRep As String = ""
        Private Shared _savedPhone As String = ""
        Private Shared _savedLandline As String = ""
        Private Shared _savedEmail As String = ""

        Public Sub New()
            InitializeComponent()

            ' 2. RESTORE DATA
            ' Load data from memory immediately
            txtCompanyName.Text = _savedCompany
            txtRepresentative.Text = _savedRep
            txtPhone.Text = _savedPhone
            txtLandline.Text = _savedLandline
            txtEmail.Text = _savedEmail

            ' 3. AUTO-SAVE HANDLERS
            AddHandler txtCompanyName.TextChanged, AddressOf SaveToMemory
            AddHandler txtRepresentative.TextChanged, AddressOf SaveToMemory
            AddHandler txtLandline.TextChanged, AddressOf SaveToMemory
            AddHandler txtPhone.TextChanged, AddressOf SaveToMemory
            AddHandler txtEmail.TextChanged, AddressOf SaveToMemory

            ' 4. FORMATTING (Uppercase)
            AddHandler txtCompanyName.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtRepresentative.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtLandline.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtPhone.TextChanged, AddressOf TxtToUpper_TextChanged
        End Sub

        ' --- MEMORY MANAGEMENT ---
        Private Sub SaveToMemory(sender As Object, e As TextChangedEventArgs)
            ' 1. Save to Local Memory
            _savedCompany = txtCompanyName.Text
            _savedRep = txtRepresentative.Text
            _savedPhone = txtPhone.Text
            _savedLandline = txtLandline.Text
            _savedEmail = txtEmail.Text

            ' 2. Save to Global Model (For the Add Button checks)
            CorporationalClientDetails.CompanyName = txtCompanyName.Text
            CorporationalClientDetails.Representative = txtRepresentative.Text
            CorporationalClientDetails.Phone = txtPhone.Text
            CorporationalClientDetails.Landline = txtLandline.Text
            CorporationalClientDetails.Email = txtEmail.Text
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

        Private Sub txtInput_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            ' Regex allows digits, symbols, and space
            ' Removed regex restriction as per your code
        End Sub

        ' --- ADD CLIENT BUTTON ---
        Private Sub AddClient(sender As Object, e As RoutedEventArgs)
            ' Check Required Fields (Global Model Check)
            ' Note: This checks fields from ALL tabs (Billing, Shipping, etc.)
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
            _savedCompany = ""
            _savedRep = ""
            _savedPhone = ""
            _savedLandline = ""
            _savedEmail = ""

            ' Clear UI
            txtCompanyName.Text = ""
            txtRepresentative.Text = ""
            txtPhone.Text = ""
            txtLandline.Text = ""
            txtEmail.Text = ""

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