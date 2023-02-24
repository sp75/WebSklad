using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSklad.Models
{
    public class BasePage : System.Web.UI.Page
    {
        public SPBaseModel _db { get; set; }

        public BasePage()
        {
            _db = Database.SPBase();
        }
    }
}