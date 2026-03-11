' CRMResidentialClientShippingAddress.xaml.vb
Imports System.Windows.Markup
Imports DPC.Data.Helpers.ViewLoader
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers.ViewLoader
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMResidentialClientShippingAddress
        Inherits UserControl

        ' =========================================================
        ' 1. INTERNAL MEMORY (Keeps data alive across tabs)
        ' =========================================================
        Private Shared _savedAddress As String = ""
        Private Shared _savedCity As String = ""
        Private Shared _savedRegion As String = ""
        Private Shared _savedCountry As String = ""
        Private Shared _savedZipCode As String = ""
        Private Shared _savedSameAsBilling As Boolean = False

        Public Sub New()
            InitializeComponent()

            ' 2. RESTORE DATA
            txtAddress.Text = _savedAddress
            txtCity.Text = _savedCity
            txtRegion.Text = _savedRegion
            txtCountry.Text = _savedCountry
            txtZipCode.Text = _savedZipCode
            billingCheckBox.IsChecked = _savedSameAsBilling

            If _savedSameAsBilling Then
                GetInfoBillAddress()
            End If

            ' 3. AUTO-SAVE HANDLERS
            AddHandler txtAddress.TextChanged, AddressOf SaveToMemory
            AddHandler txtCity.TextChanged, AddressOf SaveToMemory
            AddHandler txtRegion.TextChanged, AddressOf SaveToMemory
            AddHandler txtCountry.TextChanged, AddressOf SaveToMemory
            AddHandler txtZipCode.TextChanged, AddressOf SaveToMemory

            ' 4. CHECKBOX LOGIC
            AddHandler billingCheckBox.Checked, AddressOf CheckBox_Changed
            AddHandler billingCheckBox.Unchecked, AddressOf CheckBox_Changed

            ' 5. UPPERCASE FORMATTING
            AddHandler txtAddress.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtCity.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtRegion.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtCountry.TextChanged, AddressOf TxtToUpper_TextChanged
            AddHandler txtZipCode.TextChanged, AddressOf TxtToUpper_TextChanged
        End Sub

        ' --- MEMORY MANAGEMENT ---
        Private Sub SaveToMemory(sender As Object, e As TextChangedEventArgs)
            _savedAddress = txtAddress.Text
            _savedCity = txtCity.Text
            _savedRegion = txtRegion.Text
            _savedCountry = txtCountry.Text
            _savedZipCode = txtZipCode.Text

            ResidentialClientDetails.Address = txtAddress.Text
            ResidentialClientDetails.City = txtCity.Text
            ResidentialClientDetails.Region = txtRegion.Text
            ResidentialClientDetails.Country = txtCountry.Text
            ResidentialClientDetails.ZipCode = txtZipCode.Text
        End Sub

        ' --- CHECKBOX LOGIC ---
        Private Sub CheckBox_Changed(sender As Object, e As RoutedEventArgs)
            _savedSameAsBilling = billingCheckBox.IsChecked.GetValueOrDefault()
            ResidentialClientDetails.SameAsBilling = _savedSameAsBilling

            If _savedSameAsBilling Then
                GetInfoBillAddress()
            Else
                SetFieldsEnabled(True)
                txtAddress.Text = ""
                txtCity.Text = ""
                txtRegion.Text = ""
                txtCountry.Text = ""
                txtZipCode.Text = ""
            End If
        End Sub

        Private Sub GetInfoBillAddress()
            txtAddress.Text = ResidentialClientDetails.BillAddress
            txtCity.Text = ResidentialClientDetails.BillCity
            txtRegion.Text = ResidentialClientDetails.BillRegion
            txtCountry.Text = ResidentialClientDetails.BillCountry
            txtZipCode.Text = ResidentialClientDetails.BillZipCode

            SetFieldsEnabled(False)

            _savedAddress = txtAddress.Text
            _savedCity = txtCity.Text
            _savedRegion = txtRegion.Text
            _savedCountry = txtCountry.Text
            _savedZipCode = txtZipCode.Text
        End Sub

        Private Sub SetFieldsEnabled(isEnabled As Boolean)
            txtAddress.IsEnabled = isEnabled
            txtCity.IsEnabled = isEnabled
            txtRegion.IsEnabled = isEnabled
            txtCountry.IsEnabled = isEnabled
            txtZipCode.IsEnabled = isEnabled
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

        ' --- ADD CLIENT BUTTON ---
        Private Sub AddClient(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrEmpty(ResidentialClientDetails.ClientName) OrElse
               String.IsNullOrEmpty(ResidentialClientDetails.BillAddress) Then

                MessageBox.Show("Please fill in required fields in Personal Info and Billing tabs.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            If String.IsNullOrEmpty(txtAddress.Text) OrElse
               String.IsNullOrEmpty(txtCity.Text) Then

                MessageBox.Show("Please fill in all shipping address fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            Dim client As New Client With {
                .ClientGroupID = ResidentialClientDetails.ClientGroupID,
                .Name = ResidentialClientDetails.ClientName,
                .Phone = ResidentialClientDetails.Phone,
                .Email = ResidentialClientDetails.Email,
                .BillingAddress = $"{ResidentialClientDetails.BillAddress}, {ResidentialClientDetails.BillCity}, {ResidentialClientDetails.BillRegion}, {ResidentialClientDetails.BillCountry}, {ResidentialClientDetails.BillZipCode}",
                .ShippingAddress = $"{txtAddress.Text}, {txtCity.Text}, {txtRegion.Text}, {txtCountry.Text}, {txtZipCode.Text}",
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
            ' Layer 1 - Clear Local Memory
            _savedAddress = ""
            _savedCity = ""
            _savedRegion = ""
            _savedCountry = ""
            _savedZipCode = ""
            _savedSameAsBilling = False

            ' Layer 2 - Clear UI
            txtAddress.Text = ""
            txtCity.Text = ""
            txtRegion.Text = ""
            txtCountry.Text = ""
            txtZipCode.Text = ""
            billingCheckBox.IsChecked = False
            SetFieldsEnabled(True)

            ' Layer 3 - Clear Global Model (ALL fields)
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
