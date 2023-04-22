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
using System.Windows.Media;

namespace Myshop.ViewModel
{
    public class CustomControl: ViewModelBase
    {
        private List<Book> bookItems;
        private ImageSource _currentImage;
        public ImageSource CurrentImage
        {
            get { return _currentImage; }
            set
            {
                _currentImage = value;
            }
        }

        private ObservableCollection<string> bookTitles = new();
        public ObservableCollection<string> BookTitles
        {
            get { return bookTitles; }
            set { bookTitles = value; }
        }


        private string _currentBookName;
        public string CurrentBookName
        {
            get;set;
        }

        private int _amount = 1;
        public int Amount
        {
            get; set;
        }
        public ICommand IncreaseAmount { get; }
        public ICommand DecreaseAmount { get; }

        public CustomControl(List<Book> books)
        {
            IncreaseAmount = new ViewModelCommand(ExecuteIncrease);
            DecreaseAmount = new ViewModelCommand(ExecuteDecrease);
            bookItems = books;

            GenerateData();
        }

        private void GenerateData()
        {
            foreach(var book in bookItems)
            {
                BookTitles.Add(book.title);
            }
            OnPropertyChanged(nameof(BookTitles));
        }

        public void SelectionChanged(object sender, EventArgs e)
        {
            System.Windows.Controls.ComboBox SelectBox = (System.Windows.Controls.ComboBox)sender;
            CurrentBookName = SelectBox.SelectedValue.ToString();
            OnPropertyChanged(nameof(CurrentBookName));
            getImageSource();
            OnPropertyChanged(nameof(CurrentImage));
        }

        private void getImageSource()
        {
            foreach(var book in bookItems)
            {
                if (book.title.Equals(CurrentBookName))
                {
                    CurrentImage = book.coverImage;
                    return;
                }
            }
        }

        private void ExecuteIncrease(object ob)
        {
            Amount += 1;
            OnPropertyChanged(nameof(Amount));
        }

        private void ExecuteDecrease(object ob)
        {
            Amount -= 1;
            OnPropertyChanged(nameof(Amount));
        }
    }
}
