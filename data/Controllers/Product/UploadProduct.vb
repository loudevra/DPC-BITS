Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Views.Stocks.ItemManager.NewProduct

Namespace DPC.Data.Controllers
    Public Class UploadProduct
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

        'function to import serialnumber
        Public Shared Sub ImportSerialNumbers()
            If ProductController.IsVariation = False Then
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

            ElseIf ProductController.IsVariation = True Then
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

                        ' Find the current ProductVariationDetails instance
                        Dim currentWindow = Application.Current.Windows.OfType(Of ProductVariationDetails)().FirstOrDefault()

                        If currentWindow IsNot Nothing Then
                            ' Get the container that holds all serial number rows
                            Dim containerBorder As Border = TryCast(currentWindow.StackPanelSerialRow.Children(1), Border)
                            If containerBorder IsNot Nothing Then
                                Dim scrollViewer As ScrollViewer = TryCast(containerBorder.Child, ScrollViewer)
                                If scrollViewer IsNot Nothing Then
                                    Dim container = TryCast(scrollViewer.Content, StackPanel)

                                    If container IsNot Nothing Then
                                        ' Process serial numbers
                                        Dim serialNumbersData As New List(Of String)
                                        For Each line In lines
                                            Dim serialNumber As String = line.Trim()
                                            If Not String.IsNullOrWhiteSpace(serialNumber) Then
                                                serialNumbersData.Add(serialNumber)
                                            End If
                                        Next

                                        If serialNumbersData.Any() Then
                                            ' Clear existing rows first
                                            container.Children.Clear()
                                            ProductController.serialNumberTextBoxes.Clear()

                                            ' Update the current variation data
                                            Dim currentData = ProductController.variationManager.GetCurrentVariationData()
                                            If currentData IsNot Nothing Then
                                                ' Update stock units and serial numbers in the data model
                                                currentData.StockUnits = serialNumbersData.Count
                                                currentData.SerialNumbers = New List(Of String)(serialNumbersData)
                                                currentData.IncludeSerialNumbers = True

                                                ' Make sure checkbox is checked
                                                currentWindow.CheckBoxSerialNumber.IsChecked = True
                                                currentWindow.UpdateSerialNumberPanelVisibility()
                                            End If

                                            ' Add all the new rows from CSV - and ONLY these rows
                                            For i As Integer = 0 To serialNumbersData.Count - 1
                                                currentWindow.AddSerialNumberRow(container, i + 1)

                                                ' Find and set the textbox text
                                                If i < container.Children.Count Then
                                                    Dim rowPanel = TryCast(container.Children(i), StackPanel)
                                                    If rowPanel IsNot Nothing Then
                                                        Dim grid = TryCast(rowPanel.Children(0), Grid)
                                                        If grid IsNot Nothing Then
                                                            Dim border = TryCast(grid.Children(0), Border)
                                                            If border IsNot Nothing Then
                                                                Dim textBox = TryCast(border.Child, TextBox)
                                                                If textBox IsNot Nothing Then
                                                                    textBox.Text = serialNumbersData(i)
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            Next

                                            ' Update stock units text AFTER adding all rows, to avoid triggering events
                                            currentWindow.TxtStockUnits.Text = serialNumbersData.Count.ToString()

                                            ' Scroll to show the first imported serial number
                                            scrollViewer.ScrollToTop()

                                            MessageBox.Show($"Successfully imported {serialNumbersData.Count} serial numbers.")
                                        Else
                                            MessageBox.Show("No valid serial numbers found.")
                                        End If
                                    End If
                                End If
                            End If
                        Else
                            MessageBox.Show("ProductVariationDetails window not found.")
                        End If
                    Catch ex As Exception
                        MessageBox.Show($"Error importing serial numbers: {ex.Message}")
                    End Try
                End If
            End If


        End Sub
    End Class
End Namespace
