using System;
using System.Collections.Generic;
using System.Linq;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Windows.Win32;
using Windows.Win32.System.Memory;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;

namespace TheLeftExit.Trickster {
    public unsafe sealed class Process : IDisposable {
        public HANDLE Handle { get; }

        public Process(uint processId) {
            Handle = Kernel32.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, false, processId);
            if (Handle.IsNull)
                throw new ApplicationException($"Unable to open processId {processId}.");
        }

        public void Dispose() {
            Kernel32.CloseHandle(Handle);
        }

        public List<MemoryRegionInfo> GetRegions() {
            ulong stop = 0x7fffffffffffffff;
            nuint size = (nuint)sizeof(MEMORY_BASIC_INFORMATION);

            List<MemoryRegionInfo> regions = new();

            MEMORY_BASIC_INFORMATION mbi;
            ulong address = 0;

            while (address < stop && Kernel32.VirtualQueryEx(Handle, (void*)address, &mbi, size) > 0 && address + mbi.RegionSize > address) {
                if (mbi.State == VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT &&
                    !mbi.Protect.HasFlag(PAGE_PROTECTION_FLAGS.PAGE_NOACCESS) &&
                    !mbi.Protect.HasFlag(PAGE_PROTECTION_FLAGS.PAGE_GUARD) &&
                    !mbi.Protect.HasFlag(PAGE_PROTECTION_FLAGS.PAGE_NOCACHE))
                    regions.Add(new MemoryRegionInfo(mbi));
                address += mbi.RegionSize;
            }

            return regions;
        }
    }
}
