Imports System.Windows.Controls.Primitives
Imports MaterialDesignThemes.Wpf

Namespace DPC.Components.Forms
    Public Class AddVariation
        Inherits UserControl

        Private variationCount As Integer = 1
        Private Const MaxVariations As Integer = 2
        Private ChangeIcon As Boolean = False

        Public Sub New()
            InitializeComponent()

            ' Add the first variation
            AddNewVariation()
        End Sub

        Private Sub AddNewVariation()
            ' Create the main StackPanel for this variation
            Dim variationPanel As New StackPanel With {
        .Orientation = Orientation.Vertical,
        .Margin = New Thickness(0, 0, 0, 20)
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
        .Text = $"Variation {variationCount}",
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

                                                MainVariationContainer.Children.Remove(variationPanel)
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
                .IsChecked = True,
                .Style = CType(FindResource("ToggleSwitchStyle"), Style),
                .Tag = optionsContainer ' Now this reference works because optionsContainer is declared
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
                                           End Sub

            ' Add all elements to variation panel
            variationPanel.Children.Add(lblName)
            variationPanel.Children.Add(nameBorder)
            variationPanel.Children.Add(toggleGrid)
            variationPanel.Children.Add(divider)
            variationPanel.Children.Add(scrollOptions)
            variationPanel.Children.Add(btnAddOption)

            ' Add the new variation panel to the main container
            MainVariationContainer.Children.Add(variationPanel)

            ' Add the first option to this variation
            AddOptionToPanel(optionsContainer)

            ' Increment the variation counter for the next variation
            variationCount += 1
        End Sub

        Private Sub AddOptionToPanel(targetPanel As StackPanel)
            ' Create Grid for option row
            Dim optionGrid As New Grid With {.Margin = New Thickness(0, 0, 0, 10)}

            ' Define grid columns
            optionGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            optionGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            optionGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            optionGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            ' Find the parent container and check for the toggle state
            Dim parentVariationPanel As StackPanel = TryCast(VisualTreeHelper.GetParent(targetPanel), StackPanel)
            Dim isImagesVisible As Boolean = True ' Default to visible

            If parentVariationPanel IsNot Nothing Then
                For Each child As UIElement In parentVariationPanel.Children
                    If TypeOf child Is Grid Then
                        Dim grid As Grid = TryCast(child, Grid)
                        For Each gridChild As UIElement In grid.Children
                            If TypeOf gridChild Is ToggleButton Then
                                Dim toggle As ToggleButton = TryCast(gridChild, ToggleButton)
                                isImagesVisible = toggle.IsChecked
                                Exit For
                            End If
                        Next
                        If Not isImagesVisible Then Exit For
                    End If
                Next
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
            Grid.SetColumn(imageBorder, 0)
            optionGrid.Children.Add(imageBorder)

            ' Option text field
            Dim txtOption As New TextBox With {
            .Text = "Option",
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
            AddHandler btnEdit.Click, AddressOf BtnEditOption
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
        End Sub

        ' Add this method to handle the "Add Variation" button click
        Private Sub BtnAddVariation_Click(sender As Object, e As RoutedEventArgs)
            ' Check if the number of variations is less than the maximum allowed
            If variationCount <= MaxVariations Then
                AddNewVariation()
            Else
                MessageBox.Show("You can only add up to 2 variations.", "Limit Reached", MessageBoxButton.OK, MessageBoxImage.Information)
            End If
        End Sub
    End Class
End Namespace