Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Helpers

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

        ' Static property to store variations data globally
        Private Shared _savedVariations As List(Of ProductVariation) = New List(Of ProductVariation)



        ' Save all variations data
        Public Shared Sub SaveVariations(MainVariationContainer As StackPanel)
            ' Clear the current saved variations
            _savedVariations.Clear()

            ' Loop through each variation panel in the UI
            For Each child As UIElement In MainVariationContainer.Children
                If TypeOf child Is StackPanel Then
                    Dim variationPanel As StackPanel = DirectCast(child, StackPanel)

                    ' Create new variation object
                    Dim variation As New ProductVariation With {
                .Options = New List(Of VariationOption)()
            }

                    ' Extract variation name
                    Dim nameBorder As Border = TryCast(variationPanel.Children(1), Border)
                    If nameBorder IsNot Nothing Then
                        Dim nameGrid As Grid = TryCast(nameBorder.Child, Grid)
                        If nameGrid IsNot Nothing Then
                            For Each gridChild As UIElement In nameGrid.Children
                                If TypeOf gridChild Is TextBox Then
                                    variation.VariationName = DirectCast(gridChild, TextBox).Text
                                    Exit For
                                End If
                            Next
                        End If
                    End If

                    ' Extract enabled image state
                    Dim toggleGrid As Grid = TryCast(variationPanel.Children(2), Grid)
                    If toggleGrid IsNot Nothing Then
                        For Each gridChild As UIElement In toggleGrid.Children
                            If TypeOf gridChild Is ToggleButton Then
                                variation.EnableImage = DirectCast(gridChild, ToggleButton).IsChecked
                                Exit For
                            End If
                        Next
                    End If

                    ' Extract options
                    Dim scrollViewer As ScrollViewer = TryCast(variationPanel.Children(4), ScrollViewer)
                    If scrollViewer IsNot Nothing Then
                        Dim optionsContainer As StackPanel = TryCast(scrollViewer.Content, StackPanel)
                        If optionsContainer IsNot Nothing Then
                            For Each optionChild As UIElement In optionsContainer.Children
                                If TypeOf optionChild Is Grid Then
                                    Dim optionGrid As Grid = DirectCast(optionChild, Grid)
                                    Dim optionName As String = String.Empty
                                    Dim imageData As ImageData = TryCast(optionGrid.Tag, ImageData)

                                    ' Find the text box for option name
                                    For Each gridChild As UIElement In optionGrid.Children
                                        If TypeOf gridChild Is TextBox Then
                                            Dim optionTextBox As TextBox = DirectCast(gridChild, TextBox)
                                            optionName = optionTextBox.Text
                                            Exit For
                                        End If
                                    Next

                                    ' Create new option with image data
                                    Dim opt As New VariationOption With {
                                .OptionName = optionName
                            }

                                    ' Add image data if available
                                    If imageData IsNot Nothing Then
                                        opt.ImageBase64 = imageData.Base64String
                                        opt.ImageFileName = imageData.FileName
                                        opt.ImageFileExtension = imageData.FileExtension
                                    End If

                                    ' Add to options list
                                    variation.Options.Add(opt)
                                End If
                            Next
                        End If
                    End If

                    ' Add the variation to the saved list
                    _savedVariations.Add(variation)
                End If
            Next

            ProductController.InitializeVariationCombinations()
        End Sub

    End Class
End Namespace
