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
    Public Class ProductVariationDetails
        Inherits Window

        ' Add class-level declarations for all the missing controls
        Private variationManager As ProductVariationManager
        Private BorderStockUnits As Border
        Private CheckBoxSerialNumber As System.Windows.Controls.CheckBox
        Private MainContainer As System.Windows.Controls.StackPanel
        Private StackPanelSerialRow As Grid
        Private TxtStockUnits As System.Windows.Controls.TextBox
        Private serialRowPanel As New Grid With {.Name = "StackPanelSerialRow"}
        Public Shared SerialNumbers As New List(Of TextBox)()
        Private _comboBoxWarehouse As System.Windows.Controls.ComboBox

        Public Sub New()
            InitializeComponent()

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNav

            Dim calendarViewModel As New CalendarController.SingleCalendar()
            Me.DataContext = calendarViewModel

            variationManager = New ProductVariationManager()



            ' Load variations panel
            LoadVariationCombinations()

            ' Initialize dynamic content
            InitializeVariations()
        End Sub

        Private Sub BtnBatchEdit(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim OpenBatchEdit As New ProductBatchEdit()

            Me.Close()
            OpenBatchEdit.Show()
        End Sub
        ' Function to handle integer only input on textboxes
        Private Sub IntegerOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            ProductController.IntegerOnlyTextInputHandler(sender, e)
        End Sub

        Private Sub TxtStockUnits_KeyDown(sender As Object, e As KeyEventArgs)
            ' Call the centralized method in ProductController
            Dim handled As Boolean = ProductController.HandleStockUnitsKeyDown(TxtStockUnits, MainContainer, e)
            If handled Then
                e.Handled = True
            End If

            ' Any additional code specific to this form (if needed)
            ' Update the current variation's data
            If Not String.IsNullOrEmpty(variationManager.CurrentCombination) AndAlso e.Key = Key.Enter Then
                Dim variationData As ProductVariationData = variationManager.GetVariationData(variationManager.CurrentCombination)
                If variationData IsNot Nothing Then
                    If Integer.TryParse(TxtStockUnits.Text, Nothing) Then
                        variationData.StockUnits = Integer.Parse(TxtStockUnits.Text)
                    End If
                End If
            End If
        End Sub

        Public Function HandleStockUnitsKeyDown(stockUnitsTextBox As System.Windows.Controls.TextBox,
                                      mainContainer As System.Windows.Controls.StackPanel,
                                      e As KeyEventArgs) As Boolean
            ' General input validation logic
            If e.Key = Key.Enter Then
                ' Logic for updating UI elements based on stock unit changes
                If Not String.IsNullOrEmpty(stockUnitsTextBox.Text) AndAlso Integer.TryParse(stockUnitsTextBox.Text, Nothing) Then
                    Dim stockUnits As Integer = Integer.Parse(stockUnitsTextBox.Text)
                    ' Additional logic for updating UI components
                    Return True ' Event was handled
                End If
            End If

            Return False ' Event was not handled
        End Function

        Private Sub IntegerOnlyPasteHandler(sender As Object, e As DataObjectPastingEventArgs)
            ProductController.IntegerOnlyPasteHandler(sender, e)
        End Sub

        Private Sub IncludeSerial_Click(sender As Object, e As RoutedEventArgs)
            ' Call the centralized method in ProductController
            ProductController.SerialNumberChecker(CheckBoxSerialNumber,
                                         serialRowPanel,
                                         TxtStockUnits,
                                         BorderStockUnits,
                                         MainContainer)
        End Sub

        ' Handles the serial table components
        Private Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs)
            ' Simply call the ProductController's implementation
            ProductController.BtnAddRow_Click(Nothing, Nothing)

            ' Update the stock units textbox to match row count
            If MainContainer IsNot Nothing AndAlso TxtStockUnits IsNot Nothing Then
                TxtStockUnits.Text = MainContainer.Children.Count.ToString()
            End If
        End Sub

        Private Sub BtnRemoveRow_Click(sender As Object, e As RoutedEventArgs)
            ProductController.BtnRemoveRow_Click(Nothing, Nothing)
        End Sub



        'loading variations and combining options
        ' Replace the existing LoadVariationCombinations method with:
        Public Sub LoadVariationCombinations()
            Dim variationsPanel As StackPanel = FindName("VariationsPanel")
            If variationsPanel Is Nothing Then
                ' Create the panel if it doesn't exist
                variationsPanel = New StackPanel()
                variationsPanel.Name = "VariationsPanel"
                variationsPanel.Orientation = Orientation.Horizontal

                ' Add to parent container (same code as before)
                Dim currentPanel = FindName("StackPanel1")
                If currentPanel IsNot Nothing AndAlso TypeOf currentPanel.Parent Is Grid Then
                    Dim parentGrid As Grid = DirectCast(currentPanel.Parent, Grid)
                    Dim row As Integer = Grid.GetRow(currentPanel)
                    Dim column As Integer = Grid.GetColumn(currentPanel)

                    parentGrid.Children.Remove(currentPanel)
                    Grid.SetRow(variationsPanel, row)
                    Grid.SetColumn(variationsPanel, column)
                    parentGrid.Children.Add(variationsPanel)
                End If
            End If

            ' Call the ProductController method to load the variations
            ProductController.LoadVariationCombinations(variationsPanel, AddressOf VariationButton_Click)
        End Sub

        ' Add a click handler method for variation buttons
        Private Sub VariationButton_Click(sender As Object, e As RoutedEventArgs)
            Dim btn As System.Windows.Controls.Button = DirectCast(sender, System.Windows.Controls.Button)
            Dim combinationName As String = TryCast(btn.Tag, String)

            ' Update the UI to show this variation is selected
            Dim variationsPanel As StackPanel = FindName("VariationsPanel")
            If variationsPanel IsNot Nothing Then
                ProductController.UpdateVariationSelection(variationsPanel, btn)
            End If

            ' Load the specific variation data
            LoadVariationDetails(combinationName)
        End Sub



        ' Helper method to add a variation button with consistent styling
        Private Sub AddVariationButton(container As StackPanel, labelText As String, isSelected As Boolean)
            Dim btn As New System.Windows.Controls.Button With {
            .Style = CType(FindResource("RoundedButtonStyle"), Style),
            .Width = Double.NaN,  ' Auto width
            .Height = Double.NaN, ' Auto height
            .Background = Brushes.Transparent,
            .HorizontalAlignment = HorizontalAlignment.Left,
            .BorderThickness = New Thickness(0),
            .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(0, 0, 15, 0)
        }

            ' Create the Grid layout for the button content
            Dim grid As New Grid()
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            ' Add vertical line if selected
            Dim border As New Border With {
            .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#555555")),
            .BorderThickness = New Thickness(1, 0, 0, 0),
            .Width = 1,
            .Height = Double.NaN,  ' Auto height
            .Margin = New Thickness(0),
            .Visibility = If(isSelected, Visibility.Visible, Visibility.Collapsed)
        }
            Grid.SetColumn(border, 0)
            grid.Children.Add(border)

            ' Add the text
            Dim textBlock As New TextBlock With {
            .Text = labelText,
            .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString(If(isSelected, "#555555", "#AEAEAE"))),
            .FontSize = 14,
            .FontWeight = FontWeights.SemiBold,
            .Margin = New Thickness(5, 0, 0, 0),
            .VerticalAlignment = VerticalAlignment.Center
        }
            Grid.SetColumn(textBlock, 1)
            grid.Children.Add(textBlock)

            ' Set the grid as button content
            btn.Content = grid

            ' Add click handler to select this variation
            AddHandler btn.Click, Sub(sender, e)
                                      ' Update UI to show this variation is selected
                                      For Each child As UIElement In container.Children
                                          If TypeOf child Is System.Windows.Controls.Button Then
                                              Dim childBtn As System.Windows.Controls.Button = DirectCast(child, System.Windows.Controls.Button)
                                              Dim childGrid As Grid = TryCast(childBtn.Content, Grid)
                                              If childGrid IsNot Nothing Then
                                                  ' Update the border visibility and text color for all buttons
                                                  For Each gridChild As UIElement In childGrid.Children
                                                      If TypeOf gridChild Is Border Then
                                                          DirectCast(gridChild, Border).Visibility = If(child Is sender, Visibility.Visible, Visibility.Collapsed)
                                                      ElseIf TypeOf gridChild Is TextBlock Then
                                                          DirectCast(gridChild, TextBlock).Foreground = New SolidColorBrush(
                                                          ColorConverter.ConvertFromString(If(child Is sender, "#555555", "#AEAEAE")))
                                                      End If
                                                  Next
                                              End If
                                          End If
                                      Next

                                      ' Load the specific variation data here
                                      LoadVariationDetails(labelText)
                                  End Sub

            ' Add to container
            container.Children.Add(btn)
        End Sub

        ' Method to load details for selected variation
        Private Sub LoadVariationDetails(combinationName As String)
            ' Get current values before switching (save current data)
            If Not String.IsNullOrEmpty(variationManager.CurrentCombination) Then
                SaveCurrentFormData(variationManager.CurrentCombination)
            End If

            ' Select the new combination
            variationManager.SelectVariationCombination(combinationName)

            ' Create and set up dynamic containers if they don't exist yet
            If DynamicFormContainer.Content Is Nothing Then
                DynamicFormContainer.Content = CreateDynamicContainer("dynamicform")
            End If

            If SerialNumberContainer.Content Is Nothing Then
                SerialNumberContainer.Content = CreateDynamicContainer("serialnumber")
            End If

            ' Load data for the selected combination
            LoadFormData(combinationName)

            ' Update the selection title
            Dim titleTextBlock As TextBlock = TryCast(FindName("SelectedVariationTitle"), TextBlock)
            If titleTextBlock IsNot Nothing Then
                titleTextBlock.Text = $"Selected: {combinationName}"
            End If
        End Sub

        ' To be called when the form loads
        Public Sub InitializeVariations()
            ' Create dynamic containers
            DynamicFormContainer.Content = CreateDynamicContainer("dynamicform")
            SerialNumberContainer.Content = CreateDynamicContainer("serialnumber")

            ' Initialize warehouse dropdown and other controls
            Dim comboBox As System.Windows.Controls.ComboBox = FindVisualChild(Of System.Windows.Controls.ComboBox)(DynamicFormContainer, "ComboBoxWarehouse")
            If comboBox IsNot Nothing Then
                ProductController.GetWarehouse(comboBox)
            End If

            ' Set up other references
            Dim mainContainer As System.Windows.Controls.StackPanel = FindVisualChild(Of System.Windows.Controls.StackPanel)(SerialNumberContainer, "MainContainer")
            If mainContainer IsNot Nothing Then
                ProductController.MainContainer = mainContainer
            End If

            Dim txtStockUnits As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtStockUnits")
            If txtStockUnits IsNot Nothing Then
                ProductController.TxtStockUnits = txtStockUnits
                AddHandler txtStockUnits.KeyDown, AddressOf TxtStockUnits_KeyDown
            End If

            ' Add an initial serial number row
            If ProductController.SerialNumbers.Count = 0 Then
                ProductController.BtnAddRow_Click(Nothing, Nothing)
            End If

            ' Set default tax value
            Dim txtDefaultTax As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtDefaultTax")
            If txtDefaultTax IsNot Nothing Then
                txtDefaultTax.Text = "12"
            End If

            ' Call this during form initialization
            LoadVariationCombinations()

            ' Set the first variation as selected by default
            Dim variations As List(Of ProductVariation) = ProductController._savedVariations
            If variations IsNot Nothing AndAlso variations.Count > 0 Then
                Dim allCombinations As New List(Of String)

                If variations.Count = 1 Then
                    ' Single variation case
                    Dim variation As ProductVariation = variations(0)
                    If variation.Options IsNot Nothing AndAlso variation.Options.Count > 0 Then
                        For Each opt As VariationOption In variation.Options
                            allCombinations.Add(opt.OptionName)
                        Next
                    End If
                ElseIf variations.Count = 2 Then
                    ' Two variations case - create combinations
                    Dim variation1 As ProductVariation = variations(0)
                    Dim variation2 As ProductVariation = variations(1)

                    If variation1.Options IsNot Nothing AndAlso variation1.Options.Count > 0 AndAlso
                   variation2.Options IsNot Nothing AndAlso variation2.Options.Count > 0 Then
                        For Each option1 As VariationOption In variation1.Options
                            For Each option2 As VariationOption In variation2.Options
                                ' Create combination label: "Color, Size"
                                Dim combinationName As String = $"{option1.OptionName}, {option2.OptionName}"
                                allCombinations.Add(combinationName)
                            Next
                        Next
                    End If
                End If

                ' Initialize each variation with default values
                For Each combination As String In allCombinations
                    Dim variationData As ProductVariationData = variationManager.GetVariationData(combination)
                    If variationData.StockUnits = 0 Then
                        variationData.StockUnits = 1 ' Default to 1 stock unit
                    End If
                Next
            End If
        End Sub


        Private Function FindVisualChild(Of T As DependencyObject)(parent As DependencyObject, name As String) As T
            For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(parent) - 1
                Dim child As DependencyObject = VisualTreeHelper.GetChild(parent, i)

                ' Check if this is the element we're looking for
                If TypeOf child Is T AndAlso (TryCast(child, FrameworkElement)).Name = name Then
                    Return DirectCast(child, T)
                End If

                ' Search in this child's children
                Dim result As T = FindVisualChild(Of T)(child, name)
                If result IsNot Nothing Then
                    Return result
                End If
            Next

            Return Nothing
        End Function

        Private Sub SaveCurrentFormData(combinationName As String)
            ' Find required controls
            Dim txtRetailPrice As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtRetailPrice")
            Dim txtPurchaseOrder As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtPurchaseOrder")
            Dim txtDefaultTax As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtDefaultTax")
            Dim txtDiscountRate As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtDiscountRate")
            Dim txtStockUnits As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtStockUnits")
            Dim txtAlertQuantity As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtAlertQuantity")
            Dim checkBoxSerialNumber As System.Windows.Controls.CheckBox = FindVisualChild(Of System.Windows.Controls.CheckBox)(SerialNumberContainer, "CheckBoxSerialNumber")
            Dim mainContainer As StackPanel = FindVisualChild(Of StackPanel)(SerialNumberContainer, "MainContainer")

            ' Call the controller method to save the data
            ProductController.SaveVariationData(combinationName, txtRetailPrice, txtPurchaseOrder,
                                      txtDefaultTax, txtDiscountRate, txtStockUnits,
                                      txtAlertQuantity, checkBoxSerialNumber,
                                      _comboBoxWarehouse, mainContainer)
        End Sub

        Private Sub LoadFormData(combinationName As String)
            ' Call the controller method to load the data
            ProductController.LoadVariationData(combinationName,
                                      FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtRetailPrice"),
                                      FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtPurchaseOrder"),
                                      FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtDefaultTax"),
                                      FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtDiscountRate"),
                                      FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtStockUnits"),
                                      FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtAlertQuantity"),
                                      FindVisualChild(Of System.Windows.Controls.CheckBox)(SerialNumberContainer, "CheckBoxSerialNumber"),
                                      _comboBoxWarehouse,
                                      FindVisualChild(Of StackPanel)(SerialNumberContainer, "MainContainer"))

            ' Update the selection title (this part stays in the UI class)
            Dim titleTextBlock As TextBlock = TryCast(FindName("SelectedVariationTitle"), TextBlock)
            If titleTextBlock IsNot Nothing Then
                titleTextBlock.Text = $"Selected: {combinationName}"
            End If
        End Sub

        Private Sub BtnSave_Click(sender As Object, e As RoutedEventArgs)
            Try
                ' Save current variation data before proceeding
                If Not String.IsNullOrEmpty(variationManager.CurrentCombination) Then
                    SaveCurrentFormData(variationManager.CurrentCombination)
                End If

                ' Get all variation data
                Dim allVariationData As Dictionary(Of String, ProductVariationData) = GetAllVariationData()

                ' Validate required fields
                If Not ValidateForm() Then
                    Return
                End If

                ' TODO: Save to database or perform other actions with the variation data
                MessageBox.Show("Product variations saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                ' Navigate back to product list or another appropriate screen
                Dim mainProductsWindow As New Views.Stocks.ItemManager.NewProduct.AddNewProducts()
                mainProductsWindow.Show()
                Me.Close()
            Catch ex As Exception
                MessageBox.Show("An error occurred while saving: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
        Private Function ValidateForm() As Boolean
            Return ProductController.ValidateForm(
        FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtRetailPrice"),
        FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtPurchaseOrder"),
        _comboBoxWarehouse,
        FindVisualChild(Of System.Windows.Controls.CheckBox)(SerialNumberContainer, "CheckBoxSerialNumber"))
        End Function

        ' Add a serial number row
        Private Sub AddSerialRow(Optional initialValue As String = "")
            ' Simply forward the call to ProductController
            ProductController.BtnAddRow_Click(Nothing, Nothing)

            ' If an initial value was provided, set it on the last added textbox
            If Not String.IsNullOrEmpty(initialValue) AndAlso ProductController.SerialNumbers IsNot Nothing AndAlso ProductController.SerialNumbers.Count > 0 Then
                ProductController.SerialNumbers(ProductController.SerialNumbers.Count - 1).Text = initialValue
            End If

            ' Update the stock units textbox to match row count
            If TxtStockUnits IsNot Nothing AndAlso MainContainer IsNot Nothing Then
                TxtStockUnits.Text = MainContainer.Children.Count.ToString()
            End If
        End Sub

        ' Remove a serial number row
        Private Sub BtnRemove_Click(sender As Object, e As RoutedEventArgs)
            Dim button As System.Windows.Controls.Button = TryCast(sender, System.Windows.Controls.Button)
            If button Is Nothing Then Return

            ' Find the parent grid and remove it
            Dim grid As Grid = TryCast(button.Parent, Grid)
            If grid Is Nothing Then Return

            Dim mainContainer As System.Windows.Controls.StackPanel = TryCast(grid.Parent, System.Windows.Controls.StackPanel)
            If mainContainer Is Nothing Then Return

            ' Find the textbox to remove from tracking
            Dim txtToRemove As System.Windows.Controls.TextBox = Nothing
            For Each child As UIElement In grid.Children
                If TypeOf child Is System.Windows.Controls.TextBox Then
                    txtToRemove = DirectCast(child, System.Windows.Controls.TextBox)
                    Exit For
                End If
            Next

            ' Remove from tracking
            If txtToRemove IsNot Nothing AndAlso ProductController.SerialNumbers IsNot Nothing Then
                ProductController.SerialNumbers.Remove(txtToRemove)
            End If

            ' Remove from UI
            mainContainer.Children.Remove(grid)

            ' Update the stock units textbox to match row count
            If TxtStockUnits IsNot Nothing Then
                TxtStockUnits.Text = mainContainer.Children.Count.ToString()
            End If
        End Sub

        Private Sub BtnBack(sender As Object, e As RoutedEventArgs)
            ' Notify the parent window to update its variation text
            Dim parentWindow = Window.GetWindow(Me)
            If parentWindow IsNot Nothing AndAlso TypeOf parentWindow Is DPC.Views.Stocks.ItemManager.NewProduct.ProductVariationDetails Then
                Dim addNewProductsWindow = DirectCast(parentWindow, DPC.Views.Stocks.ItemManager.NewProduct.ProductVariationDetails)
            End If

            Dim VariationDetails As New Views.Stocks.ItemManager.NewProduct.AddNewProducts()
            VariationDetails.Show()

            Dim currentWindow As Window = Window.GetWindow(Me)
            currentWindow?.Close()
        End Sub

        Protected Overrides Sub OnClosing(e As System.ComponentModel.CancelEventArgs)
            ' Save current form data before closing
            If Not String.IsNullOrEmpty(variationManager.CurrentCombination) Then
                SaveCurrentFormData(variationManager.CurrentCombination)
            End If

            MyBase.OnClosing(e)
        End Sub

        Public Function GetAllVariationData() As Dictionary(Of String, ProductVariationData)
            ' Make sure current form data is saved
            If Not String.IsNullOrEmpty(variationManager.CurrentCombination) Then
                SaveCurrentFormData(variationManager.CurrentCombination)
            End If

            Return variationManager.GetAllVariationData()
        End Function

        Private Function CreateDynamicContainer(containerType As String) As ContentControl
            Dim container As New ContentControl()

            Select Case containerType
                Case "dynamicform"
                    ' Create the dynamic form content
                    Dim scrollViewer As New System.Windows.Controls.ScrollViewer With {
                    .VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                    .HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled,
                    .VerticalAlignment = VerticalAlignment.Stretch,
                    .Style = CType(FindResource("ModernScrollViewerStyle"), Style)
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
        .Style = CType(FindResource("RoundedComboBoxStyle"), Style),
        .Width = Double.NaN,
        .Height = 40,
        .Margin = New Thickness(0)
    }

                    ' Add the ComboBox to the parent panel
                    warehousePanel.Children.Add(comboBoxWarehouse)
                    stackPanel.Children.Add(warehousePanel)

                    ' Initialize the ComboBox right after creating it to ensure it's correctly set up
                    Try
                        ProductController.GetWarehouse(comboBoxWarehouse)
                    Catch ex As Exception
                        MessageBox.Show("Error initializing ComboBox: " & ex.Message)
                    End Try
                    _comboBoxWarehouse = comboBoxWarehouse


                    ' Retail Price
                    Dim retailPricePanel As New System.Windows.Controls.StackPanel With {.Margin = New Thickness(0, 10, 0, 0), .Name = "StackPanelRetailPrice"}
                    Dim retailPriceInnerPanel As New System.Windows.Controls.StackPanel()
                    Dim retailPriceHeaderPanel As New System.Windows.Controls.StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0, 0, 0, 5)}
                    retailPriceHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "Selling Price:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)})
                    retailPriceHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "*", .FontSize = 14, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636")), .FontWeight = FontWeights.Bold})
                    retailPriceInnerPanel.Children.Add(retailPriceHeaderPanel)

                    Dim retailPriceBorder As New Border With {.Style = CType(FindResource("RoundedBorderStyle"), Style)}
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
                    .Style = CType(FindResource("RoundedTextboxStyle"), Style),
                    .Margin = New Thickness(0, 0, 25, 0)
                }
                    AddHandler txtRetailPrice.PreviewTextInput, AddressOf IntegerOnlyTextInputHandler
                    ' Fix DataObject.Pasting by using a separate method for attaching the event handler
                    DataObject.AddPastingHandler(txtRetailPrice, New DataObjectPastingEventHandler(AddressOf IntegerOnlyPasteHandler))
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

                    Dim purchaseOrderBorder As New Border With {.Style = CType(FindResource("RoundedBorderStyle"), Style)}
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
                    .Style = CType(FindResource("RoundedTextboxStyle"), Style),
                    .Margin = New Thickness(0, 0, 25, 0)
                }
                    AddHandler txtPurchaseOrder.PreviewTextInput, AddressOf IntegerOnlyTextInputHandler
                    ' Fix DataObject.Pasting
                    DataObject.AddPastingHandler(txtPurchaseOrder, New DataObjectPastingEventHandler(AddressOf IntegerOnlyPasteHandler))
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

                    Dim taxRateBorder As New Border With {.Style = CType(FindResource("RoundedBorderStyle"), Style)}
                    Dim taxRateGrid As New System.Windows.Controls.Grid()
                    taxRateGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
                    taxRateGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

                    Dim txtDefaultTax As New System.Windows.Controls.TextBox With {
                    .Name = "TxtDefaultTax",
                    .Style = CType(FindResource("RoundedTextboxStyle"), Style),
                    .Margin = New Thickness(25, 0, 0, 0),
                    .Text = "12"
                }
                    AddHandler txtDefaultTax.PreviewTextInput, AddressOf IntegerOnlyTextInputHandler
                    ' Fix DataObject.Pasting
                    DataObject.AddPastingHandler(txtDefaultTax, New DataObjectPastingEventHandler(AddressOf IntegerOnlyPasteHandler))
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

                    Dim discountRateBorder As New Border With {.Style = CType(FindResource("RoundedBorderStyle"), Style)}
                    Dim discountRateGrid As New System.Windows.Controls.Grid()
                    discountRateGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
                    discountRateGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

                    Dim txtDiscountRate As New System.Windows.Controls.TextBox With {
                    .Name = "TxtDiscountRate",
                    .Style = CType(FindResource("RoundedTextboxStyle"), Style),
                    .Margin = New Thickness(25, 0, 0, 0)
                }
                    AddHandler txtDiscountRate.PreviewTextInput, AddressOf IntegerOnlyTextInputHandler
                    ' Fix DataObject.Pasting
                    DataObject.AddPastingHandler(txtDiscountRate, New DataObjectPastingEventHandler(AddressOf IntegerOnlyPasteHandler))
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

                    BorderStockUnits = New Border With {
                    .Style = CType(FindResource("RoundedBorderStyle"), Style),
                    .Name = "BorderStockUnits"
                }
                    TxtStockUnits = New System.Windows.Controls.TextBox With {
                    .Name = "TxtStockUnits",
                    .Style = CType(FindResource("RoundedTextboxStyle"), Style)
                }
                    AddHandler TxtStockUnits.PreviewTextInput, AddressOf IntegerOnlyTextInputHandler
                    DataObject.AddPastingHandler(TxtStockUnits, New DataObjectPastingEventHandler(AddressOf IntegerOnlyPasteHandler))

                    ProductController.TxtStockUnits = TxtStockUnits
                    AddHandler TxtStockUnits.KeyDown, AddressOf TxtStockUnits_KeyDown

                    BorderStockUnits.Child = TxtStockUnits
                    stockUnitsPanel.Children.Add(BorderStockUnits)
                    stackPanel.Children.Add(stockUnitsPanel)

                    ' Alert Quantity
                    Dim alertQuantityPanel As New System.Windows.Controls.StackPanel With {.Margin = New Thickness(0, 10, 0, 0), .Name = "StackPanelAlertQuantity"}
                    Dim alertQuantityHeaderPanel As New System.Windows.Controls.StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(0)}
                    alertQuantityHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "Alert Quantity:", .FontSize = 14, .FontWeight = FontWeights.SemiBold, .Margin = New Thickness(0, 0, 5, 0)})
                    alertQuantityHeaderPanel.Children.Add(New System.Windows.Controls.TextBlock With {.Text = "*", .FontSize = 14, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636")), .FontWeight = FontWeights.Bold})
                    alertQuantityPanel.Children.Add(alertQuantityHeaderPanel)

                    Dim alertQuantityBorder As New Border With {.Style = CType(FindResource("RoundedBorderStyle"), Style)}
                    Dim txtAlertQuantity As New System.Windows.Controls.TextBox With {
                    .Name = "TxtAlertQuantity",
                    .Style = CType(FindResource("RoundedTextboxStyle"), Style)
                }
                    AddHandler txtAlertQuantity.PreviewTextInput, AddressOf IntegerOnlyTextInputHandler
                    ' Fix DataObject.Pasting
                    DataObject.AddPastingHandler(txtAlertQuantity, New DataObjectPastingEventHandler(AddressOf IntegerOnlyPasteHandler))

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
                    Dim serialCheckboxPanel As New System.Windows.Controls.StackPanel With {
                            .Margin = New Thickness(0, 10, 0, 10),
                            .Name = "StackPanelSerialNumber"
                        }

                    Dim checkboxStack As New StackPanel With {.Orientation = Orientation.Horizontal}
                    CheckBoxSerialNumber = New System.Windows.Controls.CheckBox With {.Name = "CheckBoxSerialNumber"}
                    AddHandler CheckBoxSerialNumber.Click, AddressOf IncludeSerial_Click

                    checkboxStack.Children.Add(CheckBoxSerialNumber)
                    checkboxStack.Children.Add(New System.Windows.Controls.TextBlock With {
                            .Text = "Include Serial Number:",
                            .FontSize = 14,
                            .FontWeight = FontWeights.SemiBold,
                            .Margin = New Thickness(10, 0, 0, 0)
                        })

                    serialCheckboxPanel.Children.Add(checkboxStack)
                    stackPanel.Children.Add(serialCheckboxPanel)

                    ' Serial Number Row
                    Dim StackPanelSerialRow = New System.Windows.Controls.StackPanel With {
                            .Margin = New Thickness(0, 10, 0, 10),  ' Left, Top, Right, Bottom
                            .Name = "StackPanelSerialRow"
                        }

                    ' Header border with label
                    Dim headerBorder As New Border With {
                            .Style = CType(FindResource("RoundedBorderStyle"), Style),
                            .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
                            .BorderThickness = New Thickness(0),
                            .CornerRadius = New CornerRadius(15, 15, 0, 0)
                        }

                    Dim headerPanel As New System.Windows.Controls.StackPanel With {
                            .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
                            .Orientation = Orientation.Horizontal,
                            .Margin = New Thickness(20, 10, 20, 10)  ' Left, Top, Right, Bottom
                        }

                    headerPanel.Children.Add(New System.Windows.Controls.TextBlock With {
                            .Text = "Serial Number:",
                            .Foreground = Brushes.White,
                            .FontSize = 14,
                            .FontWeight = FontWeights.SemiBold,
                            .Margin = New Thickness(0, 0, 5, 0)
                        })

                    headerPanel.Children.Add(New System.Windows.Controls.TextBlock With {
                            .Text = "*",
                            .FontSize = 14,
                            .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636")),
                            .FontWeight = FontWeights.Bold
                        })

                    headerBorder.Child = headerPanel
                    StackPanelSerialRow.Children.Add(headerBorder)

                    ' Main container for serial numbers
                    MainContainer = New System.Windows.Controls.StackPanel With {
                            .Name = "MainContainer",
                            .Background = Brushes.White
                        }

                    ProductController.MainContainer = MainContainer
                    If ProductController.SerialNumbers Is Nothing Then
                        ProductController.SerialNumbers = New List(Of System.Windows.Controls.TextBox)()
                    End If

                    ' Add an initial serial number row if needed
                    If MainContainer.Children.Count = 0 AndAlso TxtStockUnits IsNot Nothing AndAlso
                            (String.IsNullOrEmpty(TxtStockUnits.Text) OrElse CInt(TxtStockUnits.Text) > 0) Then
                        ProductController.BtnAddRow_Click(Nothing, Nothing)
                    End If

                    StackPanelSerialRow.Children.Add(MainContainer)
                    stackPanel.Children.Add(StackPanelSerialRow)

                    container.Content = stackPanel
            End Select
            Return container
        End Function
    End Class
End Namespace