
using FontAwesome.Sharp;
using IronXL;
using Myshop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using static System.Reflection.Metadata.BlobBuilder;
using System.Net.Http.Json;
using System.Reflection;
using System.Windows.Resources;
using System.Windows;

namespace Myshop.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private CollectionViewSource MenuItemsCollection;

        // ICollectionView enables collections to have the functionalities of current record management,
        // custom sorting, filtering, and grouping.
        public ICollectionView SourceCollection => MenuItemsCollection.View;

        private ViewModelBase _currentChildView;
        private string _caption;
        private IconChar _icon;
        private string fileName = Directory.GetCurrentDirectory() + "/" + "data\\BookStoreData.xlsx";

        public ViewModelBase CurrentChildView
        {
            get
            {
                return _currentChildView;
            }

            set
            {
                _currentChildView = value;
                OnPropertyChanged(nameof(CurrentChildView));
            }
        }

        public string Caption
        {
            get
            {
                return _caption;
            }

            set
            {
                _caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }

        public IconChar Icon
        {
            get
            {
                return _icon;
            }

            set
            {
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }

        //--> Commands
        public ICommand ShowHomeViewCommand { get; }
        public ICommand ShowBookViewCommand { get; }
        public ICommand ShowOrderViewCommand { get; }

        public ICommand ImportData { get; }

        public MainViewModel()
        {

            // ObservableCollection represents a dynamic data collection that provides notifications when items
            // get added, removed, or when the whole list is refreshed.
            ObservableCollection<MenuItems> menuItems = new ObservableCollection<MenuItems>
            {
                new MenuItems { MenuName = "Dashboard", MenuImage = "Home" },
                new MenuItems { MenuName = "Books", MenuImage = "Book" },
                new MenuItems{MenuName="Orders", MenuImage="FirstOrder"}
            };

            MenuItemsCollection = new CollectionViewSource { Source = menuItems };
            MenuItemsCollection.Filter += MenuItems_Filter;

            //Initialize commands
            ShowHomeViewCommand = new ViewModelCommand(ExecuteShowHomeViewCommand);
            ShowBookViewCommand = new ViewModelCommand(ExecuteShowBooksViewCommand);
            ShowOrderViewCommand = new ViewModelCommand(ExecuteShowOrdersViewCommand);

            ImportData = new ViewModelCommand(ExecuteImportCommand);

            CurrentChildView = new DashBoardViewModel();
            ExecuteShowHomeViewCommand(null);
        }

        private async void ExecuteImportCommand(object obj)
        {
            WorkBook workbook = WorkBook.Load(fileName);
            if (workbook == null) return;

            //Read category data
            WorkSheet sheetCategory = workbook.GetWorkSheet("Category");
            var rows = sheetCategory.Rows;
            int count = 0;
            
            List<Category> cateList = new();
            foreach(var row in rows)
            {
                if (count == 0)
                {
                    count++; continue;
                }
                else
                {
                    count++;
                    Category category = new Category();
                    foreach (var cell in row)
                    {
                        category.Name = cell.StringValue;
                    }
                    cateList.Add(category);
                }
            }
            //Read Product data
            WorkSheet sheetProduct = workbook.GetWorkSheet("Book");
            rows = sheetProduct.Rows;
            count = 0;

            List<Book> bookList = new();
            foreach (var row in rows)
            {
                if (count == 0)
                {
                    count++; continue;
                }
                else
                {
                    count++;
                    Book book = new Book();
                    int cellCount = 0;
                    foreach (var cell in row)
                    {
                        switch (cellCount)
                        {
                            case 0:
                                book.title = cell.StringValue.Trim();
                                break;
                            case 1:
                                book.publishedYear = cell.IntValue;
                                break;
                            case 2:
                                book.author = cell.StringValue.Trim();
                                break;
                            case 3:
                                book.ImageBase64 = convertImageToBase64(cell.StringValue); 
                                break;
                            case 4:
                                book.price = cell.IntValue;
                                break;
                            default:
                                cellCount = 0; break;
                        }
                        cellCount++;
                    }
                    bookList.Add(book);
                }
            }

            var bookRequest = "https://hcmusshop.azurewebsites.net/api/Book";
            var catRequest = "https://hcmusshop.azurewebsites.net/api/Category";
            //await SendPostRequestAsyncForBook(bookList, bookRequest);
            await SendPostRequestAsyncForCat(cateList, catRequest);
        }


        private string convertImageToBase64(string stringValue)
        {
            Uri uri = new Uri(stringValue, UriKind.Relative);
            StreamResourceInfo info = Application.GetResourceStream(uri);
            var imageArray = new BinaryReader(info.Stream).ReadBytes((int)info.Stream.Length);
            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            return base64ImageRepresentation;
        }

        public async Task SendPostRequestAsyncForBook(List<Book> listOb, string request)
        {
            foreach (var b in listOb)
            {
                var json = new JsonObject
                {
                    {"title", b.title },
                    {"datePublished", b.publishedYear.ToString() },
                    {"author", b.author },
                    {"image", b.ImageBase64 },
                    {"price", b.price },
                    {"amount", b.Amount }
                };
                var content = new StringContent(JsonSerializer.Serialize(json), Encoding.Default, "application/json");

                using var httpClient = new HttpClient();
                using (var response = await httpClient.PostAsync(request, content))
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();
                        var r = await response.Content.ReadAsStringAsync();
                        Caption = r.ToString();
                    } catch (Exception) { }
                }
            }
        }

        public async Task SendPostRequestAsyncForCat(List<Category> listOb, string request)
        {
            foreach (var c in listOb)
            {
                var json = JsonSerializer.Serialize(c);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var httpClient = new HttpClient();
                using (var response = await httpClient.PostAsync(request, content))
                {
                    try { 
                        response.EnsureSuccessStatusCode();
                        var r = await response.Content.ReadAsStringAsync();
                        Caption = r.ToString();
                    }
                    catch (Exception) { }
                }
            }
        }


        // Text Search Filter.
        private string filterText;
        public string FilterText
        {
            get => filterText;
            set
            {
                filterText = value;
                MenuItemsCollection.View.Refresh();
                OnPropertyChanged("FilterText");
            }
        }

        private void MenuItems_Filter(object sender, FilterEventArgs e)
        {
            if (string.IsNullOrEmpty(FilterText))
            {
                e.Accepted = true;
                return;
            }

            MenuItems _item = e.Item as MenuItems;
            if (_item.MenuName.ToUpper().Contains(FilterText.ToUpper()))
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }

        // Switch Views
        public void SwitchViews(object parameter)
        {
            switch (parameter)
            {
                case "Dashboard":
                    CurrentChildView = new DashBoardViewModel();
                    break;
                case "Books":
                    CurrentChildView = new BookViewModel();
                    break;
                case "Orders":
                    CurrentChildView = new OrderViewModel();
                    break;
                default:
                    CurrentChildView = new DashBoardViewModel();
                    break;
            }
        }

        private void ExecuteShowBooksViewCommand(object obj)
        {
            CurrentChildView = new BookViewModel();
            Caption = "Sách";
            Icon = IconChar.Book;
        }

        private void ExecuteShowHomeViewCommand(object obj)
        {
            CurrentChildView = new DashBoardViewModel();
            Caption = "Dashboard";
            Icon = IconChar.Dashboard;
        }

        private void ExecuteShowOrdersViewCommand(object obj)
        {
            CurrentChildView = new OrderViewModel();
            Caption = "Đơn hàng";
            Icon = IconChar.FirstOrder;
        }
    }
}
