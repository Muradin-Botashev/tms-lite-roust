namespace Domain.Services.Translations
{
    public class TranslationDto : IDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Ru { get; set; }
        public string En { get; set; }
    }
}