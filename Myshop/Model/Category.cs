using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Myshop.Model
{
    public class Category : INotifyPropertyChanged
    {
        private string name;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
