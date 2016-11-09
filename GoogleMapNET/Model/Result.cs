using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleMapNET.Model
{
    public class Result
    {
        public Geometry geometry { get; set; }
        public string icon { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string placeId { get; set; }
        public string reference { get; set; }
        public string scope { get; set; }
        public List<Type> type { get; set; }
        public string vicinity { get; set; }

    }
}
