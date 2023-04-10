using Myshop.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Myshop.ViewModel
{
    
    public class DashBoardViewModel: ViewModelBase
    {
        private readonly CollectionViewSource HomeItemsCollection;
        public ICollectionView HomeSourceCollection => HomeItemsCollection.View;

        public DashBoardViewModel()
        {
            ObservableCollection<HomeItems> homeItems = new ObservableCollection<HomeItems>
            {
                new HomeItems { HomeName = "This PC", HomeImage = @"images/back-image.png" },
            };

            HomeItemsCollection = new CollectionViewSource { Source = homeItems };

        }
    }
}
