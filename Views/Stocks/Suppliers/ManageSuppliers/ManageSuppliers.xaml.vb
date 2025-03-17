Imports System.ComponentModel
Imports System.Windows
Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports Org.BouncyCastle.Asn1.X509.SigI

Namespace DPC.Views.Stocks.Suppliers.ManageSuppliers

    Public Class ManageSuppliers
        Inherits Window

        ' ViewModel for Date Range
        Public Property DateRangeVM As New DateRangeViewModel()

        ' Constructor
        Public Sub New()
            InitializeComponent()
            DataContext = DateRangeVM ' Bind DataContext to ViewModel
            LoadData()
        End Sub

        ' Open Start Date Picker when clicking the text
        Private Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            StartDatePicker.IsDropDownOpen = True
        End Sub

        ' Open End Date Picker when clicking the text
        Private Sub EndDate_Click(sender As Object, e As RoutedEventArgs)
            EndDatePicker.IsDropDownOpen = True
        End Sub

        Public Sub LoadData()
            Dim connectionString As String = "server=localhost;userid=root;password=;database=dpc;"
            Dim query As String = "SELECT supplierid, representative,
                                           CONCAT(officeAddress, ', ', city, ', ', region, ', ', country, ', ', postalcode) AS address, 
                                           email, phoneNumber 
                                    From supplier;"


            Dim dataList As New ObservableCollection(Of PersonData)

            Try
                Using conn As New MySqlConnection(connectionString)
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        Using reader As MySqlDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                dataList.Add(New PersonData With {
                                    .ID = reader.GetInt32("supplierid"),
                                    .Name = reader.GetString("representative"),
                                    .Address = reader.GetString("address"),
                                    .Email = reader.GetString("email"),
                                    .Phone = reader.GetInt32("phoneNumber")
                                })
                            End While
                        End Using
                    End Using
                End Using

                ' Bind Data to DataGrid
                dataGrid.ItemsSource = dataList

            Catch ex As Exception
                MessageBox.Show("Error: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub
    End Class

    Public Class PersonData
        Public Property ID As Integer
        Public Property Name As String
        Public Property Address As String
        Public Property Email As String
        Public Property Phone As Integer
    End Class

    ' ViewModel for Date Range Picker
    Public Class DateRangeViewModel
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

End Namespace
