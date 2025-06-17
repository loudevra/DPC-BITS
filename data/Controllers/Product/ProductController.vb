Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports System.IO
Imports System.Text
Imports DocumentFormat.OpenXml.Bibliography

Namespace DPC.Data.Controllers
    ''' <summary>
    ''' Controller class for managing product-related operations
    ''' </summary>
    Public Class ProductController
#Region "Properties and Fields"
        ''' <summary>
        ''' List to store serial number TextBox controls
        ''' </summary>
        Public Shared SerialNumbers As New List(Of TextBox)

        ''' <summary>
        ''' Main container for the UI
        ''' </summary>
        Public Shared Property MainContainer As StackPanel

        ''' <summary>
        ''' TextBox for stock units
        ''' </summary>
        Public Shared Property TxtStockUnits As TextBox

        ''' <summary>
        ''' Popup control for UI
        ''' </summary>
        Public Shared popup As Popup

        ''' <summary>
        ''' Flag to indicate if a popup was recently closed
        ''' </summary>
        Public Shared RecentlyClosed As Boolean = False

        ''' <summary>
        ''' Local list of product variations
        ''' </summary>
        Public _variations As New List(Of ProductVariation)

        ''' <summary>
        ''' Static list to store variations data globally
        ''' </summary>
        Public Shared _savedVariations As List(Of ProductVariation) = New List(Of ProductVariation)

        ''' <summary>
        ''' Product variation manager instance to handle all variations
        ''' </summary>
        Public Shared variationManager As New ProductVariationManager()

        ''' <summary>
        ''' Keep track of serial number textboxes
        ''' </summary>
        Public Shared serialNumberTextBoxes As New List(Of TextBox)

        ''' <summary>
        ''' Keep if product has variation or not
        ''' </summary>
        Public Shared Property IsVariation As Boolean = False

#End Region

#Region "Combobox Data Loading Methods"
        ''' <summary>
        ''' Populates a combobox with brand data
        ''' </summary>
        Public Shared Sub GetBrands(comboBox As ComboBox)
            GetProduct.GetBrands(comboBox)
        End Sub

        Public Shared Sub GetBrandsWithSupplier(comboBox As ComboBox)
            GetProduct.GetBrandsWithSupplier(comboBox)
        End Sub

        ''' <summary>
        ''' Populates a combobox with suppliers filtered by brand
        ''' </summary>
        Public Shared Sub GetSuppliersByBrand(brandID As Integer, comboBox As ComboBox)
            GetProduct.GetSuppliersByBrand(brandID, comboBox)
        End Sub

        ''' <summary>
        ''' Populates a combobox with product categories
        ''' </summary>
        Public Shared Sub GetProductCategory(comboBox As ComboBox)
            GetProduct.GetProductCategory(comboBox)
        End Sub

        ''' <summary>
        ''' Populates a combobox with product subcategories based on the selected category
        ''' </summary>
        Public Shared Sub GetProductSubcategory(categoryName As String, comboBox As ComboBox, label As TextBlock, stackPanel As StackPanel)
            GetProduct.GetProductSubcategory(categoryName, comboBox, label, stackPanel)
        End Sub

        ''' <summary>
        ''' Populates a combobox with warehouse data
        ''' </summary>
        Public Shared Sub GetWarehouse(comboBox As ComboBox)
            GetProduct.GetWarehouse(comboBox)
        End Sub
#End Region

#Region "Product Code Generation"
        ''' <summary>
        ''' Generates a unique product code
        ''' </summary>
        Public Shared Function GenerateProductCode() As String
            Return GenerateProduct.GenerateProductCode()
        End Function

        ''' <summary>
        ''' Gets the next product counter for generating product codes
        ''' </summary>
        Public Shared Function GetNextProductCounter(datePart As String) As Integer
            Return GenerateProduct.GetNextProductCounter(datePart)
        End Function
#End Region

