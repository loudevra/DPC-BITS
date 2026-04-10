Imports System.ComponentModel

Public Class PermissionItem
    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler _
        Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    ' --- ID and Name ---
    Private _id As Integer
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

    Private _name As String
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

    ' --- Original 6 Roles ---
    Private _hasInventoryManager As Boolean
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

    Private _hasSalesPerson As Boolean
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

    Private _hasSalesManager As Boolean
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

    Private _hasBusinessManager As Boolean
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

    Private _hasBusinessOwner As Boolean
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

    Private _hasProjectManager As Boolean
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

    ' --- 3 New Roles ---
    Private _hasAdministrator As Boolean
    Public Property HasAdministrator As Boolean
        Get
            Return _hasAdministrator
        End Get
        Set(value As Boolean)
            If _hasAdministrator <> value Then
                _hasAdministrator = value
                OnPropertyChanged("HasAdministrator")
            End If
        End Set
    End Property

    Private _hasIT As Boolean
    Public Property HasIT As Boolean

        Get
            Return _hasIT
        End Get
        Set(value As Boolean)
            If _hasIT <> value Then
                _hasIT = value
                OnPropertyChanged("HasIT")
            End If
        End Set
    End Property

    Private _hasTech As Boolean
    Public Property HasTech As Boolean
        Get
            Return _hasTech
        End Get
        Set(value As Boolean)
            If _hasTech <> value Then
                _hasTech = value
                OnPropertyChanged("HasTech")
            End If
        End Set
    End Property

End Class

