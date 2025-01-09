using Microsoft.AspNetCore.Mvc;
using SpendLess.Domain.Transactions.Services;
using SpendLess.UI.Shared.Controllers;

namespace SpendLess.UI.Controllers.Transactions
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
