using Unity.Plastic.Newtonsoft.Json;

namespace RubbishPot.Core
{
    public static class PlotLoader
    {
        private static JsonSerializerSettings GetSettings() => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All, // ВАЖНО: сохраняет имена классов для полиморфизма
            Formatting = Formatting.Indented
        };

        public static Plot Load(string json) => JsonConvert.DeserializeObject<Plot>(json, GetSettings());
    
        public static string Save(Plot plot) => JsonConvert.SerializeObject(plot, GetSettings());
    }
}
