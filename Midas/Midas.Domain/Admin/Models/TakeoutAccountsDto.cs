namespace Midas.Domain.Admin.Models
{
    public class TakeoutAccountsDto
    {
        public int Version { get; set; } = 0;
        public Dictionary<string, TakeoutAccountDto> Accounts { get; set; } = [];
    }

    public class TakeoutAccountDto
    {
        public required string Name { get; set; }
        public required bool IsMine { get; set; }
    }

}
