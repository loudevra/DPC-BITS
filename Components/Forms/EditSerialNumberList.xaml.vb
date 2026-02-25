Imports System.Text.RegularExpressions
Imports System.Windows.Controls
Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient

Namespace DPC.Components.Forms
    Public Class EditSerialNumberList
        Public Property SerialResult As String = ""
        Public Property ListLength As Integer
        Public Property IsSaved As Boolean = False
        Private _productName As String
        Private _maxQty As Integer
        Private _serialsList As String

        Public Sub New(productName As String, currentSerials As String, maxQty As Integer)
            InitializeComponent()
            _productName = productName
            _maxQty = maxQty
            _serialsList = currentSerials

            LoadList(_maxQty)
        End Sub

        Private Sub LoadList(qty As Integer)
            SerialNumberList.Children.Clear()

            For i As Integer = 0 To qty - 1
                Dim outerGrid As New Grid() With {.Margin = New Thickness(0, 0, 0, 10)}
                outerGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = GridLength.Auto})
                outerGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Star)})

                ' --- Left Side: Index Number ---
                Dim borderLeft As New Border With {
                    .BorderBrush = Brushes.Black, .BorderThickness = New Thickness(2),
                    .CornerRadius = New CornerRadius(5), .Margin = New Thickness(0, 0, 5, 0),
                    .VerticalAlignment = VerticalAlignment.Center
                }
                Dim txtIndex As New TextBox With {
                    .Text = (i + 1).ToString(), .IsReadOnly = True,
                    .Style = CType(Me.FindResource("RoundedTextboxStyle"), Style),
                    .FontFamily = New FontFamily("Lexend"), .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#555555")),
                    .FontWeight = FontWeights.SemiBold
                }
                borderLeft.Child = txtIndex
                Grid.SetColumn(borderLeft, 0)


                Dim borderRight As New Border With {
                    .BorderBrush = Brushes.Black, .BorderThickness = New Thickness(2),
                    .CornerRadius = New CornerRadius(5), .Margin = New Thickness(5, 0, 0, 0)
                }

                Dim innerGrid As New Grid()
                innerGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Star)})
                innerGrid.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = GridLength.Auto})

                Dim txtSerial As New TextBox With {
                    .Name = $"txtSerialList_{i}",
                    .Style = CType(Me.FindResource("RoundedTextboxStyle"), Style),
                    .FontFamily = New FontFamily("Lexend"),
                    .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#555555")),
                    .FontWeight = FontWeights.SemiBold,
                    .VerticalContentAlignment = VerticalAlignment.Center
                }

                If Me.FindName(txtSerial.Name) IsNot Nothing Then Me.UnregisterName(txtSerial.Name)
                Me.RegisterName(txtSerial.Name, txtSerial)

                Dim btnClear As New Button With {
                    .Style = CType(Me.FindResource("MaterialDesignIconButton"), Style),
                    .Width = 30, .Height = 30, .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#C75757"))
                }
                btnClear.Content = New MaterialDesignThemes.Wpf.PackIcon With {.Kind = MaterialDesignThemes.Wpf.PackIconKind.CloseCircle, .Width = 18, .Height = 18}

                AddHandler btnClear.Click, Sub() txtSerial.Text = ""

                innerGrid.Children.Add(txtSerial) : Grid.SetColumn(txtSerial, 0)
                innerGrid.Children.Add(btnClear) : Grid.SetColumn(btnClear, 1)
                borderRight.Child = innerGrid
                Grid.SetColumn(borderRight, 1)

                outerGrid.Children.Add(borderLeft)
                outerGrid.Children.Add(borderRight)

                SerialNumberList.Children.Add(outerGrid)

                PopulateSerialList(_serialsList)
            Next
        End Sub

        Private Sub PopulateSerialList(serials As String)
            Dim pattern As String = "\(\d+\)\s*(.*?)(?=\s*\(\d+\)|$)"
            Dim matches = Regex.Matches(serials, pattern)

            For i As Integer = 0 To _maxQty - 1
                Dim targetTxt = TryCast(Me.FindName($"txtSerialList_{i}"), TextBox)

                If targetTxt IsNot Nothing Then
                    If i < matches.Count Then
                        targetTxt.Text = matches(i).Groups(1).Value.Trim()
                    Else
                        targetTxt.Text = ""
                    End If
                End If
            Next
        End Sub

        Private Sub btnSaveDelivery_Click(sender As Object, e As RoutedEventArgs)
            Dim allSerials As New List(Of String)

            For Each child In SerialNumberList.Children
                ' Find the TextBox inside the Borders/Grids
                ' Concatenate them into a single string for the result
            Next

            SerialResult = String.Join("  ", allSerials)

            Dim parentWindow = Window.GetWindow(Me)
            If parentWindow IsNot Nothing Then
                parentWindow.DialogResult = True
                parentWindow.Close()
            End If
        End Sub

        Private Sub btnDeleteAll_Click(sender As Object, e As RoutedEventArgs)
            ' Clear all textboxes in the SerialNumberList
        End Sub
    End Class
End Namespace