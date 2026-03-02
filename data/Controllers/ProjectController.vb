Imports MySql.Data.MySqlClient
Imports DPC.DPC.Data.Helpers

Namespace DPC.Data.Controllers
    Public Class ProjectController

        Public Shared Function CreateProject(proj As DPC.Data.Model.Project) As Boolean
            Dim query As String = "INSERT INTO project (ProjectName, Status, Customer, Budget, StartDate, DueDate, " &
                                  "CalculationMode, LinkToCalendar, AssignedTo, Note, CreatedAt, UpdatedAt) " &
                                  "VALUES (@ProjectName, @Status, @Customer, @Budget, @StartDate, @DueDate, " &
                                  "@CalculationMode, @LinkToCalendar, @AssignedTo, @Note, @CreatedAt, @UpdatedAt)"
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@ProjectName", proj.ProjectName)
                        cmd.Parameters.AddWithValue("@Status", If(proj.Status, DBNull.Value))
                        cmd.Parameters.AddWithValue("@Customer", If(proj.Customer, DBNull.Value))
                        cmd.Parameters.AddWithValue("@Budget", proj.Budget)
                        cmd.Parameters.AddWithValue("@StartDate", If(proj.StartDate.HasValue, proj.StartDate.Value, DBNull.Value))
                        cmd.Parameters.AddWithValue("@DueDate", If(proj.DueDate.HasValue, proj.DueDate.Value, DBNull.Value))
                        cmd.Parameters.AddWithValue("@CalculationMode", If(proj.CalculationMode, DBNull.Value))
                        cmd.Parameters.AddWithValue("@LinkToCalendar", proj.LinkToCalendar)
                        cmd.Parameters.AddWithValue("@AssignedTo", If(String.IsNullOrEmpty(proj.AssignedTo), DBNull.Value, proj.AssignedTo))
                        cmd.Parameters.AddWithValue("@Note", If(proj.Note, DBNull.Value))
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now)
                        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now)
                        Dim result As Integer = cmd.ExecuteNonQuery()
                        Return result > 0
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error creating project: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                End Try
            End Using
        End Function

        Public Shared Function GetProjects() As List(Of DPC.Data.Model.Project)
            Dim results As New List(Of DPC.Data.Model.Project)()
            Dim query As String = "SELECT p.ProjectID, p.ProjectName, p.Status, p.Customer, p.Budget, " &
                      "p.StartDate, p.DueDate, p.CalculationMode, p.LinkToCalendar, p.AssignedTo, " &
                      "p.Note, IFNULL(e.Name, '') AS AssignedToName " &
                      "FROM project p " &
                      "LEFT JOIN employee e ON e.EmployeeID = p.AssignedTo " &
                      "ORDER BY p.ProjectID DESC"
            Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                Try
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                results.Add(New DPC.Data.Model.Project With {
                                    .ProjectID = Convert.ToInt32(reader("ProjectID")),
                                    .ProjectName = reader("ProjectName").ToString(),
                                    .Status = If(IsDBNull(reader("Status")), "", reader("Status").ToString()),
                                    .Customer = If(IsDBNull(reader("Customer")), "", reader("Customer").ToString()),
                                    .Budget = If(IsDBNull(reader("Budget")), 0L, Convert.ToInt64(reader("Budget"))),
                                    .StartDate = If(IsDBNull(reader("StartDate")), Nothing, CType(reader("StartDate"), Date?)),
                                    .DueDate = If(IsDBNull(reader("DueDate")), Nothing, CType(reader("DueDate"), Date?)),
                                    .CalculationMode = If(IsDBNull(reader("CalculationMode")), "", reader("CalculationMode").ToString()),
                                    .LinkToCalendar = If(IsDBNull(reader("LinkToCalendar")), False, Convert.ToBoolean(reader("LinkToCalendar"))),
                                    .AssignedTo = If(IsDBNull(reader("AssignedTo")), "", reader("AssignedTo").ToString()),
                                    .AssignedToName = If(IsDBNull(reader("AssignedToName")), "", reader("AssignedToName").ToString()),
                                    .Note = If(IsDBNull(reader("Note")), "", reader("Note").ToString())
                                })
                            End While
                        End Using
                    End Using
                Catch ex As Exception
                    MessageBox.Show("Error loading projects: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End Using
            Return results
        End Function

    End Class
End Namespace