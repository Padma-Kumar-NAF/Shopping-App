using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ShoppingApp.Filters
{
    public class MyResultFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            Console.WriteLine("Before Result Execution");
            //if (context.Result is ObjectResult objectResult)
            //{
            //    var originalData = objectResult.Value;

            //    objectResult.Value = new
            //    {
            //        status = "success",
            //        data = originalData
            //    };
            //}
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            Console.WriteLine("After Result Execution");
        }
    }
}
