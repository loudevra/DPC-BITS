Imports System.Threading
Imports DocumentFormat.OpenXml.Bibliography
Imports DocumentFormat.OpenXml.Drawing.Diagrams
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.Misc.CashAdvance
Imports MaterialDesignThemes.Wpf
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Namespace DPC.Data.Controllers.Misc
    Public Class NewCashAdvanceController

        ' This class handles the logic for managing cash advance requests in the application.
        Private isFormatting As Boolean = False
        Private debouceCts As CancellationTokenSource
        Dim checkInput As Boolean
        Dim addRowSuccess As Boolean
        Private cashadvancerequest As CashAdvanceNewRequest
        Public RequestAmountRows As New List(Of Grid)
        Dim MaxNum As Integer = 8

        ' Constructor to initialize the controller with a reference to the CashAdvanceNewRequest form instance.
        Public Sub New(formInstance As CashAdvanceNewRequest)
            cashadvancerequest = formInstance
        End Sub


        ' This function checks if any of the TextBox controls in the RequestAmountRows list are empty or contain the default value "₱ 0.00".
        Public Function HasAnyRowWithEmptyTextBox() As Boolean

            ' Check if any TextBox in the RequestAmountRows has an empty or "₱ 0.00" value
            For Each rowGrid As Grid In RequestAmountRows
                For Each child As UIElement In rowGrid.Children
                    If TypeOf child Is Border Then
                        Dim border = DirectCast(child, Border)
                        If TypeOf border.Child Is TextBox Then
                            Dim txt = DirectCast(border.Child, TextBox)
                            If String.IsNullOrWhiteSpace(txt.Text) Or txt.Text = "₱ 0.00" Then
                                Return True
                            End If
                        End If
                    End If
                Next
            Next

            Return False ' All rows are fully filled
        End Function

        ' This function manages the addition of a new row when the Tab key is pressed in the Reason TextBox.
        Private Sub ManageRowAdding(e As KeyEventArgs, txtReason As TextBox, txtRequest As TextBox)
            If e.Key = Key.Tab Then
                e.Handled = True
                AddAmountRequestRow()
                If addRowSuccess Then
                    txtReason.MoveFocus(New TraversalRequest(FocusNavigationDirection.Next))
                End If
                addRowSuccess = False
            ElseIf e.Key = Key.Back Then
                If String.IsNullOrWhiteSpace(txtReason.Text) Then
                    e.Handled = True
                    txtRequest.Focus()
                    txtRequest.CaretIndex = txtRequest.Text.Length
                End If
            End If
        End Sub

        ' This function manages the deletion of a row when the Backspace key is pressed or when the remove button is clicked.
        Private Sub ManageRowDeletion(e As KeyEventArgs, txtRequest As TextBox, txtReason As TextBox, Inputcon As Grid, isBtnClicked As Boolean)
            If e IsNot Nothing Then
                If e.Key = Key.Back Then
                    If String.IsNullOrWhiteSpace(txtRequest.Text) Then
                        If RequestAmountRows.Count = 1 Then
                            MessageBox.Show("You can't remove this single row")
                        Else
                            Dim result As MessageBoxResult = MessageBox.Show("Do you want to delete this row?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question)

                            If result = MessageBoxResult.Yes Then
                                e.Handled = True
                                txtRequest.MoveFocus(New TraversalRequest(FocusNavigationDirection.Previous))
                                cashadvancerequest.SPRequestAmount.Children.Remove(Inputcon)
                                RequestAmountRows.Remove(Inputcon)
                            End If

                        End If
                    End If
                ElseIf e.Key = Key.Tab Then
                    e.Handled = True
                    txtReason.Focus()
                End If
            End If

            If isBtnClicked = True Then
                If RequestAmountRows.Count = 1 Then
                    MessageBox.Show("You can't remove this single row")
                Else
                    Dim result As MessageBoxResult = MessageBox.Show("Do you want to delete this row?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question)

                    If result = MessageBoxResult.Yes Then
                        txtRequest.MoveFocus(New TraversalRequest(FocusNavigationDirection.Previous))
                        cashadvancerequest.SPRequestAmount.Children.Remove(Inputcon)
                        RequestAmountRows.Remove(Inputcon)
                    End If
                End If
            End If
        End Sub


        ' This function calculates the total amount of all request amounts in the RequestAmountRows and updates the total display.
        Private Sub TotalRequestAmount()
            Dim requestAmounts As New List(Of String)

            For Each row As Grid In cashadvancerequest.SPRequestAmount.Children
                For Each child In row.Children
                    If TypeOf child Is Border AndAlso TypeOf CType(child, Border).Child Is TextBox Then
                        Dim tb = CType(CType(child, Border).Child, TextBox)

                        If tb.Name.StartsWith("TxtRequestAmount") Then
                            requestAmounts.Add(tb.Text)
                        End If
                    End If
                Next
            Next

            Dim totalAmount As Decimal = 0D

            For Each amountText In requestAmounts
                Dim amount As Decimal
                If Decimal.TryParse(amountText.Replace("₱", "").Replace(",", "").Trim(), amount) Then
                    totalAmount += amount
                End If
            Next

            ' Update the total amount display
            If totalAmount > 0 Then
                cashadvancerequest.TbTotalRequestAmount.Text = "₱ " & totalAmount.ToString("N2")
            Else
                cashadvancerequest.TbTotalRequestAmount.Text = "₱ 0.00"
            End If

        End Sub


        ' This function adds a new row for requesting an amount, ensuring that all required fields are filled and that the maximum number of rows is not exceeded.
        Public Sub AddAmountRequestRow()

            If HasAnyRowWithEmptyTextBox() Then
                MessageBox.Show("All columns must be filled correctly.")
                addRowSuccess = False
            Else
                If RequestAmountRows.Count >= MaxNum Then
                    MessageBox.Show("You can only add up to " & MaxNum & " rows.")
                    Return
                Else
                    addRowSuccess = True

                    ' Check if the SPRequestAmount container is initialized
#Region "ROW"

                    ' Create parent grid
                    Dim Inputs_container As New Grid With {
                .Name = "GridContainer" & Guid.NewGuid().ToString("N")
            }

                    ' Column definitions
                    For i = 1 To 4
                        Inputs_container.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
                    Next

                    ' ===== Request Amount =====
                    Dim RequestAmountBorder As New Border With {
                .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
                .Margin = New Thickness(0, -1, 0, 0),
                .BorderThickness = New Thickness(2, 0, 0, 2)
            }

                    Dim TxtRequestAmount As New TextBox With {
                .FontFamily = TryCast(cashadvancerequest.FindResource("Lexend"), FontFamily),
                .FontSize = 14,
                .TextAlignment = TextAlignment.Center,
                .Padding = New Thickness(0, 10, 0, 10),
                .BorderThickness = New Thickness(0),
                .Text = "₱ 0.00",
                .Name = "TxtRequestAmount" & Guid.NewGuid().ToString("N")
            }

                    RequestAmountBorder.Child = TxtRequestAmount
                    Grid.SetColumn(RequestAmountBorder, 0)
                    Inputs_container.Children.Add(RequestAmountBorder)

                    ' ===== Date Needed with DatePicker, Icon, and Button =====
                    Dim DateNeededBorder As New Border With {
                .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
                .Background = Brushes.White,
                .Margin = New Thickness(-1, -1, 0, 0),
                .BorderThickness = New Thickness(2, 0, 1, 2)
            }

                    Dim dateGrid As New Grid()
                    dateGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(50)})
                    dateGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})

                    Dim RequestAmountDatePicker As New DatePicker With {
                .Style = TryCast(cashadvancerequest.FindResource("SingleDatePickerStyle"), System.Windows.Style),
                .FontFamily = TryCast(cashadvancerequest.FindResource("Lexend"), FontFamily),
                .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
                .FontWeight = FontWeights.SemiBold,
                .Name = "RequestAmountDatePicker" & Guid.NewGuid().ToString("N")
            }

                    Dim CalendarIcon As New PackIcon With {
                .Kind = PackIconKind.Calendar,
                .Width = 25,
                .Height = 25,
                .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
                .HorizontalAlignment = HorizontalAlignment.Center,
                .VerticalAlignment = VerticalAlignment.Center
            }
                    Grid.SetColumn(CalendarIcon, 0)

                    Dim BtnDate As New Button With {
                .Style = TryCast(cashadvancerequest.FindResource("DateButtonStyle"), System.Windows.Style),
                .BorderThickness = New Thickness(0),
                .Cursor = Cursors.Hand,
                .HorizontalContentAlignment = HorizontalAlignment.Center
            }

                    Grid.SetColumn(BtnDate, 1)

                    Dim TxtDateDisplay As New TextBlock With {
                .Height = 14,
                .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
                .FontFamily = TryCast(cashadvancerequest.FindResource("Lexend"), FontFamily),
                .FontWeight = FontWeights.SemiBold
            }

                    BtnDate.Content = TxtDateDisplay

                    dateGrid.Children.Add(RequestAmountDatePicker)
                    dateGrid.Children.Add(CalendarIcon)
                    dateGrid.Children.Add(BtnDate)
                    DateNeededBorder.Child = dateGrid
                    Grid.SetColumn(DateNeededBorder, 1)
                    Inputs_container.Children.Add(DateNeededBorder)

                    ' ===== Reason for Cash Advance =====
                    Dim ReasonCashAdvanceBorder As New Border With {
                .BorderBrush = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
                .Margin = New Thickness(-1, -1, 0, 0),
                .BorderThickness = New Thickness(1, 0, 2, 2)
            }

                    Dim TxtReasonCashAdvance As New TextBox With {
                .FontFamily = TryCast(cashadvancerequest.FindResource("Lexend"), FontFamily),
                .FontSize = 14,
                .TextAlignment = TextAlignment.Center,
                .Padding = New Thickness(0, 10, 0, 10),
                .BorderThickness = New Thickness(0),
                .Text = "",
                .Name = "TxtReasonCashAdvance" & Guid.NewGuid().ToString("N")
            }


                    ReasonCashAdvanceBorder.Child = TxtReasonCashAdvance
                    Grid.SetColumn(ReasonCashAdvanceBorder, 2)
                    Grid.SetColumnSpan(ReasonCashAdvanceBorder, 2)
                    Inputs_container.Children.Add(ReasonCashAdvanceBorder)

                    Dim RemoveBtn As New Button With {
                .Background = New SolidColorBrush(Colors.Transparent),
                .Width = 35,
                .Height = 35,
                .Margin = New Thickness(0, -2, 3, 0),
                .HorizontalAlignment = HorizontalAlignment.Right,
                .Cursor = Cursors.Hand,
                .BorderThickness = New Thickness(0),
                .IsTabStop = False
            }

                    Dim removeIcon As New PackIcon() With {
                .Kind = PackIconKind.PlaylistRemove,
                .Width = 30,
                .Height = 30,
                .Foreground = Brushes.Red
            }

                    Grid.SetColumn(RemoveBtn, 3)
                    RemoveBtn.Content = removeIcon
                    Inputs_container.Children.Add(RemoveBtn)