#Region "Serial Number Management (Non-Variations)"
        ''' <summary>
        ''' Event handler for adding a new serial number row
        ''' </summary>
        Public Shared Sub BtnAddRow_Click(sender As Object, e As RoutedEventArgs, Optional skipStockUpdate As Boolean = False)
            RenderProduct.AddSerialRow(sender, e, skipStockUpdate)
        End Sub

        ''' <summary>
        ''' Event handler for removing a serial number row
        ''' </summary>
        Public Shared Sub BtnRemoveRow_Click(sender As Object, e As RoutedEventArgs)
            DeleteProduct.RemoveSerialRow(sender, e)
        End Sub

        ''' <summary>
        ''' Event handler for opening the row controller
        ''' </summary>
        Public Shared Sub BtnRowController_Click(sender As Object, e As RoutedEventArgs)
            RenderProduct.OpenRowController(sender, e)
        End Sub

        ''' <summary>
        ''' Removes the most recently added serial number row
        ''' </summary>
        Public Shared Sub RemoveLatestRow()
            DeleteProduct.RemoveLatestRow()
        End Sub

        ''' <summary>
        ''' Imports serial numbers from external source
        ''' </summary>
        Public Shared Sub ImportSerialNumbers_Click()
            UploadProduct.ImportSerialNumbers()
        End Sub



#End Region

#Region "Product Data Management"
        ''' <summary>
        ''' Loads product data into the specified DataGrid
        ''' </summary>
        Public Shared Sub LoadProductData(dataGrid As DataGrid)
            GetProduct.LoadProductData(dataGrid)
        End Sub
#End Region

#Region "UI Logic Methods"
        ''' <summary>
        ''' Handles the logic for checking if a product has variations
        ''' </summary>
        Public Shared Sub VariationChecker(toggle As ToggleButton,
                          stackPanelVariation As StackPanel,
                          stackPanelWarehouse As StackPanel,
                          stackPanelRetailPrice As StackPanel,
                          stackPanelOrderPrice As StackPanel,
                          stackPanelTaxRate As StackPanel,
                          stackPanelDiscountRate As StackPanel,
                          stackPanelMarkupRate As StackPanel,
                          borderStocks As Border,
                          stackPanelAlertQuantity As StackPanel,
                          stackPanelStockUnits As StackPanel,
                          outerStackPanel As StackPanel)

            LogicProduct.VariationChecker(toggle,
                          stackPanelVariation,
                          stackPanelWarehouse,
                          stackPanelRetailPrice,
                          stackPanelOrderPrice,
                          stackPanelTaxRate,
                          stackPanelDiscountRate,
                          stackPanelMarkupRate,
                          borderStocks,
                          stackPanelAlertQuantity,
                          stackPanelStockUnits,
                          outerStackPanel)
        End Sub

        ''' <summary>
        ''' Handles the logic for checking if a product has serial numbers
        ''' </summary>
        Public Shared Sub SerialNumberChecker(checkbox As CheckBox,
                     stackPanelSerialRow As StackPanel,
                     txtStockUnits As TextBox,
                     borderStockUnits As Border)

            LogicProduct.SerialNumberChecker(checkbox,
                     stackPanelSerialRow,
                     txtStockUnits,
                     borderStockUnits)
        End Sub

        ''' <summary>
        ''' Clears all input fields in the product form
        ''' </summary>
        Public Shared Sub ClearInputFields(txtProductName As TextBox,
                                           txtProductCode As TextBox,
                          txtRetailPrice As TextBox,
                          txtPurchaseOrder As TextBox,
                          txtDefaultTax As TextBox,
                          txtDiscountRate As TextBox,
                          txtStockUnits As TextBox,
                          txtAlertQuantity As TextBox,
                          txtDescription As TextBox,
                          comboBoxCategory As ComboBox,
                          comboBoxSubCategory As ComboBox,
                          comboBoxWarehouse As ComboBox,
                          comboBoxMeasurementUnit As ComboBox,
                          comboBoxBrand As ComboBox,
                          comboBoxSupplier As ComboBox,
                          singleDatePicker As DatePicker,
                          mainContainer As Panel)

            RenderProduct.ClearInputFieldsNoVariation(txtProductName,
                                                      txtProductCode,
                          txtRetailPrice,
                          txtPurchaseOrder,
                          txtDefaultTax,
                          txtDiscountRate,
                          txtStockUnits,
                          txtAlertQuantity,
                          txtDescription,
                          comboBoxCategory,
                          comboBoxSubCategory,
                          comboBoxWarehouse,
                          comboBoxMeasurementUnit,
                          comboBoxBrand,
                          comboBoxSupplier,
                          singleDatePicker,
                          mainContainer)
        End Sub
#End Region

#Region "Validation Methods"
        ''' <summary>
        ''' Validates if the provided file is a valid image
        ''' </summary>
        Public Function ValidateImageFile(filePath As String) As Boolean
            Return LogicProduct.ValidateImageFile(filePath)
        End Function

        ''' <summary>
        ''' Ensures that only integer values can be input in a TextBox
        ''' </summary>
        Public Shared Sub IntegerOnlyTextInputHandler(sender As Object, e As TextCompositionEventArgs)
            LogicProduct.IntegerOnlyTextInputHandler(sender, e)
        End Sub

        ''' <summary>
        ''' Ensures that only integer values can be pasted into a TextBox
        ''' </summary>
        Public Shared Sub IntegerOnlyPasteHandler(sender As Object, e As DataObjectPastingEventArgs)
            LogicProduct.IntegerOnlyPasteHandler(sender, e)
        End Sub

        ''' <summary>
        ''' Processes stock units entry and updates UI accordingly
        ''' </summary>
        Public Sub ProcessStockUnitsEntry(txtStockUnits As TextBox, mainContainer As Panel)
            LogicProduct.ProcessStockUnitsEntry(txtStockUnits, mainContainer)
        End Sub

        ''' <summary>
        ''' Updates the product variation text display
        ''' </summary>
        Public Sub UpdateProductVariationText(variations As List(Of ProductVariation), txtProductVariation As TextBlock)
            LogicProduct.UpdateProductVariationText(variations, txtProductVariation)
        End Sub

        ''' <summary>
        ''' Validates all product fields before saving
        ''' </summary>
        ''' 
        Public Shared Function ValidateProductFieldsWithVariation(ProductName As TextBox, ProductImage As String, Category As ComboBox, Brand As ComboBox,
                                              Supplier As ComboBox, MeasurementUnit As ComboBox, Description As TextBox,
                                               allVariationData As Dictionary(Of String, ProductVariationData)) As Boolean

            Return LogicProduct.ValidateProductFieldsWithVariation(ProductName, ProductImage, Category, Brand,
                                              Supplier, MeasurementUnit, Description,
                                              allVariationData)
        End Function

        Public Shared Function ValidateProductFields(Checkbox As Controls.CheckBox, ProductName As TextBox, ProductCode As TextBox, Category As ComboBox,
                                              SubCategory As ComboBox, Warehouse As ComboBox, Brand As ComboBox,
                                              Supplier As ComboBox, RetailPrice As TextBox, PurchaseOrder As TextBox,
                                              DefaultTax As TextBox, DiscountRate As TextBox, StockUnits As TextBox,
                                              AlertQuantity As TextBox, MeasurementUnit As ComboBox, Description As TextBox,
                                              ValidDate As DatePicker, SerialNumbers As List(Of TextBox)) As Boolean

            Return LogicProduct.ValidateProductFields(Checkbox, ProductName, ProductCode, Category,
                                              SubCategory, Warehouse, Brand, Supplier, RetailPrice, PurchaseOrder,
                                              DefaultTax, DiscountRate, StockUnits, AlertQuantity, MeasurementUnit,
                                              Description, ValidDate, SerialNumbers)
        End Function

