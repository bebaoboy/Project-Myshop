using Myshop.Model;
using Myshop.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
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
        private CollectionViewSource orderItemsSource = new CollectionViewSource { };
        public ICollectionView orderSourceCollection => orderItemsSource.View;

        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ViewDetailCommand { get; }
        public ICommand OpenAddNewOrder { get; }

        int _currentPage = 1;
        int _rowsPerPage = 10;
        int _totalItems = 0;
        int _totalPages = 0;

        private string _keyword = "";

        int _currentIndex = -1;

        ObservableCollection<Order> orderItems = new();
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

        public string coverImage = "/Images/logo.png";

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
            set { _currentTotalPrice = orderItems[_currentIndex].Total;
                OnPropertyChanged(nameof(CurrentTotalPrice));
            }
        }

        private string customerName;
        public string CustomerName
        {
            get { return customerName; }
            set { 
                customerName = value;
                OnPropertyChanged(nameof(customerName));
            }
        }

        private string phone;
        public string Phone
        {
            get { return phone; }
            set
            {
                phone = value;
                OnPropertyChanged(nameof(phone));
            }
        }

        private string address;
        public string Address
        {
            get { return address; }
            set
            {
                address = value;
                OnPropertyChanged(nameof(address));
            }
        }

        private DateTime _dateMax = DateTime.Today;
        public DateTime DateMax
        {
            get { return _dateMax; }
            set
            {
                _dateMax = value;
                OnPropertyChanged(nameof(DateMax));
            }
        }
        private DateTime DateEnd = DateTime.Today;

        private DateTime _dateMin = DateTime.MinValue;

        public DateTime DateMin
        {
            get { return _dateMin; }
            set
            {
                _dateMin = value;
                OnPropertyChanged(nameof(DateMin));
            }
        }

        public OrderViewModel()
        {
            // _updateDataSource(1);
            // _updatePagingInfo();
            NextPageCommand = new ViewModelCommand(ExecuteNextPageCommand);
            PrevPageCommand = new ViewModelCommand(ExecutePrevPageCommand);
            EditCommand = new ViewModelCommand(ExecuteEditCommand);
            DeleteCommand = new ViewModelCommand(ExecuteDeleteCommand);
            ViewDetailCommand = new ViewModelCommand(ExcecuteViewDetailCommand);
            OpenAddNewOrder = new ViewModelCommand(ExecuteOpenAddOrderPageCommand);

            getOrderList();
            // ReadImageAsync();
        }

        public async Task getOrderList()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://hcmusshop.azurewebsites.net/api/Order");
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var r = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(r).AsArray();

                orderItems.Clear();

                for (int i = 0; i < json.Count; i++)
                {
                    var jsonArray = json[i]["orderDetail"].AsArray();
                    List<BookInOrder> bookInOrders = new();
                    foreach (var item in jsonArray)
                    {
                        var amount = int.Parse(item["amount"].ToString());
                        var bookString = item["book"].AsObject();
                        bookInOrders.Add(new BookInOrder(bookString, amount));
                    }
                    var o = new Order()
                    {
                        Id = int.Parse(json[i]["id"].ToString()),
                        Address = json[i]["address"].ToString(),
                        DateCreated = json[i]["dateCreated"].ToString(),
                        CustomerName = json[i]["customerName"].ToString(),
                        PhoneNumber = json[i]["phoneNumber"].ToString(),
                        Total = double.Parse(json[i]["total"].ToString()),
                        OrderedBook = bookInOrders
                    };
                    if (i <= orderItems.Count - 1)
                    {
                        orderItems[i] = o;
                    }
                    else
                    {
                        orderItems.Add(o);
                    }
                }
                _updateDataSource(_currentPage);
                _updatePagingInfo();
            }
        }


        private List<Order> _updateDataSource(int page)
        {
            if (page >= _totalPages) page = _totalPages;
            if (page < 1) page = 1;
            _currentPage = page;
            (var orders, _totalItems) = GetAll(
                _currentPage, _rowsPerPage);
            _totalPages = _totalItems / _rowsPerPage +
       (_totalItems % _rowsPerPage == 0 ? 0 : 1);
            orderItemsSource = new CollectionViewSource { Source = orders };
            OnPropertyChanged(nameof(orderSourceCollection));
            if (ComboBoxItemsSource != null && _totalPages != ComboBoxItemsSource.Count)
            {
                _updatePagingInfo();
                //_currentPage = ComboBoxItemsSource.Count;
                //(books, _totalItems) = GetAll(
                //_currentPage, _rowsPerPage, _keyword);
            }
            ComboBoxCurrentPage = _currentPage - 1;

            PageInfoText =
                $"Displaying {orders.Count} / {_totalItems} orders.";
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
            return orders;
        }

        private void _updatePagingInfo()
        {

            // Cập nhật ComboBox
            var lines = new ObservableCollection<Tuple<int, int>>();
            for (int i = 1; i <= _totalPages; i++)
            {
                lines.Add(new Tuple<int, int>(i, _totalPages));
            }
            ComboBoxItemsSource = lines;
        }

        public Tuple<List<Order>, int> GetAll(
                int currentPage = 1, int rowsPerPage = 10,
                string keyword = "", bool? sortAsc = null)
        {
            IEnumerable<Order> list;

            list = orderItems;
               
            if (sortAsc != null)
            {
                if ((bool)sortAsc)
                {
                    list = list.OrderBy(keySelector: x => x.Total);
                }
                else
                {
                    list = list.OrderByDescending(keySelector: x => x.Total);
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
            var result = new Tuple<List<Order>, int>(
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
            System.Windows.Controls.ComboBox SelectBox = (System.Windows.Controls.ComboBox)sender;
           _updateDataSource(SelectBox.SelectedIndex + 1);
        }

        public void ExcecuteViewDetailCommand(object obj)
        {

        }


        public void ExecuteDeleteCommand(object obj)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Chắc chắn?", "Xóa đơn hàng này chứ?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {

                MenuItem menuItem = (MenuItem)obj;

                int i = _currentIndex;
                if (i == -1) return;
                orderItems.RemoveAt(_rowsPerPage * (_currentPage - 1) + _currentIndex);
                _updateDataSource(_currentPage);
                deleteRequest();
            }
        }

        public async Task deleteRequest()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Delete, "https://hcmusshop.azurewebsites.net/api/Order/" + orderItems[_currentIndex].Id);
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                MessageBox.Show("Xóa đơn hàng", "thành công");
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

        public void ExecuteOpenAddOrderPageCommand(object ob)
        {
            AddOrderView addView = new AddOrderView();
            var addOrderViewModel = new AddOrderViewModel();
            addView.DataContext = addOrderViewModel;
            addView.Show();
        }

        public void setUpdateBookData()
        {

        }

        public void ListSelectionChanged(object sender, EventArgs e)
        {
            System.Windows.Controls.ListView SelectBox = (System.Windows.Controls.ListView)sender;
            _currentIndex = SelectBox.SelectedIndex;

            BookInOrders = orderItems[_currentIndex].OrderedBook;
            CurrentTotalPrice = orderItems[_currentIndex].Total;
            CustomerName = orderItems[_currentIndex].CustomerName;
            Phone = orderItems[_currentIndex].PhoneNumber;
            Address = orderItems[_currentIndex].Address;
        }
    }
}
