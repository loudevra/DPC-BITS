Imports System

Namespace DPC.Data.Models
    Public Class Document
        Public Property DocumentID As Integer
        Public Property EmployeeID As String
        Public Property Title As String
        Public Property FileName As String
        Public Property FileContent As String  ' Base64 encoded content
        Public Property FileType As String
        Public Property FileSize As Integer
        Public Property UploadDate As DateTime

        Public Sub New()
            UploadDate = DateTime.Now
        End Sub

        Public Sub New(employeeID As Integer, title As String, fileName As String, fileContent As String, fileType As String, fileSize As Integer)
            Me.EmployeeID = employeeID
            Me.Title = title
            Me.FileName = fileName
            Me.FileContent = fileContent
            Me.FileType = fileType
            Me.FileSize = fileSize
            Me.UploadDate = DateTime.Now
        End Sub
    End Class
End Namespace