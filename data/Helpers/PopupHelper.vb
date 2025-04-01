Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Threading.Tasks

Public Class PopupHelper
    Private Shared popup As Popup
    Private Shared recentlyClosed As Boolean = False

    ' Generalized method to open a popup with any user control and button click
    Public Shared Sub OpenPopupWithControl(sender As Object, control As UserControl)
        Dim clickedButton As Button = TryCast(sender, Button)
        If clickedButton Is Nothing Then Return

        ' Prevent reopening if the popup was recently closed by background click
        If recentlyClosed Then
            recentlyClosed = False
            Return
        End If

        ' If popup is already open, close it
        If popup IsNot Nothing AndAlso popup.IsOpen Then
            popup.IsOpen = False
            recentlyClosed = True
            Return
        End If

        ' Create a new popup and set its properties
        popup = New Popup With {
            .PlacementTarget = clickedButton,
            .Placement = PlacementMode.Bottom,
            .StaysOpen = False,
            .AllowsTransparency = True
        }

        ' Assign the provided control as the child of the popup
        popup.Child = control

        ' Handle the popup close event
        AddHandler popup.Closed, Sub()
                                     recentlyClosed = True
                                     Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                 End Sub


        ' Center the popup on the screen
        AddHandler control.Loaded, Sub()
                                       Dim mainWindow As Window = Application.Current.MainWindow
                                       If mainWindow IsNot Nothing Then
                                           Dim windowLeft = mainWindow.Left
                                           Dim windowTop = mainWindow.Top
                                           Dim windowWidth = mainWindow.ActualWidth
                                           Dim windowHeight = mainWindow.ActualHeight

                                           Dim popupWidth = popup.Child.DesiredSize.Width
                                           Dim popupHeight = popup.Child.DesiredSize.Height

                                           Dim centerX = windowLeft + (windowWidth - popupWidth) / 2
                                           Dim centerY = windowTop + (windowHeight - popupHeight) / 2

                                           popup.HorizontalOffset = centerX - clickedButton.PointToScreen(New Point(0, 0)).X
                                           popup.VerticalOffset = centerY - clickedButton.PointToScreen(New Point(0, 0)).Y
                                       End If
                                   End Sub

        ' Open the popup
        popup.IsOpen = True
    End Sub
End Class

