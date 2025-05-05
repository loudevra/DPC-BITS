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
            ' Change default to null
            Private _selectedDate As DateTime? = Nothing
            Private _minimumDate As DateTime = DateTime.Today

            Public Property SelectedDate As DateTime?
                Get
                    Return _selectedDate
                End Get
                Set(value As DateTime?)
                    If Not Equals(_selectedDate, value) Then
                        _selectedDate = value
                        OnPropertyChanged("SelectedDate")
                        ' Also raise property changed for FormattedDate when SelectedDate changes
                        OnPropertyChanged("FormattedDate")
                    End If
                End Set
            End Property

            ' Add a new property for formatted date display
            Public ReadOnly Property FormattedDate As String
                Get
                    If _selectedDate.HasValue Then
                        Return _selectedDate.Value.ToString("MMM dd, yyyy")
                    Else
                        Return "Select a date"  ' Default text when no date is selected
                    End If
                End Get
            End Property

            Public Property MinimumDate As DateTime
                Get
                    Return _minimumDate
                End Get
                Set(value As DateTime)
                    If _minimumDate <> value Then
                        _minimumDate = value
                        OnPropertyChanged("MinimumDate")
                    End If
                End Set
            End Property

            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

            Protected Sub OnPropertyChanged(propertyName As String)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
            End Sub
        End Class
    End Class
End Namespace