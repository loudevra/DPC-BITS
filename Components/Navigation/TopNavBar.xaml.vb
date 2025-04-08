Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports DPC.DPC.Data.Converters.ValueConverter

Namespace DPC.Components.Navigation
    Partial Public Class TopNavBar
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

        ' Open POS
        Private Sub OpenPOS(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigating to POS...")
        End Sub

        ' Change Business Location
        Private Sub ChangeLocation(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Changing business location...")
        End Sub

        ' Search Customer
        Private Sub SearchCustomer(sender As Object, e As RoutedEventArgs)
            Dim searchQuery As String = SearchBar.Text
            MessageBox.Show($"Searching for: {searchQuery}")
        End Sub

        ' Show Notifications
        Private Sub ShowNotifications(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Showing notifications...")
        End Sub

        ' Show Messages
        Private Sub ShowMessages(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Showing messages...")
        End Sub

        ' Toggle Clock In/Out
        Private Sub ToggleClockInOut(sender As Object, e As RoutedEventArgs)
            If ClockIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ClockOutline Then
                ClockIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Clock
                MessageBox.Show("Clocked In!")
            Else
                ClockIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ClockOutline
                MessageBox.Show("Clocked Out!")
            End If
        End Sub


    End Class
End Namespace
