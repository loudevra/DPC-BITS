Imports System.Collections.ObjectModel
Imports DocumentFormat.OpenXml.Office2016.Drawing
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.CRM
Imports MailKit.Search
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Namespace DPC.Data.Controllers
    Public Class ClientController

        Public Shared Function CreateClientCorporational(client As ClientCorporational) As Boolean
            ' Generate the custom Client ID
            client.ClientID = GenerateClientIDCorporational()

            Dim query As String = "INSERT INTO clientcorporational (ClientID, ClientGroupID, Company, Representative, Phone, Landline, Email, BillingAddress, ShippingAddress, CustomerGroup, Language, TinID, ClientType, CreatedAt, UpdatedAt) " &
                                  "VALUES (@ClientID, @ClientGroupID, @Company, @Representative, @Phone, @Landline, @Email, @BillingAddress, " &
                                  "@ShippingAddress, @CustomerGroup, @ClientLanguage, @TinID, @ClientType, @CreatedAt, @UpdatedAt)"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ClientID", client.ClientID)
                        cmd.Parameters.AddWithValue("@ClientGroupID", client.ClientGroupID)
                        cmd.Parameters.AddWithValue("@Company", client.Company)
                        cmd.Parameters.AddWithValue("@Representative", client.Representative)
                        cmd.Parameters.AddWithValue("@Phone", client.Phone)
                        cmd.Parameters.AddWithValue("@Landline", client.Landline)
                        cmd.Parameters.AddWithValue("@Email", client.Email)
                        cmd.Parameters.AddWithValue("@BillingAddress", client.BillingAddress)
                        cmd.Parameters.AddWithValue("@ShippingAddress", client.ShippingAddress)
                        cmd.Parameters.AddWithValue("@CustomerGroup", client.CustomerGroup)
                        cmd.Parameters.AddWithValue("@ClientLanguage", client.ClientLanguage)
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now)
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now)
                        cmd.Parameters.AddWithValue("@TinID", client.TinID)
                        cmd.Parameters.AddWithValue("@ClientType", client.ClientType)

                        Dim result As Integer = cmd.ExecuteNonQuery()
                        Return result > 0
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error creating client: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                End Try
            End Using
        End Function

        ' Function to generate ClientID in format 40MMDDYYYYXXXX
        Private Shared Function GenerateClientIDCorporational() As String
            Dim prefix As String = "50"
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy")
            Dim counter As Integer = GetNextClientCounterCorporational(datePart)

            ' Format counter to be 4 digits (e.g., 0001, 0025)
            Dim counterPart As String = counter.ToString("D4")

            Return prefix & datePart & counterPart
        End Function

        ' Function to get the next Client counter (last 4 digits)
        Private Shared Function GetNextClientCounterCorporational(datePart As String) As Integer
            Dim query As String = "SELECT MAX(CAST(SUBSTRING(ClientID, 11, 4) AS UNSIGNED)) FROM clientcorporational " &
                                  "WHERE ClientID LIKE '50" & datePart & "%'"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Dim result As Object = cmd.ExecuteScalar()
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            Return Convert.ToInt32(result) + 1
                        Else
                            Return 1
                        End If
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error generating Client ID: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return 1
                End Try
            End Using
        End Function



        ' Function to create a new client
        Public Shared Function CreateClient(client As Client) As Boolean
            ' Generate the custom Client ID
            client.ClientID = GenerateClientID()

            Dim query As String = "INSERT INTO client (ClientID, ClientGroupID, Name, Phone, Email, BillingAddress, ShippingAddress, " &
                                  "CustomerGroup, Language, CreatedAt, UpdatedAt, ClientType) " &
                                  "VALUES (@ClientID, @ClientGroupID, @Name, @Phone, @Email, @BillingAddress, @ShippingAddress, " &
                                  "@CustomerGroup, @Language, @CreatedAt, @UpdatedAt, @ClientType)"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ClientID", client.ClientID)
                        cmd.Parameters.AddWithValue("@ClientGroupID", client.ClientGroupID)
                        cmd.Parameters.AddWithValue("@Name", client.Name)
                        cmd.Parameters.AddWithValue("@Phone", client.Phone)
                        cmd.Parameters.AddWithValue("@Email", client.Email)
                        cmd.Parameters.AddWithValue("@BillingAddress", client.BillingAddress)
                        cmd.Parameters.AddWithValue("@ShippingAddress", client.ShippingAddress)
                        cmd.Parameters.AddWithValue("@CustomerGroup", client.CustomerGroup)
                        cmd.Parameters.AddWithValue("@Language", client.ClientLanguage)
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now)
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now)
                        cmd.Parameters.AddWithValue("@ClientType", client.ClientType)

                        Dim result As Integer = cmd.ExecuteNonQuery()
                        Return result > 0
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error creating client: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                End Try
            End Using
        End Function

        ' Function to generate ClientID in format 40MMDDYYYYXXXX
        Private Shared Function GenerateClientID() As String
            Dim prefix As String = "40"
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy")
            Dim counter As Integer = GetNextClientCounter(datePart)

            ' Format counter to be 4 digits (e.g., 0001, 0025)
            Dim counterPart As String = counter.ToString("D4")

            Return prefix & datePart & counterPart
        End Function

        ' Function to get the next Client counter (last 4 digits)
        Private Shared Function GetNextClientCounter(datePart As String) As Integer
            Dim query As String = "SELECT MAX(CAST(SUBSTRING(ClientID, 11, 4) AS UNSIGNED)) FROM client " &
                                  "WHERE ClientID LIKE '40" & datePart & "%'"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Dim result As Object = cmd.ExecuteScalar()
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            Return Convert.ToInt32(result) + 1
                        Else
                            Return 1
                        End If
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error generating Client ID: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return 1
                End Try
            End Using
        End Function

        ' Function to get all clients
        Public Shared Function GetAllClients() As ObservableCollection(Of Client)
            Dim clients As New ObservableCollection(Of Client)()
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT 
    ClientID,
    BillingAddress,
    Email,
    Phone,
    Name,
    NULL AS Company,
    'client' AS Source
