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

            ' Initialize dynamic content
            InitializeVariations()

            ' Load variations panel
            LoadVariationCombinations()
        End Sub

        Private Sub BtnBatchEdit(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim OpenBatchEdit As New ProductBatchEdit()

            Me.Close()
            OpenBatchEdit.Show()
        End Sub
        ' Function to handle integer only input on textboxes
        Private Sub IntegerOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            If Not IsNumeric(e.Text) OrElse Not Integer.TryParse(e.Text, New Integer()) Then
                e.Handled = True ' Block non-integer input
            End If
        End Sub

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


        'loading variations and combining options
        Public Sub LoadVariationCombinations()
            ' Clear existing buttons in the variations panel
            Dim variationsPanel As StackPanel = FindName("VariationsPanel")
            If variationsPanel Is Nothing Then
                ' If the panel doesn't exist in XAML, we need to create it
                variationsPanel = New StackPanel()
                variationsPanel.Name = "VariationsPanel"
                variationsPanel.Orientation = Orientation.Horizontal

                ' Replace the existing hardcoded panel with our dynamic one
                ' Find the container that holds the current hardcoded buttons
                Dim currentPanel = FindName("StackPanel1") ' Update this name to match your actual container
                If currentPanel IsNot Nothing AndAlso TypeOf currentPanel.Parent Is Grid Then
                    Dim parentGrid As Grid = DirectCast(currentPanel.Parent, Grid)
                    Dim row As Integer = Grid.GetRow(currentPanel)
                    Dim column As Integer = Grid.GetColumn(currentPanel)

                    parentGrid.Children.Remove(currentPanel)
                    Grid.SetRow(variationsPanel, row)
                    Grid.SetColumn(variationsPanel, column)
                    parentGrid.Children.Add(variationsPanel)
                End If
            Else
                variationsPanel.Children.Clear()
            End If

            ' Get variations from the shared static property
            Dim variations As List(Of ProductVariation) = DPC.Components.Forms.AddVariation.SavedVariations

            ' Check if we have any variations
            If variations Is Nothing OrElse variations.Count = 0 Then
                ' No variations, add a default placeholder option
                AddVariationButton(variationsPanel, "Default Variation", True)
                Return
            End If

            ' Check if we have one or two variations
            If variations.Count = 1 Then
                ' Single variation case - just show options
                Dim variation As ProductVariation = variations(0)
                If variation.Options IsNot Nothing AndAlso variation.Options.Count > 0 Then
                    Dim isFirst As Boolean = True
                    For Each opt As VariationOption In variation.Options
                        AddVariationButton(variationsPanel, opt.OptionName, isFirst)
                        isFirst = False
                    Next
                End If
            ElseIf variations.Count = 2 Then
                ' Two variations case - create combinations
                Dim variation1 As ProductVariation = variations(0)
                Dim variation2 As ProductVariation = variations(1)

                If variation1.Options IsNot Nothing AndAlso variation1.Options.Count > 0 AndAlso
               variation2.Options IsNot Nothing AndAlso variation2.Options.Count > 0 Then

                    Dim isFirst As Boolean = True
                    For Each option1 As VariationOption In variation1.Options
                        For Each option2 As VariationOption In variation2.Options
                            ' Create combination label: "Color, Size"
                            Dim combinationName As String = $"{option1.OptionName}, {option2.OptionName}"
                            AddVariationButton(variationsPanel, combinationName, isFirst)
                            isFirst = False
                        Next
                    Next
                End If
            End If
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
            ProductController.BtnAddRow_Click(Nothing, Nothing)

            ' Set default tax value
            Dim txtDefaultTax As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtDefaultTax")
            If txtDefaultTax IsNot Nothing Then
                txtDefaultTax.Text = "12"
            End If

            ' Call this during form initialization
            LoadVariationCombinations()

            ' Set the first variation as selected by default
            Dim variationsPanel As System.Windows.Controls.StackPanel = TryCast(FindName("VariationsPanel"), System.Windows.Controls.StackPanel)
            If variationsPanel IsNot Nothing AndAlso variationsPanel.Children.Count > 0 Then
                Dim firstButton As System.Windows.Controls.Button = TryCast(variationsPanel.Children(0), System.Windows.Controls.Button)
                If firstButton IsNot Nothing Then
                    ' Simulate click on first button
                    firstButton.RaiseEvent(New RoutedEventArgs(System.Windows.Controls.Button.ClickEvent))
                End If
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
            If String.IsNullOrEmpty(combinationName) Then
                Return
            End If

            ' Get the current variation data
            Dim variationData As ProductVariationData = variationManager.GetVariationData(combinationName)
            If variationData Is Nothing Then
                Return
            End If

            ' Find controls in the dynamic form container
            Dim txtRetailPrice As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtRetailPrice")
            Dim txtPurchaseOrder As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtPurchaseOrder")
            Dim txtDefaultTax As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtDefaultTax")
            Dim txtDiscountRate As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtDiscountRate")
            Dim txtStockUnits As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtStockUnits")
            Dim txtAlertQuantity As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtAlertQuantity")
            Dim checkBoxSerialNumber As System.Windows.Controls.CheckBox = FindVisualChild(Of System.Windows.Controls.CheckBox)(SerialNumberContainer, "CheckBoxSerialNumber")
            Dim comboBoxWarehouse As System.Windows.Controls.ComboBox = _comboBoxWarehouse


            ' Save form values to the model (with improved error handling)
            Try
                If txtRetailPrice IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtRetailPrice.Text) Then
                    variationData.RetailPrice = Decimal.Parse(txtRetailPrice.Text)
                End If

                If txtPurchaseOrder IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtPurchaseOrder.Text) Then
                    variationData.PurchaseOrder = Decimal.Parse(txtPurchaseOrder.Text)
                End If

                If txtDefaultTax IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtDefaultTax.Text) Then
                    variationData.DefaultTax = Decimal.Parse(txtDefaultTax.Text)
                End If

                If txtDiscountRate IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtDiscountRate.Text) Then
                    variationData.DiscountRate = Decimal.Parse(txtDiscountRate.Text)
                End If

                If txtStockUnits IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtStockUnits.Text) Then
                    variationData.StockUnits = Integer.Parse(txtStockUnits.Text)
                End If

                If txtAlertQuantity IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txtAlertQuantity.Text) Then
                    variationData.AlertQuantity = Integer.Parse(txtAlertQuantity.Text)
                End If

                If comboBoxWarehouse IsNot Nothing AndAlso comboBoxWarehouse.SelectedItem IsNot Nothing Then
                    Dim selectedItem As ComboBoxItem = DirectCast(comboBoxWarehouse.SelectedItem, ComboBoxItem)
                    If selectedItem IsNot Nothing AndAlso selectedItem.Tag IsNot Nothing Then
                        variationData.WarehouseId = Convert.ToInt32(selectedItem.Tag)
                    End If
                End If

                ' Save serial number settings
                If checkBoxSerialNumber IsNot Nothing Then
                    variationData.IncludeSerialNumbers = checkBoxSerialNumber.IsChecked.Value

                    ' Save serial numbers if needed
                    If variationData.IncludeSerialNumbers Then
                        variationData.SerialNumbers.Clear()

                        ' Make sure ProductController.SerialNumbers is not null
                        If ProductController.SerialNumbers IsNot Nothing Then
                            ' Save serial numbers from your serial number controls
                            For Each serialNumber As System.Windows.Controls.TextBox In ProductController.SerialNumbers
                                If serialNumber IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(serialNumber.Text) Then
                                    variationData.SerialNumbers.Add(serialNumber.Text)
                                End If
                            Next
                        End If
                    End If
                End If
            Catch ex As Exception
                ' Handle parsing errors gracefully
                MessageBox.Show("An error occurred while saving form data: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub LoadFormData(combinationName As String)
            ' Get the variation data
            Dim variationData As ProductVariationData = variationManager.GetVariationData(combinationName)

            ' Find controls in the dynamic form container
            Dim txtRetailPrice As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtRetailPrice")
            Dim txtPurchaseOrder As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtPurchaseOrder")
            Dim txtDefaultTax As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtDefaultTax")
            Dim txtDiscountRate As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtDiscountRate")
            Dim txtStockUnits As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtStockUnits")
            Dim txtAlertQuantity As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtAlertQuantity")
            Dim checkBoxSerialNumber As System.Windows.Controls.CheckBox = FindVisualChild(Of System.Windows.Controls.CheckBox)(SerialNumberContainer, "CheckBoxSerialNumber")

            ' Find the warehouse ComboBox
            Dim comboBoxWarehouse As System.Windows.Controls.ComboBox = _comboBoxWarehouse


            ' Debug message to check if ComboBox is found
            If comboBoxWarehouse Is Nothing Then
                MessageBox.Show("Debug: ComboBoxWarehouse was not found in the visual tree")
            Else
                ' Ensure the ComboBox is populated
                Try
                    ProductController.GetWarehouse(comboBoxWarehouse)

                    ' Set the selected warehouse if one was previously saved
                    If variationData.WarehouseId > 0 Then
                        For i As Integer = 0 To comboBoxWarehouse.Items.Count - 1
                            Dim item As ComboBoxItem = DirectCast(comboBoxWarehouse.Items(i), ComboBoxItem)
                            If item IsNot Nothing AndAlso item.Tag IsNot Nothing AndAlso CInt(item.Tag) = variationData.WarehouseId Then
                                comboBoxWarehouse.SelectedIndex = i
                                Exit For
                            End If
                        Next
                    End If
                Catch ex As Exception
                    MessageBox.Show("Error initializing warehouse dropdown: " & ex.Message)
                End Try
            End If


            ' Load data into form controls
            If txtRetailPrice IsNot Nothing Then
                txtRetailPrice.Text = If(variationData.RetailPrice = 0, "", variationData.RetailPrice.ToString())
            End If

            If txtPurchaseOrder IsNot Nothing Then
                txtPurchaseOrder.Text = If(variationData.PurchaseOrder = 0, "", variationData.PurchaseOrder.ToString())
            End If

            If txtDefaultTax IsNot Nothing Then
                txtDefaultTax.Text = If(variationData.DefaultTax = 0, "12", variationData.DefaultTax.ToString())
            End If

            If txtDiscountRate IsNot Nothing Then
                txtDiscountRate.Text = If(variationData.DiscountRate = 0, "", variationData.DiscountRate.ToString())
            End If

            If txtStockUnits IsNot Nothing Then
                txtStockUnits.Text = If(variationData.StockUnits = 0, "", variationData.StockUnits.ToString())
            End If

            If txtAlertQuantity IsNot Nothing Then
                txtAlertQuantity.Text = If(variationData.AlertQuantity = 0, "", variationData.AlertQuantity.ToString())
            End If

            ' Set checkbox state and update visibility
            If checkBoxSerialNumber IsNot Nothing Then
                checkBoxSerialNumber.IsChecked = variationData.IncludeSerialNumbers
                SerialNumberChecker(checkBoxSerialNumber)
            End If

            If comboBoxWarehouse IsNot Nothing AndAlso variationData.WarehouseId > 0 Then
                For i As Integer = 0 To comboBoxWarehouse.Items.Count - 1
                    Dim item As ComboBoxItem = DirectCast(comboBoxWarehouse.Items(i), ComboBoxItem)
                    If item IsNot Nothing AndAlso item.Tag IsNot Nothing AndAlso CInt(item.Tag) = variationData.WarehouseId Then
                        comboBoxWarehouse.SelectedIndex = i
                        Exit For
                    End If
                Next
            End If

            ' Clear existing serial numbers
            ProductController.SerialNumbers.Clear()

            ' Find MainContainer
            Dim mainContainer As System.Windows.Controls.StackPanel = FindVisualChild(Of System.Windows.Controls.StackPanel)(SerialNumberContainer, "MainContainer")
            If mainContainer IsNot Nothing Then
                mainContainer.Children.Clear()

                ' If we have serial numbers, load them
                If variationData.IncludeSerialNumbers And variationData.SerialNumbers.Count > 0 Then
                    ' Load serial numbers into UI
                    For Each serialNumber As String In variationData.SerialNumbers
                        ' Add a row for each serial number
                        ProductController.BtnAddRow_Click(Nothing, Nothing)

                        ' Now set the value of the last added row
                        If mainContainer.Children.Count > 0 Then
                            Dim lastGrid As System.Windows.Controls.Grid = TryCast(mainContainer.Children(mainContainer.Children.Count - 1), System.Windows.Controls.Grid)
                            If lastGrid IsNot Nothing Then
                                ' Find the TextBox in the grid
                                For Each child As UIElement In lastGrid.Children
                                    If TypeOf child Is System.Windows.Controls.TextBox Then
                                        Dim textBox As System.Windows.Controls.TextBox = DirectCast(child, System.Windows.Controls.TextBox)
                                        textBox.Text = serialNumber
                                        Exit For
                                    End If
                                Next
                            End If
                        End If
                    Next
                Else
                    ' Add at least one empty row for serial input
                    ProductController.BtnAddRow_Click(Nothing, Nothing)
                End If
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
            ' Find controls to validate
            Dim txtRetailPrice As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtRetailPrice")
            Dim txtPurchaseOrder As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtPurchaseOrder")
            Dim comboBoxWarehouse As System.Windows.Controls.ComboBox = _comboBoxWarehouse


            ' Check for required fields
            If txtRetailPrice Is Nothing OrElse String.IsNullOrWhiteSpace(txtRetailPrice.Text) Then
                MessageBox.Show("Please enter a selling price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                If txtRetailPrice IsNot Nothing Then
                    txtRetailPrice.Focus()
                End If
                Return False
            End If

            If txtPurchaseOrder Is Nothing OrElse String.IsNullOrWhiteSpace(txtPurchaseOrder.Text) Then
                MessageBox.Show("Please enter a buying price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                If txtPurchaseOrder IsNot Nothing Then
                    txtPurchaseOrder.Focus()
                End If
                Return False
            End If

            If comboBoxWarehouse Is Nothing OrElse comboBoxWarehouse.SelectedIndex < 0 Then
                MessageBox.Show("Please select a warehouse.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                If comboBoxWarehouse IsNot Nothing Then
                    comboBoxWarehouse.Focus()
                End If
                Return False
            End If

            ' Add validation for serial numbers if included
            Dim checkBoxSerialNumber As System.Windows.Controls.CheckBox = FindVisualChild(Of System.Windows.Controls.CheckBox)(SerialNumberContainer, "CheckBoxSerialNumber")
            If checkBoxSerialNumber IsNot Nothing AndAlso checkBoxSerialNumber.IsChecked.Value Then
                ' Check if any serial numbers are added
                If ProductController.SerialNumbers Is Nothing OrElse ProductController.SerialNumbers.Count = 0 Then
                    MessageBox.Show("Please add at least one serial number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return False
                End If

                ' Check if all serial numbers have values
                For Each serialNumber As System.Windows.Controls.TextBox In ProductController.SerialNumbers
                    If String.IsNullOrWhiteSpace(serialNumber.Text) Then
                        MessageBox.Show("All serial numbers must have a value.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                        serialNumber.Focus()
                        Return False
                    End If
                Next
            End If

            Return True
        End Function

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
                    Dim grid As New Grid()
                    grid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})
                    grid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})


                    ' Serial Checkbox
                    Dim serialCheckboxPanel As New System.Windows.Controls.StackPanel With {
                .Margin = New Thickness(0, 10, 0, 10),
                .Name = "StackPanelSerialNumber",
                .Orientation = Orientation.Horizontal
            }
                    Grid.SetRow(serialCheckboxPanel, 0)

                    CheckBoxSerialNumber = New System.Windows.Controls.CheckBox With {.Name = "CheckBoxSerialNumber", .IsChecked = True}
                    AddHandler CheckBoxSerialNumber.Click, AddressOf IncludeSerial_Click
                    serialCheckboxPanel.Children.Add(CheckBoxSerialNumber)
                    serialCheckboxPanel.Children.Add(New System.Windows.Controls.TextBlock With {
                .Text = "Include Serial Number:",
                .FontSize = 14,
                .FontWeight = FontWeights.SemiBold,
                .Margin = New Thickness(10, 0, 0, 0)
            })

                    grid.Children.Add(serialCheckboxPanel)

                    ' Serial Number Section with Scroll
                    StackPanelSerialRow = New Grid With {
                .Name = "StackPanelSerialRow",
                .VerticalAlignment = VerticalAlignment.Stretch
            }
                    StackPanelSerialRow.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})
                    StackPanelSerialRow.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
                    Grid.SetRow(StackPanelSerialRow, 1)

                    ' Serial Label Header
                    Dim headerBorder As New Border With {
        .Style = CType(FindResource("RoundedBorderStyle"), Style),
        .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
        .BorderThickness = New Thickness(0),
        .CornerRadius = New CornerRadius(15, 15, 0, 0)
    }

                    Dim headerPanel As New System.Windows.Controls.StackPanel With {
        .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
        .Orientation = Orientation.Horizontal,
        .Margin = New Thickness(20, 10, 20, 10)
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
                    Grid.SetRow(headerBorder, 0)
                    serialRowPanel.Children.Add(headerBorder)

                    ' Scrollable Serial Input Container
                    Dim serialBorder As New Border With {
        .Background = Brushes.White,
        .BorderThickness = New Thickness(1),
        .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#E0E0E0"))
    }

                    Dim serialScrollViewer As New ScrollViewer With {
        .VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        .HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        .Style = CType(FindResource("ModernScrollViewerStyle"), Style),
        .Height = 400,
        .MinHeight = 300
    }

                    MainContainer = New System.Windows.Controls.StackPanel With {
                .Name = "MainContainer",
                .Margin = New Thickness(10)
            }
                    ProductController.MainContainer = MainContainer
                    If ProductController.SerialNumbers Is Nothing Then
                        ProductController.SerialNumbers = New List(Of System.Windows.Controls.TextBox)()
                    End If

                    ' Add an initial serial number row if needed
                    If mainContainer.Children.Count = 0 AndAlso TxtStockUnits IsNot Nothing AndAlso
               (String.IsNullOrEmpty(TxtStockUnits.Text) OrElse CInt(TxtStockUnits.Text) > 0) Then
                        ProductController.BtnAddRow_Click(Nothing, Nothing)
                    End If


                    serialScrollViewer.Content = mainContainer
                    serialBorder.Child = serialScrollViewer
                    Grid.SetRow(serialBorder, 1)
                    serialRowPanel.Children.Add(serialBorder)

                    grid.Children.Add(serialRowPanel)

                    container.Content = grid

            End Select

            Return container
        End Function
    End Class
End Namespace