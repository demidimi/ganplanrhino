using System;
using System.IO;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Rhino.Collections;
using System.Runtime.Serialization;

namespace Rhino.Compute
{
    public static class ComputeServer
    {
        public static string WebAddress { get; set; } = "https://compute.rhino3d.com";
        public static string AuthToken { get; set; }
        public static string Version => "0.11.0";

        public static T Post<T>(string function, params object[] postData)
        {
            return PostWithConverter<T>(function, null, postData);
        }

        public static T PostWithConverter<T>(string function, JsonConverter converter, params object[] postData)
        {
            if (string.IsNullOrWhiteSpace(AuthToken) && WebAddress.Equals("https://compute.rhino3d.com"))
                throw new UnauthorizedAccessException("AuthToken must be set for compute.rhino3d.com");

            for( int i=0; i<postData.Length; i++ )
            {
                if( postData[i]!=null &&
                    postData[i].GetType().IsGenericType &&
                    postData[i].GetType().GetGenericTypeDefinition() == typeof(Remote<>) )
                {
                    var mi = postData[i].GetType().GetMethod("JsonObject");
                    postData[i] = mi.Invoke(postData[i], null);
                }
            }

            string json = converter == null ?
                JsonConvert.SerializeObject(postData, Formatting.None) :
                JsonConvert.SerializeObject(postData, Formatting.None, converter);
            if (!function.StartsWith("/"))
                function = "/" + function;
            string uri = (WebAddress + function).ToLower();
            var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(uri);
            request.ContentType = "application/json";
            if(!string.IsNullOrEmpty(AuthToken))
                request.Headers.Add("Authorization", "Bearer " + AuthToken);
            request.UserAgent = "compute.rhino3d.cs/" + Version;
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var response = request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                if (converter == null)
                    return JsonConvert.DeserializeObject<T>(result);
                return JsonConvert.DeserializeObject<T>(result, converter);
            }
        }

