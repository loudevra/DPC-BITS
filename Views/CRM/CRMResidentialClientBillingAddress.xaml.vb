Imports System.Windows.Markup
Imports DocumentFormat.OpenXml.Wordprocessing
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMResidentialClientBillingAddress
        Inherits UserControl

        ' =========================================================
        ' 1. INTERNAL MEMORY
        ' These variables ensure data persists when you switch tabs
        ' =========================================================
        Private Shared _cachedAddress As String = ""
        Private Shared _cachedCity As String = ""
        Private Shared _cachedRegion As String = ""
        Private Shared _cachedCountry As String = ""
        Private Shared _cachedZipCode As String = ""

        Public Sub New()
            InitializeComponent()

            ' 1. RESTORE: Load saved data immediately when the page loads
            txtAddress.Text = _cachedAddress
            txtCity.Text = _cachedCity
            txtRegion.Text = _cachedRegion
            txtCountry.Text = _cachedCountry
            txtZipCode.Text = _cachedZipCode

            ' 2. AUTO-SAVE: Save to memory every time you type
            AddHandler txtAddress.TextChanged, AddressOf SaveToMemory
            AddHandler txtCity.TextChanged, AddressOf SaveToMemory
            AddHandler txtRegion.TextChanged, AddressOf SaveToMemory
            AddHandler txtCountry.TextChanged, AddressOf SaveToMemory
            AddHandler txtZipCode.TextChanged, AddressOf SaveToMemory

            ' 3. FORMATTING: Uppercase logic
            AddHandler txtAddress.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtCity.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtRegion.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtCountry.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtZipCode.TextChanged, AddressOf TxtToUpper_TextChanged
        End Sub

        ' --- MEMORY MANAGEMENT ---
        Private Sub SaveToMemory(sender As Object, e As RoutedEventArgs)
            _cachedAddress = txtAddress.Text
            _cachedCity = txtCity.Text
            _cachedRegion = txtRegion.Text
            _cachedCountry = txtCountry.Text
            _cachedZipCode = txtZipCode.Text

            ' Optional: Sync with global model if needed for the Add button
            ResidentialClientDetails.BillAddress = txtAddress.Text
            ResidentialClientDetails.BillCity = txtCity.Text
            ResidentialClientDetails.BillRegion = txtRegion.Text
            ResidentialClientDetails.BillCountry = txtCountry.Text
            ResidentialClientDetails.BillZipCode = txtZipCode.Text
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
            ' Note: This checks global ResidentialClientDetails for Name/Phone/Email. 
            ' Ensure those are set in the other tabs for this check to pass.
            If String.IsNullOrEmpty(ResidentialClientDetails.ClientName) OrElse
               String.IsNullOrEmpty(ResidentialClientDetails.Phone) OrElse
               String.IsNullOrEmpty(ResidentialClientDetails.Email) OrElse
               String.IsNullOrEmpty(txtAddress.Text) OrElse
               String.IsNullOrEmpty(txtCity.Text) OrElse
               String.IsNullOrEmpty(txtRegion.Text) OrElse
               String.IsNullOrEmpty(txtCountry.Text) OrElse
               String.IsNullOrEmpty(txtZipCode.Text) Then

                MessageBox.Show("Please fill in all required fields before adding a client.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            Dim client As New Client With {
                .ClientGroupID = ResidentialClientDetails.ClientGroupID,
                .Name = ResidentialClientDetails.ClientName,
                .Phone = ResidentialClientDetails.Phone,
                .Email = ResidentialClientDetails.Email,
                .BillingAddress = $"{txtAddress.Text}, {txtCity.Text}, {txtRegion.Text}, {txtCountry.Text}, {txtZipCode.Text}",
                .ShippingAddress = $"{ResidentialClientDetails.Address}, {ResidentialClientDetails.City}, {ResidentialClientDetails.Region}, {ResidentialClientDetails.Country}, {ResidentialClientDetails.ZipCode}",
                .CustomerGroup = ResidentialClientDetails.CustomerGroup,
                .ClientLanguage = ResidentialClientDetails.CustomerLanguage,
                .ClientType = "Residential"
            }

            Dim success As Boolean = ClientController.CreateClient(client)

            If success Then
                MessageBox.Show("Client added successfully.")
                ClearCache()
            End If
        End Sub

        Private Sub ClearCache()
            ' Clear Local Memory
            _cachedAddress = ""
            _cachedCity = ""
            _cachedRegion = ""
            _cachedCountry = ""
            _cachedZipCode = ""

            ' Clear UI
            txtAddress.Text = ""
            txtCity.Text = ""
            txtRegion.Text = ""
            txtCountry.Text = ""
            txtZipCode.Text = ""

            ' Clear Global Model (Optional)
            ResidentialClientDetails.ClientName = Nothing
            ResidentialClientDetails.Phone = Nothing
            ResidentialClientDetails.Email = Nothing
            ResidentialClientDetails.BillAddress = Nothing
            ResidentialClientDetails.BillCity = Nothing
            ResidentialClientDetails.BillRegion = Nothing
            ResidentialClientDetails.BillCountry = Nothing
            ResidentialClientDetails.BillZipCode = Nothing
            ' ... (Clear other fields as needed)
        End Sub

    End Class
End Namespace