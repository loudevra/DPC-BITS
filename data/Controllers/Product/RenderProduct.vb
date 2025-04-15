Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports System.IO
Imports System.Text
Imports DocumentFormat.OpenXml.Bibliography
Imports MaterialDesignThemes.Wpf
Imports Microsoft.Win32
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Controllers

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

        Public Shared ReadOnly Property variationCount As Integer
            Get
                Return variationCount
            End Get
        End Property

        Public Shared ReadOnly Property ChangeIcon As Boolean
            Get
                Return ChangeIcon
            End Get
        End Property

        'Variation dynamic components
        Public Shared Function CreateVariationPanel(variation As ProductVariation, MainVariationContainer As StackPanel) As StackPanel
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
        .Style = CType(Application.Current.FindResource("RoundedBorderStyle"), Style),
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
        .Style = CType(Application.Current.FindResource("RoundedTextboxStyle"), Style),
        .IsReadOnly = True,
        .VerticalAlignment = VerticalAlignment.Center
    }

            Dim btnEditName As New Button With {
        .Background = Brushes.Transparent,
        .BorderThickness = New Thickness(0),
        .Margin = New Thickness(0),
        .Style = CType(Application.Current.FindResource("RoundedButtonStyle"), Style),
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
                                              Dim isEditing = ProductController.EditFunction(txtVariationName, True, ChangeIcon)
                                              editIcon.Kind = If(isEditing, PackIconKind.PencilOffOutline, PackIconKind.PencilOutline)
                                          End Sub

            Dim btnDeleteName As New Button With {
        .Background = Brushes.Transparent,
        .BorderThickness = New Thickness(0),
        .Margin = New Thickness(0, 0, 15, 0),
        .Style = CType(Application.Current.FindResource("RoundedButtonStyle"), Style),
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
                                                ProductController.RecalculateVariationCount(MainVariationContainer)

                                                ' Save variations after deletion
                                                ProductController.SaveVariations(MainVariationContainer)
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
        .Style = CType(Application.Current.FindResource("ToggleSwitchStyle"), Style),
        .Tag = optionsContainer
    }

            ' Add event handler for toggle state changes
            AddHandler toggle.Checked, Sub(sender, e)
                                           ProductController.Toggle_CheckedChanged(sender, e, MainVariationContainer)
                                       End Sub
            AddHandler toggle.Unchecked, Sub(sender, e)
                                             ProductController.Toggle_CheckedChanged(sender, e, MainVariationContainer)
                                         End Sub

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
        .Style = CType(Application.Current.FindResource("ModernScrollViewerStyle"), Style)
    }

            ' Add Option Button
            Dim btnAddOption As New Button With {
        .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")),
        .Style = CType(Application.Current.FindResource("RoundedButtonStyle"), Style),
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
                                               ProductController.AddOptionToPanel(optionsContainer, MainVariationContainer)
                                               ' Save after adding a new option
                                               ProductController.SaveVariations(MainVariationContainer)
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
                    ProductController.AddOptionToPanel(optionsContainer, MainVariationContainer, opt)
                Next
            End If

            Return variationPanel
        End Function

        'method to set default image icon
        Public Shared Sub SetDefaultImageIcon(imageBorder As Border)
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

        'method to handle image selection
        Public Shared Sub SelectImage(optionGrid As Grid, imageBorder As Border, MainVariationContainer As StackPanel)
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
                    ProductController.UpdateImageDisplay(imageBorder, filePath)

                    ' Save after image selection
                    ProductController.SaveVariations(MainVariationContainer)

                Catch ex As Exception
                    MessageBox.Show("Error loading image: " & ex.Message, "Image Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub

        '  method to update the image display
        Public Shared Sub UpdateImageDisplay(imageBorder As Border, imagePath As String)
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

        Public Shared Sub AddOptionToPanel(targetPanel As StackPanel, MainVariationContainer As StackPanel, Optional optionData As VariationOption = Nothing)
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
                    Dim bitmap As BitmapImage = Base64Utility.Base64StringToBitmapImage(imageData.Base64String)
                    img.Source = bitmap

                    ' Set the image as the border's content
                    imageBorder.Child = img
                Catch ex As Exception
                    ' If there's an error loading the image, fall back to the default icon
                    ProductController.SetDefaultImageIcon(imageBorder)
                End Try
            Else
                ' No image data, use default icon
                ProductController.SetDefaultImageIcon(imageBorder)
            End If

            ' Add click handler for image selection
            AddHandler imageBorder.MouseDown, Sub(sender, e)
                                                  ProductController.SelectImage(optionGrid, imageBorder, MainVariationContainer)
                                                  ' Save after selecting an image
                                                  ProductController.SaveVariations(MainVariationContainer)
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
        .Style = CType(Application.Current.FindResource("RoundedTextboxStyle"), Style)
    }
            Grid.SetColumn(txtOption, 1)
            optionGrid.Children.Add(txtOption)

            ' Edit button
            Dim btnEdit As New Button With {
        .Margin = New Thickness(0, 0, 5, 0),
        .BorderThickness = New Thickness(0),
        .Style = CType(Application.Current.FindResource("RoundedButtonStyle"), Style)
    }

            Dim editIcon As New PackIcon With {
        .Kind = PackIconKind.PencilOffOutline,
        .Width = 25,
        .Height = 25,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE"))
    }
            btnEdit.Content = editIcon
            AddHandler btnEdit.Click, Sub(sender, e)
                                          ProductController.BtnEditOption(sender, e, MainContainer)
                                          ' Save after editing an option
                                          ProductController.SaveVariations(MainVariationContainer)
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
            AddHandler btnDelete.Click, Sub(sender, e)
                                            ProductController.DeleteOptionRow(sender, e, MainVariationContainer)
                                        End Sub

            Grid.SetColumn(btnDelete, 3)
            optionGrid.Children.Add(btnDelete)

            ' Add the option grid to the target panel
            targetPanel.Children.Add(optionGrid)
        End Sub

        'Delete Optionn row
        Public Shared Sub DeleteOptionRow(sender As Object, e As RoutedEventArgs, MainVariationContainer As StackPanel)
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
                    ProductController.SaveVariations(MainVariationContainer)
                End If
            End If
        End Sub

        'Add Edit dynamic btn component 
        Public Shared Sub BtnEditOption(sender As Object, e As RoutedEventArgs, MainVariationContainer As StackPanel)
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
                Dim isReadOnlyNow As Boolean = ProductController.EditFunction(txtOption, True, ChangeIcon)
                icon.Kind = If(isReadOnlyNow, PackIconKind.PencilOffOutline, PackIconKind.PencilOutline)

                ' Save after editing option name
                If isReadOnlyNow Then
                    ProductController.SaveVariations(MainVariationContainer)
                End If
            End If
        End Sub

        'Edit textbox of the variation popups
        Public Shared Function EditFunction(TxtBoxName As TextBox, shouldToggle As Boolean, ChangeIcon As Boolean) As Boolean
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

        'Add new variation panel
        Public Shared Sub AddNewVariation(MainVariationContainer As StackPanel)
            ' Create a new ProductVariation object
            Dim newVariation As New ProductVariation With {
                .VariationName = $"Variation {variationCount}",
                .EnableImage = True,
                .Options = New List(Of VariationOption)()
            }

            ' Add a default option
            newVariation.Options.Add(New VariationOption With {.OptionName = "Option"})

            ' Create the UI panel for this variation
            Dim variationPanel = ProductController.CreateVariationPanel(newVariation, MainVariationContainer)

            ' Add the new variation panel to the main container
            MainVariationContainer.Children.Add(variationPanel)

            ' Increment the variation counter for the next variation
            ProductController.variationCount += 1

            ' Save variations after adding a new one
            ProductController.SaveVariations(MainVariationContainer)
        End Sub
    End Class
End Namespace
