using SP.Base;
using SP.Base.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Core
{
    public class BaseRepository
    {
        public SPBaseModel db => SPDatabase.SPBase();
        public int system_user_id => 1320;
    }
}