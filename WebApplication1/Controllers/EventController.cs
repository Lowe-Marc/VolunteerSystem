using MVCFullCalendarDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class EventController : ApiController
    {
        private EventContext db = new EventContext();

        // GET api/Event
        public IQueryable GetEvents()
        {
            return db.Events;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
