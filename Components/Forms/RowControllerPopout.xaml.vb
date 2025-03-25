Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Components

Namespace DPC.Components.Forms
    Public Class RowControllerPopout
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Private popup As Popup
        Private recentlyClosed As Boolean = False

        Private Sub AddRows_Click(sender As Object, e As RoutedEventArgs)
            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing Then Return

            ' Prevent reopening if the popup was just closed
            If recentlyClosed Then
                recentlyClosed = False
                Return
            End If

            ' If the popup exists and is open, close it
            If popup IsNot Nothing AndAlso popup.IsOpen Then
                popup.IsOpen = False
                recentlyClosed = True
                Return
            End If

            ' Ensure the popup is only created once
            popup = New Popup With {
                .PlacementTarget = clickedButton,
                .Placement = PlacementMode.Relative,
                .StaysOpen = False,
                .AllowsTransparency = True
            }

            Dim popOutContent As New DPC.Components.Forms.AddRowPopout()
            popup.Child = popOutContent

            ' Adjust position once the content is loaded
            AddHandler popOutContent.Loaded, Sub()
                                                 popup.HorizontalOffset = -popup.Child.DesiredSize.Width
                                                 popup.VerticalOffset = -10
                                             End Sub

            ' Handle popup closure
            AddHandler popup.Closed, Sub()
                                         recentlyClosed = True
                                         Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                     End Sub

            ' Open the popup
            popup.IsOpen = True
        End Sub

        Private Sub RemoveRows_Click(sender As Object, e As RoutedEventArgs)
            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing Then Return

            ' Prevent reopening if the popup was just closed
            If recentlyClosed Then
                recentlyClosed = False
                Return
            End If

            ' If the popup exists and is open, close it
            If popup IsNot Nothing AndAlso popup.IsOpen Then
                popup.IsOpen = False
                recentlyClosed = True
                Return
            End If

            ' Ensure the popup is only created once
            popup = New Popup With {
                .PlacementTarget = clickedButton,
                .Placement = PlacementMode.Relative,
                .StaysOpen = False,
                .AllowsTransparency = True
            }

            Dim popOutContent As New DPC.Components.Forms.RemoveRowPopout()
            popup.Child = popOutContent

            ' Adjust position once the content is loaded
            AddHandler popOutContent.Loaded, Sub()
                                                 popup.HorizontalOffset = -popup.Child.DesiredSize.Width
                                                 popup.VerticalOffset = -20
                                             End Sub

            ' Handle popup closure
            AddHandler popup.Closed, Sub()
                                         recentlyClosed = True
                                         Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                     End Sub

            ' Open the popup
            popup.IsOpen = True
        End Sub


    End Class

End Namespace
