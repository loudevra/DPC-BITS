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

        ' Cache the value of the textbox whenever it is empty
        Private Shared viewCache As New Dictionary(Of String, UserControl)

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
                    MessageBox.Show("Error loading brands: " & ex.Message)
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
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                ' Get selected brand IDs from the helper
                Dim brandIDs As List(Of String) = autocompleteHelper.SelectedItems.Select(Function(b) b.ID.ToString()).ToList()

                ' Call the InsertSupplier function
                SupplierController.InsertSupplier(supplierName, companyName, phone, email, address, city, region, country, postalCode, tinID, brandIDs)

                ' Clear form and reset fields after successful insertion

                UnloadCache()

                ViewLoader.DynamicView.NavigateToView("managesuppliers", Me)
            Catch ex As Exception
                MessageBox.Show("An error occurred while adding the supplier: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub



        Private Sub TxtPhone_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            If Not e.Text.All(AddressOf Char.IsDigit) Then
                e.Handled = True
                Return
            End If

            ' Limit to 11 digits
            Dim textBox = CType(sender, TextBox)
            If textBox.Text.Length >= 11 Then
                e.Handled = True
            End If
        End Sub

        Private Sub NewSupplierUserControl_Unloaded(sender As Object, e As RoutedEventArgs)
            StoreCache()
        End Sub

        Private Sub NewSupplierUserControl_Loaded(sender As Object, e As RoutedEventArgs)
            UnloadCache()
        End Sub

        ' Stores in a Module serve as cache
        Private Sub StoreCache()
            CacheCompanyRepresentative = If(TxtRepresentative IsNot Nothing, TxtRepresentative.Text.ToUpperInvariant(), String.Empty)
            CacheCompanyName = If(TxtCompany IsNot Nothing, TxtCompany.Text.ToUpperInvariant(), String.Empty)
            CachePhone = If(TxtPhone IsNot Nothing, TxtPhone.Text, String.Empty)
            CacheEmail = If(TxtEmail IsNot Nothing, TxtEmail.Text.ToUpperInvariant(), String.Empty)
            CacheCompanyAddress = If(TxtAddress IsNot Nothing, TxtAddress.Text.ToUpperInvariant(), String.Empty)
            CacheCompanyCity = If(TxtCity IsNot Nothing, TxtCity.Text.ToUpperInvariant(), String.Empty)
            CacheCompanyRegion = If(TxtRegion IsNot Nothing, TxtRegion.Text.ToUpperInvariant(), String.Empty)
            CacheCompanyCountry = If(TxtCountry IsNot Nothing, TxtCountry.Text.ToUpperInvariant(), String.Empty)
            CacheCompanyPostalCode = If(TxtPostalCode IsNot Nothing, TxtPostalCode.Text, String.Empty)
            CacheCompanyTINID = If(TxtTINID IsNot Nothing, TxtTINID.Text, String.Empty)
        End Sub

        ' Ensure any user input is displayed in uppercase regardless of keyboard state
        Private Sub ForceUppercase_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return

            Dim original = tb.Text
            Dim upper = original.ToUpperInvariant()
            If original = upper Then Return

            Dim selStart = tb.SelectionStart
            Dim selLength = tb.SelectionLength

            RemoveHandler tb.TextChanged, AddressOf ForceUppercase_TextChanged
            tb.Text = upper
            ' Restore selection/caret
            tb.SelectionStart = Math.Min(selStart, tb.Text.Length)
            tb.SelectionLength = selLength
            AddHandler tb.TextChanged, AddressOf ForceUppercase_TextChanged
        End Sub

        ' Reload all of the unsave data again
        Private Sub UnloadCache()
            TxtRepresentative.Text = CacheCompanyRepresentative
            TxtCompany.Text = CacheCompanyName
            TxtPhone.Text = CachePhone
            TxtEmail.Text = CacheEmail
            TxtAddress.Text = CacheCompanyAddress
            TxtCity.Text = CacheCompanyCity
            TxtRegion.Text = CacheCompanyRegion
            TxtCountry.Text = CacheCompanyCountry
            TxtPostalCode.Text = CacheCompanyPostalCode
            TxtTINID.Text = CacheCompanyTINID
        End Sub

        Private Sub TxtTINID_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            If Not e.Text.All(AddressOf Char.IsDigit) Then
                e.Handled = True
                Return
            End If
        End Sub

        Private Sub TxtPostalCode_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            ' Limit to 11 digits
            Dim textBox = CType(sender, TextBox)
            If textBox.Text.Length >= 11 Then
                e.Handled = True
            End If
        End Sub

        Private Sub TxtCountry_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            If Not e.Text.All(AddressOf Char.IsLetter) Then
                e.Handled = True
                Return
            End If
        End Sub
    End Class
End Namespace