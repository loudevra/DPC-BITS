Imports System.Threading.Tasks
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Media.Effects
Imports DPC.DPC.Data.Helpers
Imports MongoDB.Driver
Imports MongoDB.Driver.GridFS
Imports MySql.Data.MySqlClient

Namespace DPC
    Public Class SplashScreen
        ' Store Connection String (Enable Connection Pooling)
        Private Shared ReadOnly ConnectionString As String =
            $"server={EnvLoader.GetEnv("DB_HOST")};" &
            $"userid={EnvLoader.GetEnv("DB_USER")};" &
            $"password={EnvLoader.GetEnv("DB_PASS")};" &
            $"port={EnvLoader.GetEnv("DB_PORT")};" &
            $"database={EnvLoader.GetEnv("DB_NAME")};" &
            $"Pooling=True;" &
            $"Min Pool Size={EnvLoader.GetEnv("DB_POOL_MIN")};" &
            $"Max Pool Size={EnvLoader.GetEnv("DB_POOL_MAX")};"

        Private Async Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
            ' Load environment variables
            Try
                EnvLoader.LoadEnv()
            Catch ex As Exception
                MessageBox.Show("Failed to load environment variables: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Application.Current.Shutdown()
            End Try

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

        Public Shared Function GetMongoDatabaseConnection() As IMongoDatabase
            Dim settings As New MongoClientSettings With {
                .Server = New MongoServerAddress(EnvLoader.GetEnv("MDB_HOST"), EnvLoader.GetEnv("MDB_PORT")),
                .Credential = MongoCredential.CreateCredential(EnvLoader.GetEnv("MDB_NAME"), EnvLoader.GetEnv("MDB_USER"), EnvLoader.GetEnv("MDB_PASS"))
            }

            Dim client As New MongoClient(settings)
            Return client.GetDatabase($"{EnvLoader.GetEnv("MDB_GETDB")}") 
        End Function

        Public Shared Function GetGridFSConnection() As GridFSBucket
            Return New GridFSBucket(GetMongoDatabaseConnection())
        End Function

    End Class
End Namespace
