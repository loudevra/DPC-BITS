Imports DPC.DPC.Views
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media

Namespace DPC.Data.Helpers.ViewLoader
    ''' <summary>
    ''' Responsible for loading view components by name
    ''' </summary>
    Public Class ViewLoader
        ''' <summary>
        ''' Loads the requested view by name
        ''' </summary>
        ''' <param name="viewName">Name of the view to load</param>
        ''' <returns>UserControl representing the requested view</returns>
        Public Shared Function Load(viewName As String) As UserControl
            Try
                Select Case viewName.ToLower()
                    ' Add new cases here for each view you want to load

                    ' Dashboard Navigation
                    Case "dashboard"
                        Return New Dashboard.Dashboard()

                    ' Stocks Navigation
                    Case "stockstransfer"
                        Return New Stocks.StocksTransfer.StocksTransfer()
                    Case "newsuppliers"
                        Return New Stocks.Supplier.NewSuppliers.NewSuppliers()
                    Case "managesuppliers"
                        Return New Stocks.Suppliers.ManageSuppliers.ManageSuppliers()
                    Case "managebrands"
                        Return New Stocks.Suppliers.ManageBrands.ManageBrands()
                    Case "warehouses"
                        Return New Stocks.Warehouses.Warehouses()
                    Case "productcategories"
                        Return New Stocks.ProductCategories.ProductCategories()
                    Case "manageproducts"
                        Return New Stocks.ItemManager.ProductManager.ManageProducts()
                    Case "newproducts"
                        Return New Stocks.ItemManager.NewProduct.AddNewProducts()
                    Case "batcheditproductvar"
                        Return New Stocks.ItemManager.NewProduct.ProductBatchEdit()
                    Case "productvariationdetails"
                        Return New Stocks.ItemManager.NewProduct.ProductVariationDetails()
                    Case "customlabel"
                        Return New Stocks.ProductsLabel.CustomLabel.CustomLabel()
                    Case "standardlabel"
                        Return New Stocks.ProductsLabel.StandardLabel.StandardLabel()
                    Case "manageorder"
                        Return New Stocks.PurchaseOrder.ManageOrders.ManageOrders()
                    Case "neworder"
                        Return New Stocks.PurchaseOrder.NewOrder.NewOrder()
                    Case "customersrecords"
                        Return New Stocks.StockReturn.CustomersRecords.CustomersRecords()
                    Case "suppliersrecords"
                        Return New Stocks.StockReturn.SupplierRecords.SuppliersRecords()
                        ' Promo Codes Navigation
                    Case "promocodes"
                        Return New PromoCodes.ManagePromoCodes()

                         ' CRM Navigation
                    Case "clientgroups"
                        Return New CRM.ClientGroup.ClientGroups()

                    Case "manageclients"
                        Return New CRM.CRMClients()
                    Case "managetickets"
                        Return New CRM.SupportTicket()
                    Case "newresidentialclient"
                        Return New CRM.CRMNewResidentialClient

                        ' Employees Navigation
                    Case "permissions"
                        Return New HRM.Employees.Permissions.PermissionsEmployee()
                        'Holidays Navigation
                    Case "holidays"
                        Return New HRM.Employees.Holidays.EmployeeHolidays()
                           'Payroll Navigation
                    Case "payrolltransaction"
                        Return New HRM.Employees.Payroll.PayrollTransaction()
                        'Salaries Navigation
                    Case "salaries"
                        Return New HRM.Employees.Salaries.EmployeeSalaries()
                    Case "addpromocode"
                        Return New DPC.Views.PromoCodes.AddPromoCode()
                    Case "editbrand"
                        Return New DPC.Components.Forms.EditBrand()
                        'Departments Navigation
                    Case "departments"
                        Return New HRM.Departments.DepartmentsView()
                         'Attendance Navigation
                    Case "attendance"
                        Return New HRM.Employees.Attendance.AttendanceEmployee()
                    Case "addnewemployee"
                        Return New HRM.Employees.Employees.AddEmployee()
                    Case "viewemployee"
                        Return New HRM.Employees.Employees.EmployeesView()
                        ' Accounts Navigation
                    Case "manageaccounts"
                        Return New Accounts.Accounts.ManageAccounts.ManageAccounts()


                    Case Else
                        ' Return a placeholder UserControl with error text
                        Dim errorContent As New TextBlock With {
                            .Text = $"View not found: {viewName}",
                            .FontSize = 20,
                            .HorizontalAlignment = HorizontalAlignment.Center,
                            .VerticalAlignment = VerticalAlignment.Center
                        }
                        Return New UserControl With {.Content = errorContent}
                End Select
            Catch ex As Exception
                MessageBox.Show($"Error loading view '{viewName}': {ex.Message}")
                ' Return an error UserControl in case of exception
                Dim errorContent As New TextBlock With {
                    .Text = $"Error loading view: {viewName}",
                    .FontSize = 20,
                    .Foreground = New SolidColorBrush(Colors.Red),
                    .HorizontalAlignment = HorizontalAlignment.Center,
                    .VerticalAlignment = VerticalAlignment.Center
                }
                Return New UserControl With {.Content = errorContent}
            End Try
        End Function

        ''' <summary>
        ''' Helper function to get the name of a view
        ''' </summary>
        Public Shared Function GetViewName(view As Object) As String
            If view Is Nothing Then Return String.Empty

            Dim typeName As String = view.GetType().Name.ToLower()
            ' Check if the type name is a known view type

            ' Dashboard Navigation
            If typeName = "dashboard" Then
                Return "dashboard"

                ' Stocks Navigation
            ElseIf typeName = "stockstransfer" Then
                Return "stocks.stocktransfer"
            ElseIf typeName = "newsuppliers" Then
                Return "newsuppliers"
            ElseIf typeName = "managesuppliers" Then
                Return "managesuppliers"
            ElseIf typeName = "managebrands" Then
                Return "managebrands"
            ElseIf typeName = "warehouses" Then
                Return "warehouses"
            ElseIf typeName = "productcategories" Then
                Return "productcategories"
            ElseIf typeName = "manageproducts" Then
                Return "manageproducts"
            ElseIf typeName = "newproducts" Then
                Return "newproducts"
            ElseIf typeName = "batcheditproductvar" Then
                Return "batcheditproductvar"
            ElseIf typeName = "productvariationdetails" Then
                Return "productvariationdetails"
            ElseIf typeName = "customlabel" Then
                Return "customlabel"
            ElseIf typeName = "standardlabel" Then
                Return "standardlabel"
            ElseIf typeName = "manageorder" Then
                Return "manageorder"
            ElseIf typeName = "standardlabel" Then
                Return "standardlabel"
            ElseIf typeName = "customersrecord" Then
                Return "customersrecord"
            ElseIf typeName = "suppliersrecord" Then
                Return "suppliersrecord"


                ' Promo Codes Navigation
            ElseIf typeName = "promocodes" Then
                Return "promocodes"

                ' Employees Navigation
            ElseIf typeName = "permissions" Then
                Return "permissions"

            ElseIf typeName = "holidays" Then
                Return "holidays"

                ' ClientGroup Navigation
            ElseIf typeName = "clientgroups" Then
                Return "clientgroups"

                ' Client Navigation
            ElseIf typeName = "manageclients" Then
                Return "manageclients"
                ' Tickets Navigation
            ElseIf typeName = "managetickets" Then
                Return "managetickets"
            ElseIf typeName = "newresidentialclient" Then
                Return "newresidentialclient"

                ' Salaries Navigation
            ElseIf typeName = "salaries" Then
                Return "salaries"

                ' addpromocode Navigation
            ElseIf typeName = "addpromocode" Then
                Return "addpromocode"

                ' EditBrand Navigation
            ElseIf typeName = "editbrand" Then
                Return "editbrand"
                ' departments Navigation
            ElseIf typeName = "departments" Then
                Return "departments"
                ' attendance Navigation
            ElseIf typeName = "attendance" Then
                Return "attendance"
                ' Payroll Navigation
            ElseIf typeName = "payrolltransaction" Then
                Return "payrolltransaction"
            ElseIf typeName = "addnewemployee" Then
                Return "addnewemployee"
            ElseIf typeName = "viewemployee" Then
                Return "viewemployee"

                ' Accounts Navigation
            ElseIf typeName = "manageaccounts" Then
                Return "manageaccounts"
            Else
                Return typeName
            End If
        End Function
    End Class
End Namespace