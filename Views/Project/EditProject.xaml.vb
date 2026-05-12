Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports Microsoft.Win32
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Project
    Partial Public Class EditProject
        Inherits UserControl

        Private dateViewModel As New CalendarController.SingleCalendar()
        Private preBidDateViewModel As New CalendarController.SingleCalendar()
        Private closingDateViewModel As New CalendarController.SingleCalendar()
        Private receiveDateViewModel As New CalendarController.SingleCalendar()

        Public Sub New()
            InitializeComponent()
            SetupDatePickers()
            AddHandler Me.Loaded, AddressOf EditProject_Loaded
        End Sub

        Private Sub SetupDatePickers()
            DatePicker.DataContext = dateViewModel
            DateButton.DataContext = dateViewModel
            PreBidDatePicker.DataContext = preBidDateViewModel
            PreBidDateButton.DataContext = preBidDateViewModel
            ClosingDatePicker.DataContext = closingDateViewModel
            ClosingDateButton.DataContext = closingDateViewModel
            ReceiveDatePicker.DataContext = receiveDateViewModel
            ReceiveDateButton.DataContext = receiveDateViewModel
        End Sub

        Private Sub EditProject_Loaded(sender As Object, e As RoutedEventArgs)
            Try
                Dim p = DPC.Views.Project.ManageProject.SelectedProject
                If p Is Nothing Then Return

                txtProjectTitle.Text = If(p.ProjectTitle, "")
                txtReferenceNumber.Text = If(p.ReferenceNumber, "")
                txtContactPerson.Text = If(p.ContactPerson, "")
                txtContactNumber.Text = If(p.ContactNumber, "")
                txtEmailAddress.Text = If(p.EmailAddress, "")
                txtAreaOfDelivery.Text = If(p.AreaOfDelivery, "")
                txtABC.Text = If(p.ABC > 0, p.ABC.ToString("N0"), "")
                txtBidRFQOffer.Text = If(p.BidRFQOffer > 0, p.BidRFQOffer.ToString("N0"), "")

                If p.ProjectDate.HasValue Then DatePicker.SelectedDate = p.ProjectDate.Value
                If p.PreBidDate.HasValue Then PreBidDatePicker.SelectedDate = p.PreBidDate.Value
                If p.ClosingDate.HasValue Then ClosingDatePicker.SelectedDate = p.ClosingDate.Value
                If p.ReceiveDate.HasValue Then ReceiveDatePicker.SelectedDate = p.ReceiveDate.Value

                SelectComboByContent(cmbCategory, p.Category)
                SelectComboByContent(cmbProjectType, p.ProjectType)
                SelectComboByContent(cmbModeOfSubmission, p.ModeOfSubmission)
                SelectComboByContent(cmbStatus, p.Status)
                SelectComboByContent(cmbRemarks, p.Remarks)
                SelectComboByContent(cmbAssignSales, p.AssignSales)

                ' ── Restore ProjectList radio button ──
                Select Case p.ProjectList
                    Case "AWARDED_PROJECTS"
                        radListAwarded.IsChecked = True
                    Case "COLLECTION"
                        radListCollection.IsChecked = True
                    Case Else
                        ' DPC_GOV_SALES is the default — leave both unchecked
                        ' (or add a radListDPCGov radio and check it here)
                End Select

                If p.IsAwarded Then
                    radAwardedYes.IsChecked = True
                Else
                    radAwardedNo.IsChecked = True
                End If

                If Not String.IsNullOrWhiteSpace(p.Note) Then
                    EditorBox.Document.Blocks.Clear()
                    EditorBox.Document.Blocks.Add(New Paragraph(New Run(p.Note)))
                End If

            Catch ex As Exception
                MessageBox.Show("Error loading project details: " & ex.Message, "Load Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub SelectComboByContent(cmb As ComboBox, value As String)
            If String.IsNullOrWhiteSpace(value) Then Return
            For Each item As ComboBoxItem In cmb.Items
                If item.Content.ToString().Equals(value, StringComparison.OrdinalIgnoreCase) Then
                    cmb.SelectedItem = item
                    Return
                End If
            Next
        End Sub

        ' =========================================================
        ' PROJECT LIST HELPER
        ' Reads the radListAwarded / radListCollection / default
        ' radio buttons and returns the matching DB list key.
        ' =========================================================
        Private Function GetSelectedProjectList() As String
            If radListAwarded.IsChecked = True Then
                Return "AWARDED_PROJECTS"
            ElseIf radListCollection.IsChecked = True Then
                Return "COLLECTION"
            Else
                Return "DPC_GOV_SALES"   ' default when neither is checked
            End If
        End Function

        ' =========================================================
        ' TO-UPPER HANDLER
        ' =========================================================
        Private Sub TxtToUpper_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return
            Dim caretPos As Integer = tb.SelectionStart
            Dim upper As String = tb.Text.ToUpperInvariant()
            If Not String.Equals(tb.Text, upper, StringComparison.Ordinal) Then
                RemoveHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
                tb.Text = upper
                tb.SelectionStart = Math.Min(caretPos, tb.Text.Length)
                AddHandler tb.TextChanged, AddressOf TxtToUpper_TextChanged
            End If
        End Sub

        ' =========================================================
        ' NUMBER ONLY
        ' =========================================================
        Private Sub txtZipCode_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
            If Not Char.IsDigit(e.Text, e.Text.Length - 1) Then
                e.Handled = True
            End If
        End Sub

        ' =========================================================
        ' BUDGET / NUMERIC FORMATTER
        ' =========================================================
        Private Sub txtBudget_TextChanged(sender As Object, e As TextChangedEventArgs)
            Dim tb = TryCast(sender, TextBox)
            If tb Is Nothing Then Return
            RemoveHandler tb.TextChanged, AddressOf txtBudget_TextChanged
            Dim rawText As String = tb.Text.Replace(",", "").Trim()
            Dim number As Long
            If rawText = "" Then
                tb.Text = ""
            ElseIf Long.TryParse(rawText, number) Then
                Dim formatted As String = number.ToString("N0")
                Dim caretOffset As Integer = tb.Text.Length - tb.CaretIndex
                tb.Text = formatted
                tb.CaretIndex = Math.Max(0, formatted.Length - caretOffset)
            Else
                If tb.Text.Length > 0 Then
                    tb.Text = tb.Text.Remove(tb.Text.Length - 1)
                    tb.CaretIndex = tb.Text.Length
                End If
            End If
            AddHandler tb.TextChanged, AddressOf txtBudget_TextChanged
        End Sub

        ' =========================================================
        ' DATE PICKER CLICK HANDLERS
        ' =========================================================
        Private Sub DateButton_Click(sender As Object, e As RoutedEventArgs)
            DatePicker.DisplayDateStart = DateTime.Today
            DatePicker.IsDropDownOpen = True
        End Sub

        Private Sub PreBidDateButton_Click(sender As Object, e As RoutedEventArgs)
            PreBidDatePicker.DisplayDateStart = DateTime.Today
            PreBidDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub ClosingDateButton_Click(sender As Object, e As RoutedEventArgs)
            ClosingDatePicker.DisplayDateStart = DateTime.Today
            ClosingDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub ReceiveDateButton_Click(sender As Object, e As RoutedEventArgs)
            ReceiveDatePicker.DisplayDateStart = DateTime.Today
            ReceiveDatePicker.IsDropDownOpen = True
        End Sub

        ' =========================================================
        ' DATE PICKER SELECTION CHANGED HANDLERS
        ' =========================================================
        Private Sub DatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
            SyncDateViewModel(sender, dateViewModel, DateButton)
        End Sub

        Private Sub PreBidDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
            SyncDateViewModel(sender, preBidDateViewModel, PreBidDateButton)
        End Sub

        Private Sub ClosingDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
            SyncDateViewModel(sender, closingDateViewModel, ClosingDateButton)
        End Sub

        Private Sub ReceiveDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs)
            SyncDateViewModel(sender, receiveDateViewModel, ReceiveDateButton)
        End Sub

        Private Sub SyncDateViewModel(sender As Object, vm As CalendarController.SingleCalendar, btn As Button)
            Dim dp = TryCast(sender, DatePicker)
            If dp Is Nothing OrElse dp.DataContext Is Nothing Then Return
            vm.SelectedDate = dp.SelectedDate
            Dim be = BindingOperations.GetBindingExpression(btn, Button.DataContextProperty)
            If be IsNot Nothing Then be.UpdateTarget()
        End Sub

        ' =========================================================
        ' SAVE / UPDATE
        ' =========================================================
        Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrWhiteSpace(txtProjectTitle.Text) Then
                MessageBox.Show("Project Title is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Try
                Dim connStr As String = DPC.SplashScreen.GetDatabaseConnection().ConnectionString
                Using conn As New MySqlConnection(connStr)
                    conn.Open()

                    Dim selectedStatus As String = GetComboText(cmbStatus)
                    Dim selectedCategory As String = GetComboText(cmbCategory)
                    Dim selectedProjectType As String = GetComboText(cmbProjectType)
                    Dim selectedMode As String = GetComboText(cmbModeOfSubmission)
                    Dim selectedRemarks As String = GetComboText(cmbRemarks)
                    Dim selectedAssignSales As String = GetComboText(cmbAssignSales)

                    ' ── NEW: resolve which project list was selected ──
                    Dim projectList As String = GetSelectedProjectList()

                    Dim noteText As String = New TextRange(EditorBox.Document.ContentStart, EditorBox.Document.ContentEnd).Text.Trim()
                    Dim rawABC As Long
                    Long.TryParse(txtABC.Text.Replace(",", ""), rawABC)
                    Dim rawBidOffer As Long
                    Long.TryParse(txtBidRFQOffer.Text.Replace(",", ""), rawBidOffer)

                    Dim query As String =
                        "UPDATE project SET ProjectTitle=@name, Status=@status, Category=@category, " &
                        "ProjectType=@projtype, ContactPerson=@contact, ContactNumber=@contactnum, " &
                        "EmailAddress=@email, AreaOfDelivery=@area, PreBidDate=@prebid, " &
                        "ClosingDate=@closing, ABC=@abc, BidRFQOffer=@bid, ReceiveDate=@receive, " &
                        "ModeOfSubmission=@mode, Remarks=@remarks, AssignSales=@sales, " &
                        "ProjectList=@projectList, " &
                        "Note=@note, ProjectDate=@projdate, " &
                        "ReferenceNumber=@refnum WHERE projectID=@id"

                    Using cmd As New MySqlCommand(query, conn)
                        cmd.Parameters.AddWithValue("@name", txtProjectTitle.Text)
                        cmd.Parameters.AddWithValue("@status", If(selectedStatus, ""))
                        cmd.Parameters.AddWithValue("@category", If(selectedCategory, ""))
                        cmd.Parameters.AddWithValue("@projtype", If(selectedProjectType, ""))
                        cmd.Parameters.AddWithValue("@contact", txtContactPerson.Text)
                        cmd.Parameters.AddWithValue("@contactnum", txtContactNumber.Text)
                        cmd.Parameters.AddWithValue("@email", txtEmailAddress.Text)
                        cmd.Parameters.AddWithValue("@area", txtAreaOfDelivery.Text)
                        cmd.Parameters.AddWithValue("@prebid", If(PreBidDatePicker.SelectedDate, DBNull.Value))
                        cmd.Parameters.AddWithValue("@closing", If(ClosingDatePicker.SelectedDate, DBNull.Value))
                        cmd.Parameters.AddWithValue("@abc", rawABC)
                        cmd.Parameters.AddWithValue("@bid", rawBidOffer)
                        cmd.Parameters.AddWithValue("@receive", If(ReceiveDatePicker.SelectedDate, DBNull.Value))
                        cmd.Parameters.AddWithValue("@mode", If(selectedMode, ""))
                        cmd.Parameters.AddWithValue("@remarks", If(selectedRemarks, ""))
                        cmd.Parameters.AddWithValue("@sales", If(selectedAssignSales, ""))
                        cmd.Parameters.AddWithValue("@projectList", projectList)   ' ── NEW ──
                        cmd.Parameters.AddWithValue("@note", noteText)
                        cmd.Parameters.AddWithValue("@projdate", If(DatePicker.SelectedDate, DBNull.Value))
                        cmd.Parameters.AddWithValue("@refnum", txtReferenceNumber.Text)
                        cmd.Parameters.AddWithValue("@id", DPC.Views.Project.ManageProject.SelectedProject.ProjectID)
                        cmd.ExecuteNonQuery()
                    End Using
                End Using

                MessageBox.Show("Project updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                ViewLoader.DynamicView.NavigateToView("manageproject", Me)

            Catch ex As Exception
                MessageBox.Show("Update failed: " & ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Function GetComboText(cmb As ComboBox) As String
            If cmb.SelectedItem Is Nothing Then Return Nothing
            Dim item = TryCast(cmb.SelectedItem, ComboBoxItem)
            If item IsNot Nothing Then Return item.Content?.ToString()
            Return cmb.SelectedItem.ToString()
        End Function

        ' =========================================================
        ' RICH TEXT EDITOR FORMATTING
        ' =========================================================
        Private Sub Format_Bold_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.ToggleBold.Execute(Nothing, EditorBox)
        End Sub
        Private Sub Format_Italic_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.ToggleItalic.Execute(Nothing, EditorBox)
        End Sub
        Private Sub Format_Underline_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.ToggleUnderline.Execute(Nothing, EditorBox)
        End Sub
        Private Sub Format_Strike_Click(sender As Object, e As RoutedEventArgs)
            Dim selection = EditorBox.Selection
            If Not selection.IsEmpty Then
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough)
            End If
        End Sub
        Private Sub Format_Subscript_Click(sender As Object, e As RoutedEventArgs)
            EditorBox.Selection.ApplyPropertyValue(Typography.VariantsProperty, FontVariants.Subscript)
        End Sub
        Private Sub Format_Superscript_Click(sender As Object, e As RoutedEventArgs)
            EditorBox.Selection.ApplyPropertyValue(Typography.VariantsProperty, FontVariants.Superscript)
        End Sub
        Private Sub Format_TextColor_Click(sender As Object, e As RoutedEventArgs)
            EditorBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.RoyalBlue)
        End Sub
        Private Sub Format_Highlight_Click(sender As Object, e As RoutedEventArgs)
            EditorBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow)
        End Sub
        Private Sub Format_Clear_Click(sender As Object, e As RoutedEventArgs)
            EditorBox.Selection.ClearAllProperties()
        End Sub
        Private Sub FontSize_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If EditorBox Is Nothing OrElse cmbFontSize.SelectedItem Is Nothing Then Return
            Dim item As ComboBoxItem = TryCast(cmbFontSize.SelectedItem, ComboBoxItem)
            If item IsNot Nothing Then
                Dim size As Double
                If Double.TryParse(item.Content.ToString(), size) Then
                    EditorBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, size)
                End If
            End If
        End Sub
        Private Sub Format_AlignLeft_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.AlignLeft.Execute(Nothing, EditorBox)
        End Sub
        Private Sub Format_AlignCenter_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.AlignCenter.Execute(Nothing, EditorBox)
        End Sub
        Private Sub Format_AlignRight_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.AlignRight.Execute(Nothing, EditorBox)
        End Sub
        Private Sub Format_AlignJustify_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.AlignJustify.Execute(Nothing, EditorBox)
        End Sub
        Private Sub Format_List_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.ToggleBullets.Execute(Nothing, EditorBox)
        End Sub
        Private Sub Format_IndentInc_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.IncreaseIndentation.Execute(Nothing, EditorBox)
        End Sub
        Private Sub Format_IndentDec_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.DecreaseIndentation.Execute(Nothing, EditorBox)
        End Sub
        Private Sub Insert_Quote_Click(sender As Object, e As RoutedEventArgs)
            EditingCommands.IncreaseIndentation.Execute(Nothing, EditorBox)
            EditingCommands.ToggleItalic.Execute(Nothing, EditorBox)
        End Sub
        Private Sub Insert_Link_Click(sender As Object, e As RoutedEventArgs)
            Dim url As String = Microsoft.VisualBasic.Interaction.InputBox("Enter the URL:", "Insert Link", "http://")
            If String.IsNullOrWhiteSpace(url) Then Return
            If Not url.StartsWith("http") Then url = "http://" & url
            Dim link As New Hyperlink(New Run(url))
            link.NavigateUri = New Uri(url)
            AddHandler link.RequestNavigate, AddressOf Hyperlink_RequestNavigate
            If Not EditorBox.Selection.IsEmpty Then
                Dim selectedText As String = EditorBox.Selection.Text
                EditorBox.Selection.Text = ""
                Dim newLink As New Hyperlink(New Run(selectedText))
                newLink.NavigateUri = New Uri(url)
                AddHandler newLink.RequestNavigate, AddressOf Hyperlink_RequestNavigate
                InsertLinkAtCaret(newLink)
            Else
                InsertLinkAtCaret(link)
            End If
        End Sub
        Private Sub InsertLinkAtCaret(link As Hyperlink)
            If EditorBox.CaretPosition.Paragraph IsNot Nothing Then
                EditorBox.CaretPosition.Paragraph.Inlines.Add(link)
            Else
                EditorBox.Document.Blocks.Add(New Paragraph(link))
            End If
        End Sub
        Private Sub Hyperlink_RequestNavigate(sender As Object, e As RequestNavigateEventArgs)
            System.Diagnostics.Process.Start(New System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) With {.UseShellExecute = True})
            e.Handled = True
        End Sub
        Private Sub Insert_Image_Click(sender As Object, e As RoutedEventArgs)
            Dim dlg As New OpenFileDialog()
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            If dlg.ShowDialog() = True Then
                Try
                    Dim bitmap As New BitmapImage()
                    bitmap.BeginInit()
                    bitmap.UriSource = New Uri(dlg.FileName)
                    bitmap.CacheOption = BitmapCacheOption.OnLoad
                    bitmap.EndInit()
                    Dim img As New Image()
                    img.Source = bitmap
                    img.Width = 300
                    img.Stretch = Stretch.Uniform
                    Dim container As New InlineUIContainer(img, EditorBox.CaretPosition)
                Catch ex As Exception
                    MessageBox.Show("Unable to    insert this image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub
        Private Sub Insert_Code_Click(sender As Object, e As RoutedEventArgs)
            Dim range As New TextRange(EditorBox.Selection.Start, EditorBox.Selection.End)
            range.ApplyPropertyValue(TextElement.FontFamilyProperty, New FontFamily("Consolas"))
            range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.LightGray)
            range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black)
        End Sub
        Private isFullscreen As Boolean = False
        Private Sub Toggle_Fullscreen_Click(sender As Object, e As RoutedEventArgs)
            isFullscreen = Not isFullscreen
            EditorBox.Height = If(isFullscreen, 600, 250)
            EditorBox.VerticalScrollBarVisibility = If(isFullscreen, ScrollBarVisibility.Visible, ScrollBarVisibility.Auto)
        End Sub

    End Class
End Namespace
