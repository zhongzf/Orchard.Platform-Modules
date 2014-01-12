using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaisingStudio.SessionFactory.Models
{
    public class ConnectionsRecord
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Provider { get; set; }
        public virtual string ConnectionString { get; set; }
    }
}