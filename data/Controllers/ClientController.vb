Imports System.Collections.ObjectModel
Imports DocumentFormat.OpenXml.Office2016.Drawing
Imports DPC.DPC.Data.Model
Imports DPC.DPC.Data.Models
Imports MailKit.Search
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Namespace DPC.Data.Controllers
    Public Class ClientController

        Public Shared Function CreateClientCorporational(client As ClientCorporational) As Boolean
            ' Generate the custom Client ID
            client.ClientID = GenerateClientIDCorporational()

            Dim query As String = "INSERT INTO clientcorporational (ClientID, ClientGroupID, Company, Representative, Phone, Landline, Email, BillingAddress, ShippingAddress, CustomerGroup, ClientLanguage, TinID, ClientType, CreatedAt, UpdatedAt) " &
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
    NULL AS Company
FROM client

UNION

SELECT 
    ClientID,
    BillingAddress,
    Email,
    Phone,
    NULL AS Name,
    Company
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

                                Dim client As New Client With {
                            .ClientID = reader("ClientID"),
                            .Name = nameOrCompany,
                            .Phone = reader("Phone").ToString(),
                            .Email = reader("Email").ToString(),
                            .BillingAddress = reader("BillingAddress").ToString()
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
    NULL AS Company
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
    Company
FROM clientcorporational
WHERE Company LIKE @searchText 
   OR ClientID LIKE @searchText 
   OR Email LIKE @searchText

ORDER BY Name ASC
LIMIT 10;
"

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

                                Dim client As New Client With {
                            .ClientID = reader("ClientID"),
                            .Name = nameOrCompany,
                            .Phone = reader("Phone").ToString(),
                            .Email = reader("Email").ToString(),
                            .BillingAddress = reader("BillingAddress").ToString()
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
                                    .ClientID = Convert.ToInt32(reader("ClientID")),
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

        Public Shared Function SearchClient(searchClientName As String) As ObservableCollection(Of UpdatedClient)
            Dim clients As New ObservableCollection(Of UpdatedClient)
            Dim SearchClientQuery As String = "SELECT * FROM client
                                        WHERE Name LIKE @searchText 
                                           OR ClientID LIKE @searchText 
                                           OR Email LIKE @searchText
                                        ORDER BY Name ASC
                                        LIMIT 10"
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()

                    Using cmd As New MySqlCommand(SearchClientQuery, conn)
                        cmd.Parameters.AddWithValue("@searchText", "%" & searchClientName & "%")
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim client As New UpdatedClient With {
                            .ClientID = Convert.ToInt64(reader("ClientID")),
                            .ClientGroupID = If(IsDBNull(reader("ClientGroupID")), 0, Convert.ToInt32(reader("ClientGroupID"))),
                            .Name = reader("Name").ToString(),
                            .Company = If(IsDBNull(reader("Company")), String.Empty, reader("Company").ToString()),
                            .Phone = If(IsDBNull(reader("Phone")), String.Empty, reader("Phone").ToString()),
                            .Email = If(IsDBNull(reader("Email")), String.Empty, reader("Email").ToString()),
                            .CustomerGroup = If(IsDBNull(reader("CustomerGroup")), String.Empty, reader("CustomerGroup").ToString()),
                            .Language = If(IsDBNull(reader("Language")), String.Empty, reader("Language").ToString()),
                            .BillingAddress = Nothing,
                            .ShippingAddress = Nothing
                        }

                                ' Read and deserialize BillingAddress JSON
                                If Not IsDBNull(reader("BillingAddress")) Then
                                    Dim billingJson As String = reader("BillingAddress").ToString()
                                    If Not String.IsNullOrWhiteSpace(billingJson) Then
                                        Try
                                            client.BillingAddress = JsonConvert.DeserializeObject(Of UpdatedClientBillingAddress)(billingJson)
                                        Catch ex As Exception
                                            ' Optionally log or handle error
                                            client.BillingAddress = Nothing
                                        End Try
                                    End If
                                End If

                                ' Read and deserialize ShippingAddress JSON
                                If Not IsDBNull(reader("ShippingAddress")) Then
                                    Dim shippingJson As String = reader("ShippingAddress").ToString()
                                    If Not String.IsNullOrWhiteSpace(shippingJson) Then
                                        Try
                                            client.ShippingAddress = JsonConvert.DeserializeObject(Of UpdatedClientBillingAddress)(shippingJson)
                                        Catch ex As Exception
                                            ' Optionally log or handle error
                                            client.ShippingAddress = Nothing
                                        End Try
                                    End If
                                End If

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
    End Class
End Namespace
