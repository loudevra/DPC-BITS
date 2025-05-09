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
Imports NuGet.Protocol.Plugins

Namespace DPC.Views.POS
    Public Class POSForm
        Inherits UserControl

        Public Sub New()
            InitializeComponent()

            'Opens POS Settings on startup
            OpenSettings(Nothing, Nothing)
        End Sub

        'This function is used to open and close the content properties of the POS form
        Private Sub OpenContent(sender As Object, e As RoutedEventArgs,
                                OpenStackPanel As StackPanel, OpenIcon As PackIcon, OpenText As TextBlock,
                                CloseStackPanel As StackPanel, CloseIcon As PackIcon, CloseText As TextBlock,
                                CloseStackPanel2 As StackPanel, CloseIcon2 As PackIcon, CloseText2 As TextBlock)
            If OpenStackPanel.Visibility = Visibility.Visible Then
                OpenIcon.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                OpenText.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                OpenStackPanel.Visibility = Visibility.Collapsed
                CloseStackPanel.Visibility = Visibility.Collapsed
                CloseStackPanel2.Visibility = Visibility.Collapsed
            Else
                OpenIcon.Foreground = New BrushConverter().ConvertFromString("#555555")
                OpenText.Foreground = New BrushConverter().ConvertFromString("#555555")

                CloseIcon.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                CloseText.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                CloseIcon2.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")
                CloseText2.Foreground = New BrushConverter().ConvertFromString("#AEAEAE")

                OpenStackPanel.Visibility = Visibility.Visible
                CloseStackPanel.Visibility = Visibility.Collapsed
                CloseStackPanel2.Visibility = Visibility.Collapsed
            End If

        End Sub
        Private Sub OpenSettings(sender As Object, e As RoutedEventArgs)
            OpenContent(sender, e,
                        StackPanelSettings, SettingsIcon, SettingsText,
                        StackPanelInvoice, InvoiceIcon, InvoiceText,
                        StackPanelCoupon, CouponIcon, CouponText)
        End Sub
        Private Sub OpenProperties(sender As Object, e As RoutedEventArgs)
            OpenContent(sender, e,
            StackPanelInvoice, InvoiceIcon, InvoiceText,
            StackPanelSettings, SettingsIcon, SettingsText,
            StackPanelCoupon, CouponIcon, CouponText)
        End Sub
        Private Sub OpenCoupon(sender As Object, e As RoutedEventArgs)
            OpenContent(sender, e,
            StackPanelCoupon, CouponIcon, CouponText,
            StackPanelSettings, SettingsIcon, SettingsText,
            StackPanelInvoice, InvoiceIcon, InvoiceText)
        End Sub
    End Class
End Namespace