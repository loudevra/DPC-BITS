

Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model


Namespace DPC.Data.Controllers
    Public Class SupplierController
        ' Fetch Supplier Data Using Connection Pooling
        Public Shared Function GetSuppliers() As ObservableCollection(Of SupplierDataModel)
            Dim supplierList As New ObservableCollection(Of SupplierDataModel)()
            Dim query As String = "
                    SELECT s.supplierID,
                           s.supplierName,
                           s.supplierCompany,
                           CONCAT(s.officeAddress, ', ', s.city, ', ', s.region, ', ', s.country, ', ', s.postalCode) AS address,
                           s.supplierEmail,
                           s.supplierPhone,
                           COALESCE(GROUP_CONCAT(b.brandName SEPARATOR ', '), '') AS Brands
                    FROM supplier s
                    LEFT JOIN supplierbrand sb ON sb.supplierID = s.supplierID
                    LEFT JOIN brand b ON b.brandID = sb.brandID
                    GROUP BY s.supplierID, s.supplierName, s.supplierCompany, s.officeAddress, s.city, s.region, s.country, s.postalCode, s.supplierEmail, s.supplierPhone;
                    "

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                ' Combine all brands into a single string
                                Dim brandString As String = If(reader.IsDBNull(reader.GetOrdinal("Brands")), "No Brands", reader.GetString("Brands"))

                                supplierList.Add(New SupplierDataModel With {
                            .SupplierID = reader.GetString("supplierID"),
                            .SupplierName = reader.GetString("supplierName"),
                            .SupplierCompany = reader.GetString("supplierCompany"),
                            .OfficeAddress = reader.GetString("address"),
                            .SupplierEmail = reader.GetString("supplierEmail"),
                            .SupplierPhone = reader.GetString("supplierPhone"),
                            .BrandNames = brandString
                        })
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error fetching suppliers: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Return supplierList
        End Function

        ' Function to generate SupplierID in format 20MMDDYYYYXXXX
        Private Shared Function GenerateSupplierID() As String
            Dim prefix As String = "20"
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy") ' MMDDYYYY format
            Dim counter As Integer = GetNextSupplierCounter(datePart)

            ' Format counter to be 4 digits (e.g., 0001, 0025, 0150)
            Dim counterPart As String = counter.ToString("D4")

            ' Concatenate to get full SupplierID
            Return prefix & datePart & counterPart
        End Function

        ' Function to get the next Supplier counter (last 4 digits) with reset condition
        Private Shared Function GetNextSupplierCounter(datePart As String) As Integer
            Dim query As String = "SELECT MAX(CAST(SUBSTRING(supplierID, 11, 4) AS UNSIGNED)) FROM supplier " &
                           "WHERE supplierID LIKE '20" & datePart & "%'"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Dim result As Object = cmd.ExecuteScalar()

                        ' If no previous records exist for today, start with 0001
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            Return Convert.ToInt32(result) + 1
                        Else
                            Return 1
                        End If
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error generating SupplierID: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return 1
                End Try
            End Using
        End Function

        ' Insert Supplier Data using Parameters
        Public Shared Sub InsertSupplier(supplierName As String, companyName As String, phone As String, email As String,
                                  address As String, city As String, region As String, country As String,
                                  postalCode As String, tinID As String, brandIDs As List(Of String))

            Dim supplierID As String = GenerateSupplierID()

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()

                    ' Insert into Supplier table
                    Dim supplierQuery As String = "INSERT INTO supplier (supplierID, supplierName, supplierCompany, officeAddress, city, region, country, postalCode, supplierEmail, supplierPhone, tinID) " &
                                           "VALUES (@SupplierID, @SupplierName, @CompanyName, @OfficeAddress, @City, @Region, @Country, @PostalCode, @Email, @PhoneNumber, @TINID);"

                    Using cmd As New MySqlCommand(supplierQuery, conn)
                        cmd.Parameters.AddWithValue("@SupplierID", supplierID)
                        cmd.Parameters.AddWithValue("@SupplierName", supplierName)
                        cmd.Parameters.AddWithValue("@CompanyName", companyName) ' Added companyName here
                        cmd.Parameters.AddWithValue("@OfficeAddress", address)
                        cmd.Parameters.AddWithValue("@City", city)
                        cmd.Parameters.AddWithValue("@Region", region)
                        cmd.Parameters.AddWithValue("@Country", country)
                        cmd.Parameters.AddWithValue("@PostalCode", postalCode)
                        cmd.Parameters.AddWithValue("@Email", email)
                        cmd.Parameters.AddWithValue("@PhoneNumber", phone)
                        cmd.Parameters.AddWithValue("@TINID", tinID)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Insert into SupplierBrand table for each brand
                    Dim brandQuery As String = "INSERT INTO supplierbrand (SupplierID, BrandID) VALUES (@SupplierID, @BrandID);"
                    For Each brandID As String In brandIDs
                        Using cmdBrand As New MySqlCommand(brandQuery, conn)
                            cmdBrand.Parameters.AddWithValue("@SupplierID", supplierID)
                            cmdBrand.Parameters.AddWithValue("@BrandID", brandID)
                            cmdBrand.ExecuteNonQuery()
                        End Using
                    Next

                    MessageBox.Show("Supplier and associated brands inserted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                Catch ex As Exception
                    MessageBox.Show("Error inserting supplier: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub
        ' Search suppliers based on input text
        ' Search suppliers based on input text and specific fields
        Public Shared Function SearchSuppliers(searchText As String) As ObservableCollection(Of SupplierDataModel)
            Dim suppliers As New ObservableCollection(Of SupplierDataModel)
            Dim query As String = "
        SELECT supplierID, supplierName, supplierCompany, supplierPhone, 
               supplierEmail, officeAddress, city, region, country, 
               postalCode, tinID
        FROM supplier
        WHERE supplierName LIKE @searchText 
           OR supplierID LIKE @searchText 
           OR supplierEmail LIKE @searchText
        ORDER BY supplierName ASC
        LIMIT 10;
        "
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@searchText", "%" & searchText & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim supplier As New SupplierDataModel With {
                            .SupplierID = reader("supplierID").ToString(),
                            .SupplierName = reader("supplierName").ToString(),
                            .SupplierCompany = reader("supplierCompany").ToString(),
                            .SupplierPhone = reader("supplierPhone").ToString(),
                            .SupplierEmail = reader("supplierEmail").ToString(),
                            .OfficeAddress = reader("officeAddress").ToString(),
                            .City = reader("city").ToString(),
                            .Region = reader("region").ToString(),
                            .Country = reader("country").ToString(),
                            .PostalCode = reader("postalCode").ToString(),
                            .TinID = reader("tinID").ToString()
                        }
                                suppliers.Add(supplier)
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error searching suppliers: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
            Return suppliers
        End Function

        ' Get supplier by ID
        Public Shared Function GetSupplierById(supplierId As String) As SupplierDataModel
            Dim supplier As New SupplierDataModel()
            Dim query As String = "
                SELECT supplierID, supplierName, supplierCompany, supplierPhone, 
                       supplierEmail, officeAddress, city, region, country, 
                       postalCode, tinID
                FROM supplier
                WHERE supplierID = @supplierId
                "
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@supplierId", supplierId)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            If reader.Read() Then
                                supplier.SupplierID = reader("supplierID").ToString()
                                supplier.SupplierName = reader("supplierName").ToString()
                                supplier.SupplierCompany = reader("supplierCompany").ToString()
                                supplier.SupplierPhone = reader("supplierPhone").ToString()
                                supplier.SupplierEmail = reader("supplierEmail").ToString()
                                supplier.OfficeAddress = reader("officeAddress").ToString()
                                supplier.City = reader("city").ToString()
                                supplier.Region = reader("region").ToString()
                                supplier.Country = reader("country").ToString()
                                supplier.PostalCode = reader("postalCode").ToString()
                                supplier.TinID = reader("tinID").ToString()
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error retrieving supplier: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
            Return supplier
        End Function


        Public Shared Function SearchProducts(searchText As String) As ObservableCollection(Of ProductDataModel)
            Dim products As New ObservableCollection(Of ProductDataModel)
            Dim query As String = "
            SELECT *
            FROM product
            WHERE productName LIKE @searchText 
               OR productID LIKE @searchText 
            ORDER BY productName ASC
            LIMIT 10;
            "
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@searchText", "%" & searchText & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim product As New ProductDataModel With {
                            .ProductID = reader("productID").ToString(),
                            .ProductName = reader("productName").ToString()
                        }
                                products.Add(product)
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error searching suppliers: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
            Return products
        End Function

        ' Update Supplier Data
        Public Shared Sub UpdateSupplier(supplierID As String, supplierName As String, companyName As String,
                                        phone As String, email As String, address As String, city As String,
                                        region As String, country As String, postalCode As String, tinID As String,
                                        brandIDs As List(Of String))
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()

                    ' Update Supplier table
                    Dim supplierQuery As String = "UPDATE supplier SET 
                                          supplierName = @SupplierName, 
                                          supplierCompany = @CompanyName, 
                                          officeAddress = @OfficeAddress, 
                                          city = @City, 
                                          region = @Region, 
                                          country = @Country, 
                                          postalCode = @PostalCode, 
                                          supplierEmail = @Email, 
                                          supplierPhone = @PhoneNumber, 
                                          tinID = @TINID 
                                          WHERE supplierID = @SupplierID;"

                    Using cmd As New MySqlCommand(supplierQuery, conn)
                        cmd.Parameters.AddWithValue("@SupplierID", supplierID)
                        cmd.Parameters.AddWithValue("@SupplierName", supplierName)
                        cmd.Parameters.AddWithValue("@CompanyName", companyName)
                        cmd.Parameters.AddWithValue("@OfficeAddress", address)
                        cmd.Parameters.AddWithValue("@City", city)
                        cmd.Parameters.AddWithValue("@Region", region)
                        cmd.Parameters.AddWithValue("@Country", country)
                        cmd.Parameters.AddWithValue("@PostalCode", postalCode)
                        cmd.Parameters.AddWithValue("@Email", email)
                        cmd.Parameters.AddWithValue("@PhoneNumber", phone)
                        cmd.Parameters.AddWithValue("@TINID", tinID)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Delete existing brand associations
                    Dim deleteBrandQuery As String = "DELETE FROM supplierbrand WHERE SupplierID = @SupplierID;"
                    Using cmdDelete As New MySqlCommand(deleteBrandQuery, conn)
                        cmdDelete.Parameters.AddWithValue("@SupplierID", supplierID)
                        cmdDelete.ExecuteNonQuery()
                    End Using

                    ' Insert new brand associations
                    Dim brandQuery As String = "INSERT INTO supplierbrand (SupplierID, BrandID) VALUES (@SupplierID, @BrandID);"
                    For Each brandID As String In brandIDs
                        Using cmdBrand As New MySqlCommand(brandQuery, conn)
                            cmdBrand.Parameters.AddWithValue("@SupplierID", supplierID)
                            cmdBrand.Parameters.AddWithValue("@BrandID", brandID)
                            cmdBrand.ExecuteNonQuery()
                        End Using
                    Next

                    MessageBox.Show("Supplier updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

                Catch ex As Exception
                    MessageBox.Show("Error updating supplier: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
        End Sub

        ' Get Brands for a specific Supplier
        Public Shared Function GetBrandsForSupplier(supplierID As String) As List(Of Brand)
            Dim brands As New List(Of Brand)()
            Dim query As String = "SELECT b.brandID, b.brandName 
                          FROM brand b
                          INNER JOIN supplierbrand sb ON b.brandID = sb.brandID
                          WHERE sb.supplierID = @SupplierID;"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@SupplierID", supplierID)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                brands.Add(New Brand With {
                                    .ID = reader.GetInt32("brandID"),
                                    .Name = reader.GetString("brandName")
                                })
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error loading supplier brands: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Return brands
        End Function

    End Class
End Namespace

