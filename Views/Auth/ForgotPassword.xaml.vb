Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Controls
Imports DPC.DPC.Data.Controllers
Imports NuGet.Common

Namespace DPC.Views.Auth
    Partial Public Class ForgotPassword
        Inherits UserControl
        Implements INotifyPropertyChanged

        ' Constructor
        Public Sub New()
            InitializeComponent()
        End Sub

        ' Event to switch back to Sign-In
        Public Event BackToSignIn As EventHandler

        ' Property Changed Event (for future data binding)
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Private Sub RaisePropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub

        ' Send verification code logic
        Private Sub BtnSendCode_Click(sender As Object, e As RoutedEventArgs)
            Dim email As String = txtEmail.Text.Trim()

            If String.IsNullOrWhiteSpace(email) Then
                ShowMessage("Email is required.")
                Return
            End If

            If Not ResetPassController.DoesEmailExist(email) Then
                ShowMessage("Email not found.")
                Return
            End If

            Dim verificationCode As String = ResetPassController.GenerateAndStoreVerificationCode(email)
            If String.IsNullOrEmpty(verificationCode) Then
                ShowMessage("Failed to send verification code. Try again.")
            Else
                ' Switch to VerifyCode view and pass the email
                Dim parentWindow As MainWindow = CType(Window.GetWindow(Me), MainWindow)
                parentWindow.LoadView(2, email)
            End If
        End Sub

        Private Sub ShowMessage(message As String)
            lblMessage.Text = message
            lblMessage.Visibility = Visibility.Visible
        End Sub

        Private Sub BackToSignIn_Click(sender As Object, e As MouseButtonEventArgs)
            RaiseEvent BackToSignIn(Me, EventArgs.Empty)
        End Sub

    End Class
End Namespace
