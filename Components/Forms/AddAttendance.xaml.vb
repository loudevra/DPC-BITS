Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Input
Imports MySql.Data.MySqlClient

Namespace DPC.Components.Forms
    Public Class AddAttendance
        Inherits UserControl

        Public Event OnAttendanceAdded(employeeName As String, attendanceDate As String, timeIn As String, timeOut As String, note As String)

        Private _allEmployeeNames As New List(Of String)
        Private _suppressTextChanged As Boolean = False

        Public Sub New()
            InitializeComponent()
            AddHandler BtnClose.Click, AddressOf BtnClose_Click
            LoadEmployeeNames()
        End Sub

        Private Sub LoadEmployeeNames()
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()
                    Using cmd As New MySqlCommand("SELECT Name FROM employee ORDER BY Name", conn)
                        Using reader = cmd.ExecuteReader()
                            While reader.Read()
                                If Not IsDBNull(reader("Name")) Then
                                    _allEmployeeNames.Add(reader("Name").ToString())
                                End If
                            End While
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                _allEmployeeNames.Clear()
            End Try
        End Sub

        Private Sub TxtEmployee_TextChanged(sender As Object, e As TextChangedEventArgs)
            If _suppressTextChanged Then Return

            Dim query As String = TxtEmployee.Text.Trim()

            If query.Length < 1 Then
                SuggestionPopup.IsOpen = False
                Return
            End If

            Dim matches = _allEmployeeNames _
                .Where(Function(n) n.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) _
                .Take(8) _
                .ToList()

            If matches.Count = 0 Then
                SuggestionPopup.IsOpen = False
                Return
            End If

            SuggestionList.ItemsSource = matches
            SuggestionPopup.IsOpen = True
        End Sub

        Private Sub SuggestionList_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
            SelectSuggestion()
        End Sub

        Private Sub TxtEmployee_PreviewKeyDown(sender As Object, e As KeyEventArgs)
            If Not SuggestionPopup.IsOpen Then Return

            Select Case e.Key
                Case Key.Down
                    SuggestionList.Focus()
                    If SuggestionList.Items.Count > 0 Then
                        SuggestionList.SelectedIndex = 0
                        Dim item = TryCast(SuggestionList.ItemContainerGenerator.ContainerFromIndex(0), ListBoxItem)
                        item?.Focus()
                    End If
                    e.Handled = True

                Case Key.Escape
                    SuggestionPopup.IsOpen = False
                    e.Handled = True

                Case Key.Enter
                    SelectSuggestion()
                    e.Handled = True
            End Select
        End Sub

        Protected Overrides Sub OnPreviewKeyDown(e As KeyEventArgs)
            MyBase.OnPreviewKeyDown(e)
            If SuggestionPopup.IsOpen AndAlso e.Key = Key.Enter Then
                SelectSuggestion()
                e.Handled = True
            End If
        End Sub

        Private Sub TxtEmployee_LostFocus(sender As Object, e As RoutedEventArgs)
            Dispatcher.BeginInvoke(New Action(Sub()
                                                  If Not SuggestionList.IsKeyboardFocusWithin Then
                                                      SuggestionPopup.IsOpen = False
                                                  End If
                                              End Sub), System.Windows.Threading.DispatcherPriority.Background)
        End Sub

        Private Sub SelectSuggestion()
            If SuggestionList.SelectedItem IsNot Nothing Then
                _suppressTextChanged = True
                TxtEmployee.Text = SuggestionList.SelectedItem.ToString()
                TxtEmployee.CaretIndex = TxtEmployee.Text.Length
                _suppressTextChanged = False
                SuggestionPopup.IsOpen = False
                TxtEmployee.Focus()
            End If
        End Sub

        Private Sub StartDate_Click(sender As Object, e As RoutedEventArgs)
            Dim minDate As DateTime = DateTime.Today
            Dim existingDate As DateTime
            If DateTime.TryParse(TxtDateDisplay.Text, existingDate) AndAlso existingDate < DateTime.Today Then
                minDate = existingDate
            End If
            SingleDatePicker.DisplayDateStart = minDate
            SingleDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub SingleDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles SingleDatePicker.SelectedDateChanged
            If SingleDatePicker.SelectedDate.HasValue Then
                TxtDateDisplay.Text = SingleDatePicker.SelectedDate.Value.ToString("MM-dd-yyyy")
            End If
        End Sub

        Private Sub BtnAdd_Click(sender As Object, e As RoutedEventArgs)
            Dim empName As String = TxtEmployee.Text.Trim()
            Dim selectedDate As String = TxtDateDisplay.Text.Trim()
            If selectedDate = "Select a date" Then selectedDate = ""
            Dim tIn As String = TpStartTime.Text.Trim()
            Dim tOut As String = TpEndTime.Text.Trim()
            Dim noteVal As String = TxtNote.Text.Trim()

            If String.IsNullOrWhiteSpace(empName) OrElse
               String.IsNullOrWhiteSpace(selectedDate) OrElse
               String.IsNullOrWhiteSpace(tIn) OrElse
               String.IsNullOrWhiteSpace(tOut) Then
                MessageBox.Show("Please fill in all required data fields (Employee, Date, Time In, and Time Out).",
                                "Missing Data", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            If TpStartTime.SelectedTime.HasValue Then tIn = TpStartTime.SelectedTime.Value.ToString("hh:mm tt")
            If TpEndTime.SelectedTime.HasValue Then tOut = TpEndTime.SelectedTime.Value.ToString("hh:mm tt")

            RaiseEvent OnAttendanceAdded(empName, selectedDate, tIn, tOut, noteVal)
            MessageBox.Show("Attendance record successfully saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
            BtnClose_Click(sender, e)
        End Sub

        Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
            Dim parent = TryCast(Me.Parent, ContentControl)
            If parent IsNot Nothing Then
                Dim container = TryCast(parent.Parent, Panel)
                If container IsNot Nothing Then container.Children.Remove(parent)
            Else
                Dim parentPopup = TryCast(Me.Parent, Popup)
                If parentPopup IsNot Nothing Then
                    parentPopup.IsOpen = False
                Else
                    Window.GetWindow(Me)?.Close()
                End If
            End If
        End Sub

    End Class
End Namespace