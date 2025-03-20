Imports System.Windows.Controls.Primitives

Namespace DPC.Components.UI
    Public Class PopUpMenuHRM
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Friend Sub ShowPopup(parent As UIElement, sender As Object)
            ' Ensure sender is a Button
            Dim button As Button = CType(sender, Button)

            ' Check if the button has a valid parent (it must be part of the visual tree)
            If button Is Nothing Then
                MessageBox.Show("Button is null.")
                Return
            End If

            ' Get the button's position relative to the parent container (Main Window or Panel)
            Dim buttonPosition As Point = button.TransformToAncestor(parent).Transform(New Point(0, 0))

            ' Adjust the offset values based on button's actual size and desired placement
            Dim horizontalOffset As Double = buttonPosition.X + button.ActualWidth + 10  ' 10px to the right of the button
            Dim verticalOffset As Double = buttonPosition.Y  ' Same Y position as the button

            ' Create the popup
            Dim popup As New Popup With {
                .Child = Me,
                .StaysOpen = False,
                .Placement = PlacementMode.Relative,
                .PlacementTarget = button,
                .HorizontalOffset = 233,  ' Set the horizontal offset
                .VerticalOffset = -100,      ' Set the vertical offset
                .IsOpen = True
            }

            ' Ensure the Popup adjusts when the window is resized or moved
            AddHandler parent.LayoutUpdated, AddressOf OnLayoutUpdated
        End Sub

        ''' <summary>
        ''' This handler will be triggered when the layout of the parent (window or panel) is updated.
        ''' We can reposition the popup based on the new layout.
        ''' </summary>
        Private Sub OnLayoutUpdated(sender As Object, e As EventArgs)
            ' Ensure the sender is a valid Button
            Dim button As Button = TryCast(sender, Button)
            If button Is Nothing Then
                Return
            End If

            ' Get the parent of the button (ensure it exists)
            Dim parent As UIElement = TryCast(VisualTreeHelper.GetParent(button), UIElement)
            If parent Is Nothing Then
                Return
            End If

            ' Get the position of the button relative to the parent
            Dim buttonPosition As Point = button.TransformToAncestor(parent).Transform(New Point(0, 0))

            ' Adjust the popup's position dynamically
            Dim horizontalOffset As Double = buttonPosition.X + button.ActualWidth + 10
            Dim verticalOffset As Double = buttonPosition.Y

            ' Update the position of the popup (for example, you can reposition it if needed)
            ' popup.HorizontalOffset = horizontalOffset
            ' popup.VerticalOffset = verticalOffset
        End Sub

        Private Sub NavigateToEmployees(sender As Object, e As RoutedEventArgs)
            ' Open EmployeesView
            Dim employeesWindow As New Views.HRM.Employees.Employees.EmployeesView()
            employeesWindow.Show()

            ' Close the current window where this UserControl is being used
            Dim currentWindow As Window = Window.GetWindow(Me)
            currentWindow?.Close()
        End Sub

        Private Sub NavigateToPermissions(sender As Object, e As RoutedEventArgs)
            ' Open EmployeesView

        End Sub

        Private Sub NavigateToSalaries(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigating to Salaries")
        End Sub

        Private Sub NavigateToAttendance(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigating to Attendance")
        End Sub

        Private Sub NavigateToHolidays(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigating to Holidays")
        End Sub

        Private Sub NavigateToDepartments(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigating to Departments")
        End Sub

        Private Sub NavigateToPayroll(sender As Object, e As RoutedEventArgs)
            MessageBox.Show("Navigating to Payroll")
        End Sub
    End Class
End Namespace
