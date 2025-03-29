Imports System.Collections.ObjectModel
Imports System.Windows.Controls
Imports DPC.DPC.Data.Model
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Controllers

Imports System.Windows
Imports DPC.DPC.Components



Namespace DPC.Views.Stocks.Supplier.NewSuppliers
    Public Class NewSuppliers
        Inherits Window

        Private brandList As ObservableCollection(Of Brand)
        Private selectedBrands As New ObservableCollection(Of Brand)
        Dim Brand As New Brand()

        Public Sub New()
            InitializeComponent()

            Dim sidebar As New Components.Navigation.Sidebar()
            SidebarContainer.Content = sidebar

            ' Load Top Navigation Bar
            Dim topNav As New Components.Navigation.TopNavBar()
            TopNavBarContainer.Content = topNav

            LoadBrands()
        End Sub

        Private Sub BtnAddSupplier(sender As Object, e As RoutedEventArgs)
            Try
                ' Collect input values
                Dim supplierName As String = TxtRepresentative.Text.Trim()
                Dim companyName As String = TxtCompany.Text.Trim()
                Dim phone As String = TxtPhone.Text.Trim()
                Dim email As String = TxtEmail.Text.Trim()
                Dim address As String = TxtAddress.Text.Trim()
                Dim city As String = TxtCity.Text.Trim()
                Dim region As String = TxtRegion.Text.Trim()
                Dim country As String = TxtCountry.Text.Trim()
                Dim postalCode As String = TxtPostalCode.Text.Trim()
                Dim tinID As String = TxtTINID.Text.Trim()

                ' Validate fields
                If String.IsNullOrWhiteSpace(supplierName) OrElse String.IsNullOrWhiteSpace(companyName) OrElse
           String.IsNullOrWhiteSpace(email) OrElse String.IsNullOrWhiteSpace(phone) Then
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                ' Collect selected brand IDs from the chips
                Dim brandIDs As List(Of String) = selectedBrands.Select(Function(b) b.ID.ToString()).ToList()

                ' Call the InsertSupplier function
                SupplierController.InsertSupplier(supplierName, companyName, phone, email, address, city, region, country, postalCode, tinID, brandIDs)

                ' Clear form and reset fields after successful insertion
                ClearForm()

            Catch ex As Exception
                MessageBox.Show("An error occurred while adding the supplier: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Clear input fields and chips
        Private Sub ClearForm()
            TxtRepresentative.Clear()
            TxtCompany.Clear()
            TxtPhone.Clear()
            TxtEmail.Clear()
            TxtAddress.Clear()
            TxtCity.Clear()
            TxtRegion.Clear()
            TxtCountry.Clear()
            TxtPostalCode.Clear()
            TxtTINID.Clear()
            TxtBrand.Clear()

            ' Clear Chips
            ChipPanel.Children.Clear()
            selectedBrands.Clear()

            MessageBox.Show("Form cleared!", "Info", MessageBoxButton.OK, MessageBoxImage.Information)
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

            Dim filteredBrands = brandList.
                          Where(Function(b) b.Name.ToLower().Contains(searchText) AndAlso
                                           Not selectedBrands.Any(Function(sb) sb.ID = b.ID)).
                          ToList()

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

                ' Clear selection to allow selecting the same item again
                LstBrands.SelectedItem = Nothing
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

            ' Refresh the ListBox to show the removed brand again if it matches the search
            TxtBrand_TextChanged(Nothing, Nothing)
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
    End Class
End Namespace
