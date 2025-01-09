using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Midas.Core.Constants;
using Midas.Persistence.Models;

namespace Midas.Domain.Accounts.Models
{
    public class AccountDetails
    {
        public required string Name { get; set; }
        public required bool IsIncludedInNetWorth { get; set; }
        public decimal Balance { get; set; }

        public static AccountDetails FromAccountDto(Optional<AccountDto> account, decimal balance)
        {
            return new AccountDetails
            {
                Name = account.As(x => x.Name).Or(MidasConstants.DefaultAccountName),
                IsIncludedInNetWorth = account.As(x => x.IsMine).Or(false),
                Balance = balance
            };
        }
    }
}