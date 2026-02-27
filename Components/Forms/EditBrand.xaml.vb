' EditBrand.xaml.vb
Imports DPC.DPC.Data.Controllers
Imports MySql.Data.MySqlClient
Imports DPC.DPC.Views.Stocks.Suppliers.ManageBrands
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives

Namespace DPC.Components.Forms
    Public Class EditBrand
        Public Event BrandAdded()
        Public brandID As Integer?
        Public manageBrands As ManageBrands
        Public Property CategoryName As String
        Public Property SubCategoryName As String

        Private ProductController As New ProductController()
        Private popupAddCategory As Popup
        Private popupAddSubCategory As Popup
        Private _isLoadingCategories As Boolean = False

        Public Sub New()
            InitializeComponent()
        End Sub

        ' Called from ManageBrands after all properties are set
        Public Sub LoadData()
            _isLoadingCategories = True
            ProductController.GetProductCategory(CmbCategory)

            ' Pre-select the matching category
            If Not String.IsNullOrEmpty(CategoryName) Then
                For Each item As ComboBoxItem In CmbCategory.Items
                    If item.Content?.ToString() = CategoryName Then
                        CmbCategory.SelectedItem = item
                        Exit For
                    End If
                Next
            End If
            _isLoadingCategories = False

            ' Load subcategories for selected category then pre-select
            If CmbCategory.SelectedItem IsNot Nothing Then
                Dim selectedCategory As String = TryCast(CmbCategory.SelectedItem, ComboBoxItem)?.Content?.ToString()
                ProductController.GetProductSubcategory(selectedCategory, CmbSubCategory, SubCategoryLabel, New StackPanel())

                If Not String.IsNullOrEmpty(SubCategoryName) Then
                    For Each item As ComboBoxItem In CmbSubCategory.Items
                        If item.Content?.ToString() = SubCategoryName Then
                            CmbSubCategory.SelectedItem = item
                            Exit For
                        End If
                    Next
                End If
            End If
        End Sub

        Private Sub CmbCategory_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles CmbCategory.SelectionChanged
            If _isLoadingCategories Then Return
            Dim selectedCategory As String = TryCast(CmbCategory.SelectedItem, ComboBoxItem)?.Content?.ToString()
            If Not String.IsNullOrEmpty(selectedCategory) Then
                ProductController.GetProductSubcategory(selectedCategory, CmbSubCategory, SubCategoryLabel, New StackPanel())
            Else
                CmbSubCategory.Items.Clear()
            End If
        End Sub

        Private Sub TxtBrand_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return
            Dim original = tb.Text
            Dim upper = original.ToUpperInvariant()
            If original = upper Then Return
            Dim selStart = tb.SelectionStart
            Dim selLength = tb.SelectionLength
            RemoveHandler tb.TextChanged, AddressOf TxtBrand_TextChanged
            tb.Text = upper
            tb.SelectionStart = Math.Min(selStart, tb.Text.Length)
            tb.SelectionLength = selLength
            AddHandler tb.TextChanged, AddressOf TxtBrand_TextChanged
        End Sub

        Private Sub SaveBrand_Click(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrWhiteSpace(TxtBrand.Text) Then
                MessageBox.Show("Please enter a brand name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            If CmbCategory.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a category.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim selectedItem As ComboBoxItem = TryCast(CmbCategory.SelectedItem, ComboBoxItem)
            If selectedItem Is Nothing Then Return
            Dim categoryID As Integer = Convert.ToInt32(selectedItem.Tag)

            Dim subCategoryID As Integer = 0
            Dim selectedSubItem As ComboBoxItem = TryCast(CmbSubCategory.SelectedItem, ComboBoxItem)
            If selectedSubItem IsNot Nothing AndAlso selectedSubItem.Tag IsNot Nothing Then
                subCategoryID = Convert.ToInt32(selectedSubItem.Tag)
            End If

            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    ' Check brand exists
                    Dim existsQuery As String = "SELECT COUNT(*) FROM brand WHERE BrandID = @BrandID"
                    Using existsCmd As New MySqlCommand(existsQuery, conn)
                        existsCmd.Parameters.AddWithValue("@BrandID", brandID.Value)
                        Dim count As Integer = Convert.ToInt32(existsCmd.ExecuteScalar())
                        If count = 0 Then
                            MessageBox.Show("Brand not found.")
                            Return
                        End If
                    End Using

                    ' Check for duplicate name excluding current brand
                    Dim checkQuery As String = "SELECT COUNT(*) FROM brand WHERE brandName = @BrandName AND BrandID <> @BrandID"
                    Using checkCmd As New MySqlCommand(checkQuery, conn)
                        checkCmd.Parameters.AddWithValue("@BrandName", TxtBrand.Text)
                        checkCmd.Parameters.AddWithValue("@BrandID", brandID.Value)
                        Dim count As Integer = Convert.ToInt32(checkCmd.ExecuteScalar())
                        If count > 0 Then
                            MessageBox.Show("Brand name already exists.")
                            Return
                        End If
                    End Using

                    ' Update brand name, category, and subcategory
                    Dim updateQuery As String = "UPDATE brand SET BrandName = @BrandName, categoryID = @CategoryID, subcategoryID = @SubCategoryID WHERE BrandID = @BrandID"
                    Using updateCmd As New MySqlCommand(updateQuery, conn)
                        updateCmd.Parameters.AddWithValue("@BrandName", TxtBrand.Text)
                        updateCmd.Parameters.AddWithValue("@CategoryID", categoryID)
                        updateCmd.Parameters.AddWithValue("@SubCategoryID", If(subCategoryID = 0, DBNull.Value, CObj(subCategoryID)))
                        updateCmd.Parameters.AddWithValue("@BrandID", brandID.Value)
                        updateCmd.ExecuteNonQuery()
                    End Using
                End Using

                MessageBox.Show("Brand updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                RaiseEvent BrandAdded()

            Catch ex As Exception
                MessageBox.Show($"An error occurred: {ex.Message}")
            End Try
        End Sub

        Private Sub BtnAddCategory_Click(sender As Object, e As RoutedEventArgs)
            If popupAddCategory IsNot Nothing Then
                popupAddCategory.IsOpen = False
                popupAddCategory.Child = Nothing
            End If

            Dim addNewCategory As New DPC.Components.Forms.AddCategory()

            popupAddCategory = New Popup With {
                .Placement = PlacementMode.AbsolutePoint,
                .StaysOpen = False,
                .AllowsTransparency = True,
                .Child = addNewCategory
            }

            AddHandler popupAddCategory.Opened, Sub()
                                                    Dim screenWidth As Double = SystemParameters.PrimaryScreenWidth
                                                    Dim screenHeight As Double = SystemParameters.PrimaryScreenHeight
                                                    popupAddCategory.HorizontalOffset = (screenWidth / 2) - (addNewCategory.ActualWidth / 2)
                                                    popupAddCategory.VerticalOffset = (screenHeight / 2) - (addNewCategory.ActualHeight / 2)
                                                End Sub

            AddHandler popupAddCategory.Closed, Sub()
                                                    ProductController.GetProductCategory(CmbCategory)
                                                End Sub

            popupAddCategory.IsOpen = True
        End Sub

        Private Sub BtnAddSubcategory_Click(sender As Object, e As RoutedEventArgs)
            If popupAddSubCategory IsNot Nothing Then
                popupAddSubCategory.IsOpen = False
                popupAddSubCategory.Child = Nothing
            End If

            Dim addNewSubCategory As New DPC.Components.Forms.AddSubcategory()

            popupAddSubCategory = New Popup With {
                .Placement = PlacementMode.AbsolutePoint,
                .StaysOpen = False,
                .AllowsTransparency = True,
                .Child = addNewSubCategory
            }

            AddHandler popupAddSubCategory.Opened, Sub()
                                                       Dim screenWidth As Double = SystemParameters.PrimaryScreenWidth
                                                       Dim screenHeight As Double = SystemParameters.PrimaryScreenHeight
                                                       popupAddSubCategory.HorizontalOffset = (screenWidth / 2) - (addNewSubCategory.ActualWidth / 2)
                                                       popupAddSubCategory.VerticalOffset = (screenHeight / 2) - (addNewSubCategory.ActualHeight / 2)
                                                   End Sub

            AddHandler popupAddSubCategory.Closed, Sub()
                                                       Dim selectedCategory As String = TryCast(CmbCategory.SelectedItem, ComboBoxItem)?.Content?.ToString()
                                                       ProductController.GetProductSubcategory(If(selectedCategory, String.Empty), CmbSubCategory, SubCategoryLabel, New StackPanel())
                                                   End Sub

            popupAddSubCategory.IsOpen = True
        End Sub

    End Class
End Namespace
