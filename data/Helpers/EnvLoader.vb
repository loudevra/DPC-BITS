Imports System.IO

Namespace DPC.Data.Helpers
    Public Class EnvLoader
        Private Shared ReadOnly envVariables As New Dictionary(Of String, String)

        ''' <summary>
        ''' Loads environment variables from the .env file.
        ''' </summary>
        Public Shared Sub LoadEnv()
            ' Ensure the .env file is in the correct location
            Dim envFilePath As String = Path.Combine(Directory.GetCurrentDirectory(), ".env")

            ' If the file is not found in the execution directory, check the application base directory
            If Not File.Exists(envFilePath) Then
                envFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env")
            End If

            ' Final check before loading
            If File.Exists(envFilePath) Then
                Dim lines() As String = File.ReadAllLines(envFilePath)
                For Each line As String In lines
                    ' Ignore empty lines and comments
                    If Not String.IsNullOrWhiteSpace(line) AndAlso Not line.Trim().StartsWith("#") Then
                        Dim parts() As String = line.Split({"="}, 2, StringSplitOptions.None)
                        If parts.Length = 2 Then
                            Dim key As String = parts(0).Trim()
                            Dim value As String = parts(1).Trim()
                            envVariables(key) = value
                        End If
                    End If
                Next
            Else
                Throw New FileNotFoundException("Missing .env file. Ensure it's in the project root and set to 'Copy to Output Directory'.")
            End If
        End Sub

        ''' <summary>
        ''' Retrieves the value of an environment variable.
        ''' </summary>
        ''' <param name="key">The environment variable key.</param>
        ''' <returns>The value or an empty string if not found.</returns>
        Public Shared Function GetEnv(key As String) As String
            Return If(envVariables.ContainsKey(key), envVariables(key), String.Empty)
        End Function
    End Class
End Namespace