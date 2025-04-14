Imports DPC.DPC.Data.Helpers

Namespace DPC.Data.Controllers
    Public Class DeleteProductData
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

        'Function to call when removing a row via button
        Public Shared Sub RemoveSerialRow(sender As Object, e As RoutedEventArgs)
            If MainContainer Is Nothing OrElse TxtStockUnits Is Nothing Then
                MessageBox.Show("MainContainer or TxtStockUnits is not initialized.")
                Return
            End If

            Dim button As Button = CType(sender, Button)
            Dim parentGrid As Grid = FindParentGrid.FindParentGrid(button)

            If parentGrid IsNot Nothing Then
                Dim parentStackPanel As StackPanel = TryCast(parentGrid.Parent, StackPanel)

                ' Check the current value of TxtStockUnits
                Dim currentValue As Integer
                If Integer.TryParse(TxtStockUnits.Text, currentValue) Then
                    ' If the current value is 1, stop execution
                    If currentValue = 1 Then Return
                End If

                ' Find the TextBox inside the grid and remove it from SerialNumbers
                Dim serialTextBox As TextBox = TryCast(parentGrid.Children.OfType(Of TextBox)().FirstOrDefault(), TextBox)
                If serialTextBox IsNot Nothing AndAlso SerialNumbers.Contains(serialTextBox) Then
                    SerialNumbers.Remove(serialTextBox)
                End If

                ' Remove the row from MainContainer
                If parentStackPanel IsNot Nothing AndAlso MainContainer.Children.Contains(parentStackPanel) Then
                    MainContainer.Children.Remove(parentStackPanel)

                    ' Update TxtStockUnits only if the value is greater than 1
                    TxtStockUnits.Text = Math.Max(currentValue - 1, 0).ToString()
                End If
            End If
        End Sub

        'Function to remove the latest row
        Public Shared Sub RemoveLatestRow()
            Dim parentPanel As StackPanel = MainContainer

            ' Check if there is only one row left
            If parentPanel IsNot Nothing AndAlso parentPanel.Children.Count = 1 Then
                MessageBox.Show("Cannot remove the last remaining row.")
                Return
            End If

            ' Proceed to remove the latest row if there are multiple rows
            If parentPanel IsNot Nothing AndAlso parentPanel.Children.Count > 0 Then
                ' Find the latest row
                Dim latestStackPanel As StackPanel = TryCast(parentPanel.Children(parentPanel.Children.Count - 1), StackPanel)

                ' Remove the TextBox from SerialNumbers if it exists
                If latestStackPanel IsNot Nothing Then
                    Dim latestTextBox As TextBox = latestStackPanel.Children.OfType(Of TextBox)().FirstOrDefault()
                    If latestTextBox IsNot Nothing AndAlso SerialNumbers.Contains(latestTextBox) Then
                        SerialNumbers.Remove(latestTextBox)
                    End If
                End If

                ' Remove the row
                parentPanel.Children.RemoveAt(parentPanel.Children.Count - 1)
            Else
                MessageBox.Show("No rows available to remove.")
            End If
        End Sub


    End Class
End Namespace
