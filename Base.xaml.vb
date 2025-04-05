Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Media.Animation
Imports DPC.DPC.Components.Navigation

Namespace DPC
    Public Class Base
        Implements INotifyPropertyChanged

        ' Store sidebar expanded width for animations
        Private SidebarExpandedWidth As Double = 260
        Private SidebarCollapsedWidth As Double = 80
        Private AnimationDuration As TimeSpan = TimeSpan.FromSeconds(0.3)
        Private SidebarAnimClock As AnimationClock

        Public Sub New()
            InitializeComponent()

            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar

            ' Add TopNavBar to TopNavBarContainer
            Dim topNavBar As New TopNavBar()
            TopNavBarContainer.Content = topNavBar

            ' 🔥 Attach event listener for sidebar toggle
            AddHandler sidebar.SidebarToggled, AddressOf OnSidebarToggled
        End Sub

        ' 🔥 Event handler to resize and animate MainContentColumn when Sidebar is toggled
        Private Sub OnSidebarToggled(isExpanded As Boolean)
            Dim targetWidth As Double = If(isExpanded, SidebarExpandedWidth, SidebarCollapsedWidth)

            ' Stop previous animation if still running
            If SidebarAnimClock IsNot Nothing Then
                SidebarAnimClock.Controller.Stop()
                SidebarAnimClock = Nothing
            End If

            ' Get current width (safely)
            Dim currentWidth As Double = SidebarColumn.Width.Value

            ' Setup animation
            Dim widthAnimation As New DoubleAnimation With {
        .From = currentWidth,
        .To = targetWidth,
        .Duration = AnimationDuration,
        .EasingFunction = New QuadraticEase() With {.EasingMode = EasingMode.EaseInOut}
    }

            ' Create AnimationClock
            SidebarAnimClock = widthAnimation.CreateClock()

            ' On every frame, update actual GridLength width
            AddHandler SidebarAnimClock.CurrentTimeInvalidated, Sub()
                                                                    If SidebarAnimClock.CurrentProgress.HasValue Then
                                                                        Dim progress = SidebarAnimClock.CurrentProgress.Value
                                                                        Dim currentValue = currentWidth + (targetWidth - currentWidth) * progress
                                                                        SidebarColumn.Width = New GridLength(currentValue)
                                                                    End If
                                                                End Sub

            ' Begin animation
            SidebarAnimClock.Controller.Begin()
        End Sub


        ' Current View Property
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

        ' Property Changed Handler
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        ' Rename this method to avoid shadowing FrameworkElement.OnPropertyChanged
        Protected Sub RaisePropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub

    End Class
End Namespace