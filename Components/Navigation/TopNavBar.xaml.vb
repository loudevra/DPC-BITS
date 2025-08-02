Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports DPC.DPC.Data.Converters.ValueConverter
Imports DPC.DPC.Data.Helpers
Imports MaterialDesignThemes.Wpf

Namespace DPC.Components.Navigation
    Partial Public Class TopNavBar
        ' Events for navigation
        Public Event NavigateToPOS()
        Public Event RestoreDefaultSidebar()

        Public Sub New()
            InitializeComponent()
            ApplyHalfWidthConverter()
        End Sub

        Private Sub ApplyHalfWidthConverter()
            Dim binding As New Binding("ActualWidth") With {
                .Source = Me,
                .Converter = New HalfWidthConverter(),
                .Mode = BindingMode.OneWay
            }
            SearchBar.SetBinding(Grid.MaxWidthProperty, binding)
        End Sub

        ' Open POS - Modified to load POS form in sidebar
        Private Sub OpenPOS(sender As Object, e As RoutedEventArgs)
            ' Raise event to notify Base class to change the sidebar
            RaiseEvent NavigateToPOS()
        End Sub

        ' Change Business Location
        Private Sub ChangeLocation(sender As Object, e As RoutedEventArgs)
            ' Restore default sidebar if POS is open
            RaiseEvent RestoreDefaultSidebar()
            MessageBox.Show("Changing business location...")
        End Sub

        ' Search Customer
        Private Sub SearchCustomer(sender As Object, e As RoutedEventArgs)
            ' Restore default sidebar if POS is open
            RaiseEvent RestoreDefaultSidebar()
            Dim searchQuery As String = SearchBar.Text
            MessageBox.Show($"Searching for: {searchQuery}")
        End Sub

        ' Show Notifications
        Private Sub ShowNotifications(sender As Object, e As RoutedEventArgs)
            ' Restore default sidebar if POS is open
            RaiseEvent RestoreDefaultSidebar()
            MessageBox.Show("Showing notifications...")
        End Sub

        ' Show Messages
        Private Sub ShowMessages(sender As Object, e As RoutedEventArgs)
            ' Restore default sidebar if POS is open
            RaiseEvent RestoreDefaultSidebar()
            MessageBox.Show("Showing messages...")
        End Sub

        ' Toggle Clock In/Out
        Private Sub ToggleClockInOut(sender As Object, e As RoutedEventArgs)
            ' Restore default sidebar if POS is open
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
                    ' Optional: Change icon to maximize when restored
                    Maximizebtn.Kind = PackIconKind.WindowMaximize
                Else
                    parentWindow.WindowState = WindowState.Maximized
                    ' Optional: Change icon to restore when maximized
                    Maximizebtn.Kind = PackIconKind.WindowRestore
                End If
            End If
        End Sub
    End Class
End Namespace