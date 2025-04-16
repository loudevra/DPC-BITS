Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Helpers

Namespace DPC.Data.Controllers
    Public Class RenderProduct
        Private Shared ReadOnly Property MainContainer As StackPanel
            Get
                Return ProductController.MainContainer
            End Get
        End Property
        Private Shared ReadOnly Property TxtStockUnits As TextBox
            Get
                Return ProductController.TxtStockUnits
            End Get
        End Property
        Private Shared ReadOnly Property SerialNumbers As List(Of TextBox)
            Get
                Return ProductController.SerialNumbers
            End Get
        End Property

        'Add serial number row
        Public Shared Sub AddSerialRow(sender As Object, e As RoutedEventArgs, Optional skipStockUpdate As Boolean = False)
            If MainContainer Is Nothing OrElse TxtStockUnits Is Nothing Then
                MessageBox.Show("MainContainer or TxtStockUnits is not initialized.")
                Return
            End If

            Dim outerStackPanel As New StackPanel With {.Margin = New Thickness(25, 20, 10, 20)}

            Dim grid As New Grid()
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            ' Access the named component using Application.Current.TryFindResource
            Dim textBox As New TextBox With {.Style = CType(Application.Current.TryFindResource("RoundedTextboxStyle"), Style)}
            SerialNumbers.Add(textBox)

            Dim textBoxBorder As New Border With {.Style = CType(Application.Current.TryFindResource("RoundedBorderStyle"), Style), .Child = textBox}
            Grid.SetColumn(textBoxBorder, 0)
            grid.Children.Add(textBoxBorder)

            Dim buttonPanel As New StackPanel With {.Orientation = Orientation.Horizontal, .Margin = New Thickness(10, 0, 0, 0)}

            ' Add Row Button
            Dim addRowButton As New Button With {.Background = Brushes.White, .BorderThickness = New Thickness(0), .Name = "BtnAddRow"}
            Dim addIcon As New MaterialDesignThemes.Wpf.PackIcon With {.Kind = MaterialDesignThemes.Wpf.PackIconKind.TableRowAddAfter, .Width = 40, .Height = 30, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#456B2E"))}
            addRowButton.Content = addIcon
            AddHandler addRowButton.Click, Sub(s, ev) AddSerialRow(s, ev)
            buttonPanel.Children.Add(addRowButton)

            ' Remove Row Button
            Dim removeRowButton As New Button With {.Background = Brushes.White, .BorderThickness = New Thickness(0), .Name = "BtnRemoveRow"}
            Dim removeIcon As New MaterialDesignThemes.Wpf.PackIcon With {.Kind = MaterialDesignThemes.Wpf.PackIconKind.TableRowRemove, .Width = 40, .Height = 30, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#D23636"))}
            removeRowButton.Content = removeIcon
            AddHandler removeRowButton.Click, Sub(s, ev) ProductController.BtnRemoveRow_Click(s, ev)
            buttonPanel.Children.Add(removeRowButton)

            ' Separator Border
            Dim separatorBorder As New Border With {.BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")), .BorderThickness = New Thickness(1), .Height = 30}
            buttonPanel.Children.Add(separatorBorder)

            ' Row Controller Button
            Dim rowControllerButton As New Button With {.Background = Brushes.White, .BorderThickness = New Thickness(0), .Name = "BtnRowController"}
            Dim menuIcon As New MaterialDesignThemes.Wpf.PackIcon With {.Kind = MaterialDesignThemes.Wpf.PackIconKind.MenuDown, .Width = 30, .Height = 30, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE"))}
            rowControllerButton.Content = menuIcon
            AddHandler rowControllerButton.Click, AddressOf ProductController.BtnRowController_Click
            buttonPanel.Children.Add(rowControllerButton)

            Grid.SetColumn(buttonPanel, 1)
            grid.Children.Add(buttonPanel)
            outerStackPanel.Children.Add(grid)

            MainContainer.Children.Add(outerStackPanel)

            ' Update TxtStockUnits value only if not skipped
            If Not skipStockUpdate Then
                Dim currentValue As Integer
                If Integer.TryParse(TxtStockUnits.Text, currentValue) Then
                    TxtStockUnits.Text = (currentValue + 1).ToString()
                Else
                    TxtStockUnits.Text = "1"
                End If
            End If
        End Sub

        'Open Row Controller Popup
        Public Shared Sub OpenRowController(sender As Object, e As RoutedEventArgs)
            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing Then Return

            ' Prevent reopening if the popup was just closed
            If ProductController.RecentlyClosed Then
                ProductController.RecentlyClosed = False
                Return
            End If

            ' If the popup exists and is open, close it
            If ProductController.popup IsNot Nothing AndAlso ProductController.popup.IsOpen Then
                ProductController.popup.IsOpen = False
                ProductController.RecentlyClosed = True
                Return
            End If

            ' Ensure the popup is only created once
            ProductController.popup = New Popup With {
            .PlacementTarget = clickedButton,
            .Placement = PlacementMode.Bottom,
            .StaysOpen = False,
            .AllowsTransparency = True
        }

            Dim popOutContent As New DPC.Components.Forms.RowControllerPopout()
            ProductController.popup.Child = popOutContent

            ' Handle popup closure
            AddHandler ProductController.popup.Closed, Sub()
                                                           ProductController.RecentlyClosed = True
                                                           Task.Delay(100).ContinueWith(Sub() ProductController.RecentlyClosed = False, TaskScheduler.FromCurrentSynchronizationContext())
                                                       End Sub

            ' Open the popup
            ProductController.popup.IsOpen = True
        End Sub

        'Clear fields in firstpage of add product
        Public Shared Sub ClearInputFieldsNoVariation(txtProductName As TextBox,
                         txtRetailPrice As TextBox,
                         txtPurchaseOrder As TextBox,
                         txtDefaultTax As TextBox,
                         txtDiscountRate As TextBox,
                         txtStockUnits As TextBox,
                         txtAlertQuantity As TextBox,
                         txtDescription As TextBox,
                         comboBoxCategory As ComboBox,
                         comboBoxSubCategory As ComboBox,
                         comboBoxWarehouse As ComboBox,
                         comboBoxMeasurementUnit As ComboBox,
                         comboBoxBrand As ComboBox,
                         comboBoxSupplier As ComboBox,
                         singleDatePicker As DatePicker,
                         mainContainer As Panel)
            ' Clear TextBoxes
            txtProductName.Clear()
            txtRetailPrice.Clear()
            txtPurchaseOrder.Clear()
            txtDefaultTax.Text = "12"
            txtDiscountRate.Clear()
            txtStockUnits.Text = "1"
            txtAlertQuantity.Clear()
            txtDescription.Clear()

            ' Reset ComboBoxes to first item (index 0)
            If comboBoxCategory.Items.Count > 0 Then comboBoxCategory.SelectedIndex = 0
            If comboBoxSubCategory.Items.Count > 0 Then comboBoxSubCategory.SelectedIndex = 0
            If comboBoxWarehouse.Items.Count > 0 Then comboBoxWarehouse.SelectedIndex = 0
            If comboBoxMeasurementUnit.Items.Count > 0 Then comboBoxMeasurementUnit.SelectedIndex = 0
            If comboBoxBrand.Items.Count > 0 Then comboBoxBrand.SelectedIndex = 0
            If comboBoxSupplier.Items.Count > 0 Then comboBoxSupplier.SelectedIndex = 0

            ' Set DatePicker to current date
            singleDatePicker.SelectedDate = DateTime.Now

            ' Clear Serial Numbers and reset to one row
            If SerialNumbers IsNot Nothing Then
                SerialNumbers.Clear()
            End If

            If mainContainer IsNot Nothing Then
                mainContainer.Children.Clear()
            End If

            ' Add back one row for Serial Number input
            ProductController.BtnAddRow_Click(Nothing, Nothing)
        End Sub
    End Class
End Namespace
