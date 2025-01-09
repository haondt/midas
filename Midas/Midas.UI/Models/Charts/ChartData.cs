namespace Midas.UI.Models.Charts
{
    public class ChartData
    {
        /// <summary>
        /// Must contain the same amount of elements as the dataset with the most values.
        /// These labels are used to label the index axis (default x axes).
        /// </summary>
        public List<string>? Labels { get; set; }

        public List<ChartDataSet> Datasets { get; set; } = [];

    }
}
