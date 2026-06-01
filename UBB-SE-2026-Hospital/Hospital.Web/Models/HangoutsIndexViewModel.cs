namespace Hospital.Web.Models
{
    using System.Collections.Generic;

    public class HangoutsIndexViewModel
    {
        public List<HangoutViewModel> Hangouts { get; set; } = new List<HangoutViewModel>();

        public List<DoctorOptionViewModel> Doctors { get; set; } = new List<DoctorOptionViewModel>();
    }
}
