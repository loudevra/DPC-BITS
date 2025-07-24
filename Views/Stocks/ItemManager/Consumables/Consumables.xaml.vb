Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Controllers.Stocks
Imports DPC.DPC.Views.ItemManager.Consumables
Imports DPC.DPC.Views.Warehouse

Namespace DPC.Views.Stocks.ItemManager.Consumables
    Public Class Consumables
        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            LoadConsumables()
        End Sub

        Private Sub LoadConsumables()
            dataGrid.ItemsSource = PullOutFormController.GetConsumables()
        End Sub

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs)
            ' Find the parent window to use as owner
            Dim parentWindow As Window = Window.GetWindow(Me)

            Dim addConsumables As New AddConsumables()

            ' Set owner if we found a parent window
            If parentWindow IsNot Nothing Then
                addConsumables.Owner = parentWindow
            End If

            addConsumables.ShowDialog() ' Show as modal popup

            dataGrid.ItemsSource = Nothing
            LoadConsumables()
        End Sub

    End Class

    Public Class ConsumableModels
        Public Property ProductID As String
        Public Property ProductName As String
        Public Property WarehouseName As String
        Public Property Stock As Integer
    End Class
End Namespace
