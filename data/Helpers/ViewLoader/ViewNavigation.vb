Imports DPC.DPC.Views
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Media

Namespace DPC.Data.Helpers.ViewLoader
    ''' <summary>
    ''' Manages navigation between different views in the application
    ''' </summary>
    Public Class ViewNavigation
        ' Flag to prevent recursive navigation
        Private Shared _isNavigating As Boolean = False
        Private Shared _viewCache As New Dictionary(Of String, Object)
        ''' <summary>
        ''' Navigate to the specified view
        ''' </summary>
        ''' <param name="viewName">Name of the view to navigate to</param>
        ''' <param name="senderControl">The control that initiated the navigation</param>
        Public Shared Sub NavigateToView(viewName As String, senderControl As DependencyObject)
            If _isNavigating Then Return

            Try
                _isNavigating = True
                Dim mainWindow As DPC.Base = FindMainWindow()

                If mainWindow IsNot Nothing Then
                    Dim targetName = viewName.ToLower()
                    Dim currentViewName = ViewLoader.GetViewName(mainWindow.CurrentView)

                    If currentViewName <> targetName Then
                        Dim targetView As Object = Nothing

                        If _viewCache.ContainsKey(targetName) Then
                            targetView = _viewCache(targetName)
                        Else
                            targetView = ViewLoader.Load(viewName)
                            _viewCache(targetName) = targetView
                        End If

                        mainWindow.CurrentView = targetView
                    End If

                    If senderControl IsNot Nothing Then
                        ' Use Dispatcher to add a delay
                        Application.Current.Dispatcher.BeginInvoke(
                            New Action(Sub() CloseParentPopup(senderControl)),
                            System.Windows.Threading.DispatcherPriority.Background)
                    End If
                Else
                    MessageBox.Show("Cannot find the main application window.")
                End If
            Catch ex As Exception
                MessageBox.Show($"Error navigating to {viewName}: {ex.Message}")
            Finally
                _isNavigating = False
            End Try
        End Sub

        Public Shared Sub ClearViewCache(viewName As String)
            Dim key = viewName.ToLower()
            If _viewCache.ContainsKey(key) Then _viewCache.Remove(key)
        End Sub

        ''' <summary>
        ''' Helper method to find the main window
        ''' </summary>
        Private Shared Function FindMainWindow() As DPC.Base
            For Each window In Application.Current.Windows
                If TypeOf window Is DPC.Base Then
                    Return DirectCast(window, DPC.Base)
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Helper method to close parent popup if present
        ''' </summary>
        Private Shared Sub CloseParentPopup(control As DependencyObject)
            Try
                ' Simple parent traversal without complex tree walking
                Dim current As DependencyObject = control
                Dim parentPopup As Popup = Nothing

                While current IsNot Nothing
                    ' Check if current element is a popup
                    parentPopup = TryCast(current, Popup)
                    If parentPopup IsNot Nothing Then
                        parentPopup.IsOpen = False
                        Return
                    End If

                    ' Try to get the parent - first logical then visual if needed
                    Dim fe As FrameworkElement = TryCast(current, FrameworkElement)
                    If fe IsNot Nothing AndAlso fe.Parent IsNot Nothing Then
                        current = fe.Parent
                    ElseIf current IsNot Nothing AndAlso VisualTreeHelper.GetParent(current) IsNot Nothing Then
                        current = VisualTreeHelper.GetParent(current)
                    Else
                        ' No more parents to check
                        current = Nothing
                    End If
                End While
            Catch ex As Exception
                ' Just suppress any errors in popup handling to prevent freezing
                Debug.WriteLine($"Error closing popup: {ex.Message}")
            End Try
        End Sub
    End Class
End Namespace