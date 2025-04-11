Imports System
Imports System.Windows.Input

Namespace DPC.Data.Helpers
    Public Class RelayCommand
        Implements ICommand

        Private ReadOnly _execute As Action(Of Object)
        Private ReadOnly _canExecute As Func(Of Object, Boolean)

        Public Sub New(execute As Action(Of Object), Optional canExecute As Func(Of Object, Boolean) = Nothing)
            If execute Is Nothing Then Throw New ArgumentNullException(NameOf(execute))
            _execute = execute
            _canExecute = canExecute
        End Sub

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return _canExecute?.Invoke(parameter) <> False
        End Function

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            _execute.Invoke(parameter)
        End Sub

        ' Manually raise CanExecuteChanged to update UI
        Public Sub RaiseCanExecuteChanged()
            RaiseEvent CanExecuteChanged(Me, EventArgs.Empty)
        End Sub
    End Class
End Namespace
