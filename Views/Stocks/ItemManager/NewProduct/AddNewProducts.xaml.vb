Imports System.IO
Imports System.Text
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
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
            ' Set the DataContext to our shared ViewModel
            Me.DataContext = ProductViewModel.Instance
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
                    StackPanelDiscountRate, BorderStocks, StackPanelAlertQuantity,
                    StackPanelStockUnits, OuterStackPanel)

            ElseIf ProductController.IsVariation = True Then
                Toggle.IsChecked = True
                ProductController.VariationChecker(Toggle, StackPanelVariation, StackPanelWarehouse,
                    StackPanelRetailPrice, StackPanelOrderPrice, StackPanelTaxRate,
                    StackPanelDiscountRate, BorderStocks, StackPanelAlertQuantity,
                    StackPanelStockUnits, OuterStackPanel)
            End If


            CheckBoxSerialNumber.IsChecked = True
            ProductController.SerialNumberChecker(CheckBoxSerialNumber, StackPanelSerialRow,
                TxtStockUnits, BorderStockUnits)

            ' Set default values
            TxtDefaultTax.Text = "12"
        End Sub

        Private Sub SetupControllerReferences()
            ' Set controller references
            ProductController.MainContainer = MainContainer
            ProductController.TxtStockUnits = TxtStockUnits

            ' Initialize data context
            Dim calendarViewModel As New CalendarController.SingleCalendar()
            Me.DataContext = calendarViewModel
        End Sub

        Private Sub LoadInitialData()
            ' Load dropdown data
            ProductController.GetBrands(ComboBoxBrand)
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
                StackPanelDiscountRate, BorderStocks, StackPanelAlertQuantity,
                StackPanelStockUnits, OuterStackPanel)
        End Sub

        Private Sub IncludeSerial_Click(sender As Object, e As RoutedEventArgs)
            ProductController.SerialNumberChecker(CheckBoxSerialNumber, StackPanelSerialRow,
                TxtStockUnits, BorderStockUnits)
        End Sub

        Private Sub BtnAddProduct_Click(sender As Object, e As RoutedEventArgs)
            ProductController.InsertNewProduct(Toggle, CheckBoxSerialNumber,
                TxtProductName, ComboBoxCategory, ComboBoxSubCategory,
                ComboBoxWarehouse, ComboBoxBrand, ComboBoxSupplier, TxtRetailPrice,
                TxtPurchaseOrder, TxtDefaultTax, TxtDiscountRate, TxtStockUnits,
                TxtAlertQuantity, ComboBoxMeasurementUnit, TxtDescription,
                SingleDatePicker, ProductController.SerialNumbers, base64Image)

            ProductController.ClearInputFields(TxtProductName, TxtRetailPrice, TxtPurchaseOrder,
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

        Public Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            SingleDatePicker.IsDropDownOpen = True
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
            AddHandler openAddVariation.Close, AddressOf AddVariation_Closed

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
    End Class
End Namespace