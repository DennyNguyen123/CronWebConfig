using Newtonsoft.Json.Linq;

public class AppConfig
{
    public dynamic JsonObj;
    public string? ConfigPath;
    public AppConfig(string appsetting)
    {
        JsonObj = GetAppSetting(appsetting);
    }
    private dynamic GetAppSetting(string appsetting = "")
    {
        appsetting = string.IsNullOrEmpty(appsetting) ? "appsettings.json" : appsetting;
        var filePath = Path.Combine(AppContext.BaseDirectory, appsetting);
        this.ConfigPath = filePath;

        string json = File.ReadAllText(filePath);
        dynamic? jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        if (jsonObj == null)
        {
            throw new Exception($"Not found config file at {filePath}");
        }
        return jsonObj;
    }

    public void SaveToFile()
    {
        if (this.JsonObj != null)
        {
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(JsonObj, Newtonsoft.Json.Formatting.Indented);
            Console.WriteLine(output);
            File.WriteAllText(this.ConfigPath, output);
        }
    }


    public dynamic? GetValueJObject(string sectionPathKey)
    {
        var lst = sectionPathKey.Split(':').ToList();
        if (lst == null)
        {
            throw new Exception();
        }
        else
        {
            if (lst.Count == 0)
            {
                throw new Exception();
            }
        }

        var lastindex = lst.IndexOf(lst.Last());
        dynamic? value = JsonObj;

        foreach (var item in lst)
        {
            var id = lst.IndexOf(item);
            if (value != null)
            {
                value = value[item];
            }

            if (id == lastindex)
            {
                return value;
            }
        }
        return null;
    }

    public void SetValueJObject(string sectionPathKey, dynamic setvalue)
    {
        var lst = sectionPathKey.Split(':').ToList();
        if (lst == null)
        {
            throw new Exception();
        }
        else
        {
            if (lst.Count == 0)
            {
                throw new Exception();
            }
        }

        var lastindex = lst.IndexOf(lst.Last());
        dynamic? value = this.JsonObj;

        foreach (var item in lst)
        {
            var id = lst.IndexOf(item);
            if (value != null)
            {
                value = value[item];

                if (id == lastindex)
                {
                    value = setvalue;
                }
            }

        }

        this.SaveToFile();
    }
}