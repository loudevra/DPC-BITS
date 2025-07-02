Imports System.IO
Imports System.Text
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports MaterialDesignThemes.Wpf
Imports Microsoft.Win32
Imports NuGet.Protocol.Plugins

Namespace DPC.Views.POS
    Public Class POSForm
        Inherits UserControl


        Public Sub New()
            InitializeComponent()

        End Sub

        Private Sub CloseSideBarPOS()
            ' Get reference to the parent Base window
            Dim baseWindow As Base = TryCast(Window.GetWindow(Me), Base)

            ' Call the ClosePOS method if the parent is a Base window
            If baseWindow IsNot Nothing Then
                baseWindow.ClosePOS()
            End If
        End Sub


        'This function is used to open and close the content properties of the POS form
        Private Sub OpenContent(sender As Object, e As RoutedEventArgs,
                                OpenStackPanel As StackPanel, OpenIcon As PackIcon, OpenText As TextBlock,
                                CloseStackPanel As StackPanel, CloseIcon As PackIcon, CloseText As TextBlock,
                                CloseStackPanel2 As StackPanel, CloseIcon2 As PackIcon, CloseText2 As TextBlock)
            If OpenStackPanel.Visibility = Visibility.Visible Then
                OpenIcon.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                OpenText.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                OpenStackPanel.Visibility = Visibility.Collapsed
                CloseStackPanel.Visibility = Visibility.Collapsed
                CloseStackPanel2.Visibility = Visibility.Collapsed
            Else
                OpenIcon.Foreground = New BrushConverter().ConvertFromString("#555555")
                OpenText.Foreground = New BrushConverter().ConvertFromString("#555555")

                CloseIcon.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                CloseText.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                CloseIcon2.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                CloseText2.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                OpenStackPanel.Visibility = Visibility.Visible
                CloseStackPanel.Visibility = Visibility.Collapsed
                CloseStackPanel2.Visibility = Visibility.Collapsed
            End If

        End Sub
        Private Sub OpenSettings(sender As Object, e As RoutedEventArgs)
            OpenContent(sender, e,
                        StackPanelSettings, SettingsIcon, SettingsText,
                        StackPanelInvoice, InvoiceIcon, InvoiceText,
                        StackPanelCoupon, CouponIcon, CouponText)
        End Sub
        Private Sub OpenProperties(sender As Object, e As RoutedEventArgs)
            OpenContent(sender, e,
            StackPanelInvoice, InvoiceIcon, InvoiceText,
            StackPanelSettings, SettingsIcon, SettingsText,
            StackPanelCoupon, CouponIcon, CouponText)
        End Sub
        Private Sub OpenCoupon(sender As Object, e As RoutedEventArgs)
            OpenContent(sender, e,
            StackPanelCoupon, CouponIcon, CouponText,
            StackPanelSettings, SettingsIcon, SettingsText,
            StackPanelInvoice, InvoiceIcon, InvoiceText)
        End Sub

        Friend Sub ShowPopup(parent As UIElement, sender As Object)
            ' Ensure sender is a Button
            Dim button As Button = TryCast(sender, Button)
            If button Is Nothing Then
                Return
            End If

            ' Get the window containing the button
            Dim window As Window = Window.GetWindow(button)
            If window Is Nothing Then
                Return
            End If

            ' Get sidebar width - determine if sidebar is expanded or collapsed
            Dim sidebarWidth As Double = 0

            ' Get parent sidebar if available
            Dim parentControl = TryCast(button.Parent, FrameworkElement)
            While parentControl IsNot Nothing
                If TypeOf parentControl Is StackPanel AndAlso parentControl.Name = "SidebarMenu" Then
                    ' Found the sidebar menu container, get its parent (likely the sidebar)
                    Dim sidebarContainer = TryCast(parentControl.Parent, FrameworkElement)
                    If sidebarContainer IsNot Nothing Then
                        sidebarWidth = sidebarContainer.ActualWidth
                        Exit While
                    End If
                ElseIf TypeOf parentControl.Parent Is DPC.Components.Navigation.Sidebar Then
                    ' Direct parent is sidebar
                    sidebarWidth = CType(parentControl.Parent, FrameworkElement).ActualWidth
                    Exit While
                End If
                parentControl = TryCast(parentControl.Parent, FrameworkElement)
            End While

            ' If we couldn't find sidebar, use a default value
            If sidebarWidth = 0 Then
                ' Default to expanded sidebar width
                sidebarWidth = 260
            End If

            ' Create the popup with proper positioning
            Dim popup As New Popup With {
    .Child = Me,
    .StaysOpen = False,
    .Placement = PlacementMode.Relative,
    .PlacementTarget = button,
    .IsOpen = True,
    .AllowsTransparency = True
}

            ' Calculate optimal position based on sidebar width
            If sidebarWidth <= 80 Then
                ' Sidebar is collapsed - position menu farther right
                popup.HorizontalOffset = 60
                popup.VerticalOffset = -button.ActualHeight * 3 ' Align with button
            Else
                ' Sidebar is expanded - position menu immediately to the right
                popup.HorizontalOffset = sidebarWidth - button.Margin.Left
                popup.VerticalOffset = -button.ActualHeight * 3 ' Align with button
            End If

            ' Store references to event handlers so we can remove them later
            Dim locationChangedHandler As EventHandler = Nothing
            Dim sizeChangedHandler As SizeChangedEventHandler = Nothing

            ' Define event handlers
            locationChangedHandler = Sub(s, e)
                                         If popup.IsOpen Then
                                             ' Recalculate position when window moves
                                             popup.HorizontalOffset = popup.HorizontalOffset
                                             popup.VerticalOffset = popup.VerticalOffset
                                         End If
                                     End Sub

            sizeChangedHandler = Sub(s, e)
                                     If popup.IsOpen Then
                                         ' Recalculate position when window resizes
                                         popup.HorizontalOffset = popup.HorizontalOffset
                                         popup.VerticalOffset = popup.VerticalOffset
                                     End If
                                 End Sub

            ' Add event handlers
            AddHandler window.LocationChanged, locationChangedHandler
            AddHandler window.SizeChanged, sizeChangedHandler

            ' Handle popup closed to cleanup event handlers
            AddHandler popup.Closed, Sub(s, e)
                                         RemoveHandler window.LocationChanged, locationChangedHandler
                                         RemoveHandler window.SizeChanged, sizeChangedHandler
                                     End Sub
        End Sub

        Private Sub NavigateToBillingStatement(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("navigatetobillingstatement", Me)
        End Sub

        Private Sub NavigateToCostEstimate(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("navigatetocostestimate", Me)
        End Sub

    End Class
End Namespace