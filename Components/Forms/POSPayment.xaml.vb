Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models
Imports MySql.Data.MySqlClient

Namespace DPC.Components.Forms
    Public Class POSPayment
        Inherits UserControl
        Public Sub New()
            InitializeComponent()
            ' Add event handler for the close button
            AddHandler BtnClose.Click, AddressOf BtnClose_Click
        End Sub

        ' Event handler for the close button
        Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
            ' Try to find the parent container that hosts this user control
            Dim parent = TryCast(Me.Parent, ContentControl)
            If parent IsNot Nothing Then
                ' Remove this user control from its parent
                Dim container = TryCast(parent.Parent, Panel)
                If container IsNot Nothing Then
                    container.Children.Remove(parent)
                End If
            Else
                ' Try if this is inside a Popup
                Dim parentPopup = TryCast(Me.Parent, Popup)
                If parentPopup IsNot Nothing Then
                    parentPopup.IsOpen = False
                Else
                    ' Try to find the parent window as a last resort
                    Dim parentWindow = Window.GetWindow(Me)
                    If parentWindow IsNot Nothing Then
                        parentWindow.Close()
                    End If
                End If
            End If
        End Sub

    End Class
End Namespace
