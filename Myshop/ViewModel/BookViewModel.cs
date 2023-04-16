using Myshop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using static System.Reflection.Metadata.BlobBuilder;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Myshop.View;
using System.Windows.Controls.DataVisualization;
using Xceed.Wpf.Toolkit;
using PixelFormatsConverter;

namespace Myshop.ViewModel
{
    public class BookViewModel: ViewModelBase
    {
        
        static private CollectionViewSource BookItemsCollection = new CollectionViewSource { };
        public ICollectionView BookSourceCollection => BookItemsCollection.View;

        static private CollectionViewSource CatItemsCollection = new CollectionViewSource { };
        public ICollectionView CatSourceCollection => CatItemsCollection.View;
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


        static ObservableCollection<Book> bookItems = new ObservableCollection<Book>();
        static ObservableCollection<Category> catItems = new ObservableCollection<Category>();
        private bool _nextPageEnabled = true;
        private List<Book> currentSearchResult = new();

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
        private int _comboBoxRowPerPage = 1;

        public int ComboBoxRowPerPage
        {
            get
            {
                return _comboBoxRowPerPage;
            }

            set
            {
                _comboBoxRowPerPage = value;
                OnPropertyChanged(nameof(ComboBoxRowPerPage));
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

        private string _currentName = "";

        public string CurrentName
        {
            get { return _currentName; }
            set
            {
                _currentName = value;
              
                OnPropertyChanged(nameof(CurrentName));
            }
        }

        private string _currentAuthor = "";

        public string CurrentAuthor
        {
            get { return _currentAuthor; }
            set
            {
                _currentAuthor = value;

                OnPropertyChanged(nameof(CurrentAuthor));
            }
        }

        private int _currentAmount = 1;

        public int CurrentAmount
        {
            get { return _currentAmount; }
            set { _currentAmount = value;
                OnPropertyChanged(nameof(CurrentAmount));
            }
        }

        private string _currentPrice = "100.000";
        public string CurrentPrice
        {
            get { return _currentPrice; }
            set { _currentPrice = value;
                OnPropertyChanged(nameof(CurrentPrice));
            }
        }

        private string _currentCategories = "";

        public string CurrentCategories
        {
            get { return _currentCategories; }
            set
            {
                _currentCategories = value;

                OnPropertyChanged(nameof(CurrentCategories));
            }
        }

        private int _catIndex = 0;

        public int CatIndex
        {
            get
            {
                return _catIndex;
            }

            set
            {
                _catIndex = value;
                OnPropertyChanged(nameof(CatIndex));
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public BookViewModel()
        {
            //for(int i = 0; i < 32; i++)
            //{
            //    bookItems.Add(new Book { title = "Book number " + i});
            //}

            //_updateDataSource(1);
            //_updatePagingInfo();
            NextPageCommand = new ViewModelCommand(ExecuteNextPageCommand);
            PrevPageCommand = new ViewModelCommand(ExecutePrevPageCommand);
            EditCommand = new ViewModelCommand(ExecuteEditCommand);
            DeleteCommand = new ViewModelCommand(ExecuteDeleteCommand);
            ViewDetailCommand = new ViewModelCommand(ExcecuteViewDetailCommand);
            ReadImageAsync();
            ReadAllCatAsync();

        }

        private List<Book> _updateDataSource(int page)
        {
            if (page >= _totalPages) page = _totalPages;
            if (page < 1) page = 1;
            _currentPage = page;
            (var books, _totalItems) = GetAll(
                _currentPage, _rowsPerPage, _keyword);
            _totalPages = _totalItems / _rowsPerPage +
       (_totalItems % _rowsPerPage == 0 ? 0 : 1);
            BookItemsCollection = new CollectionViewSource { Source = books };
            OnPropertyChanged(nameof(BookSourceCollection));
            if (ComboBoxItemsSource != null && _totalPages != ComboBoxItemsSource.Count)
            {
                _updatePagingInfo();
                //_currentPage = ComboBoxItemsSource.Count;
                //(books, _totalItems) = GetAll(
                //_currentPage, _rowsPerPage, _keyword);
            }
            ComboBoxCurrentPage = _currentPage - 1;

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
            return books;
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

        public Tuple<List<Book>, int> GetAll(
                int currentPage = 1, int rowsPerPage = 10,
                string keyword = "", bool? sortAsc = null)
        {
            IEnumerable<Book> list;

            if (CatIndex == 0)
            {
                list = bookItems;
            }
            else if (CatIndex == catItems.Count - 1)
            {
                list = bookItems.Where(
                    item => item.Categories.Count == 0
                );
            }
            else
            {
                list = bookItems.Where(
                    item => item.Categories.Contains(CatIndex)
                );
            }

            list = list.Where(
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
            System.Windows.Controls.ComboBox SelectBox = (System.Windows.Controls.ComboBox)sender;
            _updateDataSource(SelectBox.SelectedIndex + 1);
        }

        public void RowPerPageSelectionChanged(object sender, EventArgs e)
        {
            System.Windows.Controls.ComboBox SelectBox = (System.Windows.Controls.ComboBox)sender;
            ComboBoxRowPerPage = SelectBox.SelectedIndex;
            _rowsPerPage = int.Parse((string)((ComboBoxItem)SelectBox.SelectedItem).Content);
            _updateDataSource(_currentPage);
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
                bookItems.RemoveAt(_rowsPerPage * (_currentPage - 1) + _currentIndex);
                _updateDataSource(_currentPage);
            }
        }

        public void ExecuteEditCommand(object obj)
        {
            Book b = currentSearchResult.ElementAt(_rowsPerPage * (_currentPage - 1) + _currentIndex);
            EditView editView = new EditView();
            var editViewModel = new EditViewModel(b);
            editView.DataContext = editViewModel;
            editView.Show();
        }

        public void setUpdateBookData()
        {

        }

        public void ListSelectionChanged(object sender, EventArgs e)
        {
            System.Windows.Controls.ListView SelectBox = (System.Windows.Controls.ListView)sender;
            if (SelectBox.SelectedIndex == -1) return;

            _currentIndex = SelectBox.SelectedIndex;
            var index = _rowsPerPage * (_currentPage - 1) + _currentIndex;
            if (index < 0 || index >= currentSearchResult.Count) return;
            var bookItem = currentSearchResult[index];
            CurrentName = bookItem.title;
            CurrentAuthor = bookItem.author;
            CurrentPrice = bookItem.price + "";
            CurrentAmount = bookItem.Amount;
            CurrentCategories = bookItem.Categories.Count == 0 ? "Chưa phân loại" : string.Join(", ", bookItem.Categories.Select(x => GetCategory(x)));
        }

        public async Task ReadImageAsync()
        {
            System.Drawing.Image bm;
            var client = new HttpClient();
            //var request = new HttpRequestMessage(HttpMethod.Get, "https://api.imgur.com/3/image/8ABRUYt");
            var request = new HttpRequestMessage(HttpMethod.Get, "https://hcmusshop.azurewebsites.net/api/Book");
            //request.Headers.Add("Authorization", "Client-ID d27c6e0f38aa239");
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var r = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(r).AsArray();
                if (bookItems.Count != json.Count)
                {
                    bookItems.Clear();
                }
                for (int i = 0; i < json.Count; i++)
                {
                    var imgRequest = new HttpRequestMessage(HttpMethod.Get, json[i]["image"].ToString());

                    using (var imgResponse = await client.SendAsync(imgRequest))
                    {
                        imgResponse.EnsureSuccessStatusCode();
                        var imgStream = await imgResponse.Content.ReadAsByteArrayAsync();
                        using (var stream = new MemoryStream(imgStream))
                        {
                            bm = Bitmap.FromStream(stream);
                            try
                            {
                                var bookId = int.Parse(json[i]["id"].ToString());
                                using var httpClient2 = new HttpClient();
                                var request2 = new HttpRequestMessage(HttpMethod.Get, "https://hcmusshop.azurewebsites.net/api/CategoriesOfBooks/getCategories/" + bookId);
                                using var response2 = await httpClient2.SendAsync(request2);
                                response2.EnsureSuccessStatusCode();
                                var catArray = await response2.Content.ReadAsStringAsync();
                                var json2 = JsonNode.Parse(catArray).AsArray();
                                var cats = catArray.Length == 0 ? new List<int>() : json2.Select(x => (int)x).ToList();
                                var b = new Book()
                                {
                                    id = bookId,
                                    title = json[i]["title"].ToString(),
                                    author = json[i]["author"].ToString(),
                                    publishedYear = int.Parse(json[i]["datePublished"].ToString()),
                                    Amount = int.Parse(json[i]["amount"].ToString()),
                                    price = int.Parse(json[i]["price"].ToString()),
                                    coverImage = ConvertToBitmapSource((Bitmap)bm),
                                    Categories = cats
                                };
                                if (i <= bookItems.Count - 1)
                                {
                                    bookItems[i] = b;
                                }
                                else
                                {
                                    bookItems.Add(b);
                                }
                               
                                _updateDataSource(_currentPage);
                                _updatePagingInfo();
                            }
                            catch (Exception e) { Debug.WriteLine(e.Message + e.StackTrace + json[i]); }
                        }
                    }
                }
            }
        }

        public async Task ReadAllCatAsync()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://hcmusshop.azurewebsites.net/api/Category");
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var r = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(r).AsArray();

                catItems.Clear();
                
                for (int i = 0; i < json.Count; i++)
                {
                    var c = new Category()
                    {
                        Id = int.Parse(json[i]["categoryId"].ToString()),
                        Name = json[i]["categoryName"].ToString()
                    };
                    if(i <= catItems.Count - 1)
                    {
                       catItems[i] = c;
                    }
                    else
                    {
                        catItems.Add(c);
                    }
                }
                catItems.Insert(0, new Category() { Name = "Tất cả sách" });
                catItems.Add(new Category() { Name = "Chưa phân loại" });
                CatItemsCollection = new CollectionViewSource { Source = catItems.Select(x => x.Name) };
                OnPropertyChanged(nameof(CatSourceCollection));
            }
        }

        public void CatSelectionChanged(object sender, EventArgs e)
        {
            System.Windows.Controls.ComboBox SelectBox = (System.Windows.Controls.ComboBox)sender;
            CatIndex = SelectBox.SelectedIndex;
            currentSearchResult = _updateDataSource(_currentPage);
        }

        public void SearchTextChanged(object sender, EventArgs e)
        {
            var seachBar = (System.Windows.Controls.TextBox)sender;
            _keyword = seachBar.Text;
            currentSearchResult = _updateDataSource(_currentPage);
        }

        public static BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            BitmapSource bitmapSource;
            try
            {
                bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    bitmap.HorizontalResolution, bitmap.VerticalResolution,
                    bitmap.PixelFormat.Convert(), null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            } catch(Exception)
            {
                bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    bitmap.HorizontalResolution, bitmap.VerticalResolution,
                    PixelFormats.Bgr24, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            }

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }

        public static string GetCategory(int categoryId)
        {
            try
            {
                return catItems[categoryId].Name;
            } catch (Exception)
            {
                return "";
            }
        }
    }
}
