using System.Diagnostics.CodeAnalysis;

namespace System.IO;

internal static class BinarySerializerExtensions
{
    public const int NullArrayLength = -1;

    public static float[]? ReadFloatArray(this BinaryReader reader)
    {
        var length = reader.ReadArrayLength<float>(out var array);

        if (length < 1)
        {
            return array;
        }

        array = new float[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = reader.ReadSingle();
        }

        return array;
    }

    public static short[]? ReadInt16Array(this BinaryReader reader)
    {
        var length = reader.ReadArrayLength<short>(out var array);

        if (length < 1)
        {
            return array;
        }

        array = new short[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = reader.ReadInt16();
        }

        return array;
    }

    public static int[]? ReadInt32Array(this BinaryReader reader)
    {
        var length = reader.ReadArrayLength<int>(out var array);

        if (length < 1)
        {
            return array;
        }

        array = new int[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = reader.ReadInt32();
        }

        return array;
    }

    public static string?[]? ReadStringArray(this BinaryReader reader)
    {
        var length = reader.ReadArrayLength<string?>(out var array);

        if (length < 1)
        {
            return array;
        }

        array = new string[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = reader.ReadNullableString();
        }

        return array;
    }

    public static void WriteFloatArray(this BinaryWriter writer, float[]? array)
    {
        if (writer.WriteArrayLength(array))
        {
            foreach (var item in array)
            {
                writer.Write(item);
            }
        }
    }

    public static void WriteInt16Array(this BinaryWriter writer, short[]? array)
    {
        if (writer.WriteArrayLength(array))
        {
            foreach (var item in array)
            {
                writer.Write(item);
            }
        }
    }

    public static void WriteInt32Array(this BinaryWriter writer, int[]? array)
    {
        if (writer.WriteArrayLength(array))
        {
            foreach (var item in array)
            {
                writer.Write(item);
            }
        }
    }

    public static void WriteStringArray(this BinaryWriter writer, string?[]? array)
    {
        if (writer.WriteArrayLength(array))
        {
            foreach (var item in array)
            {
                writer.WriteString(item);
            }
        }
    }

    #region length

    public static int ReadArrayLength<T>(this BinaryReader reader, out T[]? array)
    {
        var length = reader.ReadInt32();

        array = length == 0
                ? Array.Empty<T>()
                : null;
        return length;
    }

    public static bool WriteArrayLength<T>(this BinaryWriter writer, [NotNullWhen(true)] T[]? array)
    {
        if (array is null)
        {
            writer.Write(NullArrayLength);
            return false;
        }
        else if (array.Length == 0)
        {
            writer.Write(0);
            return false;
        }
        else
        {
            writer.Write(array.Length);
            return true;
        }
    }

    #endregion length

    #region string

    private static string? ReadNullableString(this BinaryReader reader)
    {
        if (reader.ReadBoolean())
        {
            return reader.ReadString();
        }
        return null;
    }

    private static void WriteString(this BinaryWriter writer, string? item)
    {
        if (item is null)
        {
            writer.Write(false);
        }
        else
        {
            writer.Write(true);
            writer.Write(item);
        }
    }

    #endregion string

#if NETSTANDARD2_0_OR_GREATER

    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool _)
        {
        }
    }

#endif
}