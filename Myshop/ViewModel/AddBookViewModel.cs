using IronXL;
using Microsoft.Win32;
using Myshop.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Windows;
using System.Threading;

namespace Myshop.ViewModel
{
    public class AddBookViewModel : ViewModelBase
    {
        public ICommand UpdateInfoCommand { get; }
        public ICommand FindImageCommand { get; }
        private Book _currentBook = new Book();

        public string Title
        {
            get { return _currentBook.title; }
            set
            {
                _currentBook.title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public int PublishedYear
        {
            get { return _currentBook.publishedYear; }
            set
            {
                _currentBook.publishedYear = value;
                OnPropertyChanged(nameof(PublishedYear));
            }
        }

        public ImageSource CoverImage
        {
            get { return _currentBook.coverImage; }
            set
            {
                _currentBook.coverImage = value;
                OnPropertyChanged(nameof(CoverImage));
            }
        }

        public string Author
        {
            get { return _currentBook.author; }
            set
            {
                _currentBook.author = value;
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

        public AddBookViewModel()
        {
            _currentBook = new Book()
            {
                title = "Hành trình vô cực 2",
                author = "Lâm Vĩ Dạ, Đoàn Thư",
                Amount = 1,
                Categories = new List<int>() { 1, 10 },
                price = 200000,
                publishedYear = 2016,
        };
            UpdateInfoCommand = new ViewModelCommand(ExecuteUpdate);
            FindImageCommand = new ViewModelCommand(ExecuteFind);
        }

        private string convertImageToBase64(string stringValue)
        {
            Uri uri = new Uri(stringValue, UriKind.Relative);
            StreamReader info = new StreamReader(stringValue);
            var imageArray = new BinaryReader(info.BaseStream).ReadBytes((int)info.BaseStream.Length);
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
                    {"image", convertImageToBase64(b.ImageBase64)},
                    { "price", b.price },
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
                        var json2 = JsonNode.Parse(r);
                        foreach (var c in b.Categories)
                        {
                            var bookId = int.Parse(json2["data"]["id"].ToString());
                            var catObj = new JsonObject
                            {
                                { "bookId", bookId },
                                { "categoryId", c }

                            };
                            var content2 = new StringContent(JsonSerializer.Serialize(catObj), Encoding.Default, "application/json");
                            using var httpClient2 = new HttpClient();
                            using var response2 = await httpClient2.PostAsync("https://hcmusshop.azurewebsites.net/api/CategoriesOfBooks", content2);
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        public async void ExecuteUpdate(object obj)
        {
            await SendPostRequestAsyncForBook(new List<Book>() { _currentBook }, "https://hcmusshop.azurewebsites.net/api/Book");
        }

        public void ExecuteFind(object obj)
        {
            OpenFileDialog browseDiaglog = new OpenFileDialog();
            browseDiaglog.Multiselect = false;
            browseDiaglog.Filter = "Image Files(*.bmp; *png)|*.bmp; *png";

            if (browseDiaglog.ShowDialog() == true)
            {
                var fileName = browseDiaglog.FileName;


                //CoverImage = "./img/" + fileName;
                CoverImage = BookViewModel.ConvertToBitmapSource((Bitmap)Bitmap.FromFile(fileName));
                _currentBook.coverImage = CoverImage;
                _currentBook.ImageBase64 = fileName;
            }

        }
    }

}
