using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Myshop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Myshop.ViewModel
{
    
    public class DashBoardViewModel: ViewModelBase
    {
        private SeriesCollection profitCollection;
        public SeriesCollection ProfitCollection { get { return profitCollection; } set {
                profitCollection = value;
                OnPropertyChanged(nameof(ProfitCollection));
            } }


        private SeriesCollection topSoldCollection;
        public SeriesCollection TopSoldCollection
        {
            get { return topSoldCollection; }
            set { 
                topSoldCollection = value;
                OnPropertyChanged(nameof(TopSoldCollection));
            }
        }
        public DashBoardViewModel()
        {
            ProfitCollection = new SeriesCollection()
            {
                new LineSeries
                {
                    Values = new ChartValues<double>
                    {
                        3,4,5,6
                    },
                    Stroke= System.Windows.Media.Brushes.Blue,
                    Fill = System.Windows.Media.Brushes.Red
                },new ColumnSeries
                {
                    Values = new ChartValues<ObservableValue>
                    {
                       
                    },
                    Stroke= System.Windows.Media.Brushes.Orange,
                    Fill = System.Windows.Media.Brushes.Gray,
                    StrokeDashArray = new DoubleCollection{2}
                }  
            };

            TopSoldCollection = new SeriesCollection
            {
                new PieSeries
                {
                    Values = new ChartValues<decimal>{1, 3, 4, 5}
                }
            };
        }

        public async Task getNumBook()
        {
 
        }

        public async Task getNumOrder()
        {

        }
    }
}
