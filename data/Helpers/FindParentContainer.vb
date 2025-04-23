Public Class FindParentContainer
    Public Shared Function FindParentContainer(rowPanel As StackPanel, StackPanelSerialRow As StackPanel) As StackPanel
        ' Find the ScrollViewer and then the container
        Dim containerBorder As Border = TryCast(StackPanelSerialRow.Children(1), Border)
        If containerBorder Is Nothing Then Return Nothing

        Dim scrollViewer As ScrollViewer = TryCast(containerBorder.Child, ScrollViewer)
        If scrollViewer Is Nothing Then Return Nothing

        Dim container As StackPanel = TryCast(scrollViewer.Content, StackPanel)
        Return container
    End Function
End Class
