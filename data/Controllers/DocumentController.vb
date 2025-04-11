Imports System.Data
Imports System.IO
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Models
Imports System.Collections.ObjectModel
Imports System.Convert

Namespace DPC.Data.Controllers
    Public Class DocumentController

        ' Get all documents for a specific employee
        Public Function GetDocumentsByEmployeeID(employeeID As Integer) As ObservableCollection(Of Document)
            Dim documents As New ObservableCollection(Of Document)()

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                conn.Open()

                Dim query As String = "SELECT * FROM documents WHERE EmployeeID = @EmployeeID ORDER BY UploadDate DESC"
                Using command As New MySqlCommand(query, conn)
                    command.Parameters.AddWithValue("@EmployeeID", employeeID)

                    Using reader As MySqlDataReader = command.ExecuteReader()
                        While reader.Read()
                            Dim document As New Document() With {
                                .DocumentID = Convert.ToInt32(reader("DocumentID")),
                                .EmployeeID = Convert.ToInt32(reader("EmployeeID")),
                                .Title = reader("Title").ToString(),
                                .FileName = reader("FileName").ToString(),
                                .FileType = reader("FileType").ToString(),
                                .FileSize = Convert.ToInt64(reader("FileSize")),
                                .UploadDate = Convert.ToDateTime(reader("UploadDate"))
                            }
                            documents.Add(document)
                        End While
                    End Using
                End Using
            End Using

            Return documents
        End Function

        ' Get document by ID (including file content)
        Public Function GetDocumentByID(documentID As Integer, employeeID As Integer) As Document
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                conn.Open()

                Dim query As String = "SELECT * FROM documents WHERE DocumentID = @DocumentID AND EmployeeID = @EmployeeID"
                Using command As New MySqlCommand(query, conn)
                    command.Parameters.AddWithValue("@DocumentID", documentID)
                    command.Parameters.AddWithValue("@EmployeeID", employeeID)

                    Using reader As MySqlDataReader = command.ExecuteReader()
                        If reader.Read() Then
                            Dim document As New Document() With {
                                .DocumentID = Convert.ToInt32(reader("DocumentID")),
                                .EmployeeID = Convert.ToInt32(reader("EmployeeID")),
                                .Title = reader("Title").ToString(),
                                .FileName = reader("FileName").ToString(),
                                .FileContent = reader("FileContent").ToString(),
                                .FileType = reader("FileType").ToString(),
                                .FileSize = Convert.ToInt64(reader("FileSize")),
                                .UploadDate = Convert.ToDateTime(reader("UploadDate"))
                            }
                            Return document
                        End If
                    End Using
                End Using
            End Using

            Return Nothing
        End Function

        ' Add a new document
        Public Function AddDocument(document As Document) As Boolean
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                conn.Open()

                Dim query As String = "INSERT INTO documents (EmployeeID, Title, FileName, FileContent, FileType, FileSize, UploadDate) " &
                                    "VALUES (@EmployeeID, @Title, @FileName, @FileContent, @FileType, @FileSize, @UploadDate)"

                Using command As New MySqlCommand(query, conn)
                    command.Parameters.AddWithValue("@EmployeeID", document.EmployeeID)
                    command.Parameters.AddWithValue("@Title", document.Title)
                    command.Parameters.AddWithValue("@FileName", document.FileName)
                    command.Parameters.AddWithValue("@FileContent", document.FileContent)
                    command.Parameters.AddWithValue("@FileType", document.FileType)
                    command.Parameters.AddWithValue("@FileSize", document.FileSize)
                    command.Parameters.AddWithValue("@UploadDate", document.UploadDate)

                    Return command.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        ' Delete a document
        Public Function DeleteDocument(documentID As Integer, employeeID As Integer) As Boolean
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                conn.Open()

                Dim query As String = "DELETE FROM documents WHERE DocumentID = @DocumentID AND EmployeeID = @EmployeeID"
                Using command As New MySqlCommand(query, conn)
                    command.Parameters.AddWithValue("@DocumentID", documentID)
                    command.Parameters.AddWithValue("@EmployeeID", employeeID)

                    Return command.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        ' Helper method to convert file to Base64
        Public Shared Function FileToBase64(filePath As String) As String
            Dim bytes As Byte() = File.ReadAllBytes(filePath)
            Return Convert.ToBase64String(bytes)
        End Function

        ' Helper method to save Base64 to file
        Public Shared Sub Base64ToFile(base64String As String, outputPath As String)
            Dim bytes As Byte() = Convert.FromBase64String(base64String)
            File.WriteAllBytes(outputPath, bytes)
        End Sub
    End Class
End Namespace
