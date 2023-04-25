using Myshop.Model;
using PixelFormatsConverter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Myshop.ViewModel
{
    public class EditOrderViewModel : ViewModelBase
    {
        private List<BookInOrder> baseData = new();
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

        public ICommand UpdateOrder { get; }

        private ObservableCollection<CustomControl> _customControls = new();
        public ObservableCollection<CustomControl> CustomControls
        {
            get { return _customControls; }
        }

        public EditOrderViewModel()
        {

        }

        public EditOrderViewModel(Order order)
        {
            baseData = order.OrderedBook;
            UpdateOrder = new ViewModelCommand(ExecuteUpdateOrder);
            CustomerName = order.CustomerName;
            Phone = order.PhoneNumber;
            Address = order.Address;

            for (int i = 0; i < baseData.Count; i++)
            {
                CustomControls.Add(new CustomControl(baseData[i], this)) ;
            }
        }

        private void ExecuteUpdateOrder(object ob)
        {
            SendPutRequest();
        }

        private async Task SendPutRequest()
        {
            var request = "https://hcmusshop.azurewebsites.net/api/Order";
            DateTime now = DateTime.Now;
            var jsonArray = new JsonArray();

            decimal total = 0;

            foreach (var custom in CustomControls)
            {
                var orderJson = new JsonObject
                {
                    {"bookId", custom.Id },
                    { "amount", custom.Amount},
                    {"book", new JsonObject{
                        {"id", custom.Id},
                        {"title", custom.CurrentBookName },
                        {"datePublished", custom.currentBook.publishedYear },
                        {"author", custom.currentBook.author },
                        {"image", custom.currentBook.ImageBase64 },
                        {
                            "price", custom.currentBook.price
                        },
                        {"amount", 0 },
                        {"categoriesOfBooks", new JsonArray() }
                    } }
                };
                total += (decimal)custom.currentBook.price * custom.Amount;
                jsonArray.Add(orderJson);
            }

            var json = new JsonObject
                {
                    {"customerName", CustomerName },
                    {"dateCreated", now.ToString("dd/MM/yyyy") },
                    {"phoneNumber", Phone},
                    {"address", Address },
                    {"total", total },
                    {"orderDetail", jsonArray}
                };


            var content = new StringContent(JsonSerializer.Serialize(json), Encoding.Default, "application/json");

            using var httpClient = new HttpClient();
            using (var response = await httpClient.PutAsync(request, content))
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                    var r = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Cập nhập thông tin", " Thành công");

                }
                catch (Exception) { }
            }
        }

    }
}


