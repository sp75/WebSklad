using System;
using System.Web.Http;
using WebApi.Core;

namespace WebApi.Controllers
{
 //   [MyAuthorize]
    public class TranzitBaseController : ApiController
    {
        private TranzitContext _context;
        public TranzitContext Context
        {
            get { return _context ?? (_context = new TranzitContext(Request)); }
        }


    }
}