Imports System.ComponentModel
Imports System.Windows
Imports DPC.DPC.Components.Navigation

Namespace DPC
    Public Class Base
        Implements INotifyPropertyChanged

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

        ' 🔥 Event handler to resize MainContentColumn when Sidebar is toggled
        Private Sub OnSidebarToggled(isExpanded As Boolean)
            Dispatcher.Invoke(Sub()
                                  If isExpanded Then
                                      ' Sidebar Expanded → Main Content Shrinks
                                      MainContentColumn.Width = New GridLength(1, GridUnitType.Star)
                                  Else
                                      ' Sidebar Collapsed → Main Content Expands
                                      MainContentColumn.Width = New GridLength(1.2, GridUnitType.Star)
                                  End If
                              End Sub)
        End Sub

        ' Current View Property
        Private _currentView As Object
        Public Property CurrentView As Object
            Get
                Return _currentView
            End Get
            Set(value As Object)
                _currentView = value
            End Set
        End Property

        ' Property Changed Handler
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    End Class
End Namespace
