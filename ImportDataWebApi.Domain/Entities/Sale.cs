using System;
using System.Collections.Generic;
using System.Text;

namespace ImportDataWebApi.Domain.Entities
{
    public class Sale
    {
        public int Sale_Id { get; set; }
        public List<Item> Items { get; set; }
        public String SalesMan_Name { get; set; }
        public decimal TotalPrice {get;set;}
    }
}
