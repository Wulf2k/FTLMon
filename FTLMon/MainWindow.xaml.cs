using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FTLMon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer monTime = new System.Timers.Timer();


        const int PROCESS_ALL_ACCESS = 0x1f0fff;
        private static IntPtr FTLHandle = IntPtr.Zero;
        private static Int32 ba = 0;

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        static internal extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        static internal extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, int dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, int dwCreationFlags, IntPtr lpThreadId);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        static internal extern IntPtr OpenProcess(int dwDesiredAcess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int iSize, IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int iSize, IntPtr lpNumberOfBytesWritten);

        private static Dictionary<string, int> systems = new Dictionary<string, int>();
        
        public static void initSystems()
        {
            systems.Clear();
            systems.Add("artillery", 0);
            systems.Add("battery", 0);
            systems.Add("clonebay", 0);
            systems.Add("doors", 0);
            systems.Add("drones", 0);
            systems.Add("engines", 0);
            systems.Add("hacking", 0);
            systems.Add("medbay", 0);
            systems.Add("oxygen", 0);
            systems.Add("pilot", 0);
            systems.Add("sensors", 0);
            systems.Add("shields", 0);
            systems.Add("teleporter", 0);
            systems.Add("weapons", 0);
        }

        public static Boolean isAlphaNumeric(string strToCheck)
        {
            Regex rg = new Regex("[^a-zA-Z0-9]");
            return rg.IsMatch(strToCheck) == true ? false : true;
        }

        public bool TryAttachToClient()
        {
            Process selectedProcess = null;
            Process[] _allProcesses = Process.GetProcesses();
            try
            {
                var potentialProcesses = new List<Process>();
                foreach (Process proc in _allProcesses)
                {
                    if (proc.MainWindowTitle.ToUpper().IndexOf("FTL: FASTER THAN LIGHT") == 0)
                    {
                        potentialProcesses.Add(proc);
                    }
                }

                if (potentialProcesses.Count == 0)
                {
                    return false;
                }
                else if (potentialProcesses.Count >= 1)
                {
                    selectedProcess = potentialProcesses[0];
                }

                if (selectedProcess != null)
                {
                    FTLHandle = OpenProcess(PROCESS_ALL_ACCESS, false, selectedProcess.Id);
                    ba = selectedProcess.MainModule.BaseAddress.ToInt32();
                    return true;
                }
            }
            catch { };
            ba = 0;
            return false;
        }
        public static sbyte RInt8(Int32 Addr)
        {
            byte[] ByteBuffer = new byte[1];
            ReadProcessMemory(FTLHandle, (IntPtr)(IntPtr)Addr, ByteBuffer, 1, IntPtr.Zero);
            return (sbyte)ByteBuffer[0];
        }
        public static short RInt16(Int32 Addr)
        {
            byte[] ByteBuffer = new byte[2];
            ReadProcessMemory(FTLHandle, (IntPtr)Addr, ByteBuffer, 2, IntPtr.Zero);
            return BitConverter.ToInt16(ByteBuffer, 0);
        }
        public static int RInt32(Int32 Addr)
        {
            byte[] ByteBuffer = new byte[4];
            ReadProcessMemory(FTLHandle, (IntPtr)Addr, ByteBuffer, 4, IntPtr.Zero);
            return BitConverter.ToInt32(ByteBuffer, 0);
        }
        public static long RInt64(Int32 Addr)
        {
            byte[] ByteBuffer = new byte[8];
            ReadProcessMemory(FTLHandle, (IntPtr)Addr, ByteBuffer, 8, IntPtr.Zero);
            return BitConverter.ToInt64(ByteBuffer, 0);
        }
        public static ushort RUInt16(Int32 Addr)
        {
            byte[] ByteBuffer = new byte[2];
            ReadProcessMemory(FTLHandle, (IntPtr)Addr, ByteBuffer, 2, IntPtr.Zero);
            return BitConverter.ToUInt16(ByteBuffer, 0);
        }
        public static uint RUInt32(Int32 Addr)
        {
            byte[] ByteBuffer = new byte[4];
            ReadProcessMemory(FTLHandle, (IntPtr)Addr, ByteBuffer, 4, IntPtr.Zero);
            return BitConverter.ToUInt32(ByteBuffer, 0);
        }
        public static ulong RUInt64(Int32 Addr)
        {
            byte[] ByteBuffer = new byte[8];
            ReadProcessMemory(FTLHandle, (IntPtr)Addr, ByteBuffer, 8, IntPtr.Zero);
            return BitConverter.ToUInt64(ByteBuffer, 0);
        }
        public static float RFloat(Int32 Addr)
        {
            byte[] ByteBuffer = new byte[4];
            ReadProcessMemory(FTLHandle, (IntPtr)Addr, ByteBuffer, 4, IntPtr.Zero);
            return BitConverter.ToSingle(ByteBuffer, 0);
        }
        public static double RDouble(Int32 Addr)
        {
            byte[] ByteBuffer = new byte[8];
            ReadProcessMemory(FTLHandle, (IntPtr)Addr, ByteBuffer, 8, IntPtr.Zero);
            return BitConverter.ToDouble(ByteBuffer, 0);
        }
        public static IntPtr RIntPtr(Int32 Addr)
        {
            if (IntPtr.Size == 4)
            {
                byte[] ByteBuffer = new byte[4];
                ReadProcessMemory(FTLHandle, (IntPtr)Addr, ByteBuffer, IntPtr.Size, IntPtr.Zero);
                return new IntPtr(BitConverter.ToInt32(ByteBuffer, 0));
            }
            else
            {
                byte[] ByteBuffer = new byte[8];
                ReadProcessMemory(FTLHandle, (IntPtr)Addr, ByteBuffer, IntPtr.Size, IntPtr.Zero);
                return new IntPtr(BitConverter.ToInt64(ByteBuffer, 0));
            }
        }
        public static byte[] RBytes(Int32 Addr, int size)
        {
            byte[] _rtnBytes = new byte[size];
            ReadProcessMemory(FTLHandle, (IntPtr)Addr, _rtnBytes, size, IntPtr.Zero);
            return _rtnBytes;
        }
        public static byte RByte(Int32 Addr)
        {
            byte[] ByteBuffer = new byte[1];
            ReadProcessMemory(FTLHandle, (IntPtr)Addr, ByteBuffer, 1, IntPtr.Zero);
            return ByteBuffer[0];
        }
        public static string RAsciiStr(Int32 Addr, int maxLength = 0x100)
        {
            System.Text.StringBuilder Str = new System.Text.StringBuilder(maxLength);
            int loc = 0;

            var nextChr = '?';


            if (maxLength != 0)
            {
                byte[] bytes = new byte[2];

                while ((maxLength < 0 || loc < maxLength))
                {
                    ReadProcessMemory(FTLHandle, IntPtr.Add((IntPtr)Addr, loc), bytes, 1, IntPtr.Zero);
                    nextChr = System.Text.Encoding.ASCII.GetChars(bytes)[0];

                    if (nextChr == (char)0)
                    {
                        break; // TODO: might not be correct. Was : Exit While
                    }
                    else
                    {
                        Str.Append(nextChr);
                    }

                    loc += 1;
                }

            }

            return Str.ToString();
        }
        public static string RUnicodeStr(Int32 Addr, int maxLength = 0x100)
        {
            System.Text.StringBuilder Str = new System.Text.StringBuilder(maxLength);
            int loc = 0;

            var nextChr = '?';


            if (maxLength != 0)
            {
                byte[] bytes = new byte[3];

                while ((maxLength < 0 || loc < maxLength))
                {
                    ReadProcessMemory(FTLHandle, IntPtr.Add((IntPtr)Addr, loc * 2), bytes, 2, IntPtr.Zero);
                    nextChr = System.Text.Encoding.Unicode.GetChars(bytes)[0];

                    if (nextChr == (char)0)
                    {
                        break; // TODO: might not be correct. Was : Exit While
                    }
                    else
                    {
                        Str.Append(nextChr);
                    }

                    loc += 1;
                }

            }

            return Str.ToString();
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (TryAttachToClient())
            {
                if (lblAttached.Content.ToString() != "Attached")
                {
                    startMonTimer(); 
                }

                lblAttached.Content = "Attached";
                
            }
            else
            {
                lblAttached.Content = "Not Found";
            }
        }

        private void startMonTimer()
        {
            monTime.Interval = 1000;
            monTime.Elapsed += new System.Timers.ElapsedEventHandler(monTimeElapsed);
            monTime.Enabled = true;
        }

        private void monTimeElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (RInt32(ba) == 0x00905A4D)
                    {
                        string shipName = RAsciiStr(RInt32(ba + 0x5137ec));

                        int statLoc = RInt32(ba + 0x513490);

                        int fuel = RInt32(statLoc + 0xec);
                        int drones = RInt32(statLoc + 0xf0);
                        int scrap = RInt32(statLoc + 0xf4);
                        int missiles = RInt32(statLoc + 0xf8);
                        int hull = RInt32(statLoc + 0xfc);

                        int shipKills = RInt32(ba + 0x513988);
                        int beacons = RInt32(ba + 0x5139b0);
                        int totalScrap = RInt32(ba + 0x5139d8);
                        int sector  = RInt32(ba + 0x513c00);
                        int difficulty = RInt32(ba + 0x513c0c);

                        int crewCount = RInt32(ba + 0x514e40);

                        int reactor = RInt32(RInt32(ba + 0x51ab20) + 4);

                        float score = (totalScrap + (beacons * 10) + (shipKills * 20)) * (1 + (0.25f * difficulty));

                        txtShipName.Text = shipName;

                        txtShipKills.Text = shipKills.ToString();
                        txtTotalScrap.Text = totalScrap.ToString();

                        txtSector.Text = sector.ToString();
                        txtBeacons.Text = beacons.ToString();
                        txtDifficulty.Text = difficulty.ToString();

                        txtScore.Text = score.ToString();

                        txtCrewCount.Text = crewCount.ToString();
                        txtFuel.Text = fuel.ToString();
                        txtDrones.Text = drones.ToString();
                        txtMissiles.Text = missiles.ToString();
                        txtHull.Text = hull.ToString();
                        txtScrap.Text = scrap.ToString();

                        txtReactor.Text = reactor.ToString();

                        int sysptrs = RInt32(ba + 0x51348c);
                        sysptrs = RInt32(sysptrs + 0x18);
                        initSystems();

                        for (int i = 0; i < 12; i++)
                        {
                            int sysptr = RInt32(sysptrs + i * 4);
                            string sysname = RAsciiStr(sysptr + 0x28);
                            int sysval = RInt32(sysptr + 0x54);

                            
                            if (isAlphaNumeric(sysname))
                            {
                                systems[sysname] = sysval;
                            }
                        }

                        foreach (var sys in systems)
                        {
                            switch (sys.Key)
                            {
                                case "pilot":
                                    txtPilot.Text = sys.Value.ToString();
                                    break;
                                case "doors":
                                    txtDoors.Text = sys.Value.ToString();
                                    break;
                                case "sensors":
                                    txtSensors.Text = sys.Value.ToString();
                                    break;
                                case "medbay":
                                    txtMedBay.Text = sys.Value.ToString();
                                    break;
                                case "oxygen":
                                    txtOxygen.Text = sys.Value.ToString();
                                    break;
                                case "shields":
                                    txtShields.Text = sys.Value.ToString();
                                    break;
                                case "engines":
                                    txtEngines.Text = sys.Value.ToString();
                                    break;
                                case "weapons":
                                    txtWeapons.Text = sys.Value.ToString();
                                    break;
                                case "drones":
                                    txtDroneBay.Text = sys.Value.ToString();
                                    break;
                                case "teleporter":
                                    txtTeleporter.Text = sys.Value.ToString();
                                    break;
                                case "clonebay":
                                    txtCloneBay.Text = sys.Value.ToString();
                                    break;
                                case "battery":
                                    txtBattery.Text = sys.Value.ToString();
                                    break;
                                case "hacking":
                                    txtHacking.Text = sys.Value.ToString();
                                    break;
                                case "artillery":
                                    txtArtillery.Text = sys.Value.ToString();
                                    break;
                            }
                        }

                    }
                    else
                    {
                        monTime.Enabled = false;
                        lblAttached.Content = "Detached";
                    }
                });
            }
            catch
            { };
        }
    }
}
