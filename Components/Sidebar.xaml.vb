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
            SidebarContainer.Width = 500 ' Ensure it starts expanded
        End Sub

        Private Sub ToggleSidebar()
            Dim widthAnimation As New DoubleAnimation()

            ' Expand or Collapse
            If IsExpanded Then
                widthAnimation.To = 100 ' Collapse
            Else
                widthAnimation.To = 500 ' Expand
            End If

            widthAnimation.Duration = TimeSpan.FromSeconds(0.3)
            widthAnimation.EasingFunction = New QuadraticEase() With {.EasingMode = EasingMode.EaseInOut}

            ' Apply animation to the Sidebar Container
            SidebarContainer.BeginAnimation(WidthProperty, widthAnimation)

            ' Toggle the state
            IsExpanded = Not IsExpanded
        End Sub

        ' Logo Click Event: Toggle Sidebar
        Private Sub SidebarLogoButton_Click(sender As Object, e As RoutedEventArgs) Handles SidebarLogoButton.Click
            ToggleSidebar()
        End Sub

    End Class
End Namespace
