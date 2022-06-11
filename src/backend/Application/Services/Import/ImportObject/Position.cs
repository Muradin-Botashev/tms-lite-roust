using Newtonsoft.Json;

namespace Application.Services.Import.ImportObject
{
    public class Position
    {
        /// <summary>
        /// Количество единиц товара
        /// </summary>
        [JsonProperty("Matnr")]
        public string Nart { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        [JsonProperty("Arktx")]
        public string Description { get; set; }
    }
}