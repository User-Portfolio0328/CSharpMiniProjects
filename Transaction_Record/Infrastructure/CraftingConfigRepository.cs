using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction_Record.Domain;
using Transaction_Record.Domain.Enums;
using Transaction_Record.Domain.Interfaces;
using Transaction_Record.Infrastructure.Services;

namespace Transaction_Record.Infrastructure
{
    public class CraftingConfigRepository : ICraftingConfigRepository
    {
        private readonly DatabaseService _databaseService;

        public CraftingConfigRepository(DatabaseService databaseService)
        {
            this._databaseService = databaseService;
        }
        public ObservableCollection<CraftingCondition> LoadCondition()
        {
            var craftingConditions = new ObservableCollection<CraftingCondition>();

            using (var connection = this._databaseService.GetConnection()) 
            {
                using (var command = connection.CreateCommand()) 
                {
                    command.CommandText = "SELECT Id, Keyword, AffixType, AffixTier FROM CraftingConditions";

                    using (var reader = command.ExecuteReader()) 
                    {
                        while (reader.Read()) 
                        {
                            AffixSlot affixType;
                            if (!Enum.TryParse(reader.GetString(2), out affixType)) 
                            {
                                affixType = AffixSlot.第一條前綴;
                            }

                            craftingConditions.Add(new CraftingCondition
                            {
                                Id = reader.GetInt32(0),
                                Keyword = reader.GetString(1),
                                AffixType = affixType,
                                AffixTier = reader.GetString(3)
                            });
                        }
                    }
                }
            }

            return craftingConditions;
        }

        public void AddCondition(CraftingCondition craftingCondition)
        {
            using (var connection = this._databaseService.GetConnection()) 
            {
                using (var command = connection.CreateCommand()) 
                {
                    command.CommandText =
                        @"
                        INSERT INTO CraftingConditions (Keyword, AffixType, AffixTier)
                        VALUES (@Keyword, @AffixType, @AffixTier)
                        ";
                    command.Parameters.AddWithValue("@Keyword", craftingCondition.Keyword);
                    command.Parameters.AddWithValue("@AffixType", craftingCondition.AffixType.ToString());
                    command.Parameters.AddWithValue("@AffixTier", craftingCondition.AffixTier);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteCondition(int id) 
        {
            using (var connection = this._databaseService.GetConnection()) 
            {
                using (var command = connection.CreateCommand()) 
                {
                    command.CommandText = "DELETE FROM CraftingConditions WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
