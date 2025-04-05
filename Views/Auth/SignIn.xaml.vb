Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Controllers
Imports System.Windows

Namespace DPC.Views.Auth
    Public Class SignIn

        Inherits UserControl

        ' Fields for password handling
        Private passwordHidden As Boolean = True
        Private realPassword As String = ""

        Public Sub New()
            InitializeComponent()
        End Sub


        ' Handle Sign-In Process
        Private Sub BtnSignIn_Click(sender As Object, e As RoutedEventArgs)
            Dim username As String = txtUsername.Text.Trim()
            Dim password As String = realPassword

            ' Check if fields are empty
            If String.IsNullOrWhiteSpace(username) OrElse String.IsNullOrWhiteSpace(password) Then
                MessageBox.Show("Please enter both username and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            ' Authenticate user
            Dim authResult As (String, String) = AuthController.SignIn(username, password)
            Dim accessToken As String = authResult.Item1
            Dim refreshToken As String = authResult.Item2

            If Not String.IsNullOrEmpty(accessToken) AndAlso Not String.IsNullOrEmpty(refreshToken) Then
                MessageBox.Show("Login Successful!", "Welcome", MessageBoxButton.OK, MessageBoxImage.Information)

                ' Store tokens for session
                SessionManager.SetSessionTokens(accessToken, refreshToken)

                ' Redirect to Base.xaml and load Dashboard view
                Dim baseWindow As New Base With {
                    .CurrentView = DynamicView.Load("dashboard") ' Set CurrentView to Dashboard
                    } ' Create instance of Base.xaml

                ' Show the Base window
                baseWindow.Show()

                ' Close the current window (SignIn window)
                Dim currentWindow As Window = Window.GetWindow(Me)
                currentWindow?.Close()
            Else
                MessageBox.Show("Invalid username or password. Please try again.", "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
        End Sub


        ' Handle text input correctly while masking
        Private Sub TxtPassword_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            realPassword &= e.Text ' Append typed character to realPassword
            txtPassword.Text = New String("●"c, realPassword.Length) ' Mask the password
            txtPassword.CaretIndex = txtPassword.Text.Length ' Move cursor to end
            e.Handled = True ' Prevent actual text from appearing
        End Sub

        ' Handle backspace key
        Private Sub TxtPassword_KeyDown(sender As Object, e As KeyEventArgs)
            If e.Key = Key.Back AndAlso realPassword.Length > 0 Then
                realPassword = realPassword.Substring(0, realPassword.Length - 1)
                txtPassword.Text = New String("●"c, realPassword.Length)
                txtPassword.CaretIndex = txtPassword.Text.Length
                e.Handled = True
            End If
        End Sub

        ' Handle TextChanged for any external modifications (e.g., clearing the field)
        Private Sub TxtPassword_TextChanged(sender As Object, e As TextChangedEventArgs)
            If String.IsNullOrEmpty(txtPassword.Text) Then
                realPassword = "" ' Reset real password if input is cleared
            End If
        End Sub


        Private Sub ForgotPassword_Click(sender As Object, e As MouseButtonEventArgs)
            ' Navigate to Forgot Password screen
            Dim forgotPasswordView As New ForgotPassword()
            Dim parentWindow As Window = Window.GetWindow(Me)

            ' If inside a window, switch to ForgotPassword view
            If TypeOf parentWindow Is MainWindow Then
                Dim mainWin As MainWindow = CType(parentWindow, MainWindow)
                mainWin.CurrentViewIndex = 1 ' Switch to Forgot Password View
            End If
        End Sub


        ' Toggle password visibility
        Private Sub BtnTogglePassword_Click(sender As Object, e As RoutedEventArgs)
            passwordHidden = Not passwordHidden

            If passwordHidden Then
                txtPassword.Text = New String("●"c, realPassword.Length)
                iconPasswordToggle.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOffOutline
            Else
                txtPassword.Text = realPassword
                iconPasswordToggle.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOutline
            End If

            txtPassword.CaretIndex = txtPassword.Text.Length
        End Sub

    End Class
End Namespace