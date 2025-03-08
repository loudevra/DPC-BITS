Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media.Animation

Namespace DPC.Components
    Partial Public Class Sidebar
        Inherits UserControl

        Private IsExpanded As Boolean = True ' Start expanded

        ' Constructor
        Public Sub New()
            InitializeComponent()
            SidebarContainer.Width = 250 ' Ensure it starts expanded
        End Sub

        ''' <summary>
        ''' Toggles the sidebar width and hides text when collapsed.
        ''' </summary>
        Private Sub ToggleSidebar()
            Dim widthAnimation As New DoubleAnimation()

            ' Expand or Collapse
            If IsExpanded Then
                widthAnimation.To = 85 ' Collapse to match the image
                For Each child As UIElement In SidebarMenu.Children
                    If TypeOf child Is Button Then
                        Dim btn As Button = CType(child, Button)
                        If TypeOf btn.Content Is StackPanel Then
                            CType(btn.Content, StackPanel).Children(1).Visibility = Visibility.Collapsed
                        End If
                    End If
                Next
                SidebarTitle.Visibility = Visibility.Collapsed
                SidebarSubtitle.Visibility = Visibility.Collapsed
                UserProfile.Visibility = Visibility.Collapsed
            Else
                widthAnimation.To = 250 ' Expand
                For Each child As UIElement In SidebarMenu.Children
                    If TypeOf child Is Button Then
                        Dim btn As Button = CType(child, Button)
                        If TypeOf btn.Content Is StackPanel Then
                            CType(btn.Content, StackPanel).Children(1).Visibility = Visibility.Visible
                        End If
                    End If
                Next
                SidebarTitle.Visibility = Visibility.Visible
                SidebarSubtitle.Visibility = Visibility.Visible
                UserProfile.Visibility = Visibility.Visible
            End If

            widthAnimation.Duration = TimeSpan.FromSeconds(0.5)
            widthAnimation.EasingFunction = New QuadraticEase() With {.EasingMode = EasingMode.EaseInOut}

            ' Apply animation to the Sidebar Container
            SidebarContainer.BeginAnimation(WidthProperty, widthAnimation)

            ' Toggle the state
            IsExpanded = Not IsExpanded
        End Sub

        ''' <summary>
        ''' Handles the click event for the sidebar logo to toggle sidebar visibility.
        ''' </summary>
        Private Sub SidebarLogoButton_Click(sender As Object, e As RoutedEventArgs) Handles SidebarLogoButton.Click
            ToggleSidebar()
        End Sub
    End Class
End Namespace