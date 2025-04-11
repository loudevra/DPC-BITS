Imports System.Collections.ObjectModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports System.Windows.Media

Namespace DPC.Components.Forms
    Public Class AutocompleteHelper(Of T)
        ' Observable collection for selected items
        Private _selectedItems As New ObservableCollection(Of T)()

        ' Property to access selected items from outside
        Public ReadOnly Property SelectedItems As ObservableCollection(Of T)
            Get
                Return _selectedItems
            End Get
        End Property

        ' Event that fires when selected items change
        Public Event SelectedItemsChanged(sender As Object, items As ObservableCollection(Of T))

        ' Delegates for accessing item properties
        Private _getItemId As Func(Of T, Object)
        Private _getItemName As Func(Of T, String)
        Private _createUIElement As Func(Of T, UIElement)
        Private _getAdditionalSearchField As Func(Of T, String)
        Private _useAdditionalField As Boolean = False

        ' Popup reference
        Private _autoCompletePopup As Popup

        ' Constructor
        Public Sub New(getItemId As Func(Of T, Object), getItemName As Func(Of T, String), Optional getAdditionalSearchField As Func(Of T, String) = Nothing)
            _getItemId = getItemId
            _getItemName = getItemName

            If getAdditionalSearchField IsNot Nothing Then
                _getAdditionalSearchField = getAdditionalSearchField
                _useAdditionalField = True
            End If

            ' Default chip creation function
            _createUIElement = Function(item)
                                   Dim chip As New Border With {
                                       .Background = New SolidColorBrush(Color.FromRgb(82, 198, 157)),  ' #52C69D
                                       .BorderBrush = New SolidColorBrush(Color.FromRgb(82, 198, 157)),
                                       .CornerRadius = New CornerRadius(15),
                                       .Margin = New Thickness(5),
                                       .Padding = New Thickness(8, 4, 8, 4)
                                   }

                                   Dim panel As New StackPanel With {
                                       .Orientation = Orientation.Horizontal
                                   }

                                   Dim text As New TextBlock With {
                                       .Text = _getItemName(item),
                                       .Foreground = Brushes.White,
                                       .VerticalAlignment = VerticalAlignment.Center,
                                       .Margin = New Thickness(0, 0, 5, 0)
                                   }

                                   Dim closeIcon As New TextBlock With {
                                       .Text = "×",  ' × is the Unicode multiplication sign
                                       .FontSize = 16,
                                       .Foreground = Brushes.White,
                                       .VerticalAlignment = VerticalAlignment.Center,
                                       .Cursor = Cursors.Hand
                                   }

                                   panel.Children.Add(text)
                                   panel.Children.Add(closeIcon)
                                   chip.Child = panel

                                   Return chip
                               End Function
        End Sub

        ' Method to set custom chip creation function
        Public Sub SetChipCreator(createChipFunc As Func(Of T, UIElement))
            _createUIElement = createChipFunc
        End Sub

        ' Initialize the control with all necessary UI elements
        Public Sub Initialize(textBox As TextBox, listBox As ListBox, chipPanel As Panel, popup As Popup, itemsSource As IEnumerable(Of T))
            ' Store popup reference
            _autoCompletePopup = popup

            ' Configure popup if needed
            If _autoCompletePopup.PlacementTarget Is Nothing Then
                _autoCompletePopup.PlacementTarget = textBox
                _autoCompletePopup.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom
                _autoCompletePopup.StaysOpen = False
                _autoCompletePopup.AllowsTransparency = True
            End If

            ' Set up event handlers
            AddHandler textBox.TextChanged, Sub(sender, e) HandleTextChanged(sender, e, itemsSource, listBox)
            AddHandler textBox.KeyDown, Sub(sender, e) HandleKeyDown(sender, e, listBox, textBox, chipPanel)
            AddHandler listBox.SelectionChanged, Sub(sender, e) HandleSelectionChanged(sender, e, listBox, textBox, chipPanel)
        End Sub

        ' Clear all selected items
        Public Sub ClearSelection(chipPanel As Panel)
            _selectedItems.Clear()
            chipPanel.Children.Clear()
            RaiseEvent SelectedItemsChanged(Me, _selectedItems)
        End Sub

        ' Load existing selections
        Public Sub LoadExistingSelections(items As IEnumerable(Of T), chipPanel As Panel)
            For Each item In items
                If Not _selectedItems.Any(Function(i) Object.Equals(_getItemId(i), _getItemId(item))) Then
                    AddItemToSelection(item, chipPanel)
                End If
            Next
        End Sub

        ' Method to handle TextChanged event for filtering items
        Public Sub HandleTextChanged(sender As Object, e As TextChangedEventArgs, itemsSource As IEnumerable(Of T), listBox As ListBox)
            Dim textBox = DirectCast(sender, TextBox)
            Dim searchText = textBox.Text.ToLower()

            If String.IsNullOrWhiteSpace(searchText) Then
                _autoCompletePopup.IsOpen = False
                Return
            End If

            ' Filter items based on search text - now with condition for additional field
            Dim filteredItems As IEnumerable(Of T)

            If _useAdditionalField Then
                ' Search by both name and additional field
                filteredItems = itemsSource.
            Where(Function(i) _getItemName(i).ToLower().Contains(searchText) OrElse
                          _getAdditionalSearchField(i).ToLower().Contains(searchText) AndAlso
                          Not _selectedItems.Any(Function(si) Object.Equals(_getItemId(si), _getItemId(i)))).
            ToList()
            Else
                ' Original search by name only
                filteredItems = itemsSource.
            Where(Function(i) _getItemName(i).ToLower().Contains(searchText) AndAlso
                          Not _selectedItems.Any(Function(si) Object.Equals(_getItemId(si), _getItemId(i)))).
            ToList()
            End If

            ' Update ListBox and show/hide popup
            listBox.ItemsSource = filteredItems
            _autoCompletePopup.IsOpen = filteredItems.Any()
        End Sub

        ' Method to handle KeyDown event (Enter, Escape, Backspace)
        Public Sub HandleKeyDown(sender As Object, e As KeyEventArgs, listBox As ListBox, textBox As TextBox, chipPanel As Panel)
            ' Handle Enter key to select the currently highlighted item
            If e.Key = Key.Enter AndAlso listBox.SelectedItem IsNot Nothing Then
                AddItemToSelection(DirectCast(listBox.SelectedItem, T), chipPanel)
                textBox.Clear()
                _autoCompletePopup.IsOpen = False
                e.Handled = True

                ' Handle Escape key to close the popup
            ElseIf e.Key = Key.Escape Then
                _autoCompletePopup.IsOpen = False
                e.Handled = True

                ' Handle Backspace key to remove the last chip when the textbox is empty
            ElseIf e.Key = Key.Back AndAlso String.IsNullOrEmpty(textBox.Text) AndAlso _selectedItems.Count > 0 Then
                RemoveLastChip(chipPanel)
                e.Handled = True
            End If
        End Sub

        ' Method to handle ListBox selection changed event
        Public Sub HandleSelectionChanged(sender As Object, e As SelectionChangedEventArgs, listBox As ListBox, textBox As TextBox, chipPanel As Panel)
            If listBox.SelectedItem IsNot Nothing Then
                ' Add the selected item as a chip
                AddItemToSelection(DirectCast(listBox.SelectedItem, T), chipPanel)
                textBox.Clear()
                _autoCompletePopup.IsOpen = False

                ' Clear selection to allow selecting the same item again
                listBox.SelectedItem = Nothing
            End If
        End Sub

        ' Method to add an item to the selection
        Public Sub AddItemToSelection(item As T, chipPanel As Panel)
            If Not _selectedItems.Any(Function(i) Object.Equals(_getItemId(i), _getItemId(item))) Then
                ' Add to internal collection
                _selectedItems.Add(item)

                ' Create chip UI element
                Dim chip = _createUIElement(item)

                ' Add click handler if it's a Border (the default implementation)
                If TypeOf chip Is Border Then
                    Dim border = DirectCast(chip, Border)
                    AddHandler border.MouseLeftButtonDown, Sub(sender, e) RemoveItemFromSelection(item, border, chipPanel)
                End If

                ' Add chip to panel
                chipPanel.Children.Add(chip)

                ' Raise event
                RaiseEvent SelectedItemsChanged(Me, _selectedItems)
            End If
        End Sub

        ' Method to remove an item from the selection
        Public Sub RemoveItemFromSelection(item As T, chip As UIElement, chipPanel As Panel)
            _selectedItems.Remove(item)
            chipPanel.Children.Remove(chip)
            RaiseEvent SelectedItemsChanged(Me, _selectedItems)
        End Sub

        ' Method to remove the last chip from the panel
        Public Sub RemoveLastChip(chipPanel As Panel)
            If _selectedItems.Count > 0 Then
                Dim lastItem = _selectedItems.Last()

                ' Get the last chip element
                Dim lastChip = chipPanel.Children(chipPanel.Children.Count - 1)

                ' Remove from panel and collection
                chipPanel.Children.RemoveAt(chipPanel.Children.Count - 1)
                _selectedItems.Remove(lastItem)

                ' Raise event
                RaiseEvent SelectedItemsChanged(Me, _selectedItems)
            End If
        End Sub
    End Class
End Namespace