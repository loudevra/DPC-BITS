Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Helpers
Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Namespace DPC.Data.Controllers
    Public Class ProductViewModel
        Implements INotifyPropertyChanged

        Private Shared _instance As ProductViewModel

        ' Singleton pattern to ensure we have one instance across the application
        Public Shared ReadOnly Property Instance As ProductViewModel
            Get
                If _instance Is Nothing Then
                    _instance = New ProductViewModel()
                End If
                Return _instance
            End Get
        End Property

        Private _productName As String
        Public Property ProductName As String
            Get
                Return _productName
            End Get
            Set(value As String)
                If _productName <> value Then
                    _productName = value
                    OnPropertyChanged()
                End If
            End Set
        End Property

        Private _description As String
        Public Property Description As String
            Get
                Return _description
            End Get
            Set(value As String)
                If _description <> value Then
                    _description = value
                    OnPropertyChanged()
                End If
            End Set
        End Property

        ' Add other product properties as needed

        ' INotifyPropertyChanged implementation
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Protected Overridable Sub OnPropertyChanged(<CallerMemberName> Optional propertyName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub
    End Class
End Namespace

