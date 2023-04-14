using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;

namespace Myshop.Model
{
    public class Book: INotifyPropertyChanged
    {
        public int id
        {
            get; set;
        }
        private string _title;
        public string title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(title)));

            }
        }
        private ImageSource _coverImage;
        public ImageSource coverImage
        {
            get
            {
                return _coverImage;
            }
            set
            {
                _coverImage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(coverImage)));

            }
        }

        private double _price;
        public double price
        {
            get { return _price; }
            set { _price = value; }
        }

        private string _imageBase64;
        public string ImageBase64
        {
            get { return _imageBase64; }
            set { _imageBase64 = value; }
        }

        private string _author;
        public string author
        {
            get
            {
                return _author;
            }
            set
            {
                _author = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(author)));

            }
        }


        private int _publishedYear;
        public int publishedYear
        {
            get { return _publishedYear; }
            set
            {
                _publishedYear = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(author)));

            }
        }

        private int _amount = 10;
        public int Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Amount)));

            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Book Clone()
        {
            var temp_book = new Book()
            {
                id = this.id,
                title = this.title,
                author = this.author,
                publishedYear = this.publishedYear,
                coverImage = this.coverImage
            };
            return temp_book;
        }
        
    }
}