#End Region

                    ' Create the initial content for the DatePicker and TextBox controls
#Region "Assign Initial Content"
                    RequestAmountDatePicker.DisplayDateStart = Date.Today
                    RequestAmountDatePicker.SelectedDate = Date.Today
                    TxtDateDisplay.Text = RequestAmountDatePicker.SelectedDate?.ToString("MMM dd, yyyy")
                    TxtRequestAmount.CaretIndex = TxtRequestAmount.Text.Length
#End Region

                    ' Add event handlers for the TextBox controls to manage input and formatting
#Region "Event Handlers"
                    Dim KeyHandler As KeyEventHandler = Nothing
                    Dim TextCompositionHandler As TextCompositionEventHandler = Nothing

                    KeyHandler = Sub(s As Object, e As KeyEventArgs)
                                     Dim txtSender As TextBox = CType(s, TextBox)

                                     If txtSender.Name.StartsWith("TxtReasonCashAdvance") Then
                                         ManageRowAdding(e, TxtReasonCashAdvance, TxtRequestAmount)
                                     Else
                                         ManageRowDeletion(e, TxtRequestAmount, TxtReasonCashAdvance, Inputs_container, False)
                                     End If

                                 End Sub
                    TextCompositionHandler = Sub(s As Object, e As TextCompositionEventArgs)
                                                 Dim txtSender As TextBox = CType(s, TextBox)

                                                 If txtSender.Name.StartsWith("TxtRequestAmount") Then
                                                     Dim pattern As String = "^₱?\d{0,9}(\.\d{0,2})?$"
                                                     HandlePreviewInputRegex(s, e, pattern)
                                                 Else
                                                     Dim pattern As String = "^[A-Za-z0-9 ,!?""'\-\.]*$"
                                                     HandlePreviewInputRegex(s, e, pattern)
                                                 End If
                                             End Sub

                    AddHandler TxtReasonCashAdvance.PreviewKeyDown, KeyHandler
                    AddHandler TxtRequestAmount.PreviewKeyDown, KeyHandler
                    AddHandler TxtReasonCashAdvance.PreviewTextInput, TextCompositionHandler
                    AddHandler TxtRequestAmount.PreviewTextInput, TextCompositionHandler

                    AddHandler RemoveBtn.Click,
                                 Sub(s As Object, e As RoutedEventArgs)
                                     ManageRowDeletion(Nothing, TxtRequestAmount, TxtReasonCashAdvance, Inputs_container, True)
                                 End Sub

                    AddHandler BtnDate.Click,
                        Sub(s, e)
                            RequestAmountDatePicker.IsDropDownOpen = True
                        End Sub


                    AddHandler RequestAmountDatePicker.SelectedDateChanged,
                        Sub(s, e)
                            TxtDateDisplay.Text = RequestAmountDatePicker.SelectedDate?.ToString("MMM dd, yyyy")
                        End Sub

                    AddHandler TxtRequestAmount.TextChanged,
            Sub(s As Object, e As TextChangedEventArgs)
                RequestAmountFormatting(s, e, TxtRequestAmount)
                TotalRequestAmount()
            End Sub


