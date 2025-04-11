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

        ''' <summary>
        ''' Navigate to the specified view
        ''' </summary>
        ''' <param name="viewName">Name of the view to navigate to</param>
        ''' <param name="senderControl">The control that initiated the navigation</param>
        Public Shared Sub NavigateToView(viewName As String, senderControl As DependencyObject)
            ' Prevent reentrancy which could cause freezing
            If _isNavigating Then Return

            Try
                _isNavigating = True

                ' Check if we're in design mode
                If System.ComponentModel.DesignerProperties.GetIsInDesignMode(New DependencyObject()) Then
                    _isNavigating = False
                    Return ' Exit if in design mode
                End If

                ' Find the main window
                Dim mainWindow As DPC.Base = FindMainWindow()

                If mainWindow IsNot Nothing Then
                    ' Get the currently displayed view for comparison
                    Dim currentView = mainWindow.CurrentView
                    Dim currentViewName = ViewLoader.GetViewName(currentView)

                    ' Only navigate if we're going to a different view
                    If currentViewName <> viewName.ToLower() Then
                        ' Load the new view
                        Dim newView = ViewLoader.Load(viewName)

                        ' Navigate to the requested view
                        mainWindow.CurrentView = newView
                    End If

                    ' Close the popup if we're in one, but do this with a slight delay
                    ' to prevent UI thread blocking
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