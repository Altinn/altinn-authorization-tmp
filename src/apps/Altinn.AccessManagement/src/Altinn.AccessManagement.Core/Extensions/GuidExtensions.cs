namespace Altinn.AccessManagement.Core.Extensions;

/// <summary>
/// Provides extension methods for Guid.
/// </summary>
/// <remarks>
/// This class contains extension methods that simplify working with uuids.
/// </remarks>
public static class GuidExtensions
{
    /// <summary>
    /// Verifies if the given Guid is a version 7 UUID.
    /// </summary>
    /// <param name="uuid">The uuid to verify</param>
    /// <returns>True if the uuid is a version 7 UUID, otherwise false.</returns>
    public static bool IsVersion7Uuid(this Guid uuid)
    {
        byte[] bytes = uuid.ToByteArray();
        byte version = (byte)((bytes[7] >> 4) & 0x0F);
        return version == 7;
    }
}
