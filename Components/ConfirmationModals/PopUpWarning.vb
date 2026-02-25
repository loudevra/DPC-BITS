Imports System.Windows.Controls.Primitives

Namespace DPC.Components.PopUps
    Public Module PopUpWarning

        Public Sub Show(message As String)
            Dim popup As New Popup() With {
                .Placement = PlacementMode.Center,
                .PlacementTarget = Application.Current.MainWindow,
                .StaysOpen = False,
                .AllowsTransparency = True,
                .PopupAnimation = PopupAnimation.Fade
            }

            Dim outerBorder As New Border() With {
                .Background = Brushes.White,
                .CornerRadius = New CornerRadius(12),
                .BorderBrush = New SolidColorBrush(Color.FromRgb(224, 224, 224)),
                .BorderThickness = New Thickness(1),
                .Width = 360
            }
            outerBorder.Effect = New System.Windows.Media.Effects.DropShadowEffect() With {
                .BlurRadius = 20,
                .ShadowDepth = 4,
                .Opacity = 0.15,
                .Color = Colors.Black
            }

            Dim mainStack As New StackPanel()

            ' Header
            Dim headerBorder As New Border() With {
                .Background = New SolidColorBrush(Color.FromRgb(255, 244, 229)),
                .CornerRadius = New CornerRadius(12, 12, 0, 0),
                .Padding = New Thickness(20, 14, 20, 14)
            }
            Dim headerStack As New StackPanel() With {.Orientation = Orientation.Horizontal}

            Dim iconBorder As New Border() With {
                .Background = New SolidColorBrush(Color.FromRgb(255, 167, 38)),
                .Width = 32,
                .Height = 32,
                .CornerRadius = New CornerRadius(16),
                .Margin = New Thickness(0, 0, 12, 0)
            }
            Dim iconText As New TextBlock() With {
                .Text = "!",
                .FontSize = 18,
                .FontWeight = FontWeights.Bold,
                .Foreground = Brushes.White,
                .HorizontalAlignment = HorizontalAlignment.Center,
                .VerticalAlignment = VerticalAlignment.Center
            }
            iconBorder.Child = iconText

            Dim headerLabel As New TextBlock() With {
                .Text = "Warning",
                .FontSize = 15,
                .FontWeight = FontWeights.Bold,
                .Foreground = New SolidColorBrush(Color.FromRgb(230, 81, 0)),
                .VerticalAlignment = VerticalAlignment.Center
            }
            headerStack.Children.Add(iconBorder)
            headerStack.Children.Add(headerLabel)
            headerBorder.Child = headerStack

            ' Divider
            Dim divider1 As New Border() With {
                .Height = 1,
                .Background = New SolidColorBrush(Color.FromRgb(240, 240, 240))
            }

            ' Message
            Dim msgText As New TextBlock() With {
                .Text = message,
                .FontSize = 13,
                .Foreground = New SolidColorBrush(Color.FromRgb(51, 51, 51)),
                .TextWrapping = TextWrapping.Wrap,
                .Margin = New Thickness(24, 20, 24, 20)
            }

            ' Divider
            Dim divider2 As New Border() With {
                .Height = 1,
                .Background = New SolidColorBrush(Color.FromRgb(240, 240, 240))
            }

            ' OK Button
            Dim footerBorder As New Border() With {
                .Padding = New Thickness(20, 14, 20, 14)
            }
            Dim okButton As New Button() With {
                .Content = "OK",
                .Width = 90,
                .Height = 34,
                .HorizontalAlignment = HorizontalAlignment.Right,
                .Background = New SolidColorBrush(Color.FromRgb(58, 58, 58)),
                .Foreground = Brushes.White,
                .FontSize = 13,
                .FontWeight = FontWeights.SemiBold,
                .BorderThickness = New Thickness(0),
                .Cursor = Cursors.Hand
            }

            Dim okTemplate As New ControlTemplate(GetType(Button))
            Dim okFactory As New FrameworkElementFactory(GetType(Border))
            okFactory.SetBinding(Border.BackgroundProperty, New System.Windows.Data.Binding("Background") With {.RelativeSource = New RelativeSource(RelativeSourceMode.TemplatedParent)})
            okFactory.SetValue(Border.CornerRadiusProperty, New CornerRadius(6))
            Dim contentFactory As New FrameworkElementFactory(GetType(ContentPresenter))
            contentFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center)
            contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center)
            okFactory.AppendChild(contentFactory)
            okTemplate.VisualTree = okFactory
            okButton.Template = okTemplate

            AddHandler okButton.Click, Sub(s, ev) popup.IsOpen = False

            footerBorder.Child = okButton

            mainStack.Children.Add(headerBorder)
            mainStack.Children.Add(divider1)
            mainStack.Children.Add(msgText)
            mainStack.Children.Add(divider2)
            mainStack.Children.Add(footerBorder)

            outerBorder.Child = mainStack
            popup.Child = outerBorder
            popup.IsOpen = True
        End Sub

    End Module
End Namespace
