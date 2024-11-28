using Haondt.Web.Core.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace SpendLess.TransactionImport.Controllers
{
    [Route("transaction-import")]
    public class TransactionImportController : UIController
    //IPageComponentFactory pageFactory,
    //IComponentFactory componentFactory) : HxController
    //ITransactionImportConfigurationService configService) : BaseController
    {
        //[HttpGet]
        //public async Task<IResult> Get()
        //{

        //    //var component = await pageFactory.GetComponent<TransactionImportModel>();
        //    ////var x = D_.Documents.Projects.spend_less.SpendLess.SpendLess.TransactionImport.SpendLess.TransactionImport.RazorViews.
        //    ////var x = SpendLess.TransactionImport.
        //    //return component.CreateView(this);

        //}

        //[HttpGet("config")]
        //public async Task<IActionResult> GetConfig([FromQuery] TransactionImportUpsertConfigRequestDto dto)
        //{
        //    var config = await configService.


        //}


        //[HttpPost("{config}")]
        //public async Task<IActionResult> Upsert()
        //{


        //}

    }

    //public class Test : TransactionImportController
    //{
    //    [HttpGet]
    //    public async Task<IResult> Get()
    //    {
    //        return new RazorComponentResult<TestOne>(new TestOne
    //        {
    //            Color = "blue",
    //            Size = 100
    //        });
    //    }

    //}
}
