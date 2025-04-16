Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Helpers
Imports MaterialDesignThemes.Wpf
Imports DPC.DPC.Views.Stocks.ItemManager.NewProduct

Namespace DPC.Data.Controllers
    Public Class RenderProduct
        Private Shared ReadOnly Property MainContainer As StackPanel
            Get
                Return ProductController.MainContainer
            End Get
        End Property
        Private Shared ReadOnly Property TxtStockUnits As TextBox
            Get
                Return ProductController.TxtStockUnits
            End Get
        End Property
        Private Shared ReadOnly Property SerialNumbers As List(Of TextBox)
            Get
                Return ProductController.SerialNumbers
            End Get
        End Property

        'Add serial number row
        Public Shared Sub AddSerialRow(sender As Object, e As RoutedEventArgs, Optional skipStockUpdate As Boolean = False)
            If MainContainer Is Nothing OrElse TxtStockUnits Is Nothing Then
                MessageBox.Show("MainContainer or TxtStockUnits is not initialized.")
                Return
            End If

            Dim outerStackPanel As New StackPanel With {.Margin = New Thickness(25, 20, 10, 20)}

            Dim grid As New Grid()
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            ' Access the named component using Application.Current.TryFindResource
            Dim textBox As New TextBox With {.Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style)}
            SerialNumbers.Add(textBox)

            Dim textBoxBorder As New Border With {.Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style), .Child = textBox}
            Grid.SetColumn(textBoxBorder, 0)
            grid.Children.Add(textBoxBorder)

            Dim buttonPanel As New StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(10, 0, 0, 0)}

            ' Add Row Button
            Dim addRowButton As New Button With {.Background = Brushes.White, .BorderThickness = New Thickness(0), .Name = "BtnAddRow"}
            Dim addIcon As New MaterialDesignThemes.Wpf.PackIcon With {.Kind = MaterialDesignThemes.Wpf.PackIconKind.TableRowAddAfter, .Width = 40, .Height = 30, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#456B2E"))}
            addRowButton.Content = addIcon
            AddHandler addRowButton.Click, Sub(s, ev) AddSerialRow(s, ev)
            buttonPanel.Children.Add(addRowButton)

            ' Remove Row Button
            Dim removeRowButton As New Button With {.Background = Brushes.White, .BorderThickness = New Thickness(0), .Name = "BtnRemoveRow"}
            Dim removeIcon As New MaterialDesignThemes.Wpf.PackIcon With {.Kind = MaterialDesignThemes.Wpf.PackIconKind.TableRowRemove, .Width = 40, .Height = 30, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636"))}
            removeRowButton.Content = removeIcon
            AddHandler removeRowButton.Click, Sub(s, ev) ProductController.BtnRemoveRow_Click(s, ev)
            buttonPanel.Children.Add(removeRowButton)

            ' Separator Border
            Dim separatorBorder As New Border With {.BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")), .BorderThickness = New Thickness(1), .Height = 30}
            buttonPanel.Children.Add(separatorBorder)

            ' Row Controller Button
            Dim rowControllerButton As New Button With {.Background = Brushes.White, .BorderThickness = New Thickness(0), .Name = "BtnRowController"}
            Dim menuIcon As New MaterialDesignThemes.Wpf.PackIcon With {.Kind = MaterialDesignThemes.Wpf.PackIconKind.MenuDown, .Width = 30, .Height = 30, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE"))}
            rowControllerButton.Content = menuIcon
            AddHandler rowControllerButton.Click, AddressOf ProductController.BtnRowController_Click
            buttonPanel.Children.Add(rowControllerButton)

            Grid.SetColumn(buttonPanel, 1)
            grid.Children.Add(buttonPanel)
            outerStackPanel.Children.Add(grid)

            MainContainer.Children.Add(outerStackPanel)

            ' Update TxtStockUnits value only if not skipped
            If Not skipStockUpdate Then
                Dim currentValue As Integer
                If Integer.TryParse(TxtStockUnits.Text, currentValue) Then
                    TxtStockUnits.Text = (currentValue + 1).ToString()
                Else
                    TxtStockUnits.Text = "1"
                End If
            End If
        End Sub

        'Open Row Controller Popup
        Public Shared Sub OpenRowController(sender As Object, e As RoutedEventArgs)
            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing Then Return

            ' Prevent reopening if the popup was just closed
            If ProductController.RecentlyClosed Then
                ProductController.RecentlyClosed = False
                Return
            End If

            ' If the popup exists and is open, close it
            If ProductController.popup IsNot Nothing AndAlso ProductController.popup.IsOpen Then
                ProductController.popup.IsOpen = False
                ProductController.RecentlyClosed = True
                Return
            End If

            ' Ensure the popup is only created once
            ProductController.popup = New Popup With {
            .PlacementTarget = clickedButton,
            .Placement = PlacementMode.Bottom,
            .StaysOpen = False,
            .AllowsTransparency = True
        }

            Dim popOutContent As New DPC.Components.Forms.RowControllerPopout()
            ProductController.popup.Child = popOutContent

            ' Handle popup closure
            AddHandler ProductController.popup.Closed, Sub()
                                                           ProductController.RecentlyClosed = True
                                                           Task.Delay(100).ContinueWith(Sub() ProductController.RecentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                                       End Sub

            ' Open the popup
            ProductController.popup.IsOpen = True
        End Sub

        'Clear fields in firstpage of add product
        Public Shared Sub ClearInputFieldsNoVariation(txtProductName As TextBox,
                         txtRetailPrice As TextBox,
                         txtPurchaseOrder As TextBox,
                         txtDefaultTax As TextBox,
                         txtDiscountRate As TextBox,
                         txtStockUnits As TextBox,
                         txtAlertQuantity As TextBox,
                         txtDescription As TextBox,
                         comboBoxCategory As ComboBox,
                         comboBoxSubCategory As ComboBox,
                         comboBoxWarehouse As ComboBox,
                         comboBoxMeasurementUnit As ComboBox,
                         comboBoxBrand As ComboBox,
                         comboBoxSupplier As ComboBox,
                         singleDatePicker As DatePicker,
                         mainContainer As Panel)
            ' Clear TextBoxes
            txtProductName.Clear()
            txtRetailPrice.Clear()
            txtPurchaseOrder.Clear()
            txtDefaultTax.Text = "12"
            txtDiscountRate.Clear()
            txtStockUnits.Text = "1"
            txtAlertQuantity.Clear()
            txtDescription.Clear()

            ' Reset ComboBoxes to first item (index 0)
            If comboBoxCategory.Items.Count > 0 Then comboBoxCategory.SelectedIndex = 0
            If comboBoxSubCategory.Items.Count > 0 Then comboBoxSubCategory.SelectedIndex = 0
            If comboBoxWarehouse.Items.Count > 0 Then comboBoxWarehouse.SelectedIndex = 0
            If comboBoxMeasurementUnit.Items.Count > 0 Then comboBoxMeasurementUnit.SelectedIndex = 0
            If comboBoxBrand.Items.Count > 0 Then comboBoxBrand.SelectedIndex = 0
            If comboBoxSupplier.Items.Count > 0 Then comboBoxSupplier.SelectedIndex = 0

            ' Set DatePicker to current date
            singleDatePicker.SelectedDate = DateTime.Now

            ' Clear Serial Numbers and reset to one row
            If SerialNumbers IsNot Nothing Then
                SerialNumbers.Clear()
            End If

            If mainContainer IsNot Nothing Then
                mainContainer.Children.Clear()
            End If

            ' Add back one row for Serial Number input
            ProductController.BtnAddRow_Click(Nothing, Nothing)
        End Sub

        Public Shared Function CreateDynamicContainer(containerType As String, parentWindow As Window) As ContentControl
            Dim container As New ContentControl()

            Select Case containerType
                Case "dynamicform"
                    ' Create the dynamic form content
                    Dim scrollViewer As New System.Windows.Controls.ScrollViewer With {
                        .VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                        .HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled,
                        .VerticalAlignment = VerticalAlignment.Stretch,
                        .Style = CType(Application.Current.FindResource("ModernScrollViewerStyle"), Style)
                    }

                    Dim stackPanel As New System.Windows.Controls.StackPanel()

                    ' Warehouse
                    Dim warehousePanel As New System.Windows.Controls.StackPanel With {.Margin = New Thickness(0, 0, 0, 10), .Name = "StackPanelWarehouse"}
                    Dim warehouseHeaderPanel As New System.Windows.Controls.StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
                    warehouseHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "Warehouse:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)})
                    warehouseHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "*", .FontSize = 14, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636")), .FontWeight = FontWeights.Bold})
                    warehousePanel.Children.Add(warehouseHeaderPanel)

                    Dim comboBoxWarehouse As New System.Windows.Controls.ComboBox With {
                        .Name = "ComboBoxWarehouse",
                        .Style = CType(Application.Current.FindResource("RoundedComboBoxStyle"), Style),
                        .Width = Double.NaN,
                        .Height = 40,
                        .Margin = New Thickness(0)
                    }

                    ' Add the ComboBox to the parent panel
                    warehousePanel.Children.Add(comboBoxWarehouse)
                    stackPanel.Children.Add(warehousePanel)

                    ' Initialize the ComboBox right after creating it
                    Try
                        ProductController.GetWarehouse(comboBoxWarehouse)
                    Catch ex As Exception
                        MessageBox.Show("Error initializing ComboBox: " & ex.Message)
                    End Try

                    ' Retail Price
                    Dim retailPricePanel As New System.Windows.Controls.StackPanel With {.Margin = New Thickness(0, 10, 0, 0), .Name = "StackPanelRetailPrice"}
                    Dim retailPriceInnerPanel As New System.Windows.Controls.StackPanel()
                    Dim retailPriceHeaderPanel As New System.Windows.Controls.StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
                    retailPriceHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "Selling Price:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)})
                    retailPriceHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "*", .FontSize = 14, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636")), .FontWeight = FontWeights.Bold})
                    retailPriceInnerPanel.Children.Add(retailPriceHeaderPanel)

                    Dim retailPriceBorder As New Border With {.Style = CType(Application.Current.FindResource("RoundedBorderStyle"), Style)}
                    Dim retailPriceGrid As New System.Windows.Controls.Grid()
                    retailPriceGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
                    retailPriceGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})

                    Dim retailPriceIcon As New PackIcon With {
                        .Kind = PackIconKind.CurrencyPhp,
                        .Width = 25,
                        .Height = 20,
                        .Margin = New Thickness(10, 0, 0, 0),
                        .VerticalAlignment = VerticalAlignment.Center
                    }
                    Grid.SetColumn(retailPriceIcon, 0)

                    Dim txtRetailPrice As New System.Windows.Controls.TextBox With {
                        .Name = "TxtRetailPrice",
                        .Style = CType(Application.Current.FindResource("RoundedTextboxStyle"), Style),
                        .Margin = New Thickness(0, 0, 25, 0)
                    }
                    AddHandler txtRetailPrice.PreviewTextInput, AddressOf ProductController.IntegerOnlyTextInputHandler
                    DataObject.AddPastingHandler(txtRetailPrice, New DataObjectPastingEventHandler(AddressOf ProductController.IntegerOnlyPasteHandler))
                    Grid.SetColumn(txtRetailPrice, 1)

                    retailPriceGrid.Children.Add(retailPriceIcon)
                    retailPriceGrid.Children.Add(txtRetailPrice)
                    retailPriceBorder.Child = retailPriceGrid
                    retailPriceInnerPanel.Children.Add(retailPriceBorder)
                    retailPricePanel.Children.Add(retailPriceInnerPanel)
                    stackPanel.Children.Add(retailPricePanel)

                    ' Purchase Order
                    Dim purchaseOrderPanel As New System.Windows.Controls.StackPanel With {.Margin = New Thickness(0, 10, 0, 20), .Name = "StackPanelOrderPrice"}
                    Dim purchaseOrderInnerPanel As New System.Windows.Controls.StackPanel()
                    Dim purchaseOrderHeaderPanel As New System.Windows.Controls.StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
                    purchaseOrderHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "Buying Price:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)})
                    purchaseOrderHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "*", .FontSize = 14, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636")), .FontWeight = FontWeights.Bold})
                    purchaseOrderInnerPanel.Children.Add(purchaseOrderHeaderPanel)

                    Dim purchaseOrderBorder As New Border With {.Style = CType(Application.Current.FindResource("RoundedBorderStyle"), Style)}
                    Dim purchaseOrderGrid As New System.Windows.Controls.Grid()
                    purchaseOrderGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
                    purchaseOrderGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})

                    Dim purchaseOrderIcon As New PackIcon With {
                        .Kind = PackIconKind.CurrencyPhp,
                        .Width = 25,
                        .Height = 20,
                        .Margin = New Thickness(10, 0, 0, 0),
                        .VerticalAlignment = VerticalAlignment.Center
                    }
                    Grid.SetColumn(purchaseOrderIcon, 0)

                    Dim txtPurchaseOrder As New System.Windows.Controls.TextBox With {
                        .Name = "TxtPurchaseOrder",
                        .Style = CType(Application.Current.FindResource("RoundedTextboxStyle"), Style),
                        .Margin = New Thickness(0, 0, 25, 0)
                    }
                    AddHandler txtPurchaseOrder.PreviewTextInput, AddressOf ProductController.IntegerOnlyTextInputHandler
                    DataObject.AddPastingHandler(txtPurchaseOrder, New DataObjectPastingEventHandler(AddressOf ProductController.IntegerOnlyPasteHandler))
                    Grid.SetColumn(txtPurchaseOrder, 1)

                    purchaseOrderGrid.Children.Add(purchaseOrderIcon)
                    purchaseOrderGrid.Children.Add(txtPurchaseOrder)
                    purchaseOrderBorder.Child = purchaseOrderGrid
                    purchaseOrderInnerPanel.Children.Add(purchaseOrderBorder)
                    purchaseOrderPanel.Children.Add(purchaseOrderInnerPanel)
                    stackPanel.Children.Add(purchaseOrderPanel)

                    ' Tax Rate
                    Dim taxRatePanel As New System.Windows.Controls.StackPanel With {.Margin = New Thickness(0, 0, 0, 10), .Name = "StackPanelTaxRate"}
                    Dim taxRateHeaderPanel As New System.Windows.Controls.StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
                    taxRateHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "Default Tax Rate:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)})
                    taxRateHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "*", .FontSize = 14, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636")), .FontWeight = FontWeights.Bold})
                    taxRatePanel.Children.Add(taxRateHeaderPanel)

                    Dim taxRateBorder As New Border With {.Style = CType(Application.Current.FindResource("RoundedBorderStyle"), Style)}
                    Dim taxRateGrid As New System.Windows.Controls.Grid()
                    taxRateGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
                    taxRateGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

                    Dim txtDefaultTax As New System.Windows.Controls.TextBox With {
                        .Name = "TxtDefaultTax",
                        .Style = CType(Application.Current.FindResource("RoundedTextboxStyle"), Style),
                        .Margin = New Thickness(25, 0, 0, 0),
                        .Text = "12"
                    }
                    AddHandler txtDefaultTax.PreviewTextInput, AddressOf ProductController.IntegerOnlyTextInputHandler
                    DataObject.AddPastingHandler(txtDefaultTax, New DataObjectPastingEventHandler(AddressOf ProductController.IntegerOnlyPasteHandler))
                    Grid.SetColumn(txtDefaultTax, 0)

                    Dim taxRateIcon As New PackIcon With {
                        .Kind = PackIconKind.PercentOutline,
                        .Width = 25,
                        .Height = 20,
                        .Margin = New Thickness(0, 0, 10, 0),
                        .VerticalAlignment = VerticalAlignment.Center
                    }
                    Grid.SetColumn(taxRateIcon, 1)

                    taxRateGrid.Children.Add(txtDefaultTax)
                    taxRateGrid.Children.Add(taxRateIcon)
                    taxRateBorder.Child = taxRateGrid
                    taxRatePanel.Children.Add(taxRateBorder)
                    stackPanel.Children.Add(taxRatePanel)

                    ' Discount Rate
                    Dim discountRatePanel As New System.Windows.Controls.StackPanel With {.Margin = New Thickness(0, 10, 0, 0), .Name = "StackPanelDiscountRate"}
                    Dim discountRateHeaderPanel As New System.Windows.Controls.StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
                    discountRateHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "Default Discount Rate:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)})
                    discountRateHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "*", .FontSize = 14, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636")), .FontWeight = FontWeights.Bold})
                    discountRatePanel.Children.Add(discountRateHeaderPanel)

                    Dim discountRateBorder As New Border With {.Style = CType(Application.Current.FindResource("RoundedBorderStyle"), Style)}
                    Dim discountRateGrid As New System.Windows.Controls.Grid()
                    discountRateGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
                    discountRateGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

                    Dim txtDiscountRate As New System.Windows.Controls.TextBox With {
                        .Name = "TxtDiscountRate",
                        .Style = CType(Application.Current.FindResource("RoundedTextboxStyle"), Style),
                        .Margin = New Thickness(25, 0, 0, 0)
                    }
                    AddHandler txtDiscountRate.PreviewTextInput, AddressOf ProductController.IntegerOnlyTextInputHandler
                    DataObject.AddPastingHandler(txtDiscountRate, New DataObjectPastingEventHandler(AddressOf ProductController.IntegerOnlyPasteHandler))
                    Grid.SetColumn(txtDiscountRate, 0)

                    Dim discountRateIcon As New PackIcon With {
                        .Kind = PackIconKind.PercentOutline,
                        .Width = 25,
                        .Height = 20,
                        .Margin = New Thickness(0, 0, 10, 0),
                        .VerticalAlignment = VerticalAlignment.Center
                    }
                    Grid.SetColumn(discountRateIcon, 1)

                    discountRateGrid.Children.Add(txtDiscountRate)
                    discountRateGrid.Children.Add(discountRateIcon)
                    discountRateBorder.Child = discountRateGrid
                    discountRatePanel.Children.Add(discountRateBorder)
                    stackPanel.Children.Add(discountRatePanel)

                    ' Stock Units
                    Dim stockUnitsPanel As New System.Windows.Controls.StackPanel With {.Margin = New Thickness(0, 10, 0, 0), .Name = "StackPanelStockUnits"}
                    Dim stockUnitsHeaderPanel As New System.Windows.Controls.StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
                    stockUnitsHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "Stock Units:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)})
                    stockUnitsPanel.Children.Add(stockUnitsHeaderPanel)

                    Dim borderStockUnits As New Border With {
                        .Style = CType(Application.Current.FindResource("RoundedBorderStyle"), Style),
                        .Name = "BorderStockUnits"
                    }
                    Dim txtStockUnits As New System.Windows.Controls.TextBox With {
                        .Name = "TxtStockUnits",
                        .Style = CType(Application.Current.FindResource("RoundedTextboxStyle"), Style),
                        .Text = "1"
                    }
                    AddHandler txtStockUnits.PreviewTextInput, AddressOf ProductController.IntegerOnlyTextInputHandler
                    DataObject.AddPastingHandler(txtStockUnits, New DataObjectPastingEventHandler(AddressOf ProductController.IntegerOnlyPasteHandler))
                    AddHandler txtStockUnits.KeyDown, Sub(sender, e)
                                                          ProductController.HandleStockUnitsKeyDown(txtStockUnits, MainContainer, e)
                                                      End Sub

                    ProductController.TxtStockUnits = txtStockUnits

                    borderStockUnits.Child = txtStockUnits
                    stockUnitsPanel.Children.Add(borderStockUnits)
                    stackPanel.Children.Add(stockUnitsPanel)

                    ' Alert Quantity
                    Dim alertQuantityPanel As New System.Windows.Controls.StackPanel With {.Margin = New Thickness(0, 10, 0, 0), .Name = "StackPanelAlertQuantity"}
                    Dim alertQuantityHeaderPanel As New System.Windows.Controls.StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0)}
                    alertQuantityHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "Alert Quantity:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)})
                    alertQuantityHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "*", .FontSize = 14, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636")), .FontWeight = FontWeights.Bold})
                    alertQuantityPanel.Children.Add(alertQuantityHeaderPanel)

                    Dim alertQuantityBorder As New Border With {.Style = CType(Application.Current.FindResource("RoundedBorderStyle"), Style)}
                    Dim txtAlertQuantity As New System.Windows.Controls.TextBox With {
                        .Name = "TxtAlertQuantity",
                        .Style = CType(Application.Current.FindResource("RoundedTextboxStyle"), Style)
                    }
                    AddHandler txtAlertQuantity.PreviewTextInput, AddressOf ProductController.IntegerOnlyTextInputHandler
                    DataObject.AddPastingHandler(txtAlertQuantity, New DataObjectPastingEventHandler(AddressOf ProductController.IntegerOnlyPasteHandler))

                    alertQuantityBorder.Child = txtAlertQuantity
                    alertQuantityPanel.Children.Add(alertQuantityBorder)
                    stackPanel.Children.Add(alertQuantityPanel)

                    scrollViewer.Content = stackPanel
                    container.Content = scrollViewer

                Case "serialnumber"
                    ' Create the serial number content
                    Dim stackPanel As New StackPanel With {.Name = "OuterStackPanel"}
                    Grid.SetRow(stackPanel, 4)

                    ' Serial Checkbox
                    Dim serialCheckboxPanel As New StackPanel With {
        .Margin = New Thickness(0, 10, 0, 10),
        .Name = "StackPanelSerialNumber"
    }

                    Dim checkboxStack As New StackPanel With {.Orientation = Orientation.Horizontal}
                    Dim checkBoxSerialNumber As New CheckBox With {
        .Name = "CheckBoxSerialNumber"
    }

                    checkboxStack.Children.Add(checkBoxSerialNumber)
                    checkboxStack.Children.Add(New TextBlock With {
        .Text = "Include Serial Number:",
        .FontSize = 14,
        .FontWeight = FontWeights.SemiBold,
        .Margin = New Thickness(10, 0, 0, 0)
    })

                    serialCheckboxPanel.Children.Add(checkboxStack)
                    stackPanel.Children.Add(serialCheckboxPanel)

                    ' Serial Number Row
                    Dim serialRowPanel = New StackPanel With {
        .Margin = New Thickness(0, 10, 0, 0),
        .Name = "StackPanelSerialRow"
    }

                    Dim headerBorder As New Border With {
        .Style = CType(Application.Current.FindResource("RoundedBorderStyle"), Style),
        .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
        .BorderThickness = New Thickness(0),
        .CornerRadius = New CornerRadius(15, 15, 0, 0)
    }

                    Dim headerPanel As New StackPanel With {
        .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
        .Orientation = Orientation.Horizontal,
        .Margin = New Thickness(20, 10, 20, 10)
    }

                    headerPanel.Children.Add(New TextBlock With {
        .Text = "Serial Number:",
        .Foreground = Brushes.White,
        .FontSize = 14,
        .FontWeight = FontWeights.SemiBold,
        .Margin = New Thickness(0, 0, 5, 0)
    })

                    headerPanel.Children.Add(New TextBlock With {
        .Text = "*",
        .FontSize = 14,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636")),
        .FontWeight = FontWeights.Bold
    })

                    headerBorder.Child = headerPanel
                    serialRowPanel.Children.Add(headerBorder)

                    ' Main container for serial numbers
                    Dim mainContainer As New StackPanel With {
        .Name = "MainContainer",
        .Background = Brushes.White
    }

                    ProductController.MainContainer = mainContainer
                    If ProductController.SerialNumbers Is Nothing Then
                        ProductController.SerialNumbers = New List(Of TextBox)()
                    End If

                    ' Add an initial serial number row if needed
                    If mainContainer.Children.Count = 0 AndAlso ProductController.TxtStockUnits IsNot Nothing AndAlso
        (String.IsNullOrEmpty(ProductController.TxtStockUnits.Text) OrElse CInt(ProductController.TxtStockUnits.Text) > 0) Then
                        ProductController.BtnAddRow_Click(Nothing, Nothing)
                    End If

                    AddHandler checkBoxSerialNumber.Click, Sub()
                                                               ProductController.SerialNumberChecker(checkBoxSerialNumber, mainContainer, TxtStockUnits, headerBorder)
                                                           End Sub

                    serialRowPanel.Children.Add(mainContainer)
                    stackPanel.Children.Add(serialRowPanel)

                    container.Content = stackPanel
            End Select
            Return container
        End Function
    End Class
End Namespace
