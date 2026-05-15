Imports System.IO
Imports System.Reflection
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DocumentFormat.OpenXml.Vml.Spreadsheet
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Views.Stocks.ItemManager.NewProduct
Imports MaterialDesignThemes.Wpf
Imports Microsoft.Win32
Imports MySql.Data.MySqlClient
Imports OfficeOpenXml.FormulaParsing.Excel.Functions

Namespace DPC.Views.Stocks.ItemManager.ProductManager
    Public Class EditProduct
        Inherits UserControl

        Public ManageProduct As New ManageProducts()

        Private ProductController As New ProductController()
        Private WithEvents AddRowPopoutControl As AddRowPopout
        Private popup As Popup

        Private uploadTimer As New DispatcherTimer()
        Private base64Image As String
        Private isUploadLocked As Boolean = False

        Private Sub EditProduct_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            InitializeMarkupUI()
            base64Image = cacheProductImage
            DisplaySelectedProductImage()
            InitializeSelectedProduct()
            ApplyRolePermissions()
            UpdateSerialCheckboxAvailability()
        End Sub

        Private isSalesUser As Boolean = False

        Private Sub ApplyRolePermissions()
            Dim query As String = "SELECT ur.RoleName FROM employee e JOIN userroles ur ON e.UserRoleID = ur.RoleID WHERE e.Email = @email"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@email", CacheOnLoggedInEmail)
                        Dim roleName As Object = cmd.ExecuteScalar()
                        If roleName IsNot Nothing AndAlso roleName.ToString().ToLower().Contains("sales") Then
                            isSalesUser = True
                        End If
                    End Using
                Catch ex As Exception
                    Console.WriteLine("Error checking role: " & ex.Message)
                End Try
            End Using

            If isSalesUser Then
                Dim LockTextBox = Sub(txt As TextBox)
                                      If txt Is Nothing Then Return
                                      txt.Visibility = Visibility.Collapsed
                                      txt.Text = "0"
                                      Dim parentGrid As Grid = TryCast(txt.Parent, Grid)
                                      If parentGrid IsNot Nothing Then
                                          Dim adminMsg As New TextBlock With {
                                              .Text = "🔒 Admin Access Only",
                                              .Foreground = New SolidColorBrush(Color.FromRgb(210, 54, 54)),
                                              .FontWeight = FontWeights.SemiBold,
                                              .VerticalAlignment = VerticalAlignment.Center,
                                              .Margin = New Thickness(10, 0, 0, 0),
                                              .FontFamily = New FontFamily("Lexend")
                                          }
                                          Grid.SetColumn(adminMsg, Grid.GetColumn(txt))
                                          parentGrid.Children.Add(adminMsg)
                                      End If
                                  End Sub
                LockTextBox(TxtRetailPrice)
            End If
        End Sub

        Private Sub EditProduct_Unloaded(sender As Object, e As RoutedEventArgs) Handles Me.Unloaded
            ClearAllCacheValues(cacheProductUpdateCompletion)

            ProductController.EditProductClearInputFields(TxtProductName, TxtProductCode, TxtRetailPrice, TxtPurchaseOrder,
                TxtDefaultTax, TxtDiscountRate, TxtStockUnits, TxtAlertQuantity, TxtDescription,
                ComboBoxCategory, ComboBoxSubCategory, ComboBoxWarehouse, ComboBoxMeasurementUnit,
                ComboBoxBrand, ComboBoxSupplier, MainContainer)

            ProductController.SerialNumbers.Clear()
            TxtProductVariation.Text = Nothing
            DPC.Components.Forms.AddVariation._savedVariations.Clear()
            DPC.Data.Controllers.ProductController.variationManager.GetAllVariationData().Clear()

            If Not String.IsNullOrWhiteSpace(base64Image) Then
                ResetImageComponents()
            End If
        End Sub

