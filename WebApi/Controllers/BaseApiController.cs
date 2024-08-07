﻿using SP.Base;
using SP.Base.Models;
using System;
using System.Web.Http;
using WebApi.Core;

namespace WebApi.Controllers
{
    public class BaseApiController : ApiController
    {
        private ApiTokenContext _context;
        public ApiTokenContext Context
        {
            get { return _context ?? (_context = new ApiTokenContext(Request)); }
        }

        public SPBaseModel db => SPDatabase.SPBase();
    }
}