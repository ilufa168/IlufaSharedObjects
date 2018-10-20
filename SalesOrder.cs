using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace IlufaSharedObjects
{
    public struct SalesTotals
    {
        public double total_cost;
        public double total_qty;

        public SalesTotals(double cost = 0,double qty=0)
        {
            total_cost = cost;
            total_qty = qty;
        }
    }
    public class SalesOrder : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private string _sales_id;
        private string _item_id;
        private string _description;
        private Int32  _quantity;
        private double _unit_price;
        private double _discount;
        private string _discount_desc;
        private string _discount_id;
        public bool skip_notify = false;

        public static SalesTotals GetSalesTotal(BindingList<SalesOrder> all)
        {
            SalesTotals tot = new SalesTotals(0,0);
            
            foreach (SalesOrder so in all)
            {
                tot.total_cost += so.total;
                tot.total_qty += so.quantity;
            }
            return tot;
        }

        public void CopyMeWithNewQty(SalesOrder source, int qty)
        {
            this.sales_id = source.sales_id;
            this.item_id = source.item_id;
            this.description = source.description;
            this.quantity = qty;
            this.unit_price = source.unit_price;
            this.discount = source.discount;
            this.discount_desc = source.discount_desc;
            this.discount_id = source.discount_id;
            this.category = source.category;
            this.category_description = source.category_description;
        }

        private double _total{
            get
            {
                return _quantity * (_unit_price - _discount);
            }
        }
        private string _category;
        private string _category_description;

        public string category
        {
            get { return _category; }
            set
            {
                _category = value;
                this.NotifyPropertyChanged("category");
            }
        }

        public string category_description
        {
            get { return _category_description; }
            set
            {
                _category_description = value;
                this.NotifyPropertyChanged("category_description");
            }
        }

        public string discount_desc
        {
            get { return _discount_desc; }
            set
            {
                _discount_desc = value;
                if (!skip_notify)
                    this.NotifyPropertyChanged("discount_desc");
            }
        }

        public string sales_id
        {
            get { return _sales_id; }
            set
            {
                _sales_id = value;
                this.NotifyPropertyChanged("sales_id");
            }
        }
 
        public string item_id
        {
            get { return _item_id; }
            set
            {
                _item_id = value;
                this.NotifyPropertyChanged("item_id");
            }
        }

        public string description
        {
            get { return _description; }
            set
            {
                _description = value;
                if (_description != null)
                    this.NotifyPropertyChanged("description");
            }
        }

        public string discount_id
        {
            get { return _discount_id; }
            set
            {
                _discount_id = value;
                if (!skip_notify)
                    this.NotifyPropertyChanged("discount_id");
            }
        }

        public Int32 quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                this.NotifyPropertyChanged("quantity");
                this.NotifyPropertyChanged("total");
           }
        }

        public double unit_price
        {
            get { return _unit_price; }
            set
            {
                _unit_price = value;
                this.NotifyPropertyChanged("unit_price");
                this.NotifyPropertyChanged("total");
            }
        }

        public double discount
        {
            get { return _discount; }
            set
            {
                _discount = value;
                if (!skip_notify)
                    this.NotifyPropertyChanged("discount");
            }
        }

        public double total
        {
            get { return _total; }
//            set
//            {
//                _total = value;
//                this.NotifyPropertyChanged("total");
//            }
        }
        //Add an item to the sales order list
        //Usually called from the main form
        public void AddItem(item new_item)
        {

            this.item_id = new_item.item_id;
            this.description = new_item.description;
            this.quantity = 1;
            this.unit_price = new_item.price;
//            this.discount = 0;
            this.category = new_item.category;
            this.category_description = new_item.category_description;
//            SO_item_list.Add(so_item);
            return;

        }

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

    }
}
