Imports System.Windows

Namespace DPC.Views.Project
    Public Class ToDoList2
        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub SaveToDoList(sender As Object, e As RoutedEventArgs)
            ' Add your save logic here
            MessageBox.Show("To-Do List saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub
    End Class
End Namespace
