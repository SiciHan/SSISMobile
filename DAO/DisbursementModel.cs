using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Team8SSISMobile.DAO
{
    public class DisbursementModel
    {
        public List<ItemModel> items { get; set; }
        public string disbursementId { get; set; }
        public int id { get; set; }
    }

    public class ItemModel
    {
        public string description { get; set; }
        public string quantity { get; set; }
    }
}