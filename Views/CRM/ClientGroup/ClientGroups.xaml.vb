Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports System.Windows
Imports DPC.DPC.Data.Helpers

Namespace DPC.Views.CRM.ClientGroup
    Public Class ClientGroups
        Inherits UserControl
        ' UI elements for direct access
        'Private popup As Popup
        Private recentlyClosed As Boolean = False
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

        End Sub
        Private Sub CRMAddNewClientGroup(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim CRMAddNewClientGroup As New DPC.Components.Forms.CRMAddNewClientGroup()
            ' Get the parent window to center the popup
            Dim parentWindow = Window.GetWindow(Me)
            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, CRMAddNewClientGroup, "windowcenter", True, -50, 0, parentWindow)
        End Sub
    End Class
End Namespace
