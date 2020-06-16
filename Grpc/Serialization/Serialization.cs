using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Narupa.Grpc.Serialization
{
    public class Serialization
    {
        private JsonSerializer Serializer = new JsonSerializer()
        {
            Converters =
            {
                new Vector3Converter()
            }
        };
        
        /// <summary>
        /// Serialize an object from a data structure consisting of
        /// <see cref="Dictionary{TKey,TValue}" />,
        /// <see cref="List{Object}" />, <see cref="string" />, <see cref="float" />,
        /// <see cref="string" /> and <see cref="bool" />, using a
        /// <see cref="JsonSerializer" />.
        /// </summary>
        public static T FromDataStructure<T>(object data)
        {
            using (var reader = new CSharpObjectReader(data))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }
        
        /// <summary>
        /// Update an object from a data structure consisting of
        /// <see cref="Dictionary{TKey,TValue}" />,
        /// <see cref="List{Object}" />, <see cref="string" />, <see cref="float" />,
        /// <see cref="string" /> and <see cref="bool" />, using a
        /// <see cref="JsonSerializer" />.
        /// </summary>
        public static void UpdateFromDataStructure(object data, object target)
        {
            using (var reader = new CSharpObjectReader(data))
            {
                var serializer = new JsonSerializer();
                serializer.Populate(reader, target);
            }
        }

        /// <summary>
        /// Deserialize an object from a data structure consisting of
        /// <see cref="Dictionary{TKey,TValue}" />,
        /// <see cref="List{Object}" />, <see cref="string" />, <see cref="float" />,
        /// <see cref="string" /> and <see cref="bool" />, using a
        /// <see cref="JsonSerializer" />.
        /// </summary>
        /// <remarks>
        /// If dictionaries or lists are present in data, then these will be deserialized
        /// into <see cref="JObject" /> and <see cref="JArray" />.
        /// </remarks>
        public static object FromDataStructure(object data)
        {
            using (var reader = new CSharpObjectReader(data))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Deserialize an object of a specific type from a data structure consisting of
        /// <see cref="Dictionary{TKey,TValue}" />,
        /// <see cref="List{Object}" />, <see cref="string" />, <see cref="float" />,
        /// <see cref="string" /> and <see cref="bool" />, using a
        /// <see cref="JsonSerializer" />.
        /// </summary>
        /// <remarks>
        /// If raw dictionaries or lists are present in data, then these will be
        /// deserialized into <see cref="JObject" /> and <see cref="JArray" />.
        /// </remarks>
        public static object ToDataStructure(object data)
        {
            using (var writer = new CSharpObjectWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, data);
                return writer.Object;
            }
        }
    }
}
