Imports System.Windows.Controls.Primitives

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

    End Class
End Namespace
