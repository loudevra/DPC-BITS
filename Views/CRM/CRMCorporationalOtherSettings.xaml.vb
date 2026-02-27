Imports ClosedXML.Excel
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models

Namespace DPC.Views.CRM
    Public Class CRMCorporationalOtherSettings
        Inherits UserControl

        ' =========================================================
        ' 1. INTERNAL MEMORY (Keeps data alive across tabs)
        ' =========================================================
        Private Shared _savedTinID As String = ""
        Private Shared _savedClientGroupID As Integer? = Nothing
        Private Shared _savedCustomerGroup As String = ""
        Private Shared _savedLanguage As String = ""

        Public Sub New()
            InitializeComponent()

            ' 2. INITIALIZATION
            LoadCustomerGroups()

            ' 3. RESTORE DATA
            txtTinID.Text = _savedTinID

            If _savedClientGroupID.HasValue Then
                cmbCustomerGroup.SelectedValue = _savedClientGroupID.Value
            End If

            If Not String.IsNullOrEmpty(_savedLanguage) Then
                cmbLanguage.Text = _savedLanguage
            End If

            ' 4. AUTO-SAVE HANDLERS
            AddHandler txtTinID.TextChanged, AddressOf SaveToMemory
            AddHandler cmbCustomerGroup.SelectionChanged, AddressOf SaveToMemory
            AddHandler cmbLanguage.SelectionChanged, AddressOf SaveToMemory
            AddHandler cmbLanguage.KeyUp, AddressOf SaveToMemory
        End Sub

        ' --- FIX: Updated Validation to allow Letters and Numbers ---
        Private Sub txtInput_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            ' Allow Alphanumeric (A-Z, 0-9), Dashes (-), and Spaces
            Dim pattern As String = "^[a-zA-Z0-9\-\s]*$"

            If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, pattern) Then
                e.Handled = True ' Block input if it doesn't match
            End If
        End Sub

        Private Sub LoadCustomerGroups()
            Dim customerGroups = ClientGroupController.GetCustomerGroup()
            cmbCustomerGroup.DisplayMemberPath = "Value"
            cmbCustomerGroup.SelectedValuePath = "Key"
            cmbCustomerGroup.ItemsSource = customerGroups
        End Sub

        ' --- MEMORY MANAGEMENT ---
        Private Sub SaveToMemory(sender As Object, e As RoutedEventArgs)
            ' 1. Save TIN
            _savedTinID = txtTinID.Text

            ' 2. Save Group
            If cmbCustomerGroup.SelectedItem IsNot Nothing Then
                _savedClientGroupID = CInt(cmbCustomerGroup.SelectedValue)

                ' Safe Cast (Value Type fix)
                Dim selectedGroup As KeyValuePair(Of Integer, String) =
                    CType(cmbCustomerGroup.SelectedItem, KeyValuePair(Of Integer, String))

                _savedCustomerGroup = selectedGroup.Value
            Else
                _savedClientGroupID = Nothing
                _savedCustomerGroup = ""
            End If

            ' 3. Save Language
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

            ' 4. Update Global Model
            CorporationalClientDetails.TinID = _savedTinID
            CorporationalClientDetails.ClientGroupID = _savedClientGroupID
            CorporationalClientDetails.CustomerGroup = _savedCustomerGroup
            CorporationalClientDetails.CustomerLanguage = _savedLanguage
        End Sub

        ' --- ADD CLIENT BUTTON ---
        Private Sub AddClient(sender As Object, e As RoutedEventArgs)
            ' Check Global Fields (Personal, Billing, Shipping)
            If String.IsNullOrEmpty(CorporationalClientDetails.CompanyName) OrElse
               String.IsNullOrEmpty(CorporationalClientDetails.BillAddress) OrElse
               String.IsNullOrEmpty(CorporationalClientDetails.Address) Then

                MessageBox.Show("Please fill in required fields in other tabs (Personal, Billing, Shipping).", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                Exit Sub
            End If

            ' Check Local Fields
            If String.IsNullOrEmpty(txtTinID.Text) Then
                MessageBox.Show("Please fill in the TIN ID.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning)
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
            _savedTinID = ""
            _savedClientGroupID = Nothing
            _savedCustomerGroup = ""
            _savedLanguage = ""

            ' Clear UI
            txtTinID.Text = ""
            cmbCustomerGroup.SelectedIndex = -1
            cmbLanguage.SelectedIndex = -1
            cmbLanguage.Text = ""

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