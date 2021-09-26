using System;
using System.Diagnostics;
using System.Linq;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using Windows.Win32.System.Memory;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;

namespace TheLeftExit.Trickster {
    public struct MemoryRegionInfo {
        public ulong BaseAddress;
        public int Size;
        public unsafe MemoryRegionInfo(MEMORY_BASIC_INFORMATION mbi) {
            BaseAddress = (ulong)mbi.BaseAddress;
            Size = (int)mbi.RegionSize;
        }
    }

    public delegate bool ScanCondition(Span<byte> bytes);

    public unsafe struct MemoryRegion {
        public ulong BaseAddress;
        public int Size;
        public Memory<byte> Data;

        public MemoryRegion(MemoryRegionInfo info) {
            BaseAddress = info.BaseAddress;
            Size = info.Size;
            Data = null;
        }

        public bool Read(HANDLE handle) {
            if (Data.Length == 0) Data = new byte[Size];
            fixed (void* dataPtr = Data.Span)
                return Kernel32.ReadProcessMemory(handle, (void*)BaseAddress, dataPtr, (nuint)Size);
        }

        // Scan existing addresses, or generate a list of new ones.
        public List<ulong> Scan(int size, ScanCondition predicate, List<ulong> knownAddresses = null) {
            List<ulong> result = new();
            if (knownAddresses == null)
                for (int a = 0; a < Size - size; a += size) {
                    if (predicate(Data.Slice(a, size).Span))
                        result.Add(BaseAddress + (ulong)a);
                }
            else
                foreach (ulong a in knownAddresses) {
                    if (predicate(Data.Slice((int)(a - BaseAddress), size).Span))
                        result.Add(a);
                }
            return result;
        }
    }

    public class MemoryScanner {
        public Process Process;
        public (MemoryRegion MemoryRegion, List<ulong> Addresses)[] Regions;

        public MemoryScanner(Process process) {
            Process = process;
        }

        public void Reset() {
            Regions = null;
        }

        public void Scan(int size, ScanCondition predicate) {
            // If this is a first scan, get currently available regions. Address lists will remain null.
            Regions ??= Process.GetRegions().Select(x => (new MemoryRegion(x), (List<ulong>)null)).ToArray();
            // Read all regions.
            for (int i = 0; i < Regions.Length; i++)
                if (!Regions[i].MemoryRegion.Read(Process.Handle))
                    Regions[i].Addresses = new List<ulong>();
            // Scan each region in parallel.
            Parallel.For(0, Regions.Length, i => {
                var region = Regions[i];
                List<ulong> newAddresses = region.MemoryRegion.Scan(size, predicate, region.Addresses);
                Regions[i] = (region.MemoryRegion, newAddresses);
            });
            // Remove any regions with no addresses.
            Regions = Regions.Where(x => x.Addresses.Count > 0).ToArray();
        }

        public IEnumerable<ulong> GetAddresses() {
            foreach (var region in Regions)
                foreach (ulong address in region.Addresses)
                    yield return address;
        }
    }
}