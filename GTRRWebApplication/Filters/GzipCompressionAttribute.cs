using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace GTRRWebApplication.Filters
{
    public class GzipCompressionAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            
            var executedContext = await next();

           
            if (executedContext.Result is ObjectResult objectResult &&
                objectResult.Value != null &&
                objectResult.StatusCode >= 200 &&
                objectResult.StatusCode < 300)
            {
               
                var json = System.Text.Json.JsonSerializer.Serialize(objectResult.Value);
                var bytes = Encoding.UTF8.GetBytes(json);

             
                var compressedBytes = CompressionHelper.GzipBytes(bytes);

               
                var fileResult = new FileContentResult(compressedBytes, "application/json");

              
                context.HttpContext.Response.Headers["Content-Encoding"] = "gzip";

                executedContext.Result = fileResult;
            }
         
            else if (executedContext.Result is FileContentResult fileContent)
            {
                var compressedBytes = CompressionHelper.GzipBytes(fileContent.FileContents);

                
                var compressedResult = new FileContentResult(compressedBytes, fileContent.ContentType)
                {
                    FileDownloadName = fileContent.FileDownloadName,
                    LastModified = fileContent.LastModified,
                    EntityTag = fileContent.EntityTag
                };

                context.HttpContext.Response.Headers["Content-Encoding"] = "gzip";
                executedContext.Result = compressedResult;
            }
        }
    }
}