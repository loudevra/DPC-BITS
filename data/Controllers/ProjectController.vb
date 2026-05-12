Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Helpers

Namespace DPC.Data.Controllers
    Public Class ProjectController

        ' =========================================================
        ' MAPPER HELPER
        ' Central place that converts a reader row → Project model.
        ' All Read methods call this so ProjectList is never missed.
        ' =========================================================
        Private Shared Function MapProject(reader As MySqlDataReader) As DPC.Data.Model.Project
            Return New DPC.Data.Model.Project With {
                .ProjectID = Convert.ToInt32(reader("ProjectID")),
                .ProjectDate = If(IsDBNull(reader("ProjectDate")), Nothing, CType(reader("ProjectDate"), DateTime?)),
                .ReferenceNumber = If(IsDBNull(reader("ReferenceNumber")), "", reader("ReferenceNumber").ToString()),
                .ProjectTitle = If(IsDBNull(reader("ProjectTitle")), "", reader("ProjectTitle").ToString()),
                .Category = If(IsDBNull(reader("Category")), "", reader("Category").ToString()),
                .ProjectType = If(IsDBNull(reader("ProjectType")), "", reader("ProjectType").ToString()),
                .ContactPerson = If(IsDBNull(reader("ContactPerson")), "", reader("ContactPerson").ToString()),
                .ContactNumber = If(IsDBNull(reader("ContactNumber")), "", reader("ContactNumber").ToString()),
                .EmailAddress = If(IsDBNull(reader("EmailAddress")), "", reader("EmailAddress").ToString()),
                .AreaOfDelivery = If(IsDBNull(reader("AreaOfDelivery")), "", reader("AreaOfDelivery").ToString()),
                .PreBidDate = If(IsDBNull(reader("PreBidDate")), Nothing, CType(reader("PreBidDate"), DateTime?)),
                .ClosingDate = If(IsDBNull(reader("ClosingDate")), Nothing, CType(reader("ClosingDate"), DateTime?)),
                .ABC = If(IsDBNull(reader("ABC")), 0L, Convert.ToInt64(reader("ABC"))),
                .BidRFQOffer = If(IsDBNull(reader("BidRFQOffer")), 0L, Convert.ToInt64(reader("BidRFQOffer"))),
                .ReceiveDate = If(IsDBNull(reader("ReceiveDate")), Nothing, CType(reader("ReceiveDate"), DateTime?)),
                .ModeOfSubmission = If(IsDBNull(reader("ModeOfSubmission")), "", reader("ModeOfSubmission").ToString()),
                .Status = If(IsDBNull(reader("Status")), "", reader("Status").ToString()),
                .Remarks = If(IsDBNull(reader("Remarks")), "", reader("Remarks").ToString()),
                .AssignSales = If(IsDBNull(reader("AssignSales")), "", reader("AssignSales").ToString()),
                .ProjectList = If(IsDBNull(reader("ProjectList")), "DPC_GOV_SALES", reader("ProjectList").ToString()),
                .Note = If(IsDBNull(reader("Note")), "", reader("Note").ToString()),
                .CreatedAt = If(IsDBNull(reader("CreatedAt")), Nothing, CType(reader("CreatedAt"), DateTime?)),
                .UpdatedAt = If(IsDBNull(reader("UpdatedAt")), Nothing, CType(reader("UpdatedAt"), DateTime?))
            }
        End Function

        ' =========================================================
        ' CREATE
        ' =========================================================
        Public Shared Function CreateProject(proj As DPC.Data.Model.Project) As Boolean
            Dim query As String =
                "INSERT INTO project (" &
                "  ProjectDate, ReferenceNumber, ProjectTitle, Category, ProjectType, " &
                "  ContactPerson, ContactNumber, EmailAddress, AreaOfDelivery, " &
                "  PreBidDate, ClosingDate, ABC, BidRFQOffer, ReceiveDate, " &
                "  ModeOfSubmission, Status, Remarks, AssignSales, ProjectList, Note, " &
                "  CreatedAt, UpdatedAt" &
                ") VALUES (" &
                "  @ProjectDate, @ReferenceNumber, @ProjectTitle, @Category, @ProjectType, " &
                "  @ContactPerson, @ContactNumber, @EmailAddress, @AreaOfDelivery, " &
                "  @PreBidDate, @ClosingDate, @ABC, @BidRFQOffer, @ReceiveDate, " &
                "  @ModeOfSubmission, @Status, @Remarks, @AssignSales, @ProjectList, @Note, " &
                "  @CreatedAt, @UpdatedAt" &
                ")"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ProjectDate", If(proj.ProjectDate.HasValue, proj.ProjectDate.Value, DBNull.Value))
                        cmd.Parameters.AddWithValue("@ReferenceNumber", If(String.IsNullOrWhiteSpace(proj.ReferenceNumber), DBNull.Value, proj.ReferenceNumber))
                        cmd.Parameters.AddWithValue("@ProjectTitle", proj.ProjectTitle)
                        cmd.Parameters.AddWithValue("@Category", If(String.IsNullOrWhiteSpace(proj.Category), DBNull.Value, proj.Category))
                        cmd.Parameters.AddWithValue("@ProjectType", If(String.IsNullOrWhiteSpace(proj.ProjectType), DBNull.Value, proj.ProjectType))
                        cmd.Parameters.AddWithValue("@ContactPerson", If(String.IsNullOrWhiteSpace(proj.ContactPerson), DBNull.Value, proj.ContactPerson))
                        cmd.Parameters.AddWithValue("@ContactNumber", If(String.IsNullOrWhiteSpace(proj.ContactNumber), DBNull.Value, proj.ContactNumber))
                        cmd.Parameters.AddWithValue("@EmailAddress", If(String.IsNullOrWhiteSpace(proj.EmailAddress), DBNull.Value, proj.EmailAddress))
                        cmd.Parameters.AddWithValue("@AreaOfDelivery", If(String.IsNullOrWhiteSpace(proj.AreaOfDelivery), DBNull.Value, proj.AreaOfDelivery))
                        cmd.Parameters.AddWithValue("@PreBidDate", If(proj.PreBidDate.HasValue, proj.PreBidDate.Value, DBNull.Value))
                        cmd.Parameters.AddWithValue("@ClosingDate", If(proj.ClosingDate.HasValue, proj.ClosingDate.Value, DBNull.Value))
                        cmd.Parameters.AddWithValue("@ABC", proj.ABC)
                        cmd.Parameters.AddWithValue("@BidRFQOffer", proj.BidRFQOffer)
                        cmd.Parameters.AddWithValue("@ReceiveDate", If(proj.ReceiveDate.HasValue, proj.ReceiveDate.Value, DBNull.Value))
                        cmd.Parameters.AddWithValue("@ModeOfSubmission", If(String.IsNullOrWhiteSpace(proj.ModeOfSubmission), DBNull.Value, proj.ModeOfSubmission))
                        cmd.Parameters.AddWithValue("@Status", If(String.IsNullOrWhiteSpace(proj.Status), DBNull.Value, proj.Status))
                        cmd.Parameters.AddWithValue("@Remarks", If(String.IsNullOrWhiteSpace(proj.Remarks), DBNull.Value, proj.Remarks))
                        cmd.Parameters.AddWithValue("@AssignSales", If(String.IsNullOrWhiteSpace(proj.AssignSales), DBNull.Value, proj.AssignSales))
                        cmd.Parameters.AddWithValue("@ProjectList", If(String.IsNullOrWhiteSpace(proj.ProjectList), "DPC_GOV_SALES", proj.ProjectList))
                        cmd.Parameters.AddWithValue("@Note", If(String.IsNullOrWhiteSpace(proj.Note), DBNull.Value, proj.Note))
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now)
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now)

                        Dim result As Integer = cmd.ExecuteNonQuery()
                        Return result > 0
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error creating project: " & ex.Message,
                                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                End Try
            End Using
        End Function

        ' =========================================================
        ' READ — DPC GOV SALES (default list)
        ' =========================================================
        Public Shared Function GetProjects() As List(Of DPC.Data.Model.Project)
            Dim results As New List(Of DPC.Data.Model.Project)()

            Dim query As String =
                "SELECT * FROM project " &
                "WHERE ProjectList = 'DPC_GOV_SALES' " &
                "ORDER BY ProjectID DESC"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                results.Add(MapProject(reader))
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error loading projects: " & ex.Message,
                                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Return results
        End Function

        ' =========================================================
        ' READ — AWARDED PROJECTS
        ' =========================================================
        Public Shared Function GetAwardedProjects() As List(Of DPC.Data.Model.Project)
            Dim results As New List(Of DPC.Data.Model.Project)()

            Dim query As String =
                "SELECT * FROM project " &
                "WHERE ProjectList = 'AWARDED_PROJECTS' " &
                "ORDER BY ProjectID DESC"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                results.Add(MapProject(reader))
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error loading awarded projects: " & ex.Message,
                                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Return results
        End Function

        ' =========================================================
        ' READ — COLLECTION
        ' =========================================================
        Public Shared Function GetCollectionData() As List(Of DPC.Data.Model.Project)
            Dim results As New List(Of DPC.Data.Model.Project)()

            Dim query As String =
                "SELECT * FROM project " &
                "WHERE ProjectList = 'COLLECTION' " &
                "ORDER BY ProjectID DESC"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                results.Add(MapProject(reader))
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error loading collection data: " & ex.Message,
                                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using

            Return results
        End Function

        ' =========================================================
        ' UPDATE
        ' =========================================================
        Public Shared Function UpdateProject(proj As DPC.Data.Model.Project) As Boolean
            Dim query As String =
                "UPDATE project SET " &
                "  ProjectDate       = @ProjectDate, " &
                "  ReferenceNumber   = @ReferenceNumber, " &
                "  ProjectTitle      = @ProjectTitle, " &
                "  Category          = @Category, " &
                "  ProjectType       = @ProjectType, " &
                "  ContactPerson     = @ContactPerson, " &
                "  ContactNumber     = @ContactNumber, " &
                "  EmailAddress      = @EmailAddress, " &
                "  AreaOfDelivery    = @AreaOfDelivery, " &
                "  PreBidDate        = @PreBidDate, " &
                "  ClosingDate       = @ClosingDate, " &
                "  ABC               = @ABC, " &
                "  BidRFQOffer       = @BidRFQOffer, " &
                "  ReceiveDate       = @ReceiveDate, " &
                "  ModeOfSubmission  = @ModeOfSubmission, " &
                "  Status            = @Status, " &
                "  Remarks           = @Remarks, " &
                "  AssignSales       = @AssignSales, " &
                "  ProjectList       = @ProjectList, " &
                "  Note              = @Note, " &
                "  UpdatedAt         = @UpdatedAt " &
                "WHERE ProjectID = @ProjectID"

            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ProjectID", proj.ProjectID)
                        cmd.Parameters.AddWithValue("@ProjectDate", If(proj.ProjectDate.HasValue, proj.ProjectDate.Value, DBNull.Value))
                        cmd.Parameters.AddWithValue("@ReferenceNumber", If(String.IsNullOrWhiteSpace(proj.ReferenceNumber), DBNull.Value, proj.ReferenceNumber))
                        cmd.Parameters.AddWithValue("@ProjectTitle", proj.ProjectTitle)
                        cmd.Parameters.AddWithValue("@Category", If(String.IsNullOrWhiteSpace(proj.Category), DBNull.Value, proj.Category))
                        cmd.Parameters.AddWithValue("@ProjectType", If(String.IsNullOrWhiteSpace(proj.ProjectType), DBNull.Value, proj.ProjectType))
                        cmd.Parameters.AddWithValue("@ContactPerson", If(String.IsNullOrWhiteSpace(proj.ContactPerson), DBNull.Value, proj.ContactPerson))
                        cmd.Parameters.AddWithValue("@ContactNumber", If(String.IsNullOrWhiteSpace(proj.ContactNumber), DBNull.Value, proj.ContactNumber))
                        cmd.Parameters.AddWithValue("@EmailAddress", If(String.IsNullOrWhiteSpace(proj.EmailAddress), DBNull.Value, proj.EmailAddress))
                        cmd.Parameters.AddWithValue("@AreaOfDelivery", If(String.IsNullOrWhiteSpace(proj.AreaOfDelivery), DBNull.Value, proj.AreaOfDelivery))
                        cmd.Parameters.AddWithValue("@PreBidDate", If(proj.PreBidDate.HasValue, proj.PreBidDate.Value, DBNull.Value))
                        cmd.Parameters.AddWithValue("@ClosingDate", If(proj.ClosingDate.HasValue, proj.ClosingDate.Value, DBNull.Value))
                        cmd.Parameters.AddWithValue("@ABC", proj.ABC)
                        cmd.Parameters.AddWithValue("@BidRFQOffer", proj.BidRFQOffer)
                        cmd.Parameters.AddWithValue("@ReceiveDate", If(proj.ReceiveDate.HasValue, proj.ReceiveDate.Value, DBNull.Value))
                        cmd.Parameters.AddWithValue("@ModeOfSubmission", If(String.IsNullOrWhiteSpace(proj.ModeOfSubmission), DBNull.Value, proj.ModeOfSubmission))
                        cmd.Parameters.AddWithValue("@Status", If(String.IsNullOrWhiteSpace(proj.Status), DBNull.Value, proj.Status))
                        cmd.Parameters.AddWithValue("@Remarks", If(String.IsNullOrWhiteSpace(proj.Remarks), DBNull.Value, proj.Remarks))
                        cmd.Parameters.AddWithValue("@AssignSales", If(String.IsNullOrWhiteSpace(proj.AssignSales), DBNull.Value, proj.AssignSales))
                        cmd.Parameters.AddWithValue("@ProjectList", If(String.IsNullOrWhiteSpace(proj.ProjectList), "DPC_GOV_SALES", proj.ProjectList))
                        cmd.Parameters.AddWithValue("@Note", If(String.IsNullOrWhiteSpace(proj.Note), DBNull.Value, proj.Note))
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now)

                        Dim result As Integer = cmd.ExecuteNonQuery()
                        Return result > 0
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error updating project: " & ex.Message,
                                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                End Try
            End Using
        End Function

        ' =========================================================
        ' DELETE
        ' =========================================================
        Public Shared Function DeleteProject(projectID As Integer) As Boolean
            Dim query As String = "DELETE FROM project WHERE ProjectID = @ProjectID"
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ProjectID", projectID)
                        Return cmd.ExecuteNonQuery() > 0
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error deleting project: " & ex.Message,
                                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                End Try
            End Using
        End Function

    End Class
End Namespace
