Imports MySql.Data.MySqlClient
Imports System.Threading.Tasks
Imports System.Windows.Media.Animation
Imports System.Windows.Media
Imports System.Windows.Media.Effects

Namespace DPC
    Public Class SplashScreen
        ' Store Connection String (Enable Connection Pooling)
        Private Shared ReadOnly ConnectionString As String = "server=localhost;userid=root;password=;database=dpc;Pooling=True;Min Pool Size=5;Max Pool Size=100;"

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

        ' Initialize Database Connection (Only Test It)
        Private Async Function InitializeApplicationAsync() As Task
            Try
                Await TestDatabaseConnectionAsync() ' Only test connection, do not store it
                Await Task.Delay(6000) ' Simulated Load Time
            Catch ex As Exception
                MessageBox.Show("Error initializing application: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Function

        ' Test if Database Connection Works (DO NOT STORE CONNECTION)
        Private Async Function TestDatabaseConnectionAsync() As Task
            Try
                Using connection As New MySqlConnection(ConnectionString)
                    Await connection.OpenAsync()
                End Using
            Catch ex As Exception
                Throw New Exception("Database connection failed: " & ex.Message)
            End Try
        End Function

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

        ' Public Function to Get Database Connection
        Public Shared Function GetDatabaseConnection() As MySqlConnection
            Return New MySqlConnection(ConnectionString) ' Returns a pooled connection
        End Function
    End Class
End Namespace
