using System.Globalization;
using Engine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ProjectActions
{
    public class DynamicJsonActions : BaseActions
    {
        public DynamicJsonActions(Log l)
            : base(l, "#json")
        {
        }

        public string GetPropertyValue(string rawString, string propertyName)
        {
            string[] paths = propertyName.Split(".");
            JToken jToken = GetJTokenFromString(rawString);
            if(jToken == null)
                return null;
            JToken dynamicObjectProperty = GetJObjectOrJArrayByPropertyType(paths[0], jToken);
            if (paths.Length > 1)
            {
                for(int i = 1; i < paths.Length; i++)
                {
                    dynamicObjectProperty = GetJObjectOrJArrayByPropertyType(paths[i], dynamicObjectProperty);
                }
            }
            //workaround for validationg empty arrays and objects. HasValue == true if Jtoken has a child
            if (dynamicObjectProperty.Type == JTokenType.Object 
                || dynamicObjectProperty.Type == JTokenType.Array
                && !dynamicObjectProperty.HasValues)
                return null;
            //for arrays with values - returs joined string
            if (dynamicObjectProperty.Type == JTokenType.Array)
                return String.Join(';', dynamicObjectProperty.Values<string>());
            return dynamicObjectProperty.Value<string>();
        }

        public string SetPropertyValue(string rawString, string propertyName, string propertyValue)
        {
            string[] paths = propertyName.Split(".");
            JToken jToken = GetJTokenFromString(rawString);
            if (paths.Length == 1)
            {
                if (((JObject)jToken)[paths[0]] == null)
                    ((JObject)jToken).Add(GetJPropertyByType(propertyName, propertyValue));
                else
                    ((JObject)jToken)[paths[0]] = GetJValueByType(propertyValue);
            }
            else 
            {
                JToken dynamicObjectProperty = jToken;
                for(int i = 0; i < paths.Length - 1; i++)
                {
                    dynamicObjectProperty = GetJObjectOrJArrayByPropertyType(paths[i], dynamicObjectProperty, paths[i+1]);
                }

                if (dynamicObjectProperty.Type == JTokenType.Object)
                {
                    if (((JObject)dynamicObjectProperty)[paths[paths.Length - 1]] == null)
                        ((JObject)dynamicObjectProperty).Add(GetJPropertyByType(paths[paths.Length - 1], propertyValue));
                    else
                        ((JObject)dynamicObjectProperty)[paths[paths.Length - 1]] = GetJValueByType(propertyValue);
                }
                if (dynamicObjectProperty.Type == JTokenType.Array)
                {
                    int lastIndex = Int32.Parse(paths[paths.Length - 1]);
                    if (lastIndex >= ((JArray)dynamicObjectProperty).Count)
                    {
                        if (propertyValue != null) //HACK for getting empty array. Please use 'Property.0' as a name and 'null' as a value
                            ((JArray)dynamicObjectProperty).Add(GetJValueByType(propertyValue));
                    }
                    else
                        //((JArray)dynamicObjectProperty)[paths[paths.Length - 1]] = propertyValue;
                        if (propertyValue == null)
                            ((JArray)dynamicObjectProperty)[lastIndex].Remove();
                        else
                            ((JArray)dynamicObjectProperty)[lastIndex] = GetJValueByType(propertyValue);
                }
            }
            return GetStringFromJToken(jToken);
        }

        public string GetObjectFromArrayByPropertiesValues(string resource, string[] propertiesNames, string[] propertiesValues, string path = "")
        {
            JToken dynamicArray = GetJTokenFromString(resource);
            if (path != "")
            {
                string[] paths = path.Split(".");
                if (paths.Length > 1)
                {
                    for(int i = 1; i < paths.Length; i++)
                    {
                        dynamicArray = GetJObjectOrJArrayByPropertyType(paths[i], dynamicArray);
                    }
                }   
                else
                    dynamicArray = GetJObjectOrJArrayByPropertyType(paths[0], dynamicArray);
            }

            if (dynamicArray.Type == JTokenType.Array)
            {
                bool foundFlag = false;
                foreach(JObject dynamicObject in dynamicArray)
                {
                    for(int i = 0; i < propertiesNames.Length; i++)
                    {
                        string[] paths = propertiesNames[i].Split(".");
                        JToken dynamicObjectProperty = dynamicObject[paths[0]];
                        if (paths.Length > 1)
                        {
                            for(int j = 1; j < paths.Length; j++)
                            {
                                dynamicObjectProperty = GetJObjectOrJArrayByPropertyType(paths[j], dynamicObjectProperty);
                            }
                        }
                        if (dynamicObjectProperty.Value<string>() != propertiesValues[i])
                            break;
                        if (i == propertiesNames.Length - 1)
                            foundFlag = true;
                    }
                    if (foundFlag)
                        return GetStringFromJToken(dynamicObject);
                }
            }            
            return null;
        }

        public string GetCountOfObjectsInArray(string rawString, string propertyName)
        {
            JToken jToken = GetJTokenFromString(rawString);
            if (propertyName == "root")
            {
                return GetCountFromJArray(jToken).ToString();
            }
            string[] paths = propertyName.Split(".");
            if (paths.Length == 1)
            {
                JObject jsonObject = (JObject)jToken;                
                return GetCountFromJArray(jsonObject[paths[0]]).ToString();
            }
            else 
            {
                JToken dynamicObjectProperty = jToken;
                for(int i = 0; i < paths.Length - 1; i++)
                {
                    dynamicObjectProperty = GetJObjectOrJArrayByPropertyType(paths[i], dynamicObjectProperty, paths[i+1]);
                }
                return GetCountFromJArray(dynamicObjectProperty[paths[paths.Length - 1]]).ToString();
            }
        }

        public List<string> GetListOfPropertiesFromObject(string rawString)
        {
            JToken obj = GetJTokenFromString(rawString);
            List<string> prList = new List<string>();
            if (obj.Type == JTokenType.Object)
            {
                foreach(JToken child in ((JObject)obj).Children())
                {
                    prList.Add(((JProperty)child).Name);
                }
                return prList;
            }
            return null;
        }

        public string AddJTokenToAnoter(string source, string destination, string destinationPath)
        {
            JToken jTokenDest = GetJTokenFromString(destination);
            JToken jTokenSource = GetJTokenFromString(source);

            ((JObject)jTokenDest)[destinationPath] = jTokenSource;
            return GetStringFromJToken(jTokenDest);
        }

        private int GetCountFromJArray(JToken jsonObject)
        {
            if(jsonObject.Type == JTokenType.Array)
            {
                return ((JArray)jsonObject).Count;
            }
            return 0;
        }
        
        private JToken GetJObjectOrJArrayByPropertyType(string property, JToken obj, string nextProperty = "")
        {
            JToken dynamicObjectProperty = obj;
            int index = 0;
            int unused = 0;
            bool nextInt = Int32.TryParse(nextProperty, out unused);
            if (Int32.TryParse(property, out index))
            {
                int last = ((JArray)dynamicObjectProperty).Count - 1;
                if (last < index)
                {
                    for (int j = last; j < index; j++)
                    {
                        ((JArray)dynamicObjectProperty).Add(new JObject());
                    }
                }
                dynamicObjectProperty = dynamicObjectProperty[index];
            }
            else 
            {
                if (dynamicObjectProperty[property] == null || dynamicObjectProperty[property].Type == JTokenType.Null)
                {
                    if (dynamicObjectProperty[property] != null && dynamicObjectProperty[property].Type == JTokenType.Null)
                        ((JObject)dynamicObjectProperty).Property(property).Remove(); // or ((JObject)dynamicObjectProperty)[property].Parent.Remove();
                        // please see https://stackoverflow.com/questions/21898727/getting-the-error-cannot-add-or-remove-items-from-newtonsoft-json-linq-jpropert
                    if (nextInt)                        
                        ((JObject)dynamicObjectProperty).Add(new JProperty(property, new JArray()));
                    else
                        ((JObject)dynamicObjectProperty).Add(new JProperty(property, new JObject()));
                }
                dynamicObjectProperty = dynamicObjectProperty[property];
            }
            return dynamicObjectProperty;
        }

        // private string GetValueByJTokenType(string property, JToken source)
        // {
        //     if(source.Type == JTokenType.Array)
        //         return source[Int32.Parse(property)].Value<string>();
        //     return source[property].Value<string>();
        // }

        private JToken GetJTokenFromString(string rawString)
        {
            try
            {
                if(rawString == null)
                    return new JObject();
                return JToken.Parse(rawString);
            }
            catch(JsonReaderException)
            {
                log.Write("String cannot be parsed as JSON:", rawString);
                return null;
            }
        }

        private string GetStringFromJToken(JToken jToken)
        {
            return JsonConvert.SerializeObject(jToken);
        }

        private JProperty GetJPropertyByType(string name, string initialValue)
        {
            int intValue = 0;
            if (Int32.TryParse(initialValue, out intValue))
                return new JProperty(name, intValue);
            decimal decValue = 0;
            if (Decimal.TryParse(initialValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decValue))
                return new JProperty(name, decValue);
            bool boolValue = false;
            if (Boolean.TryParse(initialValue, out boolValue))
                return new JProperty(name, boolValue);
            return new JProperty(name, initialValue);
        }

        private JValue GetJValueByType(string initialValue)
        {
            int intValue = 0;
            if (Int32.TryParse(initialValue, out intValue))
                return new JValue(intValue);
            decimal decValue = 0;
            if (Decimal.TryParse(initialValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decValue))
                return new JValue(decValue);
            bool boolValue = false;
            if (Boolean.TryParse(initialValue, out boolValue))
                return new JValue(boolValue);
            return new JValue(initialValue);
        }
    }

    /// <summary>
    /// Static class for validating a schema of the JSON object
    /// </summary>
    public static class JsonSchemaActions
    {
        /// <summary>
        /// Read JSON schema from \TestData folder and validate JSON object.
        /// </summary>
        /// <param name="fileName">File name (without .schema.json postfix) in .\TestData folder</param>
        /// <param name="jsonObject">Serialized string of JSON object</param>
        /// /// <param name="errors">Error IList if validating is failed</param>
        /// <returns>Boolean status of validating and Errors IList if failed.</returns>
        public static bool IsSchemaFromFileValid(string filePath, string jsonObject, out IList<string> errors)
        {
            string jsonString = File.ReadAllText(filePath);

            JSchema schema = JSchema.Parse(jsonString);

            JToken j = JsonConvert.DeserializeObject<JToken>(jsonObject);
            return j.IsValid(schema, out errors);
        }
    }
}