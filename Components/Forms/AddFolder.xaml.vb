Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Views.DataReports.UploadFileOnline
Imports MySql.Data.MySqlClient

Namespace DPC.Components.Forms
    Public Class AddFolder

        Public Event AddFolder()

        Public Sub New()
            InitializeComponent()
        End Sub


        Private Sub btnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
            Try
                ' 1. Your SQL Insert logic here
                SaveFolderToDb(txtFolderName.Text, txtDescription.Text)

                ' 2. ✅ Trigger the event to tell the main page to refresh
                RaiseEvent AddFolder()

            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End Sub

        Private Sub SaveFolderToDb(name As String, desc As String)
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

            Return
        End Sub
    End Class
End Namespace
