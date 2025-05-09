Imports System.IO
Imports System.Text
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports MaterialDesignThemes.Wpf
Imports Microsoft.Win32

Namespace DPC.Views.POS
    Public Class POSForm
        Inherits UserControl

        Public Sub New()
            InitializeComponent()

            'Opens POS Settings on startup
            OpenSettings(Nothing, Nothing)
        End Sub

        Private Sub OpenSettings(sender As Object, e As RoutedEventArgs)
            If StackPanelSettings.Visibility = Visibility.Visible Then

                SettingsIcon.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                SettingsText.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                StackPanelSettings.Visibility = Visibility.Collapsed
                GridSettings.Visibility = Visibility.Collapsed
                StackPanelCoupon.Visibility = Visibility.Collapsed
            Else

                SettingsIcon.Foreground = New BrushConverter().ConvertFromString("#555555")
                SettingsText.Foreground = New BrushConverter().ConvertFromString("#555555")

                InvoiceIcon.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                InvoiceText.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                CouponIcon.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                CouponText.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                StackPanelSettings.Visibility = Visibility.Visible
                GridSettings.Visibility = Visibility.Collapsed
                StackPanelCoupon.Visibility = Visibility.Collapsed
            End If

        End Sub

        Private Sub OpenProperties(sender As Object, e As RoutedEventArgs)
            If GridSettings.Visibility = Visibility.Visible Then

                InvoiceIcon.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                InvoiceText.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                StackPanelSettings.Visibility = Visibility.Collapsed
                GridSettings.Visibility = Visibility.Collapsed
                StackPanelCoupon.Visibility = Visibility.Collapsed
            Else

                SettingsIcon.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                SettingsText.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                InvoiceIcon.Foreground = New BrushConverter().ConvertFromString("#555555")
                InvoiceText.Foreground = New BrushConverter().ConvertFromString("#555555")

                CouponIcon.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                CouponText.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                StackPanelSettings.Visibility = Visibility.Collapsed
                GridSettings.Visibility = Visibility.Visible
                StackPanelCoupon.Visibility = Visibility.Collapsed
            End If
        End Sub

        Private Sub OpenCoupon(sender As Object, e As RoutedEventArgs)
            If StackPanelCoupon.Visibility = Visibility.Visible Then

                CouponIcon.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                CouponText.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                StackPanelSettings.Visibility = Visibility.Collapsed
                GridSettings.Visibility = Visibility.Collapsed
                StackPanelCoupon.Visibility = Visibility.Collapsed
            Else

                SettingsIcon.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                SettingsText.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                InvoiceIcon.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                InvoiceText.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                CouponIcon.Foreground = New BrushConverter().ConvertFromString("#555555")
                CouponText.Foreground = New BrushConverter().ConvertFromString("#555555")

                StackPanelSettings.Visibility = Visibility.Collapsed
                GridSettings.Visibility = Visibility.Collapsed
                StackPanelCoupon.Visibility = Visibility.Visible
            End If
        End Sub
    End Class
End Namespace