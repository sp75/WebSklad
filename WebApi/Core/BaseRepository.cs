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
    }
}