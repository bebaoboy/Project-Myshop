using FontAwesome.Sharp;
using Myshop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

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

            CurrentChildView = new DashBoardViewModel();
            ExecuteShowHomeViewCommand(null);
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
