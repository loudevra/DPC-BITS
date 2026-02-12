Imports MongoDB.Bson
Imports MongoDB.Driver
Imports MySql.Data.MySqlClient


Namespace DPC.Components.UI
    Public Class PopUpOptionFolder
        Private ReadOnly collectionName As String = "media_files"
        Public Shared Event FolderDeletedSuccessfully()

        Private Async Sub DeleteFolder_Click(sender As Object, e As RoutedEventArgs)

            Try
                Dim folderId As Long = CLng(Me.Tag)

                Dim database As IMongoDatabase = SplashScreen.GetMongoDatabaseConnection()
                Dim collection As IMongoCollection(Of BsonDocument) = database.GetCollection(Of BsonDocument)(collectionName)

                Dim filter = Builders(Of BsonDocument).Filter.Eq(Of Long)("_folderId", folderId)
                Dim fileCount = Await collection.CountDocumentsAsync(filter)

                If fileCount > 0 Then
                    PopupHelper.ClosePopup()
                    MessageBox.Show($"Cannot delete. This folder contains {fileCount} file(s).",
                                    "Folder Busy", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                Dim result = MessageBox.Show("Are you sure you want to delete this empty folder?",
                                     "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question)

                If result = MessageBoxResult.Yes Then
                    DeleteFolderFromDB(folderId)
                    PopupHelper.ClosePopup()

                    RaiseEvent FolderDeletedSuccessfully()
                End If
            Catch ex As Exception

            End Try
        End Sub

        Private Sub DeleteFolderFromDB(folderId)
            Using conn = SplashScreen.GetDatabaseConnection()
                conn.Open()
                Dim sql = "DELETE FROM folders WHERE id = @id"
                Using cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@id", folderId)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub
    End Class
End Namespace
