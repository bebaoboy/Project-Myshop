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
        private string _coverImage;
        public string coverImage
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
        
        public ImageSource coverImage
        {
            get; set;
        }
        public string author
        {
            get; set;
        }
        public int publishedYear
        {
            get; set;
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