#End Region

#Region "Product Creation Methods"
        ''' <summary>
        ''' Inserts a new product into the database
        ''' </summary>
        Public Shared Sub InsertNewProduct(Toggle As System.Windows.Controls.Primitives.ToggleButton, Checkbox As Controls.CheckBox,
            ProductName As TextBox, ProductCode As TextBox, Category As ComboBox, SubCategory As ComboBox, Warehouse As ComboBox,
            Brand As ComboBox, Supplier As ComboBox,
            RetailPrice As TextBox, PurchaseOrder As TextBox, DefaultTax As TextBox,
            DiscountRate As TextBox, StockUnits As TextBox, AlertQuantity As TextBox,
            MeasurementUnit As ComboBox, Description As TextBox, ValidDate As DatePicker,
            SerialNumbers As List(Of TextBox), ProductImage As String)

            CreateProduct.InsertNewProduct(Toggle, Checkbox,
                ProductName, ProductCode, Category, SubCategory, Warehouse,
                Brand, Supplier,
                RetailPrice, PurchaseOrder, DefaultTax,
                DiscountRate, StockUnits, AlertQuantity,
                MeasurementUnit, Description, ValidDate,
                SerialNumbers, ProductImage)
        End Sub

        ''' <summary>
        ''' Inserts a product without variations into the database
        ''' </summary>
        Public Shared Sub InsertNonVariationProduct(conn As MySqlConnection, transaction As MySqlTransaction, productID As String,
                                     Warehouse As ComboBox, SellingPrice As TextBox, BuyingPrice As TextBox,
                                     DefaultTax As TextBox, DiscountRate As TextBox,
                                     StockUnits As TextBox, AlertQuantity As TextBox,
                                     ValidDate As DatePicker, SerialNumbers As List(Of TextBox),
                                     Checkbox As Controls.CheckBox)

            CreateProduct.InsertNonVariationProduct(conn, transaction, productID,
                                    Warehouse, SellingPrice, BuyingPrice,
                                    DefaultTax, DiscountRate,
                                    StockUnits, AlertQuantity,
                                    ValidDate, SerialNumbers,
                                    Checkbox)
        End Sub

        ''' <summary>
        ''' Inserts a product with variations into the database
        ''' </summary>
        Public Shared Sub InsertVariationProduct(conn As MySqlConnection, transaction As MySqlTransaction, productID As String)
            CreateProduct.InsertVariationProduct(conn, transaction, productID)
        End Sub

        ''' <summary>
        ''' Inserts serial numbers for a product into the database
        ''' </summary>
        Public Shared Sub InsertSerialNumbersForProduct(conn As MySqlConnection, transaction As MySqlTransaction,
                                          SerialNumbers As List(Of TextBox), productID As String)
            Dim query As String = "INSERT INTO serialnumberproduct (SerialNumber, ProductID) VALUES (@SerialNumber, @ProductID)"
            Using cmd As New MySqlCommand(query, conn, transaction)
                cmd.Parameters.AddWithValue("@ProductID", productID)

                For Each serialNumberTextBox As TextBox In SerialNumbers
                    If Not String.IsNullOrWhiteSpace(serialNumberTextBox.Text) Then
                        cmd.Parameters.Clear()
                        cmd.Parameters.AddWithValue("@SerialNumber", serialNumberTextBox.Text)
                        cmd.Parameters.AddWithValue("@ProductID", productID)
                        cmd.ExecuteNonQuery()
                    End If
                Next
            End Using
        End Sub
