Imports System.Windows.Controls.Primitives
Imports System.Windows

Public Class PopupHelper
    Private Shared activePopup As Popup
    Private Shared recentlyClosed As Boolean = False
    Private Shared currentControl As UserControl
    Private Shared currentWindow As Window
    Private Shared mouseHandlerAttached As Boolean = False
    Private Shared mousePreviewHandler As MouseButtonEventHandler
    Private Shared closeOnBackgroundClick As Boolean = True  ' Default value for background closing
    Private Shared sizeChangedHandler As SizeChangedEventHandler  ' Correct event handler type
    Private Shared originalOwner As Window = Nothing

    Public Shared Sub OpenPopupWithControl(sender As Object, control As UserControl, position As String,
                                          Optional verticalOffset As Double = 0,
                                          Optional horizontalOffset As Double = 0,
                                          Optional closeOnBackground As Boolean = True,
                                          Optional window As Window = Nothing
                                         )
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
        closeOnBackgroundClick = closeOnBackground  ' Store the parameter value

        ' Get the proper parent window
        Dim parentWindow As Window = If(window IsNot Nothing, window, Application.Current.MainWindow)

        ' Create a new popup with improved settings
        activePopup = New Popup With {
            .StaysOpen = True,
            .AllowsTransparency = True,
            .Child = control
        }

        ' Set proper placement settings
        If position.ToLower() = "windowcenter" Then
            ' For window center, we'll use the window as placement target
            activePopup.PlacementTarget = parentWindow
            activePopup.Placement = PlacementMode.Center
        Else
            ' For other positions, use the clicked element
            activePopup.PlacementTarget = clickedElement
            activePopup.Placement = PlacementMode.Custom
        End If

        ' Add event handlers for popup
        AddHandler activePopup.Closed, AddressOf OnPopupClosed

        ' Add event handler to detect clicks outside the popup
        mousePreviewHandler = AddressOf OnGlobalMouseDown
        If parentWindow IsNot Nothing Then
            AddHandler parentWindow.PreviewMouseDown, mousePreviewHandler
        End If
        mouseHandlerAttached = True

        ' Add handler for window deactivation
        If parentWindow IsNot Nothing Then
            AddHandler parentWindow.Deactivated, AddressOf OnWindowDeactivated
            AddHandler parentWindow.Activated, AddressOf OnWindowActivated
        End If

        ' Handler for window resizing to keep popup positioned correctly
        sizeChangedHandler = AddressOf OnWindowSizeChanged  ' Using named method for clarity
        If parentWindow IsNot Nothing Then
            AddHandler parentWindow.SizeChanged, sizeChangedHandler
        End If

        ' Now open the popup
        activePopup.IsOpen = True

        ' Position the popup after it's opened
        AddHandler control.Loaded, Sub()
                                       UpdatePopupPosition(clickedElement, control, position, horizontalOffset, verticalOffset)
                                   End Sub
    End Sub

    Private Shared Sub OnWindowSizeChanged(sender As Object, e As SizeChangedEventArgs)
        ' Get the proper parent window
        Dim parentWindow As Window = If(currentWindow IsNot Nothing, currentWindow, Application.Current.MainWindow)
        Dim clickedElement = activePopup.PlacementTarget

        If currentControl IsNot Nothing AndAlso activePopup IsNot Nothing Then
            ' Find the position by checking the current placement mode
            Dim position As String = If(activePopup.Placement = PlacementMode.Center, "windowcenter", "default")
            UpdatePopupPosition(TryCast(clickedElement, FrameworkElement), currentControl, position, activePopup.HorizontalOffset, activePopup.VerticalOffset)
        End If
    End Sub

    Private Shared Sub OnPopupClosed(sender As Object, e As EventArgs)
        recentlyClosed = True
        RemoveAllHandlers()
        Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
    End Sub

    Private Shared Sub OnGlobalMouseDown(sender As Object, e As MouseButtonEventArgs)
        ' Skip if popup is already closed or background clicking is disabled
        If activePopup Is Nothing OrElse Not activePopup.IsOpen OrElse Not closeOnBackgroundClick Then
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
            Dim parentWindow As Window = If(currentWindow IsNot Nothing, currentWindow, Application.Current.MainWindow)
            If parentWindow IsNot Nothing Then
                RemoveHandler parentWindow.PreviewMouseDown, mousePreviewHandler

                ' Also remove SizeChanged handler with correct type
                If sizeChangedHandler IsNot Nothing Then
                    RemoveHandler parentWindow.SizeChanged, sizeChangedHandler
                End If
            End If
            mouseHandlerAttached = False
        End If

        If currentWindow IsNot Nothing Then
            RemoveHandler currentWindow.Deactivated, AddressOf OnWindowDeactivated
            RemoveHandler currentWindow.Activated, AddressOf OnWindowActivated
        End If
    End Sub

    Private Shared Sub UpdatePopupPosition(clickedElement As FrameworkElement, control As UserControl, position As String,
                                          horizontalOffset As Double, verticalOffset As Double)
        If activePopup Is Nothing Then Return

        ' Ensure the control has been measured
        control.Measure(New Size(Double.PositiveInfinity, Double.PositiveInfinity))
        Dim popupWidth As Double = control.DesiredSize.Width
        Dim popupHeight As Double = control.DesiredSize.Height

        ' Use ActualWidth/Height if DesiredSize returned 0
        If popupWidth = 0 Then popupWidth = control.ActualWidth
        If popupHeight = 0 Then popupHeight = control.ActualHeight

        ' Get the proper parent window
        Dim parentWindow As Window = If(currentWindow IsNot Nothing, currentWindow, Application.Current.MainWindow)

        ' Adjust position based on requested placement
        If position.ToLower() = "windowcenter" Then
            ' For window center positioning - use PlacementMode.Center with offsets
            activePopup.HorizontalOffset = horizontalOffset
            activePopup.VerticalOffset = verticalOffset

            ' Force update if needed
            If parentWindow IsNot Nothing Then
                parentWindow.UpdateLayout()
            End If
        Else
            ' For element-based positioning
            activePopup.PlacementTarget = clickedElement

            ' Set custom placement callback for precise control
            activePopup.CustomPopupPlacementCallback = Function(popupSize, targetSize, offset)
                                                           Dim placements As New List(Of CustomPopupPlacement)()
                                                           Dim point As Point

                                                           Select Case position.ToLower()
                                                               Case "center"
                                                                   point = New Point((targetSize.Width - popupSize.Width) / 2 + horizontalOffset,
                                                                                    targetSize.Height + verticalOffset)

                                                               Case "left"
                                                                   point = New Point(-popupSize.Width - 5 + horizontalOffset,
                                                                                    verticalOffset)

                                                               Case "right"
                                                                   point = New Point(targetSize.Width + 5 + horizontalOffset,
                                                                                    verticalOffset)

                                                               Case Else ' Default positioning
                                                                   point = New Point((targetSize.Width - popupSize.Width) / 2 + horizontalOffset,
                                                                                    targetSize.Height + 5 + verticalOffset)
                                                           End Select

                                                           placements.Add(New CustomPopupPlacement(point, PopupPrimaryAxis.None))
                                                           Return placements.ToArray()
                                                       End Function
        End If
    End Sub


End Class