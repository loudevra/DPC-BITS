Imports System.Windows
Imports System.ComponentModel
Imports DPC.DPC.Views.Auth ' Ensure SignIn and ForgotPassword are accessible
Imports System.Windows.Input
Imports System.Windows.Media.Animation
Imports System.Windows.Media.Effects

Namespace DPC
    Partial Public Class MainWindow
        Implements INotifyPropertyChanged

        ' View index management
        Private _currentViewIndex As Integer = 0
        Public Property CurrentViewIndex As Integer
            Get
                Return _currentViewIndex
            End Get
            Set(value As Integer)
                _currentViewIndex = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(CurrentViewIndex)))
                LoadView(_currentViewIndex) ' Load the selected view
            End Set
        End Property

        ' INotifyPropertyChanged implementation
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

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

        ' Constructor
        Public Sub New()
            InitializeComponent()
            LoadView(0) ' Load SignIn by default
        End Sub

        ' Load views dynamically based on the index
        ' Change from Private to Public
        Public Sub LoadView(viewIndex As Integer, Optional userEmail As String = "")
            Select Case viewIndex
                Case 0
                    RightPanelContent.Content = New SignIn()
                Case 1
                    Dim forgotPasswordView As New ForgotPassword()
                    AddHandler forgotPasswordView.BackToSignIn, AddressOf BackToSignInHandler
                    RightPanelContent.Content = forgotPasswordView
                Case 2
                    RightPanelContent.Content = New VerifyCode(userEmail)
                Case 3
                    RightPanelContent.Content = New ResetPassword(userEmail)
            End Select
        End Sub


        ' Close the application
        Private Sub BtnExit_Click(sender As Object, e As RoutedEventArgs)
            Application.Current.Shutdown()
        End Sub

        ' Event handler to go back to SignIn
        Private Sub BackToSignInHandler(sender As Object, e As EventArgs)
            CurrentViewIndex = 0 ' Switch back to SignIn
        End Sub
    End Class
End Namespace
