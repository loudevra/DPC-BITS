Imports System.Windows.Controls.Primitives
Imports MaterialDesignThemes.Wpf
Imports Microsoft.Win32
Imports System.IO
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model

Namespace DPC.Components.Forms
    Public Class AddVariation
        Inherits UserControl

        Public Event close(sender As Object, e As RoutedEventArgs)
        Private variationCount As Integer = 1
        Private Const MaxVariations As Integer = 2
        Private ChangeIcon As Boolean = False


        ' Add data model properties for saving state
        Private _variations As New List(Of ProductVariation)

        ' Static property to store variations data globally
        Private Shared _savedVariations As List(Of ProductVariation) = New List(Of ProductVariation)

        Public Shared ReadOnly Property SavedVariations As List(Of ProductVariation)
            Get
                Return _savedVariations
            End Get
        End Property

        Public Sub New()
            InitializeComponent()

            ' Check if we have saved variations data
            If _savedVariations.Count > 0 Then
                ' Load saved variations
                LoadSavedVariations()
            Else
                ' Add the first variation
                AddNewVariation()
            End If
        End Sub

        ' Method to load saved variations
        Private Sub LoadSavedVariations()
            ' Clear the UI container first
            MainVariationContainer.Children.Clear()

            ' Reset variation counter
            variationCount = 1

            ' Loop through each saved variation and recreate the UI
            For Each variation As ProductVariation In _savedVariations
                ' Create the variation UI elements
                Dim variationPanel As StackPanel = CreateVariationPanel(variation)

                ' Add to the main container
                MainVariationContainer.Children.Add(variationPanel)

                ' Increment variation counter
                variationCount += 1
            Next

            ' Make sure variation count is set correctly
            RecalculateVariationCount()
        End Sub

        ' Helper method to create a variation panel from data
        ' Update the CreateVariationPanel method to handle image data
        Private Function CreateVariationPanel(variation As ProductVariation) As StackPanel
            ' Create the main StackPanel for this variation
            Dim variationPanel As New StackPanel With {
        .Orientation = Orientation.Vertical,
        .Margin = New Thickness(0, 0, 0, 20),
        .Tag = variationCount ' Store the variation number in the Tag property
    }

            Dim optionsContainer As New StackPanel With {
        .Name = $"OptionsContainer_{variationCount}"
    }

            ' === Variation Name Section ===
            Dim lblName As New TextBlock With {
        .Text = "Variation Name:",
        .Margin = New Thickness(0, 0, 0, 10),
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#555555")),
        .FontSize = 14,
        .FontWeight = FontWeights.SemiBold
    }

            Dim nameBorder As New Border With {
        .Style = CType(FindResource("RoundedBorderStyle"), Style),
        .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#555555")),
        .Margin = New Thickness(0, 0, 0, 15)
    }

            Dim nameGrid As New Grid()
            nameGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            nameGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            nameGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})

            Dim txtVariationName As New TextBox With {
        .Text = variation.VariationName,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#555555")),
        .FontSize = 13,
        .FontWeight = FontWeights.SemiBold,
        .Margin = New Thickness(20, 0, 5, 0),
        .Style = CType(FindResource("RoundedTextboxStyle"), Style),
        .IsReadOnly = True,
        .VerticalAlignment = VerticalAlignment.Center
    }

            Dim btnEditName As New Button With {
        .Background = Brushes.Transparent,
        .BorderThickness = New Thickness(0),
        .Margin = New Thickness(0),
        .Style = CType(FindResource("RoundedButtonStyle"), Style),
        .Tag = txtVariationName
    }

            Dim editIcon As New PackIcon With {
        .Kind = PackIconKind.PencilOffOutline,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")),
        .Width = 25,
        .Height = 25,
        .VerticalAlignment = VerticalAlignment.Center
    }
            btnEditName.Content = editIcon
            AddHandler btnEditName.Click, Sub(sender, e)
                                              Dim isEditing = EditFunction(txtVariationName, True)
                                              editIcon.Kind = If(isEditing, PackIconKind.PencilOffOutline, PackIconKind.PencilOutline)
                                          End Sub

            Dim btnDeleteName As New Button With {
        .Background = Brushes.Transparent,
        .BorderThickness = New Thickness(0),
        .Margin = New Thickness(0, 0, 15, 0),
        .Style = CType(FindResource("RoundedButtonStyle"), Style),
        .HorizontalAlignment = HorizontalAlignment.Right
    }

            Dim deleteIcon As New PackIcon With {
        .Kind = PackIconKind.TrashCanOutline,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636")),
        .Width = 25,
        .Height = 25
    }
            btnDeleteName.Content = deleteIcon

            ' Add handler to remove the variation panel when delete button is clicked
            AddHandler btnDeleteName.Click, Sub()
                                                ' Check if this is the only variation
                                                If MainVariationContainer.Children.Count <= 1 Then
                                                    MessageBox.Show("You cannot delete the last variation.", "Deletion Restricted", MessageBoxButton.OK, MessageBoxImage.Information)
                                                    Return
                                                End If

                                                ' Remove the panel from the container
                                                MainVariationContainer.Children.Remove(variationPanel)

                                                ' Update variationCount if needed
                                                RecalculateVariationCount()

                                                ' Save variations after deletion
                                                SaveVariations()
                                            End Sub

            Grid.SetColumn(txtVariationName, 0)
            Grid.SetColumn(btnEditName, 1)
            Grid.SetColumn(btnDeleteName, 2)

            nameGrid.Children.Add(txtVariationName)
            nameGrid.Children.Add(btnEditName)
            nameGrid.Children.Add(btnDeleteName)

            nameBorder.Child = nameGrid

            ' === Toggle Section ===
            Dim toggleGrid As New Grid()
            toggleGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(0.25, GridUnitType.Star)})
            toggleGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(0.5, GridUnitType.Star)})

            Dim lblToggle As New TextBlock With {
        .Text = "Variation Images",
        .FontSize = 14,
        .FontWeight = FontWeights.SemiBold,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#555555")),
        .VerticalAlignment = VerticalAlignment.Center
    }

            Dim toggle As New ToggleButton With {
        .IsChecked = variation.EnableImage,
        .Style = CType(FindResource("ToggleSwitchStyle"), Style),
        .Tag = optionsContainer
    }

            ' Add event handler for toggle state changes
            AddHandler toggle.Checked, AddressOf Toggle_CheckedChanged
            AddHandler toggle.Unchecked, AddressOf Toggle_CheckedChanged

            Grid.SetColumn(lblToggle, 0)
            Grid.SetColumn(toggle, 1)

            toggleGrid.Children.Add(lblToggle)
            toggleGrid.Children.Add(toggle)

            ' === Divider ===
            Dim divider As New Border With {
        .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")),
        .BorderThickness = New Thickness(0, 1, 0, 0),
        .Height = 1,
        .Margin = New Thickness(-5, 10, -5, 10)
    }

            ' === Options Container ===

            ' Scrollable options area
            Dim scrollOptions As New ScrollViewer With {
        .VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        .MaxHeight = 150,
        .Content = optionsContainer,
        .Style = CType(FindResource("ModernScrollViewerStyle"), Style)
    }

            ' Add Option Button
            Dim btnAddOption As New Button With {
        .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")),
        .Style = CType(FindResource("RoundedButtonStyle"), Style),
        .BorderThickness = New Thickness(0),
        .Margin = New Thickness(0, 10, 0, 0)
    }

            Dim stackBtnContent As New StackPanel With {
        .Orientation = Orientation.Horizontal,
        .HorizontalAlignment = HorizontalAlignment.Center
    }

            Dim plusIcon As New PackIcon With {
        .Kind = PackIconKind.PlusCircleOutline,
        .Foreground = Brushes.White,
        .Width = 20,
        .Height = 20,
        .Margin = New Thickness(0, 0, 5, 0)
    }

            Dim plusText As New TextBlock With {
        .Text = "Add Option",
        .Foreground = Brushes.White,
        .FontSize = 14,
        .FontWeight = FontWeights.SemiBold
    }

            stackBtnContent.Children.Add(plusIcon)
            stackBtnContent.Children.Add(plusText)
            btnAddOption.Content = stackBtnContent

            ' Add click handler for the Add Option button
            AddHandler btnAddOption.Click, Sub()
                                               AddOptionToPanel(optionsContainer)
                                               ' Save after adding a new option
                                               SaveVariations()
                                           End Sub

            ' Add all elements to variation panel
            variationPanel.Children.Add(lblName)
            variationPanel.Children.Add(nameBorder)
            variationPanel.Children.Add(toggleGrid)
            variationPanel.Children.Add(divider)
            variationPanel.Children.Add(scrollOptions)
            variationPanel.Children.Add(btnAddOption)

            ' Add options from the variation data
            If variation.Options IsNot Nothing Then
                For Each opt As VariationOption In variation.Options
                    AddOptionToPanel(optionsContainer, opt)
                Next
            End If

            Return variationPanel
        End Function




        Private Sub BtnVariationDetails(sender As Object, e As RoutedEventArgs)
            ' Save variations before navigating
            SaveVariations()

            ' Notify the parent window to update its variation text
            Dim parentWindow = Window.GetWindow(Me)
            If parentWindow IsNot Nothing AndAlso TypeOf parentWindow Is DPC.Views.Stocks.ItemManager.NewProduct.AddNewProducts Then
                Dim addNewProductsWindow = DirectCast(parentWindow, DPC.Views.Stocks.ItemManager.NewProduct.AddNewProducts)
                addNewProductsWindow.LoadProductVariations()
            End If

            Dim VariationDetails As New Views.Stocks.ItemManager.NewProduct.ProductVariationDetails()
            VariationDetails.Show()

            Dim currentWindow As Window = Window.GetWindow(Me)
            currentWindow?.Close()
        End Sub

        ' Also update the ClosePopup method to ensure variations are saved and the display is updated
        Public Sub ClosePopup(sender As Object, e As RoutedEventArgs)
            ' Save variations before closing
            SaveVariations()

            ' Notify the parent window to update its variation text
            Dim parentWindow = Window.GetWindow(Me)
            If parentWindow IsNot Nothing AndAlso TypeOf parentWindow Is DPC.Views.Stocks.ItemManager.NewProduct.AddNewProducts Then
                Dim addNewProductsWindow = DirectCast(parentWindow, DPC.Views.Stocks.ItemManager.NewProduct.AddNewProducts)
                addNewProductsWindow.LoadProductVariations()
            End If

            ' Raise the close event
            RaiseEvent close(Me, e)
            PopupHelper.ClosePopup()
        End Sub

        ' Method to save all variations data
        Private Sub SaveVariations()
            ' Clear the current saved variations
            _savedVariations.Clear()

            ' Loop through each variation panel in the UI
            For Each child As UIElement In MainVariationContainer.Children
                If TypeOf child Is StackPanel Then
                    Dim variationPanel As StackPanel = DirectCast(child, StackPanel)

                    ' Create new variation object
                    Dim variation As New ProductVariation With {
                .Options = New List(Of VariationOption)()
            }

                    ' Extract variation name
                    Dim nameBorder As Border = TryCast(variationPanel.Children(1), Border)
                    If nameBorder IsNot Nothing Then
                        Dim nameGrid As Grid = TryCast(nameBorder.Child, Grid)
                        If nameGrid IsNot Nothing Then
                            For Each gridChild As UIElement In nameGrid.Children
                                If TypeOf gridChild Is TextBox Then
                                    variation.VariationName = DirectCast(gridChild, TextBox).Text
                                    Exit For
                                End If
                            Next
                        End If
                    End If

                    ' Extract enabled image state
                    Dim toggleGrid As Grid = TryCast(variationPanel.Children(2), Grid)
                    If toggleGrid IsNot Nothing Then
                        For Each gridChild As UIElement In toggleGrid.Children
                            If TypeOf gridChild Is ToggleButton Then
                                variation.EnableImage = DirectCast(gridChild, ToggleButton).IsChecked
                                Exit For
                            End If
                        Next
                    End If

                    ' Extract options
                    Dim scrollViewer As ScrollViewer = TryCast(variationPanel.Children(4), ScrollViewer)
                    If scrollViewer IsNot Nothing Then
                        Dim optionsContainer As StackPanel = TryCast(scrollViewer.Content, StackPanel)
                        If optionsContainer IsNot Nothing Then
                            For Each optionChild As UIElement In optionsContainer.Children
                                If TypeOf optionChild Is Grid Then
                                    Dim optionGrid As Grid = DirectCast(optionChild, Grid)
                                    Dim optionName As String = String.Empty
                                    Dim imageData As ImageData = TryCast(optionGrid.Tag, ImageData)

                                    ' Find the text box for option name
                                    For Each gridChild As UIElement In optionGrid.Children
                                        If TypeOf gridChild Is TextBox Then
                                            Dim optionTextBox As TextBox = DirectCast(gridChild, TextBox)
                                            optionName = optionTextBox.Text
                                            Exit For
                                        End If
                                    Next

                                    ' Create new option with image data
                                    Dim opt As New VariationOption With {
                                .OptionName = optionName
                            }

                                    ' Add image data if available
                                    If imageData IsNot Nothing Then
                                        opt.ImageBase64 = imageData.Base64String
                                        opt.ImageFileName = imageData.FileName
                                        opt.ImageFileExtension = imageData.FileExtension
                                    End If

                                    ' Add to options list
                                    variation.Options.Add(opt)
                                End If
                            Next
                        End If
                    End If

                    ' Add the variation to the saved list
                    _savedVariations.Add(variation)
                End If
            Next
        End Sub

        Private Sub AddNewVariation()
            ' Create a new ProductVariation object
            Dim newVariation As New ProductVariation With {
                .VariationName = $"Variation {variationCount}",
                .EnableImage = True,
                .Options = New List(Of VariationOption)()
            }

            ' Add a default option
            newVariation.Options.Add(New VariationOption With {.OptionName = "Option"})

            ' Create the UI panel for this variation
            Dim variationPanel = CreateVariationPanel(newVariation)

            ' Add the new variation panel to the main container
            MainVariationContainer.Children.Add(variationPanel)

            ' Increment the variation counter for the next variation
            variationCount += 1

            ' Save variations after adding a new one
            SaveVariations()
        End Sub

        ' Add this method to recalculate variationCount after deletion
        Private Sub RecalculateVariationCount()
            ' Find the highest variation number used
            Dim highestVariationNumber As Integer = 0

            For Each child As UIElement In MainVariationContainer.Children
                If TypeOf child Is StackPanel Then
                    Dim panel As StackPanel = DirectCast(child, StackPanel)
                    If panel.Tag IsNot Nothing AndAlso TypeOf panel.Tag Is Integer Then
                        Dim panelNumber As Integer = DirectCast(panel.Tag, Integer)
                        If panelNumber > highestVariationNumber Then
                            highestVariationNumber = panelNumber
                        End If
                    End If
                End If
            Next

            ' Set variationCount to be one more than the highest current number
            variationCount = highestVariationNumber + 1

            ' Make sure variationCount doesn't exceed MaxVariations
            If variationCount > MaxVariations Then
                variationCount = MaxVariations
            End If
        End Sub

        ' Update the AddOptionToPanel method to handle image data
        Private Sub AddOptionToPanel(targetPanel As StackPanel, Optional optionData As VariationOption = Nothing)
            Dim optionName As String = "Option"
            Dim imageData As New ImageData()

            ' Use provided option data if available
            If optionData IsNot Nothing Then
                optionName = optionData.OptionName

                ' Set image data if available
                If Not String.IsNullOrEmpty(optionData.ImageBase64) Then
                    imageData.Base64String = optionData.ImageBase64
                    imageData.FileName = optionData.ImageFileName
                    imageData.FileExtension = optionData.ImageFileExtension
                End If
            End If

            ' Create Grid for option row
            Dim optionGrid As New Grid With {.Margin = New Thickness(0, 0, 0, 10)}

            ' Store image data
            optionGrid.Tag = imageData

            ' Define grid columns
            optionGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            optionGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            optionGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            optionGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            ' Find the parent container and check for the toggle state
            Dim isImagesVisible As Boolean = True ' Default to visible

            ' Check if the targetPanel is within a ScrollViewer
            Dim parentElement As DependencyObject = VisualTreeHelper.GetParent(targetPanel)
            If TypeOf parentElement Is ScrollViewer Then
                ' The parent of the ScrollViewer should be the variation panel
                Dim variationPanel As StackPanel = TryCast(VisualTreeHelper.GetParent(parentElement), StackPanel)

                If variationPanel IsNot Nothing Then
                    ' Look for toggle grid among the children
                    For i As Integer = 0 To variationPanel.Children.Count - 1
                        If TypeOf variationPanel.Children(i) Is Grid Then
                            Dim grid As Grid = TryCast(variationPanel.Children(i), Grid)

                            ' Check if this grid contains a toggle button
                            For Each gridChild As UIElement In grid.Children
                                If TypeOf gridChild Is ToggleButton Then
                                    Dim toggle As ToggleButton = TryCast(gridChild, ToggleButton)
                                    isImagesVisible = toggle.IsChecked
                                    Exit For
                                End If
                            Next
                        End If
                    Next
                End If
            End If

            ' Image placeholder with visibility set based on toggle state
            Dim imageBorder As New Border With {
        .Width = 40,
        .Height = 40,
        .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")),
        .BorderThickness = New Thickness(1),
        .CornerRadius = New CornerRadius(5),
        .VerticalAlignment = VerticalAlignment.Center,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .Margin = New Thickness(0, 0, 10, 0),
        .Visibility = If(isImagesVisible, Visibility.Visible, Visibility.Collapsed),
        .Cursor = Cursors.Hand  ' Change cursor to indicate it's clickable
    }

            ' If we have image data, display it
            If Not String.IsNullOrEmpty(imageData.Base64String) Then
                Try
                    ' Create image from Base64 data
                    Dim img As New System.Windows.Controls.Image With {
                .Stretch = Stretch.Uniform
            }

                    ' Convert Base64 string to BitmapImage
                    Dim bitmap As BitmapImage = Base64StringToBitmapImage(imageData.Base64String)
                    img.Source = bitmap

                    ' Set the image as the border's content
                    imageBorder.Child = img
                Catch ex As Exception
                    ' If there's an error loading the image, fall back to the default icon
                    SetDefaultImageIcon(imageBorder)
                End Try
            Else
                ' No image data, use default icon
                SetDefaultImageIcon(imageBorder)
            End If

            ' Add click handler for image selection
            AddHandler imageBorder.MouseDown, Sub(sender, e)
                                                  SelectImage(optionGrid, imageBorder)
                                                  ' Save after selecting an image
                                                  SaveVariations()
                                              End Sub

            Grid.SetColumn(imageBorder, 0)
            optionGrid.Children.Add(imageBorder)

            ' Option text field
            Dim txtOption As New TextBox With {
        .Text = optionName,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#555555")),
        .FontSize = 14,
        .FontWeight = FontWeights.SemiBold,
        .VerticalAlignment = VerticalAlignment.Center,
        .Margin = New Thickness(0),
        .IsReadOnly = True,
        .Style = CType(FindResource("RoundedTextboxStyle"), Style)
    }
            Grid.SetColumn(txtOption, 1)
            optionGrid.Children.Add(txtOption)

            ' Edit button
            Dim btnEdit As New Button With {
        .Margin = New Thickness(0, 0, 5, 0),
        .BorderThickness = New Thickness(0),
        .Style = CType(FindResource("RoundedButtonStyle"), Style)
    }

            Dim editIcon As New PackIcon With {
        .Kind = PackIconKind.PencilOffOutline,
        .Width = 25,
        .Height = 25,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE"))
    }
            btnEdit.Content = editIcon
            AddHandler btnEdit.Click, Sub(sender, e)
                                          BtnEditOption(sender, e)
                                          ' Save after editing an option
                                          SaveVariations()
                                      End Sub
            Grid.SetColumn(btnEdit, 2)
            optionGrid.Children.Add(btnEdit)

            ' Delete button
            Dim btnDelete As New Button With {
        .Background = Brushes.Transparent,
        .BorderThickness = New Thickness(0),
        .Padding = New Thickness(5)
    }

            Dim deleteIcon As New PackIcon With {
        .Kind = PackIconKind.TrashCanOutline,
        .Width = 25,
        .Height = 25,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636"))
    }
            btnDelete.Content = deleteIcon
            AddHandler btnDelete.Click, AddressOf DeleteOptionRow
            Grid.SetColumn(btnDelete, 3)
            optionGrid.Children.Add(btnDelete)

            ' Add the option grid to the target panel
            targetPanel.Children.Add(optionGrid)
        End Sub

        ' Helper method to set default image icon
        Private Sub SetDefaultImageIcon(imageBorder As Border)
            Dim imageStack As New StackPanel With {
        .Orientation = Orientation.Vertical,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .VerticalAlignment = VerticalAlignment.Center
    }

            Dim packIcon As New PackIcon With {
        .Kind = PackIconKind.ImagePlus,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#555555")),
        .Width = 20,
        .Height = 20,
        .VerticalAlignment = VerticalAlignment.Center
    }

            imageStack.Children.Add(packIcon)
            imageBorder.Child = imageStack
        End Sub


        ' New method to handle image selection
        Private Sub SelectImage(optionGrid As Grid, imageBorder As Border)
            ' Create OpenFileDialog to select an image
            Dim openFileDialog As New OpenFileDialog With {
                .Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                .Title = "Select an Image"
            }

            ' Show the dialog and get the result
            If openFileDialog.ShowDialog() = True Then
                Try
                    ' Get selected file path
                    Dim filePath As String = openFileDialog.FileName

                    ' Convert image to Base64 string
                    Dim base64String As String = Base64Utility.EncodeFileToBase64(filePath)
                    Dim fileExtension As String = Base64Utility.GetFileExtension(filePath)

                    ' Store image data in the grid's Tag property
                    Dim imgData As ImageData = DirectCast(optionGrid.Tag, ImageData)
                    imgData.Base64String = base64String
                    imgData.FileExtension = fileExtension
                    imgData.FileName = Path.GetFileName(filePath)
                    optionGrid.Tag = imgData

                    ' Update the image display
                    UpdateImageDisplay(imageBorder, filePath)

                    ' Save after image selection
                    SaveVariations()

                Catch ex As Exception
                    MessageBox.Show("Error loading image: " & ex.Message, "Image Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub

        ' New method to update the image display
        Private Sub UpdateImageDisplay(imageBorder As Border, imagePath As String)
            ' Create a new Image control
            Dim img As New System.Windows.Controls.Image With {
                .Stretch = Stretch.Uniform
            }

            ' Create BitmapImage from file
            Dim bitmap As New BitmapImage()
            bitmap.BeginInit()
            bitmap.UriSource = New Uri(imagePath)
            bitmap.CacheOption = BitmapCacheOption.OnLoad ' Load image right away and close the file
            bitmap.EndInit()

            ' Set the image source
            img.Source = bitmap

            ' Replace the content of the border with the image
            imageBorder.Child = img
        End Sub

        Private Sub BtnEditOption(sender As Object, e As RoutedEventArgs)
            Dim btn As Button = TryCast(sender, Button)
            If btn Is Nothing Then Exit Sub

            ' The icon is the direct content of the button
            Dim icon As PackIcon = TryCast(btn.Content, PackIcon)
            If icon Is Nothing Then Exit Sub

            ' Get the parent Grid of the button
            Dim parentGrid As Grid = TryCast(VisualTreeHelper.GetParent(btn), Grid)
            If parentGrid Is Nothing Then Exit Sub

            ' Find the TextBox in the grid
            Dim txtOption As TextBox = Nothing
            For Each child As UIElement In parentGrid.Children
                If TypeOf child Is TextBox Then
                    txtOption = CType(child, TextBox)
                    Exit For
                End If
            Next

            If txtOption IsNot Nothing Then
                Dim isReadOnlyNow As Boolean = EditFunction(txtOption, True)
                icon.Kind = If(isReadOnlyNow, PackIconKind.PencilOffOutline, PackIconKind.PencilOutline)

                ' Save after editing option name
                If isReadOnlyNow Then
                    SaveVariations()
                End If
            End If
        End Sub

        Private Sub DeleteOptionRow(sender As Object, e As RoutedEventArgs)
            ' Get the button that was clicked
            Dim btn As Button = CType(sender, Button)

            ' Get the Grid that is the row (the button's parent)
            Dim rowGrid As Grid = TryCast(VisualTreeHelper.GetParent(btn), Grid)

            ' Find the parent StackPanel
            If rowGrid IsNot Nothing Then
                Dim parentPanel As StackPanel = TryCast(VisualTreeHelper.GetParent(rowGrid), StackPanel)
                If parentPanel IsNot Nothing Then
                    ' Check if this is the last option in this variation
                    If parentPanel.Children.Count <= 1 Then
                        MessageBox.Show("You cannot delete the last option.", "Deletion Restricted", MessageBoxButton.OK, MessageBoxImage.Information)
                        Return
                    End If

                    parentPanel.Children.Remove(rowGrid)

                    ' Save after deleting option
                    SaveVariations()
                End If
            End If
        End Sub

        Private Function EditFunction(TxtBoxName As TextBox, shouldToggle As Boolean) As Boolean
            If shouldToggle Then
                TxtBoxName.IsReadOnly = Not TxtBoxName.IsReadOnly
            End If

            If TxtBoxName.IsReadOnly Then
                ChangeIcon = True
                Return ChangeIcon
            Else
                ChangeIcon = False
                TxtBoxName.Focus()
                TxtBoxName.SelectAll()
                Return ChangeIcon
            End If
        End Function

        Private Sub Toggle_CheckedChanged(sender As Object, e As RoutedEventArgs)
            Dim toggle As ToggleButton = TryCast(sender, ToggleButton)
            If toggle Is Nothing Then Exit Sub

            ' Get the options container from the toggle's Tag
            Dim optionsContainer As StackPanel = TryCast(toggle.Tag, StackPanel)
            If optionsContainer Is Nothing Then Exit Sub

            ' Get all option rows in the container
            For Each child As UIElement In optionsContainer.Children
                Dim grid As Grid = TryCast(child, Grid)
                If grid IsNot Nothing Then
                    ' Find the image border in each row (it should be in column 0)
                    For Each gridChild As UIElement In grid.Children
                        If TypeOf gridChild Is Border AndAlso Grid.GetColumn(gridChild) = 0 Then
                            ' Set visibility based on toggle state
                            gridChild.Visibility = If(toggle.IsChecked, Visibility.Visible, Visibility.Collapsed)
                            Exit For
                        End If
                    Next
                End If
            Next

            ' Save after toggling image visibility
            SaveVariations()
        End Sub

        ' Add this method to handle the "Add Variation" button click
        Private Sub BtnAddVariation_Click(sender As Object, e As RoutedEventArgs)
            ' Check if the current number of variations is less than the maximum allowed
            If MainVariationContainer.Children.Count < MaxVariations Then
                AddNewVariation()
            Else
                MessageBox.Show("You can only add up to 2 variations.", "Limit Reached", MessageBoxButton.OK, MessageBoxImage.Information)
            End If
        End Sub

        ' Helper class to store image data
        Private Class ImageData
            Public Property Base64String As String
            Public Property FileExtension As String
            Public Property FileName As String
        End Class

        Private Function Base64StringToBitmapImage(base64String As String) As BitmapImage
            Dim bytes As Byte() = Convert.FromBase64String(base64String)
            Dim bitmap As New BitmapImage()

            Using stream As New MemoryStream(bytes)
                bitmap.BeginInit()
                bitmap.CacheOption = BitmapCacheOption.OnLoad
                bitmap.StreamSource = stream
                bitmap.EndInit()
                bitmap.Freeze()
            End Using

            Return bitmap
        End Function
    End Class
End Namespace