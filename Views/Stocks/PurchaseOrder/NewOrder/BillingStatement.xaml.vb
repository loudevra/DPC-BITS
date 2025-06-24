Imports System.Collections.ObjectModel
Imports System.IO
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Model
Imports MaterialDesignThemes.Wpf
Imports Microsoft.Win32
Imports NuGet.Protocol.Plugins

Namespace DPC.Views.Stocks.PurchaseOrder.NewOrder
    Public Class BillingStatement

        Private base64Image As String
        Private itemDataSource As New ObservableCollection(Of OrderItems)
        Private checkingDataSource As New ObservableCollection(Of Checker)
        Private itemOrder As New List(Of Dictionary(Of String, String))


        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            InvoiceNumber.Text = StatementDetails.InvoiceNumberCache
            InvoiceDate.Text = StatementDetails.InvoiceDateCache
            DueDate.Text = StatementDetails.DueDateCache
            Tax.Text = StatementDetails.TaxCache
            TotalCost.Text = StatementDetails.TotalCostCache
            itemOrder = StatementDetails.OrderItemsCache

            For Each item In StatementDetails.OrderItemsCache

                itemDataSource.Add(New OrderItems With {
                    .Quantity = item("Quantity"),
                    .Description = item("ItemName"),
                    .UnitPrice = item("Rate"),
                    .LinePrice = item("Price")
                })
            Next

            checkingDataSource.Add(New Checker With {
                    .SalesRep = CacheOnLoggedInName
                })

            checkingGrid.ItemsSource = checkingDataSource
            dataGrid.ItemsSource = itemDataSource

            AddHandler BrowseFile.MouseLeftButtonUp, AddressOf OpenFiles
            AddHandler BackBtn.MouseLeftButtonUp, Sub()
                                                      ViewLoader.DynamicView.NavigateToView("neworder", Me)
                                                  End Sub
        End Sub

