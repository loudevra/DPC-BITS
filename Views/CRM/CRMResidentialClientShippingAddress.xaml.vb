Imports System.Windows.Markup
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMResidentialClientShippingAddress
        Public Sub New()
            InitializeComponent()

            GetInfo()

            billingCheckBox.IsChecked = ResidentialClientDetails.SameAsBilling

            If billingCheckBox.IsChecked = True Then
                GetInfoBillAddress()
            End If

            AddHandler txtAddress.TextChanged, AddressOf SetInfo
            AddHandler txtCity.SelectionChanged, AddressOf SetInfo
            AddHandler txtRegion.SelectionChanged, AddressOf SetInfo
            AddHandler txtCountry.SelectionChanged, AddressOf SetInfo
            AddHandler txtZipCode.TextChanged, AddressOf SetInfo
            ' Uppercase behaviour is wired in XAML (TextChanged attributes); no extra AddHandler needed here.
            AddHandler billingCheckBox.Checked, AddressOf GetInfoBillAddress
            AddHandler billingCheckBox.Unchecked, Sub()
                                                      txtAddress.Text = Nothing
                                                      txtCity.Text = Nothing
                                                      txtRegion.Text = Nothing
                                                      txtCountry.Text = Nothing
                                                      txtZipCode.Text = Nothing
                                                      txtAddress.IsEnabled = True
                                                      txtCity.IsEnabled = True
                                                      txtRegion.IsEnabled = True
                                                      txtCountry.IsEnabled = True
                                                      txtZipCode.IsEnabled = True
                                                  End Sub

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

        Private Sub GetInfoBillAddress()
            txtAddress.Text = ResidentialClientDetails.BillAddress
            txtCity.Text = ResidentialClientDetails.BillCity
            txtRegion.Text = ResidentialClientDetails.BillRegion
            txtCountry.Text = ResidentialClientDetails.BillCountry
            txtZipCode.Text = ResidentialClientDetails.BillZipCode
            txtAddress.IsEnabled = False
            txtCity.IsEnabled = False
            txtRegion.IsEnabled = False
            txtCountry.IsEnabled = False
            txtZipCode.IsEnabled = False
        End Sub

        Private Sub SetInfo()
            ResidentialClientDetails.Address = txtAddress.Text
            ResidentialClientDetails.City = txtCity.Text
            ResidentialClientDetails.Region = txtRegion.Text
            ResidentialClientDetails.Country = txtCountry.Text
            ResidentialClientDetails.ZipCode = txtZipCode.Text
            ResidentialClientDetails.SameAsBilling = billingCheckBox.IsChecked.Value
        End Sub

        Private Sub GetInfo()
            txtAddress.Text = ResidentialClientDetails.Address
            txtCity.Text = ResidentialClientDetails.City
            txtRegion.Text = ResidentialClientDetails.Region
            txtCountry.Text = ResidentialClientDetails.Country
            txtZipCode.Text = ResidentialClientDetails.ZipCode
        End Sub

        Private Sub AddClient(sender As Object, e As RoutedEventArgs)
            If ResidentialClientDetails.ClientName = Nothing OrElse
   ResidentialClientDetails.Phone = Nothing OrElse
   ResidentialClientDetails.Email = Nothing OrElse
   ResidentialClientDetails.BillAddress = Nothing OrElse
   ResidentialClientDetails.BillCity = Nothing OrElse
   ResidentialClientDetails.BillRegion = Nothing OrElse
   ResidentialClientDetails.BillCountry = Nothing OrElse
   ResidentialClientDetails.BillZipCode = Nothing OrElse
   ResidentialClientDetails.ClientGroupID = Nothing OrElse
   ResidentialClientDetails.CustomerGroup = Nothing OrElse
   ResidentialClientDetails.CustomerLanguage = Nothing OrElse
   ResidentialClientDetails.Address = Nothing OrElse
   ResidentialClientDetails.City = Nothing OrElse
   ResidentialClientDetails.Region = Nothing OrElse
   ResidentialClientDetails.Country = Nothing OrElse
   ResidentialClientDetails.ZipCode = Nothing OrElse
   ResidentialClientDetails.SameAsBilling = Nothing Then

                MessageBox.Show("Please fill in all required fields before adding a client.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
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
                .ClientType = "Residential"
            }

            Dim success As Boolean = ClientController.CreateClient(client)

            If success = True Then
                MessageBox.Show("Client added successfully.")
            End If

            txtAddress.Text = Nothing
            txtCity.Text = Nothing
            txtRegion.Text = Nothing
            txtCountry.Text = Nothing
            txtZipCode.Text = Nothing
            ClearCache()
        End Sub

        Private Sub ClearCache()
            ResidentialClientDetails.ClientName = Nothing
            ResidentialClientDetails.Phone = Nothing
            ResidentialClientDetails.Email = Nothing
            ResidentialClientDetails.BillAddress = Nothing
            ResidentialClientDetails.BillCity = Nothing
            ResidentialClientDetails.BillRegion = Nothing
            ResidentialClientDetails.BillCountry = Nothing
            ResidentialClientDetails.BillZipCode = Nothing
            ResidentialClientDetails.ClientGroupID = Nothing
            ResidentialClientDetails.CustomerGroup = Nothing
            ResidentialClientDetails.CustomerLanguage = Nothing
            ResidentialClientDetails.Address = Nothing
            ResidentialClientDetails.City = Nothing
            ResidentialClientDetails.Region = Nothing
            ResidentialClientDetails.Country = Nothing
            ResidentialClientDetails.ZipCode = Nothing
            ResidentialClientDetails.SameAsBilling = Nothing
        End Sub
    End Class
End Namespace