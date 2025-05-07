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

Namespace DPC.Views.HRM.Employees.Payroll
    ''' <summary>
    ''' Interaction logic for ProductCategories.xaml
    ''' </summary>
    Public Class PayrollTransaction
        Inherits UserControl

        ' Properties for pagination
        Private _paginationHelper As PaginationHelper
        Private _searchFilterHelper As SearchFilterHelper

        ' UI elements for direct access
        Private popup As Popup
        Private recentlyClosed As Boolean = False

        Public Sub New()
            InitializeComponent()

        End Sub


        Private Sub PayrollTransactionControl(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the AddCategory form
            Dim payrollTransactionControl As New DPC.Components.Forms.PayrollTransactionControl()



            ' Get the parent window to center the popup
            Dim parentWindow = Window.GetWindow(Me)

            ' Open the popup
            PopupHelper.OpenPopupWithControl(sender, payrollTransactionControl, "windowcenter", True, -50, 0, parentWindow)
        End Sub
    End Class
End Namespace