Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Controllers ' For CalendarController.SingleCalendar

Namespace DPC.Views.Project
    Partial Public Class AddProject2
        Inherits UserControl

        ' Add SingleCalendar ViewModels for both pickers
        Private startDateViewModel As New CalendarController.SingleCalendar()
        Private dueDateViewModel As New CalendarController.SingleCalendar()

        Public Sub New()
            InitializeComponent()
            SetupDatePickers()
        End Sub

        ' Setup bindings between DatePickers, Buttons, and ViewModels
        Public Sub SetupDatePickers()
            startDateViewModel.SelectedDate = Nothing
            dueDateViewModel.SelectedDate = Nothing

            StartDatePicker.DataContext = startDateViewModel
            StartDateButton.DataContext = startDateViewModel

            DueDatePicker.DataContext = dueDateViewModel
            DueDateButton.DataContext = dueDateViewModel
        End Sub

        ' Trigger dropdown open
        Private Sub StartDateButton_Click(sender As Object, e As RoutedEventArgs) Handles StartDateButton.Click
            StartDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub DueDateButton_Click(sender As Object, e As RoutedEventArgs) Handles DueDateButton.Click
            DueDatePicker.IsDropDownOpen = True
        End Sub

        ' Handle selected date change
        Private Sub StartDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles StartDatePicker.SelectedDateChanged
            Dim dp = TryCast(sender, DatePicker)
            If dp IsNot Nothing AndAlso dp.DataContext IsNot Nothing Then
                Dim vm = TryCast(dp.DataContext, CalendarController.SingleCalendar)
                If vm IsNot Nothing Then
                    vm.SelectedDate = dp.SelectedDate
                    BindingOperations.GetBindingExpression(StartDateButton, Button.DataContextProperty)?.UpdateTarget()
                End If
            End If
        End Sub

        Private Sub DueDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles DueDatePicker.SelectedDateChanged
            Dim dp = TryCast(sender, DatePicker)
            If dp IsNot Nothing AndAlso dp.DataContext IsNot Nothing Then
                Dim vm = TryCast(dp.DataContext, CalendarController.SingleCalendar)
                If vm IsNot Nothing Then
                    vm.SelectedDate = dp.SelectedDate
                    BindingOperations.GetBindingExpression(DueDateButton, Button.DataContextProperty)?.UpdateTarget()
                End If
            End If
        End Sub

        ' Popup logic
        Friend Sub ShowPopup(parent As UIElement, sender As Object)
            Dim button As Button = TryCast(sender, Button)
            If button Is Nothing Then Return

            Dim window As Window = Window.GetWindow(button)
            If window Is Nothing Then Return

            Dim sidebarWidth As Double = 0
            Dim parentControl = TryCast(button.Parent, FrameworkElement)

            While parentControl IsNot Nothing
                If TypeOf parentControl Is StackPanel AndAlso parentControl.Name = "SidebarMenu" Then
                    Dim sidebarContainer = TryCast(parentControl.Parent, FrameworkElement)
                    If sidebarContainer IsNot Nothing Then
                        sidebarWidth = sidebarContainer.ActualWidth
                        Exit While
                    End If
                ElseIf TypeOf parentControl.Parent Is DPC.Components.Navigation.Sidebar Then
                    sidebarWidth = CType(parentControl.Parent, FrameworkElement).ActualWidth
                    Exit While
                End If
                parentControl = TryCast(parentControl.Parent, FrameworkElement)
            End While

            If sidebarWidth = 0 Then sidebarWidth = 260

            Dim popup As New Popup With {
                .Child = Me,
                .StaysOpen = False,
                .Placement = PlacementMode.Relative,
                .PlacementTarget = button,
                .IsOpen = True,
                .AllowsTransparency = True
            }

            If sidebarWidth <= 80 Then
                popup.HorizontalOffset = 60
                popup.VerticalOffset = -button.ActualHeight * 3
            Else
                popup.HorizontalOffset = sidebarWidth - button.Margin.Left
                popup.VerticalOffset = -button.ActualHeight * 3
            End If

            Dim locationChangedHandler As EventHandler = Nothing
            Dim sizeChangedHandler As SizeChangedEventHandler = Nothing

            locationChangedHandler = Sub(s, e)
                                         If popup.IsOpen Then
                                             popup.HorizontalOffset = popup.HorizontalOffset
                                             popup.VerticalOffset = popup.VerticalOffset
                                         End If
                                     End Sub

            sizeChangedHandler = Sub(s, e)
                                     If popup.IsOpen Then
                                         popup.HorizontalOffset = popup.HorizontalOffset
                                         popup.VerticalOffset = popup.VerticalOffset
                                     End If
                                 End Sub

            AddHandler window.LocationChanged, locationChangedHandler
            AddHandler window.SizeChanged, sizeChangedHandler

            AddHandler popup.Closed, Sub(s, e)
                                         RemoveHandler window.LocationChanged, locationChangedHandler
                                         RemoveHandler window.SizeChanged, sizeChangedHandler
                                     End Sub
        End Sub

        Private Sub NavigateToNewProject(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("newproject", Me)
        End Sub

        Private Sub NavigateToAddProj3(sender As Object, e As RoutedEventArgs)
            ViewLoader.DynamicView.NavigateToView("addproject3", Me)
        End Sub

    End Class
End Namespace
