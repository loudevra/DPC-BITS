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

        ' Events for communication with parent forms/popups
        Public Event SupplierAdded()
        Public Event ClosePopup()

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
                TxtItem,
                LstItems,
                ChipPanel,
                AutoCompletePopup,
                brandList
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
                If String.IsNullOrWhiteSpace(TxtRepresentative.Text) OrElse
           String.IsNullOrWhiteSpace(TxtCompany.Text) OrElse
           String.IsNullOrWhiteSpace(TxtEmail.Text) OrElse
           String.IsNullOrWhiteSpace(TxtPhone.Text) Then
                    MessageBox.Show("Please fill in all required fields (*).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                Dim brandIDs As List(Of String) = autocompleteHelper.SelectedItems.Select(Function(b) b.ID.ToString()).ToList()

                SupplierController.InsertSupplier(
            TxtRepresentative.Text.Trim(), TxtCompany.Text.Trim(), TxtPhone.Text.Trim(),
            TxtEmail.Text.Trim(), TxtAddress.Text.Trim(), TxtCity.Text.Trim(),
            TxtRegion.Text.Trim(), TxtCountry.Text.Trim(), TxtPostalCode.Text.Trim(),
            TxtTINID.Text.Trim(), brandIDs)

                RaiseEvent SupplierAdded()
                ClearCacheModule()
                ClearFields()          ' <-- clears the actual UI controls
                RaiseEvent ClosePopup()

            Catch ex As Exception
                MessageBox.Show("An error occurred while adding the supplier: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Cancel button for Popup use
        Private Sub BtnCancel_Click(sender As Object, e As RoutedEventArgs)
            RaiseEvent ClosePopup()
        End Sub

#Region "Cache Management"
        ' Helper to reset the global module variables after successful save
        Private Sub ClearCacheModule()
            CacheCompanyRepresentative = String.Empty
            CacheCompanyName = String.Empty
            CachePhone = String.Empty
            CacheEmail = String.Empty
            CacheCompanyAddress = String.Empty
            CacheCompanyCity = String.Empty
            CacheCompanyRegion = String.Empty
            CacheCompanyCountry = String.Empty
            CacheCompanyPostalCode = String.Empty
            CacheCompanyTINID = String.Empty
        End Sub

        ' Add this method inside the NewSuppliers class
        Private Sub ClearFields()
            TxtRepresentative.Text = String.Empty
            TxtCompany.Text = String.Empty
            TxtPhone.Text = String.Empty
            TxtEmail.Text = String.Empty
            TxtAddress.Text = String.Empty
            TxtCity.Text = String.Empty
            TxtRegion.Text = String.Empty
            TxtCountry.Text = String.Empty
            TxtPostalCode.Text = String.Empty
            TxtTINID.Text = String.Empty
            TxtItem.Text = String.Empty
            autocompleteHelper.ClearSelection(ChipPanel)  ' Clear brand chips too
        End Sub

        ' Stores in a Module serve as cache when navigating away
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

        ' Reload all of the unsaved data again
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
#End Region

#Region "Formatting & Validation"
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
            tb.SelectionStart = Math.Min(selStart, tb.Text.Length)
            tb.SelectionLength = selLength
            AddHandler tb.TextChanged, AddressOf ForceUppercase_TextChanged
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

        Private Sub TxtTINID_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            If Not e.Text.All(AddressOf Char.IsDigit) Then
                e.Handled = True
            End If
        End Sub

        Private Sub TxtPostalCode_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            Dim textBox = CType(sender, TextBox)
            If Not e.Text.All(AddressOf Char.IsDigit) OrElse textBox.Text.Length >= 11 Then
                e.Handled = True
            End If
        End Sub

        Private Sub TxtCountry_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            If Not e.Text.All(AddressOf Char.IsLetter) Then
                e.Handled = True
            End If
        End Sub
#End Region

        ' Lifecycle event handlers
        Private Sub NewSupplierUserControl_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            UnloadCache()
        End Sub

        Private Sub NewSupplierUserControl_Unloaded(sender As Object, e As RoutedEventArgs) Handles Me.Unloaded
            StoreCache()
        End Sub
    End Class
End Namespace