using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class CollabController : ControllerBase
    {
        private readonly ICollabBL collabBL;
        private readonly UserContext userContext;
        private readonly IMemoryCache memoryCache;  
        private readonly ILogger<NotesController> logger;
        private readonly IDistributedCache distributedCache;

        public CollabController(ICollabBL collabBL, UserContext userContext, IMemoryCache memoryCache, ILogger<NotesController> logger, IDistributedCache distributedCache)
        {
            this.collabBL = collabBL;
            this.userContext = userContext;
            this.memoryCache = memoryCache;
            this.logger = logger;
            this.distributedCache = distributedCache;
        }

        [HttpPost]
        [Route("CollabCreate")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CreateCollab(long noteId,string emailTo)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "UserId").Value);
                var emailFrom = User.Claims.FirstOrDefault(x => x.Type.Equals("Email")).ToString();
                var result = collabBL.CreateCollab(noteId, emailTo,emailFrom);
                if(result!=null)
                {
                    logger.LogInformation($"Collab created Successfully {result}");
                    return Ok(new { success = true, message = "Collab created Successfully", data = result });
                }
                else
                {
                    logger.LogInformation($"Collab creation in unsuccesssfull with {emailFrom} and ID {noteId}");
                    return BadRequest(new { success = false, message = "Collab creation in unsuccesssfull" });
                }
            }
            catch (Exception ex) {
                logger.LogWarning(ex.Message, ex);
                throw;
            }

        }
        
        [HttpGet]
        [Route("RetriveCollab")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult RetriveCollab(long collbId)
        {
            try
            {
                var result=collabBL.RetriveCollab(collbId);
                if(result!=null) {
                    logger.LogInformation($"RetriveCollab: {result}");
                    return Ok(new { success = true, message = "Collab Retrived", data = result });
                }
                else
                {
                    logger.LogInformation($"RetriveCollab Unsuccessfull: {collbId}");
                    return Ok(new { success = true, message = "Collab not Retrived"});
                }
            }
            catch (Exception ex) {
                logger.LogWarning(ex.Message, ex);
                throw;
            }
        }
       
        [HttpDelete]
        [Route("Delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteCollab(long collabId)
        {
            try
            {
                long userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);

                var result =collabBL.DeleteCollab(collabId, userId);

                if (result != null)
                {
                    logger.LogInformation($"Collab Deletd:{collabId}");
                    return Ok(new { success = true, message = "Data Deleted Successful" });
                }
                else
                {
                    logger.LogInformation($"Collab Deletion Unsuccessfull with id {collabId}");
                    return BadRequest(new { success = false, message = "Data Deletetion UnSuccessful" });
                }
            }
            catch(Exception ex)
            {
                logger.LogWarning(ex.Message, ex);
                throw;
            }
        }
       
        [HttpGet]
        [Route("Redis")]
        [Authorize]
        //Redis Implementation 
        public async Task<IActionResult> GetAllCollabUsingRedisCache()
        {
            long userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
            //var result = icollabBL.CreateCollab(notesId, email);

            var cacheKey = "CollabList";
            string serializedCollabList;
            var CollabList = new List<CollabEntity>();
            var redisCollabList = await distributedCache.GetAsync(cacheKey);
            if (redisCollabList != null)
            {
                serializedCollabList = Encoding.UTF8.GetString(redisCollabList);
                CollabList = JsonConvert.DeserializeObject<List<CollabEntity>>(serializedCollabList);
            }
            else
            {
                CollabList =userContext.tblCollab.ToList();
                serializedCollabList = JsonConvert.SerializeObject(CollabList);
                redisCollabList = Encoding.UTF8.GetBytes(serializedCollabList);
                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));
                await distributedCache.SetAsync(cacheKey, redisCollabList, options);
            }
            return Ok(CollabList);
        }
    }
}
