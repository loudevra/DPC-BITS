Imports System.IO
Imports System.Text
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports MaterialDesignThemes.Wpf
Imports Microsoft.Win32

Namespace DPC.Views.Stocks.ItemManager.NewProduct
    Public Class AddNewProducts
        Inherits UserControl

        Private ProductController As New ProductController()
        Private WithEvents AddRowPopoutControl As AddRowPopout
        Private popup As Popup
        Private uploadTimer As New DispatcherTimer()
        Private base64Image As String
        Private isUploadLocked As Boolean = False

#Region "Initialization"
        Public Sub New()
            InitializeComponent()
            SetupTimers()
            InitializeUIElements()
            SetupControllerReferences()
            LoadInitialData()

            Me.DataContext = ProductViewModel.Instance
        End Sub

        Private Sub AddNewProducts_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            ' Now initialize the markup UI after the control has been loaded
            InitializeMarkupUI()
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
            ' Initialize UI state
            If ProductController.IsVariation = Nothing Or ProductController.IsVariation = False Then
                Toggle.IsChecked = False
                ProductController.VariationChecker(Toggle, StackPanelVariation, StackPanelWarehouse,
        StackPanelRetailPrice, StackPanelOrderPrice, StackPanelTaxRate,
        StackPanelDiscountRate, StackPanelMarkup, BorderStocks, StackPanelAlertQuantity,
        StackPanelStockUnits, OuterStackPanel)

            ElseIf ProductController.IsVariation = True Then
                Toggle.IsChecked = True
                ProductController.VariationChecker(Toggle, StackPanelVariation, StackPanelWarehouse,
        StackPanelRetailPrice, StackPanelOrderPrice, StackPanelTaxRate,
        StackPanelDiscountRate, StackPanelMarkup, BorderStocks, StackPanelAlertQuantity,
        StackPanelStockUnits, OuterStackPanel)
            End If

            CheckBoxSerialNumber.IsChecked = True
            ProductController.SerialNumberChecker(CheckBoxSerialNumber, StackPanelSerialRow,
    TxtStockUnits, BorderStockUnits)

            ' Set default values
            TxtDefaultTax.Text = "12"
            TxtDiscountRate.Text = "0"
            ' Move this AFTER the controls are initialized
            ' Initialize markup UI - But ONLY after the form has loaded
            ' We'll handle this in the Loaded event instead
            ' InitializeMarkupUI()

            SingleDatePicker.DisplayDateStart = DateTime.Today
        End Sub

        Private Sub SetupControllerReferences()
            ' Set controller references
            ProductController.MainContainer = MainContainer
            ProductController.TxtStockUnits = TxtStockUnits

            ' Create calendar view model with null date
            Dim calendarViewModel As New CalendarController.SingleCalendar()
            calendarViewModel.SelectedDate = Nothing  ' Set to null as default
            calendarViewModel.MinimumDate = DateTime.Today

            ' Set the DataContext to our calendar view model
            SingleDatePicker.DataContext = calendarViewModel  ' Set DataContext only for DatePicker

            ' Make sure the date button also uses the same DataContext
            Dim dateButton As Button = CType(FindName("DateButton"), Button)
            If dateButton IsNot Nothing Then
                dateButton.DataContext = calendarViewModel
            End If
        End Sub

        Private Sub LoadInitialData()
            ' Load dropdown data
            ProductController.GetBrandsWithSupplier(ComboBoxBrand)
            ProductController.GetProductCategory(ComboBoxCategory)
            ProductController.GetWarehouse(ComboBoxWarehouse)

            ' Add initial row
            ProductController.BtnAddRow_Click(Nothing, Nothing)

            ' Load variations if any
            Dim existingVariations As List(Of ProductVariation) = ProductController.GetProductVariations()
            If existingVariations IsNot Nothing Then
                ProductController.UpdateProductVariationText(existingVariations, TxtProductVariation)
            End If
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
            ProductController.SerialNumberChecker(CheckBoxSerialNumber, StackPanelSerialRow,
                TxtStockUnits, BorderStockUnits)
        End Sub

        Private Sub BtnAddProduct_Click(sender As Object, e As RoutedEventArgs)
            ProductController.InsertNewProduct(Toggle, CheckBoxSerialNumber,
                TxtProductName, TxtProductCode, ComboBoxCategory, ComboBoxSubCategory,
                ComboBoxWarehouse, ComboBoxBrand, ComboBoxSupplier, TxtRetailPrice,
                TxtPurchaseOrder, TxtDefaultTax, TxtDiscountRate, TxtStockUnits,
                TxtAlertQuantity, ComboBoxMeasurementUnit, TxtDescription,
                SingleDatePicker, ProductController.SerialNumbers, base64Image)

            ProductController.ClearInputFields(TxtProductName, TxtProductCode, TxtRetailPrice, TxtPurchaseOrder,
                TxtDefaultTax, TxtDiscountRate, TxtStockUnits, TxtAlertQuantity, TxtDescription,
                ComboBoxCategory, ComboBoxSubCategory, ComboBoxWarehouse, ComboBoxMeasurementUnit,
                ComboBoxBrand, ComboBoxSupplier, SingleDatePicker, MainContainer)

            ResetImageComponents()
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

        Private Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            SingleDatePicker.IsDropDownOpen = True
        End Sub

        ' Add a new handler for date changes
        Private Sub SingleDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles SingleDatePicker.SelectedDateChanged
            ' This will ensure that when a date is selected, the UI updates
            Dim datePicker As DatePicker = TryCast(sender, DatePicker)
            If datePicker IsNot Nothing AndAlso datePicker.DataContext IsNot Nothing Then
                Dim calendarViewModel As CalendarController.SingleCalendar = TryCast(datePicker.DataContext, CalendarController.SingleCalendar)
                If calendarViewModel IsNot Nothing Then
                    calendarViewModel.SelectedDate = datePicker.SelectedDate
                End If
            End If
        End Sub

        Private Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs)
            ProductController.BtnAddRow_Click(Nothing, Nothing)
        End Sub

        Private Sub BtnRemoveRow_Click(sender As Object, e As RoutedEventArgs)
            ProductController.BtnRemoveRow_Click(Nothing, Nothing)
        End Sub

        Private Sub TxtStockUnits_KeyDown(sender As Object, e As KeyEventArgs)
            ' Check if Enter key is pressed
            If e.Key = Key.Enter Then
                ProductController.ProcessStockUnitsEntry(TxtStockUnits, MainContainer)
                ' Prevent further propagation of the event
                e.Handled = True
            End If
        End Sub

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