Imports DPC.DPC.Views
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Media

Namespace DPC.Data.Helpers
    Public Class DynamicView
        ' Flag to prevent recursive navigation
        Private Shared _isNavigating As Boolean = False

        ' Load the requested view
        Public Shared Function Load(viewName As String) As UserControl
            Try
                Select Case viewName.ToLower()
                    Case "dashboard"
                        Return New Dashboard.Dashboard() ' This is a UserControl
                    Case "stockstransfer"
                        Return New Stocks.StocksTransfer.StocksTransfer() ' This is a UserControl
                    Case "newsuppliers"
                        Return New Stocks.Supplier.NewSuppliers.NewSuppliers() ' This is now a UserControl
                    Case "managesuppliers"
                        Return New Stocks.Suppliers.ManageSuppliers.ManageSuppliers() ' This is now a UserControl
                    Case "managebrands"
                        Return New Stocks.Suppliers.ManageBrands.ManageBrands() ' This is now a UserControl
                    Case Else
                        ' Return a placeholder UserControl with error text
                        Dim errorContent As New TextBlock With {
                            .Text = $"View not found: {viewName}",
                            .FontSize = 20,
                            .HorizontalAlignment = HorizontalAlignment.Center,
                            .VerticalAlignment = VerticalAlignment.Center
                        }
                        Return New UserControl With {.Content = errorContent}
                End Select
            Catch ex As Exception
                MessageBox.Show($"Error loading view '{viewName}': {ex.Message}")
                ' Return an error UserControl in case of exception
                Dim errorContent As New TextBlock With {
                    .Text = $"Error loading view: {viewName}",
                    .FontSize = 20,
                    .Foreground = New SolidColorBrush(Colors.Red),
                    .HorizontalAlignment = HorizontalAlignment.Center,
                    .VerticalAlignment = VerticalAlignment.Center
                }
                Return New UserControl With {.Content = errorContent}
            End Try
        End Function

        ' Navigate to the specified view
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
                    Dim currentViewName = GetViewName(currentView)

                    ' Only navigate if we're going to a different view
                    If currentViewName <> viewName.ToLower() Then
                        ' Load the new view
                        Dim newView = Load(viewName)

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

        ' Helper function to get the name of a view
        Private Shared Function GetViewName(view As Object) As String
            If view Is Nothing Then Return String.Empty

            Dim typeName As String = view.GetType().Name.ToLower()

            If typeName = "dashboard" Then
                Return "dashboard"
            ElseIf typeName = "stockstransfer" Then
                Return "stocks.stocktransfer"
            ElseIf typeName = "newsuppliers" Then
                Return "newsuppliers"
            ElseIf typeName = "managesuppliers" Then
                Return "managesuppliers"
            ElseIf typeName = "managebrands" Then
                Return "managebrands"
            Else
                Return typeName
            End If
        End Function

        ' Helper method to find the main window
        Private Shared Function FindMainWindow() As DPC.Base
            For Each window In Application.Current.Windows
                If TypeOf window Is DPC.Base Then
                    Return DirectCast(window, DPC.Base)
                End If
            Next
            Return Nothing
        End Function

        ' Helper method to close parent popup if present
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