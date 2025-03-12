Imports System.Windows
Imports System.Windows.Input

Namespace DPC
    Partial Public Class MainWindow
        Public Sub New()
            InitializeComponent()
        End Sub

        ' Switch to Sign-Up panel
        Private Sub SwitchToSignUp(sender As Object, e As MouseButtonEventArgs)
            SignInPanel.Visibility = Visibility.Hidden
            SignUpPanel.Visibility = Visibility.Visible
        End Sub

        ' Switch to Sign-In panel
        Private Sub SwitchToSignIn()
            SignUpPanel.Visibility = Visibility.Hidden
            SignInPanel.Visibility = Visibility.Visible
        End Sub


        ' Sign-In button click event

        Private Sub BtnSignIn_Click(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Welcome: " & txtEmail.Text)
            ' Redirect to Dashboard.xaml
            Dim dashboard As New Views.Dashboard.Dashboard()
            dashboard.Show()
            Me.Close()
        End Sub

        ' Sign-Up button click event
        Private Sub BtnSignUp_Click(sender As Object, e As RoutedEventArgs)
            If txtSignUpPassword.Password <> txtConfirmPassword.Password Then
                MessageBox.Show("Passwords do not match!")
                Return
            End If
            MessageBox.Show("Sign-Up Clicked: " & txtSignUpEmail.Text)
        End Sub
        ' Close the application when the close button is clicked
        Private Sub CloseApp_Click(sender As Object, e As RoutedEventArgs)
            Me.Close()
        End Sub

    End Class
End Namespace
