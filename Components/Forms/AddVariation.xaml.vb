Imports System.Windows.Controls.Primitives
Imports MaterialDesignThemes.Wpf

Namespace DPC.Components.Forms
    Public Class AddVariation
        Inherits UserControl

        Private variationCount As Integer = 1

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub BtnEditVariationName(sender As Object, e As RoutedEventArgs)
            EditVariationFunction(TxtVariationName, True)
        End Sub

        Private Sub BtnClearVariationName(sender As Object, e As RoutedEventArgs)
            TxtVariationName.Focus()
            TxtVariationName.Clear()

            ' Do not toggle the read-only state when clearing
            EditVariationFunction(TxtVariationName, False)
        End Sub

        Private Sub EditVariationFunction(VariationName As TextBox, shouldToggle As Boolean)
            If shouldToggle Then
                VariationName.IsReadOnly = Not VariationName.IsReadOnly
            End If

            If VariationName.IsReadOnly Then
                BtnIconVariationName.Kind = MaterialDesignThemes.Wpf.PackIconKind.PencilOffOutline
            Else
                BtnIconVariationName.Kind = MaterialDesignThemes.Wpf.PackIconKind.RenameOutline
                VariationName.Focus()
                VariationName.SelectAll()
            End If
        End Sub

        Private Sub AddBtnVariationsPanel(sender As Object, e As RoutedEventArgs)
            variationCount += 1

            Dim variationGrid As New Grid With {
                .Margin = New Thickness(0, 10, 0, 0)
            }
            variationGrid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})
            variationGrid.RowDefinitions.Add(New RowDefinition With {.Height = GridLength.Auto})

            variationGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            variationGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            Dim variationStack As New StackPanel With {
                .HorizontalAlignment = HorizontalAlignment.Stretch
            }

            Dim lbl As New TextBlock With {
                .Text = "Variation Name:",
                .Margin = New Thickness(0, 0, 0, 10),
                .Foreground = CType((New BrushConverter()).ConvertFrom("#555555"), Brush),
                .FontSize = 14,
                .FontWeight = FontWeights.SemiBold
            }



            Dim nameBorder As New Border With {
                .Style = CType(FindResource("RoundedBorderStyle"), Style),
                .BorderBrush = CType((New BrushConverter()).ConvertFrom("#555555"), Brush),
                .Margin = New Thickness(0, 0, 0, 15)
            }

            Dim nameGrid As New Grid()
            nameGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            nameGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            nameGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})

            Dim variationName As New TextBox With {
                .Text = $"Variation {variationCount}",
                .Foreground = CType((New BrushConverter()).ConvertFrom("#555555"), Brush),
                .FontSize = 13,
                .FontWeight = FontWeights.SemiBold,
                .Margin = New Thickness(20, 0, 5, 0),
                .Style = CType(FindResource("RoundedTextboxStyle"), Style),
                .IsReadOnly = True
            }
            Grid.SetColumn(variationName, 0)

            Dim editBtn As New Button With {
                .Margin = New Thickness(5, 0, 10, 0),
                .Background = Brushes.Transparent,
                .BorderThickness = New Thickness(0),
                .Padding = New Thickness(5)
            }
            Grid.SetColumn(editBtn, 1)

            Dim editIcon As New MaterialDesignThemes.Wpf.PackIcon With {
                .Kind = MaterialDesignThemes.Wpf.PackIconKind.PencilOffOutline,
                .Foreground = CType((New BrushConverter()).ConvertFrom("#AEAEAE"), Brush),
                .Width = 25,
                .Height = 25,
                .VerticalAlignment = VerticalAlignment.Center,
                .HorizontalAlignment = HorizontalAlignment.Right
            }
            editBtn.Content = editIcon

            Dim clearBtn As New Button With {
                .Margin = New Thickness(0, 0, 15, 0),
                .VerticalAlignment = VerticalAlignment.Center,
                .HorizontalAlignment = HorizontalAlignment.Right,
                .Background = Brushes.Transparent,
                .BorderThickness = New Thickness(0),
                .Padding = New Thickness(5)
            }
            Grid.SetColumn(clearBtn, 2)

            Dim clearIcon As New MaterialDesignThemes.Wpf.PackIcon With {
                .Kind = MaterialDesignThemes.Wpf.PackIconKind.TrashCanOutline,
                .Foreground = CType((New BrushConverter()).ConvertFrom("#D23636"), Brush),
                .Width = 25,
                .Height = 25
            }
            clearBtn.Content = clearIcon

            nameGrid.Children.Add(variationName)
            nameGrid.Children.Add(editBtn)
            nameGrid.Children.Add(clearBtn)
            nameBorder.Child = nameGrid

            Dim toggleGrid As New Grid()
            toggleGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(0.25, GridUnitType.Star)})
            toggleGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(0.5, GridUnitType.Star)})

            Dim toggleLabel As New TextBlock With {
                .Text = "Variation Images",
                .Foreground = CType((New BrushConverter()).ConvertFrom("#555555"), Brush),
                .FontSize = 14,
                .FontWeight = FontWeights.SemiBold,
                .VerticalAlignment = VerticalAlignment.Center
            }
            Grid.SetColumn(toggleLabel, 0)

            Dim toggleSwitch As New ToggleButton With {
                .Style = CType(FindResource("ToggleSwitchStyle"), Style),
                .IsChecked = True
            }
            Grid.SetColumn(toggleSwitch, 1)

            toggleGrid.Children.Add(toggleLabel)
            toggleGrid.Children.Add(toggleSwitch)

            variationStack.Children.Add(lbl)
            variationStack.Children.Add(nameBorder)
            variationStack.Children.Add(toggleGrid)

            Grid.SetRow(variationStack, 0)
            Grid.SetColumn(variationStack, 0)
            Grid.SetColumnSpan(variationStack, 2)

            variationGrid.Children.Add(variationStack)

            VariationListPanel.Children.Add(variationGrid)
        End Sub

        Private Sub AddBtnVariationsOptionPanel(sender As Object, e As RoutedEventArgs)
            ' Create a new Grid
            Dim optionGrid As New Grid With {
        .Margin = New Thickness(0, 5, 0, 5)
    }

            ' Define 4 columns
            optionGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            optionGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            optionGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            optionGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            ' Image placeholder (Border + Icon)
            Dim imageBorder As New Border With {
        .Width = 40,
        .Height = 40,
        .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")),
        .BorderThickness = New Thickness(1),
        .CornerRadius = New CornerRadius(5),
        .VerticalAlignment = VerticalAlignment.Center,
        .HorizontalAlignment = HorizontalAlignment.Center
    }

            Dim imageIcon As New PackIcon With {
        .Kind = PackIconKind.ImagePlus,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#555555")),
        .Width = 20,
        .Height = 20,
        .VerticalAlignment = VerticalAlignment.Center
    }

            Dim imageStack As New StackPanel With {
        .Orientation = Orientation.Vertical,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .VerticalAlignment = VerticalAlignment.Center
    }

            imageStack.Children.Add(imageIcon)
            imageBorder.Child = imageStack
            Grid.SetColumn(imageBorder, 0)

            ' Variation Option Name
            Dim optionText As New TextBlock With {
        .Text = "New Option",
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#555555")),
        .FontSize = 14,
        .FontWeight = FontWeights.SemiBold,
        .VerticalAlignment = VerticalAlignment.Center,
        .Margin = New Thickness(10, 0, 0, 0)
    }
            Grid.SetColumn(optionText, 1)

            ' Edit Button
            Dim editBtnIcon As New PackIcon With {
        .Kind = PackIconKind.PencilOutline,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")),
        .Width = 20,
        .Height = 20
    }

            Dim editBtn As New Button With {
        .Background = Brushes.Transparent,
        .BorderThickness = New Thickness(0),
        .Padding = New Thickness(5),
        .Margin = New Thickness(5, 0, 0, 0),
        .Content = editBtnIcon
    }
            Grid.SetColumn(editBtn, 2)

            ' Delete Button
            Dim deleteBtnIcon As New PackIcon With {
        .Kind = PackIconKind.TrashCanOutline,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636")),
        .Width = 20,
        .Height = 20
    }

            Dim deleteBtn As New Button With {
        .Background = Brushes.Transparent,
        .BorderThickness = New Thickness(0),
        .Padding = New Thickness(5),
        .Content = deleteBtnIcon
    }
            Grid.SetColumn(deleteBtn, 3)

            ' Add all children to the Grid
            optionGrid.Children.Add(imageBorder)
            optionGrid.Children.Add(optionText)
            optionGrid.Children.Add(editBtn)
            optionGrid.Children.Add(deleteBtn)

            ' Finally, add the Grid to the StackPanel
            VariationOptionsPanel.Children.Add(optionGrid)
        End Sub


    End Class
End Namespace
