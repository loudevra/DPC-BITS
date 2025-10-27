Imports System.Collections.ObjectModel
Imports System.Windows.Controls
Imports DPC.DPC.Data.Model
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Helpers
Imports System.Windows
Imports System.Windows.Input ' Added for Cursors

Namespace DPC.Views.Stocks.Suppliers.NewSupplier
    Public Class EditSuppliers
        Inherits UserControl

        Private brandList As ObservableCollection(Of Brand)
        Private autocompleteHelper As AutocompleteHelper(Of Brand)
        Private currentSupplierID As String = ""

        ' Assuming these cache variables are declared globally or shared elsewhere, 
        ' but using them as they appear in the original code.
        ' They are added here for context, assuming they are Public Shared or accessible.

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

        ' Load supplier data when the control is loaded - Using Handles
        Private Sub EditSuppliers_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            LoadSupplierData()
        End Sub

        ' Unload handler - Using Handles
        Private Sub EditSuppliers_Unloaded(sender As Object, e As RoutedEventArgs) Handles Me.Unloaded
            ClearAllCacheValues(False)
        End Sub

        ' Load supplier data from cache
        Private Sub LoadSupplierData()
            ' Check if we have a supplier ID in cache
            If Not String.IsNullOrEmpty(cacheSupplierID) Then
                currentSupplierID = cacheSupplierID

                ' Load supplier details
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

                ' Load associated brands
                LoadSupplierBrands()
            End If
        End Sub

        ' Load brands associated with this supplier
        Private Sub LoadSupplierBrands()
            If String.IsNullOrEmpty(currentSupplierID) Then Return

            Dim supplierBrands As List(Of Brand) = SupplierController.GetBrandsForSupplier(currentSupplierID)

            ' Clear existing selections
            ChipPanel.Children.Clear()
            autocompleteHelper.SelectedItems.Clear()

            ' Manually add each brand
            For Each brand As Brand In supplierBrands
                Dim matchingBrand = brandList.FirstOrDefault(Function(b) b.ID = brand.ID)
                If matchingBrand IsNot Nothing Then
                    ' Add to the internal selected items list
                    autocompleteHelper.SelectedItems.Add(matchingBrand)

                    ' Create chip manually
                    Dim chip As Border = CreateSupplierChip(matchingBrand)
                    ChipPanel.Children.Add(chip)
                End If
            Next
        End Sub

        ' Helper method to create a chip
        Private Function CreateSupplierChip(brand As Brand) As Border
            Dim chip As New Border With {
                .Background = New SolidColorBrush(Color.FromRgb(&H52, &HC6, &H9D)),
                .CornerRadius = New CornerRadius(15),
                .Padding = New Thickness(10, 5, 10, 5),
                .Margin = New Thickness(5)
            }

            Dim stackPanel As New StackPanel With {
                .Orientation = Orientation.Horizontal
            }

            Dim textBlock As New TextBlock With {
                .Text = brand.Name,
                .Foreground = Brushes.White,
                .VerticalAlignment = VerticalAlignment.Center,
                .Margin = New Thickness(0, 0, 5, 0),
                .FontFamily = New FontFamily("Lexend"),
                .FontWeight = FontWeights.SemiBold
            }

            Dim closeButton As New Button With {
                .Content = "×",
                .Background = Brushes.Transparent,
                .Foreground = Brushes.White,
                .BorderThickness = New Thickness(0),
                .FontSize = 16,
                .FontWeight = FontWeights.Bold,
                .Cursor = Cursors.Hand,
                .Padding = New Thickness(5, 0, 5, 0)
            }

            ' Handle chip removal
            AddHandler closeButton.Click, Sub(s, args)
                                              ChipPanel.Children.Remove(chip)
                                              autocompleteHelper.SelectedItems.Remove(brand)
                                          End Sub

            stackPanel.Children.Add(textBlock)
            stackPanel.Children.Add(closeButton)
            chip.Child = stackPanel

            Return chip
        End Function

        ' Update supplier
        Private Sub BtnUpdateSupplier_click(sender As Object, e As RoutedEventArgs) Handles BtnUpdateSupplier.Click
            ' Collect all input values outside the Try block for cleaner access in the Catch block if needed
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
            If String.IsNullOrWhiteSpace(companyName) OrElse String.IsNullOrWhiteSpace(phone) Then
                MessageBox.Show("Please fill in all required fields (Company Name and Phone Number).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' Validate currentSupplierID is set (Critical for Update)
            If String.IsNullOrEmpty(currentSupplierID) Then
                MessageBox.Show("Cannot update: Supplier ID is missing from cache.", "System Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            ' The invalid cache logic has been removed from here.

            Try
                ' Get selected brand IDs from the helper
                Dim brandIDs As List(Of String) = autocompleteHelper.SelectedItems.Select(Function(b) b.ID.ToString()).ToList()

                ' Call the UpdateSupplier function
                SupplierController.UpdateSupplier(currentSupplierID, supplierName, companyName, phone, email, address, city, region, country, postalCode, tinID, brandIDs)

                ' Clear cache and navigate back to manage suppliers
                ClearAllCacheValues(False)
                ViewLoader.DynamicView.NavigateToView("managesuppliers", Me)

            Catch ex As Exception
                ' Display the actual error message to help debugging, especially if the exception 
                ' is related to database constraints or remaining type conversion issues.
                MessageBox.Show("An error occurred while updating the supplier: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Cancel button handler
        Private Sub BtnCancelSupUpdate_click(sender As Object, e As RoutedEventArgs) Handles BtnCancelSupUpdate.Click ' Assuming you add Handles here
            ClearAllCacheValues(False)
            ViewLoader.DynamicView.NavigateToView("managesuppliers", Me)
        End Sub

        ' Clear all cache values (Product and Supplier related)
        Private Sub ClearAllCacheValues(Confirm As Boolean)
            ' --- Product Cache Clear ---
            cacheProductUpdateCompletion = False
            cacheProductID = Nothing
            cacheProductName = Nothing
            cacheProductCode = Nothing
            cacheCategoryID = Nothing
            cacheSubCategoryID = Nothing
            ' cacheSupplierID = Nothing  <- Cleared below to ensure supplier-specific cache is cleared
            cacheBrandID = Nothing
            cacheWarehouseID = Nothing
            ' The user's list had cacheProductCode twice; only clearing once.
            cacheMeasurementUnit = Nothing
            cacheProductVariation = False
            cacheProductDescription = Nothing
            cacheSellingPrice = Nothing
            cacheBuyingPrice = Nothing
            cacheStockUnit = Nothing
            cacheAlertQuantity = Nothing

            ' Clear collection references if they exist
            If cacheSerialNumbers IsNot Nothing Then cacheSerialNumbers.Clear()
            If cacheSerialID IsNot Nothing Then cacheSerialID.Clear()

            ' --- Supplier Cache Clear (Crucial for EditSuppliers context) ---
            ' Using Nothing for consistency and to prevent InvalidCastException if the cache properties are numeric.
            cacheSupplierID = Nothing
            CacheCompanyRepresentative = Nothing
            CacheCompanyName = Nothing
            CachePhone = Nothing
            CacheEmail = Nothing
            CacheCompanyAddress = Nothing
            CacheCompanyCity = Nothing
            CacheCompanyRegion = Nothing
            CacheCompanyCountry = Nothing
            CacheCompanyPostalCode = Nothing
            CacheCompanyTINID = Nothing
        End Sub

        ' Validation event handlers
        Private Sub TxtPhone_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TxtPhone.PreviewTextInput
            If Not e.Text.All(AddressOf Char.IsDigit) Then
                e.Handled = True
                Return
            End If

            ' Limit to 11 digits
            Dim textBox = CType(sender, TextBox)
            If textBox.Text.Length >= 11 AndAlso textBox.SelectionLength = 0 Then
                e.Handled = True
            End If
        End Sub

        Private Sub TxtTINID_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TxtTINID.PreviewTextInput
            If Not e.Text.All(AddressOf Char.IsDigit) Then
                e.Handled = True
                Return
            End If
        End Sub

        Private Sub TxtPostalCode_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TxtPostalCode.PreviewTextInput
            If Not e.Text.All(AddressOf Char.IsDigit) Then
                e.Handled = True
                Return
            End If

            ' Limit to 11 digits (Changed from 11 to 8 as Postal Codes are usually shorter, but keeping 11 as per original)
            Dim textBox = CType(sender, TextBox)
            If textBox.Text.Length >= 11 AndAlso textBox.SelectionLength = 0 Then
                e.Handled = True
            End If
        End Sub

        Private Sub TxtCountry_PreviewTextInput(sender As Object, e As TextCompositionEventArgs) Handles TxtCountry.PreviewTextInput
            If Not e.Text.All(AddressOf Char.IsLetter) AndAlso Not e.Text = " " Then
                e.Handled = True
                Return
            End If
        End Sub
    End Class
End Namespace
