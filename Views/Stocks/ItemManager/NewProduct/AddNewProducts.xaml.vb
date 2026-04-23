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
Imports MySql.Data.MySqlClient
Namespace DPC.Views.Stocks.ItemManager.NewProduct
    Public Class AddNewProducts
        Inherits UserControl
        Private ProductController As New ProductController()
        Private WithEvents AddRowPopoutControl As AddRowPopout
        Private popup As Popup
        Private uploadTimer As New DispatcherTimer()
        Private base64Image As String
        Private isUploadLocked As Boolean = False
        Private popupAddBrand As Popup
        Private popupAddSupplier As Popup
        Private popupAddCategory As Popup
        Private popupAddSubCategory As Popup
        Private recentlyClosed As Boolean = False
        Private filterTimer As New DispatcherTimer()

        ' Persists combo selections across page navigation
        Private Shared _savedBrandTag As String = Nothing
        Private Shared _savedSupplierTag As String = Nothing
        Private Shared _savedCategoryContent As String = Nothing
        Private Shared _savedSubCategoryContent As String = Nothing
        Private Shared _savedWarehouseTag As String = Nothing
        Private Shared _savedMeasurementUnitIndex As Integer = 0

#Region "Initialization"
        Public Sub New()
            InitializeComponent()
            SetupTimers()
            SetupControllerReferences()
            LoadInitialData()
            Me.DataContext = ProductViewModel.Instance
        End Sub

        Private Sub RestoreComboSelections()
            ' Brand
            If _savedBrandTag IsNot Nothing Then
                For Each item As ComboBoxItem In ComboBoxBrand.Items
                    If item.Tag?.ToString() = _savedBrandTag Then
                        ComboBoxBrand.SelectedItem = item
                        Exit For
                    End If
                Next
            End If

            ' Supplier
            If _savedSupplierTag IsNot Nothing Then
                For Each item As ComboBoxItem In ComboBoxSupplier.Items
                    If item.Tag?.ToString() = _savedSupplierTag Then
                        ComboBoxSupplier.SelectedItem = item
                        Exit For
                    End If
                Next
            End If

            ' Category
            If _savedCategoryContent IsNot Nothing Then
                For Each item As ComboBoxItem In ComboBoxCategory.Items
                    If item.Content?.ToString() = _savedCategoryContent Then
                        ComboBoxCategory.SelectedItem = item
                        Exit For
                    End If
                Next
            End If

            ' SubCategory
            If _savedSubCategoryContent IsNot Nothing Then
                For Each item As ComboBoxItem In ComboBoxSubCategory.Items
                    If item.Content?.ToString() = _savedSubCategoryContent Then
                        ComboBoxSubCategory.SelectedItem = item
                        Exit For
                    End If
                Next
            End If

            ' Warehouse
            If _savedWarehouseTag IsNot Nothing Then
                For Each item As ComboBoxItem In ComboBoxWarehouse.Items
                    If item.Tag?.ToString() = _savedWarehouseTag Then
                        ComboBoxWarehouse.SelectedItem = item
                        Exit For
                    End If
                Next
            End If

            ' Measurement Unit (static items, index is stable)
            ComboBoxMeasurementUnit.SelectedIndex = _savedMeasurementUnitIndex
        End Sub

        Private Sub AddNewProducts_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            InitializeMarkupUI()
            InitializeUIElements()
            ApplyRolePermissions()

            ' Brand Search
            Dim cbTextBox As TextBox = CType(ComboBoxBrand.Template.FindName("PART_EditableTextBox", ComboBoxBrand), TextBox)
            If cbTextBox IsNot Nothing Then
                filterTimer = New DispatcherTimer()
                filterTimer.Interval = TimeSpan.FromMilliseconds(300)
                AddHandler filterTimer.Tick, Sub(src, args)
                                                 filterTimer.Stop()
                                                 Dim view = CollectionViewSource.GetDefaultView(ComboBoxBrand.Items)
                                                 If view IsNot Nothing Then
                                                     view.Refresh()
                                                     If Not view.IsEmpty Then
                                                         ComboBoxBrand.IsDropDownOpen = True
                                                     End If
                                                 End If
                                             End Sub
                AddHandler cbTextBox.PreviewMouseLeftButtonDown, Sub(src, args)
                                                                     ComboBoxBrand.IsDropDownOpen = True
                                                                 End Sub
                AddHandler cbTextBox.TextChanged, Sub(s, args)
                                                      Dim originalText = cbTextBox.Text
                                                      Dim upperText = originalText.ToUpper()
                                                      If originalText <> upperText Then
                                                          Dim selStart = cbTextBox.SelectionStart
                                                          cbTextBox.Text = upperText
                                                          cbTextBox.SelectionStart = selStart
                                                          Return
                                                      End If
                                                      If Not cbTextBox.IsFocused Then Return
                                                      Dim selectedBrand = TryCast(ComboBoxBrand.SelectedItem, ComboBoxItem)
                                                      If selectedBrand IsNot Nothing AndAlso selectedBrand.Content?.ToString() = originalText Then
                                                          ComboBoxBrand.IsDropDownOpen = False
                                                          Return
                                                      End If
                                                      ComboBoxBrand.IsDropDownOpen = True
                                                      filterTimer.Stop()
                                                      filterTimer.Start()
                                                  End Sub
            End If

            AddHandler ComboBoxBrand.SelectionChanged, Sub(sender2, e2)
                                                           Dim selectedBrand = TryCast(ComboBoxBrand.SelectedItem, ComboBoxItem)
                                                           If selectedBrand IsNot Nothing Then
                                                               Dim brandID As Integer
                                                               If Integer.TryParse(selectedBrand.Tag?.ToString(), brandID) Then
                                                                   ProductController.GetSuppliersByBrand(brandID, ComboBoxSupplier)
                                                               End If
                                                           End If
                                                       End Sub

            ' Supplier Search
            Dim cbSupplierTextBox As TextBox = CType(ComboBoxSupplier.Template.FindName("PART_EditableTextBox", ComboBoxSupplier), TextBox)
            If cbSupplierTextBox IsNot Nothing Then
                Dim supplierTimer As New DispatcherTimer With {.Interval = TimeSpan.FromMilliseconds(300)}
                AddHandler supplierTimer.Tick, Sub(src, args)
                                                   supplierTimer.Stop()
                                                   Dim view = CollectionViewSource.GetDefaultView(ComboBoxSupplier.Items)
                                                   If view IsNot Nothing Then
                                                       view.Refresh()
                                                       If Not view.IsEmpty Then ComboBoxSupplier.IsDropDownOpen = True
                                                   End If
                                               End Sub
                AddHandler cbSupplierTextBox.PreviewMouseLeftButtonDown, Sub(src, args)
                                                                             ComboBoxSupplier.IsDropDownOpen = True
                                                                         End Sub
                AddHandler cbSupplierTextBox.TextChanged, Sub(s, args)
                                                              Dim originalText = cbSupplierTextBox.Text
                                                              Dim upperText = originalText.ToUpper()
                                                              If originalText <> upperText Then
                                                                  Dim selStart = cbSupplierTextBox.SelectionStart
                                                                  cbSupplierTextBox.Text = upperText
                                                                  cbSupplierTextBox.SelectionStart = selStart
                                                                  Return
                                                              End If
                                                              If Not cbSupplierTextBox.IsFocused Then Return
                                                              Dim selectedSupplier = TryCast(ComboBoxSupplier.SelectedItem, ComboBoxItem)
                                                              If selectedSupplier IsNot Nothing AndAlso selectedSupplier.Content?.ToString() = originalText Then
                                                                  ComboBoxSupplier.IsDropDownOpen = False
                                                                  Return
                                                              End If
                                                              ComboBoxSupplier.IsDropDownOpen = True
                                                              supplierTimer.Stop()
                                                              supplierTimer.Start()
                                                          End Sub
            End If

            ' Category Search
            Dim cbCategoryTextBox As TextBox = CType(ComboBoxCategory.Template.FindName("PART_EditableTextBox", ComboBoxCategory), TextBox)
            If cbCategoryTextBox IsNot Nothing Then
                Dim categoryTimer As New DispatcherTimer With {.Interval = TimeSpan.FromMilliseconds(300)}
                AddHandler categoryTimer.Tick, Sub(src, args)
                                                   categoryTimer.Stop()
                                                   Dim view = CollectionViewSource.GetDefaultView(ComboBoxCategory.Items)
                                                   If view IsNot Nothing Then
                                                       view.Refresh()
                                                       If Not view.IsEmpty Then ComboBoxCategory.IsDropDownOpen = True
                                                   End If
                                               End Sub
                AddHandler cbCategoryTextBox.PreviewMouseLeftButtonDown, Sub(src, args)
                                                                             ComboBoxCategory.IsDropDownOpen = True
                                                                         End Sub
                AddHandler cbCategoryTextBox.TextChanged, Sub(s, args)
                                                              Dim originalText = cbCategoryTextBox.Text
                                                              Dim upperText = originalText.ToUpper()
                                                              If originalText <> upperText Then
                                                                  Dim selStart = cbCategoryTextBox.SelectionStart
                                                                  cbCategoryTextBox.Text = upperText
                                                                  cbCategoryTextBox.SelectionStart = selStart
                                                                  Return
                                                              End If
                                                              If Not cbCategoryTextBox.IsFocused Then Return
                                                              Dim selectedItem = TryCast(ComboBoxCategory.SelectedItem, ComboBoxItem)
                                                              If selectedItem IsNot Nothing AndAlso selectedItem.Content?.ToString() = originalText Then
                                                                  ComboBoxCategory.IsDropDownOpen = False
                                                                  Return
                                                              End If
                                                              ComboBoxCategory.IsDropDownOpen = True
                                                              categoryTimer.Stop()
                                                              categoryTimer.Start()
                                                          End Sub
            End If

            ' SubCategory Search
            Dim cbSubCategoryTextBox As TextBox = CType(ComboBoxSubCategory.Template.FindName("PART_EditableTextBox", ComboBoxSubCategory), TextBox)
            If cbSubCategoryTextBox IsNot Nothing Then
                Dim subCategoryTimer As New DispatcherTimer With {.Interval = TimeSpan.FromMilliseconds(300)}
                AddHandler subCategoryTimer.Tick, Sub(src, args)
                                                      subCategoryTimer.Stop()
                                                      Dim view = CollectionViewSource.GetDefaultView(ComboBoxSubCategory.Items)
                                                      If view IsNot Nothing Then
                                                          view.Refresh()
                                                          If Not view.IsEmpty Then ComboBoxSubCategory.IsDropDownOpen = True
                                                      End If
                                                  End Sub
                AddHandler cbSubCategoryTextBox.PreviewMouseLeftButtonDown, Sub(src, args)
                                                                                ComboBoxSubCategory.IsDropDownOpen = True
                                                                            End Sub
                AddHandler cbSubCategoryTextBox.TextChanged, Sub(s, args)
                                                                 Dim originalText = cbSubCategoryTextBox.Text
                                                                 Dim upperText = originalText.ToUpper()
                                                                 If originalText <> upperText Then
                                                                     Dim selStart = cbSubCategoryTextBox.SelectionStart
                                                                     cbSubCategoryTextBox.Text = upperText
                                                                     cbSubCategoryTextBox.SelectionStart = selStart
                                                                     Return
                                                                 End If
                                                                 If Not cbSubCategoryTextBox.IsFocused Then Return
                                                                 Dim selectedItem = TryCast(ComboBoxSubCategory.SelectedItem, ComboBoxItem)
                                                                 If selectedItem IsNot Nothing AndAlso selectedItem.Content?.ToString() = originalText Then
                                                                     ComboBoxSubCategory.IsDropDownOpen = False
                                                                     Return
                                                                 End If
                                                                 ComboBoxSubCategory.IsDropDownOpen = True
                                                                 subCategoryTimer.Stop()
                                                                 subCategoryTimer.Start()
                                                             End Sub
            End If

            ' Save selections whenever user picks from each dropdown
            AddHandler ComboBoxSupplier.SelectionChanged, Sub(s, ev)
                                                              Dim item = TryCast(ComboBoxSupplier.SelectedItem, ComboBoxItem)
                                                              _savedSupplierTag = item?.Tag?.ToString()
                                                          End Sub

            AddHandler ComboBoxSubCategory.SelectionChanged, Sub(s, ev)
                                                                 Dim item = TryCast(ComboBoxSubCategory.SelectedItem, ComboBoxItem)
                                                                 _savedSubCategoryContent = item?.Content?.ToString()
                                                             End Sub

            AddHandler ComboBoxWarehouse.SelectionChanged, Sub(s, ev)
                                                               Dim item = TryCast(ComboBoxWarehouse.SelectedItem, ComboBoxItem)
                                                               _savedWarehouseTag = item?.Tag?.ToString()
                                                           End Sub

            AddHandler ComboBoxMeasurementUnit.SelectionChanged, Sub(s, ev)
                                                                     _savedMeasurementUnitIndex = ComboBoxMeasurementUnit.SelectedIndex
                                                                 End Sub

            RestoreComboSelections()
        End Sub

        Private Sub SetupTimers()
            uploadTimer.Interval = TimeSpan.FromMilliseconds(100)
        End Sub

        Private Sub OpenProductVariationDetails()
            Dim productVariationDetails = New ProductVariationDetails()
        End Sub

        Private Sub InitializeUIElements()
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
            CheckBoxSerialNumber.IsChecked = False
            ProductController.SerialNumberChecker(CheckBoxSerialNumber, StackPanelSerialRow,
                                                  TxtStockUnits, BorderStockUnits)
            TxtDefaultTax.Text = "12"
            TxtDiscountRate.Text = "0"
            SingleDatePicker.DisplayDateStart = DateTime.Today
        End Sub

        Private Sub SetupControllerReferences()
            ProductController.MainContainer = MainContainer
            ProductController.TxtStockUnits = TxtStockUnits
            Dim calendarViewModel As New CalendarController.SingleCalendar()
            calendarViewModel.SelectedDate = Nothing
            calendarViewModel.MinimumDate = DateTime.Today
            SingleDatePicker.DataContext = calendarViewModel
            Dim dateButton As Button = CType(FindName("DateButton"), Button)
            If dateButton IsNot Nothing Then
                dateButton.DataContext = calendarViewModel
            End If
        End Sub

        Private Sub LoadInitialData()
            ProductController.GetBrandsWithSupplier(ComboBoxBrand)
            ProductController.GetProductCategory(ComboBoxCategory)
            ProductController.GetWarehouse(ComboBoxWarehouse)
            ProductController.GetProductSubcategory(String.Empty, ComboBoxSubCategory, SubCategoryLabel, StackPanelSubCategory)
            ProductController.BtnAddRow_Click(Nothing, Nothing)
            Dim existingVariations As List(Of ProductVariation) = ProductController.GetProductVariations()
            If existingVariations IsNot Nothing Then
                ProductController.UpdateProductVariationText(existingVariations, TxtProductVariation)
            End If
        End Sub
        ' --- SECURITY TOOLKIT: HIDE SENSITIVE PANELS FROM SALES ---
        ' Class-level variable to track if the user is Sales
        Private isSalesUser As Boolean = False

        ' --- SECURITY TOOLKIT: LOCK SENSITIVE INPUTS ---
        ' Class-level variable to track if the user is Sales


        ' --- SECURITY TOOLKIT: LOCK SENSITIVE INPUTS ---
        Private Sub ApplyRolePermissions()
            ' Fetch role based on the logged-in user's cached email
            Dim query As String = "SELECT ur.RoleName FROM employee e JOIN userroles ur ON e.UserRoleID = ur.RoleID WHERE e.Email = @email"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@email", CacheOnLoggedInEmail)
                        Dim roleName As Object = cmd.ExecuteScalar()

                        ' Check if the role contains "Sales"
                        If roleName IsNot Nothing AndAlso roleName.ToString().ToLower().Contains("sales") Then
                            isSalesUser = True
                        End If
                    End Using
                Catch ex As Exception
                    Console.WriteLine("Error checking role: " & ex.Message)
                End Try
            End Using

            ' If Sales, hide ONLY the Retail Price input and inject Admin message
            If isSalesUser Then
                Dim LockTextBox = Sub(txt As TextBox)
                                      If txt Is Nothing Then Return

                                      ' Hide the textbox (your CalculateSellingPrice will still update it invisibly!)
                                      txt.Visibility = Visibility.Collapsed
                                      txt.Text = "0"

                                      ' Inject the Admin Message into the same container
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

                ' ONLY lock the Product Selling Price
                LockTextBox(TxtRetailPrice)
            End If
        End Sub
