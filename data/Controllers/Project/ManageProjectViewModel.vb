' ManageProjectViewModel.vb
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Windows.Input
Imports DPC.DPC.ViewModels

Namespace DPC

    Public Class ManageProjectViewModel
        Implements INotifyPropertyChanged

        ' ─── Backing Fields ──────────────────────────────────────────────────────
        Private _allProjects As List(Of ProjectModel) = New List(Of ProjectModel)()
        Private _pagedProjects As ObservableCollection(Of ProjectModel) = New ObservableCollection(Of ProjectModel)()
        Private _searchText As String = String.Empty
        Private _selectedPageSize As Integer = 10
        Private _currentPage As Integer = 1
        Private _totalPages As Integer = 1

        ' ─── Stats ───────────────────────────────────────────────────────────────
        Private _countWaiting As Integer
        Private _countProcessing As Integer
        Private _countSolved As Integer
        Private _countTotal As Integer

        ' ─── Properties ──────────────────────────────────────────────────────────
        Public Property PagedProjects As ObservableCollection(Of ProjectModel)
            Get
                Return _pagedProjects
            End Get
            Set(value As ObservableCollection(Of ProjectModel))
                _pagedProjects = value
                OnPropertyChanged()
            End Set
        End Property

        Public Property SearchText As String
            Get
                Return _searchText
            End Get
            Set(value As String)
                If _searchText <> value Then
                    _searchText = value
                    OnPropertyChanged()
                    _currentPage = 1
                    ApplyFilterAndPage()
                End If
            End Set
        End Property

        Public Property SelectedPageSize As Integer
            Get
                Return _selectedPageSize
            End Get
            Set(value As Integer)
                If _selectedPageSize <> value Then
                    _selectedPageSize = value
                    OnPropertyChanged()
                    _currentPage = 1
                    ApplyFilterAndPage()
                End If
            End Set
        End Property

        Public Property CurrentPage As Integer
            Get
                Return _currentPage
            End Get
            Set(value As Integer)
                If _currentPage <> value Then
                    _currentPage = value
                    OnPropertyChanged()
                    ApplyFilterAndPage()
                End If
            End Set
        End Property

        Public Property TotalPages As Integer
            Get
                Return _totalPages
            End Get
            Set(value As Integer)
                _totalPages = value
                OnPropertyChanged()
                OnPropertyChanged(NameOf(PageNumbers))
            End Set
        End Property

        ''' <summary>Returns the list of page numbers to display (up to 5 around current).</summary>
        Public ReadOnly Property PageNumbers As List(Of Integer)
            Get
                Dim pages As New List(Of Integer)()
                Dim startP = Math.Max(1, _currentPage - 2)
                Dim endP = Math.Min(_totalPages, startP + 4)
                For i = startP To endP
                    pages.Add(i)
                Next
                Return pages
            End Get
        End Property

        ' Stats
        Public Property CountWaiting As Integer
            Get
                Return _countWaiting
            End Get
            Set(value As Integer)
                _countWaiting = value
                OnPropertyChanged()
            End Set
        End Property

        Public Property CountProcessing As Integer
            Get
                Return _countProcessing
            End Get
            Set(value As Integer)
                _countProcessing = value
                OnPropertyChanged()
            End Set
        End Property

        Public Property CountSolved As Integer
            Get
                Return _countSolved
            End Get
            Set(value As Integer)
                _countSolved = value
                OnPropertyChanged()
            End Set
        End Property

        Public Property CountTotal As Integer
            Get
                Return _countTotal
            End Get
            Set(value As Integer)
                _countTotal = value
                OnPropertyChanged()
            End Set
        End Property

        ' ─── Commands ────────────────────────────────────────────────────────────
        Public ReadOnly Property NextPageCommand As ICommand
        Public ReadOnly Property PrevPageCommand As ICommand
        Public ReadOnly Property GoToPageCommand As ICommand
        Public ReadOnly Property ExportExcelCommand As ICommand

        ' ─── Constructor ─────────────────────────────────────────────────────────
        Public Sub New()
            NextPageCommand = New RelayCommand(AddressOf GoNext, Function() _currentPage < _totalPages)
            PrevPageCommand = New RelayCommand(AddressOf GoPrev, Function() _currentPage > 1)
            GoToPageCommand = New RelayCommand(Of Integer)(AddressOf GoToPage)
            ExportExcelCommand = New RelayCommand(AddressOf ExportToExcel)

            LoadProjects()
        End Sub

        ' ─── Data Loading ─────────────────────────────────────────────────────────
        ''' <summary>
        ''' Replace this stub with your real data-access layer (EF, ADO.NET, etc.).
        ''' </summary>
        Public Sub LoadProjects()
            ' TODO: replace with real DB / service call
            _allProjects = GetSampleData()
            UpdateStats()
            ApplyFilterAndPage()
        End Sub

        Private Function GetSampleData() As List(Of ProjectModel)
            Dim statuses = {"Waiting", "Processing", "Solved", "Waiting", "Processing", "Solved", "Waiting", "Solved"}
            Dim data As New List(Of ProjectModel)()
            For i = 1 To 50
                data.Add(New ProjectModel With {
                    .ProjectID = 1000 + i,
                    .Task = $"Project Task {i}",
                    .Customer = $"Customer {((i - 1) Mod 10) + 1}",
                    .ABC = $"ABC-{i:D3}",
                    .StartDate = DateTime.Today.AddDays(-i * 3),
                    .DueDate = DateTime.Today.AddDays(i * 2),
                    .Status = statuses((i - 1) Mod statuses.Length),
                    .AssignedTo = $"User {((i - 1) Mod 5) + 1}"
                })
            Next
            Return data
        End Function

        ' ─── Filtering & Paging ──────────────────────────────────────────────────
        Private Sub ApplyFilterAndPage()
            Dim filtered = _allProjects.AsEnumerable()

            ' Search filter (case-insensitive across key fields)
            If Not String.IsNullOrWhiteSpace(_searchText) Then
                Dim q = _searchText.ToLowerInvariant()
                filtered = filtered.Where(Function(p)
                                              Return (p.Task?.ToLowerInvariant().Contains(q) = True) OrElse
                                                     (p.Customer?.ToLowerInvariant().Contains(q) = True) OrElse
                                                     (p.Status?.ToLowerInvariant().Contains(q) = True) OrElse
                                                     (p.AssignedTo?.ToLowerInvariant().Contains(q) = True) OrElse
                                                     p.ProjectID.ToString().Contains(q)
                                          End Function)
            End If

            Dim filteredList = filtered.ToList()
            Dim total = filteredList.Count
            TotalPages = Math.Max(1, CInt(Math.Ceiling(total / _selectedPageSize)))

            ' Clamp current page
            If _currentPage > _totalPages Then _currentPage = _totalPages
            If _currentPage < 1 Then _currentPage = 1

            Dim page = filteredList.
                Skip((_currentPage - 1) * _selectedPageSize).
                Take(_selectedPageSize).
                ToList()

            _pagedProjects.Clear()
            For Each item In page
                _pagedProjects.Add(item)
            Next

            OnPropertyChanged(NameOf(PageNumbers))
            OnPropertyChanged(NameOf(CurrentPage))
        End Sub

        ' ─── Stats ───────────────────────────────────────────────────────────────
        Private Sub UpdateStats()
            CountWaiting = _allProjects.Count(Function(p) p.Status = "Waiting")
            CountProcessing = _allProjects.Count(Function(p) p.Status = "Processing")
            CountSolved = _allProjects.Count(Function(p) p.Status = "Solved")
            CountTotal = _allProjects.Count
        End Sub

        ' ─── Command Handlers ────────────────────────────────────────────────────
        Private Sub GoNext()
            CurrentPage += 1
        End Sub

        Private Sub GoPrev()
            CurrentPage -= 1
        End Sub

        Private Sub GoToPage(pageNumber As Integer)
            CurrentPage = pageNumber
        End Sub

        Private Sub ExportToExcel()
            ' TODO: integrate ClosedXML / EPPlus to export _allProjects
            System.Windows.MessageBox.Show("Excel export coming soon.", "Export", System.Windows.MessageBoxButton.OK)
        End Sub

        ' ─── INotifyPropertyChanged ──────────────────────────────────────────────
        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Protected Sub OnPropertyChanged(<CallerMemberName> Optional name As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
        End Sub

    End Class

End Namespace