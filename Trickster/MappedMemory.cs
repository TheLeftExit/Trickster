#pragma warning disable CS0436
#pragma warning disable CA1416

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

namespace Trickster {
    public class MemoryRegion<T> where T : unmanaged {
        public ulong BaseAddress;
        public ulong Size;
        public Memory<byte> Memory;
        public List<ulong> Addresses;

        public unsafe void Scan(Func<T, bool> predicate) {
            List<ulong> result = new();
            nuint size = (nuint)sizeof(T);
            fixed(byte* basePointer = Memory.Span) {
                if (Addresses == null)
                    for (ulong a = 0; a < Size - size; a += size) {
                        if (predicate(Unsafe.Read<T>((void*)((ulong)basePointer + a))))
                            result.Add(a);
                    }
                else
                    foreach (ulong a in Addresses) {
                        if (predicate(Unsafe.Read<T>((void*)(a + (ulong)basePointer))))
                            result.Add(a);
                    }
            }
            Addresses = result;
        }
    }

    public class MappedMemory<T> where T : unmanaged {
        private HANDLE handle;
        private List<MemoryRegion<T>> regions;

        public MappedMemory(HANDLE pHandle) => handle = pHandle;

        // Initializes regions without allocating any memory
        public unsafe void LoadRegions() {
            // This routine is an almost 1-to-1 transcription of Cheat Engine's GetMemoryRegions procedure.
            // (the only thing I've changed is splicing adjanced regions, see lines 66-68)
            ulong stop = 0x7fffffffffffffff;
            nuint size = (nuint)sizeof(MEMORY_BASIC_INFORMATION);

            MEMORY_BASIC_INFORMATION mbi;
            ulong address = 0;

            regions = new List<MemoryRegion<T>>();

            while (address < stop && Kernel32.VirtualQueryEx(handle, (void*)address, &mbi, size) > 0 && address + mbi.RegionSize > address) {
                if (mbi.State == VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT &&
                    !mbi.Protect.HasFlag(PAGE_PROTECTION_FLAGS.PAGE_NOACCESS) &&
                    !mbi.Protect.HasFlag(PAGE_PROTECTION_FLAGS.PAGE_GUARD) &&
                    !mbi.Protect.HasFlag(PAGE_PROTECTION_FLAGS.PAGE_NOCACHE)) {
                    if (regions.Count > 0 && regions[^1].BaseAddress + regions[^1].Size == (ulong)mbi.BaseAddress)
                        regions[^1].Size += (ulong)mbi.RegionSize;
                    else
                        regions.Add(new MemoryRegion<T> {
                            BaseAddress = (ulong)mbi.BaseAddress,
                            Size = (ulong)mbi.RegionSize
                        });
                }

                address += mbi.RegionSize;
            }
        }

        // Allocates memory for regions
        public void AllocateRegions() {
            for (int i = 0; i < regions.Count; i++)
                regions[i].Memory = new byte[regions[i].Size];
        }

        // Reads process memory into regions
        public unsafe void ReadRegions() {
            for(int i = 0; i < regions.Count; i++) {
                bool read;
                fixed (void* basePointer = regions[i].Memory.Span)
                    read = Kernel32.ReadProcessMemory(handle, (void*)regions[i].BaseAddress, basePointer, (nuint)regions[i].Size);
                if (!read)
                    regions.RemoveAt(i--);
            }
        }

        // Invokes a scan on all remaining regions
        public void Scan(Func<T, bool> predicate) {
            Parallel.For(0, regions.Count, i => regions[i].Scan(predicate));
        }

        // Removes any regions that don't have any addresses remaining
        public void Truncate() {
            for (int i = 0; i < regions.Count; i++)
                if (regions[i].Addresses.Count == 0)
                    regions.RemoveAt(i--);
        }

        public IEnumerable<ulong> GetAddresses() {
            foreach (MemoryRegion<T> region in regions)
                foreach (ulong address in region.Addresses)
                    yield return address;
        }

        public int GetRegionCount() => regions.Count;
    }
}
