Imports System.Collections.ObjectModel
Imports System.Windows.Controls
Imports DPC.DPC.Data.Model
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Helpers


Namespace DPC.Views.Stocks.Supplier.NewSuppliers
    Public Class NewSuppliers
        Inherits UserControl

        Private brandList As ObservableCollection(Of Brand)
        Private autocompleteHelper As AutocompleteHelper(Of Brand)

        ' Static form data to persist between instances
        Private Shared _formData As SupplierFormData = New SupplierFormData()

        ' Class to hold form data
        Public Class SupplierFormData
            Public Property Representative As String = ""
            Public Property CompanyName As String = ""
            Public Property PhoneNumber As String = ""
            Public Property Email As String = ""
            Public Property Address As String = ""
            Public Property City As String = ""
            Public Property Region As String = ""
            Public Property Country As String = ""
            Public Property PostalCode As String = ""
            Public Property TINID As String = ""
            Public Property SelectedBrands As New List(Of Brand)()
        End Class

        Public Sub New()
            InitializeComponent()

            ' Load Brands from the database
            LoadBrands()

            ' Initialize autocomplete helper
            autocompleteHelper = New AutocompleteHelper(Of Brand)(
                Function(b) b.ID,
                Function(b) b.Name
            )

            ' Configure and initialize autocomplete control
            autocompleteHelper.Initialize(
                TxtItem,                   ' TextBox for input
                LstItems,                  ' ListBox for suggestions
                ChipPanel,                 ' Panel for chips
                AutoCompletePopup,         ' Popup for suggestions
                brandList                  ' Data source
            )

            ' Add event handlers for text changed events to save data as it's changed
            AddHandler TxtRepresentative.TextChanged, AddressOf SaveFormData
            AddHandler TxtCompany.TextChanged, AddressOf SaveFormData
            AddHandler TxtPhone.TextChanged, AddressOf SaveFormData
            AddHandler TxtEmail.TextChanged, AddressOf SaveFormData
            AddHandler TxtAddress.TextChanged, AddressOf SaveFormData
            AddHandler TxtCity.TextChanged, AddressOf SaveFormData
            AddHandler TxtRegion.TextChanged, AddressOf SaveFormData
            AddHandler TxtCountry.TextChanged, AddressOf SaveFormData
            AddHandler TxtPostalCode.TextChanged, AddressOf SaveFormData
            AddHandler TxtTINID.TextChanged, AddressOf SaveFormData

            ' Add handler for when brands selection changes
            AddHandler autocompleteHelper.SelectedItemsChanged, AddressOf BrandsSelectionChanged

            ' Restore previously saved form data
            RestoreFormData()
        End Sub

        ' Save form data as it changes
        Private Sub SaveFormData(sender As Object, e As TextChangedEventArgs)
            ' Get all current values directly from the text boxes to ensure accuracy
            _formData.Representative = TxtRepresentative.Text
            _formData.CompanyName = TxtCompany.Text
            _formData.PhoneNumber = TxtPhone.Text
            _formData.Email = TxtEmail.Text
            _formData.Address = TxtAddress.Text
            _formData.City = TxtCity.Text
            _formData.Region = TxtRegion.Text
            _formData.Country = TxtCountry.Text
            _formData.PostalCode = TxtPostalCode.Text
            _formData.TINID = TxtTINID.Text
        End Sub

        ' Handle changes to brand selection
        Private Sub BrandsSelectionChanged(sender As Object, items As ObservableCollection(Of Brand))
            ' Update the stored brands list
            _formData.SelectedBrands = New List(Of Brand)(items)
        End Sub

        ' Restore form data from static storage
        Private Sub RestoreFormData()
            ' Temporarily remove event handlers to prevent triggering unnecessary saves during restoration
            RemoveTextChangedHandlers()

            ' Restore text fields
            TxtRepresentative.Text = _formData.Representative
            TxtCompany.Text = _formData.CompanyName
            TxtPhone.Text = _formData.PhoneNumber
            TxtEmail.Text = _formData.Email
            TxtAddress.Text = _formData.Address
            TxtCity.Text = _formData.City
            TxtRegion.Text = _formData.Region
            TxtCountry.Text = _formData.Country
            TxtPostalCode.Text = _formData.PostalCode
            TxtTINID.Text = _formData.TINID

            ' Restore selected brands if we have any saved
            If _formData.SelectedBrands.Count > 0 Then
                ' Clear any existing selection first
                autocompleteHelper.ClearSelection(ChipPanel)

                ' Load the saved brands into the selection
                autocompleteHelper.LoadExistingSelections(_formData.SelectedBrands, ChipPanel)
            End If

            ' Re-attach event handlers after restoration
            AddTextChangedHandlers()
        End Sub

        ' Helper method to remove text changed handlers during form restoration
        Private Sub RemoveTextChangedHandlers()
            RemoveHandler TxtRepresentative.TextChanged, AddressOf SaveFormData
            RemoveHandler TxtCompany.TextChanged, AddressOf SaveFormData
            RemoveHandler TxtPhone.TextChanged, AddressOf SaveFormData
            RemoveHandler TxtEmail.TextChanged, AddressOf SaveFormData
            RemoveHandler TxtAddress.TextChanged, AddressOf SaveFormData
            RemoveHandler TxtCity.TextChanged, AddressOf SaveFormData
            RemoveHandler TxtRegion.TextChanged, AddressOf SaveFormData
            RemoveHandler TxtCountry.TextChanged, AddressOf SaveFormData
            RemoveHandler TxtPostalCode.TextChanged, AddressOf SaveFormData
            RemoveHandler TxtTINID.TextChanged, AddressOf SaveFormData
        End Sub

        ' Helper method to add text changed handlers after form restoration
        Private Sub AddTextChangedHandlers()
            AddHandler TxtRepresentative.TextChanged, AddressOf SaveFormData
            AddHandler TxtCompany.TextChanged, AddressOf SaveFormData
            AddHandler TxtPhone.TextChanged, AddressOf SaveFormData
            AddHandler TxtEmail.TextChanged, AddressOf SaveFormData
            AddHandler TxtAddress.TextChanged, AddressOf SaveFormData
            AddHandler TxtCity.TextChanged, AddressOf SaveFormData
            AddHandler TxtRegion.TextChanged, AddressOf SaveFormData
            AddHandler TxtCountry.TextChanged, AddressOf SaveFormData
            AddHandler TxtPostalCode.TextChanged, AddressOf SaveFormData
            AddHandler TxtTINID.TextChanged, AddressOf SaveFormData
        End Sub

        ' Load brands from the database
        Private Sub LoadBrands()
            brandList = New ObservableCollection(Of Brand)()

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim query As String = "SELECT brandid, brandname FROM brand;"
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                brandList.Add(New Brand With {
                                    .ID = reader.GetInt32("brandid"),
                                    .Name = reader.GetString("brandname")
                                })
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    ' Changed from MessageBox to DynamicDialogs
                    DynamicDialogs.ShowError(Me, "Error loading brands: " & ex.Message)
                End Try
            End Using
        End Sub

        ' Add supplier
        Private Sub BtnAddSupplier(sender As Object, e As RoutedEventArgs)
            Try
                ' Collect input values
                Dim supplierName As String = TxtRepresentative.Text.Trim()
                Dim companyName As String = TxtCompany.Text.Trim()
                Dim phone As String = TxtPhone.Text.Trim()
                Dim email As String = TxtEmail.Text.Trim()
                Dim address As String = TxtAddress.Text.Trim()
                Dim city As String = TxtCity.Text.Trim()
                Dim region As String = TxtRegion.Text.Trim()
                Dim country As String = TxtCountry.Text.Trim()
                Dim postalCode As String = TxtPostalCode.Text.Trim()
                Dim tinID As String = TxtTINID.Text.Trim()

                ' Validate fields
                If String.IsNullOrWhiteSpace(supplierName) OrElse String.IsNullOrWhiteSpace(companyName) OrElse
                   String.IsNullOrWhiteSpace(email) OrElse String.IsNullOrWhiteSpace(phone) Then
                    ' Changed from MessageBox to DynamicDialogs
                    DynamicDialogs.ShowWarning(Me, "Please fill in all required fields.", "Validation Error")
                    Return
                End If

                ' Get selected brand IDs from the helper
                Dim brandIDs As List(Of String) = autocompleteHelper.SelectedItems.Select(Function(b) b.ID.ToString()).ToList()

                ' Call the InsertSupplier function
                SupplierController.InsertSupplier(supplierName, companyName, phone, email, address, city, region, country, postalCode, tinID, brandIDs)

                ' Clear form and reset fields after successful insertion
                ClearForm()


            Catch ex As Exception
                ' Changed from MessageBox to DynamicDialogs
                DynamicDialogs.ShowError(Me, "An error occurred while adding the supplier: " & ex.Message, "Error")
            End Try
        End Sub

        ' Clear input fields and chips
        Private Sub ClearForm()
            ' Temporarily remove event handlers to prevent triggering saves during form clearing
            RemoveTextChangedHandlers()

            TxtRepresentative.Clear()
            TxtCompany.Clear()
            TxtPhone.Clear()
            TxtEmail.Clear()
            TxtAddress.Clear()
            TxtCity.Clear()
            TxtRegion.Clear()
            TxtCountry.Clear()
            TxtPostalCode.Clear()
            TxtTINID.Clear()
            TxtItem.Clear()

            ' Clear selected brands using the helper
            autocompleteHelper.ClearSelection(ChipPanel)


        End Sub

        ' Optional: Public method to check if there is saved form data
        Public Function HasSavedData() As Boolean
            Return Not String.IsNullOrEmpty(_formData.CompanyName) OrElse
                   Not String.IsNullOrEmpty(_formData.Representative) OrElse
                   Not String.IsNullOrEmpty(_formData.PhoneNumber) OrElse
                   Not String.IsNullOrEmpty(_formData.Email) OrElse
                   Not String.IsNullOrEmpty(_formData.Address) OrElse
                   _formData.SelectedBrands.Count > 0
        End Function
    End Class
End Namespace