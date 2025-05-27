
namespace Core_Engine.Modules.Networking.Types
{
    public static class UUID
    {
        public static byte[] GetBytes(UInt128 uuid)
        {
            return BitConverter.GetBytes(uuid);
        }
    }
}