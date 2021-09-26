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

namespace Trickster {
    class Program {
        static unsafe void Main(string[] args) {
            Console.WriteLine("Trickster demo.");
            Console.WriteLine("This tool scans the process memory for a value, similar to what Cheat Engine does.");
            Console.WriteLine("This demo is hardcoded to attach to Growtopia.exe and search for a UInt16 of your choice.");
            Console.WriteLine("Press any key to attach and initialize the memory regions...");
            Console.ReadKey();

            Process p = Process.GetProcessesByName("Growtopia").Single();
            HANDLE handle = Kernel32.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, false, (uint)p.Id);

            try {
                MappedMemory<ushort> memory = new MappedMemory<ushort>(handle);

                memory.LoadRegions();
                memory.AllocateRegions();

                Stopwatch sw = new Stopwatch();

                while (true) {
                    Console.Write("Value: ");
                    ushort searchValue = ushort.Parse(Console.ReadLine());

                    sw.Restart();

                    memory.ReadRegions();
                    memory.Scan(x => x == searchValue);
                    memory.Truncate();

                    sw.Stop();

                    Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
                    Console.WriteLine($"Addresses found: {memory.GetAddresses().Count()}");
                    Console.WriteLine($"Regions remaining: {memory.GetRegionCount()}");
                    Console.WriteLine();
                }

            } catch {
                Kernel32.CloseHandle(handle);
                throw;
            } finally {
                Kernel32.CloseHandle(handle);
            }
        }
    }
}
