using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KlimSoft
{
    public static class Serializer
    {
        [AttributeUsage(AttributeTargets.Field)]
        public class Ignore : Attribute { }
        public static string Serialize(object obj, Type type)
        {
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
                res += $"\"{(@bool ? '1' : '0')}\"";
            //byte.
            else if (obj is byte @byte)
                res += $"\"{(BytesToEcoHex(new byte[] { @byte }))}\"";
            //sbyte.
            else if (obj is sbyte @sbyte)
                res += $"\"{(BytesToEcoHex(new byte[] { (byte)@sbyte }))}\"";
            //short.
            else if (obj is short @short)
                res += res += $"\"{(BytesToEcoHex(BitConverter.GetBytes(@short)))}\"";
            //ushort.
            else if (obj is ushort @ushort)
                res += $"\"{(BytesToEcoHex(BitConverter.GetBytes(@ushort)))}\"";
            //int.
            else if (obj is int @int)
                res += $"\"{(BytesToEcoHex(BitConverter.GetBytes(@int)))}\"";
            //uint.
            else if (obj is uint @uint)
                res += $"\"{(BytesToEcoHex(BitConverter.GetBytes(@uint)))}\"";
            //long.
            else if (obj is long @long)
                res += $"\"{(BytesToEcoHex(BitConverter.GetBytes(@long)))}\"";
            //ulong.
            else if (obj is ulong @ulong)
                res += $"\"{(BytesToEcoHex(BitConverter.GetBytes(@ulong)))}\"";
            //float.
            else if (obj is float @float)
                res += $"\"{(BytesToEcoHex(BitConverter.GetBytes(@float)))}\"";
            //double.
            else if (obj is double @double)
                res += $"\"{(BytesToEcoHex(BitConverter.GetBytes(@double)))}\"";
            //decimal.
            else if (obj is decimal @decimal)
                res += $"\"{BytesToEcoHex(DecimalToBytes(@decimal))}\"";
            //char.
            else if (obj is char @char)
                res += $"\"{BytesToHex(Encoding.UTF8.GetBytes(new char[] { @char }))}\"";
            //string.
            else if (obj is string @string)
                res += $"\"{Convert.ToBase64String(Encoding.UTF8.GetBytes(@string))}\"";
            //DateTime.
            else if (obj is DateTime @DateTime)
                res += $"\"{(BytesToEcoHex(BitConverter.GetBytes(DateTime.Ticks)))}\"";
            //TimeSpan.
            else if (obj is TimeSpan @TimeSpan)
                res += $"\"{(BytesToEcoHex(BitConverter.GetBytes(TimeSpan.Ticks)))}\"";
            //enum.
            else if (obj.GetType().IsEnum)
                res += $"\"{(BytesToEcoHex(BitConverter.GetBytes((int)obj)))}\"";
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
            //Special initialize for ScriptableObjects.
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
                        Debug.LogWarningFormat("Cannot find variable with name \"{0}\" in deserialization of \"{1}\" class. Version: {2}.", names[i], type.Name, Application.version);
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
                return EcoHexToBytes(input, 1)[0];
            //sbyte.
            else if (type == typeof(sbyte))
                return (sbyte)EcoHexToBytes(input, 1)[0];
            //short.
            else if (type == typeof(short))
                return BitConverter.ToInt16(EcoHexToBytes(input, 2), 0);
            //ushort.
            else if (type == typeof(ushort))
                return BitConverter.ToUInt16(EcoHexToBytes(input, 2), 0);
            //int.
            else if (type == typeof(int))
                return BitConverter.ToInt32(EcoHexToBytes(input, 4), 0);
            //uint.
            else if (type == typeof(uint))
                return BitConverter.ToUInt32(EcoHexToBytes(input, 4), 0);
            //long.
            else if (type == typeof(long))
                return BitConverter.ToInt64(EcoHexToBytes(input, 8), 0);
            //ulong.
            else if (type == typeof(ulong))
                return BitConverter.ToUInt64(EcoHexToBytes(input, 8), 0);
            //float.
            else if (type == typeof(float))
                return BitConverter.ToSingle(EcoHexToBytes(input, 4), 0);
            //double.
            else if (type == typeof(double))
                return BitConverter.ToDouble(EcoHexToBytes(input, 8), 0);
            //decimal.
            else if (type == typeof(decimal))
                return BytesToDecimal(EcoHexToBytes(input, 16));
            //char.
            else if (type == typeof(char))
                return Encoding.UTF8.GetChars(HexToBytes(input))[0];
            //string.
            else if (type == typeof(string))
                return Encoding.UTF8.GetString(Convert.FromBase64String(input));
            //DateTime.
            else if (type == typeof(DateTime))
                return new DateTime(BitConverter.ToInt64(EcoHexToBytes(input, 8), 0));
            //TimeSpan.
            else if (type == typeof(TimeSpan))
                return new TimeSpan(BitConverter.ToInt64(EcoHexToBytes(input, 8), 0));
            //enum.
            else if (type.IsEnum)
                return Enum.ToObject(type, BitConverter.ToInt32(EcoHexToBytes(input, 4), 0));
            //Other class.
            else
                return Deserialize(input, type);
        }

        //     |                    decimal                    |
        //     |  int[0]   |  int[1]   |  int[2]   |  int[3]   |
        //     |b0|b1|b2|b3|b4|b5|b6|b7|b8|b9|bA|bB|bC|bD|bE|bF|

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
        //AB - eco, 000000AB - not eco.
        private static string BytesToEcoHex(byte[] bytes)
        {
            //Convert to standard hex.
            string hex = BitConverter.ToString(bytes).RemoveChar('-');
            //And then trim zeros at start.
            return hex.TrimEnd('0');
        }
        private static string BytesToHex(byte[] bytes)
        {
            //Convert to standard hex.
            string hex = BitConverter.ToString(bytes).RemoveChar('-');
            return hex;
        }
        private static byte[] EcoHexToBytes(string ecoHex, int length)
        {
            //Future standard hex length.
            int neededHexLen = length * 2;
            //Standard hex.
            string hex = null;
            //Add main part (ecoHex).
            hex += ecoHex;
            //Add zeros to it`s end.
            for (int i = ecoHex.Length; i < neededHexLen; i++)
                hex += '0';
            //Finally, convert standard hex.
            byte[] res = new byte[length];
            for (int i = 0; i < neededHexLen; i += 2)
                res[i / 2] = Convert.ToByte(new string(new char[] { hex[i], hex[i + 1] }), 16);
            return res;
        }
        private static byte[] HexToBytes(string hex)
        {
            //Convert standard hex.
            byte[] res = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
                res[i / 2] = Convert.ToByte(new string(new char[] { hex[i], hex[i + 1] }), 16);
            return res;
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