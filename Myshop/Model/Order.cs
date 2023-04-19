using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myshop.Model
{
    public class Order
    {
        private int id;
        public int Id { get; set; }

        private string customerName;
        public string CustomerName
        {
            get { return customerName; }
            set { customerName = value; }
        }

        private string phoneNumber;
        public string PhoneNumber
        {
            get;set;
        }

        private string address;
        public string Address
        {
            get;set;
        }

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

        private double total;
        public double Total
        {
            get { return total; }
            set {  total = value; }
        }
    }
}
