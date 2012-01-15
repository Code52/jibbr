using System;
using System.ComponentModel.Composition;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Jabbot.AspNetBotHost.Components
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MongoSettingsService : ISettingsService
    {
        private readonly MongoCollection<BsonDocument> _settings;
        private readonly BsonDocument _document;

        public MongoSettingsService()
        {
            // connect
            try
            {
                var server = MongoServer.Create("mongodb://localhost:27017");
                server.Connect();

                var database = server.GetDatabase("jibbr");

                _settings = database.GetCollection("Settings");

                if (_settings.Count() == 0)
                {
                    _document = new BsonDocument();
                    _settings.Insert(_document);
                }
                else
                {
                    _document = _settings.FindAll().First().ToBsonDocument();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool ContainsKey(string key)
        {
            BsonElement value;

            return _document.TryGetElement(key, out value);
        }

        public T Get<T>(string key)
        {
            var value = _document.GetElement(key).Value.AsString;
            return JsonConvert.DeserializeObject<T>(value);
        }

        public void Set<T>(string key, T value)
        {
            var result = JsonConvert.SerializeObject(value);
            _document.SetElement(new BsonElement(key, result));
        }

        public void Save()
        {
            // ??
        }
    }
}