using System;

namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleAwake<T> : IRoleUpdate where T :  struct, Enum
    {
        public bool IsAwake { get; }
        public T NoneAwakeRole { get; }

        public string GetFakeOptionString();

    }
}
