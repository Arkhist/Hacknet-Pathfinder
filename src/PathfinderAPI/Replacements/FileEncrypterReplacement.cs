using System.Runtime.InteropServices;
using System.Text;
using Hacknet;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Pathfinder.Util;

namespace Pathfinder.Replacements;

[HarmonyPatch]
internal static class FileEncrypterReplacement
{
    // Two things need to be replaced here
    // First, decryption needs to be attempted at max 4 times for all three hash types when it wasn't created by Pathfinder.
    // 1. Windows .NET Framework 64 bit's hash
    // 2. Windows .NET Framework 32 bit's hash (yes, they're different)
    // 3. Mono's older hash (the one shipped with Hacknet on Linux, newer Mono uses the NetFX hash (i think))
    // 4. as a last resort, try GetHashCode (bleh)
    // Second, encryption should *always* use Pathfinder's stable hash.
    // Files encrypted by Pathfinder will get an extra header section just with PATHFINDER, so we can tell which are stable.
    // Stable hash I'm using here is FNV1a xor-folded to 16 bits, for its simplicity. https://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
    // We don't need anything good (we only have 16 bits in the first place), I don't care that its not for cryptographic use
    // ALL I care about is that the hash is *stable*.
    // - Aaron
    
    // Just keep all the legacy hash functions in an array to make this easy to loop over
    private static readonly Func<string, ushort>[] _hashFunctions = [Fx64Hash, MonoHash, Fx32Hash, GetHashCodeHashBad];

    private static readonly ushort Fnv1aEmptyHash = Fnv1aHash(string.Empty);
    
