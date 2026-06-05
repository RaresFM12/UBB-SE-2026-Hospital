namespace Hospital.Data.Models;

public class BasketItemDto
{
    public int ItemId { get; set; }
    public string ItemThumbnailImagePath { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemProducer { get; set; } = string.Empty;
    public int ItemQuantityInBasket { get; set; }
    public float BaseItemDiscount { get; set; }
    public float ExtraItemDiscount { get; set; }
    public float ItemActiveUserDiscount { get; set; }
    public float InitialPricePerBox { get; set; }
    public float FinalPriceBeforeDiscount { get; set; }
    public float FinalPriceAfterDiscount { get; set; }

    public static BasketItemDto FromViewModel(BasketItemViewModel item) =>
        new()
        {
            ItemId = item.ItemId,
            ItemThumbnailImagePath = item.ItemThumbnailImagePath,
            ItemName = item.ItemName,
            ItemProducer = item.ItemProducer,
            ItemQuantityInBasket = item.ItemQuantityInBasket,
            BaseItemDiscount = item.BaseItemDiscount,
            ExtraItemDiscount = item.ExtraItemDiscount,
            ItemActiveUserDiscount = item.ItemActiveUserDiscount,
            InitialPricePerBox = item.InitialPricePerBox,
            FinalPriceBeforeDiscount = item.FinalPriceBeforeDiscount,
            FinalPriceAfterDiscount = item.FinalPriceAfterDiscount,
        };

    public BasketItemViewModel ToViewModel()
    {
        var item = new BasketItemViewModel(
            ItemId,
            ItemThumbnailImagePath,
            ItemName,
            ItemProducer,
            ItemQuantityInBasket,
            BaseItemDiscount,
            ExtraItemDiscount,
            ItemActiveUserDiscount,
            InitialPricePerBox);
        item.SetFinalPrices(FinalPriceBeforeDiscount, FinalPriceAfterDiscount);
        return item;
    }
}
