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

        ' FIXED: Was Shared (class-level), which caused duplicates on re-open.
        ' Now it's Private (instance-level) so it resets every time the view loads.
        Private moduleData As New List(Of Object)

        Private _permissionItems As ObservableCollection(Of PermissionItem)
        Private _hasUnsavedChanges As Boolean = False

        Public Sub New()
            InitializeComponent()
            _permissionItems = New ObservableCollection(Of PermissionItem)
            AddHandler Me.Loaded, AddressOf PermissionsEmployee_Loaded
        End Sub

        Private Sub PermissionsEmployee_Loaded(sender As Object, e As RoutedEventArgs)
            LoadPermissionsFromDatabase()

            If dataGrid IsNot Nothing Then
                dataGrid.ItemsSource = _permissionItems
            End If

            ' Track changes so the Update button activates
            AddHandler _permissionItems.CollectionChanged, AddressOf OnPermissionItemsChanged
            For Each item In _permissionItems
                AddHandler item.PropertyChanged, AddressOf OnPermissionItemPropertyChanged
            Next
        End Sub

        Private Sub LoadPermissionsFromDatabase()
            _permissionItems.Clear()
            moduleData.Clear() ' Clear instance list before repopulating

            ' Define the known modules and their display order
            moduleData.Add(New With {.Id = 1, .Name = "Dashboard"})
            moduleData.Add(New With {.Id = 2, .Name = "Sales"})
            moduleData.Add(New With {.Id = 3, .Name = "Stocks"})
            moduleData.Add(New With {.Id = 4, .Name = "CRM"})
            moduleData.Add(New With {.Id = 5, .Name = "Project"})
            moduleData.Add(New With {.Id = 6, .Name = "Data & Reports"})
            moduleData.Add(New With {.Id = 7, .Name = "Miscellaneous"})
            moduleData.Add(New With {.Id = 8, .Name = "HRM"})
            moduleData.Add(New With {.Id = 9, .Name = "Software Updates"})

            ' Build one PermissionItem per module, fill role columns from DB
            Dim moduleDict As New Dictionary(Of String, PermissionItem)()

            ' Pre-create a PermissionItem for each known module
            For Each m In moduleData
                moduleDict(m.Name) = New PermissionItem() With {
                    .Id = m.Id,
                    .Name = m.Name
                }
            Next

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                conn.Open()
                Dim cmd As New MySqlCommand("SELECT * FROM permissions", conn)

                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        Dim role As String = reader("Role").ToString()

                        ' For each module column in the DB row,
                        ' assign the boolean to the correct role property
                        For Each m In moduleData
                            Dim colName As String = m.Name
                            Dim hasAccess As Boolean = False

                            Try
                                Dim colVal = reader(colName)
                                hasAccess = (colVal IsNot DBNull.Value AndAlso
                                             Convert.ToInt32(colVal) = 1)
                            Catch
                                ' Column might not exist for this module name — skip
                                Continue For
                            End Try

                            With moduleDict(colName)
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

            ' Add to observable collection in module order
            For Each m In moduleData
                If moduleDict.ContainsKey(m.Name) Then
                    _permissionItems.Add(moduleDict(m.Name))
                End If
            Next
        End Sub

        ' ---- Change Tracking ----

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

        ' ---- Save ----

        Private Sub BtnSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSave.Click
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

                ' ADD THIS — refreshes cache immediately after save
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
            ' Maps role display name -> which property on PermissionItem holds its value
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

            For Each kvp In roleMap
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    ' FIXED: Using parameters (@val, @role) instead of string 
                    ' concatenation — prevents SQL injection.
                    ' Backticks around column name handle spaces (e.g. "Sales Edit")
                    Dim sql = $"UPDATE permissions SET `{item.Name}` = @val WHERE Role = @role"
                    Dim cmd As New MySqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@val", If(kvp.Value, 1, 0))
                    cmd.Parameters.AddWithValue("@role", kvp.Key)
                    cmd.ExecuteNonQuery()
                End Using
            Next
        End Sub

        ' ---- Navigation ----

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

        ' ---- Helpers ----

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
End Namespace

