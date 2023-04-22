using LiveCharts.Defaults;
using Myshop.Model;
using PixelFormatsConverter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Myshop.ViewModel
{
    public class AddOrderViewModel: ViewModelBase
    {

        private ObservableCollection<Book> baseData = new();
        private string _currentName;
        public string CustomerName
        {
            get
            {
                return _currentName;
            }
            set
            {
                _currentName = value;
                OnPropertyChanged(nameof(CustomerName));
            }
        }

        private string _phone;
        public string Phone
        {
            get { return _phone; }
            set
            {
                _phone = value;
                OnPropertyChanged(nameof(Phone));
            }
        }

        private string _address;
        public string Address
        {
            get { return _address; }
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        private Visibility visibility = Visibility.Hidden;
        public Visibility ButtonVisibility
        {
            get { return visibility; }
            set { visibility = value;
                OnPropertyChanged(nameof(visibility));
            }
        }

        public ICommand AddNewBookToOrder { get; }

        private ObservableCollection<CustomControl> _customControls = new();
        public ObservableCollection<CustomControl> CustomControls {
            get { return _customControls; }
        }

        public AddOrderViewModel()
        {
            ReadImageAsync();
            AddNewBookToOrder = new ViewModelCommand(ExecuteAddNewBook);  
        }

        public async Task ReadImageAsync()
        {
            System.Drawing.Image bm;
            var client = new HttpClient();
            //var request = new HttpRequestMessage(HttpMethod.Get, "https://api.imgur.com/3/image/8ABRUYt");
            var request = new HttpRequestMessage(HttpMethod.Get, "https://hcmusshop.azurewebsites.net/api/Book");
            //request.Headers.Add("Authorization", "Client-ID d27c6e0f38aa239");
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var r = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(r).AsArray();
                if (baseData.Count != json.Count)
                {
                    baseData.Clear();
                }
                for (int i = 0; i < json.Count; i++)
                {
                    var imgRequest = new HttpRequestMessage(HttpMethod.Get, json[i]["image"].ToString());

                    using (var imgResponse = await client.SendAsync(imgRequest))
                    {
                        imgResponse.EnsureSuccessStatusCode();
                        var imgStream = await imgResponse.Content.ReadAsByteArrayAsync();
                        using (var stream = new MemoryStream(imgStream))
                        {
                            bm = Bitmap.FromStream(stream);
                            try
                            {
                                var bookId = int.Parse(json[i]["id"].ToString());
                                using var httpClient2 = new HttpClient();
                                var request2 = new HttpRequestMessage(HttpMethod.Get, "https://hcmusshop.azurewebsites.net/api/CategoriesOfBooks/getCategories/" + bookId);
                                using var response2 = await httpClient2.SendAsync(request2);
                                response2.EnsureSuccessStatusCode();
                                var catArray = await response2.Content.ReadAsStringAsync();
                                var json2 = JsonNode.Parse(catArray).AsArray();
                                var cats = catArray.Length == 0 ? new List<int>() : json2.Select(x => (int)x).ToList();
                                var b = new Book()
                                {
                                    id = bookId,
                                    title = json[i]["title"].ToString(),
                                    author = json[i]["author"].ToString(),
                                    publishedYear = int.Parse(json[i]["datePublished"].ToString()),
                                    Amount = int.Parse(json[i]["amount"].ToString()),
                                    price = int.Parse(json[i]["price"].ToString()),
                                    coverImage = ConvertToBitmapSource((Bitmap)bm),
                                    Categories = cats
                                };
                                if (i <= baseData.Count - 1)
                                {
                                    baseData[i] = b;
                                }
                                else
                                {
                                    baseData.Add(b);
                                }
                            }
                            catch (Exception e) { Debug.WriteLine(e.Message + e.StackTrace + json[i]); }
                        }
                    }
                }
            }
        }

        public static BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            BitmapSource bitmapSource;
            try
            {
                bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    bitmap.HorizontalResolution, bitmap.VerticalResolution,
                    bitmap.PixelFormat.Convert(), null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            }
            catch (Exception)
            {
                bitmapSource = BitmapSource.Create(
                    bitmapData.Width, bitmapData.Height,
                    bitmap.HorizontalResolution, bitmap.VerticalResolution,
                    PixelFormats.Bgr24, null,
                    bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            }

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }


        private void ExecuteAddNewBook(object ob)
        {
            if(ButtonVisibility == Visibility.Hidden)
            {
                ButtonVisibility = Visibility.Visible;
                OnPropertyChanged(nameof(ButtonVisibility));
            }
            CustomControls.Add(new CustomControl(getActualData()));
        }

        private List<Book> getActualData() {
            List<Book> tempData = new();
            tempData.AddRange(baseData);
            foreach(var control in CustomControls)
            {
                for(int i = 0; i < tempData.Count; i++)
                {
                    if (tempData[i].title.Equals(control.CurrentBookName))
                    {
                        tempData.Remove(tempData[i]);
                        return tempData;
                    }
                }
            }
            return tempData;
        }
    }
}
