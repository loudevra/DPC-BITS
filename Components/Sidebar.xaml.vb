Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media.Animation

Public Class Sidebar
    Inherits UserControl ' Add this line

    Private IsExpanded As Boolean = False

    ' Toggle Sidebar Expansion
    Private Sub ToggleSidebar()
        Dim widthAnimation As New DoubleAnimation()
        If IsExpanded Then
            widthAnimation.To = 65 ' Collapse
        Else
            widthAnimation.To = 250 ' Expand
        End If
        widthAnimation.Duration = TimeSpan.FromSeconds(0.3)

        ' Ensure Transform is applied
        If Me.RenderTransform Is Nothing OrElse Not TypeOf Me.RenderTransform Is ScaleTransform Then
            Me.RenderTransform = New ScaleTransform(1, 1)
        End If

        Dim scaleTransform As ScaleTransform = TryCast(Me.RenderTransform, ScaleTransform)
        If scaleTransform IsNot Nothing Then
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, widthAnimation)
        End If

        IsExpanded = Not IsExpanded
    End Sub


    ' Handle Button Clicks
    Private Sub SidebarButton_Click(sender As Object, e As RoutedEventArgs)
        Dim button As Button = TryCast(sender, Button)
        If button IsNot Nothing Then
            Select Case button.Tag.ToString()
                Case "Logout"
                    MessageBox.Show("Logging Out...")
                Case Else
                    MessageBox.Show("Navigating to " & button.Tag.ToString())
            End Select
        End If
    End Sub

    ' Expand Sidebar on Hover
    Private Sub Sidebar_MouseEnter(sender As Object, e As MouseEventArgs)
        If Not IsExpanded Then ToggleSidebar()
    End Sub

    ' Collapse Sidebar on Leave
    Private Sub Sidebar_MouseLeave(sender As Object, e As MouseEventArgs)
        If IsExpanded Then ToggleSidebar()
    End Sub
End Class
