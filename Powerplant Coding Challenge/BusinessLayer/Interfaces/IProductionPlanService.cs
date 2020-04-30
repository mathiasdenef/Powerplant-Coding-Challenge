using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Interfaces
{
    public interface IProductionPlanService
    {
        ProductionPlan GetProductionPlanByPayload(Payload payload);
    }
}
