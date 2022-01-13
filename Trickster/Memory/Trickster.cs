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
using System.Runtime.InteropServices;

namespace TheLeftExit.Trickster.Memory {
    public class TricksterException : Exception { }

    public record struct TypeInfo(string Name, nuint Address, nuint Offset) {
        public override string ToString() {
            return $"{Name} - {Offset:X}";
        }
    }

    public unsafe struct MemoryRegionInfo {
        public void* BaseAddress;
        public nuint Size;
        public MemoryRegionInfo(void* baseAddress, nuint size) { BaseAddress = baseAddress; Size = size; }
    }

    public unsafe struct MemoryRegion {
        public void* Pointer;
        public void* BaseAddress;
        public nuint Size;
        public MemoryRegion(void* pointer, void* baseAddress, nuint size) { Pointer = pointer; BaseAddress = baseAddress; Size = size; }
    }

    public unsafe class Trickster : IDisposable {
        private HANDLE _processHandle;

        private nuint _mainModuleBaseAddress;
        private nuint _mainModuleSize;
        private bool _is32Bit;

        public TypeInfo[] ScannedTypes;
        public MemoryRegion[] Regions;

        public Trickster(Process process) {
            _processHandle = Kernel32.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, true, (uint)process.Id);
            if (_processHandle.IsNull) throw new TricksterException();

            _mainModuleBaseAddress = (nuint)process.MainModule.BaseAddress.ToPointer();
            _mainModuleSize = (nuint)process.MainModule.ModuleMemorySize;

            BOOL is32Bit; Kernel32.IsWow64Process(_processHandle, &is32Bit); _is32Bit = is32Bit;
        }

        private TypeInfo[] ScanTypesCore() {
            List<TypeInfo> list = new();

            using (RttiScanner processMemory = new(_processHandle, _mainModuleBaseAddress, _mainModuleSize)) {
                nuint inc = (nuint)(_is32Bit ? 4 : 8);
                Func<ulong, string> getClassName = _is32Bit ? processMemory.GetClassName32 : processMemory.GetClassName64;
                for (nuint offset = inc; offset < _mainModuleSize; offset += inc) {
                    nuint address = _mainModuleBaseAddress + offset;
                    if (getClassName(address) is string className) {
                        list.Add(new TypeInfo(className, address, offset));
                    }
                }
            }

            return list.ToArray();
        }

        private MemoryRegionInfo[] ScanRegionInfoCore() {
            ulong stop = _is32Bit ? uint.MaxValue : 0x7ffffffffffffffful;
            nuint size = (nuint)sizeof(MEMORY_BASIC_INFORMATION);

            List<MemoryRegionInfo> list = new();

            MEMORY_BASIC_INFORMATION mbi;
            nuint address = 0;


            while (address < stop && Kernel32.VirtualQueryEx(_processHandle, (void*)address, &mbi, size) > 0 && address + mbi.RegionSize > address) {
                if (mbi.State == VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT &&
                    !mbi.Protect.HasFlag(PAGE_PROTECTION_FLAGS.PAGE_NOACCESS) &&
                    !mbi.Protect.HasFlag(PAGE_PROTECTION_FLAGS.PAGE_GUARD) &&
                    !mbi.Protect.HasFlag(PAGE_PROTECTION_FLAGS.PAGE_NOCACHE))
                    list.Add(new MemoryRegionInfo(mbi.BaseAddress, mbi.RegionSize));
                address += mbi.RegionSize;
            }

            return list.ToArray();
        }

        private MemoryRegion[] ReadRegionsCore(MemoryRegionInfo[] infoArray) {
            MemoryRegion[] regionArray = new MemoryRegion[infoArray.Length];
            for(int i = 0; i < regionArray.Length; i++) {
                void* baseAddress = infoArray[i].BaseAddress;
                nuint size = infoArray[i].Size;
                void* pointer = NativeMemory.Alloc(size);
                Kernel32.ReadProcessMemory(_processHandle, baseAddress, pointer, size);
                regionArray[i] = new(pointer, baseAddress, size);
            }
            return regionArray;
        }

        private void FreeRegionsCore(MemoryRegion[] regionArray) {
            for(int i = 0; i < regionArray.Length; i++) {
                NativeMemory.Free(regionArray[i].Pointer);
            }
        }

        private ulong[] ScanRegionsCore(MemoryRegion[] regionArray, ulong value) {
            List<ulong> list = new();

            Parallel.For(0, regionArray.Length, i => {
                MemoryRegion region = regionArray[i];
                byte* start = (byte*)region.Pointer;
                byte* end = start + region.Size;
                if (_is32Bit) {
                    for (byte* a = start; a < end; a += 4)
                        if (*(uint*)a == value)
                            lock (list) {
                                ulong result = (ulong)region.BaseAddress + (ulong)(a - start);
                                list.Add(result);
                            }
                } else {
                    for (byte* a = start; a < end; a += 8)
                        if (*(ulong*)a == value)
                            lock (list) {
                                ulong result = (ulong)region.BaseAddress + (ulong)(a - start);
                                list.Add(result);
                            }
                }
            });

            return list.ToArray();
        }

        public void ScanTypes() {
            ScannedTypes = ScanTypesCore();
        }

        public void ReadRegions() {
            if(Regions is not null) {
                FreeRegionsCore(Regions);
            }

            MemoryRegionInfo[] scannedRegions = ScanRegionInfoCore();
            Regions = ReadRegionsCore(scannedRegions);
        }

        public ulong[] ScanRegions(ulong value) {
            return ScanRegionsCore(Regions, value);
        }

        public void Dispose() {
            if (Regions is not null) {
                FreeRegionsCore(Regions);
            }
        }
    }
}
