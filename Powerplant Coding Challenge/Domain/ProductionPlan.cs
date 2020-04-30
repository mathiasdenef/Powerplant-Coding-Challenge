using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class ProductionPlan
    {
        public ICollection<PowerplantNameAndPower> PowerplantNameAndPowers { get; set; }
    }
}
