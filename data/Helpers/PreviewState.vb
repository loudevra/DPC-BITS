Imports DPC.DPC.Data

Public Module PreviewState
    Public Property CurrentPreview As New Models.UniversalPreviewModel()

    Public Sub ResetPreview()
        CurrentPreview = New Models.UniversalPreviewModel()
    End Sub
End Module