using System;
using Simple.Data;
using Simple.Data.MongoDB;

namespace Jabbot.AspNetBotHost.Components
{
    public class MongoSettingsService : ISettingsService
    {
        private dynamic db;

        public MongoSettingsService()
        {
            //// connect
            //db = Database.Opener.OpenMongo(serverUrl);
            //var db2 = Database.Opener.OpenMongo(serverUrl);
            //db2.
        }

        public bool ContainsKey(string key)
        {
            return db.Settings.FindBy(key) == null;
        }

        public T Get<T>(string key)
        {
            return (T)db.Settings.FindBy(key);
        }

        public void Set<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}