using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Myshop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Myshop.ViewModel
{
    public class WeekOfYear
    {
        public int WeekNumber { get; set; }
        public DateTime FirstDayOfWeek { get; set; }
        public DateTime LastDayOfWeek { get; set; }
        public static List<WeekOfYear> GetWeeksOfYear(int year)
        {
            var weeksQuantity = GetNumberOfWeeksInYear(year);
            var weeksList = new List<WeekOfYear>();

            for (int i = 1; i <= weeksQuantity; i++)
            {
                var weekNumber = i;

                DateTime firstDay;
                DateTime lastDay;
                GetFirstAndLastDateOfWeek(year, weekNumber, out firstDay, out lastDay);

                weeksList.Add(new WeekOfYear
                {
                    FirstDayOfWeek = firstDay,
                    LastDayOfWeek = lastDay,
                    WeekNumber = weekNumber
                });
            }

            return weeksList;
        }

        private static int GetNumberOfWeeksInYear(int year)
        {
            var dfi = DateTimeFormatInfo.CurrentInfo;
            var date1 = new DateTime(year, 12, 31);
            if (dfi != null)
            {
                System.Globalization.Calendar cal = dfi.Calendar;
                return cal.GetWeekOfYear(date1, dfi.CalendarWeekRule,
                    dfi.FirstDayOfWeek);
            }

            return 0;
        }

        private static void GetFirstAndLastDateOfWeek(int year, int weekOfYear, out DateTime firstDay, out DateTime lastDay)
        {
            var ci = CultureInfo.CurrentCulture;

            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = (int)ci.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;
            DateTime firstWeekDay = jan1.AddDays(daysOffset);
            int firstWeek = ci.Calendar.GetWeekOfYear(jan1, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek);
            if ((firstWeek <= 1 || firstWeek >= 52) && daysOffset >= -3)
            {
                weekOfYear -= 1;
            }

            firstDay = firstWeekDay.AddDays(weekOfYear * 7);
            lastDay = firstDay.AddDays(6);
        }
    }
    public class DashBoardViewModel: ViewModelBase
    {
        public enum ProfitType
        {
            date = 1,
            month = 2,
            year = 3,
            week = 4,
            monthly = 5
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
        public int TotalBooks {
            get { return totalBooks; }
            set
            {
                totalBooks = value;
                OnPropertyChanged(nameof(TotalBooks));
            }
        }

        private int totalOrders;
        public int TotalOrders {
            get { return totalOrders; }
            set
            {
                totalOrders = value;
                OnPropertyChanged(nameof(TotalOrders));
            }
        }

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
        public ICommand ChartMonthlyCommand { get; }
        public ICommand ChartYearCommand { get; }
        public DashBoardViewModel()
        {
            getProfit("", _profitType);
            getNumBook();
            getNumOrder();
            getOutOfStock();
            ChartTodayCommand = new ViewModelCommand(ExecuteChartDate);
            ChartMonthCommand = new ViewModelCommand(ExecuteChartMonth);
            ChartMonthlyCommand = new ViewModelCommand(ExecuteChartMonthly);
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

        public void ExecuteChartMonthly(object obj)
        {
            _profitType = ProfitType.monthly;
            ChartStep = 1;
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
            TotalOrders = 0;
            TotalBooks = 0;
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var r = await response.Content.ReadAsStringAsync();
                var json = JsonNode.Parse(r).AsArray();

                DateTime currentDate = DateTime.Today;
                ChartValues<Decimal> decimals = new();
                Dictionary<string, int> soldItems = new();
                try
                {
                    if (typeProfit == ProfitType.date)
                    {
                        SortedDictionary<string, decimal> orderTotal = new(Comparer<string>.Create(
                            (x, y) =>
                            {
                                return DateTime.Compare(DateTime.ParseExact(x, "dd/MM/yyyy", null), DateTime.ParseExact(y, "dd/MM/yyyy", null));
                            }));

                        for (int i = 0; i < json.Count; i++)
                        {
                            string dateString = json[i]["dateCreated"].ToString();
                            DateTime orderCreatedDate = DateTime.ParseExact(dateString, "dd/MM/yyyy", null);
                            if (DateTime.Compare(DateMin, orderCreatedDate) <= 0 && DateTime.Compare(orderCreatedDate, DateEnd) <= 0)
                            {
                                TotalOrders++;
                                if (orderTotal.ContainsKey(dateString))
                                {
                                    orderTotal[dateString] += decimal.Parse(json[i]["total"].ToString());
                                }
                                else orderTotal[dateString] = decimal.Parse(json[i]["total"].ToString());
                                var jsonArray = json[i]["orderDetail"].AsArray();
                                foreach (var item in jsonArray)
                                {
                                    var amount = int.Parse(item["amount"].ToString());
                                    TotalBooks += amount;
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

                        foreach (var item in orderTotal.Values)
                        {
                            decimals.Add(item);
                        }

                        ProfitCollection = new SeriesCollection {
                                new ColumnSeries{
                                    DataLabels = true,
                                    Values = decimals,
                                    Title = "Doanh thu theo ngày"
                                }
                            };

                        OnPropertyChanged(nameof(ProfitCollection));


                        XTitle = DateMin != DateEnd ? "Ngày (" + (DateMin == DateTime.MinValue ? "0" : DateMin.ToString("dd/MM/yyyy")) + " - " + DateEnd.ToString("dd/MM/yyyy") + ")" : ("Ngày " + DateEnd.ToString("dd/MM/yyyy"));
                        XLabels = new List<string>(orderTotal.Keys);

                        OnPropertyChanged(nameof(XTitle));
                        OnPropertyChanged(nameof(XLabels));
                    }
                    else if (typeProfit == ProfitType.month)
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
                                TotalOrders++;

                                if (dailyProfit.ContainsKey(orderCreatedDate.Day))
                                {
                                    dailyProfit[orderCreatedDate.Day] += decimal.Parse(json[i]["total"].ToString());
                                }
                                else
                                {
                                    dailyProfit[orderCreatedDate.Day] = decimal.Parse(json[i]["total"].ToString());
                                }
                                var jsonArray = json[i]["orderDetail"].AsArray();
                                foreach (var item in jsonArray)
                                {
                                    var amount = int.Parse(item["amount"].ToString());
                                    TotalBooks += amount;

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

                        foreach (var item in dailyProfit.Values)
                        {
                            decimals.Add((int)item);
                        }


                        ProfitCollection = new SeriesCollection {
                                new ColumnSeries{
                                    DataLabels = true,
                                    Values = decimals,
                                    Title = "Doanh thu mỗi ngày"
                                }
                            };

                        OnPropertyChanged(nameof(ProfitCollection));

                        List<string> labels = new();
                        foreach (var day in dailyProfit.Keys)
                        {
                            labels.Add(day.ToString());
                        }

                        XTitle = "Ngày (Tháng " + currentMonth + "/" + currentDate.Year + ")";
                        XLabels = labels;

                        OnPropertyChanged(nameof(XTitle));
                        OnPropertyChanged(nameof(XLabels));
                    }
                    else if (typeProfit == ProfitType.monthly)
                    {
                        int currentYear = currentDate.Year;
                        Dictionary<int, decimal> monthlyProfit = new();
                        for (int i = 1; i <= 12; i++)
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
                                TotalOrders++;

                                if (monthlyProfit.ContainsKey(orderCreatedDate.Month))
                                {
                                    monthlyProfit[orderCreatedDate.Month] += decimal.Parse(json[i]["total"].ToString());
                                }
                                else
                                {
                                    monthlyProfit[orderCreatedDate.Month] = decimal.Parse(json[i]["total"].ToString());
                                }
                                var jsonArray = json[i]["orderDetail"].AsArray();
                                foreach (var item in jsonArray)
                                {
                                    var amount = int.Parse(item["amount"].ToString());
                                    TotalBooks += amount;

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

                        foreach (var item in monthlyProfit.Values)
                        {
                            decimals.Add((int)item);
                        }


                        ProfitCollection = new SeriesCollection {
                                new ColumnSeries{
                                    DataLabels = true,
                                    Values = decimals,
                                    Title = "Doanh thu theo năm nay"
                                }
                            };

                        OnPropertyChanged(nameof(ProfitCollection));

                        List<string> labels = new();
                        foreach (var day in monthlyProfit.Keys)
                        {
                            labels.Add(day.ToString());
                        }

                        XTitle = "Tháng/" + currentYear;
                        XLabels = labels;

                        OnPropertyChanged(nameof(XTitle));
                        OnPropertyChanged(nameof(XLabels));
                    }
                    else if (typeProfit == ProfitType.year)
                    {
                        SortedDictionary<string, decimal> orderTotal = new();

                        for (int i = 0; i < json.Count; i++)
                        {
                            TotalOrders++;

                            string dateString = json[i]["dateCreated"].ToString();
                            DateTime orderCreatedDate = DateTime.ParseExact(dateString, "dd/MM/yyyy", null);
                            var yearCreated = orderCreatedDate.Year.ToString();
                            if (orderTotal.ContainsKey(yearCreated))
                            {
                                orderTotal[yearCreated] += decimal.Parse(json[i]["total"].ToString());
                            }
                            else orderTotal[yearCreated] = decimal.Parse(json[i]["total"].ToString());
                            var jsonArray = json[i]["orderDetail"].AsArray();
                            foreach (var item in jsonArray)
                            {
                                var amount = int.Parse(item["amount"].ToString());
                                TotalBooks += amount;

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

                        foreach (var item in orderTotal.Values)
                        {
                            decimals.Add(item);
                        }


                        ProfitCollection = new SeriesCollection {
                                new ColumnSeries{
                                    DataLabels = true,
                                    Values = decimals,
                                    Title = "Doanh thu theo năm"
                                }
                            };

                        OnPropertyChanged(nameof(ProfitCollection));


                        XTitle = "Năm";
                        XLabels = new List<string>(orderTotal.Keys);

                        OnPropertyChanged(nameof(XTitle));
                        OnPropertyChanged(nameof(XLabels));
                    }
                    else // weekly
                    {
                        Dictionary<string, decimal> weeklyProfit = new();
                        var firstDate = currentDate.AddDays(-14);
                        for (var i = firstDate; i <= currentDate; i = i.AddDays(1))
                        {
                            weeklyProfit[i.ToString("dd/MM")] = 0;
                        }
                        for (int i = 0; i < json.Count; i++)
                        {
                            string dateString = json[i]["dateCreated"].ToString();
                            DateTime orderCreatedDate = DateTime.ParseExact(dateString, "dd/MM/yyyy", null);
                            if (DateTime.Compare(firstDate, orderCreatedDate) <= 0 && DateTime.Compare(orderCreatedDate, currentDate) <= 0)
                            {
                                TotalOrders++;

                                if (weeklyProfit.ContainsKey(orderCreatedDate.ToString("dd/MM")))
                                {
                                    weeklyProfit[orderCreatedDate.ToString("dd/MM")] += decimal.Parse(json[i]["total"].ToString());
                                }
                                else
                                {
                                    weeklyProfit[orderCreatedDate.ToString("dd/MM")] = decimal.Parse(json[i]["total"].ToString());
                                }
                                var jsonArray = json[i]["orderDetail"].AsArray();
                                foreach (var item in jsonArray)
                                {
                                    var amount = int.Parse(item["amount"].ToString());
                                    TotalBooks += amount;

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

                        foreach (var item in weeklyProfit.Values)
                        {
                            decimals.Add((int)item);
                        }


                        ProfitCollection = new SeriesCollection {
                                new ColumnSeries{
                                    DataLabels = true,
                                    Values = decimals,
                                    Title = "Doanh thu tuần này"
                                }
                            };

                        OnPropertyChanged(nameof(ProfitCollection));

                        List<string> labels = new();
                        foreach (var day in weeklyProfit.Keys)
                        {
                            if (weeklyProfit[day] != 0)
                            {
                                labels.Add(day.ToString());
                            }
                            else
                            {
                                labels.Add("");
                            }
                        }

                        XTitle = "Ngày (" + firstDate.ToString("dd/MM") + " - " + currentDate.ToString("dd/MM") + ")";
                        XLabels = labels;

                        OnPropertyChanged(nameof(XTitle));
                        OnPropertyChanged(nameof(XLabels));
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
                                Values = new ChartValues<int>() { value }
                            }
                         );
                    }
                }
                catch (Exception ex)
                {
                    var b = 50;
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