#End Region

#Region "Variation Methods"
        ''' <summary>
        ''' Gets the list of product variations
        ''' </summary>
        Public Function GetProductVariations() As List(Of ProductVariation)
            Return DPC.Components.Forms.AddVariation.SavedVariations
        End Function
#End Region
    End Class

    ''' <summary>
    ''' Manager class to handle all variation data for a product
    ''' </summary>
    Public Class ProductVariationManager
#Region "Properties"
        ''' <summary>
        ''' Dictionary to store variation data, keyed by combination name
        ''' </summary>
        Private Property VariationDataDict As Dictionary(Of String, ProductVariationData)

        ''' <summary>
        ''' Currently selected variation combination
        ''' </summary>
        Public Property CurrentCombination As String
#End Region

#Region "Constructor"
        ''' <summary>
        ''' Initializes a new instance of the ProductVariationManager class
        ''' </summary>
        Public Sub New()
            VariationDataDict = New Dictionary(Of String, ProductVariationData)()
            CurrentCombination = String.Empty
        End Sub
#End Region

#Region "Variation Management Methods"
        ''' <summary>
        ''' Adds a new variation combination
        ''' </summary>
        Public Sub AddVariationCombination(combinationName As String)
            If Not VariationDataDict.ContainsKey(combinationName) Then
                VariationDataDict.Add(combinationName, New ProductVariationData(combinationName))
            End If
        End Sub

        ''' <summary>
        ''' Gets variation data for a specific combination
        ''' </summary>
        Public Function GetVariationData(combinationName As String) As ProductVariationData
            If String.IsNullOrEmpty(combinationName) Then
                Return Nothing
            End If

            ' Create and add if doesn't exist
            If Not VariationDataDict.ContainsKey(combinationName) Then
                Dim newData As New ProductVariationData(combinationName)
                VariationDataDict.Add(combinationName, newData)
            End If

            Return VariationDataDict(combinationName)
        End Function

        ''' <summary>
        ''' Selects a variation combination
        ''' </summary>
        Public Sub SelectVariationCombination(combinationName As String)
            CurrentCombination = combinationName
        End Sub

        ''' <summary>
        ''' Gets currently selected variation data
        ''' </summary>
        Public Function GetCurrentVariationData() As ProductVariationData
            If String.IsNullOrEmpty(CurrentCombination) Then
                Return Nothing
            End If
            Return GetVariationData(CurrentCombination)
        End Function

        ''' <summary>
        ''' Gets all variation data
        ''' </summary>
        Public Function GetAllVariationData() As Dictionary(Of String, ProductVariationData)
            Return VariationDataDict
        End Function


#End Region
    End Class
End Namespace