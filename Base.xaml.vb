Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media.Animation
Imports DPC.Components.Navigation
Imports DPC.DPC.Components.Navigation
Imports DPC.DPC.Views.POS

Namespace DPC
    Public Class Base
        Inherits Window
        Implements INotifyPropertyChanged

        ' Sidebar settings
        Private Property RoleName As String
        Private SidebarExpandedWidth As Double = 260
        Private SidebarCollapsedWidth As Double = 80
        Private SidebarPOSWidth As Double = 360
        Private AnimationDuration As TimeSpan = TimeSpan.FromSeconds(0.5)
        Private POSAnimationDuration As TimeSpan = TimeSpan.FromSeconds(0.2)
        Private SidebarAnimClock As AnimationClock

        ' State Tracking
        Private wasDefaultSidebarExpanded As Boolean = True
        Private IsAIPanelOpen As Boolean = False
        Private defaultSidebar As Sidebar

        ' Property for Dynamic Content
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
        Public Sub New(_roleName As String)
            InitializeComponent()
            RoleName = _roleName

            ' 1. Load Sidebar
            defaultSidebar = New Sidebar()
            SidebarContainer.Child = defaultSidebar

            ' 2. Load Top Navigation Bar
            Dim topNavBar As New TopNavBar()
            TopNavBarContainer.Content = topNavBar

            ' 3. Set DataContext for Binding
            Me.DataContext = Me
            CurrentView = Data.Helpers.ViewLoader.DynamicView.Load("dashboard")

            ' 4. Event Handlers
            AddHandler defaultSidebar.SidebarToggled, AddressOf OnSidebarToggled
            AddHandler topNavBar.NavigateToPOS, AddressOf LoadPOSForm
            AddHandler topNavBar.RestoreDefaultSidebar, AddressOf RestoreDefaultSidebar

            ' New AI Toggle Handler (matches the event in TopNavBar)
            AddHandler topNavBar.NavigateToAI, AddressOf ToggleAIPanel
        End Sub

        ' --- AI PANEL LOGIC (Responsive Split Screen) ---
        Private Sub ToggleAIPanel()
            If IsAIPanelOpen Then
                ' Close the panel
                AIColumn.Width = New GridLength(0)
                AIPanelContainer.Visibility = Visibility.Collapsed
                IsAIPanelOpen = False
            Else
                ' Load the view only once (Lazy Loading)
                If AIContentControl.Content Is Nothing Then
                    AIContentControl.Content = New DPC.Components.Navigation.AIWebView()
                End If

                ' Open the panel - MainContentColumn (Width="*") will shrink automatically
                AIPanelContainer.Visibility = Visibility.Visible
                AIColumn.Width = New GridLength(450) ' Adjust width as needed
                IsAIPanelOpen = True
            End If
        End Sub

        ' Change "Private" to "Public" so POSForm can call it
        Public Sub ClosePOS()
            ' Your existing code to close POS...
            Dim targetWidth As Double = If(wasDefaultSidebarExpanded, SidebarExpandedWidth, SidebarCollapsedWidth)
            SidebarContainer.Child = defaultSidebar
            AnimateSidebarWidth(targetWidth, True)
        End Sub

        ' --- EXISTING POS & SIDEBAR LOGIC ---
        Private Sub LoadPOSForm()
            wasDefaultSidebarExpanded = SidebarColumn.Width.Value > SidebarCollapsedWidth
            Dim posForm As New POSForm()
            SidebarContainer.Child = posForm
            AnimateSidebarWidth(SidebarPOSWidth, True)
        End Sub

        Private Sub RestoreDefaultSidebar()
            If Not TypeOf SidebarContainer.Child Is Sidebar Then
                Dim targetWidth As Double = If(wasDefaultSidebarExpanded, SidebarExpandedWidth, SidebarCollapsedWidth)
                SidebarContainer.Child = defaultSidebar
                AnimateSidebarWidth(targetWidth, True)
            End If
        End Sub

        Private Sub AnimateSidebarWidth(targetWidth As Double, Optional isPOSAnimation As Boolean = False)
            If SidebarAnimClock IsNot Nothing Then
                SidebarAnimClock.Controller.Stop()
            End If

            Dim currentWidth As Double = SidebarColumn.Width.Value
            Dim duration As TimeSpan = If(isPOSAnimation, POSAnimationDuration, AnimationDuration)

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
                                                                        Dim val = currentWidth + (targetWidth - currentWidth) * progress
                                                                        SidebarColumn.Width = New GridLength(val)
                                                                    End If
                                                                End Sub
            SidebarAnimClock.Controller.Begin()
        End Sub

        Private Sub OnSidebarToggled(isExpanded As Boolean)
            Dim targetWidth As Double = If(isExpanded, SidebarExpandedWidth, SidebarCollapsedWidth)
            AnimateSidebarWidth(targetWidth)
            wasDefaultSidebarExpanded = isExpanded
        End Sub

        ' --- NOTIFY PROPERTY CHANGED ---
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Protected Sub RaisePropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub
    End Class
End Namespace