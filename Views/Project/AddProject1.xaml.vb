Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Threading
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Data.Helpers
Imports Microsoft.Win32
Imports MySql.Data.MySqlClient

Namespace DPC.Views.Project
    Partial Public Class AddProject1
        Inherits UserControl

        Private dateViewModel As New CalendarController.SingleCalendar()
        Private preBidDateViewModel As New CalendarController.SingleCalendar()
        Private closingDateViewModel As New CalendarController.SingleCalendar()
        Private receiveDateViewModel As New CalendarController.SingleCalendar()

        ' =========================================================
        ' 1. INTERNAL MEMORY (Keeps data alive across tabs)
        ' =========================================================
        Private Shared _savedDate As DateTime? = Nothing
        Private Shared _savedReferenceNumber As String = ""
        Private Shared _savedProjectTitle As String = ""
        Private Shared _savedCategoryIndex As Integer = -1
        Private Shared _savedProjectTypeIndex As Integer = -1
        Private Shared _savedContactPerson As String = ""
        Private Shared _savedContactNumber As String = ""
        Private Shared _savedEmailAddress As String = ""
        Private Shared _savedAreaOfDelivery As String = ""
        Private Shared _savedPreBidDate As DateTime? = Nothing
        Private Shared _savedClosingDate As DateTime? = Nothing
        Private Shared _savedABC As String = ""
        Private Shared _savedBidRFQOffer As String = ""
        Private Shared _savedReceiveDate As DateTime? = Nothing
        Private Shared _savedModeIndex As Integer = -1
        Private Shared _savedStatusIndex As Integer = -1
        Private Shared _savedRemarksIndex As Integer = -1
        Private Shared _savedAssignSalesIndex As Integer = -1
        Private Shared _savedBidDocsLink As String = ""
        Private Shared _savedNote As String = ""

        Public Sub New()
            InitializeComponent()
            SetupDatePickers()
            RestoreFields()
            WireAutoSaveHandlers()

            AddHandler Me.Loaded, AddressOf AddProject1_Loaded
        End Sub



        ' =========================================================
        ' SETUP DATE PICKERS
        ' =========================================================
        Public Sub SetupDatePickers()
            DatePicker.DataContext = dateViewModel
            DateButton.DataContext = dateViewModel

            PreBidDatePicker.DataContext = preBidDateViewModel
            PreBidDateButton.DataContext = preBidDateViewModel

            ClosingDatePicker.DataContext = closingDateViewModel
            ClosingDateButton.DataContext = closingDateViewModel

            ReceiveDatePicker.DataContext = receiveDateViewModel
            ReceiveDateButton.DataContext = receiveDateViewModel
        End Sub

        ' =========================================================
        ' 2. RESTORE DATA FROM MEMORY
        ' =========================================================
        Private Sub RestoreFields()
            DatePicker.SelectedDate = _savedDate
            dateViewModel.SelectedDate = _savedDate

            txtReferenceNumber.Text = _savedReferenceNumber
            txtProjectTitle.Text = _savedProjectTitle
            cmbCategory.SelectedIndex = _savedCategoryIndex
            cmbProjectType.SelectedIndex = _savedProjectTypeIndex
            txtContactPerson.Text = _savedContactPerson
            txtContactNumber.Text = _savedContactNumber
            txtEmailAddress.Text = _savedEmailAddress
            txtAreaOfDelivery.Text = _savedAreaOfDelivery

            PreBidDatePicker.SelectedDate = _savedPreBidDate
            preBidDateViewModel.SelectedDate = _savedPreBidDate

            ClosingDatePicker.SelectedDate = _savedClosingDate
            closingDateViewModel.SelectedDate = _savedClosingDate

            txtABC.Text = _savedABC
            txtBidRFQOffer.Text = _savedBidRFQOffer

            ReceiveDatePicker.SelectedDate = _savedReceiveDate
            receiveDateViewModel.SelectedDate = _savedReceiveDate

            cmbModeOfSubmission.SelectedIndex = _savedModeIndex
            cmbStatus.SelectedIndex = _savedStatusIndex
            cmbRemarks.SelectedIndex = _savedRemarksIndex
            cmbAssignSales.SelectedIndex = _savedAssignSalesIndex

            If Not String.IsNullOrWhiteSpace(_savedNote) Then
                EditorBox.Document.Blocks.Clear()
                EditorBox.Document.Blocks.Add(New Paragraph(New Run(_savedNote)))
            End If
        End Sub

        ' =========================================================
        ' 3. WIRE AUTO-SAVE HANDLERS (after restoring to avoid overwrite)
        ' =========================================================
        Private Sub WireAutoSaveHandlers()
            AddHandler DatePicker.SelectedDateChanged, AddressOf SaveDateToMemory
            AddHandler txtReferenceNumber.TextChanged, AddressOf SaveTextToMemory
            AddHandler txtProjectTitle.TextChanged, AddressOf SaveTextToMemory
            AddHandler cmbCategory.SelectionChanged, AddressOf SaveComboToMemory
            AddHandler cmbProjectType.SelectionChanged, AddressOf SaveComboToMemory
            AddHandler txtContactPerson.TextChanged, AddressOf SaveTextToMemory
            AddHandler txtContactNumber.TextChanged, AddressOf SaveTextToMemory
            AddHandler txtEmailAddress.TextChanged, AddressOf SaveTextToMemory
            AddHandler txtAreaOfDelivery.TextChanged, AddressOf SaveTextToMemory
            AddHandler PreBidDatePicker.SelectedDateChanged, AddressOf SaveDateToMemory
            AddHandler ClosingDatePicker.SelectedDateChanged, AddressOf SaveDateToMemory
            AddHandler txtABC.TextChanged, AddressOf SaveTextToMemory
            AddHandler txtBidRFQOffer.TextChanged, AddressOf SaveTextToMemory
            AddHandler ReceiveDatePicker.SelectedDateChanged, AddressOf SaveDateToMemory
            AddHandler cmbModeOfSubmission.SelectionChanged, AddressOf SaveComboToMemory
            AddHandler cmbStatus.SelectionChanged, AddressOf SaveComboToMemory
            AddHandler cmbRemarks.SelectionChanged, AddressOf SaveComboToMemory
            AddHandler cmbAssignSales.SelectionChanged, AddressOf SaveComboToMemory
            AddHandler EditorBox.TextChanged, AddressOf SaveEditorToMemory
        End Sub

        ' =========================================================
        ' MEMORY SAVERS
        ' =========================================================
        Private Sub SaveTextToMemory(sender As Object, e As TextChangedEventArgs)
            _savedReferenceNumber = txtReferenceNumber.Text
            _savedProjectTitle = txtProjectTitle.Text
            _savedContactPerson = txtContactPerson.Text
            _savedContactNumber = txtContactNumber.Text
            _savedEmailAddress = txtEmailAddress.Text
            _savedAreaOfDelivery = txtAreaOfDelivery.Text
            _savedABC = txtABC.Text
            _savedBidRFQOffer = txtBidRFQOffer.Text
        End Sub

        Private Sub SaveComboToMemory(sender As Object, e As SelectionChangedEventArgs)
            _savedCategoryIndex = cmbCategory.SelectedIndex
            _savedProjectTypeIndex = cmbProjectType.SelectedIndex
            _savedModeIndex = cmbModeOfSubmission.SelectedIndex
            _savedStatusIndex = cmbStatus.SelectedIndex
            _savedRemarksIndex = cmbRemarks.SelectedIndex
            _savedAssignSalesIndex = cmbAssignSales.SelectedIndex
        End Sub

        Private Sub SaveDateToMemory(sender As Object, e As SelectionChangedEventArgs)
            _savedDate = DatePicker.SelectedDate
            _savedPreBidDate = PreBidDatePicker.SelectedDate
            _savedClosingDate = ClosingDatePicker.SelectedDate
            _savedReceiveDate = ReceiveDatePicker.SelectedDate
        End Sub

        Private Sub SaveEditorToMemory(sender As Object, e As TextChangedEventArgs)
            _savedNote = New TextRange(EditorBox.Document.ContentStart, EditorBox.Document.ContentEnd).Text.Trim()
        End Sub

        ' =========================================================
        ' LOADED — nothing to auto-fill since Assign Sales is now a dropdown
        ' =========================================================
        Private Sub AddProject1_Loaded(sender As Object, e As RoutedEventArgs)
            ' No auto-fill needed; cmbAssignSales is user-selected
        End Sub

        ' =========================================================
        ' TO-UPPER HANDLER (TextBoxes that need uppercase)
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
            ' This only allows digits (0-9)
            ' If the character typed is NOT a digit, we set e.Handled to True to block it
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
        Private Sub DateButton_Click(sender As Object, e As RoutedEventArgs) Handles DateButton.Click
            DatePicker.DisplayDateStart = DateTime.Today
            DatePicker.IsDropDownOpen = True
        End Sub

        Private Sub PreBidDateButton_Click(sender As Object, e As RoutedEventArgs) Handles PreBidDateButton.Click
            PreBidDatePicker.DisplayDateStart = DateTime.Today
            PreBidDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub ClosingDateButton_Click(sender As Object, e As RoutedEventArgs) Handles ClosingDateButton.Click
            ClosingDatePicker.DisplayDateStart = DateTime.Today
            ClosingDatePicker.IsDropDownOpen = True
        End Sub

        Private Sub ReceiveDateButton_Click(sender As Object, e As RoutedEventArgs) Handles ReceiveDateButton.Click
            ReceiveDatePicker.DisplayDateStart = DateTime.Today
            ReceiveDatePicker.IsDropDownOpen = True
        End Sub

        ' =========================================================
        ' DATE PICKER SELECTION CHANGED HANDLERS
        ' =========================================================
        Private Sub DatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles DatePicker.SelectedDateChanged
            SyncDateViewModel(sender, dateViewModel, DateButton)
        End Sub

        Private Sub PreBidDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles PreBidDatePicker.SelectedDateChanged
            SyncDateViewModel(sender, preBidDateViewModel, PreBidDateButton)
        End Sub

        Private Sub ClosingDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles ClosingDatePicker.SelectedDateChanged
            SyncDateViewModel(sender, closingDateViewModel, ClosingDateButton)
        End Sub

        Private Sub ReceiveDatePicker_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles ReceiveDatePicker.SelectedDateChanged
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
        ' SAVE / SUBMIT
        ' =========================================================
        Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
            ' Validation
            If String.IsNullOrWhiteSpace(txtProjectTitle.Text) Then
                MessageBox.Show("Project Title is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' Get selected dropdown text values
            Dim selectedCategory As String = GetComboText(cmbCategory)
            Dim selectedProjectType As String = GetComboText(cmbProjectType)
            Dim selectedMode As String = GetComboText(cmbModeOfSubmission)
            Dim selectedStatus As String = GetComboText(cmbStatus)
            Dim selectedRemarks As String = GetComboText(cmbRemarks)
            Dim selectedAssignSales As String = GetComboText(cmbAssignSales)

            Dim noteText As String = New TextRange(EditorBox.Document.ContentStart, EditorBox.Document.ContentEnd).Text.Trim()

            Dim rawABC As Long
            Long.TryParse(txtABC.Text.Replace(",", ""), rawABC)

            Dim rawBidOffer As Long
            Long.TryParse(txtBidRFQOffer.Text.Replace(",", ""), rawBidOffer)

            Dim proj As New DPC.Data.Model.Project With {
                .ProjectDate = DatePicker.SelectedDate,
                .ReferenceNumber = txtReferenceNumber.Text,
                .ProjectTitle = txtProjectTitle.Text,
                .Category = selectedCategory,
                .ProjectType = selectedProjectType,
                .ContactPerson = txtContactPerson.Text,
                .ContactNumber = txtContactNumber.Text,
                .EmailAddress = txtEmailAddress.Text,
                .AreaOfDelivery = txtAreaOfDelivery.Text,
                .PreBidDate = PreBidDatePicker.SelectedDate,
                .ClosingDate = ClosingDatePicker.SelectedDate,
                .ABC = rawABC,
                .BidRFQOffer = rawBidOffer,
                .ReceiveDate = ReceiveDatePicker.SelectedDate,
                .ModeOfSubmission = selectedMode,
                .Status = selectedStatus,
                .Remarks = selectedRemarks,
                .AssignSales = selectedAssignSales,
                .Note = noteText
            }

            Dim success As Boolean = DPC.Data.Controllers.ProjectController.CreateProject(proj)

            If success Then
                MessageBox.Show("Project added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                ClearFields()
                ViewLoader.DynamicView.NavigateToView("manageproject", Me)
            End If
        End Sub

        ' Helper to safely get ComboBox selected text
        Private Function GetComboText(cmb As ComboBox) As String
            If cmb.SelectedItem Is Nothing Then Return Nothing
            Dim item = TryCast(cmb.SelectedItem, ComboBoxItem)
            If item IsNot Nothing Then Return item.Content?.ToString()
            Return cmb.SelectedItem.ToString()
        End Function

        ' =========================================================
        ' CLEAR ALL FIELDS
        ' =========================================================
        Private Sub ClearFields()
            ' UI
            DatePicker.SelectedDate = Nothing
            txtReferenceNumber.Clear()
            txtProjectTitle.Clear()
            cmbCategory.SelectedIndex = -1
            cmbProjectType.SelectedIndex = -1
            txtContactPerson.Clear()
            txtContactNumber.Clear()
            txtEmailAddress.Clear()
            txtAreaOfDelivery.Clear()
            PreBidDatePicker.SelectedDate = Nothing
            ClosingDatePicker.SelectedDate = Nothing
            txtABC.Clear()
            txtBidRFQOffer.Clear()
            ReceiveDatePicker.SelectedDate = Nothing
            cmbModeOfSubmission.SelectedIndex = -1
            cmbStatus.SelectedIndex = -1
            cmbRemarks.SelectedIndex = -1
            cmbAssignSales.SelectedIndex = -1
            EditorBox.Document.Blocks.Clear()

            ' ViewModels
            dateViewModel.SelectedDate = Nothing
            preBidDateViewModel.SelectedDate = Nothing
            closingDateViewModel.SelectedDate = Nothing
            receiveDateViewModel.SelectedDate = Nothing

            ' Shared memory
            _savedDate = Nothing
            _savedReferenceNumber = ""
            _savedProjectTitle = ""
            _savedCategoryIndex = -1
            _savedProjectTypeIndex = -1
            _savedContactPerson = ""
            _savedContactNumber = ""
            _savedEmailAddress = ""
            _savedAreaOfDelivery = ""
            _savedPreBidDate = Nothing
            _savedClosingDate = Nothing
            _savedABC = ""
            _savedBidRFQOffer = ""
            _savedReceiveDate = Nothing
            _savedModeIndex = -1
            _savedStatusIndex = -1
            _savedRemarksIndex = -1
            _savedAssignSalesIndex = -1
            _savedBidDocsLink = ""
            _savedNote = ""

            txtProjectTitle.Focus()
        End Sub

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
                    MessageBox.Show("Unable to insert this image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
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
