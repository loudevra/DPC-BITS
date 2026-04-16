Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports DPC.Data.Controllers
Imports DPC.DPC.Data.Controllers
Imports DPC.DPC.Views.HRM.Employees.Holidays
Imports DPC.Views.HRM.Employees.Holidays

Namespace DPC.Components.Forms
    Public Class AddHoliday
        Inherits UserControl

        ' The live list from the parent view
        Public Property ParentHolidayList As System.Collections.ObjectModel.ObservableCollection(Of HolidayModel)

        ' Reference to the parent view so we can call RefreshData() after save
        Public Property ParentView As DPC.Views.HRM.Employees.Holidays.EmployeeHolidays

        ' Holds the item being edited (Nothing = Add mode)
        Private _editingItem As HolidayModel = Nothing

        Public Sub New()
            InitializeComponent()
            AddHandler BtnClose.Click, AddressOf BtnClose_Click
        End Sub

        ' ── Close popup ────────────────────────────────────────────────────────
        Private Sub BtnClose_Click(sender As Object, e As RoutedEventArgs)
            Dim parent = TryCast(Me.Parent, ContentControl)
            If parent IsNot Nothing Then
                Dim container = TryCast(parent.Parent, Panel)
                If container IsNot Nothing Then
                    container.Children.Remove(parent)
                End If
            Else
                Dim parentPopup = TryCast(Me.Parent, Popup)
                If parentPopup IsNot Nothing Then
                    parentPopup.IsOpen = False
                Else
                    Dim parentWindow = Window.GetWindow(Me)
                    If parentWindow IsNot Nothing Then
                        parentWindow.Close()
                    End If
                End If
            End If
        End Sub

        ' ── Add / Update button ────────────────────────────────────────────────
        Private Sub BtnAdd_Click(sender As Object, e As RoutedEventArgs)
            ' Validation
            If Not dpFromHidden.SelectedDate.HasValue OrElse Not dpToHidden.SelectedDate.HasValue Then
                MessageBox.Show("Please select dates.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim startD As Date = dpFromHidden.SelectedDate.Value
            Dim endD As Date = dpToHidden.SelectedDate.Value

            If endD < startD Then
                MessageBox.Show("The 'To' date cannot be earlier than the 'From' date.", "Invalid Range", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim daysCount As Integer = (endD - startD).Days + 1
            Dim noteText As String = TxtNote.Text.Trim()

            If _editingItem IsNot Nothing Then
                ' ── UPDATE MODE ──────────────────────────────────────────────
                If HolidayController.UpdateHoliday(_editingItem.HolidayID, startD, endD, daysCount, noteText) Then
                    MessageBox.Show("Holiday updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    ' Reload the list from DB so the table is in sync
                    If ParentView IsNot Nothing Then
                        ParentView.RefreshData()
                    End If
                    BtnClose_Click(Nothing, Nothing)
                End If
            Else
                ' ── INSERT MODE ──────────────────────────────────────────────
                If HolidayController.InsertHoliday(startD, endD, daysCount, noteText) Then
                    MessageBox.Show("Holiday added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                    ' Reload the list from DB so the new row (with real DB ID) appears
                    If ParentView IsNot Nothing Then
                        ParentView.RefreshData()
                    End If
                    BtnClose_Click(Nothing, Nothing)
                End If
            End If
        End Sub

        ' ── Open FROM calendar ─────────────────────────────────────────────────
        Private Sub BtnFrom_Click(sender As Object, e As RoutedEventArgs)
            Dim minDate As DateTime = DateTime.Today
            If dpFromHidden.SelectedDate.HasValue AndAlso dpFromHidden.SelectedDate.Value < DateTime.Today Then
                minDate = dpFromHidden.SelectedDate.Value
            End If
            dpFromHidden.DisplayDateStart = minDate
            dpFromHidden.IsDropDownOpen = True
        End Sub

        ' ── Open TO calendar ───────────────────────────────────────────────────
        Private Sub BtnTo_Click(sender As Object, e As RoutedEventArgs)
            Dim minDate As DateTime = DateTime.Today
            If dpToHidden.SelectedDate.HasValue AndAlso dpToHidden.SelectedDate.Value < DateTime.Today Then
                minDate = dpToHidden.SelectedDate.Value
            End If
            dpToHidden.DisplayDateStart = minDate
            dpToHidden.IsDropDownOpen = True
        End Sub

        ' ── Called by parent when opening in Edit mode ─────────────────────────
        Public Sub PrepareForEdit(item As HolidayModel)
            _editingItem = item

            TxtNote.Text = item.Note

            Dim parsedFrom As DateTime
            If DateTime.TryParse(item.FromDate, parsedFrom) Then
                dpFromHidden.SelectedDate = parsedFrom
            End If

            Dim parsedTo As DateTime
            If DateTime.TryParse(item.ToDate, parsedTo) Then
                dpToHidden.SelectedDate = parsedTo
            End If

            If TxtBtnAdd IsNot Nothing Then
                TxtBtnAdd.Text = "UPDATE"
            End If
        End Sub

    End Class
End Namespace