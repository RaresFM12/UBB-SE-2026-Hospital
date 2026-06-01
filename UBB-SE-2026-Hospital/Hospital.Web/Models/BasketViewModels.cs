namespace Hospital.Web.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Hospital.Shared.Models.Orders;

    public class BasketViewModel
    {
        public List<BasketItemViewModel> Items { get; set; } = new();

        public float TotalBeforeDiscount { get; set; }

        public float TotalAfterDiscount { get; set; }

        public string? PrescriptionId { get; set; }

        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class BasketQuantityViewModel
    {
        public int ItemId { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class BasketAddItemViewModel
    {
        public int ItemId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        public float ExtraDiscountPercentage { get; set; }

        public string? ReturnController { get; set; }

        public string? ReturnAction { get; set; }
    }
}
