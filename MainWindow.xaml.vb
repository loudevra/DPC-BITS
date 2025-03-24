Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Media.Animation
Imports System.Windows.Media.Effects
Imports DPC.DPC.Data.Converters
Imports DPC.DPC.Data.Controllers
Imports System.Web.SessionState
Imports DPC.DPC.Views.Auth

Namespace DPC
    Partial Public Class MainWindow
        Public Sub New()
            InitializeComponent()
            PasswordMaskingBehavior.SetEnablePasswordMasking(txtPassword, True)
        End Sub

        ' Sign-In button click event Uncomment to Enable Authentication

        Private Sub BtnSignIn_Click(sender As Object, e As RoutedEventArgs)
            'Dim username As String = txtEmail.Text.Trim()
            'Dim password As String = txtPassword.Text.Trim()

            '' Check if fields are empty
            'If String.IsNullOrWhiteSpace(username) OrElse String.IsNullOrWhiteSpace(password) Then
            '    MessageBox.Show("Please enter both username and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            '    Return
            'End If

            '' Authenticate user
            'Dim authResult As (String, String) = AuthController.SignIn(username, password)
            'Dim accessToken As String = authResult.Item1
            'Dim refreshToken As String = authResult.Item2

            'If Not String.IsNullOrEmpty(accessToken) AndAlso Not String.IsNullOrEmpty(refreshToken) Then
            'MessageBox.Show("Login Successful!", "Welcome", MessageBoxButton.OK, MessageBoxImage.Information)

            '    ' Store tokens for session
            'SessionManager.SetSessionTokens(accessToken, refreshToken)

            ' Redirect to Dashboard.xaml
            Dim dashboard As New Views.Dashboard.Dashboard()
                dashboard.Show()
                Me.Close()
            'Else
            '    MessageBox.Show("Invalid username or password. Please try again.", "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Warning)
            'End If
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

        Private Sub ForgotPassword_Click(sender As Object, e As MouseButtonEventArgs)
            Dim forgotPasswordWindow As New ForgotPassword()
            forgotPasswordWindow.Show()
            Me.Close() ' Close Sign-In window
        End Sub

    End Class
End Namespace
