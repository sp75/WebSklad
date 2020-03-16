using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebSklad.Models
{
    public abstract class CustomerPage : System.Web.UI.Page
    {
        public SPBaseModel _db { get; set; }
        public int _ka_id { get; set; }
        public bool is_sp_sklad_user { get; set; }
        public int? _user_id { get; set; }

        public CustomerPage()
        {
            _db = Database.SPBase();

            var uid = _db.AspNetUsers.FirstOrDefault(w => w.UserName == User.Identity.Name).Id;
            var kagent = _db.Kagent.FirstOrDefault(w => w.AspNetUserId == uid);

            _ka_id = kagent.KaId;
            is_sp_sklad_user = kagent.UserId.HasValue;
            _user_id = kagent.UserId;
        }
    }
}