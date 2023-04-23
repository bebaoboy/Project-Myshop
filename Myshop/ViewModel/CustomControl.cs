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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace Myshop.ViewModel
{
    public class CustomControl: ViewModelBase
    {
        private AddOrderViewModel parent;
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

        public Book currentBook;

        private string _currentBookName;
        public string CurrentBookName
        {
            get;set;
        }

        private int _amount = 1;
        public int Amount
        {
            get { return _amount; } set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        private int id;
        public int Id { get; set; }

        private int maxAmount;

        public ICommand IncreaseAmount { get; }
        public ICommand DecreaseAmount { get; }
        public ICommand Remove { get; }

        public CustomControl(List<Book> books, AddOrderViewModel parent)
        {
            IncreaseAmount = new ViewModelCommand(ExecuteIncrease);
            DecreaseAmount = new ViewModelCommand(ExecuteDecrease);
            Remove = new ViewModelCommand(ExecuteRemove);
            bookItems = books;

            this.parent = parent;

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
                    currentBook = book;
                    CurrentImage = book.coverImage;
                    maxAmount = book.Amount;
                    Id = book.id;
                    return;
                }
            }
        }



        private void ExecuteIncrease(object ob)
        {
            if (Amount >= maxAmount) Amount = maxAmount;
            Amount += 1;
            OnPropertyChanged(nameof(Amount));
        }

        private void ExecuteDecrease(object ob)
        {
            if (Amount == 1)
            {
                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Chắc chắn?", "Xóa cuốn sách này chứ?", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    ExecuteRemove(this);
                }
            }
            else
            {
                Amount -= 1;
            }
            OnPropertyChanged(nameof(Amount));
        }

        private void ExecuteRemove(object ob)
        {
            parent.Remove(this);
        }
    }
}
