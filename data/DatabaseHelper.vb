Imports MySql.Data.MySqlClient
Imports System.Data

Namespace DPC.Data
    Public Class DatabaseHelper
        Private Shared ReadOnly ConnectionString As String = "server=localhost;userid=root;password=;database=dpc;SslMode=none;"
        Private Shared _connection As MySqlConnection

        ' Open Connection Once During Splash Screen Initialization
        Public Shared Sub Initialize()
            Try
                If _connection Is Nothing Then
                    _connection = New MySqlConnection(ConnectionString)
                End If
                If _connection.State = ConnectionState.Closed Then
                    _connection.Open()
                End If
            Catch ex As Exception
                MessageBox.Show("Database connection failed: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Get the Open Connection
        Public Shared Function GetConnection() As MySqlConnection
            If _connection Is Nothing Then
                _connection = New MySqlConnection(ConnectionString)
            End If
            If _connection.State = ConnectionState.Closed Then
                _connection.Open()
            End If
            Return _connection
        End Function

        ' Close Connection (Only on Application Exit)
        Public Shared Sub CloseConnection()
            If _connection IsNot Nothing AndAlso _connection.State = ConnectionState.Open Then
                _connection.Close()
            End If
        End Sub
    End Class
End Namespace
