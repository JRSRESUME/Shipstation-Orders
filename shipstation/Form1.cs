using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using shipstationtest4.shipstation;
using System.Threading;
using System.Data.Services.Client;
using shipstationtest4.com.channeladvisor.api;

namespace shipstationtest4
{


    public partial class Form1 : Form
    {
        private List<OrderItem> _CurrentOrderList;
        private int _CurrentOrderIndex;
        int oid;

         

        [Serializable]
        public class DisplayOrder
        {
            public int OrderID { get; set; }
            public string Description { get; set; }
            public string SKU { get; set; }
            public string WarehouseLocation { get; set; }
            public string ThumbnailUrl { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Shipping { get; set; }
            public int OrderNumber { get; set; }
            public string Store { get; set; }
            public string Country { get; set; }
            public decimal Total { get; set; }
            public int StoreID { get; set; }


        }

        public Form1()
        {
            InitializeComponent();

            

        }



        public void button1_Click(object sender, EventArgs e)
        {
            ShipStationEntities entities = new ShipStationEntities(new Uri("http://data.shipstation.com/1.2"));
            entities.Credentials = new System.Net.NetworkCredential("eewholesale", "kennyb15");

            try
            {
               
                var query = (from item in entities.OrderItems.Expand("Order").Expand("Order/OrderItems")
                             where item.WarehouseLocation.StartsWith(start.Text) && item.Order.OrderStatusID == 2
                             select item) as DataServiceQuery<OrderItem>;

                var response = query.Execute() as QueryOperationResponse<OrderItem>;

                var orderList = new List<OrderItem>(response);

                DataServiceQueryContinuation<OrderItem> token;
                while ((token = response.GetContinuation()) != null)
                {
                    // The API returned a token for retrieving additional records, request the next page now:
                    response = entities.Execute<OrderItem>(token) as QueryOperationResponse<OrderItem>;
                    orderList.AddRange(response);
                }

                var filteredList = orderList.Where(x => x.Order.OrderItems.Count == 1).ToList();
                var sortedList = filteredList.OrderBy(x => x.WarehouseLocation).ToList();

                _CurrentOrderList = sortedList;
                _CurrentOrderIndex = 0;

                FillFormWithOrder(_CurrentOrderList, _CurrentOrderIndex);
            }
            catch
            {
                MessageBox.Show("Query Failed");
            }
        }

        private void FillFormWithOrder(List<OrderItem> orderList, int orderIndex)
        {

            try
            {

                if (orderList.Count == 0)
                {
                    MessageBox.Show("No items found");
                    return;
                }

                OrderItem orderItem = orderList[orderIndex];

                APICredentials cred = new APICredentials();
                cred.DeveloperKey = "2476c33f-a06b-41f7-8af3-3f337bbfdca6";
                cred.Password = "kennyb15";

                InventoryService svc = new InventoryService();
                svc.APICredentialsValue = cred;


                APIResultOfQuantityInfoResponse result = svc.GetInventoryItemQuantityInfo("afe6a7ba-dd0c-4292-827d-4385fba66ae2", orderItem.SKU);

                shipcountry.Text = orderItem.Order.ShipCountryCode;
                total.Text = orderItem.Order.OrderTotal.ToString();
                store.Text = orderItem.Order.Source;
                order.Text = orderItem.Order.OrderNumber;
                shipping.Text = orderItem.ShippingAmount.Value.ToString();
                price.Text = orderItem.UnitPrice.ToString();
                title.Text = orderItem.Description;
                sku.Text = orderItem.SKU;
                location.Text = orderItem.WarehouseLocation;
                picture1.ImageLocation = orderItem.ThumbnailUrl;
                quantity.Text = orderItem.Quantity.ToString();
                qtyleft.Text = result.ResultData.Available.ToString();
                oid = orderItem.OrderID;
                

                bin.Focus();
            }
            catch
            {
                if (orderIndex <= 0)
                {
                    orderIndex = 0;
                }

            }

        }

        public void next_Click(object sender, EventArgs e)
        {
            ShipStationEntities entities = new ShipStationEntities(new Uri("http://data.shipstation.com/1.2"));
            entities.Credentials = new System.Net.NetworkCredential("eewholesale", "kennyb15");
            var orderToUpdate = entities.Orders.Where(x => x.OrderID == oid).FirstOrDefault();

            orderToUpdate.CustomField1 = bin.Text;

            entities.UpdateObject(orderToUpdate);
            entities.SaveChanges();
            bin.Clear();


            if (_CurrentOrderList != null && _CurrentOrderList.Count > 0)
            {
                _CurrentOrderIndex = (_CurrentOrderIndex + 1) % _CurrentOrderList.Count;
                FillFormWithOrder(_CurrentOrderList, _CurrentOrderIndex);
                bin.Clear();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void right_Click(object sender, EventArgs e)
        {
            if (_CurrentOrderList != null && _CurrentOrderList.Count > 0)
            {
                _CurrentOrderIndex = (_CurrentOrderIndex + 1) % _CurrentOrderList.Count;
                FillFormWithOrder(_CurrentOrderList, _CurrentOrderIndex);
                bin.Clear();
            }
        }

        private void left_Click(object sender, EventArgs e)
        {
            if (_CurrentOrderList != null && _CurrentOrderList.Count > 0)
            {
                _CurrentOrderIndex = (_CurrentOrderIndex - 1) % _CurrentOrderList.Count;
                FillFormWithOrder(_CurrentOrderList, _CurrentOrderIndex);
                bin.Clear();
            }
        }

        

    }
}