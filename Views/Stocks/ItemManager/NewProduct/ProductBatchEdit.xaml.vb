Imports System.IO
Imports System.Windows.Controls.Primitives
Imports System.Windows.Threading
Imports DocumentFormat.OpenXml.Office.CustomUI
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports MaterialDesignThemes.Wpf
Imports MaterialDesignThemes.Wpf.Theme
Imports Microsoft.Win32

Namespace DPC.Views.Stocks.ItemManager.NewProduct
    Public Class ProductBatchEdit
        Inherits UserControl

        ' Event for when the save button is clicked
        Public Event SaveBatchEditClicked As RoutedEventHandler

        Public Sub New()
            InitializeComponent()

            ' No need for navigation components in a UserControl
            ProductController.GetWarehouse(ComboBoxWarehouse)

            Dim calendarViewModel As New CalendarController.SingleCalendar()
            Me.DataContext = calendarViewModel

            ProductController.TxtStockUnits = TxtStockUnits
            ProductController.BtnAddRow_Click(Nothing, Nothing)
            TxtDefaultTax.Text = 12
        End Sub

        ' Function to handle integer only input on textboxes
        Private Sub IntegerOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            If Not IsNumeric(e.Text) OrElse Not Integer.TryParse(e.Text, New Integer()) Then
                e.Handled = True ' Block non-integer input
            End If
        End Sub

        ' Function to handle pasting
        Private Sub IntegerOnlyPasteHandler(sender As Object, e As DataObjectPastingEventArgs)
            If e.DataObject.GetDataPresent(GetType(String)) Then
                Dim pastedText As String = CStr(e.DataObject.GetData(GetType(String)))
                If Not Integer.TryParse(pastedText, New Integer()) Then
                    e.CancelCommand() ' Cancel if pasted data is not an integer
                End If
            Else
                e.CancelCommand()
            End If
        End Sub

        ' Handles the serial table components
        Private Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs)
            ProductController.BtnAddRow_Click(Nothing, Nothing)
        End Sub

        Private Sub BtnRemoveRow_Click(sender As Object, e As RoutedEventArgs)
            ProductController.BtnRemoveRow_Click(Nothing, Nothing)
        End Sub

        Private Function FindParentGrid(element As DependencyObject) As Grid
            ' Traverse up the visual tree to find the Grid
            While element IsNot Nothing AndAlso TypeOf element IsNot Grid
                element = VisualTreeHelper.GetParent(element)
            End While
            Return TryCast(element, Grid)
        End Function

        ' Handle save button click
        Private Sub BtnSaveBatchEdit_Click(sender As Object, e As RoutedEventArgs)
            ' Raise the event so parent containers can handle the save action
            RaiseEvent SaveBatchEditClicked(Me, e)
        End Sub

        ' Public methods to access form values from parent containers
        Public Function GetFormData() As Dictionary(Of String, Object)
            ' Add all form values to the dictionary
            Dim formData As New Dictionary(Of String, Object) From {
                {"WarehouseId", ComboBoxWarehouse.SelectedValue},
                {"RetailPrice", If(String.IsNullOrEmpty(TxtRetailPrice.Text), 0, Convert.ToInt32(TxtRetailPrice.Text))},
                {"PurchaseOrder", If(String.IsNullOrEmpty(TxtPurchaseOrder.Text), 0, Convert.ToInt32(TxtPurchaseOrder.Text))},
                {"DefaultTax", If(String.IsNullOrEmpty(TxtDefaultTax.Text), 0, Convert.ToInt32(TxtDefaultTax.Text))},
                {"DiscountRate", If(String.IsNullOrEmpty(TxtDiscountRate.Text), 0, Convert.ToInt32(TxtDiscountRate.Text))},
                {"StockUnits", If(String.IsNullOrEmpty(TxtStockUnits.Text), 0, Convert.ToInt32(TxtStockUnits.Text))},
                {"AlertQuantity", If(String.IsNullOrEmpty(TxtAlertQuantity.Text), 0, Convert.ToInt32(TxtAlertQuantity.Text))}
            }

            Return formData
        End Function
    End Class
End Namespace