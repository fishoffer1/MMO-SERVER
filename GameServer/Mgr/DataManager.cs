using Newtonsoft.Json;
using Summer;


public class DataManager : Singleton<DataManager>
{
    public Dictionary<int, SpaceDefine> Spaces ;
    public Dictionary<int, UnitDefine> Units ;
    public Dictionary<int, SpawnDefine> Spawns ;
    public void Init()
    {
        Spaces = Load<SpaceDefine>("Data/SpaceDefine.json");
        Units = Load<UnitDefine>("Data/UnitDefine.json");
        Spawns = Load<SpawnDefine>("Data/SpawnDefine.json");

    }

    public Dictionary<int, T> Load<T>(string filePath)
    {
        //获取exe文件所在目录的绝对路径
        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string exeDirectory = Path.GetDirectoryName(exePath);
        //构建1.txt文件的完整路径
        string txtFilePath = Path.Combine(exeDirectory, filePath);
        //读取1.txt文件内容
        string content = File.ReadAllText(txtFilePath);
        //打印1.txt文件内容
        
        return JsonConvert.DeserializeObject<Dictionary<int, T>>(content);
    }

}

