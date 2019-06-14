using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[Serializable]
public class SerialHelper
{
    public void Init(int size)
    {
        FieldNames = new string[size];
        FieldValues = new string[size];
    }
    public string[] FieldNames;
    public string[] FieldValues;
}

[Serializable]
public class TileEntry
{
    public int ID = 0;
    //Holds Extra Json Data Used to serialize properties on this object.
    public string ExtraData = "";
    [NonSerialized]
    Dictionary<string, FieldInfo> SerialFields = new Dictionary<string, FieldInfo>();
    public void BuildForTile(Tile t)
    {
        ID = t.GetID();
        BuildSerialData(t);
        SerialHelper Helper = new SerialHelper();
        Helper.Init(SerialFields.Count);
        int i = 0;
        foreach (KeyValuePair<string, FieldInfo> item in SerialFields)
        {
            Helper.FieldNames[i] = item.Key;
            Helper.FieldValues[i] = GetStringOfProp(item.Value, t);
            i++;
        }
        ExtraData = JsonUtility.ToJson(Helper);
    }

    string GetStringOfProp(FieldInfo i, Tile t)
    {
        Type ValueType = i.FieldType;
        object obj = i.GetValue(t);
        if (ValueType == typeof(int))
        {
            int value = (int)obj;
            return value.ToString();
        }
        if (ValueType == typeof(bool))
        {
            bool value = (bool)obj;
            return value.ToString();
        }
        if (ValueType == typeof(float))
        {
            float value = (float)obj;
            return value.ToString();
        }
        if (ValueType.IsEnum)
        {
            int value = (int)obj;
            return value.ToString();
        }
        Debug.LogError("Failed to Save Field " + ValueType);
        return "-";
    }

    void SetValueFromString(FieldInfo i, Tile t, string valuedata)
    {
        Type ValueType = i.FieldType;
        if (ValueType == typeof(int))
        {
            int Stringv = int.Parse(valuedata);
            i.SetValue(t, Stringv);
            return;
        }
        else if (ValueType == typeof(bool))
        {
            bool Stringv = bool.Parse(valuedata);
            i.SetValue(t, Stringv);
            return;
        }
        else if (ValueType == typeof(float))
        {
            float Stringv = float.Parse(valuedata);
            i.SetValue(t, Stringv);
            return;
        }
        else if (ValueType.IsEnum)
        {
            if (int.TryParse(valuedata, out int stringv))
            {
                i.SetValue(t, stringv);
                return;
            }
            else
            {
              //  Debug.LogError("Failed to Parse Field Data '" + valuedata + "' of type " + ValueType);
            }
        }
        else
        {
            Debug.LogError("Failed to Load Field " + ValueType);
        }
    }

    void BuildSerialData(Tile t)
    {
        FieldInfo[] objectFields = t.GetType().GetFields();//Adding any flags here seems to be broken as it just returns no fields.
        // search all fields and find the attribute
        for (int i = 0; i < objectFields.Length; i++)
        {
            TileAttrib attribute = Attribute.GetCustomAttribute(objectFields[i], typeof(TileAttrib)) as TileAttrib;
            if (attribute != null)
            {
                //Debug.Log("Saving " + objectFields[i].Name);
                SerialFields.Add(objectFields[i].Name, objectFields[i]);
            }
        }
    }

    public void ApplySeralToTile(Tile t)
    {
        if (ExtraData.Length == 0)
        {
            return;
        }
        BuildSerialData(t);

        SerialHelper helper = JsonUtility.FromJson<SerialHelper>(ExtraData);
        if (helper.FieldNames.Length != helper.FieldValues.Length)
        {
            Debug.LogError("Extra Data Corrupt Tile will be default");
            return;
        }
        for (int i = 0; i < helper.FieldNames.Length; i++)
        {
            FieldInfo FI = null;
            SerialFields.TryGetValue(helper.FieldNames[i], out FI);
            if (FI != null)
            {
                SetValueFromString(FI, t, helper.FieldValues[i]);
            }
        }

    }

}
