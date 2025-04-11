Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Media.Animation
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Data.Helpers ' Required for DynamicView

Namespace DPC
    Public Class Base
        Inherits Window
        Implements INotifyPropertyChanged

        ' Sidebar animation settings
        Private SidebarExpandedWidth As Double = 260
        Private SidebarCollapsedWidth As Double = 80
        Private AnimationDuration As TimeSpan = TimeSpan.FromSeconds(0.5)
        Private SidebarAnimClock As AnimationClock

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

        ' Constructor
        Public Sub New()
            InitializeComponent()

            ' Load Sidebar
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar

            ' Load Top Navigation Bar
            Dim topNavBar As New TopNavBar()
            TopNavBarContainer.Content = topNavBar

            ' 🔥 Set default view using DynamicView
            CurrentView = ViewLoader.DynamicView.Load("dashboard")

            ' 🔥 Bind data context to this class for ContentPresenter binding
            Me.DataContext = Me

            ' Handle sidebar toggle animation
            AddHandler sidebar.SidebarToggled, AddressOf OnSidebarToggled
        End Sub

        ' Handle sidebar toggle animation
        Private Sub OnSidebarToggled(isExpanded As Boolean)
            Dim targetWidth As Double = If(isExpanded, SidebarExpandedWidth, SidebarCollapsedWidth)

            ' Stop current animation if needed
            If SidebarAnimClock IsNot Nothing Then
                SidebarAnimClock.Controller.Stop()
                SidebarAnimClock = Nothing
            End If

            Dim currentWidth As Double = SidebarColumn.Width.Value

            ' Create animation
            Dim widthAnimation As New DoubleAnimation With {
                .From = currentWidth,
                .To = targetWidth,
                .Duration = AnimationDuration,
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

        ' INotifyPropertyChanged Implementation
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Protected Sub RaisePropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub
    End Class
End Namespace
