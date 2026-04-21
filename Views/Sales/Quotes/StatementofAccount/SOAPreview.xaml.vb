Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media
Imports DPC.DPC.Data.Models ' <== Ensure this maps correctly to your StatementModel location

Namespace DPC.Views.SOA
    Public Class SOAPreview
        Inherits UserControl

        ' Variable to track if images are currently visible
        Private _imagesVisible As Boolean = True

        ' Default Constructor
        Public Sub New()
            InitializeComponent()
            SetImageColumnVisibility(Visibility.Visible)
        End Sub

        ' Overloaded Constructor that receives Data
        Public Sub New(data As StatementModel)
            InitializeComponent()
            Me.DataContext = data
            SetImageColumnVisibility(Visibility.Visible)
        End Sub

        ' ==========================================
        ' 1. TOGGLE IMAGE FUNCTIONALITY
        ' ==========================================
        Private Sub BtnHideImages_Click(sender As Object, e As RoutedEventArgs)
            ' Flip the boolean
            _imagesVisible = Not _imagesVisible

            If _imagesVisible Then
                SetImageColumnVisibility(Visibility.Visible)
            Else
                SetImageColumnVisibility(Visibility.Collapsed)
            End If
        End Sub

        Private Sub SetImageColumnVisibility(state As Visibility)
            If dgSOAItems IsNot Nothing AndAlso ColImage IsNot Nothing Then
                ColImage.Visibility = state
            End If
        End Sub

        ' ==========================================
        ' 2. OPEN PRINT VIEW FILE ("View" Button)
        ' ==========================================
        Private Sub BtnViewImages_Click(sender As Object, e As RoutedEventArgs)
            ' Find the parent ContentControl (your overlay container)
            Dim parentContent = TryCast(Me.Parent, ContentControl)

            If parentContent IsNot Nothing Then
                ' Grab the current data
                Dim data = TryCast(Me.DataContext, StatementModel)

                ' Create the Print view and pass the data so it populates automatically!
                Dim printLayout As New PreviewPrintStatementOfAccount(data)

                ' Replace the current SOAPreview with the Print Preview inside the overlay
                parentContent.Content = printLayout
            End If
        End Sub

        ' ==========================================
        ' 3. NAVIGATION: BACK TO FORM OVERLAY CLOSE
        ' ==========================================
        Private Sub BtnBack_Click(sender As Object, e As RoutedEventArgs)
            ' This safely closes the overlay without closing the host application Window
            Dim parentContent = TryCast(Me.Parent, ContentControl)
            If parentContent IsNot Nothing Then
                Dim overlayGrid = TryCast(parentContent.Parent, Grid)
                If overlayGrid IsNot Nothing Then
                    ' Hide the dark transparent overlay entirely
                    overlayGrid.Visibility = Visibility.Collapsed

                    ' Free up memory so it creates fresh next time
                    parentContent.Content = Nothing
                End If
            End If
        End Sub

        Private Sub dgPaymentDetails_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles dgPaymentDetails.SelectionChanged

        End Sub
    End Class
End Namespace