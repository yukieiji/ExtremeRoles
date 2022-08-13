using Hazel;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion
    {
        public enum XionRpcOpsCode : byte
        {

        }

        public static void UseAbility(ref MessageReader reader)
        {
            byte playerId = reader.ReadByte();
            XionRpcOpsCode ops = (XionRpcOpsCode)reader.ReadByte();
            switch (ops)
            {
                default:
                    break;
            }
        }

    }
}
