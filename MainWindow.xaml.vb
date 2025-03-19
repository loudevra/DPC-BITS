Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Media.Animation
Imports System.Windows.Media.Effects
Imports DPC.DPC.Data.Converters

Namespace DPC
    Partial Public Class MainWindow
        Public Sub New()
            InitializeComponent()
            PasswordMaskingBehavior.SetEnablePasswordMasking(txtPassword, True)
        End Sub

        ' Sign-In button click event

        Private Sub BtnSignIn_Click(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Welcome: " & txtEmail.Text)
            ' Redirect to Dashboard.xaml
            Dim dashboard As New Views.Dashboard.Dashboard()
            dashboard.Show()
            Me.Close()
        End Sub

        Private Sub BtnExit_Click(sender As Object, e As RoutedEventArgs)
            Application.Current.Shutdown()
        End Sub

        ' Animate the Logo with Slow RGB Glow
        Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            Dim colorAnimation As New ColorAnimation With {
                .From = Colors.Red,
                .To = Colors.Blue,
                .Duration = New Duration(TimeSpan.FromSeconds(4)),
                .AutoReverse = True,
                .RepeatBehavior = RepeatBehavior.Forever
            }

            Dim glowEffect As DropShadowEffect = CType(LogoImage.Effect, DropShadowEffect)
            glowEffect.BeginAnimation(DropShadowEffect.ColorProperty, colorAnimation)
        End Sub

    End Class
End Namespace
