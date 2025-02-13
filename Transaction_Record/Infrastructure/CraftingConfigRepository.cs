using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Record.Domain;
using Transaction_Record.Domain.Interfaces;

namespace Transaction_Record.Infrastructure
{
    public class CraftingConfigRepository : ICraftingConfigRepository
    {
        private const string FILE_PATH = "crafting_conditions.json";

        public CraftingConfigRepository()
        {
            // 初始化空檔案（若不存在）
            if (!File.Exists(FILE_PATH))
            {
                File.WriteAllText(FILE_PATH, "[]");
            }
        }
        public ObservableCollection<CraftingCondition> LoadCondition()
        {
            if (File.Exists(FILE_PATH))
            {
                var json = File.ReadAllText(FILE_PATH);
                return JsonConvert.DeserializeObject<ObservableCollection<CraftingCondition>>(json);
            }
            return new ObservableCollection<CraftingCondition>();
        }

        public void SaveCondition(ObservableCollection<CraftingCondition> craftingConditions)
        {
            var json = JsonConvert.SerializeObject(craftingConditions);
            File.WriteAllText(FILE_PATH, json);
        }

    }
}
