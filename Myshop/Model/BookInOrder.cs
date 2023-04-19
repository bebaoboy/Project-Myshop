using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace Myshop.Model
{
    public class BookInOrder
    {
        private string id;
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        private int amount;
        public int Amount
        {
            get; set;
        }

        private string title;
        public string Title
        {
            get; set;
        }


        private double price;   
        public double Price
        {
            get { return price; }
            set { price = value; }
        }

        public BookInOrder(JsonObject ob, int amount)
        {
            try
            {
                Amount = amount;
                Title = ob["title"].ToString();
                Price = double.Parse(ob["price"].ToString()); 
            }catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
