using Microsoft.Win32;
using Myshop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.DataVisualization;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Myshop.ViewModel
{
    public class EditViewModel: ViewModelBase
    {
        public ICommand UpdateInfoCommand;
        public ICommand FindImageCommand;
        private Book _currentBook = new Book();

        public string Title
        {
            get { return _currentBook.title; }
            set { _currentBook.title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public int PublishedYear
        {
            get { return _currentBook.publishedYear; }
            set { _currentBook.publishedYear = value;
                OnPropertyChanged(nameof(PublishedYear));
            }
        }

        public ImageSource CoverImage
        {
            get { return _currentBook.coverImage; }
            set {
                _currentBook.coverImage = value;
                OnPropertyChanged(nameof(CoverImage));
            }
        }

        public string Author
        {
            get { return _currentBook.author; }
            set { _currentBook.author = value;
                OnPropertyChanged(nameof(Author));
            }
        }

        public string Category
        {
            get
            {
                return BookViewModel.GetCategory(_currentBook);
            }
            set
            {
                
            }
        }

        public EditViewModel()
        {

        }

        public EditViewModel(Book b)
        {
            _currentBook = b;
            UpdateInfoCommand = new ViewModelCommand(ExecuteUpdate);
            FindImageCommand = new ViewModelCommand(ExecuteFind);
        }


        public void ExecuteUpdate(object obj)
        {

        }

        public void ExecuteFind(object obj)
        {
            OpenFileDialog browseDiaglog = new OpenFileDialog();
            browseDiaglog.Multiselect = false;
            browseDiaglog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *png)|*.jpg; *.jpeg; *.gif; *.bmp; *png";

            if(browseDiaglog.ShowDialog() == true)
            {
                var fileName = browseDiaglog.FileName.Split("\\").Last();

                try
                {
                    File.Copy(browseDiaglog.FileName, "./img/" + fileName, true);
                }
                catch (Exception ex)
                {
                    
                }


                //CoverImage = "./img/" + fileName;
                CoverImage = BookViewModel.ConvertToBitmapSource((Bitmap)Bitmap.FromFile("./img/" + fileName));
            }
        }
    }
}
