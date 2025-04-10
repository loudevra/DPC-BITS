Imports System.Windows.Controls.Primitives
Imports System.Windows

Public Class PopupHelper
    Private Shared activePopup As Popup
    Private Shared recentlyClosed As Boolean = False
    Private Shared currentControl As UserControl
    Private Shared currentWindow As Window
    Private Shared mouseHandlerAttached As Boolean = False
    Private Shared mousePreviewHandler As MouseButtonEventHandler  ' Changed to the correct type

    Public Shared Sub OpenPopupWithControl(sender As Object, control As UserControl, position As String,
                                          Optional verticalOffset As Double = 0,
                                          Optional horizontalOffset As Double = 0,
                                          Optional window As Window = Nothing)
        Dim clickedElement As FrameworkElement = TryCast(sender, FrameworkElement)
        If clickedElement Is Nothing Then Return

        If recentlyClosed Then
            recentlyClosed = False
            Return
        End If

        If activePopup IsNot Nothing AndAlso activePopup.IsOpen Then
            ClosePopup()
            recentlyClosed = True
            Return
        End If

        ' Store references for reuse
        currentControl = control
        currentWindow = window

        ' Create a new popup with improved settings
        activePopup = New Popup With {
            .StaysOpen = True, ' Changed to True to prevent auto-closing
            .AllowsTransparency = True,
            .Child = control,
            .IsOpen = True
        }

        ' Add event handlers for popup
        AddHandler activePopup.Closed, AddressOf OnPopupClosed

        ' Add event handler to detect clicks outside the popup
        mousePreviewHandler = AddressOf OnGlobalMouseDown
        If window IsNot Nothing Then
            AddHandler window.PreviewMouseDown, mousePreviewHandler
        Else
            ' If no window is provided, attach to application main window
            If Application.Current.MainWindow IsNot Nothing Then
                AddHandler Application.Current.MainWindow.PreviewMouseDown, mousePreviewHandler
            End If
        End If
        mouseHandlerAttached = True

        ' Add handler for window deactivation
        If window IsNot Nothing Then
            AddHandler window.Deactivated, AddressOf OnWindowDeactivated
            AddHandler window.Activated, AddressOf OnWindowActivated
        End If

        AddHandler control.Loaded, Sub()
                                       PositionPopup(clickedElement, control, position, horizontalOffset, verticalOffset)
                                   End Sub
    End Sub

    Private Shared Sub OnPopupClosed(sender As Object, e As EventArgs)
        recentlyClosed = True
        RemoveAllHandlers()
        Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
    End Sub

    Private Shared Sub OnGlobalMouseDown(sender As Object, e As MouseButtonEventArgs)
        ' Skip if popup is already closed
        If activePopup Is Nothing OrElse Not activePopup.IsOpen Then
            Return
        End If

        ' Check if click was inside the popup
        Dim popupChild = activePopup.Child
        Dim hitTestResult = VisualTreeHelper.HitTest(popupChild, e.GetPosition(popupChild))

        ' If click was outside the popup and no dialog is open, close the popup
        If hitTestResult Is Nothing AndAlso Not IsDialogOpen() Then
            ClosePopup()
        End If
    End Sub

    Private Shared Sub OnWindowDeactivated(sender As Object, e As EventArgs)
        ' Don't close the popup when the window is deactivated
        ' This allows MessageBox and OpenFileDialog to open without closing the popup
    End Sub

    Private Shared Sub OnWindowActivated(sender As Object, e As EventArgs)
        ' Check if we should restore focus to the popup content
        If activePopup IsNot Nothing AndAlso activePopup.IsOpen AndAlso currentControl IsNot Nothing Then
            ' Optional: Force focus back to popup content
        End If
    End Sub

    Private Shared Function IsDialogOpen() As Boolean
        ' Check if any of the dialogs are currently open
        ' This is an approximation as there's no direct way to check for all dialogs
        Return DialogPanes.Count > 0
    End Function

    Private Shared ReadOnly Property DialogPanes As ICollection(Of DependencyObject)
        Get
            Dim list As New List(Of DependencyObject)
            For Each window In Application.Current.Windows
                If TypeOf window Is Window Then
                    If CType(window, Window).Owner Is currentWindow Then
                        ' This is likely a dialog
                        list.Add(CType(window, DependencyObject))
                    End If
                End If
            Next
            Return list
        End Get
    End Property

    Public Shared Sub ClosePopup()
        If activePopup IsNot Nothing Then
            activePopup.IsOpen = False
            RemoveAllHandlers()
            activePopup = Nothing
        End If
    End Sub

    Private Shared Sub RemoveAllHandlers()
        ' Clean up event handlers
        If activePopup IsNot Nothing Then
            RemoveHandler activePopup.Closed, AddressOf OnPopupClosed
        End If

        ' Remove mouse handler if attached
        If mouseHandlerAttached AndAlso mousePreviewHandler IsNot Nothing Then
            If currentWindow IsNot Nothing Then
                RemoveHandler currentWindow.PreviewMouseDown, mousePreviewHandler
            ElseIf Application.Current.MainWindow IsNot Nothing Then
                RemoveHandler Application.Current.MainWindow.PreviewMouseDown, mousePreviewHandler
            End If
            mouseHandlerAttached = False
        End If

        If currentWindow IsNot Nothing Then
            RemoveHandler currentWindow.Deactivated, AddressOf OnWindowDeactivated
            RemoveHandler currentWindow.Activated, AddressOf OnWindowActivated
        End If
    End Sub

    Private Shared Sub PositionPopup(clickedElement As FrameworkElement, control As UserControl, position As String,
                                    horizontalOffset As Double, verticalOffset As Double)
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
                                          ' Use actual window dimensions
                                          updatedWindowWidth = currentWindow.ActualWidth
                                          updatedWindowHeight = currentWindow.ActualHeight
                                          updatedWindowLeft = currentWindow.Left
                                          updatedWindowTop = currentWindow.Top
                                      End If

                                      ' Calculate absolute center with customizable offsets
                                      Dim updatedCenterX As Double = updatedWindowLeft + (updatedWindowWidth - popupWidth) / 2 + horizontalOffset
                                      Dim updatedCenterY As Double = updatedWindowTop + (updatedWindowHeight - popupHeight) / 2 + verticalOffset

                                      ' Apply new position
                                      activePopup.HorizontalOffset = updatedCenterX
                                      activePopup.VerticalOffset = updatedCenterY
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
            activePopup.PlacementTarget = clickedElement
            activePopup.Placement = PlacementMode.Relative

            Select Case position.ToLower()
                Case "center"
                    activePopup.HorizontalOffset = (clickedElement.ActualWidth - popupWidth) / 2 + horizontalOffset
                    activePopup.VerticalOffset = clickedElement.ActualHeight + verticalOffset

                Case "left"
                    activePopup.HorizontalOffset = -popupWidth - 5 + horizontalOffset
                    activePopup.VerticalOffset = 0 + verticalOffset

                Case "right"
                    activePopup.HorizontalOffset = clickedElement.ActualWidth + 5 + horizontalOffset
                    activePopup.VerticalOffset = 0 + verticalOffset

                Case Else
                    activePopup.HorizontalOffset = (clickedElement.ActualWidth - popupWidth) / 2 + horizontalOffset
                    activePopup.VerticalOffset = clickedElement.ActualHeight + 5 + verticalOffset
            End Select
        End If
    End Sub
End Class