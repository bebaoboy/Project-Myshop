using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;

namespace Myshop.Model
{
    public class Book
    {
        public int id
        {
            get; set;
        }
        public string title
        {
            get; set;
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
