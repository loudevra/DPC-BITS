Imports System.Collections.ObjectModel
Imports MySql.Data.MySqlClient

Namespace DPC.Data.Controllers
    Public Class GlobalSearchController
        ''' <summary>
        ''' Represents a unified search result that can be from any module
        ''' </summary>
        Public Class SearchResultItem
            Public Property Title As String
            Public Property Subtitle As String
            Public Property Category As String ' e.g., "Client", "Product", "Supplier", "View"
            Public Property Icon As String ' MaterialDesign icon name
            Public Property NavigationTarget As String ' View name or action
            Public Property ID As String
            Public Property AdditionalInfo As String
        End Class

        ''' <summary>
        ''' Performs a global search across all modules
        ''' </summary>
        Public Shared Function GlobalSearch(searchText As String) As ObservableCollection(Of SearchResultItem)
            Dim results As New ObservableCollection(Of SearchResultItem)

            If String.IsNullOrWhiteSpace(searchText) Then
                Return results
            End If

            ' Search in parallel (add all results)
            SearchClients(searchText, results)
            SearchProducts(searchText, results)
            SearchSuppliers(searchText, results)
            SearchViews(searchText, results)
            SearchOrders(searchText, results)
            SearchInvoices(searchText, results)

            Return results
        End Function

        ''' <summary>
        ''' Search for clients (residential and corporate)
        ''' </summary>
        Private Shared Sub SearchClients(searchText As String, results As ObservableCollection(Of SearchResultItem))
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "
                        SELECT ClientID, Name, Email, Phone, 'Residential Client' AS Type
                        FROM client 
                        WHERE Name LIKE @search OR ClientID LIKE @search OR Email LIKE @search OR Phone LIKE @search
                        UNION
                        SELECT ClientID, Company AS Name, Email, Phone, 'Corporate Client' AS Type
                        FROM clientcorporational
                        WHERE Company LIKE @search OR ClientID LIKE @search OR Email LIKE @search OR Phone LIKE @search
                        LIMIT 10"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@search", "%" & searchText & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                results.Add(New SearchResultItem With {
                                    .Title = reader("Name").ToString(),
                                    .Subtitle = $"ID: {reader("ClientID")} | {reader("Email")}",
                                    .Category = reader("Type").ToString(),
                                    .Icon = "AccountCircle",
                                    .NavigationTarget = "manageclients",
                                    .ID = reader("ClientID").ToString(),
                                    .AdditionalInfo = reader("Phone").ToString()
                                })
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                ' Silent fail for individual search
            End Try
        End Sub

        ''' <summary>
        ''' Search for products
        ''' </summary>
        Private Shared Sub SearchProducts(searchText As String, results As ObservableCollection(Of SearchResultItem))
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "
                        SELECT ProductID, ProductName, Category, Price, StockQuantity
                        FROM product 
                        WHERE ProductName LIKE @search OR ProductID LIKE @search OR Category LIKE @search
                        LIMIT 10"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@search", "%" & searchText & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim price = If(IsDBNull(reader("Price")), 0, Convert.ToDecimal(reader("Price")))
                                Dim stock = If(IsDBNull(reader("StockQuantity")), 0, Convert.ToInt32(reader("StockQuantity")))

                                results.Add(New SearchResultItem With {
                                    .Title = reader("ProductName").ToString(),
                                    .Subtitle = $"ID: {reader("ProductID")} | Stock: {stock} | Price: ₱{price:N2}",
                                    .Category = "Product",
                                    .Icon = "Package",
                                    .NavigationTarget = "manageproducts",
                                    .ID = reader("ProductID").ToString(),
                                    .AdditionalInfo = reader("Category").ToString()
                                })
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                ' Silent fail
            End Try
        End Sub

        ''' <summary>
        ''' Search for suppliers
        ''' </summary>
        Private Shared Sub SearchSuppliers(searchText As String, results As ObservableCollection(Of SearchResultItem))
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "
                        SELECT SupplierID, SupplierName, ContactNumber, Email
                        FROM supplier 
                        WHERE SupplierName LIKE @search OR SupplierID LIKE @search OR Email LIKE @search
                        LIMIT 10"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@search", "%" & searchText & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                results.Add(New SearchResultItem With {
                                    .Title = reader("SupplierName").ToString(),
                                    .Subtitle = $"ID: {reader("SupplierID")} | {If(IsDBNull(reader("Email")), "", reader("Email").ToString())}",
                                    .Category = "Supplier",
                                    .Icon = "TruckDelivery",
                                    .NavigationTarget = "managesuppliers",
                                    .ID = reader("SupplierID").ToString(),
                                    .AdditionalInfo = If(IsDBNull(reader("ContactNumber")), "", reader("ContactNumber").ToString())
                                })
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                ' Silent fail
            End Try
        End Sub

        ''' <summary>
        ''' Search for purchase orders
        ''' </summary>
        Private Shared Sub SearchOrders(searchText As String, results As ObservableCollection(Of SearchResultItem))
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "
                        SELECT OrderID, SupplierName, OrderDate, TotalAmount, Status
                        FROM purchaseorder 
                        WHERE OrderID LIKE @search OR SupplierName LIKE @search
                        LIMIT 10"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@search", "%" & searchText & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim amount = If(IsDBNull(reader("TotalAmount")), 0, Convert.ToDecimal(reader("TotalAmount")))
                                Dim status = If(IsDBNull(reader("Status")), "N/A", reader("Status").ToString())

                                results.Add(New SearchResultItem With {
                                    .Title = $"Order #{reader("OrderID")}",
                                    .Subtitle = $"Supplier: {reader("SupplierName")} | Amount: ₱{amount:N2}",
                                    .Category = "Purchase Order",
                                    .Icon = "FileDocument",
                                    .NavigationTarget = "manageorder",
                                    .ID = reader("OrderID").ToString(),
                                    .AdditionalInfo = $"Status: {status}"
                                })
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                ' Silent fail
            End Try
        End Sub

        ''' <summary>
        ''' Search for invoices
        ''' </summary>
        Private Shared Sub SearchInvoices(searchText As String, results As ObservableCollection(Of SearchResultItem))
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "
                        SELECT InvoiceID, ClientName, InvoiceDate, TotalAmount, Status
                        FROM invoice 
                        WHERE InvoiceID LIKE @search OR ClientName LIKE @search
                        LIMIT 10"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@search", "%" & searchText & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim amount = If(IsDBNull(reader("TotalAmount")), 0, Convert.ToDecimal(reader("TotalAmount")))

                                results.Add(New SearchResultItem With {
                                    .Title = $"Invoice #{reader("InvoiceID")}",
                                    .Subtitle = $"Client: {reader("ClientName")} | Amount: ₱{amount:N2}",
                                    .Category = "Invoice",
                                    .Icon = "Receipt",
                                    .NavigationTarget = "salesinvoices",
                                    .ID = reader("InvoiceID").ToString(),
                                    .AdditionalInfo = reader("Status").ToString()
                                })
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                ' Silent fail
            End Try
        End Sub

        ''' <summary>
        ''' Search for views/pages in the system
        ''' </summary>
        Private Shared Sub SearchViews(searchText As String, results As ObservableCollection(Of SearchResultItem))
            Dim views As New Dictionary(Of String, ViewInfo) From {
                {"dashboard", New ViewInfo("Dashboard", "Main dashboard view", "ViewDashboard")},
                {"manageclients", New ViewInfo("Manage Clients", "View and manage all clients", "AccountMultiple")},
                {"manageproducts", New ViewInfo("Manage Products", "View and manage inventory", "Package")},
                {"managesuppliers", New ViewInfo("Manage Suppliers", "View and manage suppliers", "TruckDelivery")},
                {"manageorder", New ViewInfo("Purchase Orders", "Manage purchase orders", "FileDocument")},
                {"salesinvoices", New ViewInfo("Sales Invoices", "View sales invoices", "Receipt")},
                {"salesquote", New ViewInfo("Sales Quotes", "Manage sales quotes", "FileDocumentEdit")},
                {"manageaccounts", New ViewInfo("Manage Accounts", "Financial accounts", "BankOutline")},
                {"warehouses", New ViewInfo("Warehouses", "Warehouse management", "Warehouse")},
                {"viewemployee", New ViewInfo("Employees", "Employee management", "AccountTie")},
                {"promocodes", New ViewInfo("Promo Codes", "Manage promotional codes", "TagMultiple")},
                {"newproducts", New ViewInfo("Add New Product", "Create new product", "PackagePlus")},
                {"neworder", New ViewInfo("New Purchase Order", "Create purchase order", "FilePlus")},
                {"newresidentialclient", New ViewInfo("Add Client", "Add new residential client", "AccountPlus")},
                {"productcategories", New ViewInfo("Product Categories", "Manage categories", "Shape")},
                {"subscriptions", New ViewInfo("Subscriptions", "Manage subscriptions", "CalendarRepeat")}
            }

            For Each kvp In views
                If kvp.Value.Name.ToLower().Contains(searchText.ToLower()) OrElse
                   kvp.Value.Description.ToLower().Contains(searchText.ToLower()) Then
                    results.Add(New SearchResultItem With {
                        .Title = kvp.Value.Name,
                        .Subtitle = kvp.Value.Description,
                        .Category = "Navigation",
                        .Icon = kvp.Value.Icon,
                        .NavigationTarget = kvp.Key,
                        .ID = kvp.Key,
                        .AdditionalInfo = "Click to navigate"
                    })
                End If
            Next
        End Sub

        Private Class ViewInfo
            Public Property Name As String
            Public Property Description As String
            Public Property Icon As String

            Public Sub New(_name As String, _description As String, _icon As String)
                Name = _name
                Description = _description
                Icon = _icon
            End Sub
        End Class
    End Class
End Namespace