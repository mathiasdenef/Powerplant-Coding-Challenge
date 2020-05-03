using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Business.Interfaces;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Powerplant_Coding_Challenge.Hubs;

namespace Powerplant_Coding_Challenge.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductionPlanController : ControllerBase
    {
        private readonly IProductionPlanService _productionPlanService;
        private readonly IHubContext<ProductionPlanHub> _productionPlanHub;

        public ProductionPlanController(IProductionPlanService productionPlanService, IHubContext<ProductionPlanHub> productionPlanHub)
        {
            _productionPlanService = productionPlanService;
            _productionPlanHub = productionPlanHub;
        }

        [HttpPost]
        public ActionResult<ProductionPlan> GetProductionPlanByPayload([FromBody] Payload payload)
        {
            try
            {
                var productionPlan = _productionPlanService.GetProductionPlanForPayload(payload);
                _productionPlanHub.Clients.All.SendAsync("ReceiveProductionPlan", productionPlan);
                return productionPlan;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
