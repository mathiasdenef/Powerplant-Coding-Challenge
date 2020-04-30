using System;
using System.Collections;
using System.Collections.Generic;

namespace Domain
{
    public class Payload
    {
        public int Load { get; set; }
        public Fuels Fuels { get; set; }
        public IList<Powerplant> Powerplants { get; set; }
    }
}
