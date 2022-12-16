namespace WebUI.Components;
public class LanguageVC : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        string[] filePaths = Directory.GetFiles("Resources", "*.json");
        List<string> languages = new List<string>();
        foreach (string filePath in filePaths)
        {
            using var str = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var sReader = new StreamReader(str);
            using var reader = new JsonTextReader(sReader);
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName && reader.Value as string == "languageDescription")
                {
                    reader.Read();
                    string shortlanguageName = filePath.Split(new[] { "Resources\\" }, StringSplitOptions.None)[1].Split(new[] { ".json" }, StringSplitOptions.None)[0];
                    languages.Add(shortlanguageName + "_" + reader.Value.ToString());
                }
            }
        }
        return View(model: languages);
    }
}

