Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Media.Animation
Imports System.Windows.Controls
Imports System.Reflection
Imports System.Diagnostics
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Helpers ' Required for DynamicView
Imports DPC.DPC.Views.POS ' Added for POSForm

Namespace DPC
    Public Class Base
        Inherits Window
        Implements INotifyPropertyChanged

        ' Sidebar animation settings
        Private SidebarExpandedWidth As Double = 260
        Private SidebarCollapsedWidth As Double = 80
        Private SidebarPOSWidth As Double = 360  ' Special width for POS
        Private AnimationDuration As TimeSpan = TimeSpan.FromSeconds(0.5)
        Private POSAnimationDuration As TimeSpan = TimeSpan.FromSeconds(0.2)  ' Faster animation for POS
        Private SidebarAnimClock As AnimationClock

        ' Track sidebar state before opening POS
        Private wasDefaultSidebarExpanded As Boolean = True

        ' Property: CurrentView for dynamic content
        Private _currentView As Object
        Public Property CurrentView As Object
            Get
                Return _currentView
            End Get
            Set(value As Object)
                _currentView = value
                RaisePropertyChanged("CurrentView")
            End Set
        End Property

        ' Keep reference to sidebar for restoration
        Private defaultSidebar As Sidebar

        ' Constructor
        Public Sub New()
            InitializeComponent()

            ' Load Sidebar
            defaultSidebar = New Sidebar()
            SidebarContainer.Child = defaultSidebar

            ' Load Top Navigation Bar
            Dim topNavBar As New TopNavBar()
            TopNavBarContainer.Content = topNavBar

            ' Set default view using DynamicView
            CurrentView = ViewLoader.DynamicView.Load("dashboard")

            ' Bind data context to this class for ContentPresenter binding
            Me.DataContext = Me

            ' Handle sidebar toggle animation
            AddHandler defaultSidebar.SidebarToggled, AddressOf OnSidebarToggled

            ' Handle navigation events
            AddHandler topNavBar.NavigateToPOS, AddressOf LoadPOSForm
            AddHandler topNavBar.RestoreDefaultSidebar, AddressOf RestoreDefaultSidebar
        End Sub

        ' Load POS Form into the sidebar and ensure sidebar is expanded
        Private Sub LoadPOSForm()
            ' Save current sidebar state before switching to POS
            wasDefaultSidebarExpanded = SidebarColumn.Width.Value > SidebarCollapsedWidth

            ' Create a new POS form
            Dim posForm As New POSForm()

            ' Replace sidebar content with the POS form
            SidebarContainer.Child = posForm

            ' Expand sidebar to POS width (360px) with faster animation
            AnimateSidebarWidth(SidebarPOSWidth, True)
        End Sub

        ' Restore the default sidebar
        Private Sub RestoreDefaultSidebar()
            ' Only restore if current sidebar is not the default one
            If Not TypeOf SidebarContainer.Child Is Sidebar Then
                ' Store the target width before changing the sidebar content
                Dim targetWidth As Double = If(wasDefaultSidebarExpanded, SidebarExpandedWidth, SidebarCollapsedWidth)

                ' Restore sidebar content
                SidebarContainer.Child = defaultSidebar

                ' Set sidebar width with animation
                AnimateSidebarWidth(targetWidth, True)

                ' For now, we'll simply ensure the visual width is correct
                ' The internal state of the sidebar might not match visually, but
                ' functionally it will work as expected
            End If
        End Sub

        ' New method to close POS and return to normal sidebar
        Public Sub ClosePOS()
            ' Store the target width before changing the sidebar content
            Dim targetWidth As Double = If(wasDefaultSidebarExpanded, SidebarExpandedWidth, SidebarCollapsedWidth)

            ' Restore sidebar content
            SidebarContainer.Child = defaultSidebar

            ' Set sidebar width with animation
            AnimateSidebarWidth(targetWidth, True)

            ' For now, we'll simply ensure the visual width is correct
            ' The internal state of the sidebar might not match visually, but
            ' functionally it will work as expected
        End Sub

        ' General method to animate sidebar width to any target width
        ' Added isPOSAnimation parameter to determine which duration to use
        Private Sub AnimateSidebarWidth(targetWidth As Double, Optional isPOSAnimation As Boolean = False)
            ' Stop current animation if needed
            If SidebarAnimClock IsNot Nothing Then
                SidebarAnimClock.Controller.Stop()
                SidebarAnimClock = Nothing
            End If

            Dim currentWidth As Double = SidebarColumn.Width.Value

            ' Choose the animation duration based on whether it's a POS-related animation
            Dim duration As TimeSpan = If(isPOSAnimation, POSAnimationDuration, AnimationDuration)

            ' Create animation to target width
            Dim widthAnimation As New DoubleAnimation With {
                .From = currentWidth,
                .To = targetWidth,
                .Duration = duration,
                .EasingFunction = New QuadraticEase() With {.EasingMode = EasingMode.EaseInOut}
            }

            SidebarAnimClock = widthAnimation.CreateClock()

            AddHandler SidebarAnimClock.CurrentTimeInvalidated, Sub()
                                                                    If SidebarAnimClock.CurrentProgress.HasValue Then
                                                                        Dim progress = SidebarAnimClock.CurrentProgress.Value
                                                                        Dim currentValue = currentWidth + (targetWidth - currentWidth) * progress
                                                                        SidebarColumn.Width = New GridLength(currentValue)
                                                                    End If
                                                                End Sub

            SidebarAnimClock.Controller.Begin()
        End Sub

        ' Method to explicitly expand the sidebar (used by other methods)
        Private Sub ExpandSidebar()
            AnimateSidebarWidth(SidebarExpandedWidth)
        End Sub

        ' Handle sidebar toggle animation
        Private Sub OnSidebarToggled(isExpanded As Boolean)
            ' When sidebar is toggled by user, use regular expanded/collapsed width
            ' not the special POS width
            Dim targetWidth As Double = If(isExpanded, SidebarExpandedWidth, SidebarCollapsedWidth)
            AnimateSidebarWidth(targetWidth)

            ' Update our tracking variable
            wasDefaultSidebarExpanded = isExpanded
        End Sub

        ' INotifyPropertyChanged Implementation
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Protected Sub RaisePropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub
    End Class
End Namespace