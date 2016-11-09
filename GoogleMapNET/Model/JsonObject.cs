using System;
using System.Collections.Generic;

namespace GoogleMapNET.Model
{
    public class JsonObject
    {
        public string htmlAttributions { get; set; }
        public List<Result> results { get; set; }
        public string status { get; set; }
    }
}
