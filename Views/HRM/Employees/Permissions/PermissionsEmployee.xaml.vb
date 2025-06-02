Imports DPC.DPC.Data.Controllers
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Helpers
Imports System.Windows
Imports System.Windows.Controls
Imports System.ComponentModel
Imports System.Windows.Media

Namespace DPC.Views.HRM.Employees.Permissions
    Public Class PermissionsEmployee
        Inherits UserControl

        Private _permissionItems As ObservableCollection(Of PermissionItem)
        Private _hasUnsavedChanges As Boolean = False

        Public Sub New()
            InitializeComponent()
            _permissionItems = New ObservableCollection(Of PermissionItem)
            AddHandler Me.Loaded, AddressOf PermissionsEmployee_Loaded
        End Sub

        Private Sub PermissionsEmployee_Loaded(sender As Object, e As RoutedEventArgs)
            LoadSampleData()
            If dataGrid IsNot Nothing Then
                dataGrid.ItemsSource = _permissionItems
            End If

            ' Subscribe to collection changed events to track changes
            AddHandler _permissionItems.CollectionChanged, AddressOf OnPermissionItemsChanged

            ' Subscribe to property changed events for each item
            For Each item In _permissionItems
                AddHandler item.PropertyChanged, AddressOf OnPermissionItemPropertyChanged
            Next
        End Sub

        Private Sub LoadSampleData()
            _permissionItems.Clear()

            ' Define the modules with their default permissions
            Dim moduleData As New List(Of Object) From {
                New With {.Id = 1, .Name = "Sales"},
                New With {.Id = 2, .Name = "Stock"},
                New With {.Id = 3, .Name = "Crm"},
                New With {.Id = 4, .Name = "Project"},
                New With {.Id = 5, .Name = "Accounts"},
                New With {.Id = 6, .Name = "Miscellaneous"},
                New With {.Id = 7, .Name = "Assign Project"},
                New With {.Id = 8, .Name = "Customer Profile"},
                New With {.Id = 9, .Name = "Employees"},
                New With {.Id = 10, .Name = "Reports"},
                New With {.Id = 11, .Name = "Delete"},
                New With {.Id = 12, .Name = "POS"},
                New With {.Id = 13, .Name = "Sales Edit"},
                New With {.Id = 14, .Name = "Stock Edit"}
            }

            For Each item In moduleData
                _permissionItems.Add(New PermissionItem() With {
                    .Id = item.Id,
                    .Name = item.Name,
                    .HasInventoryManager = False,
                    .HasSalesPerson = False,
                    .HasSalesManager = False,
                    .HasBusinessManager = False,
                    .HasBusinessOwner = True,
                    .HasProjectManager = False
                })
            Next
        End Sub

        Private Sub OnPermissionItemsChanged(sender As Object, e As System.Collections.Specialized.NotifyCollectionChangedEventArgs)
            _hasUnsavedChanges = True
            UpdateButtonState()
        End Sub

        Private Sub OnPermissionItemPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
            _hasUnsavedChanges = True
            UpdateButtonState()

            ' Optional: Show visual feedback for changed items
            Dim item As PermissionItem = DirectCast(sender, PermissionItem)
            Console.WriteLine($"Permission changed for {item.Name}: {e.PropertyName}")
        End Sub

        Private Sub UpdateButtonState()
            ' Enable/disable the update button based on whether there are unsaved changes
            If btnSave IsNot Nothing Then
                btnSave.IsEnabled = _hasUnsavedChanges

                ' Optional: Change button text to indicate unsaved changes
                If _hasUnsavedChanges Then
                    ' Find the TextBlock inside the button and update its text
                    Dim textBlock As TextBlock = FindVisualChild(Of TextBlock)(btnSave)
                    If textBlock IsNot Nothing Then
                        textBlock.Text = "Update*"
                    End If
                End If
            End If
        End Sub

        Private Sub BtnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
            Try
                ' Show confirmation dialog
                Dim result As MessageBoxResult = MessageBox.Show(
                    "Are you sure you want to update the permissions for all modules?",
                    "Confirm Update",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question)

                If result = MessageBoxResult.Yes Then
                    SaveAllPermissions()
                End If

            Catch ex As Exception
                MessageBox.Show($"Error updating permissions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub SaveAllPermissions()
            Try
                Dim updatedCount As Integer = 0

                For Each item As PermissionItem In _permissionItems
                    ' Here you would typically save to database
                    ' For now, we'll just simulate the save operation
                    SavePermissionToDatabase(item)
                    updatedCount += 1
                Next

                ' Reset the unsaved changes flag
                _hasUnsavedChanges = False
                UpdateButtonState()

                ' Reset button text
                Dim textBlock As TextBlock = FindVisualChild(Of TextBlock)(btnSave)
                If textBlock IsNot Nothing Then
                    textBlock.Text = "Update"
                End If

                ' Show success message
                MessageBox.Show($"Successfully updated permissions for {updatedCount} modules.",
                               "Update Complete",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information)

            Catch ex As Exception
                MessageBox.Show($"Error saving permissions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub SavePermissionToDatabase(item As PermissionItem)
            ' TODO: Implement actual database save logic here
            Try
                ' Create a dictionary of role permissions for this module
                Dim permissions As New Dictionary(Of String, Boolean) From {
                    {"InventoryManager", item.HasInventoryManager},
                    {"SalesPerson", item.HasSalesPerson},
                    {"SalesManager", item.HasSalesManager},
                    {"BusinessManager", item.HasBusinessManager},
                    {"BusinessOwner", item.HasBusinessOwner},
                    {"ProjectManager", item.HasProjectManager}
                }

                ' Log the permissions being saved (for debugging)
                Console.WriteLine($"Saving permissions for module '{item.Name}' (ID: {item.Id}):")
                For Each kvp In permissions
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value}")
                Next

                ' Here you would execute your database update logic
                ' Example:
                ' Using conn As New SqlConnection(connectionString)
                '     conn.Open()
                '     For Each role In permissions
                '         Dim cmd As New SqlCommand("UPDATE ModulePermissions SET HasAccess = @hasAccess WHERE ModuleId = @moduleId AND RoleName = @roleName", conn)
                '         cmd.Parameters.AddWithValue("@hasAccess", role.Value)
                '         cmd.Parameters.AddWithValue("@moduleId", item.Id)
                '         cmd.Parameters.AddWithValue("@roleName", role.Key)
                '         cmd.ExecuteNonQuery()
                '     Next
                ' End Using

            Catch ex As Exception
                Throw New Exception($"Failed to save permissions for module '{item.Name}': {ex.Message}")
            End Try
        End Sub

        Private Sub LoadDataFromDatabase()
            ' TODO: Implement actual database loading logic here
            Try
                ' Example of what you might do:
                ' Using conn As New SqlConnection(connectionString)
                '     conn.Open()
                '     Dim cmd As New SqlCommand("SELECT * FROM ModulePermissions", conn)
                '     Dim reader = cmd.ExecuteReader()
                '     ' Process the results and populate _permissionItems
                ' End Using

            Catch ex As Exception
                MessageBox.Show("Error loading permissions: " & ex.Message)
            End Try
        End Sub

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs) Handles btnAddNew.Click
            Try
                ' Check for unsaved changes before navigating
                If _hasUnsavedChanges Then
                    Dim result As MessageBoxResult = MessageBox.Show(
                        "You have unsaved changes. Do you want to save them before adding a new employee?",
                        "Unsaved Changes",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question)

                    Select Case result
                        Case MessageBoxResult.Yes
                            SaveAllPermissions()
                            NavigateToAddEmployee()
                        Case MessageBoxResult.No
                            NavigateToAddEmployee()
                        Case MessageBoxResult.Cancel
                            ' Do nothing, stay on current view
                            Return
                    End Select
                Else
                    NavigateToAddEmployee()
                End If
            Catch ex As Exception
                MessageBox.Show($"Error navigating to add employee: {ex.Message}", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub NavigateToAddEmployee()
            Try
                ' Option 1: If DynamicView has shared/static methods
                ViewLoader.DynamicView.NavigateToView("addnewemployee", Me)

                ' Option 2: If you need to create an instance (uncomment if needed)
                ' Dim dynamicView As New ViewLoader.DynamicView()
                ' dynamicView.NavigateToView("addnewemployee", Me)

                ' Option 3: If ViewLoader has a shared method (uncomment if needed)
                ' ViewLoader.NavigateToView("addnewemployee", Me)

            Catch ex As Exception
                ' If the navigation method doesn't exist or fails, log the error
                Console.WriteLine($"Navigation failed: {ex.Message}")
                MessageBox.Show("Navigation to Add Employee view is not available.", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            End Try
        End Sub

        ' Helper method to find visual children
        Private Function FindVisualChild(Of T As DependencyObject)(parent As DependencyObject) As T
            If parent Is Nothing Then Return Nothing

            For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(parent) - 1
                Dim child As DependencyObject = VisualTreeHelper.GetChild(parent, i)
                If child IsNot Nothing AndAlso TypeOf child Is T Then
                    Return DirectCast(child, T)
                Else
                    Dim childOfChild As T = FindVisualChild(Of T)(child)
                    If childOfChild IsNot Nothing Then
                        Return childOfChild
                    End If
                End If
            Next
            Return Nothing
        End Function

        ' Method to get current permission summary (useful for reporting)
        Public Function GetPermissionSummary() As List(Of String)
            Dim summary As New List(Of String)

            For Each item In _permissionItems
                Dim roles As New List(Of String)

                If item.HasInventoryManager Then roles.Add("Inventory Manager")
                If item.HasSalesPerson Then roles.Add("Sales Person")
                If item.HasSalesManager Then roles.Add("Sales Manager")
                If item.HasBusinessManager Then roles.Add("Business Manager")
                If item.HasBusinessOwner Then roles.Add("Business Owner")
                If item.HasProjectManager Then roles.Add("Project Manager")

                summary.Add($"{item.Name}: {String.Join(", ", roles)}")
            Next

            Return summary
        End Function

        ' Method to apply bulk permissions (useful for setting default roles)
        Public Sub ApplyBulkPermissions(roleName As String, hasAccess As Boolean)
            For Each item In _permissionItems
                Select Case roleName.ToLower()
                    Case "inventorymanager"
                        item.HasInventoryManager = hasAccess
                    Case "salesperson"
                        item.HasSalesPerson = hasAccess
                    Case "salesmanager"
                        item.HasSalesManager = hasAccess
                    Case "businessmanager"
                        item.HasBusinessManager = hasAccess
                    Case "businessowner"
                        item.HasBusinessOwner = hasAccess
                    Case "projectmanager"
                        item.HasProjectManager = hasAccess
                End Select
            Next
        End Sub
    End Class
End Namespace