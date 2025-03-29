Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Components
Imports System.Windows.Controls.Primitives
Imports System.Collections.ObjectModel

Namespace DPC.Views.Stocks.Suppliers.ManageBrands
    Public Class ManageBrands
        Inherits Window

        Private popup As Popup
        Private recentlyClosed As Boolean = False

        Public Sub New()
            InitializeComponent()
            LoadBrands()
        End Sub

        Private Sub OpenAddBrand(sender As Object, e As RoutedEventArgs)
            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing Then Return

            If recentlyClosed Then
                recentlyClosed = False
                Return
            End If

            If popup IsNot Nothing AndAlso popup.IsOpen Then
                popup.IsOpen = False
                recentlyClosed = True
                Return
            End If

            popup = New Popup With {
                .PlacementTarget = clickedButton,
                .Placement = PlacementMode.Bottom,
                .StaysOpen = False,
                .AllowsTransparency = True
            }

            Dim addBrandWindow As New DPC.Components.Forms.AddBrand()

            ' Handle the BrandAdded event
            AddHandler addBrandWindow.BrandAdded, AddressOf OnBrandAdded

            popup.Child = addBrandWindow

            AddHandler popup.Closed, Sub()
                                         recentlyClosed = True
                                         Task.Delay(100).ContinueWith(Sub() recentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                     End Sub

            popup.IsOpen = True
        End Sub

        ' Callback to reload the brand data
        Private Sub OnBrandAdded()
            LoadBrands()
        End Sub

        Private Sub LoadBrands()
            Dim brands = BrandController.GetBrands()
            dataGrid.ItemsSource = brands
        End Sub
    End Class
End Namespace
