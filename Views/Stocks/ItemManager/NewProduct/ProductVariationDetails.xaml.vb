Imports System.IO
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DocumentFormat.OpenXml.Office.CustomUI
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports MaterialDesignThemes.Wpf.Theme
Imports Microsoft.Win32



Namespace DPC.Views.Stocks.ItemManager.NewProduct
    Public Class ProductVariationDetails
        Inherits Window

        Private variationManager As ProductVariationManager
        Public Sub New()
            InitializeComponent()

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNav

            ProductController.GetWarehouse(ComboBoxWarehouse)

            Dim calendarViewModel As New CalendarController.SingleCalendar()
            Me.DataContext = calendarViewModel

            ProductController.MainContainer = MainContainer
            ProductController.TxtStockUnits = TxtStockUnits

            ProductController.BtnAddRow_Click(Nothing, Nothing)

            TxtDefaultTax.Text = 12

            variationManager = New ProductVariationManager()
            ' Initialize dynamic variations panel
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
                ' No variations, add a placeholder
                AddVariationButton(variationsPanel, "No Variations", True)
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

            ' Load data for the selected combination
            LoadFormData(combinationName)

            ' Update the selection title (if it exists)
            Dim titleTextBlock As TextBlock = TryCast(FindName("SelectedVariationTitle"), TextBlock)
            If titleTextBlock IsNot Nothing Then
                titleTextBlock.Text = $"Selected: {combinationName}"
            End If
        End Sub

        ' To be called when the form loads
        Public Sub InitializeVariations()
            ' Call this during form initialization
            LoadVariationCombinations()

            ' Set the first variation as selected by default
            Dim variationsPanel As StackPanel = TryCast(FindName("VariationsPanel"), StackPanel)
            If variationsPanel IsNot Nothing AndAlso variationsPanel.Children.Count > 0 Then
                Dim firstButton As System.Windows.Controls.Button = TryCast(variationsPanel.Children(0), System.Windows.Controls.Button)
                If firstButton IsNot Nothing Then
                    ' Simulate click on first button
                    firstButton.RaiseEvent(New RoutedEventArgs(System.Windows.Controls.Button.ClickEvent))
                End If
            End If
        End Sub

        Private Sub SaveCurrentFormData(combinationName As String)
            ' Get the current variation data
            Dim variationData As ProductVariationData = variationManager.GetVariationData(combinationName)

            ' Save form values to the model
            If TxtRetailPrice.Text.Trim() <> "" Then
                variationData.RetailPrice = Decimal.Parse(TxtRetailPrice.Text)
            End If

            If TxtPurchaseOrder.Text.Trim() <> "" Then
                variationData.PurchaseOrder = Decimal.Parse(TxtPurchaseOrder.Text)
            End If

            If TxtDefaultTax.Text.Trim() <> "" Then
                variationData.DefaultTax = Decimal.Parse(TxtDefaultTax.Text)
            End If

            If TxtDiscountRate.Text.Trim() <> "" Then
                variationData.DiscountRate = Decimal.Parse(TxtDiscountRate.Text)
            End If

            If TxtStockUnits.Text.Trim() <> "" Then
                variationData.StockUnits = Integer.Parse(TxtStockUnits.Text)
            End If

            If TxtAlertQuantity.Text.Trim() <> "" Then
                variationData.AlertQuantity = Integer.Parse(TxtAlertQuantity.Text)
            End If

            ' Save serial number settings
            variationData.IncludeSerialNumbers = CheckBoxSerialNumber.IsChecked.Value

            ' Save serial numbers if needed
            If variationData.IncludeSerialNumbers Then
                variationData.SerialNumbers.Clear()
                ' Save serial numbers from your serial number controls
                ' This depends on how your ProductController stores serial numbers
                ' For example:
                For Each serialNumber As System.Windows.Controls.TextBox In ProductController.SerialNumbers
                    variationData.SerialNumbers.Add(serialNumber.Text)
                Next

            End If
        End Sub

        Private Sub LoadFormData(combinationName As String)
            ' Get the variation data
            Dim variationData As ProductVariationData = variationManager.GetVariationData(combinationName)

            ' Load data into form controls
            TxtRetailPrice.Text = If(variationData.RetailPrice = 0, "", variationData.RetailPrice.ToString())
            TxtPurchaseOrder.Text = If(variationData.PurchaseOrder = 0, "", variationData.PurchaseOrder.ToString())
            TxtDefaultTax.Text = If(variationData.DefaultTax = 0, "12", variationData.DefaultTax.ToString())
            TxtDiscountRate.Text = If(variationData.DiscountRate = 0, "", variationData.DiscountRate.ToString())
            TxtStockUnits.Text = If(variationData.StockUnits = 0, "", variationData.StockUnits.ToString())
            TxtAlertQuantity.Text = If(variationData.AlertQuantity = 0, "", variationData.AlertQuantity.ToString())

            ' Set checkbox state
            CheckBoxSerialNumber.IsChecked = variationData.IncludeSerialNumbers

            ' Update SerialNumber section visibility
            SerialNumberChecker(CheckBoxSerialNumber)

            ' Clear existing serial numbers
            ProductController.SerialNumbers.Clear()
            MainContainer.Children.Clear()

            ' If we have serial numbers, load them
            If variationData.IncludeSerialNumbers And variationData.SerialNumbers.Count > 0 Then
                ' Load serial numbers into UI
                For Each serialNumber As String In variationData.SerialNumbers
                    ' Add a row for each serial number
                    ProductController.BtnAddRow_Click(Nothing, Nothing)

                    ' Now set the value of the last added row
                    ' This might need customization based on how your ProductController creates rows
                    If MainContainer.Children.Count > 0 Then
                        Dim lastGrid As Grid = TryCast(MainContainer.Children(MainContainer.Children.Count - 1), Grid)
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
    End Class
End Namespace