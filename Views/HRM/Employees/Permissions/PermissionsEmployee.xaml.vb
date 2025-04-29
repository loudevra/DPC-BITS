Imports DPC.DPC.Data.Controllers
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Helpers

Namespace DPC.Views.HRM.Employees.Permissions
    Public Class PermissionsEmployee
        Inherits UserControl

        ' Collection for our permission items
        Private _permissionItems As ObservableCollection(Of PermissionItem)

        Public Sub New()
            ' This call is required by the designer.
            InitializeComponent()

            ' Initialize the collection
            _permissionItems = New ObservableCollection(Of PermissionItem)

            ' Add handler for when control is loaded
            AddHandler Me.Loaded, AddressOf PermissionsEmployee_Loaded
        End Sub

        Private Sub PermissionsEmployee_Loaded(sender As Object, e As RoutedEventArgs)
            ' Load sample data (in production, this would come from your database)
            LoadSampleData()

            ' Set the DataGrid's ItemsSource
            dataGrid.ItemsSource = _permissionItems
        End Sub

        Private Sub LoadSampleData()
            ' Clear existing items
            _permissionItems.Clear()

            ' Add sample items that match your image
            _permissionItems.Add(New PermissionItem() With {
                .Id = 1,
                .Name = "Sales",
                .HasInventoryManager = True,
                .HasSalesPerson = True,
                .HasSalesManager = True,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })

            _permissionItems.Add(New PermissionItem() With {
                .Id = 2,
                .Name = "Stock",
                .HasInventoryManager = True,
                .HasSalesPerson = True,
                .HasSalesManager = True,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })

            _permissionItems.Add(New PermissionItem() With {
                .Id = 3,
                .Name = "Crm",
                .HasInventoryManager = True,
                .HasSalesPerson = True,
                .HasSalesManager = True,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })

            _permissionItems.Add(New PermissionItem() With {
                .Id = 4,
                .Name = "Project",
                .HasInventoryManager = False,
                .HasSalesPerson = False,
                .HasSalesManager = False,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })

            _permissionItems.Add(New PermissionItem() With {
                .Id = 5,
                .Name = "Accounts",
                .HasInventoryManager = False,
                .HasSalesPerson = False,
                .HasSalesManager = False,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })

            _permissionItems.Add(New PermissionItem() With {
                .Id = 6,
                .Name = "Miscellaneous",
                .HasInventoryManager = False,
                .HasSalesPerson = False,
                .HasSalesManager = False,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })

            _permissionItems.Add(New PermissionItem() With {
                .Id = 7,
                .Name = "Assign Project",
                .HasInventoryManager = True,
                .HasSalesPerson = True,
                .HasSalesManager = False,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })

            _permissionItems.Add(New PermissionItem() With {
                .Id = 8,
                .Name = "Customer Profile",
                .HasInventoryManager = True,
                .HasSalesPerson = True,
                .HasSalesManager = False,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })

            _permissionItems.Add(New PermissionItem() With {
                .Id = 9,
                .Name = "Employees",
                .HasInventoryManager = False,
                .HasSalesPerson = False,
                .HasSalesManager = False,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })

            _permissionItems.Add(New PermissionItem() With {
                .Id = 10,
                .Name = "Reports",
                .HasInventoryManager = False,
                .HasSalesPerson = False,
                .HasSalesManager = False,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })

            _permissionItems.Add(New PermissionItem() With {
                .Id = 11,
                .Name = "Delete",
                .HasInventoryManager = False,
                .HasSalesPerson = False,
                .HasSalesManager = True,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })

            _permissionItems.Add(New PermissionItem() With {
                .Id = 12,
                .Name = "POS",
                .HasInventoryManager = True,
                .HasSalesPerson = True,
                .HasSalesManager = False,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })

            _permissionItems.Add(New PermissionItem() With {
                .Id = 13,
                .Name = "Sales Edit",
                .HasInventoryManager = True,
                .HasSalesPerson = True,
                .HasSalesManager = True,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })

            _permissionItems.Add(New PermissionItem() With {
                .Id = 14,
                .Name = "Stock Edit",
                .HasInventoryManager = True,
                .HasSalesPerson = False,
                .HasSalesManager = True,
                .HasBusinessManager = True,
                .HasBusinessOwner = True,
                .HasProjectManager = False
            })
        End Sub

        ' In a real application, you would add methods to load data from your database
        ' and to save changes when checkboxes are clicked

        ' Example method to load from database
        Private Sub LoadDataFromDatabase()
            ' TODO: Replace with your actual database loading code
            ' This is just a placeholder for implementation
            Try
                ' Connect to database
                ' Load permissions
                ' Populate _permissionItems collection
            Catch ex As Exception
                MessageBox.Show("Error loading permissions: " & ex.Message)
            End Try
        End Sub

        ' Example method to save changes
        Private Sub SavePermissionChanges(item As PermissionItem)
            ' TODO: Replace with your actual database save code
            ' This would be called when checkbox state changes
            Try
                ' Update database with new permission settings
            Catch ex As Exception
                MessageBox.Show("Error saving permissions: " & ex.Message)
            End Try
        End Sub

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs) Handles btnAddNew.Click
            ViewLoader.DynamicView.NavigateToView("addnewemployee", Me)
        End Sub
    End Class
End Namespace