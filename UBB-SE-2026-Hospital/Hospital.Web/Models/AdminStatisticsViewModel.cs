namespace Hospital.Web.Models
{
    using System.Collections.Generic;

    public class AdminStatisticsViewModel
    {
        public List<TopItemViewModel> TopItems { get; set; } = new List<TopItemViewModel>();

        public Dictionary<string, int> TopSubstances { get; set; } = new Dictionary<string, int>();
    }
}
