using Newtonsoft.Json;

namespace EasyProgramAccess
{
    public class PathGroup
    {
        public string GroupName { get; set; }
        public string[] Paths { get; set; }
        [JsonProperty(PropertyName = "dateadded")]
        public string DateAdded { get; set; }
        [JsonProperty(PropertyName = "dateopened")]
        public string DateOpened { get; set; }
    }
}