FROM client

UNION

SELECT 
    ClientID,
    BillingAddress,
    Email,
    Phone,
    NULL AS Name,
    Company,
    'clientcorporational' AS Source
FROM clientcorporational;"

                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim nameOrCompany As String = ""

                                If Not IsDBNull(reader("Name")) AndAlso Not String.IsNullOrWhiteSpace(reader("Name").ToString()) Then
                                    nameOrCompany = reader("Name").ToString()
                                ElseIf Not IsDBNull(reader("Company")) AndAlso Not String.IsNullOrWhiteSpace(reader("Company").ToString()) Then
                                    nameOrCompany = reader("Company").ToString()
                                End If

                                Dim source As String = reader("Source").ToString()
                                Dim client As New Client With {
                            .ClientID = reader("ClientID"),
                            .Name = nameOrCompany,
                            .Phone = reader("Phone").ToString(),
                            .Email = reader("Email").ToString(),
                            .BillingAddress = reader("BillingAddress").ToString(),
                            .ClientType = GetClientType(source)
                        }
                                clients.Add(client)
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error fetching clients: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
            Return clients
        End Function

        ' Update SearchClients() - Add ClientType to SQL query
        Public Shared Function SearchClients(_searchText As String) As ObservableCollection(Of Client)
            Dim clients As New ObservableCollection(Of Client)()
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT 
    ClientID,
    BillingAddress,
    Email,
    Phone,
    Name,
    NULL AS Company,
    'client' AS Source
FROM client
WHERE Name LIKE @searchText 
   OR ClientID LIKE @searchText 
   OR Email LIKE @searchText

UNION

SELECT 
    ClientID,
    BillingAddress,
    Email,
    Phone,
    NULL AS Name,
    Company,
    'clientcorporational' AS Source
FROM clientcorporational
WHERE Company LIKE @searchText 
   OR ClientID LIKE @searchText 
   OR Email LIKE @searchText

ORDER BY Name ASC
LIMIT 10;"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@searchText", "%" & _searchText & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim nameOrCompany As String = ""

                                If Not IsDBNull(reader("Name")) AndAlso Not String.IsNullOrWhiteSpace(reader("Name").ToString()) Then
                                    nameOrCompany = reader("Name").ToString()
                                ElseIf Not IsDBNull(reader("Company")) AndAlso Not String.IsNullOrWhiteSpace(reader("Company").ToString()) Then
                                    nameOrCompany = reader("Company").ToString()
                                End If

                                Dim source As String = reader("Source").ToString()
                                Dim client As New Client With {
                            .ClientID = reader("ClientID"),
                            .Name = nameOrCompany,
                            .Phone = reader("Phone").ToString(),
                            .Email = reader("Email").ToString(),
                            .BillingAddress = reader("BillingAddress").ToString(),
                            .ClientType = GetClientType(source)
                        }
                                clients.Add(client)
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error fetching clients: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
            Return clients
        End Function


        ' Function to get client by ID
        Public Shared Function GetClientByID(clientID As String) As Client
            Dim client As Client = Nothing
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Dim query As String = "SELECT * FROM client WHERE ClientID = @ClientID"
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ClientID", clientID)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            If reader.Read() Then
                                client = New Client With {
                                    .ClientID = reader("ClientID").ToString(), ' Change from Convert.ToInt32 to ToString()
                                    .ClientGroupID = Convert.ToInt32(reader("ClientGroupID")),
                                    .Name = reader("Name").ToString(),
                                    .Phone = reader("Phone").ToString(),
                                    .Email = reader("Email").ToString(),
                                    .BillingAddress = reader("BillingAddress").ToString(),
                                    .ShippingAddress = reader("ShippingAddress").ToString(),
                                    .CustomerGroup = reader("CustomerGroup").ToString(),
                                    .ClientLanguage = reader("Language").ToString(),
                                    .CreatedAt = Convert.ToDateTime(reader("CreatedAt")),
                                    .UpdatedAt = Convert.ToDateTime(reader("UpdatedAt"))
                                }
                            End If
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error fetching client: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
            Return client
        End Function

        Public Shared Function SearchClient(_searchText As String) As ObservableCollection(Of Client)
            Dim clients As New ObservableCollection(Of Client)()
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Dim SearchClientQuery As String = "SELECT 
    ClientID,