#End Region

                    ' Add the new row to the main container and the RequestAmountRows list
#Region "Insertion to main container"
                    RequestAmountRows.Add(Inputs_container)
                    cashadvancerequest.SPRequestAmount.Children.Add(Inputs_container)
#End Region

                End If
            End If

        End Sub

        ' This function handles the input validation for TextBox controls using regular expressions.
        Public Sub HandlePreviewInputRegex(s As Object, e As TextCompositionEventArgs, pattern As String)
            Dim textbox = TryCast(s, TextBox)
            If textbox Is Nothing Then Return

            Dim fullText = textbox.Text.Insert(textbox.SelectionStart, e.Text)

            If pattern = "^₱?\d{0,9}(\.\d{0,2})?$" Then
                fullText = fullText.Replace("₱", "").Replace(",", "").Trim()
            End If

            Dim regex = New Text.RegularExpressions.Regex(pattern)
            e.Handled = Not regex.IsMatch(fullText)
        End Sub


        ' This function formats the amount in the TextBox to a currency format after a delay, allowing for debouncing to prevent excessive formatting while typing.
        Public Async Sub RequestAmountFormatting(sender As Object, e As TextChangedEventArgs, txt As TextBox)
            If debouceCts IsNot Nothing Then
                debouceCts.Cancel()
                debouceCts.Dispose()
            End If

            debouceCts = New CancellationTokenSource()
            Try
                Await Task.Delay(1500, debouceCts.Token)

                Dim caretPos = txt.CaretIndex
                Dim originalText = txt.Text
                Dim unformatted = originalText.Replace("₱", "").Replace(",", "").Trim()

                Dim digitCountBeforeCaret = CountDigitsBeforeCaret(originalText, caretPos)

                If Decimal.TryParse(unformatted, Nothing) Then
                    Dim value As Decimal = CDec(unformatted)

                    If value <= 0 Then
                        txt.Text = "₱ 0.00"
                        txt.CaretIndex = txt.Text.Length
                    ElseIf value > 0 Then
                        txt.Text = "₱ " & value.ToString("N2")

                        ' Restore caret based on digit count before caret
                        txt.CaretIndex = FindCaretIndexAfterFormatting(txt.Text, digitCountBeforeCaret)
                    Else
                        txt.Text = "₱ 0.00"
                        txt.CaretIndex = txt.Text.Length
                    End If

                    If isFormatting = False Then
                        isFormatting = True
                    Else
                        isFormatting = False
                    End If

                End If
            Catch ex As TaskCanceledException
                ' Ignore, user kept typing
            End Try
        End Sub


        ' This function counts the number of digits in the TextBox before the caret position.
        Private Function CountDigitsBeforeCaret(text As String, caret As Integer) As Integer
            Dim count As Integer = 0
            For i = 0 To Math.Min(caret - 1, text.Length - 1)
                If Char.IsDigit(text(i)) Then count += 1
            Next
            Return count
        End Function

        ' This function finds the caret index after formatting the text to ensure that the caret remains in a logical position after the currency formatting is applied.
        Private Function FindCaretIndexAfterFormatting(formattedText As String, digitCount As Integer) As Integer
            Dim count As Integer = 0
            For i = 0 To formattedText.Length - 1
                If Char.IsDigit(formattedText(i)) Then count += 1
                If count = digitCount Then Return i + 1
            Next
            Return formattedText.Length
        End Function


        ' This function retrieves employee information based on the provided name and populates the corresponding TextBox controls with the employee ID, department, and job title.
        Public Sub FindEmployeeInfo(Name As String, EmployeeID As TextBox, Department As TextBox, JobTitle As TextBox)
            Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString()
            Try
                Using conn As New MySqlConnection(connStr)
                    conn.Open()
                    Dim cmd As New MySqlCommand("SELECT EmployeeID, UserRoleID, Department FROM employee WHERE Name = @name", conn)
                    cmd.Parameters.AddWithValue("@name", Name.Trim())
                    Dim UserRoleID As String = String.Empty
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.Read() Then
                            EmployeeID.Text = reader("EmployeeID").ToString()
                            UserRoleID = reader("UserRoleID").ToString()
                            Department.Text = reader("Department").ToString()
                        End If
                    End Using

                    Dim FindRoleCmd As New MySqlCommand("SELECT RoleName FROM userroles WHERE RoleID = @roleID", conn)
                    FindRoleCmd.Parameters.AddWithValue("@roleID", UserRoleID)
                    Using readerFind As MySqlDataReader = FindRoleCmd.ExecuteReader()
                        If readerFind.Read() Then
                            JobTitle.Text = readerFind("RoleName").ToString()
                        End If
                    End Using

                End Using
            Catch ex As Exception
                MessageBox.Show("Error loading employee info: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' This function retrieves the cash advance request entries as a JSON string, iterating through each row in the RequestAmountRows and extracting the relevant data.
        Public Function GetEntryAsJson()
            Dim entries As New List(Of DPC.Data.Models.CashAdvanceEntry)

            For Each child As UIElement In cashadvancerequest.SPRequestAmount.Children
                If TypeOf child Is Grid Then
                    Dim grid As Grid = CType(child, Grid)

                    Dim rowData As New CashAdvanceEntry()

                    For Each element In grid.Children
                        ' Request Amount
                        If TypeOf element Is Border AndAlso Grid.GetColumn(element) = 0 Then
                            Dim border = CType(element, Border)
                            If TypeOf border.Child Is TextBox Then
                                Dim txt = CType(border.Child, TextBox)
                                rowData.Amount = txt.Text.Trim()
                            End If
                        End If

                        ' Date Needed
                        If TypeOf element Is Border AndAlso Grid.GetColumn(element) = 1 Then
                            Dim dateGrid = TryCast(CType(element, Border).Child, Grid)
                            If dateGrid IsNot Nothing Then
                                For Each subEl In dateGrid.Children
                                    If TypeOf subEl Is DatePicker Then
                                        Dim dp = CType(subEl, DatePicker)
                                        rowData.DateRequested = dp.SelectedDate?.ToString("MMM dd, yyyy")
                                    End If
                                Next
                            End If
                        End If

                        ' Reason for Cash Advance
                        If TypeOf element Is Border AndAlso Grid.GetColumn(element) = 2 Then
                            Dim border = CType(element, Border)
                            If TypeOf border.Child Is TextBox Then
                                Dim txt = CType(border.Child, TextBox)
                                rowData.Reason = txt.Text.Trim()
                            End If
                        End If

                    Next

                    entries.Add(rowData)
                End If
            Next

            Return JsonConvert.SerializeObject(entries, Formatting.Indented)
        End Function

        ' This function generates a unique cash advance reference number based on the employee ID and the current date, ensuring that it follows a specific format.
        Public Function GenerateCashAdvanceRef(EmployeeID As String) As String
            Dim prefix As String = "CAR-" & EmployeeID & "-"
            Dim datePart As String = DateTime.Now.ToString("MMddyyyy") ' MMDDYYYY format
            Dim counter As Integer = GetNextCashAdvanceRef(datePart)

            Dim counterPart As String = counter.ToString("D4") ' e.g., 0001
            Return prefix & datePart & "-" & counterPart
        End Function


        ' This function retrieves the next cash advance reference number by querying the database for the maximum existing reference number that matches the specified date part.
        Public Shared Function GetNextCashAdvanceRef(datePart As String) As Integer
            Dim nextCashAdvanceID As Integer = 1
            Try
                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection()
                    conn.Open()

                    Dim query As String = "SELECT MAX(CAST(RIGHT(caRef, 4) AS UNSIGNED)) FROM cashadvance " &
                                  "WHERE caRef LIKE @prefix"

                    Using cmd As New MySqlCommand(query, conn)
                        ' Use parameter to safely insert prefix like 'QUO06182025%'
                        cmd.Parameters.AddWithValue("@prefix", "%-" & datePart & "-%")

                        Dim result = cmd.ExecuteScalar()
                        If result IsNot DBNull.Value AndAlso result IsNot Nothing Then
                            nextCashAdvanceID = Convert.ToInt32(result) + 1
                        End If
                    End Using
                End Using
            Catch ex As Exception
                Console.WriteLine("Error in GetNextCashAdvanceID: " & ex.Message)
            End Try

            Return nextCashAdvanceID
        End Function


        ' This function inserts a new cash advance request into the database, using parameters to prevent SQL injection and ensure data integrity.
        Public Function InsertRequestToDb(CaRef As String,
                                     EmpId As String,
                                     caDate As String,
                                     Supervisor As String,
                                     Rate As String,
                                     requestInfo As String,
                                     requestTotal As String,
                                     requestBy As String,
                                     requestDate As String) As Boolean

            Dim connStr As String = SplashScreen.GetDatabaseConnection().ConnectionString()
            Try
                Using conn As New MySqlConnection(connStr)
                    conn.Open()
                    Dim cmd As New MySqlCommand("INSERT INTO cashadvance (caRef, employeeID, caDate, Supervisor, Rate, RequestInfo, RequestTotal, Remarks, requestedBy, requestDate, approvedBy, approvalDate) " &
                                                "VALUES (@CaRef, @EmpId, @CaDate, @Supervisor, @Rate, @RequestInfo, @RequestTotal, NULL, @RequestBy, @RequestDate, NULL, NULL)", conn)
                    cmd.Parameters.AddWithValue("@CaRef", CaRef)
                    cmd.Parameters.AddWithValue("@EmpId", EmpId)
                    cmd.Parameters.AddWithValue("@CaDate", caDate)
                    cmd.Parameters.AddWithValue("@Supervisor", Supervisor)
                    cmd.Parameters.AddWithValue("@Rate", Rate)
                    cmd.Parameters.AddWithValue("@RequestInfo", requestInfo)
                    cmd.Parameters.AddWithValue("@RequestTotal", requestTotal)
                    cmd.Parameters.AddWithValue("@RequestBy", requestBy)
                    cmd.Parameters.AddWithValue("@RequestDate", requestDate)
                    cmd.ExecuteNonQuery()
                    Return True
                End Using
            Catch ex As Exception
                MessageBox.Show("Error inserting cash advance request: " & ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function



    End Class

End Namespace

