Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Windows.Controls
Imports System.Windows
Imports System.Windows.Controls.Primitives
Imports System.Collections.Generic

Namespace DPC.Data.Helpers

    ''' <summary>
    ''' Provides pagination functionality for DataGrid and other collection-based controls.
    ''' </summary>
    Public Class PaginationHelper
        ' Pagination properties
        Private _allItems As ObservableCollection(Of Object)
        Private _currentPage As Integer = 1
        Private _itemsPerPage As Integer = 10
        Private _totalPages As Integer = 1
        Private _maxPageButtons As Integer = 5 ' Maximum number of page buttons to show

        ' UI Elements for pagination
        Private WithEvents BtnPrevPage As Button
        Private WithEvents BtnNextPage As Button
        Private ReadOnly _pageButtons As New List(Of Button)()
        Private ReadOnly _paginationPanel As Panel
        Private ReadOnly _dataGrid As DataGrid
        Private _filterFunc As Predicate(Of Object)

        ''' <summary>
        ''' Gets or sets the collection of all items before pagination.
        ''' </summary>
        Public Property AllItems As ObservableCollection(Of Object)
            Get
                Return _allItems
            End Get
            Set(value As ObservableCollection(Of Object))
                _allItems = value
                UpdatePagination()
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the current page number.
        ''' </summary>
        Public Property CurrentPage As Integer
            Get
                Return _currentPage
            End Get
            Set(value As Integer)
                If value <> _currentPage AndAlso value >= 1 AndAlso value <= _totalPages Then
                    _currentPage = value
                    UpdatePagination()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the number of items to display per page.
        ''' </summary>
        Public Property ItemsPerPage As Integer
            Get
                Return _itemsPerPage
            End Get
            Set(value As Integer)
                If value > 0 AndAlso value <> _itemsPerPage Then
                    ' Store the page we're on before changing items per page
                    Dim currentTopItem As Integer = (_currentPage - 1) * _itemsPerPage

                    ' Update the items per page
                    _itemsPerPage = value

                    ' Calculate which page the top item would be on with new page size
                    _currentPage = Math.Floor(currentTopItem / _itemsPerPage) + 1

                    ' Update the pagination
                    UpdatePagination()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets the total number of pages.
        ''' </summary>
        Public ReadOnly Property TotalPages As Integer
            Get
                Return _totalPages
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the maximum number of page buttons to display.
        ''' </summary>
        Public Property MaxPageButtons As Integer
            Get
                Return _maxPageButtons
            End Get
            Set(value As Integer)
                If value > 0 AndAlso value <> _maxPageButtons Then
                    _maxPageButtons = value
                    UpdatePageButtons()
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the filter function used for filtering items.
        ''' </summary>
        Public Property FilterFunction As Predicate(Of Object)
            Get
                Return _filterFunc
            End Get
            Set(value As Predicate(Of Object))
                _filterFunc = value
                ApplyFilterAndPagination()
            End Set
        End Property

        ''' <summary>
        ''' Initializes a new instance of the PaginationHelper class.
        ''' </summary>
        ''' <param name="dataGrid">The DataGrid to paginate.</param>
        ''' <param name="paginationPanel">The panel where pagination controls will be added.</param>
        ''' <param name="itemsPerPage">Optional. The number of items per page.</param>
        ''' <param name="maxPageButtons">Optional. The maximum number of page buttons to display.</param>
        Public Sub New(dataGrid As DataGrid, paginationPanel As Panel, Optional itemsPerPage As Integer = 10, Optional maxPageButtons As Integer = 5)
            _dataGrid = dataGrid
            _paginationPanel = paginationPanel
            _itemsPerPage = itemsPerPage
            _maxPageButtons = maxPageButtons
            _allItems = New ObservableCollection(Of Object)()

            InitializePaginationControls()
        End Sub

        ''' <summary>
        ''' Initializes the pagination controls.
        ''' </summary>
        Private Sub InitializePaginationControls()
            ' Clear existing controls
            _paginationPanel.Children.Clear()

            ' Previous Page Button with arrow
            BtnPrevPage = New Button With {
                .Content = "←",
                .Style = TryCast(Application.Current.Resources("PaginationNavButtonStyle"), Style)
            }
            AddHandler BtnPrevPage.Click, AddressOf BtnPrevious_Click

            ' Next Page Button with arrow
            BtnNextPage = New Button With {
                .Content = "→",
                .Style = TryCast(Application.Current.Resources("PaginationNavButtonStyle"), Style)
            }
            AddHandler BtnNextPage.Click, AddressOf BtnNext_Click

            ' Add the buttons to the pagination panel
            _paginationPanel.Children.Add(BtnPrevPage)
            _paginationPanel.Children.Add(BtnNextPage)

            ' Update button states
            UpdatePageButtons()
        End Sub

        ''' <summary>
        ''' Updates the pagination display and DataGrid items.
        ''' </summary>
        Public Sub UpdatePagination()
            ' Calculate total pages
            If _allItems IsNot Nothing AndAlso _allItems.Count > 0 Then
                Dim filteredItems = GetFilteredItems()
                _totalPages = Math.Ceiling(filteredItems.Count / CDbl(_itemsPerPage))
            Else
                _totalPages = 1
            End If

            ' Ensure current page is valid
            If _currentPage < 1 Then _currentPage = 1
            If _currentPage > _totalPages Then _currentPage = _totalPages

            ' Get items for the current page
            Dim currentPageItems = GetCurrentPageItems()

            ' Set the DataGrid's ItemsSource to the current page
            _dataGrid.ItemsSource = currentPageItems

            ' Update the pagination buttons
            UpdatePageButtons()
        End Sub

        ''' <summary>
        ''' Gets the filtered items based on the current filter function.
        ''' </summary>
        ''' <returns>A collection of filtered items.</returns>
        Private Function GetFilteredItems() As ObservableCollection(Of Object)
            If _filterFunc Is Nothing OrElse _allItems Is Nothing Then
                Return _allItems
            End If

            Dim filteredItems = New ObservableCollection(Of Object)()
            For Each item In _allItems
                If _filterFunc(item) Then
                    filteredItems.Add(item)
                End If
            Next

            Return filteredItems
        End Function

        ''' <summary>
        ''' Gets the items for the current page.
        ''' </summary>
        ''' <returns>A collection of items for the current page.</returns>
        Private Function GetCurrentPageItems() As ObservableCollection(Of Object)
            Dim filteredItems = GetFilteredItems()

            ' Get items for the current page
            Dim startIndex As Integer = (_currentPage - 1) * _itemsPerPage
            Dim count As Integer = Math.Min(_itemsPerPage, filteredItems.Count - startIndex)

            ' Create a new collection for the current page
            Dim currentPageItems = New ObservableCollection(Of Object)()

            If filteredItems.Count > 0 AndAlso startIndex < filteredItems.Count Then
                For i As Integer = startIndex To Math.Min(startIndex + count - 1, filteredItems.Count - 1)
                    currentPageItems.Add(filteredItems(i))
                Next
            End If

            Return currentPageItems
        End Function

        ''' <summary>
        ''' Updates the page buttons based on the current pagination state.
        ''' </summary>
        Private Sub UpdatePageButtons()
            ' Remove all existing page buttons
            For i As Integer = _paginationPanel.Children.Count - 2 To 1 Step -1
                _paginationPanel.Children.RemoveAt(i)
            Next
            _pageButtons.Clear()

            ' No pages to show
            If _totalPages <= 0 Then
                Return
            End If

            ' Calculate which page buttons to show
            Dim startPage As Integer = Math.Max(1, _currentPage - (_maxPageButtons \ 2))
            Dim endPage As Integer = Math.Min(_totalPages, startPage + _maxPageButtons - 1)

            ' Adjust startPage if we're near the end
            If endPage - startPage + 1 < _maxPageButtons Then
                startPage = Math.Max(1, endPage - _maxPageButtons + 1)
            End If

            ' If we need "..." at beginning
            If startPage > 1 Then
                Dim firstPageBtn = CreatePageButton(1)
                _paginationPanel.Children.Insert(1, firstPageBtn)
                _pageButtons.Add(firstPageBtn)

                If startPage > 2 Then
                    Dim ellipsisText = New TextBlock With {
                        .Text = "...",
                        .VerticalAlignment = VerticalAlignment.Center,
                        .Margin = New Thickness(5, 0, 5, 0),
                        .FontFamily = New FontFamily("Lexend")
                    }
                    _paginationPanel.Children.Insert(2, ellipsisText)
                End If
            End If

            ' Add numbered page buttons
            Dim insertIndex As Integer = If(startPage > 1, If(startPage > 2, 3, 2), 1)
            For i As Integer = startPage To endPage
                Dim pageButton = CreatePageButton(i)

                ' Highlight current page
                If i = _currentPage Then
                    pageButton.Style = TryCast(Application.Current.Resources("PaginationCurrentPageButtonStyle"), Style)
                End If

                _paginationPanel.Children.Insert(insertIndex, pageButton)
                _pageButtons.Add(pageButton)
                insertIndex += 1
            Next

            ' If we need "..." at end
            If endPage < _totalPages Then
                If endPage < _totalPages - 1 Then
                    Dim ellipsisText = New TextBlock With {
                        .Text = "...",
                        .VerticalAlignment = VerticalAlignment.Center,
                        .Margin = New Thickness(5, 0, 5, 0),
                        .FontFamily = New FontFamily("Lexend")
                    }
                    _paginationPanel.Children.Insert(insertIndex, ellipsisText)
                    insertIndex += 1
                End If

                Dim lastPageBtn = CreatePageButton(_totalPages)
                _paginationPanel.Children.Insert(insertIndex, lastPageBtn)
                _pageButtons.Add(lastPageBtn)
            End If

            ' Update button states
            BtnPrevPage.IsEnabled = (_currentPage > 1)
            BtnNextPage.IsEnabled = (_currentPage < _totalPages)
        End Sub

        ''' <summary>
        ''' Creates a page number button.
        ''' </summary>
        ''' <param name="pageNumber">The page number for the button.</param>
        ''' <returns>A Button control for the specified page.</returns>
        Private Function CreatePageButton(pageNumber As Integer) As Button
            Dim pageButton As New Button With {
                .Content = pageNumber.ToString(),
                .Style = TryCast(Application.Current.Resources("PaginationButtonStyle"), Style),
                .Tag = pageNumber
            }

            AddHandler pageButton.Click, AddressOf PageButton_Click
            Return pageButton
        End Function

        ''' <summary>
        ''' Handles click events on page number buttons.
        ''' </summary>
        Private Sub PageButton_Click(sender As Object, e As RoutedEventArgs)
            Dim button = TryCast(sender, Button)
            If button IsNot Nothing AndAlso button.Tag IsNot Nothing Then
                Dim newPage As Integer = CInt(button.Tag)
                If newPage <> _currentPage Then
                    _currentPage = newPage
                    UpdatePagination()
                End If
            End If
        End Sub

        ''' <summary>
        ''' Handles click events on the previous page button.
        ''' </summary>
        Private Sub BtnPrevious_Click(sender As Object, e As RoutedEventArgs)
            If _currentPage > 1 Then
                _currentPage -= 1
                UpdatePagination()
            End If
        End Sub

        ''' <summary>
        ''' Handles click events on the next page button.
        ''' </summary>
        Private Sub BtnNext_Click(sender As Object, e As RoutedEventArgs)
            If _currentPage < _totalPages Then
                _currentPage += 1
                UpdatePagination()
            End If
        End Sub

        ''' <summary>
        ''' Applies the current filter and updates pagination.
        ''' </summary>
        Public Sub ApplyFilterAndPagination()
            ' Reset current page
            _currentPage = 1

            ' Update pagination
            UpdatePagination()
        End Sub
    End Class

    ''' <summary>
    ''' Provides search filtering functionality for DataGrid and other collection-based controls.
    ''' </summary>
    Public Class SearchFilterHelper
        Private ReadOnly _paginationHelper As PaginationHelper
        Private ReadOnly _searchProperties As List(Of String)
        Private _searchText As String = String.Empty

        ''' <summary>
        ''' Gets or sets the search text to filter by.
        ''' </summary>
        Public Property SearchText As String
            Get
                Return _searchText
            End Get
            Set(value As String)
                If value <> _searchText Then
                    _searchText = value

                    ' Update the filter function on the pagination helper
                    If _paginationHelper IsNot Nothing Then
                        _paginationHelper.FilterFunction = AddressOf FilterItem
                        _paginationHelper.ApplyFilterAndPagination()
                    End If
                End If
            End Set
        End Property

        ''' <summary>
        ''' Initializes a new instance of the SearchFilterHelper class.
        ''' </summary>
        ''' <param name="paginationHelper">The pagination helper to work with.</param>
        ''' <param name="propertiesToSearch">The names of the properties to search in.</param>
        Public Sub New(paginationHelper As PaginationHelper, ParamArray propertiesToSearch As String())
            _paginationHelper = paginationHelper
            _searchProperties = New List(Of String)()

            ' Add the provided properties to the search list
            For Each prop In propertiesToSearch
                _searchProperties.Add(prop)
            Next

            ' Set the initial filter function
            _paginationHelper.FilterFunction = AddressOf FilterItem
        End Sub

        ''' <summary>
        ''' Adds property names to search in.
        ''' </summary>
        ''' <param name="propertyNames">The property names to add.</param>
        Public Sub AddSearchProperties(ParamArray propertyNames As String())
            For Each propName In propertyNames
                If Not _searchProperties.Contains(propName) Then
                    _searchProperties.Add(propName)
                End If
            Next

            ' Update the filter
            _paginationHelper?.ApplyFilterAndPagination()
        End Sub

        ''' <summary>
        ''' Clears all search properties.
        ''' </summary>
        Public Sub ClearSearchProperties()
            _searchProperties.Clear()

            ' Update the filter
            _paginationHelper?.ApplyFilterAndPagination()
        End Sub

        ''' <summary>
        ''' Filter function that determines if an item matches the search criteria.
        ''' </summary>
        ''' <param name="item">The item to check.</param>
        ''' <returns>True if the item matches the search criteria; otherwise, false.</returns>
        Private Function FilterItem(item As Object) As Boolean
            ' If no search text, show all items
            If String.IsNullOrWhiteSpace(_searchText) Then
                Return True
            End If

            ' If no properties specified, don't filter
            If _searchProperties Is Nothing OrElse _searchProperties.Count = 0 Then
                Return True
            End If

            Dim searchText As String = _searchText.ToLower()

            ' Check each property
            For Each propName In _searchProperties
                Try
                    ' Try to get the property using reflection
                    Dim prop = item.GetType().GetProperty(propName)
                    If prop IsNot Nothing Then
                        Dim propValue = prop.GetValue(item, Nothing)?.ToString()
                        If propValue IsNot Nothing AndAlso propValue.ToLower().Contains(searchText) Then
                            Return True
                        End If
                    End If
                Catch ex As Exception
                    ' Skip this property if an error occurs
                    Continue For
                End Try
            Next

            ' No property matched the search
            Return False
        End Function
    End Class

End Namespace