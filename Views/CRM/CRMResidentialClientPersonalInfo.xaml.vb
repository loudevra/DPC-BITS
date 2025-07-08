Imports System.Windows.Markup
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMResidentialClientPersonalInfo
        Public Sub New()
            InitializeComponent()

            GetInfo()

            AddHandler txtName.TextChanged, AddressOf SetInfo
            AddHandler txtPhone.TextChanged, AddressOf SetInfo
            AddHandler txtEmail.TextChanged, AddressOf SetInfo
        End Sub

        Private Sub txtInput_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            ' Regex allows digits, symbols, and space
            Dim pattern As String = "^[0-9!@#$%^&*()_\-+=\.,:;?/ ]$"
            If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, pattern) Then
                e.Handled = True
            End If
        End Sub


        Private Sub SetInfo()
            ResidentialClientDetails.ClientName = txtName.Text
            ResidentialClientDetails.Phone = txtPhone.Text
            ResidentialClientDetails.Email = txtEmail.Text
        End Sub

        Private Sub GetInfo()
            txtName.Text = ResidentialClientDetails.ClientName
            txtPhone.Text = ResidentialClientDetails.Phone
            txtEmail.Text = ResidentialClientDetails.Email
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
            txtName.Text = Nothing
            txtEmail.Text = Nothing
            txtPhone.Text = Nothing
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