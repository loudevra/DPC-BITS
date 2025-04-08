Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Controllers.CalendarController

Namespace DPC.Views.Stocks.PurchaseOrder.NewOrder
    Public Class NewOrder
        Private rowCount As Integer = 0
        Private MyDynamicGrid As Grid

        Public Property OrderDate As New CalendarController.SingleCalendar()
        Public Property OrderDueDate As New CalendarController.SingleCalendar()

        Public Sub New()
            InitializeComponent()

            SidebarContainer.Content = New Components.Navigation.Sidebar()
            TopNavBarContainer.Content = New Components.Navigation.TopNavBar()
            OrderDate.SelectedDate = Date.Today
            OrderDueDate.SelectedDate = Date.Today.AddDays(1)

            DataContext = Me

            MyDynamicGrid = CType(TableGridPanel.Children(0), Grid)
            AddNewRow()
        End Sub

        Private Sub OrderDate_Click(sender As Object, e As RoutedEventArgs)
            OrderDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub OrderDueDate_Click(sender As Object, e As RoutedEventArgs)
            OrderDueDatePicker.IsDropDownOpen = True
        End Sub

        ' ➜ Add a New Row
        Private Sub AddNewRow()
            rowCount += 1
            Dim row1 As New RowDefinition() With {.Height = GridLength.Auto}
            Dim row2 As New RowDefinition() With {.Height = GridLength.Auto}

            MyDynamicGrid.RowDefinitions.Add(row1)
            MyDynamicGrid.RowDefinitions.Add(row2)

            Dim newRowStart As Integer = MyDynamicGrid.RowDefinitions.Count - 2

            ' Create UI Elements (Matches XAML Layout)
            CreateTextBox(newRowStart, 0) ' Item Name
            CreateTextBox(newRowStart, 1) ' Quantity
            CreateTextBox(newRowStart, 2) ' Rate
            CreateTextBox(newRowStart, 3) ' Tax %
            CreateTextBox(newRowStart, 4) ' Tax (Editable)
            CreateTextBox(newRowStart, 5) ' Discount
            CreateTextBox(newRowStart, 6) ' Amount (Editable)
            CreateDeleteButton(newRowStart, 7)
            CreateFullWidthTextBox(newRowStart)
        End Sub

        ' ➜ Create TextBox
        Private Sub CreateTextBox(row As Integer, column As Integer)
            Dim txtName As String = $"txt_{row}_{column}"

            ' Check if the name already exists and unregister it
            Dim existingElement As Object = Me.FindName(txtName)
            If existingElement IsNot Nothing Then
                Me.UnregisterName(txtName)
            End If

            ' Create TextBox
            ' Apply TextBox style dynamically
            Dim txt As New TextBox With {
                .Name = txtName,
                .Style = CType(Me.FindResource("RoundedTextboxStyle"), Style)
            }

            ' Attach numeric validation and event handlers
            If column = 1 Or column = 2 Or column = 3 Or column = 4 Or column = 5 Or column = 6 Then
                AddHandler txt.PreviewTextInput, AddressOf ValidateNumericInput
                AddHandler txt.TextChanged, AddressOf TextBoxValueChanged
            End If

            ' Create a Border and apply the style
            ' Wrap TextBox inside the Border
            Dim border As New Border With {
                .Margin = New Thickness(5, 5, 5, 5), ' Custom margin to maintain spacing
                .Style = CType(Me.FindResource("RoundedBorderStyle"), Style),
                .Child = txt
            }

            ' Set Grid position
            Grid.SetRow(border, row)
            Grid.SetColumn(border, column)

            ' Register the Border
            Me.RegisterName(txtName, border)

            ' Add to Grid
            MyDynamicGrid.Children.Add(border)
        End Sub

        ' Function to create a full-width TextBox inside a Border
        Private Sub CreateFullWidthTextBox(row As Integer)
            Dim txtName As String = $"txt_full_{row}"

            ' Check if the name already exists and unregister it
            Dim existingElement As Object = Me.FindName(txtName)
            If existingElement IsNot Nothing Then
                Me.UnregisterName(txtName)
            End If

            ' Create TextBox
            ' Apply the existing TextBox style
            Dim fullWidthTextBox As New TextBox With {
                .Name = txtName,
.Height = 60, ' Adjust height for full-width text box
.VerticalContentAlignment = VerticalAlignment.Top,
.Padding = New Thickness(0, 5, 0, 0),
                .Style = CType(Me.FindResource("RoundedTextboxStyle"), Style)
            }

            ' Create a Border and apply the existing style
            ' Wrap TextBox inside the Border
            Dim border As New Border With {
                .Margin = New Thickness(5, 5, 5, 5), ' Consistent margin for spacing
                .Style = CType(Me.FindResource("RoundedBorderStyle"), Style),
                .Child = fullWidthTextBox
            }

            ' Set Grid position
            Grid.SetRow(border, row + 1) ' Place it in the next row
            Grid.SetColumn(border, 0)
            Grid.SetColumnSpan(border, 8) ' Span across all columns

            ' Register the Border
            Me.RegisterName(txtName, border)

            ' Add to Grid
            MyDynamicGrid.Children.Add(border)
        End Sub

        ' ➜ Create TextBlock
        Private Sub CreateTextBlock(row As Integer, column As Integer, text As String)
            Dim txtBlock As New TextBlock() With {
                .Text = text,
                .FontFamily = New FontFamily("Lexend"),
                .HorizontalAlignment = HorizontalAlignment.Center,
                .VerticalAlignment = VerticalAlignment.Center
            }
            Grid.SetRow(txtBlock, row)
            Grid.SetColumn(txtBlock, column)
            MyDynamicGrid.Children.Add(txtBlock)
        End Sub

        ' ➜ Create Delete Button
        Private Sub CreateDeleteButton(row As Integer, column As Integer)
            Dim btn As New Button() With {
        .Width = Double.NaN,
        .Height = 30,
        .HorizontalAlignment = HorizontalAlignment.Center,
        .VerticalAlignment = VerticalAlignment.Center,
        .BorderThickness = New Thickness(0)
    }

            Dim stack As New StackPanel() With {
        .Orientation = Orientation.Horizontal,
        .HorizontalAlignment = HorizontalAlignment.Center
    }

            Dim icon As New MaterialDesignThemes.Wpf.PackIcon() With {
        .Kind = MaterialDesignThemes.Wpf.PackIconKind.PlaylistRemove,
        .Foreground = Brushes.Red,
        .Width = 30,
        .Height = 30,
        .Margin = New Thickness(0, 0, 5, 0)
    }

            stack.Children.Add(icon)
            btn.Content = stack

            ' Attach delete functionality
            AddHandler btn.Click, Sub(sender As Object, e As RoutedEventArgs)
                                      Dim parentRow As Integer = Grid.GetRow(CType(sender, Button))
                                      RemoveRow(parentRow)
                                  End Sub

            Grid.SetRow(btn, row)
            Grid.SetColumn(btn, column)
            MyDynamicGrid.Children.Add(btn)
        End Sub

        'Check if entered text is a number
        Private Sub ValidateNumericInput(sender As Object, e As TextCompositionEventArgs)
            Dim allowedPattern As String = "^[0-9.]+$" ' Allow only digits and decimal point
            If Not System.Text.RegularExpressions.Regex.IsMatch(e.Text, allowedPattern) Then
                e.Handled = True ' Reject input if it doesn't match
            End If
        End Sub





        ' Remove Row Functionality
        Private Sub RemoveRow(row As Integer)
            ' Find all elements in the specified row and the corresponding note row
            Dim elementsToRemove As New List(Of UIElement)

            For Each element As UIElement In MyDynamicGrid.Children
                If Grid.GetRow(element) = row Or Grid.GetRow(element) = row + 1 Then
                    elementsToRemove.Add(element)
                End If
            Next

            ' Unregister names and remove elements from the grid
            For Each element As UIElement In elementsToRemove
                If TypeOf element Is TextBox Then
                    Dim txtBox As TextBox = CType(element, TextBox)
                    If Not String.IsNullOrEmpty(txtBox.Name) AndAlso Me.FindName(txtBox.Name) IsNot Nothing Then
                        Try
                            Me.UnregisterName(txtBox.Name)
                        Catch ex As ArgumentException
                            ' Ignore error if the name is already unregistered
                        End Try
                    End If
                End If
                MyDynamicGrid.Children.Remove(element)
            Next

            ' Remove the corresponding RowDefinitions (ensure valid index before removing)
            If row < MyDynamicGrid.RowDefinitions.Count Then
                MyDynamicGrid.RowDefinitions.RemoveAt(row)
                If row < MyDynamicGrid.RowDefinitions.Count Then
                    MyDynamicGrid.RowDefinitions.RemoveAt(row) ' Remove note row as well
                End If
            End If

            ' Adjust remaining rows (shift everything above the deleted row)
            Dim elementsToShift As New List(Of UIElement)
            For Each element As UIElement In MyDynamicGrid.Children
                Dim currentRow As Integer = Grid.GetRow(element)
                If currentRow > row Then
                    elementsToShift.Add(element)
                End If
            Next

            For Each element As UIElement In elementsToShift
                Grid.SetRow(element, Grid.GetRow(element) - 2)
            Next

            ' Reduce row count
            rowCount -= 1
        End Sub


        Private Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs) Handles btnAddRow.Click
            AddNewRow()
        End Sub

        ' Function to retrieve the TextBox from a Border element
        Private Function GetTextBoxFromBorder(borderName As String) As TextBox
            Dim border As Border = TryCast(Me.FindName(borderName), Border)
            If border IsNot Nothing AndAlso TypeOf border.Child Is TextBox Then
                Return CType(border.Child, TextBox)
            End If
            Return Nothing
        End Function

        Private Sub UpdateTaxAndAmount(row As Integer)
            ' Get references to the required textboxes inside their borders
            Dim quantityTxt As TextBox = GetTextBoxFromBorder($"txt_{row}_1")
            Dim rateTxt As TextBox = GetTextBoxFromBorder($"txt_{row}_2")
            Dim taxPercentTxt As TextBox = GetTextBoxFromBorder($"txt_{row}_3")
            Dim taxTxt As TextBox = GetTextBoxFromBorder($"txt_{row}_4") ' Now a TextBox
            Dim discountTxt As TextBox = GetTextBoxFromBorder($"txt_{row}_5")
            Dim amountTxt As TextBox = GetTextBoxFromBorder($"txt_{row}_6") ' Now a TextBox

            ' Ensure all fields have valid numeric input
            Dim quantity As Integer = If(quantityTxt IsNot Nothing AndAlso Integer.TryParse(quantityTxt.Text, quantity), quantity, 0)
            Dim rate As Single = If(rateTxt IsNot Nothing AndAlso Single.TryParse(rateTxt.Text, rate), rate, 0)
            Dim taxPercent As Single = If(taxPercentTxt IsNot Nothing AndAlso Single.TryParse(taxPercentTxt.Text, taxPercent), taxPercent, 0)
            Dim discount As Single = If(discountTxt IsNot Nothing AndAlso Single.TryParse(discountTxt.Text, discount), discount, 0)

            ' Calculate subtotal (Quantity × Rate)
            Dim subtotal As Single = quantity * rate

            ' Compute tax (Tax % of subtotal)
            Dim taxAmount As Single = subtotal * (taxPercent / 100)

            ' Update the Tax field dynamically
            If taxTxt IsNot Nothing Then
                taxTxt.Text = taxAmount.ToString("0.00")
            End If

            ' Calculate total amount (Subtotal + Tax - Discount)
            Dim totalAmount As Single = subtotal + taxAmount - discount

            ' Ensure the total amount is not negative
            If totalAmount < 0 Then totalAmount = 0

            ' Update the Amount field dynamically
            If amountTxt IsNot Nothing Then
                amountTxt.Text = totalAmount.ToString("0.00")
            End If
        End Sub

        Private Sub TextBoxValueChanged(sender As Object, e As TextChangedEventArgs)
            Dim txt As TextBox = CType(sender, TextBox)

            ' Extract row number from textbox name (e.g., "txt_2_1" → row 2)
            Dim parts As String() = txt.Name.Split("_"c)
            If parts.Length < 3 Then Exit Sub

            Dim row As Integer
            If Integer.TryParse(parts(1), row) Then
                UpdateTaxAndAmount(row) ' Call function to update tax and amount
            End If
        End Sub

        Private Sub BtnAddSupplier_Click(sender As Object, e As RoutedEventArgs) Handles btnAddSupplier.Click
            DPC.Data.Helpers.DynamicView.NavigateToView("newsuppliers", Me)
        End Sub
    End Class
End Namespace
