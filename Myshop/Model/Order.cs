using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myshop.Model
{
    public class Order
    {

        private string dateCreated;
        public string DateCreated 
        {
            get { return dateCreated; } 
            set {  dateCreated = value; }
        }

        private List<BookInOrder> _books = new List<BookInOrder>();

        public List<BookInOrder> OrderedBook
        {
            get { return _books; }
            set { _books = value; }
        }

        private double _totalPrice;
        public double TotalPrice
        {
            get { return _totalPrice; }
            set { _totalPrice = value; }
        }
    }
}
