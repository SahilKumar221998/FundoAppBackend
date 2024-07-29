using BusinessLayer.Interface;
using BusinessLayer.Service;
using CommonLayers.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NUnit.Framework;
using RepositoryLayers.Context;
using RepositoryLayers.Entity;
using System.Text;

namespace Fundo_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INotesBL notesBL;
        private readonly ILogger logger;
        private readonly IDistributedCache distributedCache;
        private readonly UserContext userContext;
        private readonly IMemoryCache memoryCache;
        private readonly long noteId;

        public NotesController(UserContext userContext,INotesBL notesBL, IMemoryCache memoryCache, IDistributedCache distributedCache, ILogger<NotesController> logger)
        {

            this.userContext= userContext;
            this.notesBL = notesBL;
            this.memoryCache = memoryCache;
            this.distributedCache = distributedCache;
            this.logger = logger;

        }

 
        
        [HttpPost]
        [Route("CreateNote")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult createNote(NotesModel notesModel)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
                var result = notesBL.createNotes(notesModel, userId);
                if (result != null)
                {
                    logger.LogInformation($"Notes Created Sucessfully {result}");
                    return Ok(new { success = true, message = "Notes Created Sucessfully", data = result });
                }
                else
                {
                    logger.LogInformation("Notes Creation Unsuccessfull");
                    return BadRequest(new { success = false, message = "Notes Creation Unsuccessfull" });
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning("Notes Creation did not executed successfully",ex);
                throw;
            }
        }
       
        [HttpGet]
        [Route("RetriveNotes")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult retriveNotes(long noteId)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
                var result = notesBL.retriveNotes(noteId, userId);
                if (result != null)
                {
                    logger.LogInformation($"retrive notes: {result}");  
                    return Ok(new { success = true, message = "Retrive Data Successfull", data = result });

                }
                else
                {
                    logger.LogInformation($"retrive notes Unsuccessfull: {noteId}");  
                    return BadRequest(new { success = false, message = "Retrive Data Unsuccessfull" });

                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"{ex.Message}",ex);    
                throw;
            }
        }
       
        [HttpGet]
        [Route("RetriveAll")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult retriveAllNotes()
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
                var result = notesBL.retriveAllNotes(userId);
                if (result != null)
                {
                    logger.LogInformation($"retrive all notes {userId}");    
                    return Ok(new { success = true, message = "Retrive Data Successfull", data = result });
                }
                else
                {
                    logger.LogInformation($"retrive notes unsuccessfull {userId}");
                    return BadRequest(new { success = false, message = "Retrive Data Unsuccessfull" });
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"{ex.Message}", ex);
                throw;
            }
        }
        
        [HttpPut]
        [Route("updateNotes")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult updateNotes(long notesId,NotesModel model)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
                var result = notesBL.updateNotes(userId, notesId, model);
                if (result != null)
                {
                    logger.LogInformation($"Data Updated successfully for {userId}");
                    return Ok(new { success = true, message = "Data Updated Successfully", data = result });
                }
                else
                {
                    logger.LogInformation($"Data Updated Unsuccessfull for {userId}");
                    return BadRequest(new { success = false, message = "Update Unsuccessfull" });
                }
            }
            catch (Exception ex) {
                logger.LogWarning($"{ex.Message}", ex);
                throw;
            }
        }
       
        [HttpDelete]
        [Route("Delete")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteNote(long noteId)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
                var result =notesBL.deleteNote(userId, noteId);

                if (result != null)
                {
                    logger.LogInformation($"Deletion of note for Id {userId} is Successfull");
                    return Ok(new { success = true, message = "Deletion Successful", data = result });
                }
                else
                {
                    logger.LogInformation($"Deletion of note for Id {userId} is Unsuccessfull");
                    return BadRequest(new { success = false, message = "Deletion UnSuccessful" });
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"{ex.Message}", ex);
                throw;
            }
        }
       
        [HttpPut]
        [Route("Pin")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //HttpPut method is used to update the notes when pinned.
        public IActionResult PinNote(long noteId)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
                var result = notesBL.pinNote(noteId, userId);

                if (result != null)
                {
                    logger.LogInformation("Note pinned");
                    return Ok(new { success = true, message = "Note Pinned Successful " });
                }
                else
                {
                    logger.LogInformation("Note Pinned Unsuccessfull");
                    return BadRequest(new { success = false, message = "Note Pinned UnSuccessful" });
                }
            }
            catch (Exception e)
            {
                logger.LogWarning($"{e.Message}", e);
                throw e;
            }
        }
        [HttpPut]
        [Route("Archieve")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ArchieveNote(long noteId)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
                var result =notesBL.archiveNote(userId,noteId);

                if (result != null)
                {
                    logger.LogInformation($"Note Archived:- {userId}");
                    return Ok(new { success = true, message = "Note Archieved Successfully " });
                }
                else
                {
                    logger.LogInformation($"Note not Archived:- {userId}");
                    return BadRequest(new { success = false, message = "Note Archieve UnSuccessful" });
                }
            }
            catch (Exception e)
            {
                logger.LogWarning($"{e.Message}", e);
                throw e;
            }
        }

        [HttpPut]
        [Route("Trash")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult TrashNote(long noteId)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
                var result =notesBL.TrashNote(noteId, userId);

                if (result != null)
                {
                    logger.LogInformation($"Notes Trashed:-{userId}");
                    return Ok(new { success = true, message = "Note Moved to Bin Successfully " });
                }
                else
                {
                    logger.LogInformation($"Notes not Trashed:-{userId}");
                    return BadRequest(new { success = false, message = "Note not Moved to Bin" });
                }
            }
            catch (Exception e)
            {
                logger.LogWarning($"{e.Message}", e);
                throw e;
            }
        }
       
        [HttpPut]
        [Route("Image")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ImageNotes(IFormFile Image, long noteId)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
                var result =notesBL.ImageNotes(Image, noteId, userId);
                if (result != null)
                {
                    logger.LogInformation($"Image Added :-{userId}");
                    return Ok(new { success = true, message = "Image Added Successfully " });
                }
                else
                {
                    logger.LogInformation($"Image Added Unsuccessfull:-{userId}");
                    return BadRequest(new { success = false, message = "Image Addition UnSuccessful" });
                }
            }
            catch (Exception e)
            {
                logger.LogWarning(e.Message, e);    
                throw e;
            }
        }
       
        [HttpPut]
        [Route("Color")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult BackgroundColorNote(long noteId, string backgroundcolor)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
                var result =notesBL.BackgroundColorNote(noteId, backgroundcolor);

                if (result != null)
                {
                    logger.LogInformation($"Background color note {result}");   
                    return Ok(new { success = true, message = "Background Color Changed Successfully " });
                }
                else
                {
                    logger.LogInformation($"Background Color Change Unsuccessful:-{userId}");
                    return BadRequest(new { success = false, message = "Background Color Change Unsuccessful" });
                }

            }
            catch (Exception e)
            {
                logger.LogWarning($"{e.Message}",e);
                throw;
            }

        }
       
        [HttpGet]
        [Route("Redis")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //Redis Implementation 
        public async Task<IActionResult> GetAllNotesUsingRedisCache()
        {
            //both lines should be used to get all the data
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(e => e.Type == "UserId").Value);
            var result =notesBL.retriveNotes(noteId,userId);

            var cacheKey = "NotesList";
            string serializedNotesList;
            var NotesList = new List<NotesEntity>();
            var redisNotesList = await distributedCache.GetAsync(cacheKey);
            if (redisNotesList != null)
            {
                serializedNotesList = Encoding.UTF8.GetString(redisNotesList);
                NotesList = JsonConvert.DeserializeObject<List<NotesEntity>>(serializedNotesList);
            }
            else
            {
                NotesList = userContext.tblNotes.ToList();
                serializedNotesList = JsonConvert.SerializeObject(NotesList);
                redisNotesList = Encoding.UTF8.GetBytes(serializedNotesList);
                var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));
                await distributedCache.SetAsync(cacheKey, redisNotesList, options);
            }
            return Ok(NotesList);
        }
    }
}

