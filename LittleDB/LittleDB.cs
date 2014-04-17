using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Littledb
{
    public class LittleDB
    {
        private readonly string _dbName;

        public LittleDB(string dbName)
        {
            _dbName = dbName;
            Directory.CreateDirectory(_dbName);
        }

        /// <summary>
        /// Will save the given element. If has the property Id, will remove any previous record, if the Id is null will give one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public virtual void Save<T>(T obj)
        {
            RemovePreviousRecordIfId(obj);
            obj = AddIdIfNeeded(obj);
            var list = GetAll<T>();
            list.Add(obj);
            SaveList(list);
        }

        /// <summary>
        /// Will save the entire list, and remove the previous one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public void SaveList<T>(List<T> list)
        {
            var serializer = new JsonSerializer();
            using (var sw = new StreamWriter(Path.Combine(_dbName, typeof(T).Name)))
            using (var writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, list);
            }
        }

        /// <summary>
        /// Returns all the records of the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual List<T> GetAll<T>()
        {
            try
            {
                var serializer = new JsonSerializer();
                using (var sw = new StreamReader(Path.Combine(_dbName, typeof(T).Name)))
                using (var reader = new JsonTextReader(sw))
                {
                    return serializer.Deserialize<List<T>>(reader);
                }

            }
            catch (FileNotFoundException)
            {
                return new List<T>();
            }
        }

        protected PropertyInfo GetIdProperty<T>(T obj)
        {
            var properties = typeof(T).GetProperties();
            return properties.SingleOrDefault(p => p.Name.Contains("Id"));
        }

        protected void RemovePreviousRecordIfId<T>(T obj)
        {
            var list = GetAll<T>();
            var property = GetIdProperty(obj);
            if (property != null)
            {
                var idToRemove = property.GetValue(obj);
                list.RemoveAll(o => property.GetValue(o).Equals(idToRemove));
                SaveList(list);
            }
        }

        protected T AddIdIfNeeded<T>(T obj)
        {
            var idProperty = GetIdProperty(obj);
            if (idProperty != null && idProperty.GetValue(obj).Equals(0))
            {
                var list = GetAll<T>();
                var currentIds = new List<int>();
                list.ForEach(o => currentIds.Add((int)idProperty.GetValue(o)));

                var newId = currentIds.Any() ? currentIds.Max() + 1 : 1;
                idProperty.SetValue(obj, newId);
            }

            return obj;
        }
    }
}
