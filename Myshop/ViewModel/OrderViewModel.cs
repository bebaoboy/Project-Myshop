using Myshop.Model;
using Myshop.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using static System.Reflection.Metadata.BlobBuilder;

namespace Myshop.ViewModel
{
    public class OrderViewModel: ViewModelBase
    {
        private CollectionViewSource orderItemsSource;
        public ICollectionView orderSourceCollection => orderItemsSource.View;

        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ViewDetailCommand { get; }

        int _currentPage = 1;
        int _rowsPerPage = 10;
        int _totalItems = 0;
        int _totalPages = 0;

        private string _keyword = "";

        int _currentIndex = -1;

        ObservableCollection<Order> orderItems;
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

        private string _pageInfoText = "Displaying 10/30 orders.";
        public string PageInfoText
        {
            get
            {
                return _pageInfoText;
            }
            set
            {
                _pageInfoText = value;
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

        private List<BookInOrder> bookInOrders;
        public List<BookInOrder> BookInOrders
        {
            get { return bookInOrders; }
            set { bookInOrders = orderItems[_currentIndex].OrderedBook;
                OnPropertyChanged(nameof(BookInOrders));
            }
        }

        private double _currentTotalPrice;
        public double CurrentTotalPrice
        {
            get { return _currentTotalPrice; }
            set { _currentTotalPrice = orderItems[_currentIndex].TotalPrice;
                OnPropertyChanged(nameof(CurrentTotalPrice));
            }
        }

        public OrderViewModel()
        {
            List<BookInOrder> books1 = new(), books2 = new();
            books1.Add(
                new BookInOrder
                {
                    Id = "1",
                    Amount = 5,
                    Title = "Hello"
                }
            );

            books1.Add(
                new BookInOrder
                {
                    Id = "2",
                    Amount = 2,
                    Title = "moah"
                }
            );

            books2.Add(
               new BookInOrder
               {
                   Id = "1",
                   Amount = 5,
                   Title = "Hell"
               }
           );

            books2.Add(
                new BookInOrder
                {
                    Id = "6",
                    Amount = 1,
                    Title = "Kitty"
                }
            );

            orderItems = new ObservableCollection<Order>();
            
                orderItems.Add(
                    new Order
                    {
                        DateCreated = DateTime.Now.ToString(),
                        TotalPrice = 400000,
                        OrderedBook = books1,
                    });
            orderItems.Add(
                  new Order
                  {
                      DateCreated = DateTime.Now.ToString(),
                      TotalPrice = 400000,
                      OrderedBook = books2,
                  });


            orderItemsSource = new CollectionViewSource { Source = orderItems };
           // _updateDataSource(1);
            // _updatePagingInfo();
            NextPageCommand = new ViewModelCommand(ExecuteNextPageCommand);
            PrevPageCommand = new ViewModelCommand(ExecutePrevPageCommand);
            EditCommand = new ViewModelCommand(ExecuteEditCommand);
            DeleteCommand = new ViewModelCommand(ExecuteDeleteCommand);
            ViewDetailCommand = new ViewModelCommand(ExcecuteViewDetailCommand);
            // ReadImageAsync();
        }

        private void ExecuteNextPageCommand(object obj)
        {
           // _updateDataSource(_currentPage + 1);
        }

        private void ExecutePrevPageCommand(object obj)
        {
            // _updateDataSource(_currentPage - 1);
        }
        public void SelectionChanged(object sender, EventArgs e)
        {
            System.Windows.Controls.ComboBox SelectBox = (System.Windows.Controls.ComboBox)sender;
           // _updateDataSource(SelectBox.SelectedIndex + 1);
        }

        public void ExcecuteViewDetailCommand(object obj)
        {

        }


        public void ExecuteDeleteCommand(object obj)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Chắc chắn?", "Xóa cuốn sách này chứ?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {

                MenuItem menuItem = (MenuItem)obj;

                int i = _currentIndex;
                if (i == -1) return;
                string sql = "delete from book where id = @id";
                //var command = new SqlCommand(sql, _connection);
                //command.Parameters.Add("@id", SqlDbType.Int).Value = books[i].id;

                //int rows = command.ExecuteNonQuery();

                //if (rows > 0)
                //{
                //    MessageBox.Show($"Book {books[i].title} is deleted");
                //}
                orderItems.RemoveAt(_rowsPerPage * (_currentPage - 1) + _currentIndex);
                // _updateDataSource(_currentPage);
            }
        }

        public void ExecuteEditCommand(object obj)
        {
            Order b = orderItems.ElementAt(_rowsPerPage * (_currentPage - 1) + _currentIndex);
            //EditView editView = new EditView();
            //var editViewModel = new EditViewModel();
            //editView.DataContext = editViewModel;
            //editView.Show();
        }

        public void setUpdateBookData()
        {

        }

        public void ListSelectionChanged(object sender, EventArgs e)
        {
            System.Windows.Controls.ListView SelectBox = (System.Windows.Controls.ListView)sender;
            _currentIndex = SelectBox.SelectedIndex;

            BookInOrders = orderItems[_currentIndex].OrderedBook;
            CurrentTotalPrice = orderItems[_currentIndex].TotalPrice;
        }
    }
}
