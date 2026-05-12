Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports DPC.Data.Helpers
Imports DPC.DPC.Data.Helpers.ViewLoader
Imports MySql.Data.MySqlClient

Namespace DPC.Views.HRM.Employees.Permissions
    Public Class PermissionsEmployee
        Inherits UserControl

        Private moduleData As New List(Of PermissionModule)
        Private _permissionItems As ObservableCollection(Of PermissionItem)
        Private _hasUnsavedChanges As Boolean = False

        Public Sub New()
            InitializeComponent()
            _permissionItems = New ObservableCollection(Of PermissionItem)
            AddHandler Me.Loaded, AddressOf PermissionsEmployee_Loaded
        End Sub

        Private Sub PermissionsEmployee_Loaded(sender As Object, e As RoutedEventArgs)
            RemoveHandler Me.Loaded, AddressOf PermissionsEmployee_Loaded

            LoadPermissionsFromDatabase()

            If dataGrid IsNot Nothing Then
                dataGrid.ItemsSource = _permissionItems
            End If

            RemoveHandler _permissionItems.CollectionChanged, AddressOf OnPermissionItemsChanged
            AddHandler _permissionItems.CollectionChanged, AddressOf OnPermissionItemsChanged

            For Each item In _permissionItems
                RemoveHandler item.PropertyChanged, AddressOf OnPermissionItemPropertyChanged
                AddHandler item.PropertyChanged, AddressOf OnPermissionItemPropertyChanged
            Next
        End Sub

        Private Sub LoadPermissionsFromDatabase()
            _permissionItems.Clear()
            moduleData.Clear()

            ' Match these exactly to the actual columns in your permissions table
            moduleData.Add(New PermissionModule With {.Id = 1, .DisplayName = "Sales", .ColumnName = "Sales"})
            moduleData.Add(New PermissionModule With {.Id = 2, .DisplayName = "Stock", .ColumnName = "Stock"})
            moduleData.Add(New PermissionModule With {.Id = 3, .DisplayName = "CRM", .ColumnName = "Crm"})
            moduleData.Add(New PermissionModule With {.Id = 4, .DisplayName = "Project", .ColumnName = "Project"})
            moduleData.Add(New PermissionModule With {.Id = 5, .DisplayName = "Accounts", .ColumnName = "Accounts"})
            moduleData.Add(New PermissionModule With {.Id = 6, .DisplayName = "Miscellaneous", .ColumnName = "Miscellaneous"})
            moduleData.Add(New PermissionModule With {.Id = 7, .DisplayName = "Assign Project", .ColumnName = "Assign Project"})
            moduleData.Add(New PermissionModule With {.Id = 8, .DisplayName = "Customer Profile", .ColumnName = "Customer Profile"})
            moduleData.Add(New PermissionModule With {.Id = 9, .DisplayName = "Employees", .ColumnName = "Employees"})
            moduleData.Add(New PermissionModule With {.Id = 10, .DisplayName = "Reports", .ColumnName = "Reports"})
            moduleData.Add(New PermissionModule With {.Id = 11, .DisplayName = "Delete", .ColumnName = "Delete"})
            moduleData.Add(New PermissionModule With {.Id = 12, .DisplayName = "POS", .ColumnName = "POS"})
            moduleData.Add(New PermissionModule With {.Id = 13, .DisplayName = "Sales Edit", .ColumnName = "Sales Edit"})
            moduleData.Add(New PermissionModule With {.Id = 14, .DisplayName = "Stock Edit", .ColumnName = "Stock Edit"})

            Dim moduleDict As New Dictionary(Of String, PermissionItem)()

            For Each m In moduleData
                moduleDict(m.DisplayName) = New PermissionItem() With {
                    .Id = m.Id,
                    .Name = m.DisplayName
                }
            Next

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                conn.Open()
                Dim cmd As New MySqlCommand("SELECT * FROM permissions", conn)

                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim role As String = reader("Role").ToString()

                        For Each m In moduleData
                            Dim colName As String = m.ColumnName
                            Dim hasAccess As Boolean = False

                            Try
                                Dim colVal = reader(colName)
                                hasAccess = (colVal IsNot DBNull.Value AndAlso Convert.ToInt32(colVal) = 1)
                            Catch ex As Exception
                                Continue For
                            End Try

                            With moduleDict(m.DisplayName)
                                Select Case role
                                    Case "Inventory Manager" : .HasInventoryManager = hasAccess
                                    Case "Sales Person" : .HasSalesPerson = hasAccess
                                    Case "Sales Manager" : .HasSalesManager = hasAccess
                                    Case "Business Manager" : .HasBusinessManager = hasAccess
                                    Case "Business Owner" : .HasBusinessOwner = hasAccess
                                    Case "Project Manager" : .HasProjectManager = hasAccess
                                    Case "Administrator" : .HasAdministrator = hasAccess
                                    Case "IT" : .HasIT = hasAccess
                                    Case "Tech" : .HasTech = hasAccess
                                End Select
                            End With
                        Next
                    End While
                End Using
            End Using

            For Each m In moduleData
                If moduleDict.ContainsKey(m.DisplayName) Then
                    _permissionItems.Add(moduleDict(m.DisplayName))
                End If
            Next
        End Sub

        Private Sub OnPermissionItemsChanged(sender As Object,
            e As System.Collections.Specialized.NotifyCollectionChangedEventArgs)
            _hasUnsavedChanges = True
            UpdateButtonState()
        End Sub

        Private Sub OnPermissionItemPropertyChanged(sender As Object,
            e As PropertyChangedEventArgs)
            _hasUnsavedChanges = True
            UpdateButtonState()
        End Sub

        Private Sub UpdateButtonState()
            If btnSave IsNot Nothing Then
                btnSave.IsEnabled = _hasUnsavedChanges
                Dim textBlock As TextBlock = FindVisualChild(Of TextBlock)(btnSave)
                If textBlock IsNot Nothing Then
                    textBlock.Text = If(_hasUnsavedChanges, "Update*", "Update")
                End If
            End If
        End Sub

        Private Sub BtnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
            e.Handled = True

            Try
                Dim result As MessageBoxResult = MessageBox.Show(
                    "Are you sure you want to update permissions for all modules?",
                    "Confirm Update",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question)

                If result = MessageBoxResult.Yes Then
                    SaveAllPermissions()
                End If

            Catch ex As Exception
                MessageBox.Show($"Error updating permissions: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub SaveAllPermissions()
            Try
                For Each item As PermissionItem In _permissionItems
                    SavePermissionToDatabase(item)
                Next

                PermissionCache.LoadForRole(PermissionCache.CurrentRole)

                _hasUnsavedChanges = False
                UpdateButtonState()

                MessageBox.Show("Permissions updated successfully.",
                        "Update Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information)
            Catch ex As Exception
                MessageBox.Show($"Error saving permissions: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub SavePermissionToDatabase(item As PermissionItem)
            Dim roleMap As New Dictionary(Of String, Boolean) From {
                {"Inventory Manager", item.HasInventoryManager},
                {"Sales Person", item.HasSalesPerson},
                {"Sales Manager", item.HasSalesManager},
                {"Business Manager", item.HasBusinessManager},
                {"Business Owner", item.HasBusinessOwner},
                {"Project Manager", item.HasProjectManager},
                {"Administrator", item.HasAdministrator},
                {"IT", item.HasIT},
                {"Tech", item.HasTech}
            }

            Dim dbColumnName As String = GetDatabaseColumnName(item.Name)

            If String.IsNullOrWhiteSpace(dbColumnName) Then
                Throw New Exception($"No database column mapping found for module '{item.Name}'.")
            End If

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                conn.Open()

                For Each kvp In roleMap
                    Dim sql = $"UPDATE permissions SET `{dbColumnName}` = @val WHERE Role = @role"
                    Using cmd As New MySqlCommand(sql, conn)
                        cmd.Parameters.AddWithValue("@val", If(kvp.Value, 1, 0))
                        cmd.Parameters.AddWithValue("@role", kvp.Key)
                        cmd.ExecuteNonQuery()
                    End Using
                Next
            End Using
        End Sub

        Private Function GetDatabaseColumnName(moduleName As String) As String
            Select Case moduleName
                Case "Sales"
                    Return "Sales"
                Case "Stock"
                    Return "Stock"
                Case "CRM"
                    Return "Crm"
                Case "Project"
                    Return "Project"
                Case "Accounts"
                    Return "Accounts"
                Case "Miscellaneous"
                    Return "Miscellaneous"
                Case "Assign Project"
                    Return "Assign Project"
                Case "Customer Profile"
                    Return "Customer Profile"
                Case "Employees"
                    Return "Employees"
                Case "Reports"
                    Return "Reports"
                Case "Delete"
                    Return "Delete"
                Case "POS"
                    Return "POS"
                Case "Sales Edit"
                    Return "Sales Edit"
                Case "Stock Edit"
                    Return "Stock Edit"
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Sub BtnAddNew_Click(sender As Object, e As RoutedEventArgs) Handles btnAddNew.Click
            If _hasUnsavedChanges Then
                Dim result = MessageBox.Show(
                    "You have unsaved changes. Save before continuing?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question)

                Select Case result
                    Case MessageBoxResult.Yes
                        SaveAllPermissions()
                        DynamicView.NavigateToView("addnewemployee", Me)
                    Case MessageBoxResult.No
                        DynamicView.NavigateToView("addnewemployee", Me)
                    Case MessageBoxResult.Cancel
                        Return
                End Select
            Else
                DynamicView.NavigateToView("addnewemployee", Me)
            End If
        End Sub

        Private Function FindVisualChild(Of T As DependencyObject)(
            parent As DependencyObject) As T

            If parent Is Nothing Then Return Nothing

            For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(parent) - 1
                Dim child As DependencyObject = VisualTreeHelper.GetChild(parent, i)

                If child IsNot Nothing AndAlso TypeOf child Is T Then
                    Return DirectCast(child, T)
                End If

                Dim found As T = FindVisualChild(Of T)(child)
                If found IsNot Nothing Then Return found
            Next

            Return Nothing
        End Function

    End Class

    Public Class PermissionModule
        Public Property Id As Integer
        Public Property DisplayName As String
        Public Property ColumnName As String
    End Class
End Namespace