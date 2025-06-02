Imports System.ComponentModel

Public Class PermissionItem
    Implements INotifyPropertyChanged

    Private _id As Integer
    Private _name As String
    Private _hasInventoryManager As Boolean
    Private _hasSalesPerson As Boolean
    Private _hasSalesManager As Boolean
    Private _hasBusinessManager As Boolean
    Private _hasBusinessOwner As Boolean
    Private _hasProjectManager As Boolean

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Public Property Id As Integer
        Get
            Return _id
        End Get
        Set(value As Integer)
            If _id <> value Then
                _id = value
                OnPropertyChanged("Id")
            End If
        End Set
    End Property

    Public Property Name As String
        Get
            Return _name
        End Get
        Set(value As String)
            If _name <> value Then
                _name = value
                OnPropertyChanged("Name")
            End If
        End Set
    End Property

    Public Property HasInventoryManager As Boolean
        Get
            Return _hasInventoryManager
        End Get
        Set(value As Boolean)
            If _hasInventoryManager <> value Then
                _hasInventoryManager = value
                OnPropertyChanged("HasInventoryManager")
            End If
        End Set
    End Property

    Public Property HasSalesPerson As Boolean
        Get
            Return _hasSalesPerson
        End Get
        Set(value As Boolean)
            If _hasSalesPerson <> value Then
                _hasSalesPerson = value
                OnPropertyChanged("HasSalesPerson")
            End If
        End Set
    End Property

    Public Property HasSalesManager As Boolean
        Get
            Return _hasSalesManager
        End Get
        Set(value As Boolean)
            If _hasSalesManager <> value Then
                _hasSalesManager = value
                OnPropertyChanged("HasSalesManager")
            End If
        End Set
    End Property

    Public Property HasBusinessManager As Boolean
        Get
            Return _hasBusinessManager
        End Get
        Set(value As Boolean)
            If _hasBusinessManager <> value Then
                _hasBusinessManager = value
                OnPropertyChanged("HasBusinessManager")
            End If
        End Set
    End Property

    Public Property HasBusinessOwner As Boolean
        Get
            Return _hasBusinessOwner
        End Get
        Set(value As Boolean)
            If _hasBusinessOwner <> value Then
                _hasBusinessOwner = value
                OnPropertyChanged("HasBusinessOwner")
            End If
        End Set
    End Property

    Public Property HasProjectManager As Boolean
        Get
            Return _hasProjectManager
        End Get
        Set(value As Boolean)
            If _hasProjectManager <> value Then
                _hasProjectManager = value
                OnPropertyChanged("HasProjectManager")
            End If
        End Set
    End Property

    Protected Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class