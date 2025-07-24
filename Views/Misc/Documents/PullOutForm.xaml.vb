Imports System.Collections.ObjectModel
Imports System.Windows.Controls.Primitives
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel
Imports System.Windows.Threading
Imports DocumentFormat.OpenXml.Bibliography
Imports DocumentFormat.OpenXml.Presentation
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Controllers.Stocks
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports MongoDB.Driver.WriteConcern
Imports Newtonsoft.Json

Namespace DPC.Views.Misc.Documents
    Public Class PullOutForm
        Private MyDynamicGrid As Grid
        Private quantityList As New List(Of String)()
        Private namesList As New List(Of String)()
        Private notesList As New List(Of String)()
        Private stocksList As New List(Of String)()
        Private _typingTimer As DispatcherTimer
        Private rowCount As Integer = 0
        Private _productPopups As New Dictionary(Of String, Popup)
        Private _productListBoxes As New Dictionary(Of String, ListBox)
        Private _selectedProduct As ProductDataModel
        Private _productTypingTimers As New Dictionary(Of String, DispatcherTimer)
        Private _warehouseID As Integer
        Private _products As New ObservableCollection(Of ProductDataModel)

        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()
            ' Add any initialization after the InitializeComponent() call.

            ' Initialize typing timer for search delay
            _typingTimer = New DispatcherTimer With {
                .Interval = TimeSpan.FromMilliseconds(300)
            }

            MyDynamicGrid = CType(TableGridPanel.Children(0), Grid)

            ProductController.GetWarehouse(ComboBoxWarehouse)
            Dim selectedItem As ComboBoxItem = CType(ComboBoxWarehouse.SelectedItem, ComboBoxItem)
            _warehouseID = CType(selectedItem.Tag, Integer)

            txtPOR.Text = PullOutFormController.GeneratePOR()

        End Sub

        Private Sub ProceedButton(sender As Object, e As RoutedEventArgs)
            Dim itemsList As New List(Of Dictionary(Of String, String))()
            itemsList.Clear()

            For i As Integer = 0 To namesList.Count - 1
                Dim itemDictionary As New Dictionary(Of String, String)()

                Dim name As TextBox = GetTextBoxFromStackPanel($"{namesList(i)}")
                If name IsNot Nothing Then
                    itemDictionary.Add("ItemName", name.Text)
                End If

                Dim quantity As TextBox = GetTextBoxFromStackPanel($"{quantityList(i)}")
                If quantity IsNot Nothing Then
                    itemDictionary.Add("Quantity", quantity.Text)
                End If

                Dim notes As TextBox = GetTextBoxFromStackPanel($"{notesList(i)}")
                If notes IsNot Nothing Then
                    itemDictionary.Add("Notes", notes.Text)
                End If

                Dim stocks As TextBlock = GetTextBlockFromStackPanel($"{stocksList(i)}")
                If stocks IsNot Nothing Then
                    itemDictionary.Add("Stocks", stocks.Text)
                End If

                itemsList.Add(itemDictionary)
            Next


            PORDetails.PORNumber = txtPOR.Text
            PORDetails.PulloutTo = txtPORto.Text
            PORDetails.ItemsList = itemsList

            ViewLoader.DynamicView.NavigateToView("pulloutpreview", Me)
        End Sub

        Private Sub AddNewRow()
            rowCount += 1
            Dim row1 As New RowDefinition() With {.Height = GridLength.Auto}

            MyDynamicGrid.RowDefinitions.Add(row1)

            Dim newRowStart As Integer = MyDynamicGrid.RowDefinitions.Count - 1

            ' Create UI Elements (Matches XAML Layout)
            CreateStackPanelWithTextBox(newRowStart, 0, MyDynamicGrid) ' Quantity
            CreateStackPanelWithTextBox(newRowStart, 1, MyDynamicGrid, True) ' Item Name with autocomplete
            CreateStackPanelWithTextBox(newRowStart, 2, MyDynamicGrid) ' Notes
            CreateStackPanelWithTextBlock(newRowStart, 3, MyDynamicGrid) ' Stock
            CreateDeleteButtonStack(newRowStart, 4, MyDynamicGrid)
        End Sub

        Private Sub CreateDeleteButtonStack(row As Integer, column As Integer, GridName As Grid)
            ' Create StackPanel container
            Dim stackPanel As New StackPanel() With {
                .VerticalAlignment = VerticalAlignment.Center,
                .HorizontalAlignment = HorizontalAlignment.Center
            }

            ' Create Button
            Dim btn As New Button() With {
                .Background = Brushes.Transparent,
                .BorderBrush = Brushes.Transparent,
                .VerticalAlignment = VerticalAlignment.Center,
                .HorizontalAlignment = HorizontalAlignment.Center
            }

            Dim icon As New MaterialDesignThemes.Wpf.PackIcon() With {
                .Kind = MaterialDesignThemes.Wpf.PackIconKind.PlaylistRemove,
                .Foreground = Brushes.White,
                .Width = 35,
                .Height = 35,
                .VerticalAlignment = VerticalAlignment.Center,
                .HorizontalAlignment = HorizontalAlignment.Center
            }

            btn.Content = icon

            ' Attach delete functionality
            AddHandler btn.Click, Sub(sender As Object, e As RoutedEventArgs)
                                      Dim parentRow As Integer = Grid.GetRow(CType(sender, Button).Parent)
                                      RemoveRow(parentRow, GridName)
                                  End Sub

            ' Add button to stack panel
            stackPanel.Children.Add(btn)

            ' Set Grid position
            Grid.SetRow(stackPanel, row)
            Grid.SetColumn(stackPanel, column)

            ' Add to Grid
            GridName.Children.Add(stackPanel)
        End Sub

        ' Remove Row Functionality - Updated to clean up product autocomplete resources
        Private Sub RemoveRow(row As Integer, GridName As Grid)
            ' Step 1: Collect elements to remove
            Dim elementsToRemove As New List(Of UIElement)

            For Each element As UIElement In GridName.Children
                If Grid.GetRow(element) = row Then
                    elementsToRemove.Add(element)
                End If
            Next

            ' Step 2: Clean up autocomplete and lists
            namesList.RemoveAll(Function(name) name = $"txt_{row}_1")
            quantityList.RemoveAll(Function(name) name = $"txt_{row}_0")
            notesList.RemoveAll(Function(name) name = $"txt_{row}_2")
            stocksList.RemoveAll(Function(name) name = $"txt_{row}_3")

            ' Remove autocomplete items
            _productTypingTimers.TryGetValue($"ProductTimer_{row}", Nothing)
            If _productTypingTimers.ContainsKey($"ProductTimer_{row}") Then
                _productTypingTimers($"ProductTimer_{row}").Stop()
                _productTypingTimers.Remove($"ProductTimer_{row}")
            End If
            _productPopups.Remove($"ProductPopup_{row}")
            _productListBoxes.Remove($"LstProducts_{row}")

            ' Step 3: Unregister names and remove elements
            For Each element As UIElement In elementsToRemove
                If TypeOf element Is StackPanel Then
                    Dim stackPanel As StackPanel = CType(element, StackPanel)
                    For Each child As UIElement In stackPanel.Children
                        If TypeOf child Is Border Then
                            Dim border As Border = CType(child, Border)
                            If TypeOf border.Child Is TextBox Then
                                Dim txtBox As TextBox = CType(border.Child, TextBox)
                                If Not String.IsNullOrEmpty(txtBox.Name) AndAlso Me.FindName(txtBox.Name) IsNot Nothing Then
                                    Try
                                        Me.UnregisterName(txtBox.Name)
                                    Catch ex As ArgumentException
                                    End Try
                                End If
                            End If
                        ElseIf TypeOf child Is TextBlock Then
                            Dim txtBlock As TextBlock = CType(child, TextBlock)
                            If Not String.IsNullOrEmpty(txtBlock.Name) AndAlso Me.FindName(txtBlock.Name) IsNot Nothing Then
                                Try
                                    Me.UnregisterName(txtBlock.Name)
                                Catch ex As ArgumentException
                                End Try
                            End If
                        End If
                    Next
                End If
            Next

            ' Now remove them safely
            For Each element As UIElement In elementsToRemove
                GridName.Children.Remove(element)
            Next

            ' Step 4: Remove RowDefinition
            If row < GridName.RowDefinitions.Count Then
                GridName.RowDefinitions.RemoveAt(row)
            End If

            ' Step 5: Shift UI elements and names downward
            Dim elementsToShift As New List(Of UIElement)
            For Each element As UIElement In GridName.Children
                Dim currentRow As Integer = Grid.GetRow(element)
                If currentRow > row Then
                    elementsToShift.Add(element)
                End If
            Next

            For Each element As UIElement In elementsToShift
                Grid.SetRow(element, Grid.GetRow(element) - 1)
            Next

            ' Step 6: Shift internal lists and dictionaries
            Dim maxRow = rowCount - 1
            For i As Integer = row + 1 To maxRow
                ' Shift tracking list names
                ReplaceInList(namesList, $"txt_{i}_1", $"txt_{i - 1}_1")
                ReplaceInList(quantityList, $"txt_{i}_0", $"txt_{i - 1}_0")
                ReplaceInList(notesList, $"txt_{i}_2", $"txt_{i - 1}_2")
                ReplaceInList(stocksList, $"txt_{i}_3", $"txt_{i - 1}_3")

                ' Shift popup + autocomplete
                ShiftKey(_productTypingTimers, $"ProductTimer_{i}", $"ProductTimer_{i - 1}")
                ShiftKey(_productPopups, $"ProductPopup_{i}", $"ProductPopup_{i - 1}")
                ShiftKey(_productListBoxes, $"LstProducts_{i}", $"LstProducts_{i - 1}")
            Next

            ' Step 7: Reduce rowCount
            If rowCount > 0 Then rowCount -= 1
        End Sub

        ' --- Utility methods ---
        Private Sub ReplaceInList(lst As List(Of String), oldValue As String, newValue As String)
            Dim index As Integer = lst.IndexOf(oldValue)
            If index >= 0 Then
                lst(index) = newValue
            End If
        End Sub

        Private Sub ShiftKey(Of T)(dict As Dictionary(Of String, T), oldKey As String, newKey As String)
            If dict.ContainsKey(oldKey) Then
                dict(newKey) = dict(oldKey)
                dict.Remove(oldKey)
            End If
        End Sub


        Private Sub CreateStackPanelWithTextBlock(row As Integer, column As Integer, GridName As Grid)
            Dim txtName As String = $"txt_{row}_{column}"

            ' Check if the name already exists and unregister it
            Dim existingElement As Object = Me.FindName(txtName)
            If existingElement IsNot Nothing Then
                Me.UnregisterName(txtName)
            End If

            ' Create TextBox with adjustments for product name
            Dim txt As New TextBlock With {
                .Name = txtName,
                .Text = "0", ' Default text for quantity
                .FontFamily = New FontFamily("Lexend"),
                .Foreground = New SolidColorBrush(Colors.White),
                .FontWeight = FontWeights.SemiBold,
                .VerticalAlignment = VerticalAlignment.Center,
                .FontSize = 16,
                .Margin = New Thickness(2, 0, 0, 0),
                .Background = Brushes.Transparent
            }

            ' Create StackPanel to contain the Border
            Dim stackPanel As New StackPanel With {
                .VerticalAlignment = VerticalAlignment.Center,
                .Margin = New Thickness(2.5, 10, 2.5, 0)
            }

            stocksList.Add(txtName)

            stackPanel.Children.Add(txt)

            ' Set Grid position
            Grid.SetRow(stackPanel, row)
            Grid.SetColumn(stackPanel, column)

            ' Register the StackPanel with the TextBox name for reference
            Me.RegisterName(txtName, stackPanel)

            ' Add to Grid
            GridName.Children.Add(stackPanel)
        End Sub



        Private Sub CreateStackPanelWithTextBox(row As Integer, column As Integer, GridName As Grid, Optional isProductSearch As Boolean = False)
            Dim txtName As String = $"txt_{row}_{column}"

            ' Check if the name already exists and unregister it
            Dim existingElement As Object = Me.FindName(txtName)
            If existingElement IsNot Nothing Then
                Me.UnregisterName(txtName)
            End If

            ' Create TextBox with adjustments for product name
            Dim txt As New TextBox With {
                .Name = txtName,
                .Style = CType(Me.FindResource("RoundedTextboxStyle"), Style),
                .Margin = New Thickness(2, 0, 0, 0),
                .Background = Brushes.Transparent
            }

            ' For product name field, adjust to allow more text
            If column = 0 Then
                txt.TextWrapping = TextWrapping.NoWrap
                txt.MaxWidth = 500 ' Increase maximum width for product names
                txt.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto ' Enable horizontal scrolling if needed


            End If

            Select Case column
                Case 0
                    quantityList.Add(txtName)
                    txt.Text = 1
                Case 1
                    namesList.Add(txtName)
                Case 2
                    notesList.Add(txtName)
            End Select

            ' Create a Border and apply the style
            ' Wrap TextBox inside the Border
            Dim border As New Border With {
                .Style = CType(Me.FindResource("RoundedBorderStyle"), Style),
                .Child = txt
            }

            ' Create StackPanel to contain the Border
            Dim stackPanel As New StackPanel With {
                .Margin = New Thickness(2.5, 10, 2.5, 0)
            }

            stackPanel.Children.Add(border)

            ' Adjust border width for product name column
            If column = 0 Then
                border.MaxWidth = 500 ' Increase max width for the border too
            End If

            ' Attach numeric validation and event handlers
            If column = 0 Then
                AddHandler txt.PreviewTextInput, AddressOf ValidateNumericInput
                AddHandler txt.TextChanged, AddressOf ComputeStock
            End If

            ' If this is the product search field, add autocomplete functionality
            If isProductSearch Then
                ' Create autocomplete popup for this row
                Dim productPopup As Popup = CreateProductAutoCompletePopup(row)

                ' Add it to the grid at same position
                Grid.SetRow(productPopup, row)
                Grid.SetColumn(productPopup, column)
                GridName.Children.Add(productPopup)

                ' Add TextChanged event for autocomplete
                AddHandler txt.TextChanged, AddressOf ProductTextBox_TextChanged
            End If

            ' Set Grid position
            Grid.SetRow(stackPanel, row)
            Grid.SetColumn(stackPanel, column)

            ' Register the StackPanel with the TextBox name for reference
            Me.RegisterName(txtName, stackPanel)

            ' Add to Grid
            GridName.Children.Add(stackPanel)
        End Sub

        Private Sub ComputeStock(sender As Object, e As TextChangedEventArgs)
            Dim txt As TextBox = CType(sender, TextBox)

            Dim stockChecker As Integer = 0

            ' Extract row number from textbox name (e.g., "txt_2_1" → row 2)
            Dim parts As String() = txt.Name.Split("_"c)
            If parts.Length < 3 Then Exit Sub

            Dim row As Integer
            If Integer.TryParse(parts(1), row) Then
                Dim stocksTextblock As TextBlock = GetTextBlockFromStackPanel($"txt_{row}_3")
                Dim quantityTextBox As TextBox = GetTextBoxFromStackPanel($"txt_{row}_0")

                If Integer.TryParse(quantityTextBox.Text, Nothing) Then
                    stockChecker = CType(stocksTextblock.Text, Integer) - CType(quantityTextBox.Text, Integer)
                End If
            End If

            If stockChecker < 0 Then
                txt.Text = ""
                MessageBox.Show("Quantity exceeded stocks. Enter a valid quantity.")
                Exit Sub
            Else
                Exit Sub
            End If
        End Sub

        Private Sub ValidateNumericInput(sender As Object, e As TextCompositionEventArgs)
            Dim allowedPattern As String = "^[0-9.]+$" ' Allow only digits and decimal point

            If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, allowedPattern) Then

                e.Handled = True ' Reject input if it doesn't match
            End If
        End Sub

        Private Sub ProductTextBox_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim textBox As TextBox = CType(sender, TextBox)
            Dim parts As String() = textBox.Name.Split("_"c)
            If parts.Length < 3 Then Return

            Dim row As Integer = parts(1)
            'If Not Integer.TryParse(parts(1), row) Then Return

            Dim timerKey As String = $"ProductTimer_{row}"

            ' If timer doesn't exist for this row, create one
            If Not _productTypingTimers.ContainsKey(timerKey) Then
                Dim timer As New DispatcherTimer With {
                    .Interval = TimeSpan.FromMilliseconds(300)
                }



                ' Add a closure to capture the row
                Dim rowCaptured As Integer = row
                AddHandler timer.Tick, Sub(s, args)
                                           OnProductTypingTimerTick(s, args, rowCaptured)
                                       End Sub

                _productTypingTimers.Add(timerKey, timer)
            End If

            ' Reset and start timer
            _productTypingTimers(timerKey).Stop()

            Dim popupKey As String = $"ProductPopup_{row}"

            ' Close popup if textbox is empty
            If String.IsNullOrWhiteSpace(textBox.Text) Then

                If _productPopups.ContainsKey(popupKey) Then
                    _productPopups(popupKey).IsOpen = False
                End If
                Return
            End If

            ' Start timer
            _productTypingTimers(timerKey).Start()

        End Sub

        Private Sub OnProductTypingTimerTick(sender As Object, e As EventArgs, row As Integer)

            ' Stop the timer
            Dim timerKey As String = $"ProductTimer_{row}"
            If _productTypingTimers.ContainsKey(timerKey) Then
                _productTypingTimers(timerKey).Stop()
            End If

            ' Get the textbox
            Dim textBoxName As String = $"txt_{row}_1"
            Dim textBox As TextBox = GetTextBoxFromStackPanel(textBoxName)
            Dim stockBoxName As String = $"txt_{row}_3"
            Dim stock As TextBlock = GetTextBlockFromStackPanel(stockBoxName)

            If textBox Is Nothing Then Return

            ' Get the popup and listbox
            Dim popupKey As String = $"ProductPopup_{row}"
            Dim listBoxKey As String = $"LstProducts_{row}"

            If Not _productPopups.ContainsKey(popupKey) Or Not _productListBoxes.ContainsKey(listBoxKey) Then Return


            Dim popup As Popup = _productPopups(popupKey)
            Dim listBox As ListBox = _productListBoxes(listBoxKey)
            AddHandler listBox.SelectionChanged, Sub()
                                                     _selectedProduct = listBox.SelectedItem

                                                     If _selectedProduct IsNot Nothing Then
                                                         textBox.Text = _selectedProduct.ProductName
                                                         stock.Text = _selectedProduct.StockUnits
                                                     End If
                                                 End Sub

            ''Search for products from the supplier
            'If ComboBoxWarehouse.S IsNot Nothing Then
            ' Call product controller to search products by supplier ID and search text
            _products = PullOutFormController.SearchProducts(textBox.Text, _warehouseID)

            ' Update the ListBox
            listBox.ItemsSource = _products

            ' Show popup if we have results
            If _products IsNot Nothing Then
                popup.PlacementTarget = textBox
                popup.IsOpen = _products.Count > 0
                popup.PlacementTarget = textBox
                popup.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom
            End If

            CType(popup.Child, Border).Width = textBox.ActualWidth
            ''End If
        End Sub

        Private Sub ItemsFromWarehouse(sender As Object, e As SelectionChangedEventArgs)
            Dim selectedItem As ComboBoxItem = CType(ComboBoxWarehouse.SelectedItem, ComboBoxItem)
            _warehouseID = CType(selectedItem.Tag, Integer)
            ClearAllRows(MyDynamicGrid)
            AddNewRow()
        End Sub

        Private Sub ClearAllRows(GridName As Grid)
            ' Step 1: Remove all children from row index 1 and onward
            Dim elementsToRemove As New List(Of UIElement)

            For Each element As UIElement In GridName.Children
                If Grid.GetRow(element) >= 1 Then
                    elementsToRemove.Add(element)
                End If
            Next

            For Each element As UIElement In elementsToRemove
                GridName.Children.Remove(element)
            Next

            ' Step 2: Remove RowDefinitions from index 1 and onward
            While GridName.RowDefinitions.Count > 1
                GridName.RowDefinitions.RemoveAt(1)
            End While

            ' Step 3: Reset rowCount (to 1 if row 0 is header, or 0 if you track only data)
            rowCount = 0

            ' Step 4: Clear all associated lists and dictionaries
            namesList.Clear()
            quantityList.Clear()
            notesList.Clear()
            stocksList.Clear()

            _productTypingTimers.Clear()
            _productPopups.Clear()
            _productListBoxes.Clear()
        End Sub



        Private Function CreateProductAutoCompletePopup(row As Integer) As Popup
            ' Create a ListBox for product items
            Dim lstProducts As New ListBox()
            lstProducts.Name = $"LstProducts_{row}"

            ' Create a ItemTemplate for the ListBox
            Dim template As DataTemplate = CreateProductItemTemplate()
            lstProducts.ItemTemplate = template

            ' Create Border to contain the ListBox
            Dim border As New Border With {
        .Background = Brushes.White,
        .BorderBrush = Brushes.LightGray,
        .BorderThickness = New Thickness(1),
        .MaxHeight = 150
    }
            border.Child = lstProducts

            ' Create Popup
            Dim popup As New Popup With {
        .StaysOpen = False,
        .IsOpen = False,
        .AllowsTransparency = True,
        .PopupAnimation = PopupAnimation.Fade,
        .Child = border
    }

            ' Store references for later use - check if keys already exist first
            Dim popupKey As String = $"ProductPopup_{row}"
            Dim listBoxKey As String = $"LstProducts_{row}"

            ' Remove existing entries if they exist
            If _productPopups.ContainsKey(popupKey) Then
                _productPopups.Remove(popupKey)
            End If

            If _productListBoxes.ContainsKey(listBoxKey) Then
                _productListBoxes.Remove(listBoxKey)
            End If

            ' Now safely add the new entries
            _productPopups.Add(popupKey, popup)
            _productListBoxes.Add(listBoxKey, lstProducts)

            ' Add handlers
            AddHandler lstProducts.SelectionChanged, AddressOf ProductList_SelectionChanged

            Return popup
        End Function

        Private Function CreateProductItemTemplate() As DataTemplate
            ' Create a DataTemplate in code
            Dim template As New DataTemplate()

            ' Create the root FrameworkElementFactory
            Dim stackPanelFactory As New FrameworkElementFactory(GetType(StackPanel))
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal)
            stackPanelFactory.SetValue(StackPanel.MarginProperty, New Thickness(5))

            ' Add product ID and name TextBlock
            Dim productTextFactory As New FrameworkElementFactory(GetType(TextBlock))
            ' Use a multi-binding to combine ProductID and ProductName
            Dim multiBinding As New MultiBinding()
            multiBinding.StringFormat = "{0} - {1}" ' Format as "ProductID - ProductName"

            ' Add the ProductID binding
            Dim idBinding As New Binding("ProductID")
            multiBinding.Bindings.Add(idBinding)

            ' Add the ProductName binding
            Dim nameBinding As New Binding("ProductName")
            multiBinding.Bindings.Add(nameBinding)

            ' Set the combined binding to the Text property
            productTextFactory.SetBinding(TextBlock.TextProperty, multiBinding)
            productTextFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Normal)

            stackPanelFactory.AppendChild(productTextFactory)

            ' Set the root of the DataTemplate
            template.VisualTree = stackPanelFactory

            Return template
        End Function

        Private Sub ProductList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim listBox As ListBox = CType(sender, ListBox)

            If listBox Is Nothing Or listBox.SelectedItem Is Nothing Then Return

            ' Extract the row from the ListBox name
            Dim parts As String() = listBox.Name.Split("_"c)
            If parts.Length < 2 Then Return

            Dim row As Integer
            If Not Integer.TryParse(parts(1), row) Then Return

            ' Get selected product
            _selectedProduct = CType(listBox.SelectedItem, ProductDataModel)

            ' Define popupKey once for reuse
            Dim popupKey As String = $"ProductPopup_{row}"

            ' Check if this product is already added in another row
            If IsProductAlreadyAdded(_selectedProduct.ProductID, row) Then
                ' Show error message
                MessageBox.Show($"The product '{_selectedProduct.ProductName}' is already added to this purchase order.",
                        "Duplicate Product",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning)

                ' Close popup without adding the product
                If _productPopups.ContainsKey(popupKey) Then
                    _productPopups(popupKey).IsOpen = False
                End If

                Return ' Exit the method without updating the row
            End If

            ' Update product details
            UpdateProductRow(row, _selectedProduct)

            ' Close popup
            If _productPopups.ContainsKey(popupKey) Then
                _productPopups(popupKey).IsOpen = False
            End If
        End Sub

        Private Function IsProductAlreadyAdded(productID As String, currentRow As Integer) As Boolean
            Dim isDuplicate As Boolean = False

            ' Loop through all product rows to check if this product exists elsewhere
            For row As Integer = 0 To rowCount - 1
                ' Skip checking the current row
                If row = currentRow Then Continue For

                ' Get the product textbox from this row
                Dim productTextBox As TextBox = GetTextBoxFromStackPanel($"txt_{row}_0")
                If productTextBox IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(productTextBox.Text) Then
                    ' Check if this textbox contains the same product ID
                    ' The format is usually "ProductID - ProductName"
                    If productTextBox.Text.StartsWith(productID & " - ") Then
                        isDuplicate = True
                        Exit For
                    End If
                End If
            Next

            Return isDuplicate
        End Function

        Private Function GetTextBoxFromStackPanel(stackPanelName As String) As TextBox
            Dim stackPanel As StackPanel = TryCast(Me.FindName(stackPanelName), StackPanel)
            If stackPanel IsNot Nothing AndAlso stackPanel.Children.Count > 0 AndAlso TypeOf stackPanel.Children(0) Is Border Then
                Dim border As Border = CType(stackPanel.Children(0), Border)
                If border.Child IsNot Nothing AndAlso TypeOf border.Child Is TextBox Then
                    Return CType(border.Child, TextBox)
                End If
            End If
            Return Nothing
        End Function

        Private Function GetTextBlockFromStackPanel(stackPanelName As String) As TextBlock
            Dim stackPanel As StackPanel = TryCast(Me.FindName(stackPanelName), StackPanel)
            If stackPanel IsNot Nothing AndAlso stackPanel.Children.Count > 0 AndAlso TypeOf stackPanel.Children(0) Is TextBlock Then
                Return CType(stackPanel.Children(0), TextBlock)
            End If
            Return Nothing
        End Function

        Private Function GetTextBoxFromBorder(borderName As String) As TextBox
            Dim border As Border = TryCast(Me.FindName(borderName), Border)
            If border IsNot Nothing AndAlso TypeOf border.Child Is TextBox Then
                Return CType(border.Child, TextBox)
            End If
            Return Nothing
        End Function

        Private Sub UpdateProductRow(row As Integer, product As ProductDataModel)
            ' Update product name to include the ID
            Dim nameTextBox As TextBox = GetTextBoxFromBorder($"txt_{row}_0")
            If nameTextBox IsNot Nothing Then
                nameTextBox.Text = $"{product.ProductID} - {product.ProductName}"
            End If

            ' Update price/rate
            Dim rateTextBox As TextBox = GetTextBoxFromBorder($"txt_{row}_2")
            If rateTextBox IsNot Nothing AndAlso product.BuyingPrice > 0 Then
                rateTextBox.Text = product.BuyingPrice.ToString("0.00")
            End If

            ' Update tax percentage if available
            Dim taxTextBox As TextBox = GetTextBoxFromBorder($"txt_{row}_3")
            If taxTextBox IsNot Nothing AndAlso product.DefaultTax > 0 Then
                taxTextBox.Text = product.DefaultTax.ToString("0.00")
            End If

            ' Set default quantity to 1 if it's empty
            Dim quantityTextBox As TextBox = GetTextBoxFromBorder($"txt_{row}_1")
            If quantityTextBox IsNot Nothing AndAlso String.IsNullOrWhiteSpace(quantityTextBox.Text) Then
                quantityTextBox.Text = "1"
            End If

        End Sub
    End Class
End Namespace

