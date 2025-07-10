Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMCorporationalBillingAddress
        Inherits UserControl
        Public Sub New()
            InitializeComponent()

            GetInfo()

            AddHandler txtAddress.TextChanged, AddressOf SetInfo
            AddHandler txtCity.SelectionChanged, AddressOf SetInfo
            AddHandler txtRegion.SelectionChanged, AddressOf SetInfo
            AddHandler txtCountry.SelectionChanged, AddressOf SetInfo
            AddHandler txtZipCode.TextChanged, AddressOf SetInfo
        End Sub

        Private Sub SetInfo()
            CorporationalClientDetails.BillAddress = txtAddress.Text
            CorporationalClientDetails.BillCity = txtCity.Text
            CorporationalClientDetails.BillRegion = txtRegion.Text
            CorporationalClientDetails.BillCountry = txtCountry.Text
            CorporationalClientDetails.BillZipCode = txtZipCode.Text
        End Sub

        Private Sub GetInfo()
            txtAddress.Text = CorporationalClientDetails.BillAddress
            txtCity.Text = CorporationalClientDetails.BillCity
            txtRegion.Text = CorporationalClientDetails.BillRegion
            txtCountry.Text = CorporationalClientDetails.BillCountry
            txtZipCode.Text = CorporationalClientDetails.BillZipCode
        End Sub

        Private Sub AddClient(sender As Object, e As RoutedEventArgs)
            If CorporationalClientDetails.Representative = Nothing OrElse
                CorporationalClientDetails.TinID = Nothing OrElse
                CorporationalClientDetails.CompanyName = Nothing OrElse
                CorporationalClientDetails.Phone = Nothing OrElse
                CorporationalClientDetails.Landline = Nothing OrElse
                CorporationalClientDetails.Email = Nothing OrElse
                CorporationalClientDetails.BillAddress = Nothing OrElse
                CorporationalClientDetails.BillCity = Nothing OrElse
                CorporationalClientDetails.BillRegion = Nothing OrElse
                CorporationalClientDetails.BillCountry = Nothing OrElse
                CorporationalClientDetails.BillZipCode = Nothing OrElse
                CorporationalClientDetails.ClientGroupID = Nothing OrElse
                CorporationalClientDetails.CustomerGroup = Nothing OrElse
                CorporationalClientDetails.CustomerLanguage = Nothing OrElse
                CorporationalClientDetails.Address = Nothing OrElse
                CorporationalClientDetails.City = Nothing OrElse
                CorporationalClientDetails.Region = Nothing OrElse
                CorporationalClientDetails.Country = Nothing OrElse
                CorporationalClientDetails.ZipCode = Nothing OrElse
                CorporationalClientDetails.SameAsBilling = Nothing Then

                MessageBox.Show("Please fill in all required fields before adding a client.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)

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
            CorporationalClientDetails.Landline = Nothing
            CorporationalClientDetails.Representative = Nothing
            CorporationalClientDetails.TinID = Nothing
            CorporationalClientDetails.CompanyName = Nothing
            CorporationalClientDetails.Phone = Nothing
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

