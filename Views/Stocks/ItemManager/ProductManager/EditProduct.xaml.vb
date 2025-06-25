
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
            ' Now initialize the markup UI after the control has been loaded
            InitializeMarkupUI()
            base64Image = cacheProductImage
            DisplaySelectedProductImage()

            'Store all data in cache in their respective displays and initializes other details related to the selected product
            InitializeSelectedProduct()
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
            TxtStockUnits.Text = Convert.ToInt16(cacheStockUnit)
            EditProductProcessStockUnitsEntry(TxtStockUnits, MainContainer)
            SetSelectedWarehouse(ComboBoxWarehouse, cacheWarehouseID)
            TxtAlertQuantity.Text = cacheAlertQuantity
        End Sub

        Private Sub SetupTimers()
            uploadTimer.Interval = TimeSpan.FromMilliseconds(100)
            AddHandler uploadTimer.Tick, AddressOf UploadTimer_Tick
        End Sub

        Private Sub OpenProductVariationDetails()
            ' Create and show the ProductvariationDetails UserControl
            Dim productVariationDetails = New ProductVariationDetails()
        End Sub

        Private Sub InitializeUIElements()
            'Initialize UI state
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

            ' Set default values
            TxtDefaultTax.Text = "12"
            TxtDiscountRate.Text = "0"
            ' Move this AFTER the controls are initialized
            ' Initialize markup UI - But ONLY after the form has loaded
            ' We'll handle this in the Loaded event instead
            ' InitializeMarkupUI()
        End Sub

        Public Shared Sub EditProductProcessStockUnitsEntry(txtStockUnits As TextBox, mainContainer As Panel)
            Dim stockUnits As Integer

            ' Validate if input is a valid number and greater than zero
            If Integer.TryParse(txtStockUnits.Text, stockUnits) Then
                If stockUnits > 0 Then
                    ' Clear previous rows
                    mainContainer.Children.Clear()

                    ' Call BtnAddRow_Click the specified number of times
                    If cacheSerialNumbers.Count > 0 Then
                        If stockUnits > 1 AndAlso cacheSerialNumbers.Count = 1 Then
                            MessageBox.Show("Your Stocks: " & stockUnits & vbCrLf & "Your Serial Numbers: " & String.Join(Environment.NewLine, cacheSerialNumbers))
                        Else
                            For i As Integer = 0 To stockUnits - 1
                                ProductController.BtnEditProductAddRow_Click(cacheSerialNumbers(i))
                            Next
                        End If
                    End If

                    ' Ensure the textbox retains the correct value
                    txtStockUnits.Text = stockUnits.ToString()
                Else
                    MessageBox.Show("Please enter a number greater than zero.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning)
                End If
            Else
                MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
        End Sub


        Private Sub SetupControllerReferences()
            ' Set controller references
            ProductController.MainContainer = MainContainer
            ProductController.TxtStockUnits = TxtStockUnits
        End Sub

        Private Sub LoadInitialData()
            ' Load dropdown data
            ProductController.GetBrandsWithSupplier(ComboBoxBrand)
            ProductController.GetProductCategory(ComboBoxCategory)
            ProductController.GetWarehouse(ComboBoxWarehouse)

            ' Load variations if any
            Dim existingVariations As List(Of ProductVariation) = ProductController.GetProductVariations()
            If existingVariations IsNot Nothing Then
                ProductController.UpdateProductVariationText(existingVariations, TxtProductVariation)
            End If
        End Sub
#End Region

#Region "Selected Product Details"

        'Computation to find the percentage when getting the product
        Public Function FindPercentage(buyingPrice As Double, sellingPrice As Double)
            Dim percentage As Double = 0
            percentage = ((sellingPrice - buyingPrice) / buyingPrice) * 100
            Return percentage
        End Function

        'Selects brand on the combobox based on the product's brandID
        Public Sub SetSelectedBrand(comboBox As ComboBox, brandID As Int64)
            For Each item As ComboBoxItem In comboBox.Items
                If item.Tag IsNot Nothing AndAlso Convert.ToInt32(item.Tag) = cacheBrandID Then
                    comboBox.SelectedItem = item
                    Exit For
                End If
            Next
        End Sub

        'Selects Category on the combobox based on the product's CategoryID
        Public Sub SetSelectedCategory(comboBox As ComboBox, categoryID As Int64)
            For Each item As ComboBoxItem In comboBox.Items
                If item.Tag IsNot Nothing AndAlso Convert.ToInt32(item.Tag) = cacheCategoryID Then
                    comboBox.SelectedItem = item
                    Exit For
                End If
            Next
        End Sub

        'Selects SubCategory on the combobox (if available) based on the product's SubCategoryID
        Public Sub SetSelectedMeasureUnit(comboBox As ComboBox, MeasureUnit As String)
            For Each item As ComboBoxItem In comboBox.Items
                If item.Content IsNot Nothing AndAlso item.Content.ToString().Equals(MeasureUnit, StringComparison.OrdinalIgnoreCase) Then
                    comboBox.SelectedItem = item
                    Exit For
                End If
            Next
        End Sub

        'Selects Warehouse on the combobox based on the product's WarehouseID
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
        Private Sub Toggle_Click(sender As Object, e As RoutedEventArgs)
            ProductController.VariationChecker(Toggle, StackPanelVariation, StackPanelWarehouse,
                StackPanelRetailPrice, StackPanelOrderPrice, StackPanelTaxRate,
                StackPanelDiscountRate, StackPanelMarkup, BorderStocks, StackPanelAlertQuantity,
                StackPanelStockUnits, OuterStackPanel)
        End Sub

        Private Sub IncludeSerial_Click(sender As Object, e As RoutedEventArgs)
            ProductController.ProcessStockUnitsEntry(TxtStockUnits, MainContainer)
            ProductController.SerialNumberChecker(CheckBoxSerialNumber, StackPanelSerialRow,
            TxtStockUnits, BorderStockUnits)
        End Sub

        Private Sub BtnEditProduct_Click(sender As Object, e As RoutedEventArgs)

#Region "FOR NO VARIATION PRODUCTS"
            'Assign new values on caches
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

            'Filters empty inputs
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
                'clears cache for serial number then assign the new serial numbers on the textboxes
                cacheSerialNumbers.Clear()
                For Each row As StackPanel In MainContainer.Children.OfType(Of StackPanel)()
                    Dim grid As Grid = row.Children.OfType(Of Grid)().FirstOrDefault()
                    If grid IsNot Nothing Then
                        Dim border As Border = grid.Children.OfType(Of Border)().FirstOrDefault()
                        If border IsNot Nothing Then
                            Dim textBox As TextBox = TryCast(border.Child, TextBox)
                            If textBox IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(textBox.Text) Then
                                ' Update the cache with the current product details
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


            ' Call the controller to update the product
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

        'Private Sub TxtStockUnits_KeyDown(sender As Object, e As KeyEventArgs)
        '    ' Check if Enter key is pressed
        '    If e.Key = Key.Enter Then
        '        ProductController.ProcessStockUnitsEntry(TxtStockUnits, MainContainer)
        '        ' Prevent further propagation of the event
        '        e.Handled = True
        '    End If
        'End Sub

        Private Sub OpenAddVariation(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim openAddVariation As New DPC.Components.Forms.AddVariation()

            ' Subscribe to the ClosePopup method directly
            AddHandler openAddVariation.close, AddressOf AddVariation_Closed

            ' Get the parent Window of this UserControl
            Dim parentWindow As Window = Window.GetWindow(Me)

            ' Open the popup with the parent window instead of 'Me'
            PopupHelper.OpenPopupWithControl(sender, openAddVariation, "windowcenter", -100, 0, False, parentWindow)
        End Sub

        Private Sub AddVariation_Closed(sender As Object, e As RoutedEventArgs)
            ' Reload variations after the popup is closed
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

                ' Clean up previous image file
                If File.Exists(tempImagePath) Then
                    GC.Collect()
                    GC.WaitForPendingFinalizers()
                    File.Delete(tempImagePath)
                End If

                ' Decode and save new image
                Base64Utility.DecodeBase64ToFile(base64Image, tempImagePath)

                ' Load image safely
                Dim imageSource As New BitmapImage()
                Using stream As New FileStream(tempImagePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    imageSource.BeginInit()
                    imageSource.CacheOption = BitmapCacheOption.OnLoad
                    imageSource.StreamSource = stream
                    imageSource.EndInit()
                End Using
                imageSource.Freeze() ' Allow image to be accessed in different threads

                ' Hide Image Info Panel and Show Image Display Panel
                ImageInfoPanel.Visibility = Visibility.Collapsed
                ImageDisplayPanel.Visibility = Visibility.Visible

                ' Set the image source
                UploadedImage.Source = imageSource

                ' Make the Remove Image button visible
                BtnRemoveImage.Visibility = Visibility.Visible

                ' Disable browse button and drag-drop functionality
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
            ' Check if the dragged data is a file
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
            ' Reset upload progress
            UploadProgressBar.Value = 0
            UploadStatus.Text = "Uploading..."

            ' Update file info
            Dim fileInfo As New FileInfo(filePath)
            Dim fileSizeText As String = Base64Utility.GetReadableFileSize(fileInfo.Length)

            ImgName.Text = Path.GetFileName(filePath)
            ImgSize.Text = fileSizeText

            ' Convert image to Base64 using Base64Utility
            Try
                base64Image = Base64Utility.EncodeFileToBase64(filePath)
            Catch ex As Exception
                MessageBox.Show("Error encoding image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End Try

            ' Show the panel with image info
            ImageInfoPanel.Visibility = Visibility.Visible

            ' Disable browse button and drag-drop functionality
            BtnBrowse.IsEnabled = False
            DropBorder.AllowDrop = False
            isUploadLocked = True

            ' Configure and start the timer
            ConfigureUploadTimer()
        End Sub

        Private Sub ConfigureUploadTimer()
            If uploadTimer.IsEnabled Then
                uploadTimer.Stop()
            End If
            uploadTimer.Start()
        End Sub

        Private Sub UploadTimer_Tick(sender As Object, e As EventArgs)
            If UploadProgressBar.Value < 100 Then
                UploadProgressBar.Value += 2 ' Increase by 2% every tick
            Else
                uploadTimer.Stop()
                UploadStatus.Text = "Upload Complete"

                ' Hide Image Info Panel and Show Image Display Panel
                ImageInfoPanel.Visibility = Visibility.Collapsed
                ImageDisplayPanel.Visibility = Visibility.Visible

                DisplayUploadedImage()

                ' Make the Remove Image button visible
                BtnRemoveImage.Visibility = Visibility.Visible
            End If
        End Sub

        Private Sub DisplayUploadedImage()
            Try
                Dim tempImagePath As String = Path.Combine(Path.GetTempPath(), "decoded_image.png")

                ' Clean up previous image file
                If File.Exists(tempImagePath) Then
                    GC.Collect()
                    GC.WaitForPendingFinalizers()
                    File.Delete(tempImagePath)
                End If

                ' Decode and save new image
                Base64Utility.DecodeBase64ToFile(base64Image, tempImagePath)

                ' Load image safely
                Dim imageSource As New BitmapImage()
                Using stream As New FileStream(tempImagePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    imageSource.BeginInit()
                    imageSource.CacheOption = BitmapCacheOption.OnLoad
                    imageSource.StreamSource = stream
                    imageSource.EndInit()
                End Using
                imageSource.Freeze() ' Allow image to be accessed in different threads

                ' Set the image source
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
            ' Reset UI elements
            UploadProgressBar.Value = 0
            UploadStatus.Text = ""
            ImgName.Text = ""
            ImgSize.Text = ""

            ' Hide image panels
            ImageInfoPanel.Visibility = Visibility.Collapsed
            ImageDisplayPanel.Visibility = Visibility.Collapsed

            ' Re-enable browse button and drag-drop functionality
            BtnBrowse.IsEnabled = True
            DropBorder.AllowDrop = True
            isUploadLocked = False

            ' Hide the Remove Image button
            BtnRemoveImage.Visibility = Visibility.Collapsed

            ' Reset uploaded image source
            UploadedImage.Source = Nothing

            ' Reset Base64 string
            base64Image = String.Empty

            ' Clean up temp file
            Dim tempImagePath As String = Path.Combine(Path.GetTempPath(), "decoded_image.png")
            If File.Exists(tempImagePath) Then
                Try
                    GC.Collect()
                    GC.WaitForPendingFinalizers()
                    File.Delete(tempImagePath)
                Catch ex As Exception
                    ' Ignore deletion errors
                End Try
            End If
        End Sub
#End Region

        Public Sub LoadProductVariations()
            Dim variations As List(Of ProductVariation) = ProductController.GetProductVariations()
            ProductController.UpdateProductVariationText(variations, TxtProductVariation)
        End Sub

        ' Remove image button handler
        Private Sub BtnRemoveImage_Click(sender As Object, e As RoutedEventArgs)
            ' Clear the image from the ViewModel
            ProductViewModel.Instance.ProductImage = Nothing
            ProductViewModel.Instance.ImagePath = Nothing
        End Sub

        ' Common method to load image from file
        Private Sub LoadImageFromFile(filePath As String)
            Try
                ' Check file size (2MB limit)
                Dim fileInfo As New FileInfo(filePath)
                Dim sizeInMB As Double = fileInfo.Length / (1024 * 1024)

                If sizeInMB > 2 Then
                    MessageBox.Show("Image size exceeds 2MB limit. Please select a smaller image.", "File Too Large", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                ' Create BitmapImage
                Dim bitmap As New BitmapImage()
                bitmap.BeginInit()
                bitmap.CacheOption = BitmapCacheOption.OnLoad ' Load the image in memory
                bitmap.UriSource = New Uri(filePath)
                bitmap.EndInit()
                bitmap.Freeze() ' Make it thread safe

                ' Store in ViewModel
                ProductViewModel.Instance.ProductImage = bitmap
                ProductViewModel.Instance.ImagePath = filePath

            Catch ex As Exception
                MessageBox.Show("Error loading image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

#Region "Markup and Price Calculation"
        Private Sub CalculateSellingPrice()
            Try
                ' Check if controls exist
                If TxtPurchaseOrder Is Nothing OrElse TxtMarkup Is Nothing OrElse TxtRetailPrice Is Nothing OrElse
               RadBtnPercentage Is Nothing Then
                    Return
                End If

                ' Get the buying price
                Dim buyingPrice As Decimal
                If String.IsNullOrWhiteSpace(TxtPurchaseOrder.Text) OrElse
               Not Decimal.TryParse(TxtPurchaseOrder.Text, buyingPrice) OrElse
               buyingPrice <= 0 Then
                    ' Invalid or zero/negative buying price
                    TxtRetailPrice.Text = "0.00"
                    Return
                End If

                ' Get the markup value
                Dim markupValue As Decimal
                If String.IsNullOrWhiteSpace(TxtMarkup.Text) OrElse
               Not Decimal.TryParse(TxtMarkup.Text, markupValue) OrElse
               markupValue < 0 Then
                    ' Invalid markup value, just set selling price equal to buying price
                    TxtRetailPrice.Text = buyingPrice.ToString("N2")
                    Return
                End If

                ' Calculate selling price based on markup type
                Dim sellingPrice As Decimal

                If RadBtnPercentage.IsChecked = True Then
                    ' Percentage markup
                    sellingPrice = buyingPrice + (buyingPrice * markupValue / 100)
                Else
                    ' Flat markup
                    sellingPrice = buyingPrice + markupValue
                End If

                ' Update the retail price text box
                TxtRetailPrice.Text = sellingPrice.ToString("N2")
            Catch ex As Exception
                MessageBox.Show("Error calculating selling price: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
        ' Event handler for the markup text box
        Private Sub TxtMarkup_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TxtMarkup.TextChanged
            CalculateSellingPrice()
        End Sub

        ' Event handler for the buying price text box
        Private Sub TxtPurchaseOrder_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TxtPurchaseOrder.TextChanged
            CalculateSellingPrice()
        End Sub

        ' Event handler for the radio buttons
        Private Sub RadioButton_Checked(sender As Object, e As RoutedEventArgs) Handles RadBtnPercentage.Checked, RadBtnFlat.Checked
            UpdateMarkupLabelsIfReady()
            CalculateSellingPrice()
        End Sub

        ' Update the markup label and symbol based on selected markup type
        Private Sub UpdateMarkupLabels()
            UpdateMarkupLabelsIfReady()
        End Sub

        Private Sub UpdateMarkupLabelsIfReady()
            ' Check if all required elements are available before proceeding
            If TxtMarkupLabel Is Nothing OrElse RadBtnPercentage Is Nothing OrElse
           MarkupPrefix Is Nothing Then
                ' Exit if any required element is not yet available
                Return
            End If

            If RadBtnPercentage.IsChecked = True Then
                TxtMarkupLabel.Text = "Enter Percentage:"
                MarkupPrefix.Kind = PackIconKind.PercentOutline
            Else ' Flat markup
                TxtMarkupLabel.Text = "Enter Amount:"
                MarkupPrefix.Kind = PackIconKind.CurrencyPhp
            End If
        End Sub



        ' Initialize markup UI when the form loads
        Private Sub InitializeMarkupUI()
            ' Make sure the controls exist before trying to access them
            If TxtMarkupLabel Is Nothing OrElse RadBtnPercentage Is Nothing OrElse
           MarkupPrefix Is Nothing Then
                ' Log this or handle it accordingly - controls not ready yet
                Return
            End If

            ' Set default to percentage
            RadBtnPercentage.IsChecked = True
            ' Update the labels safely
            UpdateMarkupLabelsIfReady()

            ' Ensure initial calculation is performed if values exist
            If Not String.IsNullOrWhiteSpace(TxtPurchaseOrder?.Text) Then
                CalculateSellingPrice()
            End If
        End Sub
#End Region
    End Class

    ' You'll need to add this converter class to your project
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
