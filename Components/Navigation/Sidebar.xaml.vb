Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media.Animation
Imports DPC.DPC.Components.UI
Imports DPC.DPC.Data.Helpers
Imports MySql.Data.MySqlClient

Namespace DPC.Components.Navigation
    Public Class Sidebar
        Inherits UserControl

        Private Shared RoleName As String
        Private IsExpanded As Boolean = True
        Public Event LogoButtonClick As RoutedEventHandler
        Public Event SidebarToggled(isExpanded As Boolean)
        Private Shared Sales, Stock, Crm, Project, Accounts, Miscellaneous, AssignProject, CustomerProfile, Employees, Reports, Delete, POS, SalesEdit, StockEdit As Boolean

        Public Sub New(Optional _roleName As String = Nothing)
            InitializeComponent()

            RoleName = _roleName

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim query As String = "SELECT * FROM permissions WHERE Role = '" & RoleName & "'"
                    Dim cmd As New MySqlCommand(query, conn)
                    Dim reader = cmd.ExecuteReader()
                    While (reader.Read)
                        Sales = Convert.ToBoolean(reader("Sales"))
                        Stock = Convert.ToBoolean(reader("Stock"))
                        Crm = Convert.ToBoolean(reader("Crm"))
                        Project = Convert.ToBoolean(reader("Project"))
                        Accounts = Convert.ToBoolean(reader("Accounts"))
                        Miscellaneous = Convert.ToBoolean(reader("Miscellaneous"))
                        AssignProject = Convert.ToBoolean(reader("Assign Project"))
                        CustomerProfile = Convert.ToBoolean(reader("Customer Profile"))
                        Employees = Convert.ToBoolean(reader("Employees"))
                        Reports = Convert.ToBoolean(reader("Reports"))
                        Delete = Convert.ToBoolean(reader("Delete"))
                        POS = Convert.ToBoolean(reader("POS"))
                        SalesEdit = Convert.ToBoolean(reader("Sales Edit"))
                        StockEdit = Convert.ToBoolean(reader("Stock Edit"))
                    End While
                Catch ex As Exception

                End Try
            End Using

            ' Attach event handlers dynamically
            AddHandler SidebarLogoButton.Click, AddressOf SidebarLogoButton_Click
        End Sub

        ''' <summary>
        ''' Toggles the sidebar width and hides text when collapsed.
        ''' </summary>
        Private Sub ToggleSidebar()
            Dim sidebarAnimation As New DoubleAnimation()
            Dim newSidebarWidth As Double

            ' Get the parent window (Base.xaml)
            Dim baseWindow As Base = TryCast(Window.GetWindow(Me), Base)
            If baseWindow Is Nothing Then Exit Sub ' Ensure the base window exists

            ' Expand or Collapse Logic
            If IsExpanded Then
                newSidebarWidth = 80 ' Collapse Sidebar
                SidebarContainer.HorizontalAlignment = HorizontalAlignment.Left
            Else
                newSidebarWidth = 260 ' Expand Sidebar
                SidebarContainer.HorizontalAlignment = HorizontalAlignment.Left
            End If

            ' Sidebar Width Animation
            sidebarAnimation.To = newSidebarWidth
            sidebarAnimation.Duration = TimeSpan.FromSeconds(0.6)
            sidebarAnimation.EasingFunction = New QuadraticEase() With {.EasingMode = EasingMode.EaseInOut}

            ' Apply Animation to Sidebar
            SidebarContainer.BeginAnimation(WidthProperty, sidebarAnimation)

            ' Handle UI Visibility AFTER animation completes
            AddHandler sidebarAnimation.Completed, Sub()
                                                       If Not IsExpanded Then
                                                           ' Collapsing Sidebar: Hide labels AFTER animation finishes

                                                           UserProfile.Visibility = Visibility.Collapsed

                                                           ' Hide text inside buttons
                                                           For Each child As UIElement In SidebarMenu.Children
                                                               If TypeOf child Is Button Then
                                                                   Dim btn As Button = CType(child, Button)
                                                                   If TypeOf btn.Content Is StackPanel AndAlso CType(btn.Content, StackPanel).Children.Count > 1 Then
                                                                       CType(btn.Content, StackPanel).Children(1).Visibility = Visibility.Collapsed
                                                                   End If
                                                               End If
                                                           Next

                                                           ' Update Sidebar Style for Collapsed Mode
                                                           SidebarContainer.Style = CType(FindResource("CollapsedSidebarStyle"), Style)

                                                       Else
                                                           ' Expanding Sidebar: Show labels immediately

                                                           UserProfile.Visibility = Visibility.Visible

                                                           ' Show text inside buttons
                                                           For Each child As UIElement In SidebarMenu.Children
                                                               If TypeOf child Is Button Then
                                                                   Dim btn As Button = CType(child, Button)
                                                                   If TypeOf btn.Content Is StackPanel AndAlso CType(btn.Content, StackPanel).Children.Count > 1 Then
                                                                       CType(btn.Content, StackPanel).Children(1).Visibility = Visibility.Visible
                                                                   End If
                                                               End If
                                                           Next

                                                           ' Update Sidebar Style for Expanded Mode
                                                           SidebarContainer.Style = CType(FindResource("ExpandedSidebarStyle"), Style)
                                                       End If
                                                   End Sub

            ' Toggle state
            IsExpanded = Not IsExpanded

            ' 🔥 Trigger the event so Base.xaml knows about the toggle
            RaiseEvent SidebarToggled(IsExpanded)
        End Sub

        ''' <summary>
        ''' Handles sidebar logo button click to toggle sidebar.
        ''' </summary>
        Private Sub SidebarLogoButton_Click(sender As Object, e As RoutedEventArgs)
            ToggleSidebar()
            RaiseEvent LogoButtonClick(Me, e)
        End Sub

        ''' <summary>
        ''' Opens the Dashboard and closes the current window.
        ''' </summary>
        Private Sub OpenDashboard(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("dashboard", Me)
        End Sub


        ''' <summary>
        ''' Opens the Sales popup menu.
        ''' </summary>
        Private Sub OpenSales(sender As Object, e As RoutedEventArgs)
            If Sales = True Or RoleName = "Administrator" Then
                Dim popupMenu As New PopUpMenuSales()

                ' Get the position of the Stocks button
                Dim button As Button = CType(sender, Button)
                Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

                ' Call the ShowPopup method to show the popup at the button's position
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin")
            End If
        End Sub

        ''' <summary>
        ''' Opens the Stocks popup menu.
        ''' </summary>
        Private Sub OpenStocksPopup(sender As Object, e As RoutedEventArgs)
            If Stock = True Or RoleName = "Administrator" Then
                ' Create a new instance of the PopUpMenuStocks control
                Dim popupMenu As New PopUpMenuStocks()

                ' Get the position of the Stocks button
                Dim button As Button = CType(sender, Button)
                Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

                ' Call the ShowPopup method to show the popup at the button's position
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin")
            End If

        End Sub

        ''' <summary>
        ''' Opens the CRM popup menu.
        ''' </summary>
        Private Sub OpenCRM(sender As Object, e As RoutedEventArgs)
            If Crm = True Or RoleName = "Administrator" Then
                Dim popupMenu As New PopUpMenuCRM()

                ' Get the position of the Stocks button
                Dim button As Button = CType(sender, Button)
                Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

                ' Call the ShowPopup method to show the popup at the button's position
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin")
            End If

        End Sub

        ''' <summary>
        ''' Opens the Projects popup menu.
        ''' </summary>
        Private Sub OpenProject(sender As Object, e As RoutedEventArgs)
            If Project = True Or AssignProject = True Or AssignProject = False Or RoleName = "Administrator" Then
                Dim popupMenu As New PopUpMenuProjects(AssignProject, RoleName)

                ' Get the position of the Stocks button
                Dim button As Button = CType(sender, Button)
                Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

                ' Call the ShowPopup method to show the popup at the button's position
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin")
            End If

        End Sub

        ''' <summary>
        ''' Opens the Promo Codes popup menu.
        ''' </summary>
        Private Sub OpenPromoCodes(sender As Object, e As RoutedEventArgs)
            Dim popupMenu As New PopUpMenuPromoCodes()

            ' Get the position of the Stocks button
            Dim button As Button = CType(sender, Button)
            Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

            ' Call the ShowPopup method to show the popup at the button's position
            popupMenu.ShowPopup(Me, sender)
        End Sub

        ''' <summary>
        ''' Opens the Data Reports popup menu.
        ''' </summary>
        Private Sub OpenDataReports(sender As Object, e As RoutedEventArgs)
            If Reports = True Or RoleName = "Administrator" Then
                Dim popupMenu As New PopUpMenuDataReports()

                ' Get the position of the Stocks button
                Dim button As Button = CType(sender, Button)
                Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

                ' Call the ShowPopup method to show the popup at the button's position
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin")
            End If

        End Sub

        ''' <summary>
        ''' Opens the HRM popup menu.
        ''' </summary>
        Private Sub OpenHRM(sender As Object, e As RoutedEventArgs)
            If Employees = True Or RoleName = "Administrator" Then
                Dim popupMenu As New PopUpMenuHRM()

                ' Get the position of the Stocks button
                Dim button As Button = CType(sender, Button)
                Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

                ' Call the ShowPopup method to show the popup at the button's position
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin")
            End If

        End Sub

        ''' <summary>
        ''' Opens the Accounts popup menu.
        ''' </summary>
        Private Sub OpenAccounts(sender As Object, e As RoutedEventArgs)
            If Accounts = True Or RoleName = "Administrator" Then
                Dim popupMenu As New PopUpMenuAccounts()

                ' Get the position of the Stocks button
                Dim button As Button = CType(sender, Button)
                Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

                ' Call the ShowPopup method to show the popup at the button's position
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin")
            End If

        End Sub

        ''' <summary>
        ''' Opens the Miscellaneous popup menu.
        ''' </summary>
        Private Sub OpenMiscellaneous(sender As Object, e As RoutedEventArgs)
            If Miscellaneous = True Or RoleName = "Administrator" Then
                Dim popupMenu As New PopUpMenuMiscelleneous()

                ' Get the position of the Stocks button
                Dim button As Button = CType(sender, Button)
                Dim buttonPosition As Point = button.TransformToAncestor(Me).Transform(New Point(0, 0))

                ' Call the ShowPopup method to show the popup at the button's position
                popupMenu.ShowPopup(Me, sender)
            Else
                MessageBox.Show("Access not permitted. Consult with admin")
            End If

        End Sub

        ''' <summary>
        ''' Logs out and navigates back to the MainWindow.
        ''' </summary>
        Private Sub Logout(sender As Object, e As RoutedEventArgs)
            Dim mainWindow As New MainWindow()
            Application.Current.MainWindow = mainWindow
            mainWindow.Show()

            ' Close the current window
            Dim currentWindow As Window = Window.GetWindow(Me)
            If currentWindow IsNot Nothing Then currentWindow.Close()
        End Sub

        ' Reflects the User who logged in
        Private Sub Sidebar_Loaded(sender As Object, e As RoutedEventArgs)
            UserName.Text = CacheOnLoggedInName
            UserEmail.Text = CacheOnLoggedInEmail
        End Sub
    End Class
End Namespace