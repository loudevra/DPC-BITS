Imports DPC.DPC.Views
Imports System.Windows
Imports System.Windows.Controls

Namespace DPC.Data.Helpers.ViewLoader
    ''' <summary>
    ''' Facade class that provides the original public interface for DynamicView functionality
    ''' Maintains backward compatibility while delegating to specialized classes
    ''' </summary>
    Public Class DynamicView
        ''' <summary>
        ''' Loads the requested view
        ''' </summary>
        Public Shared Function Load(viewName As String) As UserControl
            Return ViewLoader.Load(viewName)
        End Function

        ''' <summary>
        ''' Navigate to the specified view
        ''' </summary>
        Public Shared Sub NavigateToView(viewName As String, senderControl As DependencyObject)
            ViewNavigation.NavigateToView(viewName, senderControl)
        End Sub
    End Class
End Namespace