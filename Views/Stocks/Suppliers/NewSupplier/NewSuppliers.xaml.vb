Imports System.Collections.ObjectModel
Imports System.Windows.Controls
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Stocks.Supplier.NewSuppliers
    Public Class NewSuppliers
        Inherits Window

        Private brandList As ObservableCollection(Of Brand)
        Private selectedBrands As New ObservableCollection(Of Brand)

        Public Sub New()
            InitializeComponent()
            LoadBrands()
        End Sub

        ' Load brands from database
        Private Sub LoadBrands()
            brandList = New ObservableCollection(Of Brand)()

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim query As String = "SELECT brandid, brandname FROM Brand;"
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                brandList.Add(New Brand With {
                                    .ID = reader.GetInt32("brandid"),
                                    .Name = reader.GetString("brandname")
                                })
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error loading brands: " & ex.Message)
                End Try
            End Using
        End Sub

        ' Handle text change for brand search
        Private Sub TxtBrand_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim searchText = TxtBrand.Text.ToLower()

            If String.IsNullOrWhiteSpace(searchText) Then
                LstBrands.Visibility = Visibility.Collapsed
                Return
            End If

            Dim filteredBrands = brandList.Where(Function(b) b.Name.ToLower().Contains(searchText)).ToList()

            If filteredBrands.Any() Then
                LstBrands.ItemsSource = filteredBrands
                LstBrands.Visibility = Visibility.Visible
            Else
                LstBrands.Visibility = Visibility.Collapsed
            End If
        End Sub

        ' Handle keydown events (Enter or Backspace)
        Private Sub TxtBrand_KeyDown(sender As Object, e As KeyEventArgs)
            If e.Key = Key.Enter AndAlso LstBrands.SelectedItem IsNot Nothing Then
                AddBrandChip(LstBrands.SelectedItem)
                TxtBrand.Clear()
            ElseIf e.Key = Key.Back AndAlso String.IsNullOrEmpty(TxtBrand.Text) AndAlso selectedBrands.Count > 0 Then
                RemoveLastChip()
            End If
        End Sub

        ' Handle brand selection from ListBox
        Private Sub LstBrands_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If LstBrands.SelectedItem IsNot Nothing Then
                AddBrandChip(LstBrands.SelectedItem)
                TxtBrand.Clear()
                LstBrands.Visibility = Visibility.Collapsed
            End If
        End Sub

        ' Add selected brand as a chip
        Private Sub AddBrandChip(selectedBrand As Brand)
            If Not selectedBrands.Any(Function(b) b.ID = selectedBrand.ID) Then
                selectedBrands.Add(selectedBrand)

                Dim chip As New Border With {
                    .Background = Brushes.LightBlue,
                    .CornerRadius = New CornerRadius(10),
                    .Margin = New Thickness(5),
                    .Padding = New Thickness(8),
                    .Child = New TextBlock With {.Text = selectedBrand.Name}
                }

                AddHandler chip.MouseLeftButtonDown, Sub(sender, e)
                                                         RemoveBrandChip(selectedBrand, chip)
                                                     End Sub

                ChipPanel.Children.Add(chip)
            End If
        End Sub

        ' Remove the brand chip
        Private Sub RemoveBrandChip(selectedBrand As Brand, chip As Border)
            selectedBrands.Remove(selectedBrand)
            ChipPanel.Children.Remove(chip)
        End Sub

        ' Remove the last brand chip
        Private Sub RemoveLastChip()
            If selectedBrands.Count > 0 Then
                Dim lastBrand = selectedBrands.Last()
                Dim chipToRemove = ChipPanel.Children.OfType(Of Border)().
                                   FirstOrDefault(Function(b) CType(b.Child, TextBlock).Text = lastBrand.Name)

                If chipToRemove IsNot Nothing Then
                    ChipPanel.Children.Remove(chipToRemove)
                    selectedBrands.Remove(lastBrand)
                End If
            End If
        End Sub

        ' Brand Class
        Public Class Brand
            Public Property ID As Integer
            Public Property Name As String
        End Class
    End Class
End Namespace