#Region "Initialization"

        Public Sub New()
            InitializeComponent()
            LoadInitialData()
            SetupTimers()
            InitializeUIElements()
            SetupControllerReferences()
            Me.DataContext = ProductViewModel.Instance
        End Sub

        Public Sub InitializeSelectedProduct()
            TxtProductName.Text = cacheProductName
            TxtProductCode.Text = cacheProductCode
            SetSelectedBrand(ComboBoxBrand, cacheBrandID)
            SetSelectedCategory(ComboBoxCategory, cacheCategoryID)
            TxtDescription.Text = cacheMeasurementUnit
            TxtPurchaseOrder.Text = cacheBuyingPrice
            TxtMarkup.Text = FindPercentage(cacheBuyingPrice, cacheSellingPrice)
            SetSelectedMeasureUnit(ComboBoxMeasurementUnit, cacheProductDescription)

            Dim displayStock As Integer = Math.Max(0, Convert.ToInt32(cacheStockUnit))

            If displayStock = 0 Then
                cacheSerialNumbers.Clear()
                CheckBoxSerialNumber.IsChecked = False
            End If

            TxtStockUnits.Text = displayStock.ToString()
            TxtStockUnits.IsEnabled = True  ' explicitly ensure it's editable after clamping

            EditProductProcessStockUnitsEntry(TxtStockUnits, MainContainer)
            SetSelectedWarehouse(ComboBoxWarehouse, cacheWarehouseID)
            TxtAlertQuantity.Text = cacheAlertQuantity
        End Sub

        Private Sub SetupTimers()
            uploadTimer.Interval = TimeSpan.FromMilliseconds(100)
            AddHandler uploadTimer.Tick, AddressOf UploadTimer_Tick
        End Sub

        Private Sub InitializeUIElements()
            If cacheProductVariation = Nothing Or cacheProductVariation = False Then
                Toggle.IsChecked = False
                ProductController.VariationChecker(Toggle, StackPanelVariation, StackPanelWarehouse,
                    StackPanelRetailPrice, StackPanelOrderPrice, StackPanelTaxRate,
                    StackPanelDiscountRate, StackPanelMarkup, BorderStocks, StackPanelAlertQuantity,
                    StackPanelStockUnits, OuterStackPanel)
            ElseIf cacheProductVariation = True Then
                Toggle.IsChecked = True
                ProductController.VariationChecker(Toggle, StackPanelVariation, StackPanelWarehouse,
                    StackPanelRetailPrice, StackPanelOrderPrice, StackPanelTaxRate,
                    StackPanelDiscountRate, StackPanelMarkup, BorderStocks, StackPanelAlertQuantity,
                    StackPanelStockUnits, OuterStackPanel)
            End If

            If cacheSerialNumbers.Count > 0 Then
                CheckBoxSerialNumber.IsChecked = True
                ProductController.SerialNumberChecker(CheckBoxSerialNumber, StackPanelSerialRow,
                    TxtStockUnits, BorderStockUnits)
            Else
                CheckBoxSerialNumber.IsChecked = False
                ProductController.SerialNumberChecker(CheckBoxSerialNumber, StackPanelSerialRow,
                    TxtStockUnits, BorderStockUnits)
            End If

            TxtDefaultTax.Text = "12"
            TxtDiscountRate.Text = "0"
        End Sub

        Public Shared Sub EditProductProcessStockUnitsEntry(txtStockUnits As TextBox, mainContainer As Panel)
            Dim stockUnits As Integer

            If Integer.TryParse(txtStockUnits.Text, stockUnits) Then
                If stockUnits >= 0 Then
                    mainContainer.Children.Clear()

                    If cacheSerialNumbers.Count > 0 Then
                        If stockUnits > 1 AndAlso cacheSerialNumbers.Count = 1 Then
                            MessageBox.Show("Your Stocks: " & stockUnits & vbCrLf & "Your Serial Numbers: " & String.Join(Environment.NewLine, cacheSerialNumbers))
                        Else
                            For i As Integer = 0 To stockUnits - 1
                                ProductController.BtnEditProductAddRow_Click(cacheSerialNumbers(i))
                            Next
                        End If
                    End If

                    txtStockUnits.Text = stockUnits.ToString()
                Else
                    MessageBox.Show("Please enter a number greater than zero.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning)
                End If
            Else
                MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
        End Sub

        Private Sub SetupControllerReferences()
            ProductController.MainContainer = MainContainer
            ProductController.TxtStockUnits = TxtStockUnits
        End Sub

        Private Sub LoadInitialData()
            ProductController.GetBrandsWithSupplier(ComboBoxBrand)
            ProductController.GetProductCategory(ComboBoxCategory)
            ProductController.GetWarehouse(ComboBoxWarehouse)

            Dim existingVariations As List(Of ProductVariation) = ProductController.GetProductVariations()
            If existingVariations IsNot Nothing Then
                ProductController.UpdateProductVariationText(existingVariations, TxtProductVariation)
            End If
        End Sub

#End Region

#Region "Serial Number Gate"

        Private Sub TxtStockUnits_TextChanged(sender As Object, e As TextChangedEventArgs)
            ' Only collapse serial panel if unchecked and stock is 0
            Dim qty As Integer = 0
            Dim isValid As Boolean = Integer.TryParse(TxtStockUnits.Text.Trim(), qty) AndAlso qty > 0

            If Not isValid AndAlso CheckBoxSerialNumber.IsChecked = False Then
                StackPanelSerialRow.Visibility = Visibility.Collapsed
            End If
        End Sub

        Private Sub UpdateSerialCheckboxAvailability()
            ' Always allow the checkbox to be enabled
            CheckBoxSerialNumber.IsEnabled = True

            If TxtSerialHint IsNot Nothing Then
                TxtSerialHint.Visibility = Visibility.Collapsed
            End If
        End Sub

#End Region

#Region "Duplicate Serial Number Check"

        ''' <summary>
        ''' Collects all serial number values from MainContainer rows and checks for duplicates.
        ''' Returns True if duplicates are found (and shows a warning), False if all are unique.
        ''' </summary>
        Private Function HasDuplicateSerialNumbers() As Boolean
            Dim collected As New List(Of String)

            For Each row As StackPanel In MainContainer.Children.OfType(Of StackPanel)()
                Dim grid As Grid = row.Children.OfType(Of Grid)().FirstOrDefault()
                If grid Is Nothing Then Continue For

                Dim border As Border = grid.Children.OfType(Of Border)().FirstOrDefault()
                If border Is Nothing Then Continue For

                Dim textBox As TextBox = TryCast(border.Child, TextBox)
                If textBox Is Nothing OrElse String.IsNullOrWhiteSpace(textBox.Text) Then Continue For

                Dim value As String = textBox.Text.Trim()

                ' Check against already-collected values
                Dim duplicate As String = collected.FirstOrDefault(
                    Function(s) String.Equals(s, value, StringComparison.OrdinalIgnoreCase))

                If duplicate IsNot Nothing Then
                    MessageBox.Show(
                        $"Duplicate serial number detected: ""{value}""" & vbCrLf &
                        "Each serial number must be unique. Please correct it before saving.",
                        "Duplicate Serial Number",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning)

                    ' Highlight the duplicate TextBox so user can find it easily
                    textBox.Focus()
                    textBox.SelectAll()
                    textBox.BorderBrush = New SolidColorBrush(Color.FromRgb(210, 54, 54))  ' red border
                    textBox.Background = New SolidColorBrush(Color.FromRgb(255, 235, 235)) ' light red bg

                    Return True
                End If

                collected.Add(value)
            Next

            Return False
        End Function

#End Region

#Region "Selected Product Details"

        Public Function FindPercentage(buyingPrice As Double, sellingPrice As Double)
            Dim percentage As Double = 0
            percentage = ((sellingPrice - buyingPrice) / buyingPrice) * 100
            Return percentage
        End Function

        Public Sub SetSelectedBrand(comboBox As ComboBox, brandID As Int64)
            For Each item As ComboBoxItem In comboBox.Items
                If item.Tag IsNot Nothing AndAlso Convert.ToInt32(item.Tag) = cacheBrandID Then
                    comboBox.SelectedItem = item
                    Exit For
                End If
            Next
        End Sub

        Public Sub SetSelectedCategory(comboBox As ComboBox, categoryID As Int64)
            For Each item As ComboBoxItem In comboBox.Items
                If item.Tag IsNot Nothing AndAlso Convert.ToInt32(item.Tag) = cacheCategoryID Then
                    comboBox.SelectedItem = item
                    Exit For
                End If
            Next
        End Sub

        Public Sub SetSelectedMeasureUnit(comboBox As ComboBox, MeasureUnit As String)
            For Each item As ComboBoxItem In comboBox.Items
                If item.Content IsNot Nothing AndAlso item.Content.ToString().Equals(MeasureUnit, StringComparison.OrdinalIgnoreCase) Then
                    comboBox.SelectedItem = item
                    Exit For
                End If
            Next
        End Sub

        Public Sub SetSelectedWarehouse(comboBox As ComboBox, WarehouseID As String)
            For Each item As ComboBoxItem In comboBox.Items
                If item.Content IsNot Nothing AndAlso item.Content.ToString().Equals(WarehouseID, StringComparison.OrdinalIgnoreCase) Then
                    comboBox.SelectedItem = item
                    Exit For
                End If
            Next
        End Sub

#End Region

#Region "Event Handlers"

        Public Sub IntegerOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            ProductController.IntegerOnlyTextInputHandler(sender, e)
        End Sub
        Public Sub IntegerOnlyPasteHandler(sender As Object, e As DataObjectPastingEventArgs)
            ProductController.IntegerOnlyPasteHandler(sender, e)
        End Sub
        Public Sub DecimalOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            ProductController.DecimalOnlyTextInputHandler(sender, e)
        End Sub
        Public Sub DecimalOnlyPasteHandler(sender As Object, e As DataObjectPastingEventArgs)
            ProductController.DecimalOnlyPasteHandler(sender, e)
        End Sub

        Private Sub Toggle_Click(sender As Object, e As RoutedEventArgs)
            ProductController.VariationChecker(Toggle, StackPanelVariation, StackPanelWarehouse,
                StackPanelRetailPrice, StackPanelOrderPrice, StackPanelTaxRate,
                StackPanelDiscountRate, StackPanelMarkup, BorderStocks, StackPanelAlertQuantity,
                StackPanelStockUnits, OuterStackPanel)
        End Sub

        Private Sub IncludeSerial_Click(sender As Object, e As RoutedEventArgs)
            If CheckBoxSerialNumber.IsChecked Then
                ' Auto-increment stock units to 1 if it's 0 or empty
                Dim qty As Integer = 0
                If Not Integer.TryParse(TxtStockUnits.Text.Trim(), qty) OrElse qty <= 0 Then
                    TxtStockUnits.Text = "1"
                End If
            End If

            ProductController.ProcessStockUnitsEntry(TxtStockUnits, MainContainer)
            ProductController.SerialNumberChecker(CheckBoxSerialNumber, StackPanelSerialRow,
        TxtStockUnits, BorderStockUnits)
        End Sub

        Private Sub BtnExit_Click(sender As Object, e As RoutedEventArgs)
            ClearAllCacheValues(False)

            ProductController.EditProductClearInputFields(TxtProductName, TxtProductCode, TxtRetailPrice, TxtPurchaseOrder,
                TxtDefaultTax, TxtDiscountRate, TxtStockUnits, TxtAlertQuantity, TxtDescription,
                ComboBoxCategory, ComboBoxSubCategory, ComboBoxWarehouse, ComboBoxMeasurementUnit,
                ComboBoxBrand, ComboBoxSupplier, MainContainer)

            ProductController.SerialNumbers.Clear()
            TxtProductVariation.Text = Nothing
            DPC.Components.Forms.AddVariation._savedVariations.Clear()
            DPC.Data.Controllers.ProductController.variationManager.GetAllVariationData().Clear()

            If Not String.IsNullOrWhiteSpace(base64Image) Then
                ResetImageComponents()
            End If

            ViewLoader.DynamicView.NavigateToView("manageproducts", Me)
        End Sub

        Private Sub BtnEditProduct_Click(sender As Object, e As RoutedEventArgs)

#Region "FOR NO VARIATION PRODUCTS"
            cacheProductName = TxtProductName.Text
            cacheProductCode = TxtProductCode.Text
            cacheCategoryID = If(ComboBoxCategory.SelectedItem IsNot Nothing, Convert.ToInt64(CType(ComboBoxCategory.SelectedItem, ComboBoxItem).Tag), 0)
            cacheSubCategoryID = If(ComboBoxSubCategory.SelectedItem IsNot Nothing, Convert.ToInt64(CType(ComboBoxSubCategory.SelectedItem, ComboBoxItem).Tag), 0)
            cacheSupplierID = If(ComboBoxSupplier.SelectedItem IsNot Nothing, Convert.ToInt64(CType(ComboBoxSupplier.SelectedItem, ComboBoxItem).Tag), 0)
            cacheBrandID = If(ComboBoxBrand.SelectedItem IsNot Nothing, Convert.ToInt64(CType(ComboBoxBrand.SelectedItem, ComboBoxItem).Tag), 0)
            cacheWarehouseID = If(ComboBoxWarehouse.SelectedItem IsNot Nothing, Convert.ToInt32(CType(ComboBoxWarehouse.SelectedItem, ComboBoxItem).Tag), 0)
            cacheProductImage = base64Image
            cacheMeasurementUnit = If(ComboBoxMeasurementUnit.SelectedItem IsNot Nothing, CType(ComboBoxMeasurementUnit.SelectedItem, ComboBoxItem).Content.ToString(), String.Empty)
            cacheProductVariation = Toggle.IsChecked
            cacheProductDescription = TxtDescription.Text

            Dim retailPrice As Decimal
            If Not Decimal.TryParse(TxtRetailPrice.Text, retailPrice) Then
                MessageBox.Show("Please enter a valid retail price.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            cacheSellingPrice = retailPrice

            Dim purchaseOrder As Decimal
            If Not Decimal.TryParse(TxtPurchaseOrder.Text, purchaseOrder) Then
                MessageBox.Show("Please enter a valid purchase order price.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            cacheBuyingPrice = purchaseOrder

            Dim stockUnits As Integer
            If Not Integer.TryParse(TxtStockUnits.Text, stockUnits) OrElse stockUnits < 0 Then
                MessageBox.Show("Please enter a valid stock units.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            cacheStockUnit = stockUnits

            Dim alertQuantity As Integer
            If Not Integer.TryParse(TxtAlertQuantity.Text, alertQuantity) OrElse alertQuantity < 0 Then
                MessageBox.Show("Please enter a valid alert quantity.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            cacheAlertQuantity = alertQuantity

            If CheckBoxSerialNumber.IsChecked Then

                ' ── DUPLICATE CHECK — block save if any serial number appears more than once ──
                If HasDuplicateSerialNumbers() Then Return

                ' Collect serial numbers into cache
                cacheSerialNumbers.Clear()
                For Each row As StackPanel In MainContainer.Children.OfType(Of StackPanel)()
                    Dim grid As Grid = row.Children.OfType(Of Grid)().FirstOrDefault()
                    If grid IsNot Nothing Then
                        Dim border As Border = grid.Children.OfType(Of Border)().FirstOrDefault()
                        If border IsNot Nothing Then
                            Dim textBox As TextBox = TryCast(border.Child, TextBox)
                            If textBox IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(textBox.Text) Then
                                cacheSerialNumbers.Add(textBox.Text)
                            End If
                        End If
                    End If
                Next

                If cacheSerialNumbers.Count = 0 Then
                    MessageBox.Show("Please enter at least one serial number.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If
            End If

#End Region

            ProductController.UpdateSelectedProduct(Toggle, CheckBoxSerialNumber,
                cacheProductName, cacheProductCode, cacheCategoryID, cacheSubCategoryID,
                cacheWarehouseID, cacheBrandID, cacheSupplierID, TxtRetailPrice,
                TxtPurchaseOrder, TxtDefaultTax.Text, TxtDiscountRate.Text, cacheStockUnit,
                cacheAlertQuantity, cacheMeasurementUnit, cacheProductDescription,
                cacheSerialNumbers, cacheProductImage)

            If cacheProductUpdateCompletion Then
                ClearAllCacheValues(cacheProductUpdateCompletion)
                ViewLoader.DynamicView.NavigateToView("manageproducts", Me)

                ProductController.EditProductClearInputFields(TxtProductName, TxtProductCode, TxtRetailPrice, TxtPurchaseOrder,
                    TxtDefaultTax, TxtDiscountRate, TxtStockUnits, TxtAlertQuantity, TxtDescription,
                    ComboBoxCategory, ComboBoxSubCategory, ComboBoxWarehouse, ComboBoxMeasurementUnit,
                    ComboBoxBrand, ComboBoxSupplier, MainContainer)

                ProductController.SerialNumbers.Clear()
                TxtProductVariation.Text = Nothing
                DPC.Components.Forms.AddVariation._savedVariations.Clear()
                DPC.Data.Controllers.ProductController.variationManager.GetAllVariationData().Clear()

                If Not String.IsNullOrWhiteSpace(base64Image) Then
                    ResetImageComponents()
                End If
            End If
        End Sub

        Private Sub ClearAllCacheValues(Confirm As Boolean)
            cacheProductUpdateCompletion = False
            cacheProductID = Nothing
            cacheProductName = Nothing
            cacheProductCode = Nothing
            cacheCategoryID = Nothing
            cacheSubCategoryID = Nothing
            cacheSupplierID = Nothing
            cacheBrandID = Nothing
            cacheWarehouseID = Nothing
            cacheProductCode = Nothing
            cacheMeasurementUnit = Nothing
            cacheProductVariation = False
            cacheProductDescription = Nothing
            cacheSellingPrice = Nothing
            cacheBuyingPrice = Nothing
            cacheStockUnit = Nothing
            cacheAlertQuantity = Nothing
            cacheSerialNumbers.Clear()
            cacheSerialID.Clear()
        End Sub

        Private Sub CategoryComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBoxCategory.SelectionChanged
            Dim selectedCategory As String = TryCast(ComboBoxCategory.SelectedItem, ComboBoxItem)?.Content?.ToString()
            If Not String.IsNullOrEmpty(selectedCategory) Then
                ProductController.GetProductSubcategory(selectedCategory, ComboBoxSubCategory, SubCategoryLabel, StackPanelSubCategory)
            Else
                ComboBoxSubCategory.Items.Clear()
            End If
        End Sub

        Private Sub ComboBoxBrand_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBoxBrand.SelectionChanged
            Dim selectedBrandItem As ComboBoxItem = TryCast(ComboBoxBrand.SelectedItem, ComboBoxItem)
            If selectedBrandItem IsNot Nothing AndAlso selectedBrandItem.Tag IsNot Nothing Then
                Dim brandID As Integer = Convert.ToInt32(selectedBrandItem.Tag)
                ProductController.GetSuppliersByBrand(brandID, ComboBoxSupplier)
            Else
                ComboBoxSupplier.Items.Clear()
            End If
        End Sub

        Private Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs)
            ProductController.BtnAddRow_Click(Nothing, Nothing)
        End Sub

        Private Sub BtnRemoveRow_Click(sender As Object, e As RoutedEventArgs)
            ProductController.BtnRemoveRow_Click(Nothing, Nothing)
        End Sub

        Private Sub OpenAddVariation(sender As Object, e As RoutedEventArgs)
            Dim openAddVariation As New DPC.Components.Forms.AddVariation()
            AddHandler openAddVariation.close, AddressOf AddVariation_Closed
            Dim parentWindow As Window = Window.GetWindow(Me)
            PopupHelper.OpenPopupWithControl(sender, openAddVariation, "windowcenter", -100, 0, False, parentWindow)
        End Sub

        Private Sub AddVariation_Closed(sender As Object, e As RoutedEventArgs)
            Dim variations As List(Of ProductVariation) = ProductController.GetProductVariations()
            If variations IsNot Nothing Then
                ProductController.UpdateProductVariationText(variations, TxtProductVariation)
            End If
        End Sub

#End Region

#Region "Image Handling"

        Private Sub DisplaySelectedProductImage()
            Try
                Dim tempImagePath As String = Path.Combine(Path.GetTempPath(), "decoded_image.png")
                If File.Exists(tempImagePath) Then
                    GC.Collect()
                    GC.WaitForPendingFinalizers()
                    File.Delete(tempImagePath)
                End If

                Base64Utility.DecodeBase64ToFile(base64Image, tempImagePath)

                Dim imageSource As New BitmapImage()
                Using stream As New FileStream(tempImagePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    imageSource.BeginInit()
                    imageSource.CacheOption = BitmapCacheOption.OnLoad
                    imageSource.StreamSource = stream
                    imageSource.EndInit()
                End Using
                imageSource.Freeze()

                ImageInfoPanel.Visibility = Visibility.Collapsed
                ImageDisplayPanel.Visibility = Visibility.Visible
                UploadedImage.Source = imageSource
                BtnRemoveImage.Visibility = Visibility.Visible
                BtnBrowse.IsEnabled = False
                DropBorder.AllowDrop = False
                isUploadLocked = True
            Catch ex As Exception
                MessageBox.Show("Error decoding image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub BtnBrowse_Click(sender As Object, e As RoutedEventArgs)
            If isUploadLocked Then Return
            Dim openFileDialog As New OpenFileDialog With {
                .Filter = "Image Files|*.jpg;*.jpeg;*.png",
                .Title = "Select an Image"
            }
            If openFileDialog.ShowDialog() = True Then
                Dim filePath As String = openFileDialog.FileName
                If ProductController.ValidateImageFile(filePath) Then
                    StartFileUpload(filePath)
                End If
            End If
        End Sub

        Private Sub Border_DragEnter(sender As Object, e As DragEventArgs)
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                e.Effects = DragDropEffects.Copy
            End If
        End Sub

        Private Sub Border_Drop(sender As Object, e As DragEventArgs)
            If isUploadLocked Then Return
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
                Dim filePath As String = files(0)
                If ProductController.ValidateImageFile(filePath) Then
                    StartFileUpload(filePath)
                End If
            End If
        End Sub

        Private Sub StartFileUpload(filePath As String)
            UploadProgressBar.Value = 0
            UploadStatus.Text = "Uploading..."
            Dim fileInfo As New FileInfo(filePath)
            ImgName.Text = Path.GetFileName(filePath)
            ImgSize.Text = Base64Utility.GetReadableFileSize(fileInfo.Length)
            Try
                base64Image = Base64Utility.EncodeFileToBase64(filePath)
            Catch ex As Exception
                MessageBox.Show("Error encoding image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End Try
            ImageInfoPanel.Visibility = Visibility.Visible
            BtnBrowse.IsEnabled = False
            DropBorder.AllowDrop = False
            isUploadLocked = True
            ConfigureUploadTimer()
        End Sub

        Private Sub ConfigureUploadTimer()
            If uploadTimer.IsEnabled Then uploadTimer.Stop()
            uploadTimer.Start()
        End Sub

        Private Sub UploadTimer_Tick(sender As Object, e As EventArgs)
            If UploadProgressBar.Value < 100 Then
                UploadProgressBar.Value += 2
            Else
                uploadTimer.Stop()
                UploadStatus.Text = "Upload Complete"
                ImageInfoPanel.Visibility = Visibility.Collapsed
                ImageDisplayPanel.Visibility = Visibility.Visible
                DisplayUploadedImage()
                BtnRemoveImage.Visibility = Visibility.Visible
            End If
        End Sub

        Private Sub DisplayUploadedImage()
            Try
                Dim tempImagePath As String = Path.Combine(Path.GetTempPath(), "decoded_image.png")
                If File.Exists(tempImagePath) Then
                    GC.Collect()
                    GC.WaitForPendingFinalizers()
                    File.Delete(tempImagePath)
                End If
                Base64Utility.DecodeBase64ToFile(base64Image, tempImagePath)
                Dim imageSource As New BitmapImage()
                Using stream As New FileStream(tempImagePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    imageSource.BeginInit()
                    imageSource.CacheOption = BitmapCacheOption.OnLoad
                    imageSource.StreamSource = stream
                    imageSource.EndInit()
                End Using
                imageSource.Freeze()
                UploadedImage.Source = imageSource
            Catch ex As Exception
                MessageBox.Show("Error decoding image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub RemoveImage(sender As Object, e As RoutedEventArgs)
            uploadTimer.Stop()
            ResetImageComponents()
        End Sub

        Private Sub ResetImageComponents()
            UploadProgressBar.Value = 0
            UploadStatus.Text = ""
            ImgName.Text = ""
            ImgSize.Text = ""
            ImageInfoPanel.Visibility = Visibility.Collapsed
            ImageDisplayPanel.Visibility = Visibility.Collapsed
            BtnBrowse.IsEnabled = True
            DropBorder.AllowDrop = True
            isUploadLocked = False
            BtnRemoveImage.Visibility = Visibility.Collapsed
            UploadedImage.Source = Nothing
            base64Image = String.Empty
            Dim tempImagePath As String = Path.Combine(Path.GetTempPath(), "decoded_image.png")
            If File.Exists(tempImagePath) Then
                Try
                    GC.Collect()
                    GC.WaitForPendingFinalizers()
                    File.Delete(tempImagePath)
                Catch ex As Exception
                End Try
            End If
        End Sub

#End Region

        Public Sub LoadProductVariations()
            Dim variations As List(Of ProductVariation) = ProductController.GetProductVariations()
            ProductController.UpdateProductVariationText(variations, TxtProductVariation)
        End Sub

        Private Sub BtnRemoveImage_Click(sender As Object, e As RoutedEventArgs)
            ProductViewModel.Instance.ProductImage = Nothing
            ProductViewModel.Instance.ImagePath = Nothing
        End Sub

        Private Sub LoadImageFromFile(filePath As String)
            Try
                Dim fileInfo As New FileInfo(filePath)
                Dim sizeInMB As Double = fileInfo.Length / (1024 * 1024)
                If sizeInMB > 2 Then
                    MessageBox.Show("Image size exceeds 2MB limit. Please select a smaller image.", "File Too Large", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If
                Dim bitmap As New BitmapImage()
                bitmap.BeginInit()
                bitmap.CacheOption = BitmapCacheOption.OnLoad
                bitmap.UriSource = New Uri(filePath)
                bitmap.EndInit()
                bitmap.Freeze()
                ProductViewModel.Instance.ProductImage = bitmap
                ProductViewModel.Instance.ImagePath = filePath
            Catch ex As Exception
                MessageBox.Show("Error loading image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

#Region "Markup and Price Calculation"

        Private Sub CalculateSellingPrice()
            Try
                If TxtPurchaseOrder Is Nothing OrElse TxtMarkup Is Nothing OrElse
                   TxtRetailPrice Is Nothing OrElse RadBtnPercentage Is Nothing Then Return

                Dim buyingPrice As Decimal
                If String.IsNullOrWhiteSpace(TxtPurchaseOrder.Text) OrElse
                   Not Decimal.TryParse(TxtPurchaseOrder.Text, buyingPrice) OrElse buyingPrice <= 0 Then
                    TxtRetailPrice.Text = "0.00"
                    Return
                End If

                Dim markupValue As Decimal
                If String.IsNullOrWhiteSpace(TxtMarkup.Text) OrElse
                   Not Decimal.TryParse(TxtMarkup.Text, markupValue) OrElse markupValue < 0 Then
                    TxtRetailPrice.Text = buyingPrice.ToString("N2")
                    Return
                End If

                Dim sellingPrice As Decimal
                If RadBtnPercentage.IsChecked = True Then
                    sellingPrice = buyingPrice + (buyingPrice * markupValue / 100)
                Else
                    sellingPrice = buyingPrice + markupValue
                End If

                TxtRetailPrice.Text = sellingPrice.ToString("N2")
            Catch ex As Exception
                MessageBox.Show("Error calculating selling price: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub TxtMarkup_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TxtMarkup.TextChanged
            CalculateSellingPrice()
        End Sub

        Private Sub TxtPurchaseOrder_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TxtPurchaseOrder.TextChanged
            CalculateSellingPrice()
        End Sub

        Private Sub RadioButton_Checked(sender As Object, e As RoutedEventArgs) Handles RadBtnPercentage.Checked, RadBtnFlat.Checked
            UpdateMarkupLabelsIfReady()
            CalculateSellingPrice()
        End Sub

        Private Sub UpdateMarkupLabelsIfReady()
            If TxtMarkupLabel Is Nothing OrElse RadBtnPercentage Is Nothing OrElse MarkupPrefix Is Nothing Then Return
            If RadBtnPercentage.IsChecked = True Then
                TxtMarkupLabel.Text = "Enter Percentage:"
                MarkupPrefix.Kind = PackIconKind.PercentOutline
            Else
                TxtMarkupLabel.Text = "Enter Amount:"
                MarkupPrefix.Kind = PackIconKind.CurrencyPhp
            End If
        End Sub

        Private Sub InitializeMarkupUI()
            If TxtMarkupLabel Is Nothing OrElse RadBtnPercentage Is Nothing OrElse MarkupPrefix Is Nothing Then Return
            RadBtnPercentage.IsChecked = True
            UpdateMarkupLabelsIfReady()
            If Not String.IsNullOrWhiteSpace(TxtPurchaseOrder?.Text) Then
                CalculateSellingPrice()
            End If
        End Sub

#End Region

    End Class

    Public Class InverseBooleanToVisibilityConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.Convert
            Dim boolValue As Boolean = CBool(value)
            Return If(boolValue, Visibility.Collapsed, Visibility.Visible)
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
            Dim visibility As Visibility = DirectCast(value, Visibility)
            Return visibility <> Visibility.Visible
        End Function
    End Class

End Namespace

