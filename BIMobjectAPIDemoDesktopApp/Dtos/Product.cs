namespace BIMobjectAPIDemoDesktopApp.Dtos
{
    public class Product
    {
        public string Name { get; set; }
        public string Permalink { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Depth { get; set; }
        public string DescriptionHtml { get; set; }
        public string[] ImageUrls { get; set; }
        public Brand Brand { get; set; }
        public Files[] Files { get; set; }
    }
}