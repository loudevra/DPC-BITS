Imports MySql.Data.MySqlClient
Imports System.Threading.Tasks
Imports System.Windows.Media.Animation
Imports System.Windows.Media
Imports System.Windows.Media.Effects

Namespace DPC
    Public Class SplashScreen
        Public Shared Property DBConnection As MySqlConnection

        Private Async Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            StartRGBAnimation()
            Await InitializeApplicationAsync()
            OpenMainWindow()
        End Sub

        ' Start RGB Animation
        Private Sub StartRGBAnimation()
            Dim colorAnimation As New ColorAnimationUsingKeyFrames With {
                .Duration = New Duration(TimeSpan.FromSeconds(3)),
                .RepeatBehavior = RepeatBehavior.Forever
            }

            ' Define RGB Color Cycle (Red -> Green -> Blue -> Red)
            colorAnimation.KeyFrames.Add(New LinearColorKeyFrame(Colors.Red, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))))
            colorAnimation.KeyFrames.Add(New LinearColorKeyFrame(Colors.Green, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1))))
            colorAnimation.KeyFrames.Add(New LinearColorKeyFrame(Colors.Blue, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2))))
            colorAnimation.KeyFrames.Add(New LinearColorKeyFrame(Colors.Red, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3))))

            ' Apply animation to Glow Effect
            Dim glowEffect As New DropShadowEffect With {
                .Color = Colors.Red,
                .BlurRadius = 120,
                .ShadowDepth = 0
            }
            glowEffect = glowEffect
            glowEffect.BeginAnimation(DropShadowEffect.ColorProperty, colorAnimation)

            ' Apply animation to Logo Glow
            Dim logoEffect As New DropShadowEffect With {
                .Color = Colors.Red,
                .BlurRadius = 250,
                .ShadowDepth = 0
            }
            LogoImage.Effect = logoEffect
            logoEffect.BeginAnimation(DropShadowEffect.ColorProperty, colorAnimation)
        End Sub

        ' Initialize Database Connection
        Private Async Function InitializeApplicationAsync() As Task
            Try
                InitializeDatabase()
                Await Task.Delay(6000) ' Simulated Load Time
            Catch ex As Exception
                MessageBox.Show("Error initializing application: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Function

        ' Open Connection and Store It
        Private Sub InitializeDatabase()
            Try
                If DBConnection Is Nothing Then
                    DBConnection = New MySqlConnection("server=localhost;userid=root;password=;database=dpc;")
                    DBConnection.Open()
                End If
            Catch ex As Exception
                MessageBox.Show("Database connection failed: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Open Main Window and Close Splash Screen
        Private Sub OpenMainWindow()
            ' Check if MainWindow is already open
            For Each window As Window In Application.Current.Windows
                If TypeOf window Is MainWindow Then
                    Me.Close()
                    Return ' Exit the method if MainWindow is already open
                End If
            Next

            ' Otherwise, create a new instance
            Dim mainWindow As New MainWindow()
            mainWindow.Show()
            Me.Close()
        End Sub
    End Class
End Namespace
