Imports DPC.DPC.Data.Helpers ' Corrected namespace for EnvLoader

Class Application
    Protected Overrides Sub OnStartup(e As StartupEventArgs)
        MyBase.OnStartup(e)

        ' Load environment variables from .env file
        Try
            EnvLoader.LoadEnv()
        Catch ex As Exception
            MessageBox.Show("Error loading environment variables: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            Current.Shutdown() ' Shutdown the application if .env fails to load
        End Try
    End Sub
End Class

