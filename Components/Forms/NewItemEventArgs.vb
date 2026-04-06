Namespace DPC.Components.Forms

    Public Class NewItemEventArgs
        Inherits EventArgs

        Public Property NewId As Integer
        Public Property NewName As String

        Public Sub New(id As Integer, name As String)
            NewId = id
            NewName = name
        End Sub

    End Class

End Namespace