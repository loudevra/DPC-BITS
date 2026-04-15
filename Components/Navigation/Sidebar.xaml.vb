Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media.Animation
Imports DPC.DPC.Components.UI
Imports DPC.DPC.Data.Helpers
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers.SoftwareUpdate
Imports System.Windows.Media
Imports MaterialDesignThemes.Wpf

Namespace DPC.Components.Navigation
    Public Class Sidebar
        Inherits UserControl

        Private IsExpanded As Boolean = True
        Public Event LogoButtonClick As RoutedEventHandler
        Public Event SidebarToggled(isExpanded As Boolean)
        Private _permissionTimer As System.Windows.Threading.DispatcherTimer

        Public Sub New()
            InitializeComponent()
            CheckUpdateVisibility()
            VerNum.Text = My.Application.Info.Version.ToString()
            AddHandler SidebarLogoButton.Click, AddressOf SidebarLogoButton_Click
            ApplyPermissionStyles()
        End Sub

        Private Sub ApplyPermissionStyles()
            Dim GrayOut = Sub(btn As Button)
                              If btn Is Nothing Then Return
                              Try
                                  Dim sp = TryCast(btn.Content, StackPanel)
                                  If sp IsNot Nothing Then
                                      For Each child As UIElement In sp.Children
                                          If TypeOf child Is TextBlock Then
                                              CType(child, TextBlock).Foreground = Brushes.Gray
                                          ElseIf TypeOf child Is PackIcon Then
                                              CType(child, PackIcon).Foreground = Brushes.Gray
                                          End If
                                      Next
                                  Else
                                      btn.Foreground = Brushes.Gray
                                  End If
                              Catch
                              End Try
                          End Sub

            Dim UnGrayOut = Sub(btn As Button)
                                If btn Is Nothing Then Return
                                Try
                                    Dim sp = TryCast(btn.Content, StackPanel)
                                    If sp IsNot Nothing Then
                                        For Each child As UIElement In sp.Children
                                            If TypeOf child Is TextBlock Then
                                                CType(child, TextBlock).Foreground = Brushes.White
                                            ElseIf TypeOf child Is PackIcon Then
                                                CType(child, PackIcon).Foreground = Brushes.White
                                            End If
                                        Next
                                    Else
                                        btn.Foreground = Brushes.White
                                    End If
                                Catch
                                End Try
                            End Sub

            ' Reset all first
            UnGrayOut(BtnDashboard)
            UnGrayOut(BtnSales)
            UnGrayOut(BtnStocks)
            UnGrayOut(BtnCRM)
            UnGrayOut(BtnProjects)
            UnGrayOut(BtnDataReports)
            UnGrayOut(BtnMiscellaneous)
            UnGrayOut(BtnHRM)

            ' Dashboard
            If Not PermissionCache.Can("Dashboard") Then GrayOut(BtnDashboard)

            ' Sales
            If Not PermissionCache.Can("Sales") Then GrayOut(BtnSales)

            ' Stocks
            If Not PermissionCache.Can("Stocks") Then GrayOut(BtnStocks)

            ' CRM
            If Not PermissionCache.Can("CRM") Then GrayOut(BtnCRM)

            ' Project
            If Not PermissionCache.Can("Project") Then GrayOut(BtnProjects)

            ' Data & Reports
            If Not PermissionCache.Can("Data & Reports") Then GrayOut(BtnDataReports)

            ' Miscellaneous
            If Not PermissionCache.Can("Miscellaneous") Then GrayOut(BtnMiscellaneous)

            ' HRM
            If Not PermissionCache.Can("HRM") Then GrayOut(BtnHRM)

            ' Software Updates
            If Not PermissionCache.Can("Software Updates") Then
                BtnSoftwareUpdates.Visibility = Visibility.Collapsed
            Else
                BtnSoftwareUpdates.Visibility = Visibility.Visible
            End If

        End Sub

        ' ---- Navigation Handlers ----

        Private Sub OpenDashboard(sender As Object, e As RoutedEventArgs)
            If PermissionCache.Can("Dashboard") Then
                ViewLoader.DynamicView.NavigateToView("dashboard", Me)
            Else
                MessageBox.Show("Access not permitted. Consult with admin.")
            End If
        End Sub

        Private Sub OpenSales(sender As Object, e As RoutedEventArgs)
            If PermissionCache.Can("Sales") Then
                Dim popupMenu As New PopUpMenuSales()
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin.")
            End If
        End Sub

        Private Sub OpenStocksPopup(sender As Object, e As RoutedEventArgs)
            If PermissionCache.CanAny("Stocks", "Sales") Then
                Dim popupMenu As New PopUpMenuStocks()
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin.")
            End If
        End Sub

        Private Sub OpenCRM(sender As Object, e As RoutedEventArgs)
            If PermissionCache.Can("CRM") Then
                Dim popupMenu As New PopUpMenuCRM()
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin.")
            End If
        End Sub

        Private Sub OpenProject(sender As Object, e As RoutedEventArgs)
            If PermissionCache.Can("Project") Then
                Dim popupMenu As New PopUpMenuProjects(
                    PermissionCache.Can("Project"),
                    PermissionCache.CurrentRole)
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin.")
            End If
        End Sub

        Private Sub OpenPromoCodes(sender As Object, e As RoutedEventArgs)
            If PermissionCache.Can("Project") Then
                Dim popupMenu As New PopUpMenuPromoCodes()
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin.")
            End If
        End Sub

        Private Sub OpenDataReports(sender As Object, e As RoutedEventArgs)
            If PermissionCache.Can("Data & Reports") Then
                Dim popupMenu As New PopUpMenuDataReports()
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin.")
            End If
        End Sub

        Private Sub OpenHRM(sender As Object, e As RoutedEventArgs)
            If PermissionCache.Can("HRM") Then
                Dim popupMenu As New PopUpMenuHRM()
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin.")
            End If
        End Sub

        Private Sub OpenAccounts(sender As Object, e As RoutedEventArgs)
            If PermissionCache.Can("Dashboard") Then
                Dim popupMenu As New PopUpMenuAccounts()
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin.")
            End If
        End Sub

        Private Sub OpenMiscellaneous(sender As Object, e As RoutedEventArgs)
            If PermissionCache.Can("Miscellaneous") Then
                Dim popupMenu As New PopUpMenuMiscelleneous()
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin.")
            End If
        End Sub

        ' ---- Logout ----

        Private Sub Logout(sender As Object, e As RoutedEventArgs)
            If _permissionTimer IsNot Nothing Then _permissionTimer.Stop()
            PermissionCache.Clear()
            EmployeeLoginHistoryController.AddLogOutHistory(CacheLogInHistoryID)
            Dim mainWindow As New MainWindow()
            Application.Current.MainWindow = mainWindow
            mainWindow.Show()
            Dim currentWindow As Window = Window.GetWindow(Me)
            If currentWindow IsNot Nothing Then currentWindow.Close()
        End Sub

        ' ---- Auto Refresh Timer ----

        Private Sub RefreshPermissions(sender As Object, e As EventArgs)
            PermissionCache.LoadForRole(PermissionCache.CurrentRole)
            ApplyPermissionStyles()
        End Sub

        ' ---- Sidebar Toggle ----

        Private Sub ToggleSidebar()
            Dim sidebarAnimation As New DoubleAnimation()
            Dim newSidebarWidth As Double

            Dim baseWindow As Base = TryCast(Window.GetWindow(Me), Base)
            If baseWindow Is Nothing Then Exit Sub

            If IsExpanded Then
                newSidebarWidth = 80
                SidebarContainer.HorizontalAlignment = HorizontalAlignment.Left
            Else
                newSidebarWidth = 260
                SidebarContainer.HorizontalAlignment = HorizontalAlignment.Left
            End If

            sidebarAnimation.To = newSidebarWidth
            sidebarAnimation.Duration = TimeSpan.FromSeconds(0.6)
            sidebarAnimation.EasingFunction = New QuadraticEase() With {
                .EasingMode = EasingMode.EaseInOut}

            SidebarContainer.BeginAnimation(WidthProperty, sidebarAnimation)

            AddHandler sidebarAnimation.Completed,
                Sub()
                    If Not IsExpanded Then
                        UserProfile.Visibility = Visibility.Collapsed
                        For Each child As UIElement In SidebarMenu.Children
                            If TypeOf child Is Button Then
                                Dim btn As Button = CType(child, Button)
                                If TypeOf btn.Content Is StackPanel AndAlso
                                   CType(btn.Content, StackPanel).Children.Count > 1 Then
                                    CType(btn.Content, StackPanel).Children(1).Visibility =
                                        Visibility.Collapsed
                                End If
                            End If
                        Next
                        SidebarContainer.Style = CType(FindResource("CollapsedSidebarStyle"), Style)
                    Else
                        UserProfile.Visibility = Visibility.Visible
                        For Each child As UIElement In SidebarMenu.Children
                            If TypeOf child Is Button Then
                                Dim btn As Button = CType(child, Button)
                                If TypeOf btn.Content Is StackPanel AndAlso
                                   CType(btn.Content, StackPanel).Children.Count > 1 Then
                                    CType(btn.Content, StackPanel).Children(1).Visibility =
                                        Visibility.Visible
                                End If
                            End If
                        Next
                        SidebarContainer.Style = CType(FindResource("ExpandedSidebarStyle"), Style)
                    End If
                End Sub

            IsExpanded = Not IsExpanded
            RaiseEvent SidebarToggled(IsExpanded)
        End Sub

        Private Sub SidebarLogoButton_Click(sender As Object, e As RoutedEventArgs)
            ToggleSidebar()
            RaiseEvent LogoButtonClick(Me, e)
        End Sub

        Private Async Sub BtnSoftwareUpdates_Click(sender As Object, e As RoutedEventArgs)
            Await SoftwareUpdateHelper.CheckForUpdate()
        End Sub

        Private Async Sub CheckUpdateVisibility()
            Dim isUpdateAvailable = Await SoftwareUpdateHelper.IsUpdateAvailable()
            BtnSoftwareUpdates.Visibility = If(isUpdateAvailable,
                                               Visibility.Visible,
                                               Visibility.Collapsed)
        End Sub

        Private Sub Sidebar_Loaded(sender As Object, e As RoutedEventArgs)
            UserName.Text = CacheOnLoggedInName
            UserEmail.Text = CacheOnLoggedInEmail

            ' Start auto-refresh timer — checks DB every 10 seconds
            _permissionTimer = New System.Windows.Threading.DispatcherTimer()
            _permissionTimer.Interval = TimeSpan.FromSeconds(10)
            AddHandler _permissionTimer.Tick, AddressOf RefreshPermissions
            _permissionTimer.Start()
        End Sub

    End Class
End Namespace