        public static T0 Post<T0, T1>(string function, out T1 out1, params object[] postData)
        {
            if (string.IsNullOrWhiteSpace(AuthToken))
                throw new UnauthorizedAccessException("AuthToken must be set");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            if (!function.StartsWith("/"))
                function = "/" + function;
            string uri = (WebAddress + function).ToLower();
            var request = System.Net.WebRequest.Create(uri);
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Bearer " + AuthToken);
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var response = request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var jsonString = streamReader.ReadToEnd();
                object data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
                var ja = data as Newtonsoft.Json.Linq.JArray;
                out1 = ja[1].ToObject<T1>();
                return ja[0].ToObject<T0>();
            }
        }

        public static T0 Post<T0, T1, T2>(string function, out T1 out1, out T2 out2, params object[] postData)
        {
            if (string.IsNullOrWhiteSpace(AuthToken))
                throw new UnauthorizedAccessException("AuthToken must be set");
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postData);
            if (!function.StartsWith("/"))
                function = "/" + function;
            string uri = (WebAddress + function).ToLower();
            var request = System.Net.WebRequest.Create(uri);
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Bearer " + AuthToken);
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var response = request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var jsonString = streamReader.ReadToEnd();
                object data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
                var ja = data as Newtonsoft.Json.Linq.JArray;
                out1 = ja[1].ToObject<T1>();
                out2 = ja[2].ToObject<T2>();
                return ja[0].ToObject<T0>();
            }
        }

        public static string ApiAddress(Type t, string function)
        {
            string s = t.ToString().Replace('.', '/');
            return s + "/" + function;
        }
    }

    public class Remote<T>
    {
        string _url;
        T _data;

        public Remote(string url)
        {
            _url = url;
        }

        public Remote(T data)
        {
            _data = data;
        }

        public object JsonObject()
        {
            if( _url!=null )
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict["url"] = _url;
                return dict;
            }
            return _data;
        }
    }

    public static class GrasshopperCompute
    {
        static string ApiAddress()
        {
            return "grasshopper";
        }

        public static List<GrasshopperDataTree> EvaluateDefinition(string definition, IEnumerable<GrasshopperDataTree> trees)
        {
            Schema schema = new Schema();
            string algo = null;
            string pointer = null;
            if (definition.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                pointer = definition;
            }
            else
            {
                var bytes = File.ReadAllBytes(definition);
                algo = Convert.ToBase64String(bytes);
            }

            schema.Algo = algo;
            schema.Pointer = pointer;
            schema.Values = new List<GrasshopperDataTree>(trees);
            var rc = ComputeServer.Post<Schema>(ApiAddress(), schema);

            return rc.Values;
        }

        // Keep private; only used for JSON serialization
        class Schema
        {
            public Schema() { }

            [JsonProperty(PropertyName = "algo")]
            public string Algo { get; set; }

            [JsonProperty(PropertyName = "pointer")]
            public string Pointer { get; set; }

            [JsonProperty(PropertyName = "values")]
            public List<GrasshopperDataTree> Values { get; set; } = new List<GrasshopperDataTree>();
        }

    }

    public class GrasshopperObject
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        [JsonConstructor]
        public GrasshopperObject()
        {
        }

        public GrasshopperObject(object obj)
        {
            this.Data = JsonConvert.SerializeObject(obj, GeometryResolver.Settings);
            this.Type = obj.GetType().FullName;
        }

        /// <summary>
        /// Used internally for RestHopperObject serialization
        /// </summary>
        class GeometryResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            static JsonSerializerSettings _settings;
            public static JsonSerializerSettings Settings
            {
                get
                {
                    if (_settings == null)
                    {
                        _settings = new JsonSerializerSettings { ContractResolver = new GeometryResolver() };
                        // return V6 ON_Objects for now
                        var options = new Rhino.FileIO.SerializationOptions();
                        options.RhinoVersion = 6;
                        options.WriteUserData = true;
                        _settings.Context = new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All, options);
                        //_settings.Converters.Add(new ArchivableDictionaryResolver());
                    }
                    return _settings;
                }
            }

            protected override Newtonsoft.Json.Serialization.JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                if (property.DeclaringType == typeof(Rhino.Geometry.Circle))
                {
                    property.ShouldSerialize = _ =>
                    {
                        return property.PropertyName != "IsValid" && property.PropertyName != "BoundingBox" && property.PropertyName != "Diameter" && property.PropertyName != "Circumference";
                    };

                }
                if (property.DeclaringType == typeof(Rhino.Geometry.Plane))
                {
                    property.ShouldSerialize = _ =>
                    {
                        return property.PropertyName != "IsValid" && property.PropertyName != "OriginX" && property.PropertyName != "OriginY" && property.PropertyName != "OriginZ";
                    };
                }

                if (property.DeclaringType == typeof(Rhino.Geometry.Point3f) ||
                    property.DeclaringType == typeof(Rhino.Geometry.Point2f) ||
                    property.DeclaringType == typeof(Rhino.Geometry.Vector2f) ||
                    property.DeclaringType == typeof(Rhino.Geometry.Vector3f))
                {
                    property.ShouldSerialize = _ =>
                    {
                        return property.PropertyName == "X" || property.PropertyName == "Y" || property.PropertyName == "Z";
                    };
                }

                if (property.DeclaringType == typeof(Rhino.Geometry.Line))
                {
                    property.ShouldSerialize = _ =>
                    {
                        return property.PropertyName == "From" || property.PropertyName == "To";
                    };
                }

                if (property.DeclaringType == typeof(Rhino.Geometry.MeshFace))
                {
                    property.ShouldSerialize = _ =>
                    {
                        return property.PropertyName != "IsTriangle" && property.PropertyName != "IsQuad";
                    };
                }
                return property;
            }
        }
    }


    public class GrasshopperPath
    {
        public int[] Path
        {
            get; set;
        }

        public GrasshopperPath()
        {
            //this.Path = new int[0];
        }

        public GrasshopperPath(int path)
        {
            this.Path = new int[] { path };
        }


        public GrasshopperPath(int[] path)
        {
            this.Path = path;
        }

        public GrasshopperPath(string path)
        {
            this.Path = FromString(path);
        }

        public override string ToString()
        {
            string sPath = "{ ";
            foreach (int i in this.Path)
            {
                sPath += $"{i}; ";
            }
            sPath += "}";
            return sPath;
        }

        public static int[] FromString(string path)
        {
            string primer = path.Replace(" ", "").Replace("{", "").Replace("}", "");
            string[] stringValues = primer.Split(';');
            List<int> ints = new List<int>();
            foreach (string s in stringValues)
            {
                if (s != string.Empty)
                {
                    ints.Add(Int32.Parse(s));
                }
            }
            return ints.ToArray();
        }

        public GrasshopperPath(GrasshopperPath pathObj, int i)
        {
            int[] path = pathObj.Path;
            this.Path = new int[path.Length + 1];

            for (int j = 0; j < path.Length; j++)
            {
                this.Path[j] = path[j];
            }
            this.Path[path.Length] = i;
        }
    }


    public class GrasshopperDataTree
    {
        public GrasshopperDataTree(string paramName)
        {
            ParamName = paramName;
            _tree = new Dictionary<string, List<GrasshopperObject>>();
        }

        private Dictionary<string, List<GrasshopperObject>> _tree;
        public string ParamName { get; }


        public Dictionary<string, List<GrasshopperObject>> InnerTree
        {
            get { return _tree; }
            set { _tree = value; }
        }

        public List<GrasshopperObject> this[string key]
        {
            get
            {
                return ((IDictionary<string, List<GrasshopperObject>>)_tree)[key];
            }

            set
            {
                ((IDictionary<string, List<GrasshopperObject>>)_tree)[key] = value;
            }
        }

        public bool Contains(GrasshopperObject item)
        {

            foreach (var list in _tree.Values)
            {
                if (list.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        public void Append(List<GrasshopperObject> items, GrasshopperPath GhPath)
        {
            this.Append(items, GhPath.ToString());
        }

        public void Append(List<GrasshopperObject> items, string GhPath)
        {

            if (!_tree.ContainsKey(GhPath))
            {
                _tree.Add(GhPath, new List<GrasshopperObject>());
            }
            _tree[GhPath].AddRange(items);
            //_GhPathIndexer.Add(item.Index, GhPath);
        }

        public void Append(GrasshopperObject item, GrasshopperPath path)
        {
            this.Append(item, path.ToString());
        }

        public void Append(GrasshopperObject item, string GhPath)
        {
            if (!_tree.ContainsKey(GhPath))
            {
                _tree.Add(GhPath, new List<GrasshopperObject>());
            }
            _tree[GhPath].Add(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, List<GrasshopperObject>>)_tree).ContainsKey(key);
        }

        public void Add(string key, List<GrasshopperObject> value)
        {
            ((IDictionary<string, List<GrasshopperObject>>)_tree).Add(key, value);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, List<GrasshopperObject>>)_tree).Remove(key);
        }

        public bool TryGetValue(string key, out List<GrasshopperObject> value)
        {
            return ((IDictionary<string, List<GrasshopperObject>>)_tree).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, List<GrasshopperObject>> item)
        {
            ((IDictionary<string, List<GrasshopperObject>>)_tree).Add(item);
        }

        public void Clear()
        {
            ((IDictionary<string, List<GrasshopperObject>>)_tree).Clear();
        }

        public bool Contains(KeyValuePair<string, List<GrasshopperObject>> item)
        {
            return ((IDictionary<string, List<GrasshopperObject>>)_tree).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, List<GrasshopperObject>>[] array, int arrayIndex)
        {
            ((IDictionary<string, List<GrasshopperObject>>)_tree).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, List<GrasshopperObject>> item)
        {
            return ((IDictionary<string, List<GrasshopperObject>>)_tree).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, List<GrasshopperObject>>> GetEnumerator()
        {
            return ((IDictionary<string, List<GrasshopperObject>>)_tree).GetEnumerator();
        }
    }
}
