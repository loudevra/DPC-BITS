Imports ClosedXML.Excel
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Helpers
Imports Microsoft.Win32
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Data
Imports System.Reflection
Imports System.Windows.Controls.Primitives
Imports MySql.Data.MySqlClient

Namespace DPC.Components.Forms
    Public Class AddDepartment
        Inherits UserControl

        ' Properties for pagination
        Private _paginationHelper As PaginationHelper
        Private _searchFilterHelper As SearchFilterHelper

        ' UI elements for direct access
        Private popup As Popup
        Private recentlyClosed As Boolean = False

        ' Closing the popup
        Public Event close(sender As Object, e As RoutedEventArgs)
        ' Refresh the data
        Public Event DepartmentSaved As EventHandler

        Public Sub New()
            InitializeComponent()
            InitializeControls()

            AddHandler TxtDepartment.TextChanged, AddressOf TxtToUpper_TextChanged
        End Sub

        Private Sub TxtToUpper_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return
            Dim originalSelectionStart = tb.SelectionStart
            Dim originalSelectionLength = tb.SelectionLength
            Dim originalText = tb.Text
            Dim upperText = originalText.ToUpperInvariant()
            If Not String.Equals(originalText, upperText, StringComparison.Ordinal) Then
                RemoveHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
                tb.Text = upperText
                tb.SelectionStart = Math.Min(originalSelectionStart, tb.Text.Length)
                tb.SelectionLength = originalSelectionLength
                AddHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
            End If
        End Sub

        Private Sub InitializeControls()

            ' Initialize and load product categories data
            ' LoadData()
        End Sub

        Private Sub ClosePopup_Click(sender As Object, e As RoutedEventArgs)
            ' Raise the close event
            RaiseEvent close(Me, e)
            PopupHelper.ClosePopup()
        End Sub

        Private Sub AddDepartment(sender As Object, e As RoutedEventArgs)
            Try
                Dim departmentName As String = TxtDepartment.Text()

                If HRMController.InsertDepartment(departmentName) Then
                    HRMController.ActionLogs(CacheOnLoggedInName, "Insert", Nothing, departmentName)

                    RaiseEvent close(Me, e)
                    PopupHelper.ClosePopup()
                    RaiseEvent DepartmentSaved(Me, EventArgs.Empty)
                    MessageBox.Show("Department inserted successfully.")
                Else
                    MessageBox.Show("Failed to insert department.")
                End If
            Catch ex As Exception
                MessageBox.Show("Error database addDepartment.")
            End Try
        End Sub
    End Class
End Namespace