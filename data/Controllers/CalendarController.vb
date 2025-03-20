Imports System.ComponentModel
Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model

Namespace DPC.Data.Controllers
    Public Class CalendarController
        Public Class MultiCalendar
            Implements INotifyPropertyChanged

            Private _startDate As Date? = Date.Now
            Private _endDate As Date? = Date.Now.AddDays(1) ' Tomorrow

            ' Start Date (Today)
            Public Property StartDate As Date?
                Get
                    Return _startDate
                End Get
                Set(value As Date?)
                    _startDate = value
                    OnPropertyChanged(NameOf(StartDate))
                End Set
            End Property

            ' End Date (Tomorrow)
            Public Property EndDate As Date?
                Get
                    Return _endDate
                End Get
                Set(value As Date?)
                    _endDate = value
                    OnPropertyChanged(NameOf(EndDate))
                End Set
            End Property

            ' Event to handle property changes
            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

            Protected Overridable Sub OnPropertyChanged(propertyName As String)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
            End Sub
        End Class

        Public Class SingleCalendar
            Implements INotifyPropertyChanged

            Private _selectedDate As Date? = Date.Now

            ' Single Date (Today)
            Public Property SelectedDate As Date?
                Get
                    Return _selectedDate
                End Get
                Set(value As Date?)
                    _selectedDate = value
                    OnPropertyChanged(NameOf(SelectedDate))
                End Set
            End Property

            ' Event to handle property changes
            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

            Protected Overridable Sub OnPropertyChanged(propertyName As String)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
            End Sub
        End Class
    End Class
End Namespace