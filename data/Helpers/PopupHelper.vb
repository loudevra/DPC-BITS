Imports System.Windows.Controls.Primitives
Imports System.Windows

Public Class PopupHelper
    Private Shared popup As Popup
    Private Shared recentlyClosed As Boolean = False

    Public Shared Sub OpenPopupWithControl(sender As Object, control As UserControl, position As String,
                                            Optional verticalOffset As Double = 0,
                                            Optional horizontalOffset As Double = 0,
                                            Optional currentWindow As Window = Nothing)
        Dim clickedElement As FrameworkElement = TryCast(sender, FrameworkElement)
        If clickedElement Is Nothing Then Return

        If recentlyClosed Then
            recentlyClosed = False
            Return
        End If

        If popup IsNot Nothing AndAlso popup.IsOpen Then
            popup.IsOpen = False
            recentlyClosed = True
            Return
        End If

        popup = New Popup With {
            .StaysOpen = False,
            .AllowsTransparency = True,
            .Child = control
        }

        AddHandler popup.Closed, Sub()
                                     recentlyClosed = True
                                     Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                 End Sub

        AddHandler control.Loaded, Sub()
                                       control.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
                                       Dim popupWidth As Double = control.DesiredSize.Width
                                       Dim popupHeight As Double = control.DesiredSize.Height

                                       If popupWidth = 0 Then popupWidth = control.ActualWidth
                                       If popupHeight = 0 Then popupHeight = control.ActualHeight

                                       If position.ToLower() = "windowcenter" Then
                                           If currentWindow IsNot Nothing Then
                                               ' Ensure the window is fully rendered before calculations
                                               Dim CenterPopup = Sub()
                                                                     currentWindow.UpdateLayout()

                                                                     Dim updatedWindowWidth As Double
                                                                     Dim updatedWindowHeight As Double
                                                                     Dim updatedWindowLeft As Double
                                                                     Dim updatedWindowTop As Double

                                                                     ' Check if window is maximized
                                                                     If currentWindow.WindowState = WindowState.Maximized Then
                                                                         ' Get usable screen area (excludes taskbar)
                                                                         Dim screenBounds As Rect = SystemParameters.WorkArea
                                                                         updatedWindowWidth = screenBounds.Width
                                                                         updatedWindowHeight = screenBounds.Height
                                                                         updatedWindowLeft = screenBounds.Left
                                                                         updatedWindowTop = screenBounds.Top
                                                                     Else
                                                                         ' Use RestoreBounds to avoid incorrect size values
                                                                         updatedWindowWidth = currentWindow.RestoreBounds.Width
                                                                         updatedWindowHeight = currentWindow.RestoreBounds.Height
                                                                         updatedWindowLeft = currentWindow.RestoreBounds.Left
                                                                         updatedWindowTop = currentWindow.RestoreBounds.Top
                                                                     End If

                                                                     ' Calculate absolute center with customizable offsets
                                                                     Dim updatedCenterX As Double = updatedWindowLeft + (updatedWindowWidth - popupWidth) / 2 + horizontalOffset
                                                                     Dim updatedCenterY As Double = updatedWindowTop + (updatedWindowHeight - popupHeight) / 2 + verticalOffset

                                                                     ' Apply new position
                                                                     popup.HorizontalOffset = updatedCenterX
                                                                     popup.VerticalOffset = updatedCenterY
                                                                 End Sub

                                               ' Set initial position
                                               currentWindow.Dispatcher.InvokeAsync(CenterPopup, System.Windows.Threading.DispatcherPriority.Render)

                                               ' Keep the popup centered when the window resizes
                                               AddHandler currentWindow.SizeChanged, Sub()
                                                                                         currentWindow.Dispatcher.InvokeAsync(CenterPopup, System.Windows.Threading.DispatcherPriority.Render)
                                                                                     End Sub
                                           End If
                                       Else
                                           ' Positioning logic for other cases (left, right, etc.)
                                           popup.PlacementTarget = clickedElement
                                           popup.Placement = PlacementMode.Relative

                                           Select Case position.ToLower()
                                               Case "center"
                                                   popup.HorizontalOffset = (clickedElement.ActualWidth - popupWidth) / 2 + horizontalOffset
                                                   popup.VerticalOffset = clickedElement.ActualHeight + verticalOffset

                                               Case "left"
                                                   popup.HorizontalOffset = -popupWidth - 5 + horizontalOffset
                                                   popup.VerticalOffset = 0 + verticalOffset

                                               Case "right"
                                                   popup.HorizontalOffset = clickedElement.ActualWidth + 5 + horizontalOffset
                                                   popup.VerticalOffset = 0 + verticalOffset

                                               Case Else
                                                   popup.HorizontalOffset = (clickedElement.ActualWidth - popupWidth) / 2 + horizontalOffset
                                                   popup.VerticalOffset = clickedElement.ActualHeight + 5 + verticalOffset
                                           End Select
                                       End If
                                   End Sub

        popup.IsOpen = True
    End Sub
End Class
