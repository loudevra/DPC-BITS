Imports MySql.Data.MySqlClient
Imports System.Collections.ObjectModel
Imports DPC.DPC.Data.Model
Imports System.Web
Imports System.Windows.Controls.Primitives
Imports System.Data
Imports DPC.DPC.Data.Models
Imports DPC.DPC.Data.Helpers
Imports System.ComponentModel

Namespace DPC.Data.Controllers
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

        Public Property Id As Integer
            Get
                Return _id
            End Get
            Set(value As Integer)
                _id = value
                OnPropertyChanged("Id")
            End Set
        End Property

        Public Property Name As String
            Get
                Return _name
            End Get
            Set(value As String)
                _name = value
                OnPropertyChanged("Name")
            End Set
        End Property

        Public Property HasInventoryManager As Boolean
            Get
                Return _hasInventoryManager
            End Get
            Set(value As Boolean)
                _hasInventoryManager = value
                OnPropertyChanged("HasInventoryManager")
            End Set
        End Property

        Public Property HasSalesPerson As Boolean
            Get
                Return _hasSalesPerson
            End Get
            Set(value As Boolean)
                _hasSalesPerson = value
                OnPropertyChanged("HasSalesPerson")
            End Set
        End Property

        Public Property HasSalesManager As Boolean
            Get
                Return _hasSalesManager
            End Get
            Set(value As Boolean)
                _hasSalesManager = value
                OnPropertyChanged("HasSalesManager")
            End Set
        End Property

        Public Property HasBusinessManager As Boolean
            Get
                Return _hasBusinessManager
            End Get
            Set(value As Boolean)
                _hasBusinessManager = value
                OnPropertyChanged("HasBusinessManager")
            End Set
        End Property

        Public Property HasBusinessOwner As Boolean
            Get
                Return _hasBusinessOwner
            End Get
            Set(value As Boolean)
                _hasBusinessOwner = value
                OnPropertyChanged("HasBusinessOwner")
            End Set
        End Property

        Public Property HasProjectManager As Boolean
            Get
                Return _hasProjectManager
            End Get
            Set(value As Boolean)
                _hasProjectManager = value
                OnPropertyChanged("HasProjectManager")
            End Set
        End Property

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Protected Sub OnPropertyChanged(propertyName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub
    End Class
End Namespace

