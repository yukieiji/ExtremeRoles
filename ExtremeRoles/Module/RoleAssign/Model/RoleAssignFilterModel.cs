using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace ExtremeRoles.Module.RoleAssign.Model;

public sealed class RoleAssignFilterModel
{
    public AddRoleMenuModel AddRoleMenu { get; set; }
    public Dictionary<Guid, RoleFilterSetModel> FilterSet { get; set; }

    public string SerializeToString()
    {
        StringBuilder builder = new StringBuilder();

        foreach (var filter in this.FilterSet.Values)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(RoleFilterSetModel));
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, filter);
                byte[] serializedBytes = stream.ToArray();
                string base64String = Convert.ToBase64String(serializedBytes);
                builder.Append(base64String).Append(',');
            }
        }
        string serializeStr = builder.ToString();
        if (serializeStr.EndsWith(','))
        {
            serializeStr = serializeStr.Remove(serializeStr.Length - 1);
        }
        return serializeStr;
    }

    public void DeserializeFromString(string serializedStr)
    {
        string[] splitedSerializedStr = serializedStr.Split(',');


        foreach (string encodingFilter in splitedSerializedStr)
        {
            byte[] deserializedBytes = Convert.FromBase64String(encodingFilter);

            RoleFilterSetModel model;
            DataContractSerializer serializer = new DataContractSerializer(
                typeof(RoleFilterSetModel));
            using (MemoryStream stream = new MemoryStream(deserializedBytes))
            {
                model = (RoleFilterSetModel)serializer.ReadObject(stream);
            }

            this.FilterSet.Add(Guid.NewGuid(), model);
        }
    }
}
