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

    End Class
End Namespace