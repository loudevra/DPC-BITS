Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Threading
Imports DPC.DPC.Components.Navigation.ChatBot
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

        ' Notification timer
        Private NotifTimer As DispatcherTimer

        ' ChatBot — single instance keeps conversation history alive
        Private _chatBot As ChatBotWindow = Nothing

        Public Sub New()
            InitializeComponent()
            ApplyHalfWidthConverter()
            AddHandler Me.Loaded, AddressOf OnNavBarLoaded
        End Sub

        Private Sub OnNavBarLoaded(sender As Object, e As RoutedEventArgs)
            LoadNotificationBadge()

            NotifTimer = New DispatcherTimer()
            NotifTimer.Interval = TimeSpan.FromSeconds(30)
            AddHandler NotifTimer.Tick, AddressOf AutoCheckNotifications
            NotifTimer.Start()
        End Sub

        Private Sub AutoCheckNotifications(sender As Object, e As EventArgs)
            LoadNotificationBadge()
        End Sub

        Public Sub LoadNotificationBadge()
            Dim count As Integer = 0

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT COUNT(*) FROM employeeloginhistory WHERE is_read = 0"
                    Using cmd As New MySqlCommand(query, conn)
                        count = Convert.ToInt32(cmd.ExecuteScalar())
                    End Using
                End Using
            Catch ex As Exception
                count = 0
            End Try

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

        Private Sub ShowNotifications(sender As Object, e As RoutedEventArgs)
            RaiseEvent RestoreDefaultSidebar()

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "UPDATE employeeloginhistory SET is_read = 1 WHERE is_read = 0"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
            Catch ex As Exception
            End Try

            LoadNotificationBadge()

            Dim notifModal As New DPC.Components.ConfirmationModals.NotificationModal(GlobalVariables.CacheOnEmployeeID)
            notifModal.ShowDialog()
        End Sub

        ' ── CHATBOT BUTTON (Email icon) ────────────────────────────
        ' ── CHATBOT BUTTON (Email icon) ────────────────────────────
        Private Sub ShowMessages(sender As Object, e As RoutedEventArgs)
            RaiseEvent RestoreDefaultSidebar()

            ' 1. Create a fresh instance if none exists or the window was closed
            If _chatBot Is Nothing OrElse Not _chatBot.IsLoaded Then
                _chatBot = New ChatBotWindow()
                _chatBot.Owner = Window.GetWindow(Me)

                ' 2. FIX: Assign the real username here.
                ' If you have a variable like 'CurrentEmployeeName' or 'CacheOnEmployeeID', use it.
                ' For testing purposes, you can use:
                ' _chatBot.CurrentLoggedInUser = Environment.MachineName ' Uses the PC name (Laptop vs Desktop)

                ' Ideally, use your app's logged-in variable:
                _chatBot.CurrentLoggedInUser = GlobalVariables.CurrentUserName ' Replace with your actual username variable
            End If

            If _chatBot.IsVisible Then
                ' Second click hides it (toggle behaviour)
                _chatBot.Hide()
            Else
                _chatBot.PositionNearNavBar(Window.GetWindow(Me))
                _chatBot.Show()
                _chatBot.Activate()
            End If
        End Sub

        Private Sub ToggleClockInOut(sender As Object, e As RoutedEventArgs)
            RaiseEvent RestoreDefaultSidebar()
            If ClockIcon.Kind = PackIconKind.ClockOutline Then
                ClockIcon.Kind = PackIconKind.Clock
                MessageBox.Show("Clocked In!")
            Else
                ClockIcon.Kind = PackIconKind.ClockOutline
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