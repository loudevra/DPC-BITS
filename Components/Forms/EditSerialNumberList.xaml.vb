Imports System.Text.RegularExpressions
Imports System.Windows.Controls
Imports System.Windows

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

            AddHandler btnSaveDelivery.Click, AddressOf btnSaveDelivery_Click
            AddHandler btnDeleteAll.Click, AddressOf btnDeleteAll_Click

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

                ' --- Right Side: Serial Input ---
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
            Next

            PopulateSerialList(_serialsList)
        End Sub

        Private Sub PopulateSerialList(serials As String)
            If String.IsNullOrEmpty(serials) Then Return

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
            Dim displayCounter As Integer = 1

            For i As Integer = 0 To _maxQty - 1
                Dim txt = TryCast(Me.FindName($"txtSerialList_{i}"), TextBox)
                If txt IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(txt.Text) Then
                    allSerials.Add($"({displayCounter}) {txt.Text.Trim()}")
                    displayCounter += 1
                End If
            Next

            Me.SerialResult = String.Join("  ", allSerials)
            Me.ListLength = allSerials.Count
            Me.IsSaved = True

            Dim parentPopup = TryCast(Me.Parent, System.Windows.Controls.Primitives.Popup)

            If parentPopup IsNot Nothing Then
                parentPopup.IsOpen = False
            Else
                Dim itm = Me.Parent
                While itm IsNot Nothing AndAlso Not TypeOf itm Is System.Windows.Controls.Primitives.Popup
                    itm = LogicalTreeHelper.GetParent(itm)
                End While

                If itm IsNot Nothing Then DirectCast(itm, System.Windows.Controls.Primitives.Popup).IsOpen = False
            End If
        End Sub

        Private Sub btnDeleteAll_Click(sender As Object, e As RoutedEventArgs)
            For i As Integer = 0 To _maxQty - 1
                Dim txt = TryCast(Me.FindName($"txtSerialList_{i}"), TextBox)
                If txt IsNot Nothing Then txt.Clear()
            Next
        End Sub
    End Class
End Namespace