    // *technically* an extension could have this as a file extension, but... i dont think thats going to happen
    private static readonly string PathfinderNeedle = FileEncrypter.Encrypt("PATHFINDER", Fnv1aHash("PATHFINDER"));

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FileEncrypter), nameof(FileEncrypter.DecryptString))]
    private static bool DecryptStringPrefix(string data, string pass, out string[] __result)
    {
        if (string.IsNullOrEmpty(data))
        {
            throw new NullReferenceException("String to decrypt cannot be null or empty");
        }
        var lines = data.Split(Utils.robustNewlineDelim, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
        {
            throw new FormatException("Tried to decrypt an invalid valid DEC ENC file \"" + data + "\" - not enough elements. Need 2 lines, had " + lines.Length);
        }
        var headerParts = lines[0].Split(FileEncrypter.HeaderSplitDelimiters, StringSplitOptions.None);
        if (headerParts.Length < 4)
        {
            throw new FormatException("Tried to decrypt an invalid valid DEC ENC file \"" + data + "\" - not enough headers");
        }

        if (headerParts[headerParts.Length - 1] == PathfinderNeedle)
        {
            // we know this is FNV1a
            var hash = Fnv1aHash(pass);
            var passcodeCheck = FileEncrypter.Decrypt(headerParts[3], hash);
            var correctPass = passcodeCheck == "ENCODED";
            __result =
            [
                // header text
                FileEncrypter.Decrypt(headerParts[1], Fnv1aEmptyHash),
                // IP address
                FileEncrypter.Decrypt(headerParts[2], Fnv1aEmptyHash),
                // actual content
                correctPass ? FileEncrypter.Decrypt(lines[1], hash) : null,
                // extension
                headerParts.Length > 5 ? FileEncrypter.Decrypt(headerParts[4], Fnv1aEmptyHash) : null,
                // success marker
                correctPass ? "1" : "0",
                // whatever the decryption attempt for ENCODED came back as (even if it failed)
                passcodeCheck
            ];
            return false;
        }

        foreach (var hashFunction in _hashFunctions)
        {
            ushort attempedHash = hashFunction(pass);
            // this can still hypothetically be wrong if there is a collision between the hash functions
            // but i really just do not care at that point, sorry for the people playing on vanilla saves with pathfinder who hit those odds
            var decodedMarker = FileEncrypter.Decrypt(headerParts[3], attempedHash);
            var correct = decodedMarker == "ENCODED";
            if (correct)
            {
                var emptyHash = hashFunction(string.Empty);
                __result =
                [
                    FileEncrypter.Decrypt(headerParts[1], emptyHash),
                    FileEncrypter.Decrypt(headerParts[2], emptyHash),
                    FileEncrypter.Decrypt(lines[1], attempedHash),
                    headerParts.Length > 4 ? FileEncrypter.Decrypt(headerParts[4], emptyHash) : null,
                    "1",
                    decodedMarker
                ];
                return false;
            }
        }
        
        // password wasn't correct, time to guess at header decoding! :)
        // the best way i can think of to do this is to:
        // 1. IP is
        //   a. "ERROR" (default value)
        //   b. exists on the netmap
        //   c. just contains three dots (xxx.xxx.xxx.xxx)
        // 2. header text is "ERROR" for good measure
        // should cover most cases
        foreach (var hashFunction in _hashFunctions)
        {
            var emptyHash = hashFunction(string.Empty);
            var decodedIp = FileEncrypter.Decrypt(headerParts[2], emptyHash);
            var headerText = FileEncrypter.Decrypt(headerParts[1], emptyHash);
            if (decodedIp == "ERROR" || ComputerLookup.FindByIp(decodedIp, false) != null || decodedIp.Count(c => c == '.') == 3 || headerText == "ERROR")
            {
                __result =
                [
                    headerText,
                    decodedIp,
                    null,
                    headerParts.Length > 4 ? FileEncrypter.Decrypt(headerParts[4], emptyHash) : null,
                    "0",
                    FileEncrypter.Decrypt(headerParts[3], hashFunction(pass))
                ];
                return false;
            }
        }
        
        // give up, use GetHashCode to give back something for headers :(
        var passHashBad = GetHashCodeHashBad(pass);
        var emptyHashBad = GetHashCodeHashBad(string.Empty);
        __result =
        [
            FileEncrypter.Decrypt(headerParts[1], emptyHashBad),
            FileEncrypter.Decrypt(headerParts[2], emptyHashBad),
            null,
            headerParts.Length > 4 ? FileEncrypter.Decrypt(headerParts[4], emptyHashBad) : null,
            "0",
            FileEncrypter.Decrypt(headerParts[3], passHashBad)
        ];
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FileEncrypter), nameof(FileEncrypter.DecryptHeaders))]
    private static bool DecryptHeadersPrefix(string data, string pass, out string[] __result)
    {
        // i'm too lazy to actually implement this.
        var fullDecrypt = FileEncrypter.DecryptString(data, pass);
        __result =
        [
            fullDecrypt[0],
            fullDecrypt[1],
            fullDecrypt[3]
        ];
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FileEncrypter), nameof(FileEncrypter.GetPassCodeFromString))]
    private static bool GetPassCodeFromStringReplacement(string code, out ushort __result)
    {
        __result = Fnv1aHash(code);
        return false;
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(FileEncrypter), nameof(FileEncrypter.EncryptString))]
    private static void EncryptStringIL(ILContext il)
    {
        var c = new ILCursor(il);

        var appendMethod = AccessTools.DeclaredMethod(typeof(StringBuilder), nameof(StringBuilder.Append), [typeof(string)]);
        
        c.GotoNext(MoveType.After, 
            x => x.MatchCallvirt(appendMethod),
            x => x.MatchPop()
        );

        c.Emit(OpCodes.Ldloc_2);
        c.Emit(OpCodes.Ldstr, "::");
        c.Emit(OpCodes.Callvirt, appendMethod);
        c.Emit(OpCodes.Ldsfld, AccessTools.DeclaredField(typeof(FileEncrypterReplacement), nameof(PathfinderNeedle)));
        c.Emit(OpCodes.Callvirt, appendMethod);
        c.Emit(OpCodes.Pop);
    }

    private static ushort Fnv1aHash(string str)
    {
        const uint OFFSET_BASIS = 2166136261;
        const uint FNV_PRIME = 16777619;

        uint hash = OFFSET_BASIS;

        foreach (var b in MemoryMarshal.AsBytes(str.AsSpan()))
        {
            hash ^= b;
            hash *= FNV_PRIME;
        }

        return (ushort)((hash >> 16) ^ (hash & ushort.MaxValue));
    }

    // stolen straight from a decompile of the mscorlib.dll shipped with Linux Hacknet
    private static unsafe ushort MonoHash(string str)
    {
        fixed (char* ptr = str)
        {
            char* ptr2 = ptr;
            char* ptr3 = (char*)((byte*)ptr2 + str.Length * 2 - 2);
            int num = 0;
            for (; ptr2 < ptr3; ptr2 += 2)
            {
                num = (num<<5) - num + *ptr2;
                num = (num<<5) - num + ptr2[1];
            }
            ptr3++;
            if (ptr2 < ptr3)
            {
                num = (num<<5) - num + *ptr2;
            }
            return (ushort)num;
        }
    }
    
    // Both the 32 and 64 bit hashes are taken from this source https://referencesource.microsoft.com/#mscorlib/system/string.cs,898

    private static unsafe ushort Fx32Hash(string str)
    {
        fixed (char* src = str)
        {
            int hash1 = (5381<<16) + 5381;
            int hash2 = hash1;
            
            int* pint = (int *)src;
            int len = str.Length;
            while (len > 2)
            {
                hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                pint += 2;
                len  -= 4;
            }
 
            if (len > 0)
            {
                hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
            }
            
            return (ushort)(hash1 + (hash2 * 1566083941));
        }
    }
    
    private static unsafe ushort Fx64Hash(string str)
    {
        fixed (char* src = str)
        {
            int hash1 = 5381;
            int hash2 = hash1;
            
            int     c;
            char* s = src;
            while ((c = s[0]) != 0) {
                hash1 = ((hash1 << 5) + hash1) ^ c;
                c = s[1];
                if (c == 0)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ c;
                s += 2;
            }
            
            return (ushort)(hash1 + (hash2 * 1566083941));
        }
    }

    private static ushort GetHashCodeHashBad(string str) => (ushort)str.GetHashCode();
}
