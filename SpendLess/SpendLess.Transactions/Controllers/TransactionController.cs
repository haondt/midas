using Microsoft.AspNetCore.Mvc;
using SpendLess.Transactions.Services;
using SpendLess.Web.Domain.Controllers;

namespace SpendLess.Transactions.Controllers
{
    [Route("transaction")]
    public class TransactionController(ITransactionService transactionService) : SpendLessUIController
    {
        [HttpDelete("{id}")]
        public async Task<IResult> DeleteTransaction(int id)
        {
            await transactionService.DeleteTransaction(id);
            return Results.NoContent();
        }
    }
}
