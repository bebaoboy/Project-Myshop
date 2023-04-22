using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Myshop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Myshop.ViewModel
{
    
    public class DashBoardViewModel: ViewModelBase
    {
        public enum ProfitType
        {
            date = 1,
            month = 2,
            year = 3,
            week = 4
        }
        private SeriesCollection profitCollection = new();
        public SeriesCollection ProfitCollection { get { return profitCollection; } set {
                profitCollection = value;
                OnPropertyChanged(nameof(ProfitCollection));
            } }


        private SeriesCollection topSoldCollection = new();
        public SeriesCollection TopSoldCollection
        {
            get { return topSoldCollection; }
            set { 
                topSoldCollection = value;
                OnPropertyChanged(nameof(TopSoldCollection));
            }
        }

        private int _chartStep = 1;
        public int ChartStep
        {
            get { return _chartStep; }
            set
            {
                _chartStep = value;
                OnPropertyChanged(nameof(ChartStep));
            }
        }

        private DateTime _dateMax = DateTime.Today;
        public DateTime DateMax
        {
            get { return _dateMax; }
            set
            {
                _dateMax = value;
                OnPropertyChanged(nameof(DateMax));
            }
        }
        private DateTime DateEnd = DateTime.Today;

        private DateTime _dateMin = DateTime.MinValue;

        public DateTime DateMin
        {
            get { return _dateMin; }
            set
            {
                _dateMin = value;
                OnPropertyChanged(nameof(DateMin));
            }
        }

        public CollectionViewSource RunningOutItemsSource = new CollectionViewSource { };
        public ICollectionView RunningOutCollection => RunningOutItemsSource.View;
        private ProfitType _profitType = ProfitType.date;

        private int totalBooks;
        public int TotalBooks { get; set; }

        private int totalOrders;
        public int TotalOrders { get; set; }

        private decimal totalProfit;
        public decimal TotalProfit { get; set; }

        private string xTitle;
        public string XTitle
        {
            get; set;
        }

        private List<string> xLabels;
        public List<string> XLabels { get; set; }  
        public ICommand ChartTodayCommand { get; }
        public ICommand ChartWeekCommand { get; }
        public ICommand ChartMonthCommand { get; }
        public ICommand ChartYearCommand { get; }
        public DashBoardViewModel()
        {
            getProfit("", _profitType);
            getNumBook();
            getNumOrder();
            getOutOfStock();
            ChartTodayCommand = new ViewModelCommand(ExecuteChartDate);
            ChartMonthCommand = new ViewModelCommand(ExecuteChartMonth);
            ChartYearCommand = new ViewModelCommand(ExecuteChartYear);
            ChartWeekCommand = new ViewModelCommand(ExecuteChartWeek);
        }

        public void ExecuteChartDate(object obj)
        {
            _profitType = ProfitType.date;
            DateMin = DateMax = DateEnd = DateTime.Today;
            ChartStep = 1;
            getProfit("", _profitType);
        }

        public void ExecuteChartMonth(object obj)
        {
            _profitType = ProfitType.month;
            ChartStep = 2;
            getProfit("", _profitType);
        }

        public void ExecuteChartYear(object obj)
        {
            _profitType = ProfitType.year;
            ChartStep = 1;
            getProfit("", _profitType);
        }

        public void ExecuteChartWeek(object obj)
        {
            _profitType = ProfitType.week;
            ChartStep = 1;
            getProfit("", _profitType);
        }

        public void CalendarChanged(object sender, EventArgs e)
        {
            var picker = (DatePicker)sender;
            DateMin = picker.SelectedDate ?? DateTime.Today;
            _profitType = ProfitType.date;
            ChartStep = 1;
            getProfit("", _profitType);
        }

        public void CalendarSet(object sender, EventArgs e)
        {
            var picker = (DatePicker)sender;
            DateEnd = picker.SelectedDate ?? DateTime.Today;
            DateMin = DateTime.MinValue;
            _profitType = ProfitType.date;
            ChartStep = 1;
            getProfit("", _profitType);
        }

        public async Task getProfit(string date, ProfitType typeProfit)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://hcmusshop.azurewebsites.net/api/Order");
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var r = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(r).AsArray();

                DateTime currentDate = DateTime.Today;
                ChartValues<Decimal> decimals = new();
                Dictionary<string, int> soldItems = new();
                if (typeProfit == ProfitType.date)
                {
                    Dictionary<string, decimal> orderTotal = new();

                    for (int i = 0; i < json.Count; i++)
                    {
                        string dateString = json[i]["dateCreated"].ToString();
                        DateTime orderCreatedDate = DateTime.ParseExact(dateString, "dd/MM/yyyy", null);
                        if(DateTime.Compare(DateMin, orderCreatedDate) <= 0 && DateTime.Compare(orderCreatedDate, DateEnd) <= 0)
                        {
                            if (orderTotal.ContainsKey(dateString))
                            {
                                orderTotal[dateString] += decimal.Parse(json[i]["total"].ToString());
                            }
                            else orderTotal.Add(dateString, decimal.Parse(json[i]["total"].ToString()));
                            var jsonArray = json[i]["orderDetail"].AsArray();
                            foreach (var item in jsonArray)
                            {
                                var amount = int.Parse(item["amount"].ToString());
                                var bookString = item["book"].AsObject();
                                BookInOrder temp = new BookInOrder(bookString, amount);
                                if (soldItems.ContainsKey(temp.Title))
                                {
                                    soldItems[temp.Title] += amount;
                                }
                                else
                                {
                                    soldItems[temp.Title] = amount;
                                }
                            }
                        }
                    }

                    foreach(var item in orderTotal.Values)
                    {
                        decimals.Add(item);
                    }
                    if (decimals.Count == 0)
                    {
                        var amount = 0;
                        soldItems["None"] = 10;
                        decimals.Add((int)100000);
                    }
                    TopSoldCollection.Clear();
                    foreach (var solditem in soldItems)
                    {
                        var key = solditem.Key;
                        var value = solditem.Value;

                        TopSoldCollection.Add(
                            new PieSeries
                            {
                                Title = key,
                                Values = new ChartValues<int>() { value}
                            }
                         );
                    }


                    ProfitCollection = new SeriesCollection {
                                new ColumnSeries{
                                    DataLabels = true,
                                    Values = decimals,
                                    Title = "Doanh thu theo ngày"
                                }
                            };

                    OnPropertyChanged(nameof(ProfitCollection));


                    XTitle = "Giờ";
                    XLabels = new List<string>(orderTotal.Keys);
                    
                    OnPropertyChanged(nameof(XTitle));
                    OnPropertyChanged(nameof(XLabels));
                }
                else if(typeProfit == ProfitType.month)
                {
                    int currentMonth = currentDate.Month;
                    Dictionary<int, decimal> dailyProfit = new();
                    for (int i = 1; i <= DateTime.DaysInMonth(currentDate.Year, currentMonth); i++)
                    {
                        dailyProfit[i] = 0;
                    }
                    for (int i = 0; i < json.Count; i++)
                    {
                        string dateString = json[i]["dateCreated"].ToString();
                        DateTime orderCreatedDate = DateTime.ParseExact(dateString, "dd/MM/yyyy", null);
                        int month = orderCreatedDate.Month;
                        if (month == currentMonth)
                        {
                            if (dailyProfit.ContainsKey(orderCreatedDate.Day))
                            {
                                dailyProfit[orderCreatedDate.Day] += decimal.Parse(json[i]["total"].ToString());
                            }
                            else
                            {
                                dailyProfit[orderCreatedDate.Day] = decimal.Parse(json[i]["total"].ToString());
                            }  
                        }
                    }

                    foreach (var item in dailyProfit.Values)
                    {
                        decimals.Add((int)item);
                    }


                    ProfitCollection = new SeriesCollection {
                                new ColumnSeries{
                                    DataLabels = true,
                                    Values = decimals,
                                    Title = "Doanh thu theo tháng"
                                }
                            };

                    OnPropertyChanged(nameof(ProfitCollection));

                    List<string> labels = new();
                    foreach(var day in dailyProfit.Keys)
                    {
                        labels.Add(day.ToString());
                    }

                    XTitle = "Ngày";
                    XLabels = labels;

                    OnPropertyChanged(nameof(XTitle));
                    OnPropertyChanged(nameof(XLabels));
                }
                else if(typeProfit == ProfitType.year)
                {
                    int currentYear = currentDate.Year;
                    Dictionary<int, decimal> monthlyProfit = new();
                    for(int i = 1; i <= 12; i++)
                    {
                        monthlyProfit[i] = 0;
                    }
                    for (int i = 0; i < json.Count; i++)
                    {
                        string dateString = json[i]["dateCreated"].ToString();
                        DateTime orderCreatedDate = DateTime.ParseExact(dateString, "dd/MM/yyyy", null);
                        int year = orderCreatedDate.Year;
                        if (year == currentYear)
                        {
                            if (monthlyProfit.ContainsKey(orderCreatedDate.Month))
                            {
                                monthlyProfit[orderCreatedDate.Month] += decimal.Parse(json[i]["total"].ToString());
                            }
                            else
                            {
                                monthlyProfit[orderCreatedDate.Month] = decimal.Parse(json[i]["total"].ToString());
                            }
                        }
                    }

                    foreach (var item in monthlyProfit.Values)
                    {
                        decimals.Add((int)item);
                    }


                    ProfitCollection = new SeriesCollection {
                                new ColumnSeries{
                                    DataLabels = true,
                                    Values = decimals,
                                    Title = "Doanh thu theo năm"
                                }
                            };

                    OnPropertyChanged(nameof(ProfitCollection));

                    List<string> labels = new();
                    foreach (var day in monthlyProfit.Keys)
                    {
                        labels.Add(day.ToString());
                    }

                    XTitle = "Tháng";
                    XLabels = labels;

                    OnPropertyChanged(nameof(XTitle));
                    OnPropertyChanged(nameof(XLabels));
                }       
            }
        }

        public async Task getNumBook()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://hcmusshop.azurewebsites.net/api/Book");
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var r = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(r).AsArray();

                TotalBooks = json.Count;
                OnPropertyChanged(nameof(TotalBooks));
            }
        }

        public async Task getNumOrder()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://hcmusshop.azurewebsites.net/api/Order");
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var r = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(r).AsArray();

                TotalOrders = json.Count;
                OnPropertyChanged(nameof(TotalOrders));

                decimal tempProfit = 0;
                for(int i = 0; i < TotalOrders; i++)
                {
                    var total = decimal.Parse(json[i]["total"].ToString());
                    tempProfit += total;
                }

                TotalProfit = tempProfit;
                OnPropertyChanged(nameof(TotalProfit));
            }
        }

        public async Task getOutOfStock()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://hcmusshop.azurewebsites.net/api/Book");
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var r = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(r).AsArray();

                int count = 5;
                List<Book> books = new();
                for(int i = 0; i < json.Count; i++)
                {
                    if (count > 0)
                    {
                        var amount = int.Parse(json[i]["amount"].ToString());
                        var title = json[i]["title"].ToString();

                        if (amount <= 5)
                        {
                            books.Add(new
                                Book
                            { 
                                title = title,
                                Amount = amount
                            }
                            );
                            count--;
                        }
                    }
                    else
                    {
                        RunningOutItemsSource = new CollectionViewSource { Source = books };
                        OnPropertyChanged(nameof(RunningOutCollection));
                        return;
                    }
                }
            }
        }
    }
}
