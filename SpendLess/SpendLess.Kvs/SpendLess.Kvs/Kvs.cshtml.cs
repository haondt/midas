using Haondt.Core.Models;
using Haondt.Web.Core.Components;
using SpendLess.Kvs.Models;

namespace SpendLess.Kvs.SpendLess.Kvs
{
    public class KvsModel : IComponentModel
    {
        public bool WithLayout { get; set; } = false;
        public bool LaunchSelectModal { get; set; } = false;
        public bool CloseModal { get; set; } = false;
        public Optional<EditMappingModel> EditMapping { get; set; } = new();
    }

    public class EditMappingModel
    {
        public required string Key { get; set; }
        public List<string> Aliases { get; set; } = [];
        public string Value { get; set; } = "";

        public static EditMappingModel FromExpandedMappingDto(ExpandedKvsMappingDto expandedMapping)
        {
            return new EditMappingModel
            {
                Key = expandedMapping.Key,
                Value = expandedMapping.Value,
            };
        }
    }
}
