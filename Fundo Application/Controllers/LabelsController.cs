using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RepositoryLayers.Context;
using RepositoryLayers.Entity;
using System.Text;

namespace Fundo_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabelsController : ControllerBase
    {
        private readonly ILabelBL labelBL;
        private readonly UserContext userContext;
        private readonly IMemoryCache memoryCache;
        private readonly IDistributedCache distributed;
        private readonly ILogger<LabelsController> logger;

        public LabelsController(ILabelBL labelBL, UserContext userContext, IMemoryCache memoryCache, IDistributedCache distributed, ILogger<LabelsController> logger)
        {
            this.labelBL = labelBL;
            this.userContext = userContext;
            this.memoryCache = memoryCache;
            this.distributed = distributed;
            this.logger =logger;
        }
        
        [HttpPost]
        [Route("CreateLabel")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CreateLabel(long noteId, string labelName)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
                var result = labelBL.CreateLabel(noteId, userId, labelName);
                if (result != null)
                {
                    logger.LogInformation("Label Creation Successfull");
                    return Ok(new { success = true, Message = "Label Creation Successfull", data = result });
                }
                else
                {
                    logger.LogInformation($"Failed to create label: {labelName}");
                    return BadRequest(new { success = false, messsage = "Label Creation Unsuccessfull" });
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message, ex);
                throw ex;
            }
        }
       
        [HttpGet]
        [Route("Retrieve")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult RetrieveLabel(long labelId)
        {
            try
            {
                var result =labelBL.RetrieveLabel(labelId);

                if (result != null)
                {
                    logger.LogInformation($"Retrieved label: {result}");
                    return Ok(new { success = true, message = "Label Retrieve Successful ", data = result });
                }
                else
                {
                    logger.LogInformation($"Retrieved label Unsuccessfull: {labelId}");
                    return BadRequest(new { success = false, message = "Label Retrieve UnSuccessful" });
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message, ex);
                throw ex;
            }
        }
        
        [HttpGet]
        [Route("RetriveAll")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult RetrieveAllLabel()
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
                var result = labelBL.RetrieveAllLabel(userId);
                if (result != null)
                {
                    logger.LogInformation($"Label with id {userId} retrived Successfully");
                   return Ok(new { success = true, message = "Label Retrieve Successful ", data = result });
                }
                else
                {
                    logger.LogInformation($"Label with id {userId} retrive Unsuccessfull");
                    return BadRequest(new { success = false, message = "Label Retrieve UnSuccessful" });
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning (ex.Message, ex); 
                throw ex;
            }
        }
       
        [HttpDelete]
        [Route("Delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteLabel(long labelId)
        {
            try
            {

                var result =labelBL.DeleteLabel(labelId);

                if (result != null)
                {
                    logger.LogInformation($"Label with Id {labelId} deleted");
                    return Ok(new { success = true, message = "Label Deleted Successful " });
                }
                else
                {
                    logger.LogInformation($"Label with Id {labelId} not deleted");
                    return BadRequest(new { success = false, message = "Label Deletion UnSuccessful" });
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message, ex);  
                throw ex;
            }
        }
        
        
        [HttpPut]
        [Route("Edit")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult EditLabel(long noteId, string labelName)
        {
            try
            {
                var result =labelBL.EditLabel(noteId, labelName);

                if (result != null)
                {
                    logger.LogInformation($"Label Updated {result}");
                    return Ok(new { success = true, message = "Label Updated Successful ", data = result });
                }
                else
                {
                    logger.LogInformation($"Label not Updated {result}");
                    return BadRequest(new { success = false, message = "Label updation UnSuccessful" });
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning (ex.Message, ex); 
                throw ex;
            }
        }
       
        [HttpGet]
        [Route("Redis")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //Redis Implementation 
        public async Task<IActionResult> GetAllCollabUsingRedisCache()
        {
            long userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);

            var cacheKey = "LabelList";
            string serializedLabelList;
            var LabelList = new List<LabelEntity>();
            var redisLabelList = await distributed.GetAsync(cacheKey);
            if (redisLabelList != null)
            {
                serializedLabelList = Encoding.UTF8.GetString(redisLabelList);
                LabelList = JsonConvert.DeserializeObject<List<LabelEntity>>(serializedLabelList);
            }
            else
            {
                LabelList = userContext.tblLabels.ToList();
                serializedLabelList = JsonConvert.SerializeObject(LabelList);
                redisLabelList = Encoding.UTF8.GetBytes(serializedLabelList);

                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));
                await distributed.SetAsync(cacheKey, redisLabelList, options);
            }
            return Ok(LabelList);
        }

    }
}
