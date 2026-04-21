Imports System.Collections.ObjectModel
Imports System.Data
Imports DPC.DPC.Views.HRM.Employees.Holidays
Imports DPC.Views.HRM.Employees.Holidays
Imports MySql.Data.MySqlClient

Namespace DPC.Data.Controllers
    Public Class HolidayController

        ' ─────────────────────────────────────────────
        ' LOAD – fills the ObservableCollection from DB
        ' ─────────────────────────────────────────────
        Public Shared Function LoadHolidays(holidayList As ObservableCollection(Of HolidayModel)) As Boolean
            Try
                Dim query As String = "
                    SELECT holidayID, fromDate, toDate, days, note
                    FROM holidays
                    ORDER BY holidayID ASC"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()

                            holidayList.Clear()
                            Dim rowNumber As Integer = 1

                            While reader.Read()
                                holidayList.Add(New HolidayModel With {
                                    .ID = rowNumber,
                                    .HolidayID = Convert.ToInt32(reader("holidayID")),
                                    .FromDate = Convert.ToDateTime(reader("fromDate")).ToString("MM/dd/yyyy"),
                                    .ToDate = Convert.ToDateTime(reader("toDate")).ToString("MM/dd/yyyy"),
                                    .Days = Convert.ToInt32(reader("days")),
                                    .Note = If(reader.IsDBNull(reader.GetOrdinal("note")), "", reader("note").ToString()),
                                    .Action = "Edit/Delete"
                                })
                                rowNumber += 1
                            End While

                        End Using
                    End Using
                End Using

                Return True
            Catch ex As Exception
                MessageBox.Show($"An error occurred while loading holidays: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function

        ' ─────────────────────────────────────────────
        ' INSERT – saves a new holiday row
        ' ─────────────────────────────────────────────
        Public Shared Function InsertHoliday(fromDate As Date, toDate As Date, days As Integer, note As String) As Boolean
            Try
                Dim query As String = "
                    INSERT INTO holidays (fromDate, toDate, days, note, dateCreated, dateModified)
                    VALUES (@fromDate, @toDate, @days, @note, NOW(), NOW())"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using transaction As MySqlTransaction = conn.BeginTransaction()
                        Try
                            Using cmd As New MySqlCommand(query, conn, transaction)
                                cmd.Parameters.AddWithValue("@fromDate", fromDate.ToString("yyyy-MM-dd"))
                                cmd.Parameters.AddWithValue("@toDate", toDate.ToString("yyyy-MM-dd"))
                                cmd.Parameters.AddWithValue("@days", days)
                                cmd.Parameters.AddWithValue("@note", If(String.IsNullOrWhiteSpace(note), DBNull.Value, note))
                                cmd.ExecuteNonQuery()
                            End Using

                            transaction.Commit()
                        Catch ex As Exception
                            transaction.Rollback()
                            MessageBox.Show($"Error inserting holiday: {ex.Message}", "Insert Error", MessageBoxButton.OK, MessageBoxImage.Error)
                            Return False
                        End Try
                    End Using
                End Using

                Return True
            Catch ex As Exception
                MessageBox.Show($"Error accessing database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function

        ' ─────────────────────────────────────────────
        ' UPDATE – edits an existing holiday row by DB primary key
        ' ─────────────────────────────────────────────
        Public Shared Function UpdateHoliday(holidayID As Integer, fromDate As Date, toDate As Date, days As Integer, note As String) As Boolean
            Try
                Dim query As String = "
                    UPDATE holidays
                    SET fromDate      = @fromDate,
                        toDate        = @toDate,
                        days          = @days,
                        note          = @note,
                        dateModified  = NOW()
                    WHERE holidayID   = @holidayID"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using transaction As MySqlTransaction = conn.BeginTransaction()
                        Try
                            Using cmd As New MySqlCommand(query, conn, transaction)
                                cmd.Parameters.AddWithValue("@holidayID", holidayID)
                                cmd.Parameters.AddWithValue("@fromDate", fromDate.ToString("yyyy-MM-dd"))
                                cmd.Parameters.AddWithValue("@toDate", toDate.ToString("yyyy-MM-dd"))
                                cmd.Parameters.AddWithValue("@days", days)
                                cmd.Parameters.AddWithValue("@note", If(String.IsNullOrWhiteSpace(note), DBNull.Value, note))
                                cmd.ExecuteNonQuery()
                            End Using

                            transaction.Commit()
                        Catch ex As Exception
                            transaction.Rollback()
                            MessageBox.Show($"Error updating holiday: {ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error)
                            Return False
                        End Try
                    End Using
                End Using

                Return True
            Catch ex As Exception
                MessageBox.Show($"Error accessing database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function

        ' ─────────────────────────────────────────────
        ' DELETE – removes a holiday row by DB primary key
        ' ─────────────────────────────────────────────
        Public Shared Function DeleteHoliday(holidayID As Integer) As Boolean
            Try
                Dim query As String = "DELETE FROM holidays WHERE holidayID = @holidayID"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using transaction As MySqlTransaction = conn.BeginTransaction()
                        Try
                            Using cmd As New MySqlCommand(query, conn, transaction)
                                cmd.Parameters.AddWithValue("@holidayID", holidayID)
                                cmd.ExecuteNonQuery()
                            End Using

                            transaction.Commit()
                        Catch ex As Exception
                            transaction.Rollback()
                            MessageBox.Show($"Error deleting holiday: {ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error)
                            Return False
                        End Try
                    End Using
                End Using

                Return True
            Catch ex As Exception
                MessageBox.Show($"Error accessing database: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function

    End Class
End Namespace