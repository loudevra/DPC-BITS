' ────────────────────────────────────────────────────────────────────────────
' FILE 1: EmployeeProfileService.vb  (new file — add anywhere in your project)
' A simple static holder so the selected employee survives navigation.
' ────────────────────────────────────────────────────────────────────────────
' EmployeeProfileService.vb
Namespace DPC.Data.Helpers
    Public Module EmployeeProfileService
        Public SelectedEmployee As Object   ' temporary — replace once namespace confirmed
    End Module
End Namespace


' ────────────────────────────────────────────────────────────────────────────
' FILE 2: EmployeesView.xaml.vb  — replace ONLY the ViewEmployee Sub
' (everything else stays exactly the same)
' ────────────────────────────────────────────────────────────────────────────
'
'   Private Sub ViewEmployee(sender As Object, e As RoutedEventArgs)
'       ' Get the row the button belongs to
'       Dim btn = TryCast(sender, Button)
'       Dim row = TryCast(btn?.DataContext, Employee)
'
'       ' Fall back to DataGrid.SelectedItem if DataContext isn't set
'       Dim selectedEmployee As Employee =
'           If(row IsNot Nothing, row, TryCast(EmployeesDataGrid.SelectedItem, Employee))
'
'       If selectedEmployee Is Nothing Then
'           MessageBox.Show("Please select an employee first.",
'                           "No Selection", MessageBoxButton.OK, MessageBoxImage.Information)
'           Return
'       End If
'
'       ' Fetch full employee info (same pattern as EditEmployee_Click)
'       Dim fullEmployee = EmployeeController.GetEmployeeInfo(selectedEmployee.EmployeeID)
'
'       ' Store it so EmployeesProfileView can read it after navigation
'       EmployeeProfileService.SelectedEmployee = fullEmployee
'
'       ' Navigate to the profile view
'       ViewLoader.DynamicView.NavigateToView("employeeprofile", Me)
'   End Sub


' ────────────────────────────────────────────────────────────────────────────
' FILE 3: EmployeesProfileView.xaml.vb  — handle the Loaded event
' Add this inside the class so the view populates itself on arrival.
' ────────────────────────────────────────────────────────────────────────────
'
'   Private Sub EmployeesProfileView_Loaded(sender As Object, e As RoutedEventArgs) _
'       Handles Me.Loaded
'       LoadEmployee(EmployeeProfileService.SelectedEmployee)
'   End Sub


' ────────────────────────────────────────────────────────────────────────────
' SUMMARY OF ALL CHANGES
' ────────────────────────────────────────────────────────────────────────────
'
'  1. Add  EmployeeProfileService.vb  (new module) — shared state carrier.
'
'  2. In EmployeesView.xaml.vb:
'     • Replace the existing ViewEmployee Sub with the one above.
'       Key changes:
'         – Reads DataContext from the clicked Button (works even when the
'           row isn't selected before clicking).
'         – Calls EmployeeController.GetEmployeeInfo() to get the full record.
'         – Stores it in EmployeeProfileService.SelectedEmployee.
'         – Calls NavigateToView("employeeprofile", Me).
'           *** Make sure your ViewLoader maps "employeeprofile"
'               to EmployeesProfileView ***
'
'  3. In EmployeesProfileView.xaml.vb:
'     • Add the Loaded handler above; it reads EmployeeProfileService and
'       calls LoadEmployee() which fills every named TextBlock in the XAML.
'
'  4. Replace EmployeesProfileView.xaml with the updated file provided —
'     it adds named TextBlocks (txtEmployeeName, txtProfileEmail, etc.)
'     so LoadEmployee() has elements to write into.
'
'  No changes are needed in AddEmployee.xaml.vb — when an employee is
'  successfully added, ViewLoader.DynamicView.NavigateToView("viewemployee")
'  already refreshes the list, which is correct behaviour.