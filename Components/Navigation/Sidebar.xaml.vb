Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media.Animation
Imports DPC.DPC.Components.UI

Namespace DPC.Components.Navigation
    Public Class Sidebar
        Inherits UserControl

        Private IsExpanded As Boolean = True ' Start expanded

        ' Constructor
        Public Sub New()
            InitializeComponent()
        End Sub

        ''' <summary>
        ''' Handles the click event for the Miscellaneous menu item.
        ''' </summary>
        Private Sub OpenMiscellaneous(sender As Object, e As RoutedEventArgs)
            Dim popupMenu As New PopUpMenuMiscelleneous()

            ' Get the position of the Stocks button
            Dim button As Button = CType(sender, Button)
            Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

            ' Call the ShowPopup method to show the popup at the button's position
            popupMenu.ShowPopup(Me, sender)
        End Sub

        ''' <summary>
        ''' Handles the click event for the Dashboard menu item.
        ''' </summary>
        Private Sub OpenDashboard(sender As Object, e As RoutedEventArgs)
            ' Open Dashboard
            Dim dashboardWindow As New Views.Dashboard.Dashboard()
            dashboardWindow.Show()

            ' Close the current window where this UserControl is being used
            Dim currentWindow As Window = Window.GetWindow(Me)
            If currentWindow IsNot Nothing Then
                currentWindow.Close()
            End If
        End Sub


        ''' <summary>
        ''' Handles the click event for the Sales menu item.
        ''' </summary>
        Private Sub OpenSales(sender As Object, e As RoutedEventArgs)
            Dim popupMenu As New PopUpMenuSales()

            ' Get the position of the Stocks button
            Dim button As Button = CType(sender, Button)
            Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

            ' Call the ShowPopup method to show the popup at the button's position
            popupMenu.ShowPopup(Me, sender)
        End Sub

        ''' <summary>
        ''' Handles the click event for the Stocks menu item.
        ''' </summary>
        Private Sub OpenStocksPopup(sender As Object, e As RoutedEventArgs)
            ' Create a new instance of the PopUpMenuStocks control
            Dim popupMenu As New PopUpMenuStocks()

            ' Get the position of the Stocks button
            Dim button As Button = CType(sender, Button)
            Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

            ' Call the ShowPopup method to show the popup at the button's position
            popupMenu.ShowPopup(Me, sender)
        End Sub


        ''' <summary>
        ''' Handles the click event for the CRM menu item.
        ''' </summary>
        Private Sub OpenCRM(sender As Object, e As RoutedEventArgs)
            Dim popupMenu As New PopUpMenuCRM()

            ' Get the position of the Stocks button
            Dim button As Button = CType(sender, Button)
            Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

            ' Call the ShowPopup method to show the popup at the button's position
            popupMenu.ShowPopup(Me, sender)
        End Sub
        ''' <summary>
        ''' Handles the click event for the Project menu item.
        ''' </summary>
        Private Sub OpenProject(sender As Object, e As RoutedEventArgs)
            Dim popupMenu As New PopUpMenuProjects()

            ' Get the position of the Stocks button
            Dim button As Button = CType(sender, Button)
            Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

            ' Call the ShowPopup method to show the popup at the button's position
            popupMenu.ShowPopup(Me, sender)
        End Sub
        Private Sub OpenPromoCodes(sender As Object, e As RoutedEventArgs)
            Dim popupMenu As New PopUpMenuPromoCodes()

            ' Get the position of the Stocks button
            Dim button As Button = CType(sender, Button)
            Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

            ' Call the ShowPopup method to show the popup at the button's position
            popupMenu.ShowPopup(Me, sender)
        End Sub
        Private Sub OpenDataReports(sender As Object, e As RoutedEventArgs)
            Dim popupMenu As New PopUpMenuDataReports()

            ' Get the position of the Stocks button
            Dim button As Button = CType(sender, Button)
            Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

            ' Call the ShowPopup method to show the popup at the button's position
            popupMenu.ShowPopup(Me, sender)
        End Sub
        Private Sub OpenHRM(sender As Object, e As RoutedEventArgs)
            Dim popupMenu As New PopUpMenuHRM()

            ' Get the position of the Stocks button
            Dim button As Button = CType(sender, Button)
            Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

            ' Call the ShowPopup method to show the popup at the button's position
            popupMenu.ShowPopup(Me, sender)
        End Sub
        Private Sub OpenAccounts(sender As Object, e As RoutedEventArgs)
            Dim popupMenu As New PopUpMenuAccounts()

            ' Get the position of the Stocks button
            Dim button As Button = CType(sender, Button)
            Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

            ' Call the ShowPopup method to show the popup at the button's position
            popupMenu.ShowPopup(Me, sender)
        End Sub
        Private Sub Logout(sender As Object, e As RoutedEventArgs)
            ' Navigate to MainWindow.xaml
            Dim mainWindow As New MainWindow()
            Application.Current.MainWindow = mainWindow
            mainWindow.Show()
            ' Close the current window
            Dim currentWindow As Window = Window.GetWindow(Me)
            If currentWindow IsNot Nothing Then
                currentWindow.Close()
            End If
        End Sub

        ''' <summary>
        ''' Toggles the sidebar width and hides text when collapsed.
        ''' </summary>
        Private Sub ToggleSidebar()
            Dim widthAnimation As New DoubleAnimation()

            ' Expand or Collapse
            If IsExpanded Then
                widthAnimation.To = 85 ' Collapse
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
