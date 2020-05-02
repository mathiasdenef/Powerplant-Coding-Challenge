using Domain.Enums;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Domain
{
    public class ProductionPlan
    {
        public ProductionPlan()
        {
            PowerplantDeliveries = new Collection<PowerplantDelivery>();
        }

        public IList<PowerplantDelivery> PowerplantDeliveries { get; set; }

        public void AddPowerplantDelivery(string name, int p)
        {
            PowerplantDeliveries.Add(new PowerplantDelivery()
            {
                Name = name,
                P = p
            });
        }
    }
}
