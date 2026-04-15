Imports System.Collections.ObjectModel
Imports System.Windows.Media
Imports System.Windows
Imports System.Linq

Public Class calendarview

    Public Property Days As ObservableCollection(Of CalendarDay)
    Private currentDisplayDate As DateTime

    Public Sub New()
        InitializeComponent()
        Days = New ObservableCollection(Of CalendarDay)()
        CalendarGrid.ItemsSource = Days

        ' Subscribe to the global event store trigger
        AddHandler EventStore.OnEventAdded, AddressOf RefreshCalendar

        currentDisplayDate = DateTime.Now
        GenerateCalendar(currentDisplayDate)
    End Sub

    ' Safely refresh UI when an event is added from another control
    Private Sub RefreshCalendar()
        Application.Current.Dispatcher.Invoke(Sub()
                                                  GenerateCalendar(currentDisplayDate)
                                              End Sub)
    End Sub

    Private Sub GenerateCalendar(targetDate As DateTime)
        Days.Clear()

        TxtMonth.Text = targetDate.ToString("MMMM").ToUpper()
        TxtYear.Text = targetDate.Year.ToString()

        Dim whiteBrush As New SolidColorBrush(Colors.White)
        Dim grayBrush As New SolidColorBrush(Color.FromRgb(169, 169, 169))

        Dim firstDayOfMonth As New DateTime(targetDate.Year, targetDate.Month, 1)
        Dim daysInMonth As Integer = DateTime.DaysInMonth(targetDate.Year, targetDate.Month)
        Dim startDayOfWeek As Integer = CInt(firstDayOfMonth.DayOfWeek)

        ' Padding before the 1st of the month
        For i As Integer = 0 To startDayOfWeek - 1
            Days.Add(New CalendarDay With {.DayNumber = "", .CellBackground = grayBrush})
        Next

        ' Generate actual days
        For i As Integer = 1 To daysInMonth
            Dim currentDate As New DateTime(targetDate.Year, targetDate.Month, i)

            Dim newDay As New CalendarDay With {
                .DayNumber = i.ToString("D2"),
                .CellBackground = whiteBrush
            }

            ' Fetch events for this specific date
            Dim daysEvents = EventStore.Events.Where(Function(ev) ev.EventDate.Date = currentDate.Date).ToList()

            ' Add each event to the day's collection to create the pills
            For Each ev In daysEvents
                newDay.DayEvents.Add(ev)
            Next

            Days.Add(newDay)
        Next

        ' Padding after the end of the month to keep the grid even
        While Days.Count < 42
            Days.Add(New CalendarDay With {.DayNumber = "", .CellBackground = grayBrush})
        End While
    End Sub

    Private Sub BtnPrevMonth_Click(sender As Object, e As RoutedEventArgs)
        currentDisplayDate = currentDisplayDate.AddMonths(-1)
        GenerateCalendar(currentDisplayDate)
    End Sub

    Private Sub BtnNextMonth_Click(sender As Object, e As RoutedEventArgs)
        currentDisplayDate = currentDisplayDate.AddMonths(1)
        GenerateCalendar(currentDisplayDate)
    End Sub
    Private Sub EventBlock_Click(sender As Object, e As RoutedEventArgs)
        ' 1. Get the button that was clicked
        Dim clickedButton As Button = CType(sender, Button)

        ' 2. Extract the AppEvent data attached to that specific button
        Dim clickedEvent As AppEvent = CType(clickedButton.DataContext, AppEvent)

        ' 3. Show the Preview! 
        Dim previewMessage As String = $"Event Name: {clickedEvent.Title}" & vbCrLf &
                                       $"Date: {clickedEvent.EventDate.ToShortDateString()}"

        MessageBox.Show(previewMessage, "Event Details", MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub

End Class

' Data Model for a Single Day
Public Class CalendarDay
    Public Property DayNumber As String
    Public Property CellBackground As SolidColorBrush

    ' Holds the list of events for this specific day
    Public Property DayEvents As ObservableCollection(Of AppEvent)

    Public Sub New()
        DayEvents = New ObservableCollection(Of AppEvent)()
    End Sub
End Class