ClientGroupID,
    BillingAddress,
    Email,
    Phone,
    Name,
    NULL AS Company,
CustomerGroup,
    Language,
    ShippingAddress,
NULL AS Representative,
'client' AS Source
FROM client 
WHERE Name LIKE @searchText 
   OR ClientID LIKE @searchText 
   OR Email LIKE @searchText

UNION

SELECT 
    ClientID,
ClientGroupID,
    BillingAddress,
    Email,
    Phone,
    NULL AS Name,
    Company,
CustomerGroup,
    Language,
    ShippingAddress,
    Representative,
'clientcorporational' AS Source
FROM clientcorporational AS Source
WHERE Company LIKE @searchText 
   OR ClientID LIKE @searchText 
   OR Email LIKE @searchText

ORDER BY Name ASC
LIMIT 10;"

                    Using cmd As New MySqlCommand(SearchClientQuery, conn)
                        cmd.Parameters.AddWithValue("@searchText", "%" & _searchText & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim nameOrCompany As String = ""

                                If Not IsDBNull(reader("Name")) AndAlso Not String.IsNullOrWhiteSpace(reader("Name").ToString()) Then
                                    nameOrCompany = reader("Name").ToString()
                                ElseIf Not IsDBNull(reader("Company")) AndAlso Not String.IsNullOrWhiteSpace(reader("Company").ToString()) Then
                                    nameOrCompany = reader("Company").ToString()
                                End If

                                Dim client As New Client With {
                            .ClientID = reader("ClientID"),
                            .Name = nameOrCompany,
                            .Company = If(IsDBNull(reader("Company")), String.Empty, reader("Company").ToString()),
                            .Phone = If(IsDBNull(reader("Phone")), String.Empty, reader("Phone").ToString()),
                            .Email = If(IsDBNull(reader("Email")), String.Empty, reader("Email").ToString()),
                            .CustomerGroup = If(IsDBNull(reader("CustomerGroup")), String.Empty, reader("CustomerGroup").ToString()),
                            .ClientLanguage = If(IsDBNull(reader("Language")), String.Empty, reader("Language").ToString()),
                            .BillingAddress = If(IsDBNull(reader("BillingAddress")), String.Empty, reader("BillingAddress").ToString()),
                            .ShippingAddress = If(IsDBNull(reader("ShippingAddress")), String.Empty, reader("ShippingAddress").ToString()),
                            .Representative = If(Not IsDBNull(reader("Representative")) AndAlso Not String.IsNullOrWhiteSpace(reader("Representative").ToString()),
                                             reader("Representative").ToString(),
                                             reader("Name").ToString()),
                            .Source = reader("Source").ToString()
                            }

                                clients.Add(client)
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show($"Error searching ClientController: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
            Return clients
        End Function

        Private Shared Function GetClientType(source As String) As String
            Select Case source.ToLower()
                Case "client"
                    Return "Residential"
                Case "clientcorporational"
                    Return "Corporate"
                Case Else
                    Return "Unknown"
            End Select
        End Function

        ' Add this method to your ClientController class

        ' Replace the UpdateClient function in ClientController with this:

        Public Shared Function UpdateClient(client As Client) As Boolean
            Try
                If Not ClientIDExists(client.ClientID.ToString()) Then
                    MessageBox.Show("Client ID '" & client.ClientID & "' does not exist. Cannot update.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return False
                End If

                ' Determine if it's a residential or corporate client based on the ClientID prefix
                Dim clientIDStr As String = client.ClientID.ToString()
                Dim prefix As String = clientIDStr.Substring(0, 2)
                Dim query As String = ""

                If prefix = "40" Then
                    ' Residential client
                    query = "UPDATE client SET ClientGroupID = @ClientGroupID, Name = @Name, Phone = @Phone, Email = @Email, " &
                    "BillingAddress = @BillingAddress, ShippingAddress = @ShippingAddress, CustomerGroup = @CustomerGroup, " &
                    "Language = @Language, UpdatedAt = @UpdatedAt WHERE ClientID = @ClientID"
                ElseIf prefix = "50" Then
                    ' Corporate client
                    query = "UPDATE clientcorporational SET ClientGroupID = @ClientGroupID, Company = @Name, Phone = @Phone, Email = @Email, " &
                    "BillingAddress = @BillingAddress, ShippingAddress = @ShippingAddress, CustomerGroup = @CustomerGroup, " &
                    "Language = @Language, UpdatedAt = @UpdatedAt WHERE ClientID = @ClientID"
                Else
                    MessageBox.Show("Invalid Client ID format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                End If

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    Try
                        conn.Open()
                        Using transaction As MySqlTransaction = conn.BeginTransaction()
                            Try
                                Using updateCmd As New MySqlCommand(query, conn, transaction)
                                    ' Add all parameters
                                    updateCmd.Parameters.AddWithValue("@ClientID", clientIDStr)
                                    updateCmd.Parameters.AddWithValue("@ClientGroupID", client.ClientGroupID)
                                    updateCmd.Parameters.AddWithValue("@Name", If(String.IsNullOrEmpty(client.Name), "", client.Name))
                                    updateCmd.Parameters.AddWithValue("@Phone", If(String.IsNullOrEmpty(client.Phone), "", client.Phone))
                                    updateCmd.Parameters.AddWithValue("@Email", If(String.IsNullOrEmpty(client.Email), "", client.Email))
                                    updateCmd.Parameters.AddWithValue("@BillingAddress", If(String.IsNullOrEmpty(client.BillingAddress), "", client.BillingAddress))
                                    updateCmd.Parameters.AddWithValue("@ShippingAddress", If(String.IsNullOrEmpty(client.ShippingAddress), "", client.ShippingAddress))
                                    updateCmd.Parameters.AddWithValue("@CustomerGroup", If(String.IsNullOrEmpty(client.CustomerGroup), "", client.CustomerGroup))
                                    updateCmd.Parameters.AddWithValue("@Language", If(String.IsNullOrEmpty(client.ClientLanguage), "", client.ClientLanguage))
                                    updateCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now)

                                    Dim rowsAffected As Integer = updateCmd.ExecuteNonQuery()
                                    If rowsAffected > 0 Then
                                        transaction.Commit()
                                        MessageBox.Show("Client has been successfully updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                                        Return True
                                    Else
                                        transaction.Rollback()
                                        MessageBox.Show("No rows were updated. Client ID may have changed.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Warning)
                                        Return False
                                    End If
                                End Using
                            Catch ex As Exception
                                transaction.Rollback()
                                MessageBox.Show("Error updating client: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                                Return False
                            End Try
                        End Using
                    Catch ex As Exception
                        MessageBox.Show("Error: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                        Return False
                    End Try
                End Using
            Catch ex As Exception
                MessageBox.Show("Error: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function

        Public Shared Function ClientIDExists(ClientID As String) As Boolean
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim clientIDStr As String = ClientID.ToString()
                    Dim query As String = "SELECT COUNT(*) FROM client WHERE ClientID = @ClientID " &
                                  "UNION ALL " &
                                  "SELECT COUNT(*) FROM clientcorporational WHERE ClientID = @ClientID"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ClientID", clientIDStr)
                        Dim result As Object = cmd.ExecuteScalar()

                        If result IsNot Nothing AndAlso result IsNot DBNull.Value Then
                            Dim count As Integer = Convert.ToInt32(result)
                            Return count > 0
                        End If

                        Return False
                    End Using
                End Using
            Catch ex As Exception
                Console.WriteLine("Error in ClientIDExists: " & ex.Message)
                Return False
            End Try
        End Function

        Public Shared Function DeleteClient(clientID As String) As Boolean
            Try
                ' Determine if it's a residential or corporate client based on the prefix
                Dim prefix As String = clientID.Substring(0, 2)
                Dim tableName As String = ""

                If prefix = "40" Then
                    tableName = "client"
                ElseIf prefix = "50" Then
                    tableName = "clientcorporational"
                Else
                    MessageBox.Show("Invalid Client ID format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                End If

                Dim query As String = $"DELETE FROM {tableName} WHERE ClientID = @ClientID"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    Try
                        conn.Open()
                        Using cmd As New MySqlCommand(query, conn)
                            cmd.Parameters.AddWithValue("@ClientID", clientID)

                            Dim result As Integer = cmd.ExecuteNonQuery()
                            Return result > 0
                        End Using
                    Catch ex As Exception
                        MessageBox.Show("Error deleting client: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                        Return False
                    End Try
                End Using
            Catch ex As Exception
                MessageBox.Show("Error: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function
    End Class
End Namespace
