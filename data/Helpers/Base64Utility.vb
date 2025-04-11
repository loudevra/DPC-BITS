Imports System
Imports System.IO
Imports System.Text

Namespace DPC.Data.Helpers
    Public Class Base64Utility
        ' Encode a file to Base64 string
        Public Shared Function EncodeFileToBase64(filePath As String) As String
            Try
                Dim bytes As Byte() = File.ReadAllBytes(filePath)
                Return Convert.ToBase64String(bytes)
            Catch ex As Exception
                Throw New Exception("Error encoding file to Base64: " & ex.Message)
            End Try
        End Function

        ' Decode Base64 string to file
        Public Shared Sub DecodeBase64ToFile(base64String As String, outputPath As String)
            Try
                Dim bytes As Byte() = Convert.FromBase64String(base64String)
                File.WriteAllBytes(outputPath, bytes)
            Catch ex As Exception
                Throw New Exception("Error decoding Base64 to file: " & ex.Message)
            End Try
        End Sub

        ' Get file size in readable format
        Public Shared Function GetReadableFileSize(size As Long) As String
            Dim sizes As String() = {"B", "KB", "MB", "GB", "TB"}
            Dim order As Integer = 0
            Dim len As Double = size

            While len >= 1024 AndAlso order < sizes.Length - 1
                order += 1
                len /= 1024
            End While

            Return String.Format("{0:0.##} {1}", len, sizes(order))
        End Function

        ' Get file extension from file name
        Public Shared Function GetFileExtension(fileName As String) As String
            Return Path.GetExtension(fileName).TrimStart("."c).ToLower()
        End Function

        ' Get file type based on extension
        Public Shared Function GetFileType(extension As String) As String
            Select Case extension.ToLower()
                Case "pdf"
                    Return "application/pdf"
                Case "docx"
                    Return "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                Case "doc"
                    Return "application/msword"
                Case "xls"
                    Return "application/vnd.ms-excel"
                Case "xlsx"
                    Return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                Case "txt"
                    Return "text/plain"
                Case "jpg", "jpeg"
                    Return "image/jpeg"
                Case "png"
                    Return "image/png"
                Case Else
                    Return "application/octet-stream"
            End Select
        End Function
    End Class
End Namespace