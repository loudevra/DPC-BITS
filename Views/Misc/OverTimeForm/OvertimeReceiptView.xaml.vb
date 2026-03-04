Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Media

Namespace DPC.Views.Misc.OverTime

    Public Class OvertimeReceiptView
        Inherits UserControl

        ' Shared variable to receive the selected record from Manage
        Public Shared TargetRecord As OvertimeRequestModel

        Public Sub New()
            InitializeComponent()
            LoadReceipt()
        End Sub

        Private Sub LoadReceipt()
            If TargetRecord Is Nothing Then Return

            ' Fill all the fields with the selected record's data
            TxtOvertimeID.Text = TargetRecord.OvertimeID
            TxtEmployeeName.Text = TargetRecord.EmployeeName
            TxtEmployeeID.Text = TargetRecord.EmployeeID
            TxtJobTitle.Text = TargetRecord.JobTitle
            TxtDepartment.Text = TargetRecord.Department
            TxtSupervisor.Text = TargetRecord.Supervisor
            TxtStartTime.Text = TargetRecord.StartTime
            TxtEndTime.Text = TargetRecord.EndTime
            TxtTotalHours.Text = TargetRecord.TotalHours & " hrs"
            TxtReason.Text = TargetRecord.Reason
            TxtRemarks.Text = TargetRecord.Remarks
            TxtRequestedBy.Text = TargetRecord.RequestedBy
            TxtRequestDate.Text = TargetRecord.RequestDate

            ' Color the status
            TxtStatus.Text = TargetRecord.Status
            Select Case TargetRecord.Status
                Case "Approved"
                    TxtStatus.Foreground = New SolidColorBrush(Colors.Green)
                Case "Pending"
                    TxtStatus.Foreground = New SolidColorBrush(Colors.Orange)
                Case "Rejected"
                    TxtStatus.Foreground = New SolidColorBrush(Colors.Red)
            End Select
        End Sub

        Private Sub BtnBack_Click(sender As Object, e As RoutedEventArgs)
            DPC.Data.Helpers.ViewLoader.DynamicView.NavigateToView("manageovertimerequests", Me)
        End Sub

        Private Sub BtnPrint_Click(sender As Object, e As RoutedEventArgs)
            Dim printDialog As New PrintDialog()
            If printDialog.ShowDialog() = True Then
                printDialog.PrintVisual(Me, "Overtime Request Receipt - " & TargetRecord.OvertimeID)
            End If
        End Sub

    End Class
End Namespace