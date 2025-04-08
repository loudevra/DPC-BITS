Imports System.IO
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DocumentFormat.OpenXml.Office.CustomUI
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports MaterialDesignThemes.Wpf
Imports MaterialDesignThemes.Wpf.Theme
Imports Microsoft.Win32

Namespace DPC.Views.Stocks.ItemManager.NewProduct
    Public Class AddNewProducts
        Inherits Window

        Private productController As New ProductController()
        Private WithEvents AddRowPopoutControl As AddRowPopout
        Private popup As Popup

        Public Sub New()
            InitializeComponent()

            uploadTimer.Interval = TimeSpan.FromMilliseconds(100) ' Update every 100ms
            AddHandler uploadTimer.Tick, AddressOf uploadTimer_Tick

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNav

            Toggle.IsChecked = False
            VariationChecker(Toggle)

            CheckBoxSerialNumber.IsChecked = True
            SerialNumberChecker(CheckBoxSerialNumber)

            ProductController.GetBrands(ComboBoxBrand)

            If ComboBoxBrand.SelectedItem IsNot Nothing Then
                CategoryComboBox_SelectionChanged(ComboBoxBrand, Nothing)
            End If

            ProductController.GetProductCategory(ComboBoxCategory)


            If ComboBoxCategory.SelectedItem IsNot Nothing Then
                CategoryComboBox_SelectionChanged(ComboBoxCategory, Nothing)
            End If

            ProductController.GetWarehouse(ComboBoxWarehouse)

            Dim calendarViewModel As New CalendarController.SingleCalendar()
            Me.DataContext = calendarViewModel

            ProductController.MainContainer = MainContainer
            ProductController.TxtStockUnits = TxtStockUnits
            'MessageBox.Show("MainContainer and TxtStockUnits have been initialized.")

            ProductController.BtnAddRow_Click(Nothing, Nothing)

            TxtDefaultTax.Text = 12
        End Sub

        Private Sub Toggle_Click(sender As Object, e As RoutedEventArgs)
            VariationChecker(Toggle)
        End Sub

        Private Sub VariationChecker(Toggle As System.Windows.Controls.Primitives.ToggleButton)
            Try
                If Toggle Is Nothing Then
                    Throw New Exception("ToggleButton is not initialized.")
                End If

                ' Update UI based on IsChecked state
                If Toggle.IsChecked = True Then
                    StackPanelVariation.Visibility = Visibility.Visible
                    StackPanelWarehouse.Visibility = Visibility.Collapsed
                    StackPanelRetailPrice.Visibility = Visibility.Collapsed
                    StackPanelOrderPrice.Visibility = Visibility.Collapsed

                    StackPanelTaxRate.Visibility = Visibility.Collapsed
                    StackPanelDiscountRate.Visibility = Visibility.Collapsed
                    BorderStocks.Visibility = Visibility.Collapsed
                    StackPanelAlertQuantity.Visibility = Visibility.Collapsed
                    StackPanelStockUnits.Visibility = Visibility.Collapsed
                    OuterStackPanel.Visibility = Visibility.Collapsed
                Else
                    StackPanelVariation.Visibility = Visibility.Collapsed
                    StackPanelWarehouse.Visibility = Visibility.Visible
                    StackPanelRetailPrice.Visibility = Visibility.Visible
                    StackPanelOrderPrice.Visibility = Visibility.Visible

                    StackPanelTaxRate.Visibility = Visibility.Visible
                    StackPanelDiscountRate.Visibility = Visibility.Visible
                    BorderStocks.Visibility = Visibility.Visible
                    StackPanelAlertQuantity.Visibility = Visibility.Visible
                    StackPanelStockUnits.Visibility = Visibility.Visible
                    OuterStackPanel.Visibility = Visibility.Visible
                End If

            Catch ex As Exception
                MessageBox.Show($"Error: {ex.Message}")
            End Try
        End Sub

        Private Sub IncludeSerial_Click(sender As Object, e As RoutedEventArgs)
            SerialNumberChecker(CheckBoxSerialNumber)
        End Sub
        Private Sub SerialNumberChecker(Checkbox As Controls.CheckBox)

            If Checkbox.IsChecked = True Then
                StackPanelSerialRow.Visibility = Visibility.Visible
                TxtStockUnits.IsReadOnly = True
            Else
                StackPanelSerialRow.Visibility = Visibility.Collapsed
                TxtStockUnits.IsReadOnly = False
            End If

            If TxtStockUnits.IsReadOnly = True Then
                BorderStockUnits.BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE"))
            Else
                BorderStockUnits.BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#474747"))
            End If
        End Sub
        ' Start of inserting function for add product button
        Private Sub BtnAddProduct_Click(sender As Object, e As RoutedEventArgs)
            ProductController.InsertNewProduct(Toggle, CheckBoxSerialNumber,
        TxtProductName, ComboBoxCategory, ComboBoxSubCategory,
        ComboBoxWarehouse, ComboBoxBrand, ComboBoxSupplier, TxtRetailPrice, TxtPurchaseOrder, TxtDefaultTax,
        TxtDiscountRate, TxtStockUnits, TxtAlertQuantity, ComboBoxMeasurementUnit,
        TxtDescription, SingleDatePicker, ProductController.SerialNumbers, base64Image)

            ClearInputFields()

            ' Get Category ID from the selected ComboBox item
            Dim CategoryID As String = If(ComboBoxCategory.SelectedItem IsNot Nothing,
                                  CType(ComboBoxCategory.SelectedItem, ComboBoxItem).Tag.ToString(), "")

            ' Get Subcategory ID from the selected ComboBox item
            Dim SubCategoryID As String = If(ComboBoxSubCategory.SelectedItem IsNot Nothing,
                                     CType(ComboBoxSubCategory.SelectedItem, ComboBoxItem).Tag.ToString(), "")

            Dim newProduct As New Product With {
        .CategoryID = CategoryID,
        .SubCategoryID = SubCategoryID
    }
        End Sub


        Private Sub ClearInputFields()
            ' Clear TextBoxes
            TxtProductName.Clear()
            TxtRetailPrice.Clear()
            TxtPurchaseOrder.Clear()
            TxtDefaultTax.Clear()
            TxtDefaultTax.Text = 12
            TxtDiscountRate.Clear()
            TxtStockUnits.Text = "1"
            TxtAlertQuantity.Clear()
            TxtDescription.Clear()
            base64Image = String.Empty

            ' Reset ComboBoxes to first item (index 0)
            ComboBoxCategory.SelectedIndex = 0
            ComboBoxSubCategory.SelectedIndex = 0
            ComboBoxWarehouse.SelectedIndex = 0
            ComboBoxMeasurementUnit.SelectedIndex = 0
            ComboBoxBrand.SelectedIndex = 0
            ComboBoxSupplier.SelectedIndex = 0

            ' Set DatePicker to current date
            SingleDatePicker.SelectedDate = DateTime.Now

            ' Clear Serial Numbers and reset to one row
            If ProductController.SerialNumbers IsNot Nothing Then
                ProductController.SerialNumbers.Clear()
            End If

            If MainContainer IsNot Nothing Then
                MainContainer.Children.Clear()
            End If

            ' Add back one row for Serial Number input
            ProductController.BtnAddRow_Click(Nothing, Nothing)
            TxtStockUnits.Text = "1"

            ' Clear Image-related elements
            ' Reset the uploaded image source
            UploadedImage.Source = Nothing

            ' Hide the Image Display Panel
            ImageDisplayPanel.Visibility = Visibility.Collapsed

            ' Optionally, remove the temporary image file
            Dim tempImagePath As String = Path.Combine(Path.GetTempPath(), "decoded_image.png")
            If File.Exists(tempImagePath) Then
                GC.Collect()
                GC.WaitForPendingFinalizers()
                File.Delete(tempImagePath)
            End If

            ' Reset image info
            ImgName.Text = ""
            ImgSize.Text = ""

            ' Reset the Base64 string
            base64Image = String.Empty

            ' Hide the Remove Image button
            BtnRemoveImage.Visibility = Visibility.Collapsed
        End Sub


        Private TxtSerialNumber As TextBox

        ' Function to handle integer only input on textboxes
        Private Sub IntegerOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            If Not IsNumeric(e.Text) OrElse Not Integer.TryParse(e.Text, New Integer()) Then
                e.Handled = True ' Block non-integer input
            End If
        End Sub

        ' Function to handle pasting
        Private Sub IntegerOnlyPasteHandler(sender As Object, e As DataObjectPastingEventArgs)
            If e.DataObject.GetDataPresent(GetType(String)) Then
                Dim pastedText As String = CStr(e.DataObject.GetData(GetType(String)))
                If Not Integer.TryParse(pastedText, New Integer()) Then
                    e.CancelCommand() ' Cancel if pasted data is not an integer
                End If
            Else
                e.CancelCommand()
            End If
        End Sub

        ' Handles the combobox for categories and subcategories
        Private Sub CategoryComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBoxCategory.SelectionChanged
            Dim selectedCategory As String = TryCast(ComboBoxCategory.SelectedItem, ComboBoxItem)?.Content?.ToString()
            If Not String.IsNullOrEmpty(selectedCategory) Then
                ProductController.GetProductSubcategory(selectedCategory, ComboBoxSubCategory, SubCategoryLabel, StackPanelSubCategory)
            Else
                ComboBoxSubCategory.Items.Clear()
            End If
        End Sub

        ' Handles the combobox for brands and suppliers
        Private Sub ComboBoxBrand_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBoxBrand.SelectionChanged
            Dim selectedBrandItem As ComboBoxItem = TryCast(ComboBoxBrand.SelectedItem, ComboBoxItem)

            If selectedBrandItem IsNot Nothing AndAlso selectedBrandItem.Tag IsNot Nothing Then
                Dim brandID As Integer = Convert.ToInt32(selectedBrandItem.Tag)
                ProductController.GetSuppliersByBrand(brandID, ComboBoxSupplier)
            Else
                ComboBoxSupplier.Items.Clear()
            End If
        End Sub
        ' Handles the date picker component
        Public Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            SingleDatePicker.IsDropDownOpen = True
        End Sub

        ' Handles the serial table components
        Private Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs)
            ProductController.BtnAddRow_Click(Nothing, Nothing)
        End Sub

        Private Sub BtnRemoveRow_Click(sender As Object, e As RoutedEventArgs)
            ProductController.BtnRemoveRow_Click(Nothing, Nothing)
        End Sub

        Private Function FindParentGrid(element As DependencyObject) As Grid
            ' Traverse up the visual tree to find the Grid
            While element IsNot Nothing AndAlso Not TypeOf element Is Grid
                element = VisualTreeHelper.GetParent(element)
            End While
            Return TryCast(element, Grid)
        End Function
        Private Sub TxtStockUnits_KeyDown(sender As Object, e As KeyEventArgs)
            ' Check if Enter key is pressed
            If e.Key = Key.Enter Then
                Dim stockUnits As Integer

                ' Validate if input is a valid number and greater than zero
                If Integer.TryParse(TxtStockUnits.Text, stockUnits) Then
                    If stockUnits > 0 Then
                        ' Clear previous rows
                        MainContainer.Children.Clear()

                        ' Clear SerialNumbers to remove old references
                        ProductController.SerialNumbers.Clear()

                        ' Call BtnAddRow_Click the specified number of times
                        For i As Integer = 1 To stockUnits
                            BtnAddRow_Click(Nothing, Nothing)
                        Next

                        ' Ensure the textbox retains the correct value
                        TxtStockUnits.Text = stockUnits.ToString()
                    Else
                        MessageBox.Show("Please enter a number greater than zero.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning)
                    End If
                Else
                    MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning)
                End If

                ' Prevent further propagation of the event
                e.Handled = True
            End If
        End Sub


        Private recentlyClosed As Boolean = False

        Private Sub OpenAddVariation(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim openAddVariation As New DPC.Views.Stocks.ItemManager.NewProduct.ProductVariationDetails()

            ' Open the popup
            Me.Close()
            openAddVariation.Show()
        End Sub

        'Handles file input
        ' Variable to track whether an image has been uploaded
        Private isUploadLocked As Boolean = False
        Private uploadTimer As New DispatcherTimer()
        Private base64Image As String ' Variable to store Base64 string

        Private Function ConvertImageToBase64(filePath As String) As String
            Try
                Dim imageBytes As Byte() = File.ReadAllBytes(filePath) ' Read file as byte array
                Dim base64String As String = Convert.ToBase64String(imageBytes) ' Convert to Base64
                Return base64String
            Catch ex As Exception
                MessageBox.Show("Error converting image to Base64: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return String.Empty
            End Try
        End Function

        Private Sub Border_DragEnter(sender As Object, e As DragEventArgs)
            ' Check if the dragged data is a file
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                e.Effects = DragDropEffects.Copy
            End If
        End Sub

        Private Sub BtnBrowse_Click(sender As Object, e As RoutedEventArgs)
            If isUploadLocked Then Return ' Prevent new uploads

            Dim openFileDialog As New OpenFileDialog With {
        .Filter = "Image Files|*.jpg;*.jpeg;*.png",
        .Title = "Select an Image"
    }

            If openFileDialog.ShowDialog() = True Then
                Dim filePath As String = openFileDialog.FileName
                If ValidateImageFile(filePath) Then
                    StartFileUpload(filePath)
                End If
            End If
        End Sub

        Private Sub Border_Drop(sender As Object, e As DragEventArgs)
            If isUploadLocked Then Return ' Prevent new uploads

            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
                Dim filePath As String = files(0)
                If ValidateImageFile(filePath) Then
                    StartFileUpload(filePath)
                End If
            End If
        End Sub

        ' Function to validate the image file
        Private Function ValidateImageFile(filePath As String) As Boolean
            Dim fileInfo As New FileInfo(filePath)

            ' Check file size (2MB max)
            If fileInfo.Length > 2 * 1024 * 1024 Then
                MessageBox.Show("File is too large! Please upload an image under 2MB.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End If

            ' Check file extension
            Dim validExtensions As String() = {".jpg", ".jpeg", ".png"}
            If Not validExtensions.Contains(fileInfo.Extension.ToLower()) Then
                MessageBox.Show("Invalid file format! Only JPG and PNG are allowed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End If

            Return True
        End Function
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
            isUploadLocked = True ' Lock uploading process

            ' Reset and configure the timer properly before starting
            If uploadTimer.IsEnabled Then
                uploadTimer.Stop()
                RemoveHandler uploadTimer.Tick, AddressOf uploadTimer_Tick
            End If
            AddHandler uploadTimer.Tick, AddressOf uploadTimer_Tick
            uploadTimer.Interval = TimeSpan.FromMilliseconds(100)
            uploadTimer.Start()
        End Sub
        Private Sub uploadTimer_Tick(sender As Object, e As EventArgs)
            If UploadProgressBar.Value < 100 Then
                UploadProgressBar.Value += 2 ' Increase by 2% every tick
            Else
                uploadTimer.Stop()
                RemoveHandler uploadTimer.Tick, AddressOf uploadTimer_Tick
                UploadStatus.Text = "Upload Complete"

                ' Hide Image Info Panel and Show Image Display Panel
                ImageInfoPanel.Visibility = Visibility.Collapsed
                ImageDisplayPanel.Visibility = Visibility.Visible

                ' Decode Base64 string to an image file
                Try
                    Dim tempImagePath As String = Path.Combine(Path.GetTempPath(), "decoded_image.png")

                    ' Ensure the previous file is released before writing a new one
                    If File.Exists(tempImagePath) Then
                        GC.Collect()
                        GC.WaitForPendingFinalizers()
                        File.Delete(tempImagePath)
                    End If

                    ' Decode and write the new file
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

                ' Make the Remove Image button visible
                BtnRemoveImage.Visibility = Visibility.Visible
            End If
        End Sub

        Private Sub RemoveImage(sender As Object, e As RoutedEventArgs)
            ' Stop and reset the upload timer
            uploadTimer.Stop()
            RemoveHandler uploadTimer.Tick, AddressOf uploadTimer_Tick

            ' Reset UI elements
            UploadProgressBar.Value = 0
            UploadStatus.Text = ""
            ImgName.Text = ""
            ImgSize.Text = ""

            ' Hide image info panel and image display panel
            ImageInfoPanel.Visibility = Visibility.Collapsed
            ImageDisplayPanel.Visibility = Visibility.Collapsed

            ' Re-enable browse button and drag-drop functionality
            BtnBrowse.IsEnabled = True
            DropBorder.AllowDrop = True
            isUploadLocked = False

            ' Hide the Remove Image button
            BtnRemoveImage.Visibility = Visibility.Collapsed

            ' Reset Base64 string
            base64Image = String.Empty
        End Sub


    End Class
End Namespace
