Imports DocumentFormat.OpenXml.Wordprocessing
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMCorporationalPersonalInfo
        Public Sub New()
            InitializeComponent()

            GetInfo()

            AddHandler txtCompanyName.TextChanged, AddressOf SetInfo
            AddHandler txtRepresentative.TextChanged, AddressOf SetInfo
            AddHandler txtLandline.TextChanged, AddressOf SetInfo
            AddHandler txtPhone.TextChanged, AddressOf SetInfo
            AddHandler txtEmail.TextChanged, AddressOf SetInfo
        End Sub

        Private Sub SetInfo()
            CorporationalClientDetails.CompanyName = txtCompanyName.Text
            CorporationalClientDetails.Representative = txtRepresentative.Text
            CorporationalClientDetails.Phone = txtPhone.Text
            CorporationalClientDetails.Landline = txtLandline.Text
            CorporationalClientDetails.Email = txtEmail.Text
        End Sub

        Private Sub GetInfo()
            txtCompanyName.Text = CorporationalClientDetails.CompanyName
            txtRepresentative.Text = CorporationalClientDetails.Representative
            txtPhone.Text = CorporationalClientDetails.Phone
            txtLandline.Text = CorporationalClientDetails.Landline
            txtEmail.Text = CorporationalClientDetails.Email
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

            txtCompanyName.Text = Nothing
            txtRepresentative.Text = Nothing
            txtPhone.Text = Nothing
            txtLandline.Text = Nothing
            txtEmail.Text = Nothing

            ClearCache()
        End Sub

        Private Sub ClearCache()
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

