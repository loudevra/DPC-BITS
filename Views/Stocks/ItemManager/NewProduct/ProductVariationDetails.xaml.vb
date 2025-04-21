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
        Inherits Window

#Region "Initialization and Constructor"
        Public Sub New()
            InitializeComponent()

            ' Load UI components
            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNav

            Dim calendarViewModel As New CalendarController.SingleCalendar()
            Me.DataContext = calendarViewModel

            ' Populate the warehouse combo box with data from the database
            ProductController.GetWarehouse(ComboBoxWarehouse)

            ' Initialize serial number controls
            InitializeSerialNumberControls()

            ' Load variation data
            LoadVariationData()

            ' Setup save button event handler
            AddHandler BtnSave.Click, AddressOf SaveVariationData

            ' Add serial number checkbox event handler
            AddHandler CheckBoxSerialNumber.Click, AddressOf SerialNumberCheckboxChanged

            ' Add TextChanged event handler for stock units textbox to update serial number fields
            AddHandler TxtStockUnits.TextChanged, AddressOf StockUnits_TextChanged
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
                ' Create textboxes based on stock units
                For i As Integer = 1 To stockUnits
                    AddSerialNumberRow(serialNumbersContainer, i)
                Next
            End If
        End Sub

        ''' <summary>
        ''' Adds a new serial number row to the specified container
        ''' </summary>
        Private Sub AddSerialNumberRow(container As StackPanel, rowIndex As Integer)
            Dim outerStackPanel As New StackPanel With {.Margin = New Thickness(25, 10, 10, 10)}

            Dim grid As New Grid()
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            ' Create the text box with style
            Dim textBox As New TextBox With {
                .Name = $"TxtSerial_{rowIndex}",
                .Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style)
            }
            ProductController.serialNumberTextBoxes.Add(textBox)

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
                .Tag = outerStackPanel  ' Pass the row as Tag for easy access
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

            ' Row Controller Button (we'll keep it but not implement for now)
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
            ' We'll leave this handler as is for now
            AddHandler rowControllerButton.Click, AddressOf ProductController.BtnRowController_Click
            buttonPanel.Children.Add(rowControllerButton)

            Grid.SetColumn(buttonPanel, 1)
            grid.Children.Add(buttonPanel)
            outerStackPanel.Children.Add(grid)

            ' Add the row to the container
            container.Children.Add(outerStackPanel)
        End Sub

        ''' <summary>
        ''' Handles the Add Serial Row button click
        ''' </summary>
        Private Sub BtnAddSerialRow_Click(sender As Object, e As RoutedEventArgs)
            Dim button = TryCast(sender, Button)
            If button Is Nothing Then Return

            ' Get the container from the button's Tag property
            Dim container = TryCast(button.Tag, StackPanel)
            If container Is Nothing Then Return

            ' Store existing serial numbers before adding a new row
            Dim serialValues As New List(Of String)()
            For Each textBox In ProductController.serialNumberTextBoxes
                serialValues.Add(textBox.Text)
            Next

            ' Add a new row with index based on current count
            Dim newIndex = container.Children.Count + 1
            AddSerialNumberRow(container, newIndex)

            ' Update the stock units textbox to match the number of rows
            TxtStockUnits.Text = container.Children.Count.ToString()

            ' Update our current variation data with the new count
            Dim currentData = ProductController.variationManager.GetCurrentVariationData()
            If currentData IsNot Nothing Then
                currentData.StockUnits = container.Children.Count
            End If

            ' Restore the values of existing textboxes
            For i As Integer = 0 To Math.Min(serialValues.Count - 1, ProductController.serialNumberTextBoxes.Count - 1)
                ProductController.serialNumberTextBoxes(i).Text = serialValues(i)
            Next

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
            Dim button = TryCast(sender, Button)
            If button Is Nothing Then Return

            ' Get the row panel directly from the button's Tag property
            Dim rowPanel = TryCast(button.Tag, StackPanel)
            If rowPanel Is Nothing Then Return

            ' Find the container that holds all the serial number rows
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

            ' Find the index of this row in the container
            Dim rowIndex = container.Children.IndexOf(rowPanel)

            ' Store all the current text values from textboxes
            Dim serialValues As New Dictionary(Of Integer, String)()
            For i As Integer = 0 To ProductController.serialNumberTextBoxes.Count - 1
                ' Skip the row being removed
                If i <> rowIndex Then
                    serialValues.Add(i, ProductController.serialNumberTextBoxes(i).Text)
                End If
            Next

            ' Remove the row from the UI
            container.Children.Remove(rowPanel)

            ' Rebuild the serialNumberTextBoxes collection
            ProductController.serialNumberTextBoxes.Clear()

            ' Find all text boxes in the remaining rows and add them to the collection
            For i As Integer = 0 To container.Children.Count - 1
                Dim currentRowPanel = TryCast(container.Children(i), StackPanel)
                If currentRowPanel IsNot Nothing Then
                    Dim grid = TryCast(currentRowPanel.Children(0), Grid)
                    If grid IsNot Nothing Then
                        Dim textBoxBorder = TryCast(grid.Children(0), Border)
                        If textBoxBorder IsNot Nothing Then
                            Dim textBox = TryCast(textBoxBorder.Child, TextBox)
                            If textBox IsNot Nothing Then
                                textBox.Name = $"TxtSerial_{i + 1}"
                                ProductController.serialNumberTextBoxes.Add(textBox)
                            End If
                        End If
                    End If
                End If
            Next

            ' Restore the values to the text boxes
            For i As Integer = 0 To ProductController.serialNumberTextBoxes.Count - 1
                ' Calculate the original index before removal
                Dim originalIndex = If(i >= rowIndex, i + 1, i)
                If serialValues.ContainsKey(originalIndex) Then
                    ProductController.serialNumberTextBoxes(i).Text = serialValues(originalIndex)
                End If
            Next

            ' Update the stock units textbox to match the number of rows
            TxtStockUnits.Text = container.Children.Count.ToString()

            ' Update our current variation data with the new count and serial numbers
            Dim currentData = ProductController.variationManager.GetCurrentVariationData()
            If currentData IsNot Nothing Then
                currentData.StockUnits = container.Children.Count

                ' Update the serial numbers in the data model
                If currentData.SerialNumbers IsNot Nothing AndAlso currentData.IncludeSerialNumbers Then
                    currentData.SerialNumbers.Clear()
                    For Each textBox In ProductController.serialNumberTextBoxes
                        currentData.SerialNumbers.Add(textBox.Text)
                    Next
                End If
            End If
        End Sub

        ''' <summary>
        ''' Updates the indices of all serial number rows after a removal
        ''' </summary>
        Private Sub UpdateSerialNumberIndices(container As StackPanel)
            ' Clear the serialNumberTextBoxes list
            ProductController.serialNumberTextBoxes.Clear()

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
                                ' Update the name
                                textBox.Name = $"TxtSerial_{i + 1}"
                                ' Add to our list
                                ProductController.serialNumberTextBoxes.Add(textBox)
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
            ' Update visibility based on checkbox state
            UpdateSerialNumberPanelVisibility()

            ' Update the current variation data
            Dim currentData = ProductController.variationManager.GetCurrentVariationData()
            If currentData IsNot Nothing Then
                currentData.IncludeSerialNumbers = CheckBoxSerialNumber.IsChecked

                ' If checked, create the serial number textboxes
                If CheckBoxSerialNumber.IsChecked Then
                    CreateSerialNumberTextBoxes()
                End If
            End If
        End Sub

        ''' <summary>
        ''' Updates the visibility of serial number panel based on checkbox state
        ''' </summary>
        Private Sub UpdateSerialNumberPanelVisibility()
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

            ' Set the data to controls
            If currentData IsNot Nothing Then
                ' Set checkbox state
                CheckBoxSerialNumber.IsChecked = currentData.IncludeSerialNumbers

                ' Update visibility based on checkbox state
                UpdateSerialNumberPanelVisibility()

                ' Create textboxes based on stock units
                If currentData.IncludeSerialNumbers Then
                    CreateSerialNumberTextBoxes()

                    ' Load serial numbers into textboxes
                    If currentData.SerialNumbers IsNot Nothing AndAlso currentData.SerialNumbers.Count > 0 Then
                        For i As Integer = 0 To Math.Min(currentData.SerialNumbers.Count - 1, ProductController.serialNumberTextBoxes.Count - 1)
                            ProductController.serialNumberTextBoxes(i).Text = currentData.SerialNumbers(i)
                        Next
                    End If
                End If
            End If
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

            ' Generate all possible combinations
            GenerateVariationCombinations()

            ' Create variation tabs
            CreateVariationTabs()

            ' Select the first variation by default
            If ProductController.variationManager.GetAllVariationData().Count > 0 Then
                Dim firstKey = ProductController.variationManager.GetAllVariationData().Keys.First()
                SelectVariation(firstKey)
            End If
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

            ' Create variation data entries for each combination
            For Each combo In combinations
                Dim combinationName = String.Join(", ", combo)

                ' Add the combination with default name
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

                    ' Initialize serial numbers list with one empty entry
                    If newVariation.IncludeSerialNumbers Then
                        newVariation.SerialNumbers = New List(Of String)()
                        newVariation.SerialNumbers.Add("")
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
                .FontSize = 14, ' Increased font size from default
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
            Next

            ' Set the ButtonPanel as the content of the ScrollViewer
            scrollViewer.Content = buttonPanel

            ' Add the ScrollViewer to the VariationsPanel
            VariationsPanel.Children.Add(scrollViewer)

            ' Ensure VariationsPanel has transparent background
            VariationsPanel.Background = Brushes.Transparent

            ' Select the first tab by default if there are any tabs
            If buttonPanel.Children.Count > 0 Then
                Dim firstButton = TryCast(buttonPanel.Children(0), Button)
                If firstButton IsNot Nothing Then
                    ' Simulate a click on the first button to select it
                    VariationTab_Click(firstButton, New RoutedEventArgs())
                End If
            End If
        End Sub

        ''' <summary>
        ''' Event handler for variation tab clicks
        ''' </summary>
        Private Sub VariationTab_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = TryCast(sender, Button)
            If btn IsNot Nothing Then
                Dim combinationName = TryCast(btn.Tag, String)
                If Not String.IsNullOrEmpty(combinationName) Then
                    ' Save current variation data before switching
                    SaveCurrentVariationData()

                    ' Now select the new variation
                    SelectVariation(combinationName)

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
                End If
            End If
        End Sub

        ''' <summary>
        ''' Selects a variation and displays its details
        ''' </summary>
        Private Sub SelectVariation(combinationName As String)
            ' Update the selected variation title
            SelectedVariationTitle.Text = "Selected Variation: " & combinationName

            ' Select this variation in the manager
            ProductController.variationManager.SelectVariationCombination(combinationName)

            ' Load the data into existing form fields
            LoadVariationFormData()

            ' Load serial numbers data
            LoadSerialNumberData()
        End Sub

        ''' <summary>
        ''' Loads the variation details into existing form fields
        ''' </summary>
        Private Sub LoadVariationFormData()
            ' Get the current variation data
            Dim currentData = ProductController.variationManager.GetCurrentVariationData()

            ' Set the data to the existing form fields
            If currentData IsNot Nothing Then
                TxtRetailPrice.Text = currentData.RetailPrice.ToString()
                TxtPurchaseOrder.Text = currentData.PurchaseOrder.ToString()
                TxtDefaultTax.Text = currentData.DefaultTax.ToString()
                TxtDiscountRate.Text = currentData.DiscountRate.ToString()
                TxtStockUnits.Text = currentData.StockUnits.ToString()
                TxtAlertQuantity.Text = currentData.AlertQuantity.ToString()

                ' Set warehouse selection if available
                If currentData.SelectedWarehouseIndex >= 0 AndAlso currentData.SelectedWarehouseIndex < ComboBoxWarehouse.Items.Count Then
                    ComboBoxWarehouse.SelectedIndex = currentData.SelectedWarehouseIndex
                End If
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
            ' First save the currently displayed variation
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

                ' Save warehouse selection
                currentData.SelectedWarehouseIndex = ComboBoxWarehouse.SelectedIndex
                If ComboBoxWarehouse.SelectedIndex >= 0 Then
                    ' Get the actual warehouse ID from the ComboBoxItem's Tag property
                    Dim selectedItem = TryCast(ComboBoxWarehouse.SelectedItem, ComboBoxItem)
                    If selectedItem IsNot Nothing Then
                        currentData.WarehouseId = Convert.ToInt32(selectedItem.Tag)
                    End If
                End If

                ' Save serial numbers data
                currentData.IncludeSerialNumbers = CheckBoxSerialNumber.IsChecked

                ' Get serial numbers from textboxes if enabled
                If CheckBoxSerialNumber.IsChecked Then
                    currentData.SerialNumbers = New List(Of String)()
                    For Each textBox In ProductController.serialNumberTextBoxes
                        currentData.SerialNumbers.Add(textBox.Text)
                    Next
                End If
            End If
        End Sub
#End Region

#Region "Navigation and Redirection"
        Private Sub BtnBatchEdit(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the BatchEdit form
            Dim OpenBatchEdit As New ProductBatchEdit()
            Me.Close()
            OpenBatchEdit.Show()
        End Sub

        Private Sub BtnBack(sender As Object, e As RoutedEventArgs)
            ' Navigate back to AddNewProducts
            Dim addNewProducts As New Views.Stocks.ItemManager.NewProduct.AddNewProducts()
            addNewProducts.Show()
            Me.Close()
        End Sub
#End Region

#Region "Input Validation and Events"
        ' Text input validation handlers for numeric fields
        Private Sub IntegerOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            ' Allow only digits and decimal point
            Dim regex As New Text.RegularExpressions.Regex("[^0-9]+")
            e.Handled = regex.IsMatch(e.Text)
        End Sub

        Private Sub IntegerOnlyPasteHandler(sender As Object, e As DataObjectPastingEventArgs)
            If e.DataObject.GetDataPresent(GetType(String)) Then
                Dim text As String = CType(e.DataObject.GetData(GetType(String)), String)
                Dim regex As New Text.RegularExpressions.Regex("[^0-9]+")
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
#End Region

    End Class
End Namespace