Imports System.Windows.Controls.Primitives
Imports MaterialDesignThemes.Wpf
Imports Microsoft.Win32
Imports System.IO
Imports DPC.DPC.Data.Helpers
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model

Namespace DPC.Components.Forms
    Public Class AddVariation
        Inherits UserControl

        Public Event close(sender As Object, e As RoutedEventArgs)

        Public Sub New()
            InitializeComponent()

            ' Check if we have saved variations data
            If ProductController._savedVariations.Count > 0 Then
                ' Load saved variations
                LoadSavedVariations()
            Else
                ' Add the first variation
                ProductController.AddNewVariation(MainVariationContainer)
            End If
        End Sub

        ' Method to load saved variations
        Private Sub LoadSavedVariations()
            ' Clear the UI container first
            MainVariationContainer.Children.Clear()

            ' Reset variation counter
            ProductController.variationCount = 1

            ' Loop through each saved variation and recreate the UI
            For Each variation As ProductVariation In ProductController._savedVariations
                ' Create the variation UI elements
                Dim variationPanel As StackPanel = ProductController.CreateVariationPanel(variation, MainVariationContainer)

                ' Add to the main container
                MainVariationContainer.Children.Add(variationPanel)

                ' Increment variation counter
                ProductController.variationCount += 1
            Next

            ' Make sure variation count is set correctly
            ProductController.RecalculateVariationCount(MainVariationContainer)
        End Sub

        ' Helper method to create a variation panel from data

        Private Sub BtnVariationDetails(sender As Object, e As RoutedEventArgs)
            ' Save variations before navigating
            ProductController.SaveVariations(MainVariationContainer)

            ' Notify the parent window to update its variation text
            Dim parentWindow = Window.GetWindow(Me)
            If parentWindow IsNot Nothing AndAlso TypeOf parentWindow Is DPC.Views.Stocks.ItemManager.NewProduct.AddNewProducts Then
                Dim addNewProductsWindow = DirectCast(parentWindow, DPC.Views.Stocks.ItemManager.NewProduct.AddNewProducts)
                addNewProductsWindow.LoadProductVariations()
            End If

            Dim VariationDetails As New Views.Stocks.ItemManager.NewProduct.ProductVariationDetails()
            VariationDetails.Show()

            Dim currentWindow As Window = Window.GetWindow(Me)
            currentWindow?.Close()
        End Sub

        ' Also update the ClosePopup method to ensure variations are saved and the display is updated
        Public Sub ClosePopup(sender As Object, e As RoutedEventArgs)
            ' Save variations before closing
            ProductController.SaveVariations(MainVariationContainer)

            ' Notify the parent window to update its variation text
            Dim parentWindow = Window.GetWindow(Me)
            If parentWindow IsNot Nothing AndAlso TypeOf parentWindow Is DPC.Views.Stocks.ItemManager.NewProduct.AddNewProducts Then
                Dim addNewProductsWindow = DirectCast(parentWindow, DPC.Views.Stocks.ItemManager.NewProduct.AddNewProducts)
                addNewProductsWindow.LoadProductVariations()
            End If

            ' Raise the close event
            RaiseEvent close(Me, e)
            PopupHelper.ClosePopup()
        End Sub


        ' Add this method to handle the "Add Variation" button click
        Private Sub BtnAddVariation_Click(sender As Object, e As RoutedEventArgs)
            ' Check if the current number of variations is less than the maximum allowed
            If MainVariationContainer.Children.Count < ProductController.MaxVariations Then
                ProductController.AddNewVariation(MainVariationContainer)
            Else
                MessageBox.Show("You can only add up to 2 variations.", "Limit Reached", MessageBoxButton.OK, MessageBoxImage.Information)
            End If
        End Sub




    End Class
End Namespace