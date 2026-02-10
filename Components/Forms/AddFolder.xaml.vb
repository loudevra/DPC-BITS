Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Views.DataReports.UploadFileOnline
Imports MySql.Data.MySqlClient

Namespace DPC.Components.Forms
    Public Class AddFolder

        Public Property FolderName As String
        Public Property FolderDescription As String

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub AddFolder(name As String, desc As String)
            Try
                Using DatabaseConn = SplashScreen.GetDatabaseConnection()
                    DatabaseConn.Open()
                    Dim query As String = "INSERT INTO folders (name, description, created_at) VALUES (@name, @desc, @created)"
                    Using cmd As New MySqlCommand(query, DatabaseConn)
                        cmd.Parameters.AddWithValue("@name", name)
                        cmd.Parameters.AddWithValue("@desc", desc)
                        cmd.Parameters.AddWithValue("@created", DateTime.Now)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using
            Catch ex As Exception
                Throw New Exception("Database Insert Failed: " & ex.Message)
            End Try
        End Sub
    End Class
End Namespace
