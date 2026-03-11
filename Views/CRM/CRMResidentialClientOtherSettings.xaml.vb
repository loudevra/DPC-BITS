' CRMResidentialClientOtherSettings.xaml.vb
Imports System.Windows.Markup
Imports DPC.Data.Helpers.ViewLoader
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers.ViewLoader
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMResidentialClientOtherSettings
        Inherits UserControl

        ' =========================================================
        ' 1. INTERNAL MEMORY (Keeps data alive across tabs)
        ' =========================================================
        Private Shared _savedClientGroupID As Integer? = Nothing
        Private Shared _savedCustomerGroup As String = ""
        Private Shared _savedLanguage As String = ""

        Public Sub New()
            InitializeComponent()

            ' 2. INITIALIZATION
            LoadCustomerGroups()

            ' 3. RESTORE DATA
            If _savedClientGroupID.HasValue Then
                cmbCustomerGroup.SelectedValue = _savedClientGroupID.Value
            End If

            If Not String.IsNullOrEmpty(_savedLanguage) Then
                cmbLanguage.Text = _savedLanguage
            End If

            ' 4. AUTO-SAVE HANDLERS
            AddHandler cmbCustomerGroup.SelectionChanged, AddressOf SaveToMemory
            AddHandler cmbLanguage.SelectionChanged, AddressOf SaveToMemory
            AddHandler cmbLanguage.KeyUp, AddressOf SaveToMemory
        End Sub

        Private Sub LoadCustomerGroups()
            Dim customerGroups = ClientGroupController.GetCustomerGroup()
            cmbCustomerGroup.DisplayMemberPath = "Value"
            cmbCustomerGroup.SelectedValuePath = "Key"
            cmbCustomerGroup.ItemsSource = customerGroups
        End Sub

        ' --- MEMORY MANAGEMENT ---
        Private Sub SaveToMemory(sender As Object, e As RoutedEventArgs)
            If cmbCustomerGroup.SelectedItem IsNot Nothing Then
                _savedClientGroupID = CInt(cmbCustomerGroup.SelectedValue)

                Dim selectedGroup As KeyValuePair(Of Integer, String) =
                    CType(cmbCustomerGroup.SelectedItem, KeyValuePair(Of Integer, String))
                _savedCustomerGroup = selectedGroup.Value
            Else
                _savedClientGroupID = Nothing
                _savedCustomerGroup = ""
            End If

            If cmbLanguage.SelectedItem IsNot Nothing Then
                Dim selectedItem As ComboBoxItem = TryCast(cmbLanguage.SelectedItem, ComboBoxItem)
                If selectedItem IsNot Nothing Then
                    _savedLanguage = selectedItem.Content.ToString()
                Else
                    _savedLanguage = cmbLanguage.Text
                End If
            Else
                _savedLanguage = cmbLanguage.Text
            End If

            ResidentialClientDetails.ClientGroupID = If(_savedClientGroupID.HasValue, _savedClientGroupID.Value, 0)
            ResidentialClientDetails.CustomerGroup = _savedCustomerGroup
            ResidentialClientDetails.CustomerLanguage = _savedLanguage
        End Sub

        ' --- ADD CLIENT BUTTON ---
        Private Sub AddClient(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrEmpty(ResidentialClientDetails.ClientName) OrElse
               String.IsNullOrEmpty(ResidentialClientDetails.Phone) OrElse
               String.IsNullOrEmpty(ResidentialClientDetails.BillAddress) OrElse
               String.IsNullOrEmpty(ResidentialClientDetails.Address) Then

                MessageBox.Show("Please fill in required fields in other tabs (Personal, Billing, Shipping).", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            If _savedClientGroupID Is Nothing Then
                MessageBox.Show("Please select a Customer Group.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
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
            ' Layer 1 - Clear Local Memory
            _savedClientGroupID = Nothing
            _savedCustomerGroup = ""
            _savedLanguage = ""

            ' Layer 2 - Clear UI
            cmbCustomerGroup.SelectedIndex = -1
            cmbLanguage.SelectedIndex = -1
            cmbLanguage.Text = ""

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
