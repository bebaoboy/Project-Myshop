using System.ComponentModel;

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
        
        public string coverImage;
        public string author;
        public int publishedYear;
        

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
