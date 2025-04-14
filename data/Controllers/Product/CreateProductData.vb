Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Helpers

Namespace DPC.Data.Controllers
    Public Class CreateProductData
        ' Function to generate ProductCode in format 20MMDDYYYYXXXX
        Public Shared Function GenerateProductCode() As String
            Dim prefix As String = "20"
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy") ' MMDDYYYY format
            Dim counter As Integer = GetNextProductCounter(datePart)

            ' Format counter to be 4 digits (e.g., 0001, 0025, 0150)
            Dim counterPart As String = counter.ToString("D4")

            ' Concatenate to get full ProductCode
            Return prefix & datePart & counterPart
        End Function

        ' Function to get the next Product counter (last 4 digits) with reset condition
        Public Shared Function GetNextProductCounter(datePart As String) As Integer
            Dim query As String = "SELECT MAX(CAST(SUBSTRING(productID, 11, 4) AS UNSIGNED)) FROM product " &
                  "WHERE productID LIKE '20" & datePart & "%'"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Dim result As Object = cmd.ExecuteScalar()

                        ' If no previous records exist for today, start with 0001
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            Return Convert.ToInt32(result) + 1
                        Else
                            Return 1
                        End If
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error generating Product Code: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return 1
                End Try
            End Using
        End Function

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
            AddHandler rowControllerButton.Click, AddressOf OpenRowController
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
        'Open row controller
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

        'function to import serialnumber
        Public Shared Sub ImportSerialNumbers()
            Dim openFileDialog As New Microsoft.Win32.OpenFileDialog With {
                .Filter = "CSV Files (*.csv)|*.csv"
            }

            If openFileDialog.ShowDialog() = True Then
                Try
                    Dim filePath As String = openFileDialog.FileName
                    Dim lines As List(Of String) = IO.File.ReadAllLines(filePath).ToList()

                    ' Ensure there are serial numbers available
                    If lines.Count <= 1 Then
                        MessageBox.Show("No serial numbers found in the file.")
                        Return
                    End If

                    ' Remove the first line (title "Serial Number") and read the rest
                    lines.RemoveAt(0)

                    ' Clear existing rows if any
                    MainContainer.Children.Clear()
                    SerialNumbers.Clear()

                    ' Process serial numbers
                    Dim serialNumbersData As New List(Of String)
                    For Each line In lines
                        Dim serialNumber As String = line.Trim()
                        If Not String.IsNullOrWhiteSpace(serialNumber) Then
                            serialNumbersData.Add(serialNumber)
                        End If
                    Next

                    If serialNumbersData.Any() Then
                        ' Update stock units once
                        TxtStockUnits.Text = serialNumbersData.Count.ToString()

                        ' Dynamically add rows for each serial number
                        For Each serialNumber In serialNumbersData
                            ProductController.BtnAddRow_Click(Nothing, Nothing, True)
                            If SerialNumbers.Count > 0 Then
                                SerialNumbers.Last().Text = serialNumber
                            End If
                        Next

                        'MessageBox.Show($"Successfully imported {serialNumbersData.Count} serial numbers.")
                    Else
                        MessageBox.Show("No valid serial numbers found.")
                    End If

                Catch ex As Exception
                    MessageBox.Show($"Error importing serial numbers: {ex.Message}")
                End Try
            End If
        End Sub
    End Class
End Namespace
