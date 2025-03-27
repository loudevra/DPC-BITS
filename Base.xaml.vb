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
            SidebarContainer.Child = sidebar ' Use Content instead of Child

            ' Add TopNavBar to TopNavBarContainer
            Dim topNavBar As New TopNavBar()
            TopNavBarContainer.Content = topNavBar ' Use Content instead of Child
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
