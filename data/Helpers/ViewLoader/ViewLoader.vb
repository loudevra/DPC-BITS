Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports DPC.DPC.Views
Imports DPC.DPC.Views.Accounts.Accounts.ManageAccounts
Imports DPC.DPC.Views.Stocks.PurchaseOrder.Delivery
Imports DPC.DPC.Views.Stocks.Suppliers.NewSupplier

Namespace DPC.Data.Helpers.ViewLoader
    Public Class ViewLoader
        Public Sub New()
        End Sub

        Public Shared Function Load(viewName As String) As UserControl
            Try
                Select Case viewName.ToLower()

                    Case "dashboard"
                        Return New Dashboard.Dashboard()

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
                    Case "consumables"
                        Return New Stocks.ItemManager.Consumables.Consumables()
                    Case "newproducts"
                        Return New Stocks.ItemManager.NewProduct.AddNewProducts()
                    Case "editproduct"
                        Return New Stocks.ItemManager.ProductManager.EditProduct()
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
                    Case "promocodes"
                        Return New PromoCodes.ManagePromoCodes()
                    Case "addpromocode"
                        Return New PromoCodes.AddPromoCode()
                    Case "newwalkinclient"
                        Return New Stocks.PurchaseOrder.WalkIn.AddNewWalkInClient()
                    Case "walkinorder"
                        Return New Stocks.PurchaseOrder.WalkIn.WalkInNewOrder()
                    Case "newdelivery"
                        Return New Stocks.PurchaseOrder.Delivery.NewDelivery()
                    Case "previeweditabledeliveryeeceipt"
                        Return New Stocks.PurchaseOrder.Delivery.PreviewEditableDeliveryReceipt()
                    Case "previewprintdeliveryreceipt"
                        Return New Stocks.PurchaseOrder.Delivery.PreviewPrintDeliveryReceipt()
                    Case "clientgroups"
                        Return New CRM.ClientGroup.ClientGroups()
                    Case "addnewclientgroup"
                        Return New CRM.ClientGroup.AddNewClientGroup()
                    Case "manageclients"
                        Return New CRM.CRMClients()
                    Case "managetickets"
                        Return New CRM.SupportTicket()
                    Case "newresidentialclient"
                        Return New CRM.CRMNewResidentialClient()
                    Case "editresidentialclient"
                        Return New CRM.CRMEditResidentialClient()
                    Case "newcorporationalclient"
                        Return New CRM.CRMCorporationalTabStructured()
                    Case "selectclients"
                        Return New CRM.SelectClients()
                    Case "businnessregisters"
                        Return New DPC.Views.DataReports.BusinessRegisters.DTRBusinessRegisters()
                    Case "generatestatement"
                        Return New DPC.Views.DataReports.Statements.DTRTabStructured()
                    Case "permissions"
                        Return New HRM.Employees.Permissions.PermissionsEmployee()
                    Case "holidays"
                        Return New HRM.Employees.Holidays.EmployeeHolidays()
                    Case "payrolltransaction"
                        Return New HRM.Employees.Payroll.PayrollTransaction()
                    Case "salaries"
                        Return New HRM.Employees.Salaries.EmployeeSalaries()
                    Case "addnewsalaries"
                        Return New HRM.Employees.Employees.AddEmployee()
                    Case "editbrand"
                        Return New DPC.Components.Forms.EditBrand()
                    Case "departments"
                        Return New HRM.Departments.DepartmentsView()
                    Case "attendance"
                        Return New HRM.Employees.Attendance.AttendanceEmployee()
                    Case "addnewemployee"
                        Return New HRM.Employees.Employees.AddEmployee()
                    Case "viewemployee"
                        Return New HRM.Employees.Employees.EmployeesView()
                    Case "hrmeditemployee"
                        Return New HRM.Employees.Employees.EditEmployee()
                    Case "addproject"
                        Return New Views.Project.AddProject1()
                    Case "salesnewinvoice"
                        Return New Views.Sales.Saless.SalesNewInvoice()
                    Case "manageposinvoices"
                        Return New Sales.POSSales.ManagePOSInvoices()
                    Case "salesinvoices"
                        Return New POS.POS()
                    Case "salesquote"
                        Return New Sales.Quotes.Quote()
                    Case "salesnewquote"
                        Return New Sales.Quotes.NewQuote()
                    Case "managewalkin"
                        Return New Sales.Quotes.ManageWalkInClients()
                    Case "salesquotegovernmentmanage"
                        Return New Sales.Quotes.ManageQuoteGovernment()
                    Case "creditnote"
                        Return New Sales.CreditNotes.CreditNotes()
                    Case "newsubscriptioninvoice"
                        Return New Sales.Subscriptions.NewSubscriptionInvoice()
                    Case "newquote"
                        Return New Sales.Quotes.NewQuote()
                    Case "salesnewposinvoice"
                        Return New Views.POS.SalesNewInvoice()
                    Case "salesnewposinvoicemobile"
                        Return New Views.POS.SalesNewInvoiceMobile()
                    Case "manageaccounts"
                        Return New Accounts.Accounts.ManageAccounts.ManageAccounts()
                    Case "accountsexpense"
                        Return New Accounts.Expense.ExpenseTransaction()
                    Case "accountsincome"
                        Return New Accounts.Income.IncomeTransactions()
                    Case "viewtransactions"
                        Return New Accounts.Transactions.ManageTransactions()
                    Case "addnewtransaction"
                        Return New Accounts.Accounts.ManageAccounts.AddNewTransaction()
                    Case "balancesheets"
                        Return New Accounts.Accounts.BalanceSheets()
                    Case "accountstatement"
                        Return New Accounts.Accounts.ManageAccounts.AccountStatement()
                    Case "addnewtransfer"
                        Return New Accounts.Accounts.ManageAccounts.AddNewTransfer()
                    Case "clienttransactions"
                        Return New Accounts.Transactions.ClientsTransactions()
                    Case "navaddaccount"
                        Return New Accounts.Accounts.ManageAccounts.AddAccount()
                    Case "addclienttabs"
                        Return New Accounts.Transactions.ClientAddTabs.AddClientTabs()
                    Case "newproject"
                        Return New Project.AddProject1()
                    Case "addproject2"
                        Return New Project.AddProject2()
                    Case "addproject3"
                        Return New Project.AddProject3()
                    Case "manageproject"
                        Return New Project.ManageProject()
                    Case "todolist"
                        Return New Project.ToDoList()
                    Case "costestimate"
                        Return New Sales.Quotes.CostEstimate()
                    Case "costestimategovernment"
                        Return New Sales.Quotes.CostEstimateGovernment()
                    Case "editquote"
                        Return New Sales.Quotes.EditQuote()
                    Case "previewprintquoteeditedquote"
                        Return New DPC.Components.Forms.PreviewPrintEditedQuote()
                    Case "billingestimate"
                        Return New Sales.Quotes.BillingStatement()
                    Case "navigatetoquotes"
                        Return New Sales.Quotes.NewQuote()
                    Case "printpreviewquotes"
                        Return New DPC.Components.Forms.PreviewPrintQuote()
                    Case "printpreviewquotesgovernment"
                        Return New DPC.Components.Forms.PreviewPrintQuoteGovernment()
                    Case "newsubscriptions"
                        Return New Sales.Subscriptions.NewSubscriptionInvoice()
                    Case "subscriptions"
                        Return New Sales.Subscriptions.Subscriptions()
                    Case "purchaseorderstatement"
                        Return New Stocks.PurchaseOrder.NewOrder.BillingStatement()
                    Case "printpreview"
                        Return New DPC.Components.Forms.PreviewPrintStatement()
                    Case "addcustomlabel"
                        Return New Stocks.ProductsLabel.CustomLabel.CustomLabel()
                    Case "addstandardlabel"
                        Return New Stocks.ProductsLabel.StandardLabel.StandardLabel()
                    Case "managedeliveryreceipt"
                        Return New Stocks.PurchaseOrder.Delivery.ManageDeliveryReceipt()
                    Case "editsuppliers"
                        Return New Stocks.Suppliers.NewSupplier.EditSuppliers()
                    Case "pulloutreceipt"
                        Return New Misc.Documents.PullOutForm
                    Case "pulloutpreview"
                        Return New DPC.Components.Forms.PreviewPulloutReceipt()
                    Case "navigatetobillingstatement"
                        Return New Stocks.PurchaseOrder.WalkIn.WalkInBillingStatement()
                    Case "previewwalkinclientprintstatement"
                        Return New Stocks.PurchaseOrder.WalkIn.PreviewWalkinClientPrintStatement()
                    Case "navigatetocostestimate"
                        Return New Sales.Quotes.CostEstimate()
                    Case "hrmuploadfiles"
                        Return New Views.DataReports.UploadFileOnline.UploadFileOnline()
                    Case "hrmmanageregularcostestimatefiles"
                        Return New Views.DataReports.ManageRegularCostEstimateFiles.ManageRegularCostEstimateFiles()
                    Case "hrmmanagegovernmentcostestimatefiles"
                        Return New Views.DataReports.ManageGovernmentCostEstimateFiles.ManageGovernmentCostEstimateFiles()
                    Case "cashadvancenewrequest"
                        Return New Views.Misc.CashAdvance.CashAdvanceNewRequest()
                    Case "managecashadvancerequests"
                        Return New Views.Misc.CashAdvance.ManageCashAdvanceRequests()
                    Case "editcashadvancerequest"
                        Return New Views.Misc.CashAdvance.EditCashAdvanceRequest()
                    Case "previewprintcashadvancerequestform"
                        Return New Views.Misc.CashAdvance.PreviewPrintCashAdvanceRequestForm()
                    Case "overtimerequestform"
                        Return New Views.Misc.OverTime.OverTimeRequestForm()
                    Case "manageovertimerequests"
                        Return New Views.Misc.OverTime.ManageTimeoutRequests()
                    Case "editovertime"
                        Return New Views.Misc.OverTime.EditOverTime()

                    Case Else
                        Dim errorContent As New TextBlock With {
                            .Text = $"View Not found: {viewName}",
                            .FontSize = 20,
                            .HorizontalAlignment = HorizontalAlignment.Center,
                            .VerticalAlignment = VerticalAlignment.Center
                        }
                        Return New UserControl With {.Content = errorContent}
                End Select
            Catch ex As Exception
                MessageBox.Show($"Error loading view '{viewName}': {ex.Message}")
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

        Public Shared Function GetViewName(view As Object) As String
            If view Is Nothing Then Return String.Empty

            Dim typeName As String = view.GetType().Name.ToLower()

            If typeName = "dashboard" Then
                Return "dashboard"
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
            ElseIf typeName = "customersrecord" Then
                Return "customersrecord"
            ElseIf typeName = "suppliersrecord" Then
                Return "suppliersrecord"
            ElseIf typeName = "editproduct" Then
                Return "editproduct"
            ElseIf typeName = "previewwalkinclientprintstatement" Then
                Return "previewwalkinclientprintstatement"
            ElseIf typeName = "hrmmanagecostestimatefiles" Then
                Return "hrmmanagecostestimatefiles"
            ElseIf typeName = "managedeliveryreceipt" Then
                Return "managedeliveryreceipt"
            ElseIf typeName = "promocodes" Then
                Return "promocodes"
            ElseIf typeName = "addpromocode" Then
                Return "addpromocode"
            ElseIf typeName = "permissions" Then
                Return "permissions"
            ElseIf typeName = "holidays" Then
                Return "holidays"
            ElseIf typeName = "clientgroups" Then
                Return "clientgroups"
            ElseIf typeName = "addnewclientgroup" Then
                Return "addnewclientgroup"
            ElseIf typeName = "manageclients" Then
                Return "manageclients"
            ElseIf typeName = "managetickets" Then
                Return "managetickets"
            ElseIf typeName = "newresidentialclient" Then
                Return "newresidentialclient"
            ElseIf typeName = "newcorporationalclient" Then
                Return "newcorporationalclient"
            ElseIf typeName = "businnessregisters" Then
                Return "businnessregisters"
            ElseIf typeName = "generatestatement" Then
                Return "generatestatement"
            ElseIf typeName = "selectclients" Then
                Return "selectclients"
            ElseIf typeName = "salaries" Then
                Return "salaries"
            ElseIf typeName = "editbrand" Then
                Return "editbrand"
            ElseIf typeName = "departments" Then
                Return "departments"
            ElseIf typeName = "attendance" Then
                Return "attendance"
            ElseIf typeName = "payrolltransaction" Then
                Return "payrolltransaction"
            ElseIf typeName = "addnewemployee" Then
                Return "addnewemployee"
            ElseIf typeName = "hrmeditemployee" Then
                Return "hrmeditemployee"
            ElseIf typeName = "viewemployee" Then
                Return "viewemployee"
            ElseIf typeName = "salesnewinvoice" Then
                Return "salesnewinvoice"
            ElseIf typeName = "manageposinvoices" Then
                Return "manageposinvoices"
            ElseIf typeName = "salesinvoices" Then
                Return "salesinvoices"
            ElseIf typeName = "salesquote" Then
                Return "salesquote"
            ElseIf typeName = "salesnewquote" Then
                Return "salesnewquote"
            ElseIf typeName = "printpreviewquotes" Then
                Return "printpreviewquotes"
            ElseIf typeName = "creditnote" Then
                Return "creditnote"
            ElseIf typeName = "newsubscriptioninvoice" Then
                Return "newsubscriptioninvoice"
            ElseIf typeName = "editquote" Then
                Return "editquote"
            ElseIf typeName = "previewprintquoteeditedquote" Then
                Return "previewprintquoteeditedquote"
            ElseIf typeName = "quote" Then
                Return "quote"
            ElseIf typeName = "manageaccounts" Then
                Return "manageaccounts"
            ElseIf typeName = "accountsexpense" Then
                Return "accountsexpense"
            ElseIf typeName = "accountsincome" Then
                Return "accountsincome"
            ElseIf typeName = "viewtransactions" Then
                Return "viewtransactions"
            ElseIf typeName = "addnewtransaction" Then
                Return "addnewtransaction"
            ElseIf typeName = "balancesheets" Then
                Return "balancesheets"
            ElseIf typeName = "accountstatement" Then
                Return "accountstatement"
            ElseIf typeName = "addnewtransfer" Then
                Return "addnewtransfer"
            ElseIf typeName = "clienttransactions" Then
                Return "clienttransactions"
            ElseIf typeName = "navaddaccount" Then
                Return "navaddaccount"
            ElseIf typeName = "hrmuploadfiles" Then
                Return "hrmuploadfiles"
            ElseIf typeName = "newproject" Then
                Return "newproject"
            ElseIf typeName = "editsuppliers" Then
                Return "editsuppliers"
            ElseIf typeName = "addproject2" Then
                Return "addproject2"
            ElseIf typeName = "addproject3" Then
                Return "addproject3"
            ElseIf typeName = "manageproject" Then
                Return "manageproject"
            ElseIf typeName = "todolist" Then
                Return "todolist"
            ElseIf typeName = "purchaseorderstatement" Then
                Return "purchaseorderstatement"
            ElseIf typeName = "printpreview" Then
                Return "printpreview"
            ElseIf typeName = "cashadvancenewrequest" Then
                Return "cashadvancenewrequest"
            ElseIf typeName = "overtimerequestform" Then
                Return "overtimerequestform"
            ElseIf typeName = "manageovertimerequests" Then
                Return "manageovertimerequests"
            ElseIf typeName = "editovertime" Then
                Return "editovertime"
            ElseIf typeName = "overtimereceiptview" Then
                Return "overtimereceiptview"
            ElseIf typeName = "managecashadvancerequests" Then
                Return "managecashadvancerequests"
            ElseIf typeName = "editcashadvancerequest" Then
                Return "editcashadvancerequest"
            ElseIf typeName = "previewprintcashadvancerequestform" Then
                Return "previewprintcashadvancerequestform"
            ElseIf typeName = "pulloutreceipt" Then
                Return "pulloutreceipt"
            ElseIf typeName = "pulloutpreview" Then
                Return "pulloutpreview"
            ElseIf typeName = "consumables" Then
                Return "consumables"
            Else
                Return typeName
            End If
        End Function
    End Class
End Namespace