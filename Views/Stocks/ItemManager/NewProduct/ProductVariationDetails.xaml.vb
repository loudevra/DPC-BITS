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

        Private _currentWarehouseComboBox As System.Windows.Controls.ComboBox
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

            LoadVariationCombinations()

            ' Auto-select the first variation after loading combinations
            SelectFirstVariationOnLoad()
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

        ' Update the IncludeSerial_Click method to handle serial number visibility
        Private Sub IncludeSerial_Click(sender As Object, e As RoutedEventArgs)
            ' Get current stock units value
            Dim stockUnits As Integer = 0
            If TxtStockUnits IsNot Nothing AndAlso Integer.TryParse(TxtStockUnits.Text, stockUnits) Then
                ' Now we have the stock units value
            End If

            ' Update UI based on checkbox state
            If CheckBoxSerialNumber IsNot Nothing Then
                Dim isChecked As Boolean = CheckBoxSerialNumber.IsChecked.GetValueOrDefault()

                ' Update UI elements
                If BorderStockUnits IsNot Nothing Then
                    BorderStockUnits.BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString(
                If(isChecked, "#D9D9D9", "#FF0000"))) ' Red if not checked
                End If

                If MainContainer IsNot Nothing Then
                    MainContainer.Visibility = If(isChecked, Visibility.Visible, Visibility.Collapsed)

                    ' Clear existing rows
                    MainContainer.Children.Clear()
                    ProductController.SerialNumbers.Clear()

                    ' Add rows if checked and stock units are specified
                    If isChecked Then
                        If stockUnits > 0 Then
                            For i As Integer = 1 To stockUnits
                                AddSerialRow()
                            Next
                        Else
                            ' Default to 1 row if no stock count is specified
                            AddSerialRow()
                            ' Update stock units to match
                            If TxtStockUnits IsNot Nothing Then
                                TxtStockUnits.Text = "1"
                            End If
                        End If
                    End If
                End If

                ' Update the data model
                If Not String.IsNullOrEmpty(variationManager.CurrentCombination) Then
                    Dim variationData As ProductVariationData = variationManager.GetVariationData(variationManager.CurrentCombination)
                    If variationData IsNot Nothing Then
                        variationData.IncludeSerialNumbers = isChecked
                    End If
                End If
            End If
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
            ' Save current values before switching
            If Not String.IsNullOrEmpty(variationManager.CurrentCombination) Then
                SaveCurrentFormData(variationManager.CurrentCombination)
            End If

            ' Select the new combination
            variationManager.SelectVariationCombination(combinationName)

            ' Create dynamic containers if needed
            If DynamicFormContainer.Content Is Nothing Then
                DynamicFormContainer.Content = RenderProduct.CreateDynamicContainer("dynamicform", Me)
            End If

            If SerialNumberContainer.Content Is Nothing Then
                SerialNumberContainer.Content = RenderProduct.CreateDynamicContainer("serialnumber", Me)
            End If

            ' Get references to all needed controls after containers are created
            Dim checkBoxSerialNumber As System.Windows.Controls.CheckBox = FindVisualChild(Of System.Windows.Controls.CheckBox)(SerialNumberContainer, "CheckBoxSerialNumber")
            Me.CheckBoxSerialNumber = checkBoxSerialNumber

            ' Get the warehouse ComboBox specific to this variation
            Dim comboBoxWarehouse As System.Windows.Controls.ComboBox = FindVisualChild(Of System.Windows.Controls.ComboBox)(DynamicFormContainer, "ComboBoxWarehouse")

            ' Ensure the warehouse ComboBox is populated - do this before loading the data
            If comboBoxWarehouse IsNot Nothing AndAlso comboBoxWarehouse.Items.Count = 0 Then
                ProductController.GetWarehouse(comboBoxWarehouse)
            End If

            ' Get the main container for serial numbers
            Dim mainContainer As StackPanel = FindVisualChild(Of StackPanel)(SerialNumberContainer, "MainContainer")
            If mainContainer IsNot Nothing Then
                Me.MainContainer = mainContainer
                ProductController.MainContainer = mainContainer
            End If

            ' Load data for the selected combination
            LoadFormData(combinationName)

            ' Update the selection title
            Dim titleTextBlock As TextBlock = TryCast(FindName("SelectedVariationTitle"), TextBlock)
            If titleTextBlock IsNot Nothing Then
                titleTextBlock.Text = $"Selected: {combinationName}"
            End If
        End Sub        ' To be called when the form loads
        Private Sub InitializeVariations()
            ' Create dynamic containers
            DynamicFormContainer.Content = RenderProduct.CreateDynamicContainer("dynamicform", Me)
            SerialNumberContainer.Content = RenderProduct.CreateDynamicContainer("serialnumber", Me)

            ' Initialize warehouse dropdown (but don't store it in a shared field)
            Dim comboBox As System.Windows.Controls.ComboBox = FindVisualChild(Of System.Windows.Controls.ComboBox)(DynamicFormContainer, "ComboBoxWarehouse")
            If comboBox IsNot Nothing Then
                ' Initialize with warehouse items but don't store in shared field
                ProductController.GetWarehouse(comboBox)
                _currentWarehouseComboBox = comboBox  ' Store in the instance field
            End If

            ' Set up other references
            Dim mainContainer As System.Windows.Controls.StackPanel = FindVisualChild(Of System.Windows.Controls.StackPanel)(SerialNumberContainer, "MainContainer")
            If mainContainer IsNot Nothing Then
                Me.MainContainer = mainContainer  ' Store reference to the class-level field
                ProductController.MainContainer = mainContainer
            End If

            ' Find and initialize TxtStockUnits
            Dim txtStockUnits As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtStockUnits")
            If txtStockUnits IsNot Nothing Then
                Me.TxtStockUnits = txtStockUnits  ' Store reference to the class-level field
                ProductController.TxtStockUnits = txtStockUnits
                AddHandler txtStockUnits.KeyDown, AddressOf TxtStockUnits_KeyDown
                txtStockUnits.Text = "1"  ' Set default to 1 instead of 0
            End If

            Dim checkBoxSerialNumber As System.Windows.Controls.CheckBox = FindVisualChild(Of System.Windows.Controls.CheckBox)(SerialNumberContainer, "CheckBoxSerialNumber")
            If checkBoxSerialNumber IsNot Nothing Then
                ' Store a reference to the checkbox at the class level
                Me.CheckBoxSerialNumber = checkBoxSerialNumber
                ' Add event handler
                AddHandler checkBoxSerialNumber.Click, AddressOf IncludeSerial_Click
            End If

            Dim borderStockUnits As Border = FindVisualChild(Of Border)(DynamicFormContainer, "BorderStockUnits")
            If borderStockUnits IsNot Nothing Then
                Me.BorderStockUnits = borderStockUnits
            End If

            ' Load variation combinations - do this before adding any serial rows
            LoadVariationCombinations()

            ' Don't add any default rows here - let LoadVariationDetails handle it
            ' The variations will be initialized in LoadVariationCombinations

            ' Set default tax value
            Dim txtDefaultTax As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtDefaultTax")
            If txtDefaultTax IsNot Nothing Then
                txtDefaultTax.Text = "12"
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
            Dim comboBoxWarehouse As System.Windows.Controls.ComboBox = FindVisualChild(Of System.Windows.Controls.ComboBox)(DynamicFormContainer, "ComboBoxWarehouse")

            ' Get the variation data object
            Dim variationData As ProductVariationData = variationManager.GetVariationData(combinationName)

            ' Save form values
            If txtRetailPrice IsNot Nothing AndAlso Not String.IsNullOrEmpty(txtRetailPrice.Text) Then
                Decimal.TryParse(txtRetailPrice.Text, variationData.RetailPrice)
            End If

            If txtPurchaseOrder IsNot Nothing AndAlso Not String.IsNullOrEmpty(txtPurchaseOrder.Text) Then
                Decimal.TryParse(txtPurchaseOrder.Text, variationData.PurchaseOrder)
            End If

            If txtDefaultTax IsNot Nothing AndAlso Not String.IsNullOrEmpty(txtDefaultTax.Text) Then
                Decimal.TryParse(txtDefaultTax.Text, variationData.DefaultTax)
            End If

            If txtDiscountRate IsNot Nothing AndAlso Not String.IsNullOrEmpty(txtDiscountRate.Text) Then
                Decimal.TryParse(txtDiscountRate.Text, variationData.DiscountRate)
            End If

            If txtStockUnits IsNot Nothing AndAlso Not String.IsNullOrEmpty(txtStockUnits.Text) Then
                Integer.TryParse(txtStockUnits.Text, variationData.StockUnits)
            End If

            If txtAlertQuantity IsNot Nothing AndAlso Not String.IsNullOrEmpty(txtAlertQuantity.Text) Then
                Integer.TryParse(txtAlertQuantity.Text, variationData.AlertQuantity)
            End If

            If checkBoxSerialNumber IsNot Nothing Then
                variationData.IncludeSerialNumbers = checkBoxSerialNumber.IsChecked.GetValueOrDefault()
            End If

            ' Save warehouse selection
            If comboBoxWarehouse IsNot Nothing AndAlso comboBoxWarehouse.SelectedItem IsNot Nothing Then
                Dim item As ComboBoxItem = TryCast(comboBoxWarehouse.SelectedItem, ComboBoxItem)
                If item IsNot Nothing Then
                    variationData.WarehouseId = CInt(item.Tag)
                    variationData.SelectedWarehouseIndex = comboBoxWarehouse.SelectedIndex
                End If
            End If

            ' Extract serial numbers from UI
            variationData.SerialNumbers.Clear()
            If mainContainer IsNot Nothing Then
                For Each child As UIElement In mainContainer.Children
                    If TypeOf child Is Grid Then
                        Dim grid As Grid = DirectCast(child, Grid)
                        For Each gridChild As UIElement In grid.Children
                            If TypeOf gridChild Is System.Windows.Controls.TextBox Then
                                Dim txtSerial As System.Windows.Controls.TextBox = DirectCast(gridChild, System.Windows.Controls.TextBox)
                                If Not String.IsNullOrEmpty(txtSerial.Text) Then
                                    variationData.SerialNumbers.Add(txtSerial.Text)
                                End If
                                Exit For ' Found the textbox in this row, move to next row
                            End If
                        Next
                    End If
                Next
            End If
        End Sub
        Private Sub LoadFormData(combinationName As String)
            ' Get required text boxes and controls
            Dim txtRetailPrice As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtRetailPrice")
            Dim txtPurchaseOrder As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtPurchaseOrder")
            Dim txtDefaultTax As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtDefaultTax")
            Dim txtDiscountRate As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtDiscountRate")
            Dim txtStockUnits As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtStockUnits")
            Dim txtAlertQuantity As System.Windows.Controls.TextBox = FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtAlertQuantity")
            Dim checkBoxSerialNumber As System.Windows.Controls.CheckBox = FindVisualChild(Of System.Windows.Controls.CheckBox)(SerialNumberContainer, "CheckBoxSerialNumber")
            Dim comboBoxWarehouse As System.Windows.Controls.ComboBox = FindVisualChild(Of System.Windows.Controls.ComboBox)(DynamicFormContainer, "ComboBoxWarehouse")
            Dim mainContainer As StackPanel = FindVisualChild(Of StackPanel)(SerialNumberContainer, "MainContainer")

            ' Get the variation data from the manager
            Dim variationData As ProductVariationData = variationManager.GetVariationData(combinationName)

            ' Handle case where data doesn't exist yet
            If variationData Is Nothing Then
                ' Set defaults and return
                If txtDefaultTax IsNot Nothing Then txtDefaultTax.Text = "12"
                If txtStockUnits IsNot Nothing Then txtStockUnits.Text = "1" ' Changed from "0" to "1"
                Return
            End If

            ' Populate the form with data
            If txtRetailPrice IsNot Nothing Then txtRetailPrice.Text = variationData.RetailPrice.ToString()
            If txtPurchaseOrder IsNot Nothing Then txtPurchaseOrder.Text = variationData.PurchaseOrder.ToString()
            If txtDefaultTax IsNot Nothing Then txtDefaultTax.Text = variationData.DefaultTax.ToString()
            If txtDiscountRate IsNot Nothing Then txtDiscountRate.Text = variationData.DiscountRate.ToString()
            If txtStockUnits IsNot Nothing Then txtStockUnits.Text = variationData.StockUnits.ToString()
            If txtAlertQuantity IsNot Nothing Then txtAlertQuantity.Text = variationData.AlertQuantity.ToString()

            ' Set serial number checkbox
            If checkBoxSerialNumber IsNot Nothing Then
                checkBoxSerialNumber.IsChecked = variationData.IncludeSerialNumbers
            End If

            ' Set warehouse selection
            If comboBoxWarehouse IsNot Nothing Then
                If variationData.SelectedWarehouseIndex >= 0 AndAlso variationData.SelectedWarehouseIndex < comboBoxWarehouse.Items.Count Then
                    comboBoxWarehouse.SelectedIndex = variationData.SelectedWarehouseIndex
                ElseIf variationData.WarehouseId > 0 Then
                    For i As Integer = 0 To comboBoxWarehouse.Items.Count - 1
                        Dim item As ComboBoxItem = TryCast(comboBoxWarehouse.Items(i), ComboBoxItem)
                        If item IsNot Nothing AndAlso CInt(item.Tag) = variationData.WarehouseId Then
                            comboBoxWarehouse.SelectedIndex = i
                            variationData.SelectedWarehouseIndex = i ' Update the stored index
                            Exit For
                        End If
                    Next
                End If
            End If

            ' Clear existing serial number rows (ensure this happens whether or not serial numbers are enabled)
            If mainContainer IsNot Nothing Then
                mainContainer.Children.Clear()
                If ProductController.SerialNumbers IsNot Nothing Then
                    ProductController.SerialNumbers.Clear()
                End If
            End If

            ' Recreate serial number rows based on saved data
            If variationData.IncludeSerialNumbers Then
                ' Make sure the checkbox is checked to show serial numbers
                If checkBoxSerialNumber IsNot Nothing Then
                    checkBoxSerialNumber.IsChecked = True

                    ' Enable the serial number section in the UI
                    If BorderStockUnits IsNot Nothing Then
                        BorderStockUnits.BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#D9D9D9"))
                    End If

                    If mainContainer IsNot Nothing Then
                        mainContainer.Visibility = Visibility.Visible

                        ' Add each serial number row
                        If variationData.SerialNumbers IsNot Nothing AndAlso variationData.SerialNumbers.Count > 0 Then
                            For Each serialNumber As String In variationData.SerialNumbers
                                AddSerialRow(serialNumber)
                            Next
                        ElseIf txtStockUnits IsNot Nothing AndAlso Not String.IsNullOrEmpty(txtStockUnits.Text) Then
                            ' Create empty serial number rows based on stock units
                            Dim stockCount As Integer = 0
                            If Integer.TryParse(txtStockUnits.Text, stockCount) AndAlso stockCount > 0 Then
                                For i As Integer = 1 To stockCount
                                    AddSerialRow()
                                Next
                            End If
                        End If
                    End If
                End If
            ElseIf mainContainer IsNot Nothing Then
                mainContainer.Visibility = Visibility.Collapsed
            End If

            ' Update the selection title
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
            ' Get the current warehouse ComboBox
            Dim comboBoxWarehouse As System.Windows.Controls.ComboBox = FindVisualChild(Of System.Windows.Controls.ComboBox)(DynamicFormContainer, "ComboBoxWarehouse")

            Return ProductController.ValidateForm(
        FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtRetailPrice"),
        FindVisualChild(Of System.Windows.Controls.TextBox)(DynamicFormContainer, "TxtPurchaseOrder"),
        comboBoxWarehouse,
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

        ' Method to select the first variation automatically when form loads
        Private Sub SelectFirstVariationOnLoad()
            Try
                ' Wait for the form to completely load before attempting to select a variation
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, New Action(Sub()
                                                                                 ' Get the variations panel
                                                                                 Dim variationsPanel As StackPanel = FindName("VariationsPanel")
                                                                                 If variationsPanel IsNot Nothing AndAlso variationsPanel.Children.Count > 0 Then
                                                                                     ' Find the first button in the variations panel
                                                                                     For Each child As UIElement In variationsPanel.Children
                                                                                         If TypeOf child Is System.Windows.Controls.Button Then
                                                                                             Dim firstButton As System.Windows.Controls.Button = DirectCast(child, System.Windows.Controls.Button)

                                                                                             ' Simulate a click on the first button
                                                                                             firstButton.RaiseEvent(New RoutedEventArgs(System.Windows.Controls.Button.ClickEvent))

                                                                                             ' Only need to select the first one
                                                                                             Exit For
                                                                                         End If
                                                                                     Next
                                                                                 Else
                                                                                     ' If no variations exist yet, show appropriate message or default state
                                                                                     Dim titleTextBlock As TextBlock = TryCast(FindName("SelectedVariationTitle"), TextBlock)
                                                                                     If titleTextBlock IsNot Nothing Then
                                                                                         titleTextBlock.Text = "No variations available"
                                                                                     End If
                                                                                 End If
                                                                             End Sub))
            Catch ex As Exception
                ' Handle any exceptions
                Debug.WriteLine("Error selecting first variation: " & ex.Message)
            End Try
        End Sub
    End Class
End Namespace