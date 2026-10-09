using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMSWeb.Models
{
    public class CustomerModel
    {
        public string customer_code { get; set; }
        public string customer_name_th { get; set; }
        public string customer_name_en { get; set; }
        public string customer_type_name { get; set; }
        public string customer_address { get; set; }
        public string customer_tax { get; set; }
    }
}
