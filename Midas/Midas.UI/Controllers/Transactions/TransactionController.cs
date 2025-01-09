using Microsoft.AspNetCore.Mvc;
using Midas.Domain.Transactions.Services;
using Midas.UI.Shared.Controllers;

namespace Midas.UI.Controllers.Transactions
{
    [Route("transaction")]
    public class TransactionController(ITransactionService transactionService) : MidasUIController
    {
        [HttpDelete("{id}")]
        public async Task<IResult> DeleteTransaction(int id)
        {
            await transactionService.DeleteTransaction(id);
            return Results.NoContent();
        }
    }
}
