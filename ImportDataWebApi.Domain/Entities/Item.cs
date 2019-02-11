using System;
using System.Collections.Generic;
using System.Text;

namespace ImportDataWebApi.Domain.Entities
{
    public class Item
    {
        public int Item_Id { get; set; } // Id do item
        public int Item_Qty { get; set; } //Quantidade de item
        public decimal Price { get; set; } // Valor do produto
    }
}
