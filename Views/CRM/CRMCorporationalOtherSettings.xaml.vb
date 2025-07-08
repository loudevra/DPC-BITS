Imports ClosedXML.Excel
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMCorporationalOtherSettings
        Inherits UserControl
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            LoadCustomerGroups()
            GetInfo()

            AddHandler cmbCustomerGroup.SelectionChanged, AddressOf SetInfo
            AddHandler cmbLanguage.SelectionChanged, AddressOf SetInfo
            AddHandler txtTinID.TextChanged, AddressOf SetInfo
        End Sub

        Private Sub LoadCustomerGroups()
            Dim customerGroups = ClientGroupController.GetCustomerGroup()
            cmbCustomerGroup.DisplayMemberPath = "Value"
            cmbCustomerGroup.SelectedValuePath = "Key"
            cmbCustomerGroup.ItemsSource = customerGroups
        End Sub

        Private Sub SetInfo()
            Dim selectedGroup = CType(cmbCustomerGroup.SelectedItem, KeyValuePair(Of Integer, String))
            Dim selectedItem As ComboBoxItem = TryCast(cmbLanguage.SelectedItem, ComboBoxItem)

            CorporationalClientDetails.TinID = txtTinID.Text.Trim()
            CorporationalClientDetails.ClientGroupID = cmbCustomerGroup.SelectedValue

            If String.IsNullOrWhiteSpace(selectedGroup.Value) Then
                CorporationalClientDetails.CustomerGroup = ""
            Else
                CorporationalClientDetails.CustomerGroup = selectedGroup.Value.ToString()
            End If

            CorporationalClientDetails.CustomerLanguage = selectedItem?.Content.ToString()
        End Sub

        Private Sub GetInfo()
            txtTinID.Text = CorporationalClientDetails.TinID
            cmbCustomerGroup.SelectedValue = CorporationalClientDetails.ClientGroupID
            cmbLanguage.Text = CorporationalClientDetails.CustomerLanguage
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

            txtTinID.Text = Nothing
            cmbCustomerGroup.SelectedIndex = -1
            cmbLanguage.SelectedIndex = -1

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

