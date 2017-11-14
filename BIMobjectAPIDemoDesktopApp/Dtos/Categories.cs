namespace BIMobjectAPIDemoDesktopApp.Dtos
{
    public class Categories
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public SubCategories[] Children { get; set; }
    }
}