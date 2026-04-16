Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports System.Windows

Public Class EventStore

    Public Shared Property Events As New ObservableCollection(Of AppEvent)()
    Public Shared Event OnEventAdded()

    Public Shared Sub LoadAllEvents()
        Dim tempList As New List(Of AppEvent)()

        Try
            Using conn As MySqlConnection = DPC.SplashScreen.GetDatabaseConnection()
                conn.Open()
                Dim query As String = "SELECT eventID, title, eventDate, category FROM calendar"
                Using cmd As New MySqlCommand(query, conn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            ' Store raw data only — NO brushes here (wrong thread)
                            tempList.Add(New AppEvent() With {
                                .EventID = reader.GetInt32("eventID"),
                                .Title = reader.GetString("title"),
                                .EventDate = reader.GetDateTime("eventDate"),
                                .Category = reader.GetString("category")
                            })
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Application.Current.Dispatcher.Invoke(Sub()
                                                      MessageBox.Show("Load error: " & ex.Message)
                                                  End Sub)
            Return
        End Try

        ' Update collection AND create brushes on UI thread
        Application.Current.Dispatcher.Invoke(Sub()
                                                  Events.Clear()
                                                  For Each ev In tempList
                                                      ev.EventColor = AppEvent.GetColorForCategory(ev.Category)
                                                      Events.Add(ev)
                                                  Next
                                              End Sub)
    End Sub

    Public Shared Sub AddNewEvent(newEvent As AppEvent)
        Try
            Using conn As MySqlConnection = DPC.SplashScreen.GetDatabaseConnection()
                conn.Open()
                Dim query As String =
                    "INSERT INTO calendar (title, eventDate, category) VALUES (@title, @eventDate, @category)"
                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@title", newEvent.Title)
                    cmd.Parameters.AddWithValue("@eventDate", newEvent.EventDate.ToString("yyyy-MM-dd"))
                    cmd.Parameters.AddWithValue("@category", newEvent.Category)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Application.Current.Dispatcher.Invoke(Sub()
                                                      MessageBox.Show("Save error: " & ex.Message)
                                                  End Sub)
            Return
        End Try

        LoadAllEvents()
        RaiseEvent OnEventAdded()
    End Sub

End Class