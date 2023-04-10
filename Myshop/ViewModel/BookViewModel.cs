using Myshop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Myshop.ViewModel
{
    public class BookViewModel: ViewModelBase
    {
        
        private CollectionViewSource BookItemsCollection;
        public ICollectionView BookSourceCollection => BookItemsCollection.View;
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }

        int _currentPage = 1;
        int _rowsPerPage = 10;
        int _totalItems = 0;
        int _totalPages = 0;
        private string _keyword = "";

        ObservableCollection<Book> bookItems;
        private bool _nextPageEnabled = true;

        public bool NextPageEnabled
        {
            get
            {
                return _nextPageEnabled;
            }

            set
            {
                _nextPageEnabled = value;
                OnPropertyChanged(nameof(NextPageEnabled));
            }
        }
        private bool _prevPageEnabled = true;

        public bool PrevPageEnabled
        {
            get
            {
                return _prevPageEnabled;
            }

            set
            {
                _prevPageEnabled = value;
                OnPropertyChanged(nameof(PrevPageEnabled));
            }
        }
        private int _comboBoxCurrentPage = 0;

        public int ComboBoxCurrentPage
        {
            get
            {
                return _comboBoxCurrentPage;
            }

            set
            {
                _comboBoxCurrentPage = value;
                OnPropertyChanged(nameof(ComboBoxCurrentPage));
            }
        }
        
        private string _pageInfoText = "Displaying 10/30 books.";
        public string PageInfoText
        {
            get
            {
                return _pageInfoText;
            }
            set
            {
                _pageInfoText= value;
                OnPropertyChanged(nameof(PageInfoText));
            }
        }
        private ObservableCollection<Tuple<int, int>> _comboBoxItemsSource;
        public ObservableCollection<Tuple<int, int>> ComboBoxItemsSource
        {
            get
            {
                return _comboBoxItemsSource;
            }
            set
            {
                _comboBoxItemsSource = value;
                OnPropertyChanged(nameof(ComboBoxItemsSource));
            }
        }


        /// <summary>
        /// 
        /// </summary>

        public BookViewModel()
        {
            bookItems = new ObservableCollection<Book>();
            for(int i = 0; i < 32; i++)
            {
                bookItems.Add(new Book { title = "Book number " + i });
            }

            BookItemsCollection = new CollectionViewSource { };
            _updateDataSource(1);
            _updatePagingInfo();
            NextPageCommand = new ViewModelCommand(ExecuteNextPageCommand);
            PrevPageCommand = new ViewModelCommand(ExecutePrevPageCommand);
        }

        private void _updateDataSource(int page)
        {
            if (page >= _totalPages) page = _totalPages;
            if (page < 1) page = 1;
            _currentPage = page;
            ComboBoxCurrentPage = _currentPage - 1;
            (var books, _totalItems) = GetAll(
                _currentPage, _rowsPerPage, _keyword);
            BookItemsCollection = new CollectionViewSource { Source = books };
            OnPropertyChanged(nameof(BookSourceCollection));
            PageInfoText =
                $"Displaying {books.Count} / {_totalItems} books.";
            if (ComboBoxCurrentPage == _totalPages - 1)
            {
                NextPageEnabled = false;
            }
            else
            {
                NextPageEnabled = true;
            }
            if (ComboBoxCurrentPage == 0)
            {
                PrevPageEnabled = false;
            }
            else
            {
                PrevPageEnabled = true;
            }
        }

        private void _updatePagingInfo()
        {
            _totalPages = _totalItems / _rowsPerPage +
                   (_totalItems % _rowsPerPage == 0 ? 0 : 1);

            // Cập nhật ComboBox
            var lines = new ObservableCollection<Tuple<int, int>>();
            for (int i = 1; i <= _totalPages; i++)
            {
                lines.Add(new Tuple<int, int>(i, _totalPages));
            }
            ComboBoxItemsSource = lines;
        }

        public Tuple<List<Book>, int> GetAll(
                int currentPage = 1, int rowsPerPage = 10,
                string keyword = "", bool? sortAsc = null)
        {
            var list = bookItems.Where(
                item => item.title.ToLower().Contains(keyword.ToLower())
            );

            if (sortAsc != null)
            {
                if ((bool)sortAsc)
                {
                    list = list.OrderBy(keySelector: x => x.title);
                }
                else
                {
                    list = list.OrderByDescending(keySelector: x => x.title);
                }
            }

            var items = list.Skip((currentPage - 1) * rowsPerPage)
                .Take(rowsPerPage);

            // totalItems = 27 sinh vieen
            // rowsPerPage = 10 sinh vien moi trang
            // ==> bao nhieu trang?
            // 
            // totalPages = 27 / 10 + (27 % 10 == 0? 0 : 1)
            // = 2 +  1 ==> 3
            // = totalItems / rowsPerPage +
            //   (totalItems % rowsPerPage == 0 ? 0 : 1)\
            // Trang 1 - Skip (0) Take(10)
            // Trang 2 - Skip (10 * 1) Take(10)
            // Trang 3 - Skip (10 * 2) Take(10)
            // Trang 4 - Skip (10 * 3) Take(10)

            // Trang i - Skip((i-1) * rowsPerPage ) Take(rowsPerPage)
            var result = new Tuple<List<Book>, int>(
                items.ToList(), list.Count()
            );
            return result;
        }
        private void ExecuteNextPageCommand(object obj)
        {
            _updateDataSource(_currentPage + 1);
        }

        private void ExecutePrevPageCommand(object obj)
        {
            _updateDataSource(_currentPage - 1);
        }
        public void SelectionChanged(object sender, EventArgs e)
        {
            ComboBox SelectBox = (ComboBox)sender;
            _updateDataSource(SelectBox.SelectedIndex + 1);
        }
    }
}
