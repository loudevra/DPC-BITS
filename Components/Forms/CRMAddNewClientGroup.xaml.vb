Imports System.Collections.ObjectModel
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.CRM.ClientGroup

Namespace DPC.Components.Forms

    Public Class CRMAddNewClientGroup
        Inherits UserControl
        Public Event FormClosed As EventHandler


        Public Sub New()
            InitializeComponent()
            AddHandler BtnClose.Click, AddressOf BtnClose_Click
        End Sub

        Private Sub AddClientGroup(sender As Object, e As RoutedEventArgs)
            Dim _clientGroup As New ClientGroup With {
                .GroupName = ClientGroupName.Text,
                .Description = ClientGroupDescription.Text
            }

            If ClientGroupController.CreateClientGroup(_clientGroup) Then
                CloseWindow()
            End If
        End Sub

        ' Close Button Handler
        Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
            CloseWindow()
        End Sub

        Private Sub CloseWindow()
            RaiseEvent FormClosed(Me, EventArgs.Empty)

            Dim parent = TryCast(Me.Parent, ContentControl)
            If parent IsNot Nothing Then
                Dim container = TryCast(parent.Parent, Panel)
                If container IsNot Nothing Then
                    container.Children.Remove(parent)
                End If
            Else
                Dim parentPopup = TryCast(Me.Parent, Popup)
                If parentPopup IsNot Nothing Then
                    parentPopup.IsOpen = False
                Else
                    Dim parentWindow = Window.GetWindow(Me)
                    If parentWindow IsNot Nothing Then
                        parentWindow.Close()
                    End If
                End If
            End If
        End Sub
    End Class
End Namespace



