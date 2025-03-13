Imports System.ComponentModel

Namespace DPC.Data.Model
    Public Class Employee
        Implements INotifyPropertyChanged

        Public Property Id As Integer
        Public Property Username As String
        Public Property BusinessLocation As String
        Public Property Name As String
        Public Property Role As String

        ' Boolean Properties for Role-Based Checkboxes
        Private _isInventoryManager As Boolean
        Public Property IsInventoryManager As Boolean
            Get
                Return _isInventoryManager
            End Get
            Set(value As Boolean)
                _isInventoryManager = value
                OnPropertyChanged(NameOf(IsInventoryManager))
            End Set
        End Property

        Private _isSalesPerson As Boolean
        Public Property IsSalesPerson As Boolean
            Get
                Return _isSalesPerson
            End Get
            Set(value As Boolean)
                _isSalesPerson = value
                OnPropertyChanged(NameOf(IsSalesPerson))
            End Set
        End Property

        Private _isSalesManager As Boolean
        Public Property IsSalesManager As Boolean
            Get
                Return _isSalesManager
            End Get
            Set(value As Boolean)
                _isSalesManager = value
                OnPropertyChanged(NameOf(IsSalesManager))
            End Set
        End Property

        Private _isBusinessManager As Boolean
        Public Property IsBusinessManager As Boolean
            Get
                Return _isBusinessManager
            End Get
            Set(value As Boolean)
                _isBusinessManager = value
                OnPropertyChanged(NameOf(IsBusinessManager))
            End Set
        End Property

        Private _isBusinessOwner As Boolean
        Public Property IsBusinessOwner As Boolean
            Get
                Return _isBusinessOwner
            End Get
            Set(value As Boolean)
                _isBusinessOwner = value
                OnPropertyChanged(NameOf(IsBusinessOwner))
            End Set
        End Property

        Private _isProjectManager As Boolean
        Public Property IsProjectManager As Boolean
            Get
                Return _isProjectManager
            End Get
            Set(value As Boolean)
                _isProjectManager = value
                OnPropertyChanged(NameOf(IsProjectManager))
            End Set
        End Property

        ' Implement INotifyPropertyChanged
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Protected Sub OnPropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub

        Public Property Email As String
        Public Property Password As String
        Public Property Address As String
        Public Property City As String
        Public Property Region As String
        Public Property Country As String
        Public Property PostBox As String
        Public Property Phone As String
        Public Property Salary As Decimal
        Public Property SalesCommission As Decimal
        Public Property Department As String
        Public Property Status As String
    End Class
End Namespace
