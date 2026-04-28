Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports DPC.DPC.Views
Imports DPC.DPC.Views.Accounts.Accounts.ManageAccounts
Imports DPC.DPC.Views.Stocks.PurchaseOrder.Delivery
Imports DPC.DPC.Views.Stocks.Suppliers.NewSupplier
Imports DPC.Views.Misc.EmployeeLeave

Namespace DPC.Data.Helpers.ViewLoader
    ''' <summary>
    ''' Responsible for loading view components by name
    ''' </summary>
    Public Class ViewLoader
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Loads the requested view by name
        ''' </summary>
        ''' <param name="viewName">Name of the view to load</param>
        ''' <returns>UserControl representing the requested view</returns>
        Public Shared Function Load(viewName As String) As UserControl
            Try
                Select Case viewName.ToLower()

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

                    ' Promo Codes
                    Case "promocodes"
                        Return New PromoCodes.ManagePromoCodes()
                    Case "addpromocode"
                        Return New PromoCodes.AddPromoCode()
                    Case "employeesprofileview"
                        Return New DPC.Views.HRM.Employees.Employees.EmployeesProfileView()

                    ' Purchase Order / Delivery / Walk-in
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


                    ' CRM Navigation
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

                    ' Employees / HRM
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

                    ' Project
                    Case "addproject"
                        Return New Views.Project.AddProject1()
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
                    Case "newtask"
                        Return New NewTask()
                    Case "managetask"
                        Return New ManageTask()
                    Case "edittask"
                        Return New EditTask()
                    Case "editproject"
                        Return New EditProject()

                    ' Sales Module
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
                    Case "salesnewposinvoice"
                        Return New Views.POS.SalesNewInvoice()
                    Case "salesnewposinvoicemobile"
                        Return New Views.POS.SalesNewInvoiceMobile()

                    ' Accounts Navigation
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

                    ' Cost Estimates
                    Case "costestimate"
                        Return New Sales.Quotes.CostEstimate()
                    Case "costestimategovernment"
                        Return New Sales.Quotes.CostEstimateGovernment()
                    Case "editquote"
                        Return New Sales.Quotes.EditQuote()
                    Case "universaleditablepreviewdocument"
                        Return New DPC.Components.Forms.UniversalEditablePreviewDocument()
                    Case "universalprintablepreviewdocument"
                        Return New DPC.Components.Forms.UniversalPrintablePreviewDocument()

                    ' Sales Billing Estimate / Statement of Account
                    Case "billingestimate"
                        Return New Sales.Quotes.BillingStatement()
                    Case "navigatetoquotes"
                        Return New Sales.Quotes.NewQuote()
                    Case "newsubscriptions"
                        Return New Sales.Subscriptions.NewSubscriptionInvoice()
                    Case "subscriptions"
                        Return New Sales.Subscriptions.Subscriptions()
                    Case "statementofaccount"
                        Return New StatementOfAccountForm()
                    Case "managestatementofaccount"
                        Return New ManageStatementOfAccount()

                    ' Purchase / Preview
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

                    ' Pull Out Form
                    Case "pulloutreceipt"
                        Return New Misc.Documents.PullOutForm()
                    Case "pulloutpreview"
                        Return New DPC.Components.Forms.PreviewPulloutReceipt()

                    ' POS Navigation continued
                    Case "navigatetobillingstatement"
                        Return New Stocks.PurchaseOrder.WalkIn.WalkInBillingStatement()
                    Case "previewwalkinclientprintstatement"
                        Return New Stocks.PurchaseOrder.WalkIn.PreviewWalkinClientPrintStatement()
                    Case "navigatetocostestimate"
                        Return New Sales.Quotes.CostEstimate()

                    ' DataReports - Upload
                    Case "hrmuploadfiles"
                        Return New Views.DataReports.UploadFileOnline.UploadFileOnline()
                    Case "hrmmanageregularcostestimatefiles"
                        Return New Views.DataReports.ManageRegularCostEstimateFiles.ManageRegularCostEstimateFiles()
                    Case "hrmmanagegovernmentcostestimatefiles"
                        Return New Views.DataReports.ManageGovernmentCostEstimateFiles.ManageGovernmentCostEstimateFiles()

                    ' Misc - Cash Advance
                    Case "cashadvancenewrequest"
                        Return New Views.Misc.CashAdvance.CashAdvanceNewRequest()
                    Case "managecashadvancerequests"
                        Return New Views.Misc.CashAdvance.ManageCashAdvanceRequests()
                    Case "editcashadvancerequest"
                        Return New Views.Misc.CashAdvance.EditCashAdvanceRequest()
                    Case "previewprintcashadvancerequestform"
                        Return New Views.Misc.CashAdvance.PreviewPrintCashAdvanceRequestForm()

                    ' Misc - Overtime
                    Case "overtimerequestform"
                        Return New Views.Misc.OverTime.OverTimeRequestForm()
                    Case "manageovertimerequests"
                        Return New Views.Misc.OverTime.ManageTimeoutRequests()
                    Case "editovertime"
                        Return New Views.Misc.OverTime.EditOverTime()
                    Case "previewprintovertimerequestform"
                        Return New Views.Misc.OverTime.PreviewPrintOverTimeRequestForm()

                    ' Misc - Employee Leave
                    Case "employeeleaverequestform"
                        Return New DPC.Views.Misc.EmployeeLeave.EmployeeLeaveRequestForm()
                    Case "manageemployeeleaverequests"
                        Return New DPC.Views.Misc.EmployeeLeave.ManageEmployeeLeaveRequests()
                    Case "editemployeeleave"
                        Return New DPC.Views.Misc.EmployeeLeave.EditEmployeeLeave()
                    Case "previewprintemployeeleave"
                        Return New DPC.Views.Misc.EmployeeLeave.PreviewPrintEmployeeLeave()

                    ' Calendar
                    Case "calendarview"
                        Return New calendarview()
                    Case "addevent"
                        Return New DPC.Views.Misc.Calendar.Addevent()
                    Case "employeesprofileview"
                        Return New DPC.Views.HRM.Employees.Employees.EmployeesProfileView()


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

        ''' <summary>
        ''' Helper function to get the name of a view
        ''' </summary>
        Public Shared Function GetViewName(view As Object) As String
            If view Is Nothing Then Return String.Empty

            Dim typeName As String = view.GetType().Name.ToLower()

            If typeName = "dashboard" Then Return "dashboard"
            If typeName = "stockstransfer" Then Return "stocks.stocktransfer"
            If typeName = "newsuppliers" Then Return "newsuppliers"
            If typeName = "managesuppliers" Then Return "managesuppliers"
            If typeName = "managebrands" Then Return "managebrands"
            If typeName = "warehouses" Then Return "warehouses"
            If typeName = "productcategories" Then Return "productcategories"
            If typeName = "manageproducts" Then Return "manageproducts"
            If typeName = "newproducts" Then Return "newproducts"
            If typeName = "batcheditproductvar" Then Return "batcheditproductvar"
            If typeName = "productvariationdetails" Then Return "productvariationdetails"
            If typeName = "customlabel" Then Return "customlabel"
            If typeName = "standardlabel" Then Return "standardlabel"
            If typeName = "manageorder" Then Return "manageorder"
            If typeName = "customersrecord" Then Return "customersrecord"
            If typeName = "suppliersrecord" Then Return "suppliersrecord"
            If typeName = "editproduct" Then Return "editproduct"
            If typeName = "managestatementofaccount" Then Return "managestatementofaccount"
            If typeName = "previewwalkinclientprintstatement" Then Return "previewwalkinclientprintstatement"
            If typeName = "hrmmanagecostestimatefiles" Then Return "hrmmanagecostestimatefiles"
            If typeName = "managedeliveryreceipt" Then Return "managedeliveryreceipt"
            If typeName = "promocodes" Then Return "promocodes"
            If typeName = "addpromocode" Then Return "addpromocode"
            If typeName = "permissions" Then Return "permissions"
            If typeName = "holidays" Then Return "holidays"
            If typeName = "clientgroups" Then Return "clientgroups"
            If typeName = "addnewclientgroup" Then Return "addnewclientgroup"
            If typeName = "manageclients" Then Return "manageclients"
            If typeName = "managetickets" Then Return "managetickets"
            If typeName = "newresidentialclient" Then Return "newresidentialclient"
            If typeName = "newcorporationalclient" Then Return "newcorporationalclient"
            If typeName = "businnessregisters" Then Return "businnessregisters"
            If typeName = "generatestatement" Then Return "generatestatement"
            If typeName = "selectclients" Then Return "selectclients"
            If typeName = "salaries" Then Return "salaries"
            If typeName = "editbrand" Then Return "editbrand"
            If typeName = "departments" Then Return "departments"
            If typeName = "attendance" Then Return "attendance"
            If typeName = "payrolltransaction" Then Return "payrolltransaction"
            If typeName = "addnewemployee" Then Return "addnewemployee"
            If typeName = "hrmeditemployee" Then Return "hrmeditemployee"
            If typeName = "viewemployee" Then Return "viewemployee"
            If typeName = "salesnewinvoice" Then Return "salesnewinvoice"
            If typeName = "manageposinvoices" Then Return "manageposinvoices"
            If typeName = "salesinvoices" Then Return "salesinvoices"
            If typeName = "salesquote" Then Return "salesquote"
            If typeName = "newquote" Then Return "salesnewquote"
            If typeName = "printpreviewquotes" Then Return "printpreviewquotes"
            If typeName = "creditnote" Then Return "creditnote"
            If typeName = "newsubscriptioninvoice" Then Return "newsubscriptioninvoice"
            If typeName = "editquote" Then Return "editquote"
            If typeName = "previewprintquoteeditedquote" Then Return "previewprintquoteeditedquote"
            If typeName = "quote" Then Return "quote"
            If typeName = "walkinneworder" Then Return "walkinorder"
            If typeName = "calendarview" Then Return "calendarview"
            If typeName = "addevent" Then Return "addevent"
            If typeName = "manageaccounts" Then Return "manageaccounts"
            If typeName = "accountsexpense" Then Return "accountsexpense"
            If typeName = "accountsincome" Then Return "accountsincome"
            If typeName = "viewtransactions" Then Return "viewtransactions"
            If typeName = "addnewtransaction" Then Return "addnewtransaction"
            If typeName = "balancesheets" Then Return "balancesheets"
            If typeName = "accountstatement" Then Return "accountstatement"
            If typeName = "addnewtransfer" Then Return "addnewtransfer"
            If typeName = "clienttransactions" Then Return "clienttransactions"
            If typeName = "navaddaccount" Then Return "navaddaccount"
            If typeName = "statementofaccountform" Then Return "statementofaccount"
            If typeName = "hrmuploadfiles" Then Return "hrmuploadfiles"
            If typeName = "newproject" Then Return "newproject"
            If typeName = "editsuppliers" Then Return "editsuppliers"
            If typeName = "addproject2" Then Return "addproject2"
            If typeName = "addproject3" Then Return "addproject3"
            If typeName = "manageproject" Then Return "manageproject"
            If typeName = "todolist" Then Return "todolist"
            If typeName = "newtask" Then Return "newtask"
            If typeName = "managetask" Then Return "managetask"
            If typeName = "edittask" Then Return "edittask"
            If typeName = "purchaseorderstatement" Then Return "purchaseorderstatement"
            If typeName = "printpreview" Then Return "printpreview"
            If typeName = "cashadvancenewrequest" Then Return "cashadvancenewrequest"
            If typeName = "managecashadvancerequests" Then Return "managecashadvancerequests"
            If typeName = "editcashadvancerequest" Then Return "editcashadvancerequest"
            If typeName = "previewprintcashadvancerequestform" Then Return "previewprintcashadvancerequestform"
            If typeName = "overtimerequestform" Then Return "overtimerequestform"
            If typeName = "manageovertimerequests" Then Return "manageovertimerequests"
            If typeName = "editovertime" Then Return "editovertime"
            If typeName = "previewprintovertimerequestform" Then Return "previewprintovertimerequestform"
            If typeName = "employeeleaverequestform" Then Return "employeeleaverequestform"
            If typeName = "manageemployeeleaverequests" Then Return "manageemployeeleaverequests"
            If typeName = "pulloutreceipt" Then Return "pulloutreceipt"
            If typeName = "pulloutpreview" Then Return "pulloutpreview"
            If typeName = "consumables" Then Return "consumables"

            Return typeName
        End Function
    End Class
End Namespace