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

        public static PlotData Load(string json) => JsonConvert.DeserializeObject<PlotData>(json, GetSettings());
    
        public static string Save(PlotData plotData) => JsonConvert.SerializeObject(plotData, GetSettings());
    }
}
