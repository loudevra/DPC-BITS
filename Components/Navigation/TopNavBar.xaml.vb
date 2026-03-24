Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Threading ' Required for the background timer
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Converters.ValueConverter
Imports DPC.DPC.Data.Helpers
Imports MaterialDesignThemes.Wpf
Imports MySql.Data.MySqlClient

Namespace DPC.Components.Navigation
    Partial Public Class TopNavBar
        ' Events for navigation
        Public Event NavigateToPOS()
        Public Event RestoreDefaultSidebar()

        ' Declare the background timer
        Private NotifTimer As DispatcherTimer

        Public Sub New()
            InitializeComponent()
            ApplyHalfWidthConverter()
            AddHandler Me.Loaded, AddressOf OnNavBarLoaded
        End Sub

        Private Sub OnNavBarLoaded(sender As Object, e As RoutedEventArgs)
            ' Run the first check immediately when the navbar loads
            LoadNotificationBadge()

            ' Setup the background timer to check for new notifications every 30 seconds
            NotifTimer = New DispatcherTimer()
            NotifTimer.Interval = TimeSpan.FromSeconds(30)
            AddHandler NotifTimer.Tick, AddressOf AutoCheckNotifications
            NotifTimer.Start()
        End Sub

        ' The method that runs every time the timer ticks
        Private Sub AutoCheckNotifications(sender As Object, e As EventArgs)
            LoadNotificationBadge()
        End Sub

        Public Sub LoadNotificationBadge()
            Dim count As Integer = 0

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    ' Count ALL unread login history records
                    Dim query As String = "SELECT COUNT(*) FROM employeeloginhistory WHERE is_read = 0"

                    Using cmd As New MySqlCommand(query, conn)
                        count = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using
                End Using
            Catch ex As Exception
                count = 0
            End Try

            ' Update the UI badge based on the count
            If count > 0 Then
                NotificationCount.Text = If(count > 99, "99+", count.ToString())
                NotificationBadge.Visibility = Visibility.Visible
            Else
                NotificationBadge.Visibility = Visibility.Collapsed
            End If
        End Sub

        Private Sub ApplyHalfWidthConverter()
            Dim binding As New Binding("ActualWidth") With {
                .Source = Me,
                .Converter = New HalfWidthConverter(),
                .Mode = BindingMode.OneWay
            }
            SearchBar.SetBinding(Grid.MaxWidthProperty, binding)
        End Sub

        Private Sub OpenPOS(sender As Object, e As RoutedEventArgs)
            RaiseEvent NavigateToPOS()
        End Sub

        Private Sub ChangeLocation(sender As Object, e As RoutedEventArgs)
            RaiseEvent RestoreDefaultSidebar()
            MessageBox.Show("Changing business location...")
        End Sub

        Private Sub SearchCustomer(sender As Object, e As RoutedEventArgs)
            RaiseEvent RestoreDefaultSidebar()
            Dim searchQuery As String = SearchBar.Text
            MessageBox.Show($"Searching for: {searchQuery}")
        End Sub

        ' Show Notifications and mark as read
        Private Sub ShowNotifications(sender As Object, e As RoutedEventArgs)
            RaiseEvent RestoreDefaultSidebar()

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    ' Mark ALL logins as read
                    Dim query As String = "UPDATE employeeloginhistory SET is_read = 1 WHERE is_read = 0"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
            Catch ex As Exception
            End Try

            ' Refresh the badge immediately so it disappears
            LoadNotificationBadge()

            ' Open the Notification Modal
            Dim notifModal As New DPC.Components.ConfirmationModals.NotificationModal(CacheOnEmployeeID)
            notifModal.ShowDialog()
        End Sub

        Private Sub ShowMessages(sender As Object, e As RoutedEventArgs)
            RaiseEvent RestoreDefaultSidebar()
            MessageBox.Show("Showing messages...")
        End Sub

        Private Sub ToggleClockInOut(sender As Object, e As RoutedEventArgs)
            RaiseEvent RestoreDefaultSidebar()
            If ClockIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ClockOutline Then
                ClockIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Clock
                MessageBox.Show("Clocked In!")
            Else
                ClockIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ClockOutline
                MessageBox.Show("Clocked Out!")
            End If
        End Sub

        Private Sub MinimizeWindow(sender As Object, e As RoutedEventArgs)
            Dim parentWindow As Window = Window.GetWindow(Me)
            If parentWindow IsNot Nothing Then
                parentWindow.WindowState = WindowState.Minimized
            End If
        End Sub

        Private Sub MaximizeRestoreWindow(sender As Object, e As RoutedEventArgs)
            Dim parentWindow As Window = Window.GetWindow(Me)
            If parentWindow IsNot Nothing Then
                If parentWindow.WindowState = WindowState.Maximized Then
                    parentWindow.WindowState = WindowState.Normal
                    Maximizebtn.Kind = PackIconKind.WindowMaximize
                Else
                    parentWindow.WindowState = WindowState.Maximized
                    Maximizebtn.Kind = PackIconKind.WindowRestore
                End If
            End If
        End Sub
    End Class
End Namespace