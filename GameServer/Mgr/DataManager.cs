using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Summer;
using System.Collections;
using System.Collections.Generic;


public class DataManager : Singleton<DataManager>
{

    public Dictionary<int, SpaceDefine> Spaces;
    public Dictionary<int, UnitDefine> Units;
    public Dictionary<int, SpawnDefine> Spawns;
    public Dictionary<int, SkillDefine> Skills;
    public Dictionary<int, ItemDefine> Items;

    public void Init()
    {

        Spaces = Load<SpaceDefine>("Data/SpaceDefine.json");
        Units = Load<UnitDefine>("Data/UnitDefine.json");
        Spawns = Load<SpawnDefine>("Data/SpawnDefine.json");
        Skills = Load<SkillDefine>("Data/SkillDefine.json");
        Items = Load<ItemDefine>("Data/ItemDefine.json");

    }


    public Dictionary<int, T> Load<T>(string filePath)
    {
        // 获取exe文件所在目录的绝对路径
        string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string exeDirectory = Path.GetDirectoryName(exePath);
        // 构建1.txt文件的完整路径
        string txtFilePath = Path.Combine(exeDirectory, filePath);
        // 读取1.txt文件的内容
        string content = File.ReadAllText(txtFilePath);
        // 打印1.txt文件的内容
        //Console.WriteLine(content);
        return JsonConvert.DeserializeObject<Dictionary<int, T>>(content,settings);
    }

    JsonSerializerSettings settings = new JsonSerializerSettings
    {
        Converters = new JsonConverter[] {
            new FloatArrayConverter(),
            new IntArrayConverter(),
        }
    };

    //float[]
    public class FloatArrayConverter : JsonConverter<float[]>
    {
        public override float[] ReadJson(JsonReader reader, Type objectType, float[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.String)
            {
                string[] values = token.ToString().Replace("[", "").Replace("]", "").Split(',');
                float[] result = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    float.TryParse(values[i], out result[i]);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, float[] value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    //int[]
    public class IntArrayConverter : JsonConverter<int[]>
    {
        public override int[] ReadJson(JsonReader reader, Type objectType, int[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            if (token.Type == JTokenType.String)
            {
                string[] values = token.ToString().Replace("[", "").Replace("]", "").Split(',');
                int[] result = new int[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    int.TryParse(values[i], out result[i]);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, int[] value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }


}