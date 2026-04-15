Imports System.Collections.Generic
Imports System.Windows.Media

' Represents a single event
Public Class AppEvent
    Public Property EventDate As DateTime
    Public Property Title As String
    Public Property EventColor As SolidColorBrush
End Class

' A global store to share data between your UserControls
Public Module EventStore
    Public Property Events As New List(Of AppEvent)()

    ' Event to notify the calendar that a new event was added
    Public Event OnEventAdded()

    Public Sub AddNewEvent(newEvent As AppEvent)
        Events.Add(newEvent)
        RaiseEvent OnEventAdded()
    End Sub
End Module