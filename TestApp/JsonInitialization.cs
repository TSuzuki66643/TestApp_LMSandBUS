using Newtonsoft.Json;
using System.Text.Json.Nodes;
using Windows.Networking.NetworkOperators;

namespace TestApp;
public class LMSEntries
{
    [JsonObject("entries")]
    public class LMSEntries2
    {

        

    }

    [JsonProperty("quarter")]
    public int quarter { get; set; }
    [JsonProperty("registeredAt")]
    public int registerCode { get; set; }
    [JsonProperty("year")]
    public int Year { get; set; }

}

public class LMSDataJSClass
{
    [JsonProperty("actingUrl")]
    public List<string> actURL { get; set; }

    [JsonProperty("credit")]
    public List<string> credit { get; set; }

    [JsonProperty("date")]

    public List<string> Date { get; set; }
    [JsonProperty("lctCd")]

    public List<string> LectureCode { get; set; }
    [JsonProperty("lmsUrl")]

    public List<string> LMS_URL { get; set; }
    [JsonProperty("period")]

    public List<string> Period { get; set; }
    [JsonProperty("sbjDiv")]

    public List<string> SbjDiv { get; set; }

    [JsonProperty("subjectName")]
    public List<string> Title { get; set; }

    [JsonProperty("syllabusUrl")]
    public List<string> SyllabusURL { get; set; }

    [JsonProperty("teacher")]
    public List<string> TeacherName { get; set; }
}