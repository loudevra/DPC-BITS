Imports System.Collections.ObjectModel
Imports System.Windows.Controls
Imports DPC.DPC.Data.Model
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Components.Forms


Namespace DPC.Views.Stocks.Supplier.NewSuppliers
    Public Class NewSuppliers
        Inherits UserControl

        Private brandList As ObservableCollection(Of Brand)
        Private autocompleteHelper As AutocompleteHelper(Of Brand)

        Public Sub New()
            InitializeComponent()

            ' Load Brands from the database
            LoadBrands()

            ' Initialize autocomplete helper
            autocompleteHelper = New AutocompleteHelper(Of Brand)(
                Function(b) b.ID,
                Function(b) b.Name
            )

            ' Configure and initialize autocomplete control
            autocompleteHelper.Initialize(
                TxtItem,                   ' TextBox for input
                LstItems,                  ' ListBox for suggestions
                ChipPanel,                 ' Panel for chips
                AutoCompletePopup,         ' Popup for suggestions
                brandList                  ' Data source
            )
        End Sub

        ' Load brands from the database
        Private Sub LoadBrands()
            brandList = New ObservableCollection(Of Brand)()

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim query As String = "SELECT brandid, brandname FROM brand;"
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

        ' Add supplier
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

                ' Get selected brand IDs from the helper
                Dim brandIDs As List(Of String) = autocompleteHelper.SelectedItems.Select(Function(b) b.ID.ToString()).ToList()

                ' Call the InsertSupplier function
                SupplierController.InsertSupplier(supplierName, companyName, phone, email, address, city, region, country, postalCode, tinID, brandIDs)

                ' Clear form and reset fields after successful insertion
                ClearForm()

                DPC.Data.Helpers.DynamicView.NavigateToView("managesuppliers", Me)

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
            TxtItem.Clear()

            ' Clear selected brands using the helper
            autocompleteHelper.ClearSelection(ChipPanel)

            MessageBox.Show("Form cleared!", "Info", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub
    End Class
End Namespace