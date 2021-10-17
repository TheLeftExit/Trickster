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

using TheLeftExit.Memory.Sources;
using TheLeftExit.Memory.RTTI;
using TheLeftExit.Memory.ObjectModel;

namespace TheLeftExit.Trickster {
    public class TypeScanner : IDisposable {
        public MyProcessMemory Process;
        public TypeInfo[] Types;
        public CachedMemory[] Regions;

        public ulong MainModuleBaseAddress;
        public int MainModuleSize;
        public bool Is32Bit;

        public unsafe TypeScanner(Process process) {
            Process = new MyProcessMemory((uint)process.Id);
            MainModuleBaseAddress = (ulong)process.MainModule.BaseAddress;
            MainModuleSize = process.MainModule.ModuleMemorySize;
            BOOL is32Bit; // CsWin32 is still in development...
            Kernel32.IsWow64Process(new HANDLE(Process.Handle), &is32Bit);
            Is32Bit = is32Bit;
        }

        public void InitTypes() => Types = Process.GetTypes(MainModuleBaseAddress, MainModuleSize, Is32Bit).ToArray();
        
        public void InitRegions() => Regions = Process.GetRegions().Select(x => new CachedMemory(x.BaseAddress, x.Size)).ToArray();
        
        public void ReadRegions() {
            for(int i = 0; i < Regions.Length; i++) {
                ref CachedMemory region = ref Regions[i];
                if (!Process.TryRead(region.BaseAddress, region.Memory.Span))
                    region = null;
            }
        }

        public ulong[] ScanRegions(ulong value) {
            List<ulong> result = new();
            Parallel.For(0, Regions.Length, i => {
                ref CachedMemory region = ref Regions[i];
                if (region == null) return;
                ulong mainModuleEnd = ObjectModelExtensions.ApplyOffset(region.BaseAddress, region.Size);
                if (Is32Bit) {
                    for (ulong a = region.BaseAddress; a < mainModuleEnd; a += 0x04)
                        if (region.ReadRef<uint>(a) == value)
                            lock (result)
                                result.Add(a);
                } else {
                    for (ulong a = region.BaseAddress; a < mainModuleEnd; a += 0x08)
                        if (region.ReadRef<ulong>(a) == value)
                            lock (result)
                                result.Add(a);
                }
            });
            return result.ToArray();
        }

        public void Dispose() => Process?.Dispose();
    }

    public readonly struct TypeInfo {
        public readonly int Offset;
        public readonly string[] Names;
        public TypeInfo(int offset, string[] names) {
            Offset = offset;
            Names = names;
        }
    }

    public class MyProcessMemory : ProcessMemory {
        public MyProcessMemory(uint processId, uint rights = (uint)PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, bool inheritHandle = true) :
            base(processId, rights, inheritHandle) { }

        public readonly struct MemoryRegionInfo {
            public readonly ulong BaseAddress;
            public readonly int Size;
            public unsafe MemoryRegionInfo(MEMORY_BASIC_INFORMATION mbi) {
                BaseAddress = (ulong)mbi.BaseAddress;
                Size = (int)mbi.RegionSize;
            }
        }

        public unsafe List<MemoryRegionInfo> GetRegions(bool is32Bit = false) {
            ulong stop = is32Bit ? uint.MaxValue : 0x7ffffffffffffffful;
            nuint size = (nuint)sizeof(MEMORY_BASIC_INFORMATION);

            List<MemoryRegionInfo> regions = new();

            MEMORY_BASIC_INFORMATION mbi;
            ulong address = 0;

            HANDLE handle = new HANDLE(this.Handle);

            while (address < stop && Kernel32.VirtualQueryEx(handle, (void*)address, &mbi, size) > 0 && address + mbi.RegionSize > address) {
                if (mbi.State == VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT &&
                    !mbi.Protect.HasFlag(PAGE_PROTECTION_FLAGS.PAGE_NOACCESS) &&
                    !mbi.Protect.HasFlag(PAGE_PROTECTION_FLAGS.PAGE_GUARD) &&
                    !mbi.Protect.HasFlag(PAGE_PROTECTION_FLAGS.PAGE_NOCACHE))
                    regions.Add(new MemoryRegionInfo(mbi));
                address += mbi.RegionSize;
            }

            return regions;
        }

        public List<TypeInfo> GetTypes(ulong mainModuleBaseAddress, int mainModuleSize, bool is32Bit = false) {
            List<TypeInfo> types = new();

            for (int i = is32Bit ? 4 : 8; i < mainModuleSize; i += is32Bit ? 0x04 : 0x08) {
                ulong target = ObjectModelExtensions.ApplyOffset(mainModuleBaseAddress, i);
                string[] names = is32Bit ? this.GetRTTIClassNamesByStructure32(target) : this.GetRTTIClassNamesByStructure64(target);
                if (names != null)
                    types.Add(new TypeInfo(i, names));
            }

            return types;
        }
    }
}