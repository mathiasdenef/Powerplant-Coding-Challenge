using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Business.Interfaces;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Powerplant_Coding_Challenge.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PayloadController : ControllerBase
    {
        private readonly IProductionPlanService _productionPlanService;

        public PayloadController(IProductionPlanService productionPlanService)
        {
            _productionPlanService = productionPlanService;
        }

        // POST: api/Payload
        [HttpPost]
        public ProductionPlan GetProductionPlanByPayload([FromBody] Payload payload)
        {
            var productionPlan = _productionPlanService.GetProductionPlanByPayload(payload);
            return productionPlan;
        }

        // GET: api/Payload
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Payload/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Payload
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        // PUT: api/Payload/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
