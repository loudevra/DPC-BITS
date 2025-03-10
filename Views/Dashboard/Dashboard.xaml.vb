Imports DPC.DPC.Components.Navigation

Namespace DPC.Views.Dashboard
    Partial Public Class Dashboard
        Inherits Window

        Public Sub New()
            InitializeComponent()

            ' Add Sidebar to SidebarContainer
            Dim sidebar As New Sidebar()
            SidebarContainer.Child = sidebar
        End Sub
    End Class
End Namespace