#End Region

#Region "Event Handlers"
        Public Sub IntegerOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            ProductController.IntegerOnlyTextInputHandler(sender, e)
        End Sub

        Public Sub DecimalOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            ProductController.DecimalOnlyTextInputHandler(sender, e)
        End Sub

        Public Sub DecimalOnlyPasteHandler(sender As Object, e As DataObjectPastingEventArgs)
            ProductController.DecimalOnlyPasteHandler(sender, e)
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
            Dim isSuccessAddProduct As Boolean = ProductController.InsertNewProduct(Toggle, CheckBoxSerialNumber,
            TxtProductName, TxtProductCode, ComboBoxCategory, ComboBoxSubCategory,
            ComboBoxWarehouse, ComboBoxBrand, ComboBoxSupplier, TxtRetailPrice,
            TxtPurchaseOrder, TxtDefaultTax, TxtDiscountRate, TxtStockUnits,
            TxtAlertQuantity, ComboBoxMeasurementUnit, TxtDescription,
            SingleDatePicker, ProductController.SerialNumbers, base64Image)
            If isSuccessAddProduct Then
                ProductController.ClearInputFields(TxtProductName, TxtProductCode, TxtRetailPrice, TxtPurchaseOrder,
                TxtDefaultTax, TxtDiscountRate, TxtStockUnits, TxtAlertQuantity, TxtDescription,
                ComboBoxCategory, ComboBoxSubCategory, ComboBoxWarehouse, ComboBoxMeasurementUnit,
                ComboBoxBrand, ComboBoxSupplier, SingleDatePicker, MainContainer)
                ProductController.SerialNumbers.Clear()
                TxtProductVariation.Text = Nothing
                DPC.Components.Forms.AddVariation._savedVariations.Clear()
                DPC.Data.Controllers.ProductController.variationManager.GetAllVariationData().Clear()
                DPC.Data.Controllers.ProductController.variationManager.CurrentCombination = Nothing

                ' Also clear saved combo state on successful submit
                _savedBrandTag = Nothing
                _savedSupplierTag = Nothing
                _savedCategoryContent = Nothing
                _savedSubCategoryContent = Nothing
                _savedWarehouseTag = Nothing
                _savedMeasurementUnitIndex = 0

                If Not String.IsNullOrWhiteSpace(base64Image) Then
                    ResetImageComponents()
                End If
            End If
        End Sub

        Private Sub CategoryComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBoxCategory.SelectionChanged
            Dim selectedCategory As String = TryCast(ComboBoxCategory.SelectedItem, ComboBoxItem)?.Content?.ToString()
            _savedCategoryContent = selectedCategory
            If Not String.IsNullOrEmpty(selectedCategory) Then
                ProductController.GetProductSubcategory(selectedCategory, ComboBoxSubCategory, SubCategoryLabel, StackPanelSubCategory)
            Else
                ComboBoxSubCategory.Items.Clear()
            End If
        End Sub

        Private Sub ComboBoxBrand_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ComboBoxBrand.SelectionChanged
            Dim selectedBrandItem As ComboBoxItem = TryCast(ComboBoxBrand.SelectedItem, ComboBoxItem)
            If selectedBrandItem IsNot Nothing AndAlso selectedBrandItem.Tag IsNot Nothing Then
                _savedBrandTag = selectedBrandItem.Tag.ToString()
                Dim brandID As Integer = Convert.ToInt32(selectedBrandItem.Tag)
                ProductController.GetSuppliersByBrand(brandID, ComboBoxSupplier)
                ProductController.GetCategoryByBrand(brandID, ComboBoxCategory)
            Else
                _savedBrandTag = Nothing
                ComboBoxSupplier.Items.Clear()
                ComboBoxCategory.Items.Clear()
                ComboBoxSubCategory.Items.Clear()
            End If
        End Sub

        Private Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            SingleDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub SingleDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles SingleDatePicker.SelectedDateChanged
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
            If e.Key = Key.Enter Then
                ProductController.ProcessStockUnitsEntry(TxtStockUnits, MainContainer)
                e.Handled = True
            End If
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

        Private Sub ForceUpperCase(tb As TextBox)
            If tb Is Nothing Then Return
            Dim text As String = tb.Text
            Dim upper As String = text.ToUpper()
            If text <> upper Then
                Dim caretPos As Integer = tb.SelectionStart
                tb.Text = upper
                tb.SelectionStart = Math.Min(caretPos, upper.Length)
            End If
        End Sub

        Private Sub TxtProductName_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TxtProductName.TextChanged
            ForceUpperCase(TryCast(sender, TextBox))
        End Sub

        Private Sub TxtProductCode_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TxtProductCode.TextChanged
            ForceUpperCase(TryCast(sender, TextBox))
        End Sub

        Private Sub TxtDescription_TextChanged(sender As Object, e As TextChangedEventArgs) Handles TxtDescription.TextChanged
            ForceUpperCase(TryCast(sender, TextBox))
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
            Dim fileInfo As New FileInfo(filePath)
            Dim fileSizeText As String = Base64Utility.GetReadableFileSize(fileInfo.Length)
            ImgName.Text = Path.GetFileName(filePath)
            ImgSize.Text = fileSizeText

            Try
                base64Image = Base64Utility.EncodeFileToBase64(filePath)
            Catch ex As Exception
                MessageBox.Show("Error encoding image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End Try

            Try
                Dim imageSource As New BitmapImage()
                imageSource.BeginInit()
                imageSource.CacheOption = BitmapCacheOption.OnLoad
                imageSource.DecodePixelWidth = 300
                imageSource.UriSource = New Uri(filePath)
                imageSource.EndInit()
                imageSource.Freeze()
                UploadedImage.Source = imageSource
            Catch ex As Exception
                MessageBox.Show("Error loading image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End Try

            ImageInfoPanel.Visibility = Visibility.Collapsed
            ImageDisplayPanel.Visibility = Visibility.Visible
            BtnRemoveImage.Visibility = Visibility.Visible
            BtnBrowse.IsEnabled = False
            DropBorder.AllowDrop = False
            isUploadLocked = True
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

        Private Sub OnBrandAdded()
            ' LoadBrands()
        End Sub

        Private Sub BtnAddBrand_Click(sender As Object, e As RoutedEventArgs)
            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing Then Return
            If recentlyClosed Then
                recentlyClosed = False
                Return
            End If
            If popupAddBrand IsNot Nothing AndAlso popupAddBrand.IsOpen Then
                popupAddBrand.IsOpen = False
                recentlyClosed = True
                Return
            End If
            Dim addBrandWindow As New DPC.Components.Forms.AddBrand()
            AddHandler addBrandWindow.BrandAdded, AddressOf OnBrandAdded
            popupAddBrand = New Popup With {
                .Placement = PlacementMode.AbsolutePoint,
                .StaysOpen = False,
                .AllowsTransparency = True,
                .Child = addBrandWindow
            }
            AddHandler popupAddBrand.Opened, Sub()
                                                 Dim screenWidth As Double = SystemParameters.PrimaryScreenWidth
                                                 Dim screenHeight As Double = SystemParameters.PrimaryScreenHeight
                                                 Dim popupWidth As Double = addBrandWindow.ActualWidth
                                                 Dim popupHeight As Double = addBrandWindow.ActualHeight
                                                 popupAddBrand.HorizontalOffset = (screenWidth / 2) - (popupWidth / 2)
                                                 popupAddBrand.VerticalOffset = (screenHeight / 2) - (popupHeight / 2)
                                             End Sub
            AddHandler popupAddBrand.Closed, Sub()
                                                 recentlyClosed = True
                                                 ProductController.GetBrandsWithSupplier(ComboBoxBrand)
                                                 Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                             End Sub
            popupAddBrand.IsOpen = True
        End Sub

        Private Sub BtnAddSupplier_Click(sender As Object, e As RoutedEventArgs)
            If popupAddSupplier IsNot Nothing Then
                popupAddSupplier.IsOpen = False
                popupAddSupplier.Child = Nothing
            End If
            Dim addSupplierControl As New DPC.Views.Stocks.Supplier.NewSuppliers.NewSuppliers()
            AddHandler addSupplierControl.SupplierAdded, Sub()
                                                             ProductController.GetSuppliersByBrand(0, ComboBoxSupplier)
                                                         End Sub
            AddHandler addSupplierControl.ClosePopup, Sub()
                                                          popupAddSupplier.IsOpen = False
                                                      End Sub
            popupAddSupplier = New Popup With {
                .Placement = PlacementMode.AbsolutePoint,
                .StaysOpen = False,
                .AllowsTransparency = True,
                .Child = addSupplierControl
            }
            AddHandler popupAddSupplier.Opened, Sub()
                                                    Dim screenWidth As Double = SystemParameters.PrimaryScreenWidth
                                                    Dim screenHeight As Double = SystemParameters.PrimaryScreenHeight
                                                    popupAddSupplier.HorizontalOffset = (screenWidth / 2) - (addSupplierControl.ActualWidth / 2)
                                                    popupAddSupplier.VerticalOffset = (screenHeight / 2) - (addSupplierControl.ActualHeight / 2)
                                                End Sub
            popupAddSupplier.IsOpen = True
        End Sub

        Private Sub BtnAddCategory_Click(sender As Object, e As RoutedEventArgs)
            If popupAddCategory IsNot Nothing Then
                popupAddCategory.IsOpen = False
                popupAddCategory.Child = Nothing
            End If
            Dim addNewCategory As New DPC.Components.Forms.AddCategory()
            popupAddCategory = New Popup With {
                .Placement = PlacementMode.AbsolutePoint,
                .StaysOpen = False,
                .AllowsTransparency = True,
                .Child = addNewCategory
            }
            AddHandler popupAddCategory.Opened, Sub()
                                                    Dim screenWidth As Double = SystemParameters.PrimaryScreenWidth
                                                    Dim screenHeight As Double = SystemParameters.PrimaryScreenHeight
                                                    popupAddCategory.HorizontalOffset = (screenWidth / 2) - (addNewCategory.ActualWidth / 2)
                                                    popupAddCategory.VerticalOffset = (screenHeight / 2) - (addNewCategory.ActualHeight / 2)
                                                End Sub
            AddHandler popupAddCategory.Closed, Sub()
                                                    ProductController.GetProductCategory(ComboBoxCategory)
                                                End Sub
            popupAddCategory.IsOpen = True
        End Sub

        Private Sub BtnAddSubcategory_Click(sender As Object, e As RoutedEventArgs)
            If popupAddSubCategory IsNot Nothing Then
                popupAddSubCategory.IsOpen = False
                popupAddSubCategory.Child = Nothing
            End If
            Dim addNewSubCategory As New DPC.Components.Forms.AddSubcategory()
            popupAddSubCategory = New Popup With {
                .Placement = PlacementMode.AbsolutePoint,
                .StaysOpen = False,
                .AllowsTransparency = True,
                .Child = addNewSubCategory
            }
            AddHandler popupAddSubCategory.Opened, Sub()
                                                       Dim screenWidth As Double = SystemParameters.PrimaryScreenWidth
                                                       Dim screenHeight As Double = SystemParameters.PrimaryScreenHeight
                                                       popupAddSubCategory.HorizontalOffset = (screenWidth / 2) - (addNewSubCategory.ActualWidth / 2)
                                                       popupAddSubCategory.VerticalOffset = (screenHeight / 2) - (addNewSubCategory.ActualHeight / 2)
                                                   End Sub
            AddHandler popupAddSubCategory.Closed, Sub()
                                                       ProductController.GetProductSubcategory(String.Empty, ComboBoxSubCategory, SubCategoryLabel, StackPanelSubCategory)
                                                   End Sub
            popupAddSubCategory.IsOpen = True
        End Sub


#Region "Markup and Price Calculation"
        Private Sub CalculateSellingPrice()
            Try
                If TxtPurchaseOrder Is Nothing OrElse TxtMarkup Is Nothing OrElse TxtRetailPrice Is Nothing OrElse
               RadBtnPercentage Is Nothing Then
                    Return
                End If
                Dim buyingPrice As Decimal
                If String.IsNullOrWhiteSpace(TxtPurchaseOrder.Text) OrElse
               Not Decimal.TryParse(TxtPurchaseOrder.Text, buyingPrice) OrElse
               buyingPrice <= 0 Then
                    TxtRetailPrice.Text = "0.00"
                    Return
                End If
                Dim markupValue As Decimal
                If String.IsNullOrWhiteSpace(TxtMarkup.Text) OrElse
               Not Decimal.TryParse(TxtMarkup.Text, markupValue) OrElse
               markupValue < 0 Then
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

        Private Sub UpdateMarkupLabels()
            UpdateMarkupLabelsIfReady()
        End Sub

        Private Sub UpdateMarkupLabelsIfReady()
            If TxtMarkupLabel Is Nothing OrElse RadBtnPercentage Is Nothing OrElse
           MarkupPrefix Is Nothing Then
                Return
            End If
            If RadBtnPercentage.IsChecked = True Then
                TxtMarkupLabel.Text = "Enter Percentage:"
                MarkupPrefix.Kind = PackIconKind.PercentOutline
            Else
                TxtMarkupLabel.Text = "Enter Amount:"
                MarkupPrefix.Kind = PackIconKind.CurrencyPhp
            End If
        End Sub

        Private Sub InitializeMarkupUI()
            If TxtMarkupLabel Is Nothing OrElse RadBtnPercentage Is Nothing OrElse
           MarkupPrefix Is Nothing Then
                Return
            End If
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