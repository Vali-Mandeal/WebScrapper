namespace WebScrapper.Entities;

public class WebsiteMetadata
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";

    // Metadata & Flow
    public bool ShouldAcceptTermsAndConditions { get; set; }
    public bool ShouldScrollToBottom { get; set; }
    public bool ShouldSearch { get; set; }

    public MetadataSelectors Selectors { get; set; } = new MetadataSelectors();
}

public class MetadataSelectors
{
    public string TermsAndConditionsButtonSelector { get; set; } = "";
    public string SearchSelector { get; set; } = "";

    public string ScrollToButtonCommand { get; set; } = "";


    // Card selectors
    public string CardsSelector { get; set; } = "";
    public string CardTitleSelector { get; set; } = "";
    public string CardPriceSelector { get; set; } = "";
    public string LocationAndDateSelector { get; set; } = "";
    public string AdUrlWrapperSelector { get; set; } = "";
    public string AdUrlSelector { get; set; } = "";


    public string ThumbnailUrlWrapperSelector = "";
    public string ThumbnailUrlSelector = "";

    public string BackupThumbnailUrlWrapperSelector = "";
    public string BackupThumbnailUrlSelector = "";
}
    