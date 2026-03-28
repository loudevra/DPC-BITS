Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports DPC.DPC.Views.HRM.Employees.Holidays
Imports DPC.Views.HRM.Employees.Holidays ' Ensures HolidayModel is accessible

Namespace DPC.Components.Forms
    Public Class AddHoliday
        Inherits UserControl

        ' 1. This receives the list from your main table
        Public Property ParentHolidayList As System.Collections.ObjectModel.ObservableCollection(Of HolidayModel)

        ' Keeps track of which row we are currently editing
        Private _editingItem As HolidayModel = Nothing

        Public Sub New()
            InitializeComponent()
            AddHandler BtnClose.Click, AddressOf BtnClose_Click
        End Sub

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
        Private Sub BtnAdd_Click(sender As Object, e As RoutedEventArgs)
            ' 1. Validation
            If Not dpFromHidden.SelectedDate.HasValue OrElse Not dpToHidden.SelectedDate.HasValue Then
                MessageBox.Show("Please select dates.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim startD = dpFromHidden.SelectedDate.Value
            Dim endD = dpToHidden.SelectedDate.Value
            Dim daysCount = (endD - startD).Days + 1

            ' 2. Check if we are UPDATING or ADDING
            If _editingItem IsNot Nothing Then
                ' --- UPDATE MODE (THE FIX) ---

                ' Find exactly where the old item is in the master list
                Dim index As Integer = ParentHolidayList.IndexOf(_editingItem)

                If index >= 0 Then
                    ' Create a brand NEW item with the fresh data from the form
                    Dim updatedHoliday As New HolidayModel With {
                .ID = _editingItem.ID,
                .FromDate = startD.ToString("MM/dd/yyyy"),
                .ToDate = endD.ToString("MM/dd/yyyy"),
                .Days = daysCount,
                .Note = TxtNote.Text,
                .Action = _editingItem.Action
            }

                    ' SWAP them! This forces the ObservableCollection to instantly update the UI
                    ParentHolidayList(index) = updatedHoliday

                    MessageBox.Show("Holiday updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If

            Else
                ' --- ADD MODE ---
                Dim newHoliday As New HolidayModel With {
            .ID = ParentHolidayList.Count + 1,
            .FromDate = startD.ToString("MM/dd/yyyy"),
            .ToDate = endD.ToString("MM/dd/yyyy"),
            .Days = daysCount,
            .Note = TxtNote.Text,
            .Action = "Edit/Delete"
        }
                ParentHolidayList.Add(newHoliday)

                MessageBox.Show("Holiday added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
            End If

            ' 3. Close the popup
            BtnClose_Click(Nothing, Nothing)
        End Sub

        ' FORCE THE "FROM" CALENDAR TO OPEN
        Private Sub BtnFrom_Click(sender As Object, e As RoutedEventArgs)
            dpFromHidden.IsDropDownOpen = True
        End Sub

        ' FORCE THE "TO" CALENDAR TO OPEN
        Private Sub BtnTo_Click(sender As Object, e As RoutedEventArgs)
            dpToHidden.IsDropDownOpen = True
        End Sub

        Public Sub PrepareForEdit(item As HolidayModel)
            _editingItem = item

            ' Fill the fields
            TxtNote.Text = item.Note

            ' --- SAFELY PARSE FROM DATE ---
            Dim parsedFromDate As DateTime
            ' TryParse will attempt to convert the string. If it succeeds, it puts the value in parsedFromDate.
            If DateTime.TryParse(item.FromDate, parsedFromDate) Then
                dpFromHidden.SelectedDate = parsedFromDate
            End If

            ' --- SAFELY PARSE TO DATE ---
            Dim parsedToDate As DateTime
            If DateTime.TryParse(item.ToDate, parsedToDate) Then
                dpToHidden.SelectedDate = parsedToDate
            End If

            ' Update the TextBlock inside the button safely
            If TxtBtnAdd IsNot Nothing Then
                TxtBtnAdd.Text = "UPDATE"
            End If
        End Sub
    End Class
End Namespace