Imports System.Collections.ObjectModel
Imports System.Windows
Imports DPC.DPC.Components.Forms
Imports DPC.DPC.Models
Imports DPC.Models

Namespace DPC.Views
    Public Class SuppliersDashboard


        Public Property SuppliersList As New ObservableCollection(Of Supplier)

        Public Sub New()
            InitializeComponent()

            ' Bind the DataGrid to our list
            SuppliersDataGrid.ItemsSource = SuppliersList
        End Sub

        Private Sub BtnOpenAddSupplier_Click(sender As Object, e As RoutedEventArgs)
            ' 1. Open the modal
            Dim supplierModal As New AddNewSupplierForm()
            Dim result As Boolean? = supplierModal.ShowDialog()

            ' 2. Check if the user clicked "Add"
            If result = True AndAlso supplierModal.CreatedSupplier IsNot Nothing Then

                ' 3. Add the new supplier to our list (DataGrid will update automatically)
                SuppliersList.Add(supplierModal.CreatedSupplier)

                ' NOTE: If you have a database, you would also write an INSERT query here
            End If
        End Sub

    End Class
End Namespace