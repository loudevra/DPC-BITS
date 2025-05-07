Imports System.IO
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports MaterialDesignThemes.Wpf

Namespace DPC.Views.Stocks.ItemManager.NewProduct
    Public Class ProductVariationDetails
        Inherits UserControl

#Region "Initialization and Constructor"
        Public Sub New()
            InitializeComponent()

            ' Set DataContext only once
            Me.DataContext = ProductViewModel.Instance

            ' Populate the warehouse combo box with data from the database
            ProductController.GetWarehouse(ComboBoxWarehouse)

            ' Initialize serial number controls
            InitializeSerialNumberControls()

            ' Load variation data
            LoadVariationData()

            ' Setup event handlers
            AddHandler BtnSave.Click, AddressOf SaveVariationData
            AddHandler CheckBoxSerialNumber.Click, AddressOf SerialNumberCheckboxChanged
            AddHandler TxtStockUnits.TextChanged, AddressOf StockUnits_TextChanged
            AddHandler TxtPurchaseOrder.TextChanged, AddressOf PurchaseOrder_TextChanged
            AddHandler RadBtnPercentage.Checked, AddressOf RadioButton_Checked
            AddHandler RadBtnFlat.Checked, AddressOf RadioButton_Checked
        End Sub
#End Region

#Region "Variation Loading and Management"
        ''' <summary>
        ''' Loads saved variation data and generates all combinations
        ''' </summary>
        Private Sub LoadVariationData()
            ' Check if we have any saved variations
            If AddVariation.SavedVariations.Count = 0 Then
                ' No variations defined, show message or redirect
                MessageBox.Show("No product variations have been defined. Please define variations first.", "No Variations", MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If

            ' Generate all possible combinations (now preserves existing data)
            GenerateVariationCombinations()

            ' Create variation tabs
            CreateVariationTabs()

            ' Check if we have a previously selected variation to restore
            Dim lastSelectedVariation As String = TryCast(Application.Current.Properties("LastSelectedVariation"), String)

            Debug.WriteLine($"Attempting to restore last selected variation: {lastSelectedVariation}")

            If Not String.IsNullOrEmpty(lastSelectedVariation) Then
                ' Find and click the corresponding tab button
                Dim scrollViewer = TryCast(VariationsPanel.Children(0), ScrollViewer)
                If scrollViewer IsNot Nothing Then
                    Dim buttonPanel = TryCast(scrollViewer.Content, StackPanel)
                    If buttonPanel IsNot Nothing Then
                        For Each child In buttonPanel.Children
                            Dim tabBtn = TryCast(child, Button)
                            If tabBtn IsNot Nothing AndAlso tabBtn.Tag.ToString() = lastSelectedVariation Then
                                ' Simulate click on this tab
                                Debug.WriteLine($"Found matching tab for: {lastSelectedVariation}")
                                VariationTab_Click(tabBtn, New RoutedEventArgs())
                                Return
                            End If
                        Next
                        Debug.WriteLine($"Warning: Could not find matching tab for: {lastSelectedVariation}")
                    End If
                End If
            End If

            Debug.WriteLine("No last selected variation found or couldn't restore it, selecting first tab")
            ' If no previously selected variation or it wasn't found, select the first tab
            ' The automatic tab selection in CreateVariationTabs will handle this
        End Sub


        ''' <summary>
        ''' Generates all possible combinations of variation options
        ''' </summary>
        Private Sub GenerateVariationCombinations()
            ' Check if we have saved variations
            If AddVariation.SavedVariations.Count = 0 Then
                Return
            End If

            ' Extract all variation options
            Dim variationOptions As New List(Of List(Of String))()

            For Each variation As ProductVariation In AddVariation.SavedVariations
                Dim optionsList As New List(Of String)()

                ' Add all options for this variation
                For Each varOption As VariationOption In variation.Options
                    optionsList.Add(varOption.OptionName)
                Next

                variationOptions.Add(optionsList)
            Next

            ' Generate all combinations
            Dim combinations = GenerateCombinations(variationOptions)

            ' Store existing variations to preserve their values
            Dim existingVariations = ProductController.variationManager.GetAllVariationData()
            Debug.WriteLine($"Found {existingVariations.Count} existing variations")

            ' Create variation data entries for each combination
            For Each combo In combinations
                Dim combinationName = String.Join(", ", combo)

                ' Check if this combination already exists
                Dim variationExists As Boolean = existingVariations.ContainsKey(combinationName)

                If Not variationExists Then
                    Debug.WriteLine($"Adding new variation: {combinationName}")
                    ' Only add the combination if it doesn't already exist
                    ProductController.variationManager.AddVariationCombination(combinationName)

                    ' Get the newly added variation and set default values
                    Dim newVariation = ProductController.variationManager.GetVariationData(combinationName)
                    If newVariation IsNot Nothing Then
                        ' Set default values as specified
                        newVariation.RetailPrice = 0             ' Selling price set to 0
                        newVariation.PurchaseOrder = 0           ' Buying price set to 0
                        newVariation.DefaultTax = 12             ' Default tax rate set to 12
                        newVariation.DiscountRate = 0            ' Discount rate set to 0
                        newVariation.StockUnits = 1              ' Stock units set to 1
                        newVariation.AlertQuantity = 0           ' Alert quantity set to 0
                        newVariation.SelectedWarehouseIndex = 0  ' Warehouse set to index 1
                        newVariation.IncludeSerialNumbers = True ' Serial number checkbox checked by default

                        ' Set default markup values
                        newVariation.MarkupValue = 0            ' Default 20% markup
                        newVariation.IsPercentageMarkup = True   ' Use percentage markup by default

                        ' Initialize serial numbers list with one empty entry
                        If newVariation.IncludeSerialNumbers Then
                            newVariation.SerialNumbers = New List(Of String)()
                            newVariation.SerialNumbers.Add("")
                        End If
                    End If
                Else
                    Debug.WriteLine($"Variation already exists: {combinationName}")
                    ' Log what it currently has
                    Dim existingVariation = existingVariations(combinationName)
                    If existingVariation IsNot Nothing Then
                        Debug.WriteLine($"  StockUnits: {existingVariation.StockUnits}")
                        Debug.WriteLine($"  SerialNumberChecked: {existingVariation.IncludeSerialNumbers}")
                        Debug.WriteLine($"  SerialNumbers count: {If(existingVariation.SerialNumbers Is Nothing, "NULL", existingVariation.SerialNumbers.Count.ToString())}")
                        Debug.WriteLine($"  MarkupValue: {existingVariation.MarkupValue}")
                        Debug.WriteLine($"  IsPercentageMarkup: {existingVariation.IsPercentageMarkup}")
                    End If
                End If
            Next
        End Sub

        ''' <summary>
        ''' Recursive method to generate all possible combinations of options
        ''' </summary>
        Private Function GenerateCombinations(options As List(Of List(Of String)), Optional currentIndex As Integer = 0, Optional currentCombination As List(Of String) = Nothing) As List(Of List(Of String))
            If currentCombination Is Nothing Then
                currentCombination = New List(Of String)()
            End If

            Dim result As New List(Of List(Of String))()

            ' Base case: if we've processed all variations
            If currentIndex >= options.Count Then
                result.Add(New List(Of String)(currentCombination))
                Return result
            End If

            ' Recursive case: try each option for the current variation
            For Each varOption In options(currentIndex)
                currentCombination.Add(varOption)
                result.AddRange(GenerateCombinations(options, currentIndex + 1, currentCombination))
                currentCombination.RemoveAt(currentCombination.Count - 1)
            Next

            Return result
        End Function

        ''' <summary>
        ''' Creates tabs for each variation combination
        ''' </summary>
        Private Sub CreateVariationTabs()
            ' Clear any existing buttons
            VariationsPanel.Children.Clear()

            ' Create a ScrollViewer for horizontal scrolling
            Dim scrollViewer As New ScrollViewer With {
        .HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        .VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
        .Padding = New Thickness(0, 0, 0, 5),
        .Background = Brushes.Transparent
    }

            ' Create a StackPanel for the buttons
            Dim buttonPanel As New StackPanel With {
        .Orientation = Orientation.Horizontal,
        .Background = Brushes.Transparent
    }

            ' Add buttons for each variation combination
            Dim lastSelectedVariation As String = TryCast(Application.Current.Properties("LastSelectedVariation"), String)
            Dim buttonToSelect As Button = Nothing

            For Each kvp In ProductController.variationManager.GetAllVariationData()
                Dim combinationName = kvp.Key

                ' Create a Grid for the tab layout
                Dim grid As New Grid() With {
            .Background = Brushes.Transparent
        }

                ' Define columns: one narrow for the selection indicator, one for content
                grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(5)})
                grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})

                ' Create selection indicator rectangle
                Dim selectionIndicator As New Rectangle With {
            .Fill = New SolidColorBrush(ColorConverter.ConvertFromString("#555555")),
            .Visibility = Visibility.Collapsed
        }
                Grid.SetColumn(selectionIndicator, 0)
                grid.Children.Add(selectionIndicator)

                ' Create text block for the tab text with larger font size
                Dim textBlock As New TextBlock With {
            .Text = combinationName,
            .Margin = New Thickness(10, 0, 10, 0),
            .VerticalAlignment = VerticalAlignment.Center,
            .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#AAAAAA")),
            .FontSize = 14,
            .FontFamily = CType(FindResource("Lexend"), FontFamily)
        }
                Grid.SetColumn(textBlock, 1)
                grid.Children.Add(textBlock)

                ' Create a button for this variation with transparent background
                Dim btn As New Button With {
            .Content = grid,
            .BorderThickness = New Thickness(0),
            .Style = CType(FindResource("RoundedButtonStyle"), Style),
            .Tag = combinationName,
            .Background = Brushes.Transparent
        }

                ' Store references to the visual elements for later updating
                btn.Resources.Add("SelectionIndicator", selectionIndicator)
                btn.Resources.Add("TextBlock", textBlock)

                ' Add click handler
                AddHandler btn.Click, AddressOf VariationTab_Click

                ' Add to the panel
                buttonPanel.Children.Add(btn)

                ' Check if this is the last selected variation
                If combinationName = lastSelectedVariation Then
                    buttonToSelect = btn
                End If
            Next

            ' Set the ButtonPanel as the content of the ScrollViewer
            scrollViewer.Content = buttonPanel

            ' Add the ScrollViewer to the VariationsPanel
            VariationsPanel.Children.Add(scrollViewer)

            ' Ensure VariationsPanel has transparent background
            VariationsPanel.Background = Brushes.Transparent

            ' Select the appropriate tab
            If buttonToSelect IsNot Nothing Then
                Debug.WriteLine($"Found button for last selected variation: {lastSelectedVariation}")
                ' Use dispatcher to ensure UI is fully loaded before clicking
                Dispatcher.BeginInvoke(New Action(Sub()
                                                      VariationTab_Click(buttonToSelect, New RoutedEventArgs())
                                                  End Sub), DispatcherPriority.Render)
            ElseIf buttonPanel.Children.Count > 0 Then
                ' If no previously selected tab or not found, select the first tab
                Dim firstButton = TryCast(buttonPanel.Children(0), Button)
                If firstButton IsNot Nothing Then
                    Debug.WriteLine("Selecting first tab as default")
                    ' Use dispatcher to ensure UI is fully loaded before clicking
                    Dispatcher.BeginInvoke(New Action(Sub()
                                                          VariationTab_Click(firstButton, New RoutedEventArgs())
                                                      End Sub), DispatcherPriority.Render)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Event handler for variation tab clicks - modified to save serial numbers
        ''' </summary>
        Private Sub VariationTab_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                Dim combinationName = TryCast(btn.Tag, String)
                If Not String.IsNullOrEmpty(combinationName) Then
                    Debug.WriteLine($"Tab clicked: {combinationName}")

                    Try
                        ' Always save the current serial number values and variation data first
                        If ProductController.variationManager.GetCurrentVariationData() IsNot Nothing Then
                            Debug.WriteLine("Saving current variation data before tab change")
                            SaveSerialNumberValues()
                            SaveCurrentVariationData()
                        End If

                        ' Now select the new variation
                        SelectVariation(combinationName)

                        ' Also store this as the last selected variation
                        Application.Current.Properties("LastSelectedVariation") = combinationName
                        Debug.WriteLine($"Set last selected variation to: {combinationName}")

                        ' Reset all tabs to unselected state
                        Dim scrollViewer = TryCast(VariationsPanel.Children(0), ScrollViewer)
                        If scrollViewer IsNot Nothing Then
                            Dim buttonPanel = TryCast(scrollViewer.Content, StackPanel)
                            If buttonPanel IsNot Nothing Then
                                For Each child In buttonPanel.Children
                                    Dim tabBtn = TryCast(child, Button)
                                    If tabBtn IsNot Nothing Then
                                        ' Get references to the visual elements
                                        Dim indicator = TryCast(tabBtn.Resources("SelectionIndicator"), Rectangle)
                                        Dim text = TryCast(tabBtn.Resources("TextBlock"), TextBlock)

                                        ' Reset to default unselected style
                                        If indicator IsNot Nothing Then
                                            indicator.Visibility = Visibility.Collapsed
                                        End If
                                        If text IsNot Nothing Then
                                            text.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#AAAAAA"))
                                        End If
                                    End If
                                Next
                            End If
                        End If

                        ' Apply selected style to the clicked tab
                        Dim selectedIndicator = TryCast(btn.Resources("SelectionIndicator"), Rectangle)
                        Dim selectedText = TryCast(btn.Resources("TextBlock"), TextBlock)

                        If selectedIndicator IsNot Nothing Then
                            selectedIndicator.Visibility = Visibility.Visible
                        End If
                        If selectedText IsNot Nothing Then
                            selectedText.Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#555555"))
                        End If
                    Catch ex As Exception
                        Debug.WriteLine($"Error in tab click: {ex.Message}")
                        MessageBox.Show($"Error changing tabs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    End Try
                End If
            End If
        End Sub

        ''' <summary>
        ''' Selects a variation and displays its details
        ''' </summary>
        Private Sub SelectVariation(combinationName As String)
            ' Update the selected variation title  
            SelectedVariationTitle.Text = "Selected Variation: " & combinationName
            Debug.WriteLine($"Selecting variation: {combinationName}")

            ' Select this variation in the manager
            ProductController.variationManager.SelectVariationCombination(combinationName)

            ' Load the data into form fields
            LoadVariationFormData()

            ' Explicitly reset and then load serial numbers data
            If CheckBoxSerialNumber IsNot Nothing Then
                ' Update serial number checkbox state first
                Dim currentData = ProductController.variationManager.GetCurrentVariationData()
                If currentData IsNot Nothing Then
                    CheckBoxSerialNumber.IsChecked = currentData.IncludeSerialNumbers
                    Debug.WriteLine($"Serial Number Checkbox set to: {currentData.IncludeSerialNumbers}")
                End If
            End If

            ' Now load serial number data
            LoadSerialNumberData()

            ' Additional debugging output
            Dim data = ProductController.variationManager.GetCurrentVariationData()
            If data IsNot Nothing Then
                Debug.WriteLine($"Loaded variation data: {combinationName}")
                Debug.WriteLine($"  StockUnits: {data.StockUnits}")
                Debug.WriteLine($"  SerialNumberChecked: {data.IncludeSerialNumbers}")
                Debug.WriteLine($"  SerialNumbers count: {If(data.SerialNumbers Is Nothing, "NULL", data.SerialNumbers.Count.ToString())}")
            Else
                Debug.WriteLine($"WARNING: Could not get data for variation: {combinationName}")
            End If
        End Sub

        ''' <summary>
        ''' Loads the variation details into existing form fields
        ''' </summary>
        Private Sub LoadVariationFormData()
            ' Get the current variation data
            Dim currentData = ProductController.variationManager.GetCurrentVariationData()

            ' Set the data to the existing form fields
            If currentData IsNot Nothing Then
                ' Set existing fields
                TxtRetailPrice.Text = currentData.RetailPrice.ToString()
                TxtPurchaseOrder.Text = currentData.PurchaseOrder.ToString()
                TxtDefaultTax.Text = currentData.DefaultTax.ToString()
                TxtDiscountRate.Text = currentData.DiscountRate.ToString()
                TxtStockUnits.Text = currentData.StockUnits.ToString()
                TxtAlertQuantity.Text = currentData.AlertQuantity.ToString()

                ' Set markup fields
                TxtMarkup.Text = currentData.MarkupValue.ToString()

                ' Set markup type radio buttons
                If currentData.IsPercentageMarkup Then
                    RadBtnPercentage.IsChecked = True
                    RadBtnFlat.IsChecked = False
                    TxtMarkupLabel.Text = "Enter Percentage:"
                    MarkupPrefix.Kind = MaterialDesignThemes.Wpf.PackIconKind.PercentOutline
                Else
                    RadBtnPercentage.IsChecked = False
                    RadBtnFlat.IsChecked = True
                    TxtMarkupLabel.Text = "Enter Flat Amount:"
                    MarkupPrefix.Kind = MaterialDesignThemes.Wpf.PackIconKind.CurrencyPhp
                End If

                ' Set warehouse selection if available
                If currentData.SelectedWarehouseIndex >= 0 AndAlso currentData.SelectedWarehouseIndex < ComboBoxWarehouse.Items.Count Then
                    ComboBoxWarehouse.SelectedIndex = currentData.SelectedWarehouseIndex
                End If

                Debug.WriteLine($"Loaded variation with markup: {currentData.MarkupValue}, IsPercentage: {currentData.IsPercentageMarkup}")
            End If
        End Sub
#End Region

#Region "Data Validation and Save"
        Private Function ValidateFormData() As Boolean
            ' Basic validation for required fields
            If String.IsNullOrWhiteSpace(TxtRetailPrice.Text) OrElse
                   String.IsNullOrWhiteSpace(TxtPurchaseOrder.Text) OrElse
                   String.IsNullOrWhiteSpace(TxtDefaultTax.Text) OrElse
                   String.IsNullOrWhiteSpace(TxtDiscountRate.Text) OrElse
                   String.IsNullOrWhiteSpace(TxtAlertQuantity.Text) Then

                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return False
            End If

            ' Validate numeric values
            Dim retailPrice, purchaseOrder, defaultTax, discountRate As Decimal
            Dim alertQuantity As Integer

            If Not Decimal.TryParse(TxtRetailPrice.Text, retailPrice) OrElse
                   Not Decimal.TryParse(TxtPurchaseOrder.Text, purchaseOrder) OrElse
                   Not Decimal.TryParse(TxtDefaultTax.Text, defaultTax) OrElse
                   Not Decimal.TryParse(TxtDiscountRate.Text, discountRate) OrElse
                   Not Integer.TryParse(TxtAlertQuantity.Text, alertQuantity) Then

                MessageBox.Show("Please enter valid numeric values.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return False
            End If

            ' Additional business rules
            If retailPrice < 0 OrElse purchaseOrder < 0 OrElse defaultTax < 0 OrElse discountRate < 0 OrElse alertQuantity < 0 Then
                MessageBox.Show("Please enter positive values for all numeric fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return False
            End If

            ' Validate serial numbers if enabled
            If CheckBoxSerialNumber.IsChecked Then
                Dim stockUnits As Integer = 0
                If Integer.TryParse(TxtStockUnits.Text, stockUnits) AndAlso stockUnits > 0 Then
                    For Each textBox In ProductController.serialNumberTextBoxes
                        If String.IsNullOrWhiteSpace(textBox.Text) Then
                            MessageBox.Show("Please enter all serial numbers.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                            Return False
                        End If
                    Next
                End If
            End If

            Return True
        End Function

        ''' <summary>
        ''' Saves all variation data
        ''' </summary>
        Private Sub SaveVariationData(sender As Object, e As RoutedEventArgs)
            ' Save current serial number values first
            SaveSerialNumberValues()

            ' Then save the currently displayed variation
            SaveCurrentVariationData()

            ' Validate data before proceeding
            If Not ValidateFormData() Then
                Return
            End If

            ' At this point, all variations have been saved in the variationManager
            MessageBox.Show("Variation details saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        Private Sub SaveCurrentVariationData()
            ' Get the current variation data
            Dim currentData = ProductController.variationManager.GetCurrentVariationData()
            If currentData IsNot Nothing Then
                ' Get updated values from existing form fields
                Decimal.TryParse(TxtRetailPrice.Text, currentData.RetailPrice)
                Decimal.TryParse(TxtPurchaseOrder.Text, currentData.PurchaseOrder)
                Decimal.TryParse(TxtDefaultTax.Text, currentData.DefaultTax)
                Decimal.TryParse(TxtDiscountRate.Text, currentData.DiscountRate)

                ' Add the missing fields
                Integer.TryParse(TxtStockUnits.Text, currentData.StockUnits)
                Integer.TryParse(TxtAlertQuantity.Text, currentData.AlertQuantity)

                ' Save markup values
                Decimal.TryParse(TxtMarkup.Text, currentData.MarkupValue)
                currentData.IsPercentageMarkup = RadBtnPercentage.IsChecked

                Debug.WriteLine($"Saved variation with markup: {currentData.MarkupValue}, IsPercentage: {currentData.IsPercentageMarkup}")

                ' Save warehouse selection
                currentData.SelectedWarehouseIndex = ComboBoxWarehouse.SelectedIndex
                If ComboBoxWarehouse.SelectedIndex >= 0 Then
                    ' Get the actual warehouse ID from the ComboBoxItem's Tag property
                    Dim selectedItem = TryCast(ComboBoxWarehouse.SelectedItem, ComboBoxItem)
                    If selectedItem IsNot Nothing Then
                        currentData.WarehouseId = Convert.ToInt32(selectedItem.Tag)
                    End If
                End If

                ' Save serial numbers checkbox state
                currentData.IncludeSerialNumbers = CheckBoxSerialNumber.IsChecked
            End If
        End Sub
#End Region


#Region "Input Validation and Events"
        ' Text input validation handlers for numeric fields
        Private Sub IntegerOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            ' Allow only digits and decimal point
            Dim regex As New Text.RegularExpressions.Regex("[^0-9\.]+")
            e.Handled = regex.IsMatch(e.Text)
        End Sub

        Private Sub IntegerOnlyPasteHandler(sender As Object, e As DataObjectPastingEventArgs)
            If e.DataObject.GetDataPresent(GetType(String)) Then
                Dim text As String = CType(e.DataObject.GetData(GetType(String)), String)
                Dim regex As New Text.RegularExpressions.Regex("[^0-9\.]+")
                If regex.IsMatch(text) Then
                    e.CancelCommand()
                End If
            Else
                e.CancelCommand()
            End If
        End Sub

        Private Sub TxtStockUnits_KeyDown(sender As Object, e As KeyEventArgs)
            ' Handle key press events for stock units text box
            ' For example, pressing Enter could trigger validation or move focus
            If e.Key = Key.Enter Then
                ' Validate the input
                Dim stockUnits As Integer
                If Integer.TryParse(TxtStockUnits.Text, stockUnits) Then
                    ' Valid input - could save or move focus
                    TxtAlertQuantity.Focus()
                End If
            End If
        End Sub

        ''' <summary>
        ''' Updates selling price when buying price changes
        ''' </summary>
        Private Sub PurchaseOrder_TextChanged(sender As Object, e As TextChangedEventArgs)
            CalculateSellingPrice()
        End Sub
#End Region

#Region "Markup Calculation"
        ''' <summary>
        ''' Handles the radio button checked event for markup calculation mode
        ''' </summary>
        Private Sub RadioButton_Checked(sender As Object, e As RoutedEventArgs)
            ' Add null checks for UI controls
            If TxtMarkupLabel Is Nothing OrElse MarkupPrefix Is Nothing Then
                Debug.WriteLine("UI controls not initialized in RadioButton_Checked")
                Return
            End If

            ' Update markup label and icon based on selected mode
            If RadBtnPercentage IsNot Nothing AndAlso RadBtnPercentage.IsChecked Then
                TxtMarkupLabel.Text = "Enter Percentage:"
                MarkupPrefix.Kind = MaterialDesignThemes.Wpf.PackIconKind.PercentOutline
                CalculateSellingPrice()
            ElseIf RadBtnFlat IsNot Nothing AndAlso RadBtnFlat.IsChecked Then
                TxtMarkupLabel.Text = "Enter Flat Amount:"
                MarkupPrefix.Kind = MaterialDesignThemes.Wpf.PackIconKind.CurrencyPhp
                CalculateSellingPrice()
            End If
        End Sub

        ''' <summary>
        ''' Recalculates the selling price when markup value changes
        ''' </summary>
        Private Sub TxtMarkup_TextChanged(sender As Object, e As TextChangedEventArgs)
            CalculateSellingPrice()
        End Sub

        ''' <summary>
        ''' Calculates the selling price based on buying price and markup
        ''' </summary>
        Private Sub CalculateSellingPrice()
            Dim buyingPrice As Decimal = 0
            Dim markup As Decimal = 0
            Dim sellingPrice As Decimal = 0

            ' Parse the buying price
            If Not String.IsNullOrWhiteSpace(TxtPurchaseOrder.Text) Then
                Decimal.TryParse(TxtPurchaseOrder.Text, buyingPrice)
            End If

            ' Parse the markup value
            If Not String.IsNullOrWhiteSpace(TxtMarkup.Text) Then
                Decimal.TryParse(TxtMarkup.Text, markup)
            End If

            ' Calculate selling price based on markup type
            If RadBtnPercentage.IsChecked Then
                ' Percentage markup: buying price + (buying price * markup percentage / 100)
                sellingPrice = buyingPrice + (buyingPrice * markup / 100)
            ElseIf RadBtnFlat.IsChecked Then
                ' Flat markup: buying price + flat markup amount
                sellingPrice = buyingPrice + markup
            End If

            ' Update the selling price textbox
            TxtRetailPrice.Text = sellingPrice.ToString("0.00")

            Debug.WriteLine($"Calculated selling price: {sellingPrice} (Buying: {buyingPrice}, Markup: {markup}, Mode: {If(RadBtnPercentage.IsChecked, "Percentage", "Flat")})")
        End Sub
#End Region

#Region "Navigation and Redirection"
        Private Sub BtnBatchEdit(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the BatchEdit form
            ViewLoader.DynamicView.NavigateToView("batcheditproductvar", Me)
        End Sub

        Private Sub BtnBack(sender As Object, e As RoutedEventArgs)
            ' Get the current variation name before making any changes
            Dim currentVariationName = ProductController.variationManager.CurrentCombination

            ' Create and show the AddNewProduct UserControl again
            Dim addNewProduct = New AddNewProducts()

            ' Always save the current variation data first
            If Not String.IsNullOrEmpty(currentVariationName) Then
                SaveSerialNumberValues()
                SaveCurrentVariationData()

                ' Store the currently selected variation name
                Application.Current.Properties("LastSelectedVariation") = currentVariationName

                ' Verification logging
                Dim currentData = ProductController.variationManager.GetVariationData(currentVariationName)
                If currentData IsNot Nothing Then
                    Debug.WriteLine($"Verification - After saving:")
                    Debug.WriteLine($"  StockUnits: {currentData.StockUnits}")
                    Debug.WriteLine($"  SerialNumberChecked: {currentData.IncludeSerialNumbers}")
                    Debug.WriteLine($"  SerialNumbers count: {If(currentData.SerialNumbers Is Nothing, "NULL", currentData.SerialNumbers.Count.ToString())}")
                End If
            End If

            ' Ask user if they want to acknowledge saving
            Dim result As MessageBoxResult = MessageBox.Show("Your changes have been saved. Return to previous screen?",
                               "Save Changes",
                               MessageBoxButton.YesNo,
                               MessageBoxImage.Question)

            If result = MessageBoxResult.No Then
                ' User canceled, don't navigate away
                Return
            End If

            ' Now navigate back to AddNewProducts
            ViewLoader.DynamicView.NavigateToView("newproducts", Me)
        End Sub
#End Region


        ' For the serial number management region, replacing the problematic code

#Region "Serial Number Management"
        ''' <summary>
        ''' Initializes the serial number controls in the existing StackPanelSerialRow
        ''' </summary>
        Public Sub InitializeSerialNumberControls()
            ' Clear existing items in the stack panel
            StackPanelSerialRow.Children.Clear()
            ProductController.serialNumberTextBoxes.Clear()

            ' Create header for serial numbers
            Dim headerBorder As New Border With {
                .Style = CType(FindResource("RoundedBorderStyle"), Style),
                .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
                .BorderThickness = New Thickness(0),
                .CornerRadius = New CornerRadius(15, 15, 0, 0)
            }

            Dim headerPanel As New StackPanel With {
                .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
                .Orientation = Orientation.Horizontal,
                .Margin = New Thickness(20, 10, 20, 10)
            }

            Dim headerText As New TextBlock With {
                .Text = "Serial Numbers:",
                .Foreground = Brushes.White,
                .FontSize = 14,
                .FontWeight = FontWeights.SemiBold,
                .Margin = New Thickness(0, 0, 5, 0)
            }

            Dim requiredIndicator As New TextBlock With {
                .Text = "*",
                .FontSize = 14,
                .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636")),
                .FontWeight = FontWeights.Bold
            }

            headerPanel.Children.Add(headerText)
            headerPanel.Children.Add(requiredIndicator)
            headerBorder.Child = headerPanel
            StackPanelSerialRow.Children.Add(headerBorder)

            ' Create container for serial number textboxes
            Dim containerBorder As New Border With {
                .Background = Brushes.White,
                .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")),
                .BorderThickness = New Thickness(1, 0, 1, 1),
                .CornerRadius = New CornerRadius(0, 0, 15, 15)
            }

            ' Create ScrollViewer for serial number entries
            Dim scrollViewer As New ScrollViewer With {
                .VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                .HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                .MaxHeight = 400,  ' Set maximum height before scrolling begins
                .Padding = New Thickness(0, 0, 5, 0)  ' Add padding for scrollbar
            }

            ' Create container for serial number textboxes
            Dim serialNumbersContainer As New StackPanel With {
                .Name = "SerialNumbersContainer",
                .Margin = New Thickness(0, 0, 0, 10)
            }

            ' Add the container to the ScrollViewer
            scrollViewer.Content = serialNumbersContainer
            containerBorder.Child = scrollViewer

            ' Add the container to the stack panel
            StackPanelSerialRow.Children.Add(containerBorder)

            ' Initially set visibility based on checkbox state
            UpdateSerialNumberPanelVisibility()
        End Sub

        ''' <summary>
        ''' Handles text changes in the stock units textbox
        ''' </summary>
        Private Sub StockUnits_TextChanged(sender As Object, e As TextChangedEventArgs)
            ' Update serial number fields based on stock units
            If CheckBoxSerialNumber.IsChecked = True Then
                CreateSerialNumberTextBoxes()
            End If
        End Sub

        ''' <summary>
        ''' Creates serial number textboxes based on stock units value
        ''' and populates them with saved values if available
        ''' </summary>
        Private Sub CreateSerialNumberTextBoxes()
            ' Find the ScrollViewer and then the container for serial number textboxes
            Dim containerBorder As Border = TryCast(StackPanelSerialRow.Children(1), Border)
            If containerBorder Is Nothing Then Return

            Dim scrollViewer As ScrollViewer = TryCast(containerBorder.Child, ScrollViewer)
            If scrollViewer Is Nothing Then Return

            Dim serialNumbersContainer As StackPanel = TryCast(scrollViewer.Content, StackPanel)
            If serialNumbersContainer Is Nothing Then Return

            ' Clear existing textboxes
            serialNumbersContainer.Children.Clear()
            ProductController.serialNumberTextBoxes.Clear()

            ' Get stock units value
            Dim stockUnits As Integer = 0
            If Not String.IsNullOrEmpty(TxtStockUnits.Text) AndAlso Integer.TryParse(TxtStockUnits.Text, stockUnits) Then
                ' Make sure stock units is at least 1
                If stockUnits < 1 Then stockUnits = 1

                Debug.WriteLine($"Creating {stockUnits} serial number textboxes")

                ' Get current variation data to access saved serial numbers
                Dim currentData = ProductController.variationManager.GetCurrentVariationData()

                ' Make sure the SerialNumbers list exists and has enough entries
                If currentData IsNot Nothing Then
                    If currentData.SerialNumbers Is Nothing Then
                        currentData.SerialNumbers = New List(Of String)()
                    End If

                    ' Resize the list to match stock units if needed
                    While currentData.SerialNumbers.Count < stockUnits
                        currentData.SerialNumbers.Add("")
                    End While
                    While currentData.SerialNumbers.Count > stockUnits
                        currentData.SerialNumbers.RemoveAt(currentData.SerialNumbers.Count - 1)
                    End While
                End If

                ' Create textboxes based on stock units
                For i As Integer = 1 To stockUnits
                    AddSerialNumberRow(serialNumbersContainer, i)

                    ' After adding the row, find the textbox and set its value if available
                    If currentData IsNot Nothing AndAlso
               currentData.SerialNumbers IsNot Nothing AndAlso
               i - 1 < currentData.SerialNumbers.Count Then

                        ' Get the last added row
                        Dim rowPanel = TryCast(serialNumbersContainer.Children(serialNumbersContainer.Children.Count - 1), StackPanel)
                        If rowPanel IsNot Nothing Then
                            Dim grid = TryCast(rowPanel.Children(0), Grid)
                            If grid IsNot Nothing Then
                                Dim border = TryCast(grid.Children(0), Border)
                                If border IsNot Nothing Then
                                    Dim textBox = TryCast(border.Child, TextBox)
                                    If textBox IsNot Nothing Then
                                        ' Set the saved value
                                        textBox.Text = currentData.SerialNumbers(i - 1)
                                        Debug.WriteLine($"Set serial number textbox {i} to: {currentData.SerialNumbers(i - 1)}")
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next
            End If
        End Sub
        ''' <summary>
        ''' Adds a new serial number row to the specified container
        ''' </summary>
        Public Sub AddSerialNumberRow(container As StackPanel, rowIndex As Integer)
            Dim outerStackPanel As New StackPanel With {.Margin = New Thickness(25, 10, 10, 10)}

            Dim grid As New Grid()
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            ' Create the text box with style
            Dim textBox As New TextBox With {
        .Name = $"TxtSerial_{rowIndex}",
        .Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style)
    }

            ' Create border for the text box
            Dim textBoxBorder As New Border With {
        .Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style),
        .Child = textBox
    }

            ' Add textbox border directly to grid
            Grid.SetColumn(textBoxBorder, 0)
            grid.Children.Add(textBoxBorder)

            ' Create button panel
            Dim buttonPanel As New StackPanel With {
        .Orientation = Orientation.Horizontal,
        .Margin = New Thickness(10, 0, 0, 0)
    }

            ' Add Row Button
            Dim addRowButton As New Button With {
        .Background = Brushes.White,
        .BorderThickness = New Thickness(0),
        .Name = "BtnAddRow",
        .Tag = container  ' Pass the container as Tag for easy access
    }
            Dim addIcon As New MaterialDesignThemes.Wpf.PackIcon With {
        .Kind = MaterialDesignThemes.Wpf.PackIconKind.TableRowAddAfter,
        .Width = 40,
        .Height = 30,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#456B2E"))
    }
            addRowButton.Content = addIcon
            AddHandler addRowButton.Click, AddressOf BtnAddSerialRow_Click
            buttonPanel.Children.Add(addRowButton)

            ' Remove Row Button
            Dim removeRowButton As New Button With {
        .Background = Brushes.White,
        .BorderThickness = New Thickness(0),
        .Name = "BtnRemoveRow",
        .Tag = outerStackPanel  ' Store direct reference to THIS row's panel
    }
            Dim removeIcon As New MaterialDesignThemes.Wpf.PackIcon With {
        .Kind = MaterialDesignThemes.Wpf.PackIconKind.TableRowRemove,
        .Width = 40,
        .Height = 30,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636"))
    }
            removeRowButton.Content = removeIcon
            AddHandler removeRowButton.Click, AddressOf BtnRemoveSerialRow_Click
            buttonPanel.Children.Add(removeRowButton)

            ' Separator Border
            Dim separatorBorder As New Border With {
        .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")),
        .BorderThickness = New Thickness(1),
        .Height = 30
    }
            buttonPanel.Children.Add(separatorBorder)

            ' Row Controller Button
            Dim rowControllerButton As New Button With {
        .Background = Brushes.White,
        .BorderThickness = New Thickness(0),
        .Name = "BtnRowController"
    }
            Dim menuIcon As New MaterialDesignThemes.Wpf.PackIcon With {
        .Kind = MaterialDesignThemes.Wpf.PackIconKind.MenuDown,
        .Width = 30,
        .Height = 30,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE"))
    }
            rowControllerButton.Content = menuIcon
            AddHandler rowControllerButton.Click, AddressOf ProductController.BtnRowController_Click
            buttonPanel.Children.Add(rowControllerButton)

            Grid.SetColumn(buttonPanel, 1)
            grid.Children.Add(buttonPanel)
            outerStackPanel.Children.Add(grid)

            ' Set a name on the outer stack panel for easier debugging
            outerStackPanel.Name = $"SerialRow_{rowIndex}"

            ' Add the row to the container
            container.Children.Add(outerStackPanel)

            ' Add a debug message
            Debug.WriteLine($"Added row {rowIndex} to container at index {container.Children.Count - 1}")
        End Sub

        ''' <summary>
        ''' Handles the Add Serial Row button click
        ''' </summary>
        Public Sub BtnAddSerialRow_Click(sender As Object, e As RoutedEventArgs)
            ' Save current serial number values first
            SaveSerialNumberValues()

            Dim button = TryCast(sender, Button)
            If button Is Nothing Then Return

            ' Get the container from the button's Tag property
            Dim container = TryCast(button.Tag, StackPanel)
            If container Is Nothing Then Return

            ' Add a new row with index based on current count
            Dim newIndex = container.Children.Count + 1
            AddSerialNumberRow(container, newIndex)

            ' Update the stock units textbox to match the number of rows
            TxtStockUnits.Text = container.Children.Count.ToString()

            ' Update our current variation data with the new count
            Dim currentData = ProductController.variationManager.GetCurrentVariationData()
            If currentData IsNot Nothing Then
                currentData.StockUnits = container.Children.Count

                ' Add empty entry to serial numbers list
                If currentData.SerialNumbers Is Nothing Then
                    currentData.SerialNumbers = New List(Of String)()
                End If
                currentData.SerialNumbers.Add("")
            End If

            ' Scroll to the new row that was added at the bottom
            Dim containerBorder As Border = TryCast(StackPanelSerialRow.Children(1), Border)
            If containerBorder IsNot Nothing Then
                Dim scrollViewer As ScrollViewer = TryCast(containerBorder.Child, ScrollViewer)
                If scrollViewer IsNot Nothing Then
                    ' Scroll to the bottom to show the newly added row
                    scrollViewer.ScrollToEnd()
                End If
            End If
        End Sub

        ''' <summary>
        ''' Handles the Remove Serial Row button click
        ''' </summary>
        Private Sub BtnRemoveSerialRow_Click(sender As Object, e As RoutedEventArgs)
            ' Save current serial number values first
            SaveSerialNumberValues()

            Dim button = TryCast(sender, Button)
            If button Is Nothing Then Return

            ' Find the container that holds all serial number rows
            Dim containerBorder As Border = TryCast(StackPanelSerialRow.Children(1), Border)
            If containerBorder Is Nothing Then Return

            Dim scrollViewer As ScrollViewer = TryCast(containerBorder.Child, ScrollViewer)
            If scrollViewer Is Nothing Then Return

            Dim container = TryCast(scrollViewer.Content, StackPanel)
            If container Is Nothing Then Return

            ' Don't allow removing the last row
            If container.Children.Count <= 1 Then
                MessageBox.Show("Cannot remove the last serial number row.", "Information", MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If

            ' Get the row to remove directly from the button's Tag
            Dim rowPanel = TryCast(button.Tag, StackPanel)
            If rowPanel Is Nothing Then
                MessageBox.Show("Could not locate the row to remove.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            ' Find the index of this row in the container
            Dim rowIndex = container.Children.IndexOf(rowPanel)
            If rowIndex < 0 Then
                MessageBox.Show("Could not locate the row in container.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            ' Get the textbox in this row to verify we're removing the right one
            Dim textBox As TextBox = Nothing
            Dim grid = TryCast(rowPanel.Children(0), Grid)
            If grid IsNot Nothing Then
                Dim border = TryCast(grid.Children(0), Border)
                If border IsNot Nothing Then
                    textBox = TryCast(border.Child, TextBox)
                End If
            End If

            ' Store the text value for later use
            Dim textValue = If(textBox IsNot Nothing, textBox.Text, "N/A")
            Debug.WriteLine($"Removing row at index {rowIndex} with text: {textValue}")

            ' Get the current variation data before removing the row
            Dim currentData = ProductController.variationManager.GetCurrentVariationData()

            ' Remove the specified serial number from the data model if needed
            If currentData IsNot Nothing AndAlso
       currentData.SerialNumbers IsNot Nothing AndAlso
       currentData.IncludeSerialNumbers AndAlso
       rowIndex >= 0 AndAlso
       rowIndex < currentData.SerialNumbers.Count Then
                currentData.SerialNumbers.RemoveAt(rowIndex)
            End If

            ' Remove the row from the UI
            container.Children.Remove(rowPanel)

            ' Update the stock units to match
            TxtStockUnits.Text = container.Children.Count.ToString()

            ' Update stock units in the data model
            If currentData IsNot Nothing Then
                currentData.StockUnits = container.Children.Count
            End If

            ' Force a complete refresh of all rows to ensure UI sync
            Dispatcher.BeginInvoke(New Action(Sub()
                                                  ' Update indices after removal
                                                  UpdateSerialNumberIndices(container)

                                                  ' Force the UI to refresh
                                                  scrollViewer.InvalidateVisual()
                                                  container.InvalidateVisual()

                                                  ' Show a message confirming which row was removed
                                                  MessageBox.Show($"Row with value '{textValue}' was removed.", "Row Removed", MessageBoxButton.OK, MessageBoxImage.Information)
                                              End Sub), DispatcherPriority.Render)
        End Sub

        ''' <summary>
        ''' Helper method to save all serial number values from UI to data model
        ''' </summary>
        Public Sub SaveSerialNumberValues()
            ' Get the current variation data
            Dim currentData = ProductController.variationManager.GetCurrentVariationData()
            If currentData Is Nothing OrElse Not currentData.IncludeSerialNumbers Then Return

            ' Find the container that holds all serial number rows
            Dim containerBorder As Border = TryCast(StackPanelSerialRow.Children(1), Border)
            If containerBorder Is Nothing Then Return

            Dim scrollViewer As ScrollViewer = TryCast(containerBorder.Child, ScrollViewer)
            If scrollViewer Is Nothing Then Return

            Dim container = TryCast(scrollViewer.Content, StackPanel)
            If container Is Nothing Then Return

            ' Create new list to hold serial numbers
            currentData.SerialNumbers = New List(Of String)()

            ' Collect all serial numbers from textboxes
            For Each rowPanel As FrameworkElement In container.Children
                Dim stackPanel = TryCast(rowPanel, StackPanel)
                If stackPanel IsNot Nothing Then
                    Dim grid = TryCast(stackPanel.Children(0), Grid)
                    If grid IsNot Nothing Then
                        Dim border = TryCast(grid.Children(0), Border)
                        If border IsNot Nothing Then
                            Dim textBox = TryCast(border.Child, TextBox)
                            If textBox IsNot Nothing Then
                                currentData.SerialNumbers.Add(textBox.Text)
                            End If
                        End If
                    End If
                End If
            Next
        End Sub

        ''' <summary>
        ''' Updates the indices of all serial number rows after a removal
        ''' </summary>
        Private Sub UpdateSerialNumberIndices(container As StackPanel)
            ' We don't need to store/restore values because we're only updating names
            ' not rebuilding the entire collection

            ' Go through each row and update indices
            For i As Integer = 0 To container.Children.Count - 1
                Dim rowPanel = TryCast(container.Children(i), StackPanel)
                If rowPanel IsNot Nothing Then
                    ' Get the grid
                    Dim grid = TryCast(rowPanel.Children(0), Grid)
                    If grid IsNot Nothing Then
                        ' Get the textbox border
                        Dim textBoxBorder = TryCast(grid.Children(0), Border)
                        If textBoxBorder IsNot Nothing Then
                            ' Get the textbox
                            Dim textBox = TryCast(textBoxBorder.Child, TextBox)
                            If textBox IsNot Nothing Then
                                ' Update the name only
                                textBox.Name = $"TxtSerial_{i + 1}"
                                ' Note: we don't modify the collection or textbox values
                            End If
                        End If
                    End If
                End If
            Next
        End Sub
        ''' <summary>
        ''' Handles changes to the serial number checkbox
        ''' </summary>
        Private Sub SerialNumberCheckboxChanged(sender As Object, e As RoutedEventArgs)
            Debug.WriteLine($"Serial number checkbox changed to: {CheckBoxSerialNumber.IsChecked}")

            ' Update visibility based on checkbox state
            UpdateSerialNumberPanelVisibility()

            ' Update the current variation data
            Dim currentData = ProductController.variationManager.GetCurrentVariationData()
            If currentData IsNot Nothing Then
                currentData.IncludeSerialNumbers = CheckBoxSerialNumber.IsChecked
                Debug.WriteLine($"Updated variation data with IncludeSerialNumbers: {currentData.IncludeSerialNumbers}")

                ' If checked, create the serial number textboxes
                If CheckBoxSerialNumber.IsChecked Then
                    ' Initialize serial numbers list if needed
                    If currentData.SerialNumbers Is Nothing OrElse currentData.SerialNumbers.Count = 0 Then
                        currentData.SerialNumbers = New List(Of String)()
                        Dim stockUnits As Integer = 1 ' Default to 1
                        If Not String.IsNullOrEmpty(TxtStockUnits.Text) AndAlso Integer.TryParse(TxtStockUnits.Text, stockUnits) Then
                            ' Use existing stock units
                        End If

                        ' Add empty entries based on stock units
                        For i As Integer = 0 To stockUnits - 1
                            currentData.SerialNumbers.Add("")
                        Next

                        Debug.WriteLine($"Initialized {currentData.SerialNumbers.Count} empty serial number entries")
                    End If

                    CreateSerialNumberTextBoxes()
                End If
            End If
        End Sub

        ''' <summary>
        ''' Updates the visibility of serial number panel based on checkbox state
        ''' </summary>
        Public Sub UpdateSerialNumberPanelVisibility()
            If CheckBoxSerialNumber.IsChecked Then
                StackPanelSerialRow.Visibility = Visibility.Visible
            Else
                StackPanelSerialRow.Visibility = Visibility.Collapsed
            End If
        End Sub

        ''' <summary>
        ''' Loads the serial number data into controls
        ''' </summary>
        Private Sub LoadSerialNumberData()
            ' Get the current variation data
            Dim currentData = ProductController.variationManager.GetCurrentVariationData()
            Debug.WriteLine("Loading serial number data")

            ' Set the data to controls
            If currentData IsNot Nothing Then
                ' Set checkbox state
                CheckBoxSerialNumber.IsChecked = currentData.IncludeSerialNumbers
                Debug.WriteLine($"Serial Number Checkbox set to: {currentData.IncludeSerialNumbers}")

                ' Update visibility based on checkbox state
                UpdateSerialNumberPanelVisibility()

                ' Create textboxes based on stock units
                If currentData.IncludeSerialNumbers Then
                    Debug.WriteLine("Creating serial number textboxes")
                    CreateSerialNumberTextBoxes()

                    ' Force a new reading of serial numbers from the data model
                    If currentData.SerialNumbers IsNot Nothing AndAlso currentData.SerialNumbers.Count > 0 Then
                        Debug.WriteLine($"Populating {currentData.SerialNumbers.Count} serial numbers")

                        ' Find the container that holds all serial number rows
                        Dim containerBorder As Border = TryCast(StackPanelSerialRow.Children(1), Border)
                        If containerBorder IsNot Nothing Then
                            Dim scrollViewer As ScrollViewer = TryCast(containerBorder.Child, ScrollViewer)
                            If scrollViewer IsNot Nothing Then
                                Dim container = TryCast(scrollViewer.Content, StackPanel)
                                If container IsNot Nothing Then
                                    ' Populate serial numbers into textboxes
                                    For i As Integer = 0 To Math.Min(currentData.SerialNumbers.Count - 1, container.Children.Count - 1)
                                        ' Find the textbox in this row
                                        Dim rowPanel = TryCast(container.Children(i), StackPanel)
                                        If rowPanel IsNot Nothing Then
                                            Dim grid = TryCast(rowPanel.Children(0), Grid)
                                            If grid IsNot Nothing Then
                                                Dim border = TryCast(grid.Children(0), Border)
                                                If border IsNot Nothing Then
                                                    Dim textBox = TryCast(border.Child, TextBox)
                                                    If textBox IsNot Nothing Then
                                                        textBox.Text = currentData.SerialNumbers(i)
                                                        Debug.WriteLine($"Set serial number [{i}] to: {currentData.SerialNumbers(i)}")
                                                    End If
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End Sub
#End Region
    End Class
End Namespace