using Haondt.Core.Models;
using SpendLess.Kvs.Models;

namespace SpendLess.Kvs.SpendLess.Kvs
{
    public class KvsModel
    {
        public const string AliasListViewPath = "~/SpendLess.Kvs/AliasList.cshtml";
    }

    public class KvsModalModel : KvsModel
    {

    }

    public class KvsContentModel : KvsModel
    {
        public required string Key { get; set; }
        public string Value { get; set; } = "";
        public List<string> Aliases { get; set; } = [];
        public static KvsContentModel FromExpandedMappingDto(ExpandedKvsMappingDto expandedMapping)
        {
            return new KvsContentModel
            {
                Key = expandedMapping.Key,
                Value = expandedMapping.Value,
                Aliases = expandedMapping.Aliases,
            };
        }
    }

    public class KvsLayoutModel : KvsModel
    {
        public Optional<KvsContentModel> Content { get; set; }
    }

    public class KvsInsertContentModel : KvsContentModel
    {
        public static new KvsInsertContentModel FromExpandedMappingDto(ExpandedKvsMappingDto expandedMapping)
        {
            return new KvsInsertContentModel
            {
                Key = expandedMapping.Key,
                Value = expandedMapping.Value,
                Aliases = expandedMapping.Aliases,
            };
        }
    }

    public class KvsUpdateContentModel : KvsModel
    {
        public Optional<string> Key { get; set; } = new();
        public Optional<string> Value { get; set; } = new();
        public Optional<List<string>> Aliases { get; set; } = new();
        public static KvsUpdateContentModel FromExpandedMappingDto(ExpandedKvsMappingDto expandedMapping)
        {
            return new KvsUpdateContentModel
            {
                Key = expandedMapping.Key,
                Value = expandedMapping.Value,
                Aliases = expandedMapping.Aliases,
            };
        }
    }
}