#Region "Signature Upload"
        Private Sub OpenFiles()

            Dim openFileDialog As New OpenFileDialog With {
                .Filter = "Image Files|*.jpg;*.jpeg;*.png",
                .Title = "Select an Image"
            }

            If openFileDialog.ShowDialog() = True Then
                Dim filePath As String = openFileDialog.FileName

                If LogicProduct.ValidateImageFile(filePath) Then
                    StartFileUpload(filePath)
                End If
            End If
        End Sub

        Private Sub StartFileUpload(filePath As String)
            '' Reset upload progress
            'UploadProgressBar.Value = 0
            'UploadStatus.Text = "Uploading..."

            ' Update file info
            Dim fileInfo As New FileInfo(filePath)
            Dim fileSizeText As String = Base64Utility.GetReadableFileSize(fileInfo.Length)

            'ImgName.Text = Path.GetFileName(filePath)
            'ImgSize.Text = fileSizeText

            ' Convert image to Base64 using Base64Utility
            Try
                base64Image = Base64Utility.EncodeFileToBase64(filePath)
            Catch ex As Exception
                MessageBox.Show("Error encoding image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Exit Sub
            End Try

            '' Show the panel with image info
            'ImageInfoPanel.Visibility = Visibility.Visible

            '' Disable browse button and drag-drop functionality
            'BtnBrowse.IsEnabled = False
            'DropBorder.AllowDrop = False
            'isUploadLocked = True

            '' Configure and start the timer
            'ConfigureUploadTimer()

            DisplayUploadedImage()
        End Sub

        Private Sub DisplayUploadedImage()
            Try
                Dim tempImagePath As String = Path.Combine(Path.GetTempPath(), "decoded_image.png")

                ' Clean up previous image file
                If File.Exists(tempImagePath) Then
                    GC.Collect()
                    GC.WaitForPendingFinalizers()
                    File.Delete(tempImagePath)
                End If

                ' Decode and save new image
                Base64Utility.DecodeBase64ToFile(base64Image, tempImagePath)

                ' Load image safely
                Dim imageSource As New BitmapImage()
                Using stream As New FileStream(tempImagePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    imageSource.BeginInit()
                    imageSource.CacheOption = BitmapCacheOption.OnLoad
                    imageSource.StreamSource = stream
                    imageSource.EndInit()
                End Using
                imageSource.Freeze() ' Allow image to be accessed in different threads

                BrowseFile.Child = Nothing

                Dim imagePreview As New Image()
                imagePreview.Source = imageSource
                imagePreview.MaxHeight = 50

                BrowseFile.Child = imagePreview

                '' Set the image source
                'UploadedImage.Source = imageSource
            Catch ex As Exception
                MessageBox.Show("Error decoding image: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
#End Region

#Region "Client File Upload"
        Private Sub OpenClientFiles()

            Dim openFileDialog As New OpenFileDialog With {
                .Filter = "Supported Files|*.jpg;*.jpeg;*.png;*.gif;*.docx;*.docs;*.txt;*.xls;*.xlsx;*.pdf",
                .Title = "Select an Image"
            }

            If openFileDialog.ShowDialog() = True Then
                Dim filePath As String = openFileDialog.FileName
                Dim fileInfo As New FileInfo(filePath)

                If ValidateClientFiles(filePath) Then
                    Dim fileSize As Double = Math.Round(fileInfo.Length / (1024 * 1024), 2)

                    Dim border As Border = CreateFilePreview(fileInfo.Name, fileSize)

                    ClientFiles.Children.Add(border)
                End If
            End If
        End Sub

        Public Shared Function ValidateClientFiles(filePath As String) As Boolean
            Dim fileInfo As New FileInfo(filePath)

            ' Check file size (2MB max)
            If fileInfo.Length > 2 * 1024 * 1024 Then
                MessageBox.Show("File is too large! Please upload an image under 2MB.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End If

            ' Check file extension
            Dim validExtensions As String() = {".jpg", ".jpeg", ".png", ".gif", ".docx", ".docs", ".txt", ".xls", ".xlsx", ".pdf"}
            If Not validExtensions.Contains(fileInfo.Extension.ToLower()) Then
                MessageBox.Show("Invalid file format!", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End If

            Return True
        End Function

        Private Sub Border_DragOver(sender As Object, e As DragEventArgs)
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                e.Effects = DragDropEffects.Copy
            Else
                e.Effects = DragDropEffects.None
            End If
            e.Handled = True
        End Sub

        Private Sub Border_Drop(sender As Object, e As DragEventArgs)
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())


                For Each file In files

                    Dim fileInfo As New FileInfo(file)

                    If ValidateClientFiles(file) Then
                        Dim fileSize As Double = Math.Round(fileInfo.Length / (1024 * 1024), 2)

                        Dim border As Border = CreateFilePreview(fileInfo.Name, fileSize)

                        ClientFiles.Children.Add(border)
                    End If

                Next
            End If
        End Sub

        Private Function CreateFilePreview(fileName As String, fileSizeMB As Double) As Border
            ' Outer Border
            Dim border As New Border With {
        .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")),
        .CornerRadius = New CornerRadius(3),
        .Margin = New Thickness(0, 5, 0, 0)
    }

            ' StackPanel container
            Dim stackPanel As New StackPanel With {
        .Orientation = Orientation.Vertical,
        .HorizontalAlignment = HorizontalAlignment.Stretch,
        .Margin = New Thickness(5)
    }

            ' Grid
            Dim grid As New Grid()
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            grid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = GridLength.Auto})

            ' PDF Icon
            Dim pdfIcon As New PackIcon With {
        .Kind = PackIconKind.FilePdfBox,
        .Width = 20,
        .Height = 20,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#090909")),
        .Margin = New Thickness(0, 0, 5, 0),
        .VerticalAlignment = VerticalAlignment.Center
    }
            Grid.SetColumn(pdfIcon, 0)

            ' File Info StackPanel
            Dim fileInfoPanel As New StackPanel With {
        .Orientation = Orientation.Vertical,
        .VerticalAlignment = VerticalAlignment.Center
    }
            Grid.SetColumn(fileInfoPanel, 1)

            ' File Name Text
            Dim fileNameText As New TextBlock With {
        .Text = fileName,
        .FontWeight = FontWeights.SemiBold,
        .FontFamily = New FontFamily("Lexend"),
        .FontSize = 6,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#090909"))
    }

            ' File Size Text
            Dim fileSizeText As New TextBlock With {
        .Text = $"{Math.Round(fileSizeMB, 2)} mb",
        .FontWeight = FontWeights.SemiBold,
        .FontFamily = New FontFamily("Lexend"),
        .FontSize = 6,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#555555"))
    }

            fileInfoPanel.Children.Add(fileNameText)
            fileInfoPanel.Children.Add(fileSizeText)

            ' Trash Icon
            Dim trashIcon As New PackIcon With {
        .Kind = PackIconKind.TrashCanOutline,
        .Width = 20,
        .Height = 20,
        .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#C75757")),
        .Margin = New Thickness(5, 0, 0, 0),
        .Background = New SolidColorBrush(ColorConverter.ConvertFromString("#AEAEAE")),
        .VerticalAlignment = VerticalAlignment.Center,
        .HorizontalAlignment = HorizontalAlignment.Right,
        .Cursor = Cursors.Hand
    }
            AddHandler trashIcon.MouseLeftButtonUp, Sub()
                                                        ClientFiles.Children.Remove(border)
                                                    End Sub

            Grid.SetColumn(trashIcon, 2)

            ' Combine into Grid
            grid.Children.Add(pdfIcon)
            grid.Children.Add(fileInfoPanel)
            grid.Children.Add(trashIcon)

            ' Add grid to stack panel and stack panel to border
            stackPanel.Children.Add(grid)
            border.Child = stackPanel

            Return border
        End Function
#End Region

#Region "Navigation"
        Private Sub PrintPreview(sender As Object, e As RoutedEventArgs)
            StatementDetails.signature = If(String.IsNullOrWhiteSpace(base64Image), False, True)
            StatementDetails.InvoiceNumberCache = InvoiceNumber.Text
            StatementDetails.InvoiceDateCache = InvoiceDate.Text
            StatementDetails.DueDateCache = DueDate.Text
            StatementDetails.TaxCache = Tax.Text
            StatementDetails.TotalCostCache = TotalCost.Text
            StatementDetails.OrderItemsCache = itemOrder

            ViewLoader.DynamicView.NavigateToView("printpreview", Me)
        End Sub

#End Region

    End Class
End Namespace