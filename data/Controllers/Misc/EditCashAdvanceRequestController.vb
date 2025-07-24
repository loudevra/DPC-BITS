Imports DPC.DPC.Data.Helpers.ViewLoader
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Views.Misc.CashAdvance
Imports MaterialDesignThemes.Wpf
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports OpenTK.Graphics.OpenGL
Imports System.Threading

Namespace DPC.Data.Controllers.Misc
    Public Class EditCashAdvanceRequestController

        Private isFormatting As Boolean = False
        Private debouceCts As CancellationTokenSource
        Dim checkInput As Boolean
        Dim addRowSuccess As Boolean
        Private editcashadvancerequest As EditCashAdvanceRequest
        Private managecashadvancerequests As ManageCashAdvanceRequests
        Public RequestAmountRows As New List(Of Grid)
        Dim MaxNum As Integer = 8

        Public Sub New(formInstance As EditCashAdvanceRequest)
            editcashadvancerequest = formInstance
        End Sub

        'Converts JSON string to a list of EditCashAdvanceJsonToList objects
        Public Function ConvertJSONToList(jsonString As String) As List(Of EditCashAdvanceJsonToList)
            Dim entries As List(Of EditCashAdvanceJsonToList) = JsonConvert.DeserializeObject(Of List(Of EditCashAdvanceJsonToList))(jsonString)
            Return entries
        End Function

        ' Checks if any row in the RequestAmountRows has an empty TextBox
        Public Function HasAnyRowWithEmptyTextBox() As Boolean

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

        ' Manages the addition of a new row when the Tab key is pressed in the Reason TextBox
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

        ' Manages the deletion of a row when the Backspace key is pressed or when the remove button is clicked
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
                                editcashadvancerequest.SPRequestAmount.Children.Remove(Inputcon)
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
                        editcashadvancerequest.SPRequestAmount.Children.Remove(Inputcon)
                        RequestAmountRows.Remove(Inputcon)
                    End If
                End If
            End If
        End Sub


        ' Calculates the total request amount from all rows in the RequestAmountRows
        Private Sub TotalRequestAmount()
            Dim requestAmounts As New List(Of String)

            For Each row As Grid In editcashadvancerequest.SPRequestAmount.Children
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
                editcashadvancerequest.TbTotalRequestAmount.Text = "₱ " & totalAmount.ToString("N2")
            Else
                editcashadvancerequest.TbTotalRequestAmount.Text = "₱ 0.00"
            End If

        End Sub


        ' Adds a new row to the request amount section with optional parameters for amount, request date, and reason
        Public Sub AddAmountRequestRow(Optional Amount As String = Nothing, Optional ReqDate As Date = Nothing, Optional Reason As String = Nothing)

            ' Check if any row has empty TextBox before adding a new row
            If HasAnyRowWithEmptyTextBox() Then
                MessageBox.Show("all columns must be filled correctly.")
                addRowSuccess = False
            Else
                ' Check if the maximum number of rows has been reached
                If RequestAmountRows.Count >= MaxNum Then
                    MessageBox.Show("You can only add up to " & MaxNum & " rows.")
                    Return
                Else
                    ' Create a new row only if the previous row is filled correctly
                    addRowSuccess = True

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
                .FontFamily = TryCast(editcashadvancerequest.FindResource("Lexend"), FontFamily),
                .FontSize = 14,
                .TextAlignment = TextAlignment.Center,
                .Padding = New Thickness(0, 10, 0, 10),
                .BorderThickness = New Thickness(0),
                .Text = If(String.IsNullOrWhiteSpace(Amount), "₱ 0.00", Amount),
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
                .Style = TryCast(editcashadvancerequest.FindResource("SingleDatePickerStyle"), Style),
                .FontFamily = TryCast(editcashadvancerequest.FindResource("Lexend"), FontFamily),
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
                .Style = TryCast(editcashadvancerequest.FindResource("DateButtonStyle"), Style),
                .BorderThickness = New Thickness(0),
                .Cursor = Cursors.Hand,
                .HorizontalContentAlignment = HorizontalAlignment.Center
            }

                    Grid.SetColumn(BtnDate, 1)

                    Dim TxtDateDisplay As New TextBlock With {
                .Height = 14,
                .Foreground = New SolidColorBrush(ColorConverter.ConvertFromString("#474747")),
                .FontFamily = TryCast(editcashadvancerequest.FindResource("Lexend"), FontFamily),
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
                .FontFamily = TryCast(editcashadvancerequest.FindResource("Lexend"), FontFamily),
                .FontSize = 14,
                .TextAlignment = TextAlignment.Center,
                .Padding = New Thickness(0, 10, 0, 10),
                .BorderThickness = New Thickness(0),
                .Text = If(String.IsNullOrWhiteSpace(Reason), "", Reason),
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


#Region "Assign Initial Content"
                    ' Assign initial values to the controls
                    RequestAmountDatePicker.DisplayDateStart = If(ReqDate = Nothing, Date.Today, ReqDate)
                    RequestAmountDatePicker.SelectedDate = If(ReqDate = Nothing, Date.Today, ReqDate)
                    TxtDateDisplay.Text = RequestAmountDatePicker.SelectedDate?.ToString("MMM dd, yyyy")
                    TxtRequestAmount.CaretIndex = TxtRequestAmount.Text.Length
#End Region

#Region "Event Handlers"
                    ' Add event handlers for key presses and text input validation
                    Dim KeyHandler As KeyEventHandler = Nothing
                    Dim TextCompositionHandler As TextCompositionEventHandler = Nothing

                    ' Key and Text Composition Handlers
                    KeyHandler = Sub(s As Object, e As KeyEventArgs)
                                     Dim txtSender As TextBox = CType(s, TextBox)

                                     ' Check if the sender is TxtRequestAmount or TxtReasonCashAdvance
                                     If txtSender.Name.StartsWith("TxtReasonCashAdvance") Then
                                         ManageRowAdding(e, TxtReasonCashAdvance, TxtRequestAmount)
                                     Else
                                         ManageRowDeletion(e, TxtRequestAmount, TxtReasonCashAdvance, Inputs_container, False)
                                     End If

                                 End Sub

                    TextCompositionHandler = Sub(s As Object, e As TextCompositionEventArgs)
                                                 Dim txtSender As TextBox = CType(s, TextBox)

                                                 ' Check if the sender is TxtRequestAmount or TxtReasonCashAdvance
                                                 If txtSender.Name.StartsWith("TxtRequestAmount") Then
                                                     Dim pattern As String = "^₱?\d{0,9}(\.\d{0,2})?$"
                                                     HandlePreviewInputRegex(s, e, pattern)
                                                 Else
                                                     Dim pattern As String = "^[A-Za-z0-9 ,!?""'\-\.]*$"
                                                     HandlePreviewInputRegex(s, e, pattern)
                                                 End If
                                             End Sub

                    ' Attach the handlers to the TextBox controls
                    AddHandler TxtReasonCashAdvance.PreviewKeyDown, KeyHandler
                    AddHandler TxtRequestAmount.PreviewKeyDown, KeyHandler
                    AddHandler TxtReasonCashAdvance.PreviewTextInput, TextCompositionHandler
                    AddHandler TxtRequestAmount.PreviewTextInput, TextCompositionHandler

                    ' Add handlers for the Remove button and DatePicker
                    AddHandler RemoveBtn.Click,
                             Sub(s As Object, e As RoutedEventArgs)
                                 ManageRowDeletion(Nothing, TxtRequestAmount, TxtReasonCashAdvance, Inputs_container, True)
                             End Sub

                    ' Add handler for the DatePicker button to open the date picker
                    AddHandler BtnDate.Click,
                    Sub(s, e)
                        RequestAmountDatePicker.IsDropDownOpen = True
                    End Sub

                    ' Add handler for the DatePicker's SelectedDateChanged event to update the display text
                    AddHandler RequestAmountDatePicker.SelectedDateChanged,
                    Sub(s, e)
                        TxtDateDisplay.Text = RequestAmountDatePicker.SelectedDate?.ToString("MMM dd, yyyy")
                    End Sub

                    ' Add handler for the TextBox's TextChanged event to format the request amount and update total
                    AddHandler TxtRequestAmount.TextChanged,
        Sub(s As Object, e As TextChangedEventArgs)

            ' Format the request amount and update the total
            RequestAmountFormatting(s, e, TxtRequestAmount)
            TotalRequestAmount()
        End Sub


#End Region

#Region "Insertion to main container"
                    ' Add the new row to the main container
                    RequestAmountRows.Add(Inputs_container)
                    editcashadvancerequest.SPRequestAmount.Children.Add(Inputs_container)
#End Region

                End If
            End If

        End Sub


        ' Handles the input validation for TextBox controls using regex patterns
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


        ' Formats the request amount in the TextBox after a delay to allow for typing
        Public Async Sub RequestAmountFormatting(sender As Object, e As TextChangedEventArgs, txt As TextBox)
            If debouceCts IsNot Nothing Then
                debouceCts.Cancel()
                debouceCts.Dispose()
            End If

            ' Start a new cancellation token source for debouncing
            debouceCts = New CancellationTokenSource()
            Try
                ' Wait for a short delay to allow for typing
                Await Task.Delay(1500, debouceCts.Token)

                ' Check if the TextBox is still focused and the text has changed
                Dim caretPos = txt.CaretIndex
                Dim originalText = txt.Text
                Dim unformatted = originalText.Replace("₱", "").Replace(",", "").Trim()

                ' If the text is empty, reset to default value
                Dim digitCountBeforeCaret = CountDigitsBeforeCaret(originalText, caretPos)

                ' If the text is empty or contains only the currency symbol, set it to default
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


        ' Counts the number of digits before the caret position in the TextBox
        Private Function CountDigitsBeforeCaret(text As String, caret As Integer) As Integer
            Dim count As Integer = 0
            For i = 0 To Math.Min(caret - 1, text.Length - 1)
                If Char.IsDigit(text(i)) Then count += 1
            Next
            Return count
        End Function


        ' Finds the caret index after formatting the text based on the digit count
        Private Function FindCaretIndexAfterFormatting(formattedText As String, digitCount As Integer) As Integer
            Dim count As Integer = 0
            For i = 0 To formattedText.Length - 1
                If Char.IsDigit(formattedText(i)) Then count += 1
                If count = digitCount Then Return i + 1
            Next
            Return formattedText.Length
        End Function


        ' Finds employee information based on the provided name and updates the corresponding TextBoxes
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


        ' Gets the entries from the request amount section and converts them to JSON format
        Public Function GetEntryAsJson()
            Dim entries As New List(Of DPC.Data.Models.CashAdvanceEntry)

            For Each child As UIElement In editcashadvancerequest.SPRequestAmount.Children
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

        ' Updates the cash advance request in the database with the provided parameters and returns a boolean indicating success or failure
        Public Function UpdatetoDB(CaRef As String, EmployeeID As TextBox, EmployeeName As TextBox, CashDate As CalendarController.SingleCalendar, Supervisor As TextBox, Rate As TextBox, RequestTotal As TextBlock, Remarks As TextBox, requestedBy As TextBox, requestDate As CalendarController.SingleCalendar, approvedBy As ComboBox, approvalDate As CalendarController.SingleCalendar, Approval As Integer) As Boolean

            ' Validate inputs before proceeding with the update
            If HasAnyRowWithEmptyTextBox() Or String.IsNullOrWhiteSpace(EmployeeName.Text) Or String.IsNullOrWhiteSpace(requestedBy.Text) Then
                MessageBox.Show("All inputs in the request must be filled out.", "Information", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            Else
                ' Prepare the data for the update
                Dim EmployID As String = EmployeeID.Text.Trim().ToString()
                Dim caDate As String = CashDate.SelectedDate.Value.ToString("yyyy-MM-dd")
                Dim supvisor As String = If(String.IsNullOrWhiteSpace(Supervisor.Text), "", Supervisor.Text.Trim().ToString())
                Dim CARate As String = If(String.IsNullOrWhiteSpace(Rate.Text), "", Rate.Text.Trim().ToString())
                Dim CARrequestInfo As String = GetEntryAsJson()
                Dim CARrequestTotal As String = RequestTotal.Text.Trim().ToString()
                Dim requestBy As String = requestedBy.Text.Trim().ToString()
                Dim reqDate As String = requestDate.SelectedDate.Value.ToString("yyyy-MM-dd")

                Dim CARremarks As String = Nothing
                Dim CARapprovalDate As String = Nothing
                Dim CARapprovedBy As String = Nothing


                ' Check the approval status and set the appropriate values
                If Approval = 2 Then
                    ' If the request is being saved without approval
                    Dim result As MessageBoxResult = MessageBox.Show(
                        "Please note that this request will need approval. The 'Approved By' as well as 'Remarks' field will be ignored upon saving.",
                        "Information",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Information
                    )
                    If Not result = MessageBoxResult.OK Then
                        Return False
                    End If



                ElseIf Approval = 1 Then
                    ' Check if an approver is selected
                    Dim selectedItem = TryCast(approvedBy.SelectedItem, ComboBoxItem)

                    If selectedItem IsNot Nothing Then
                        If selectedItem.Content.ToString() <> "None" Then
                            Dim result As MessageBoxResult = MessageBox.Show(
                                "This is to confirm that you approve this request.",
                                "Information",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Information
                            )
                            If result = MessageBoxResult.Yes Then
                                CARapprovedBy = selectedItem.Content.ToString()
                                CARremarks = If(String.IsNullOrWhiteSpace(Remarks.Text), "", Remarks.Text.Trim().ToString())
                                CARapprovalDate = approvalDate.SelectedDate.Value.ToString("yyyy-MM-dd")
                            Else
                                Return False
                            End If
                        Else
                            MessageBox.Show("Approver is not valid.", "Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                            Return False
                        End If
                    Else
                        MessageBox.Show("Selecting an approver is a must.", "Information", MessageBoxButton.OK, MessageBoxImage.Warning)
                        Return False
                    End If


                ElseIf Approval = 0 Then
                    ' If the request is being rejected
                    Dim result As MessageBoxResult = MessageBox.Show(
                        "This is to confirm that you reject this request.",
                        "Information",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information
                        )
                    If result = MessageBoxResult.Yes Then
                        CARapprovedBy = "Rejected"
                        CARapprovalDate = Date.Today.ToString("yyyy-MM-dd")
                        CARremarks = If(String.IsNullOrWhiteSpace(Remarks.Text), "", Remarks.Text.Trim().ToString())
                    Else
                        Return False
                    End If
                End If

                ' ' Proceed with the update query
                If UpdateQuery(CaRef, EmployID, caDate, supvisor, CARate, CARrequestInfo, CARrequestTotal, CARremarks, requestBy, reqDate, CARapprovedBy, CARapprovalDate) Then
                    If Approval = 0 Then
                        MessageBox.Show("Request has been rejected!", "Reject", MessageBoxButton.OK, MessageBoxImage.Information)
                    ElseIf Approval = 1 Then
                        MessageBox.Show("Request has been approved!", "Approve", MessageBoxButton.OK, MessageBoxImage.Information)
                    Else
                        MessageBox.Show("Request has been saved!", "Update", MessageBoxButton.OK, MessageBoxImage.Information)
                    End If

                    Return True
                Else
                    MessageBox.Show("Failed to update request.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return False
                End If
            End If
        End Function


        ' Updates the cash advance request in the database with the provided parameters and returns a boolean indicating success or failure
        Public Function UpdateQuery(CaRef As String, EmployeeID As String, CashDate As String, Supervisor As String, Rate As String, RequestInfo As String, RequestTotal As String, Remarks As String, requestedBy As String, requestDate As String, approvedBy As String, approvalDate As String) As Boolean
            Try
                Dim query As String = "UPDATE cashadvance 
                                       SET employeeID = @employeeID, 
                                           caDate = @caDate, 
                                           Supervisor = @supervisor, 
                                           Rate = @rate, 
                                           RequestInfo = @requestInfo, 
                                           RequestTotal = @requestTotal, 
                                           Remarks = @remarks, 
                                           requestedBy = @requestedBy, 
                                           requestDate = @requestDate, 
                                           approvedBy = @approvedBy, 
                                           approvalDate = @approvalDate 
                                       WHERE caRef = @caRef"

                Using conn As MySqlConnection = SplashScreen.GetDatabaseConnection
                    conn.Open()
                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@caRef", CaRef)
                        cmd.Parameters.AddWithValue("@employeeID", EmployeeID)
                        cmd.Parameters.AddWithValue("@caDate", CashDate)
                        cmd.Parameters.AddWithValue("@supervisor", Supervisor)
                        cmd.Parameters.AddWithValue("@rate", Rate)
                        cmd.Parameters.AddWithValue("@requestInfo", RequestInfo)
                        cmd.Parameters.AddWithValue("@requestTotal", RequestTotal)
                        cmd.Parameters.AddWithValue("@remarks", Remarks)
                        cmd.Parameters.AddWithValue("@requestedBy", requestedBy)
                        cmd.Parameters.AddWithValue("@requestDate", requestDate)
                        cmd.Parameters.AddWithValue("@approvedBy", approvedBy)
                        cmd.Parameters.AddWithValue("@approvalDate", approvalDate)
                        Dim rowsAffected As Integer = cmd.ExecuteNonQuery()
                        If rowsAffected > 0 Then
                            MessageBox.Show("Request updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                        Else
                            MessageBox.Show("No rows were updated. Please check the request ID.", "Information", MessageBoxButton.OK, MessageBoxImage.Information)
                        End If
                    End Using
                End Using
                Return True
            Catch ex As Exception
                MessageBox.Show("Failed to Update request.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return False
            End Try
        End Function
    End Class

End Namespace
