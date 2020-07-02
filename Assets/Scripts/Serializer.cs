using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace KlimSoft
{
    public static class Serializer
    {
        [AttributeUsage(AttributeTargets.Field)]
        public class Ignore : Attribute { }
        public static string Serialize(object obj, Type type)
        {
            DateTime dt = DateTime.Now;

            FieldInfo[] fields = type.GetFields();
            string res = null;
            for (int i = 0; i < fields.Length; i++)
            {
                //If field don`t contains attribute "Serializer.Ignore".
                if (!fields[i].CustomAttributes.Select(x => x.AttributeType).Contains(typeof(Ignore)))
                {
                    res += $"\"{fields[i].Name}\":";
                    res += SerializeField(fields[i].GetValue(obj));
                    if (i < fields.Length - 1)
                        res += ";";
                }
            }

            Debug.Log("Serialize time: " + (DateTime.Now - dt).TotalMilliseconds + " ms Type: " + type.Name);
            return res;
        }

        public static string Serialize(object obj)
        {
            return Serialize(obj, obj.GetType());
        }

        private static string SerializeField(object obj)
        {
            if (obj == null)
                return "null";

            string res = null;
            Type type = obj.GetType();
            //If is array.
            if (type.IsArray)
            {
                object[] array = ((IEnumerable)obj)
                    .Cast<object>()
                    .ToArray();
                res += "[";

                //Serialize every element of array.
                for (int i = 0; i < array.Length; i++)
                {
                    res += SerializeField(array[i]);
                    if (i < array.Length - 1)
                        res += ",";
                }

                res += "]";
            }
            //If is list.
            else if (obj is IList)
            {
                object[] array = ((IEnumerable)obj)
                    .Cast<object>()
                    .ToArray();
                res += "[";

                //Serialize every element of array.
                for (int i = 0; i < array.Length; i++)
                {
                    res += SerializeField(array[i]);
                    if (i < array.Length - 1)
                        res += ",";
                }

                res += "]";
            }
            //bool.
            else if (obj is bool @bool)
            {
                res += "\"";
                res += @bool ? '1' : '0';
                res += "\"";
            }
            //byte.
            else if (obj is byte @byte)
            {
                res += "\"";
                res += BitConverter.ToString(new byte[] { @byte });
                res += "\"";
            }
            //sbyte.
            else if (obj is sbyte @sbyte)
            {
                res += "\"";
                res += BitConverter.ToString(new byte[] { (byte)@sbyte });
                res += "\"";
            }
            //short.
            else if (obj is short @short)
            {
                res += "\"";
                res += Convert.ToBase64String(BitConverter.GetBytes(@short));
                res += "\"";
            }
            //ushort.
            else if (obj is ushort @ushort)
            {
                res += "\"";
                res += Convert.ToBase64String(BitConverter.GetBytes(@ushort));
                res += "\"";
            }
            //int.
            else if (obj is int @int)
            {
                res += "\"";
                res += Convert.ToBase64String(BitConverter.GetBytes(@int));
                res += "\"";
            }
            //uint.
            else if (obj is uint @uint)
            {
                res += "\"";
                res += Convert.ToBase64String(BitConverter.GetBytes(@uint));
                res += "\"";
            }
            //long.
            else if (obj is long @long)
            {
                res += "\"";
                res += Convert.ToBase64String(BitConverter.GetBytes(@long));
                res += "\"";
            }
            //ulong.
            else if (obj is ulong @ulong)
            {
                res += "\"";
                res += Convert.ToBase64String(BitConverter.GetBytes(@ulong));
                res += "\"";
            }
            //float.
            else if (obj is float @float)
            {
                res += "\"";
                res += Convert.ToBase64String(BitConverter.GetBytes(@float));
                res += "\"";
            }
            //double.
            else if (obj is double @double)
            {
                res += "\"";
                res += Convert.ToBase64String(BitConverter.GetBytes(@double));
                res += "\"";
            }
            //decimal.
            else if (obj is decimal @decimal)
            {
                res += $"\"{Convert.ToBase64String(DecimalToBytes(@decimal))}\"";
            }
            //char.
            else if (obj is char @char)
            {
                res += $"\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(new char[] { @char }))}\"";
            }
            //string.
            else if (obj is string @string)
            {
                res += $"\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(@string))}\"";
            }
            //DateTime.
            else if (obj is DateTime @DateTime)
            {
                res += "\"";
                res += Convert.ToBase64String(BitConverter.GetBytes(DateTime.Ticks));
                res += "\"";
            }
            //TimeSpan.
            else if (obj is TimeSpan @TimeSpan)
            {
                res += "\"";
                res += Convert.ToBase64String(BitConverter.GetBytes(TimeSpan.Ticks));
                res += "\"";
            }
            //enum.
            else if (obj.GetType().IsEnum)
            {
                res += "\"";
                res += Convert.ToBase64String(BitConverter.GetBytes((int)obj));
                res += "\"";
            }
            //Other class.
            else
            {
                res += "{";
                res += Serialize(obj, obj.GetType());
                res += "}";
            }
            return res;
        }

        public static object Deserialize(string input, Type type)
        {
            object res;
            //Special intitialize for ScriptableObjects.
            if (type.BaseType == typeof(ScriptableObject) || type.BaseType.BaseType == typeof(ScriptableObject))
                res = ScriptableObject.CreateInstance(type);
            else
                res = Activator.CreateInstance(type);

            //Get all fields in the input class.
            FieldInfo[] fields = type.GetFields();
            //Use only names of fields now.
            string[] names = fields.Select(x => x.Name).ToArray();
            for (int i = 0; i < fields.Length; i++)
            {
                //Get index of this variable assigned.
                int indexOf = input.IndexOf($"\"{names[i]}\":");
                //If it not assigned, continue and throw warning.
                if (indexOf == -1)
                {
                    //Log warning if this unassigned field hasn`t ignore attribute.
                    if (!fields[i].CustomAttributes.Select(x => x.AttributeType).Contains(typeof(Ignore)))
                        Debug.LogWarningFormat("Cannot find variable with name \"{0}\" in deserialization of \"{1}\" class.", names[i], type.Name);
                    continue;
                }

                //                           "  |  name of var  |  "   :
                int startOfValue = indexOf + 1 + names[i].Length + 1 + 1;
                int endOfValue = -1;
                if (input[startOfValue] == '{')
                {
                    int rank = 0;
                    for (int j = startOfValue; j < input.Length; j++)
                    {
                        if (input[j] == '{')
                            rank++;
                        else if (input[j] == '}')
                            rank--;

                        if (rank == 0)
                        {
                            endOfValue = j + 1;
                            break;
                        }
                    }
                }
                else if (input[startOfValue] == '[')
                {
                    int rank = 0;
                    for (int j = startOfValue; j < input.Length; j++)
                    {
                        if (input[j] == '[')
                            rank++;
                        else if (input[j] == ']')
                            rank--;

                        if (rank == 0)
                        {
                            endOfValue = j + 1;
                            break;
                        }
                    }
                }
                else
                    endOfValue = input.IndexOf(';', startOfValue);
                //If end of value (';') cannot be found, value continues to end.
                string value;
                if (endOfValue == -1)
                    value = input.Substring(startOfValue);
                else
                    value = input.Substring(startOfValue, endOfValue - startOfValue);

                fields[i].SetValue(res, DeserializeField(value, fields[i].FieldType));
            }

            return res;
        }

        private static object DeserializeField(string input, Type type)
        {
            //return null if input == "null".
            if (input == "null")
                return null;
            //Remove first '[' or '"' and last ']' or '"'.
            if (input.Length <= 2)
                input = "";
            else
                input = input.Substring(1, input.Length - 2);
            //If is array.
            if (type.IsArray)
            {
                //Split, ignoring subarrays.
                string[] valuesStr = SmartSplit(input, ',', new char[] { '[', '{' }, new char[] { ']', '}' });

                object[] values = new object[valuesStr.Length];
                for (int i = 0; i < values.Length; i++)
                    values[i] = DeserializeField(valuesStr[i], type.GetElementType());

                var temp = Array.CreateInstance(type.GetElementType(), values.Length);
                Array.Copy(values, temp, values.Length);
                return temp;
            }

            //bool.
            if (type == typeof(bool))
            {
                if (input == "1")
                    return true;
                else if (input == "0")
                    return false;
                else
                    throw new InvalidCastException("Boolean value do not equals 1 or 0.");
            }
            //byte.
            else if (type == typeof(byte))
                return Convert.ToByte(input, 16);
            //sbyte.
            else if (type == typeof(sbyte))
                return Convert.ToSByte(input, 16);
            //short.
            else if (type == typeof(short))
                return BitConverter.ToInt16(Convert.FromBase64String(input), 0);
            //ushort.
            else if (type == typeof(ushort))
                return BitConverter.ToUInt16(Convert.FromBase64String(input), 0);
            //int.
            else if (type == typeof(int))
                return BitConverter.ToInt32(Convert.FromBase64String(input), 0);
            //uint.
            else if (type == typeof(uint))
                return BitConverter.ToUInt32(Convert.FromBase64String(input), 0);
            //long.
            else if (type == typeof(long))
                return BitConverter.ToInt64(Convert.FromBase64String(input), 0);
            //ulong.
            else if (type == typeof(ulong))
                return BitConverter.ToUInt64(Convert.FromBase64String(input), 0);
            //float.
            else if (type == typeof(float))
                return BitConverter.ToSingle(Convert.FromBase64String(input), 0);
            //double.
            else if (type == typeof(double))
                return BitConverter.ToDouble(Convert.FromBase64String(input), 0);
            //decimal.
            else if (type == typeof(decimal))
                return BytesToDecimal(Convert.FromBase64String(input));
            //char.
            else if (type == typeof(char))
                return Encoding.UTF8.GetChars(Convert.FromBase64String(input))[0];
            //string.
            else if (type == typeof(string))
                return Encoding.UTF8.GetString(Convert.FromBase64String(input));
            //DateTime.
            else if (type == typeof(DateTime))
                return new DateTime(BitConverter.ToInt64(Convert.FromBase64String(input), 0));
            //TimeSpan.
            else if (type == typeof(TimeSpan))
                return new TimeSpan(BitConverter.ToInt64(Convert.FromBase64String(input), 0));
            //enum.
            else if (type.IsEnum)
                return Enum.ToObject(type, BitConverter.ToInt32(Convert.FromBase64String(input), 0));
            //Other class.
            else
                return Deserialize(input, type);
        }

        //     |                    decimal                    |
        //     |  int[0]   |  int[1]   |  int[2]   |  int[3]   |
        //     |b0|b0|b0|b0|b0|b0|b0|b0|b0|b0|b0|b0|b0|b0|b0|b0|

        private static byte[] DecimalToBytes(decimal d)
        {
            byte[][] bytes2dim = decimal.GetBits(d).Select(x => BitConverter.GetBytes(x)).ToArray();
            byte[] bytes = new byte[16];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    bytes[i * 4 + j] = bytes2dim[i][j];
                }
            }
            return bytes;
        }
        private static decimal BytesToDecimal(byte[] bytes)
        {
            byte[][] bytes2dim = new byte[4][];
            int[] bits = new int[4];
            for (int i = 0; i < 4; i++)
            {
                bytes2dim[i] = new byte[4];
                for (int j = 0; j < 4; j++)
                {
                    bytes2dim[i][j] = bytes[i * 4 + j];
                }
                bits[i] = BitConverter.ToInt32(bytes2dim[i], 0);
            }
            return new decimal(bits);
        }

        private static string[] SmartSplit(string s, char c, char[] triggersFrom, char[] triggersTo)
        {
            List<string> strings = new List<string>();
            int prevIndex = 0;
            int rank = 0;
            char? triggerFromNow = null;
            char? triggerToNow = null;
            for (int i = 0; i < s.Length; i++)
            {
                //If trigger not assigned and current char contains one of param triggers, assign the trigger.
                if (triggerFromNow == null && triggersFrom.Contains(s[i]))
                {
                    //Set trigger from.
                    triggerFromNow = s[i];
                    //Set trigger to base by trigger from.
                    triggerToNow = triggersTo[Array.IndexOf(triggersFrom, s[i])];
                }

                if (s[i] == triggerFromNow)
                    rank++;
                else if (s[i] == triggerToNow)
                    rank--;

                if (rank != 0)
                    continue;

                if (s[i] == c)
                {
                    strings.Add(s.Substring(prevIndex, i - prevIndex));
                    i = s.IndexOf(c, i);
                    prevIndex = i + 1;
                }
            }
            //Add the last element if it not alone (else if input = "", output will be [""], but i need []).
            if (strings.Count > 0)
                strings.Add(s.Substring(prevIndex));
            return strings.ToArray();
        }
    }
}