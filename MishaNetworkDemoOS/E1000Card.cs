//WIP


//using Cosmos.Core;
//using Cosmos.HAL;
//using Cosmos.HAL.Network;
//using System;
//using System.Runtime.InteropServices;

//namespace CosmosNetwork
//{
//    internal class E1000Card : NetworkDevice
//    {
//        private PCIDevice PCIDevice;
//        private uint Bar0;
//        private MACAddress addr;
//        private byte[] mac_address = new byte[6];
//        private unsafe byte* RxdescBase;

//        const uint CTRL_SLU = (1 << 6);
//        private uint i825xx_REG_EERD { get { return Bar0 + 0x0014; } }
//        private uint i825xx_REG_CTRL { get { return Bar0 + 0x0000; } }
//        private uint i825xx_REG_MTA { get { return Bar0 + 0x5200; } }
//        private uint i825xx_REG_IMS { get { return Bar0 + 0x00D0; } }
//        private uint i825xx_REG_STATUS { get { return Bar0 + 0x0008; } }

//        private const int NUM_RX_DESCRIPTORS = 768;
//        private const int NUM_TX_DESCRIPTORS = 768;
//        private unsafe E1000RxDescript* rx_desc;//[NUM_RX_DESCRIPTORS];	// receive descriptor buffer

//        public override CardType CardType => CardType.Ethernet;

//        public override MACAddress MACAddress => addr;

//        public override string Name => "E1000 Driver";

//        public override bool Ready => true;

//        public E1000Card(PCIDevice item)
//        {
//            this.PCIDevice = item;
//            Bar0 = item.BaseAddressBar[0].BaseAddress;
//            item.EnableDevice();
//            INTs.SetIrqHandler(item.InterruptLine, HandleIRQ);
//            Console.WriteLine("Detected E1000 compatible card. IRQ Lane: " + item.InterruptLine);


//            // get the MAC address
//            ushort mac16 = ReadEPROM(0);
//            mac_address[0] = (byte)(mac16 & 0xFF);
//            mac_address[1] = (byte)((mac16 >> 8) & 0xFF);
//            mac16 = ReadEPROM(1);
//            mac_address[2] = (byte)(mac16 & 0xFF);
//            mac_address[3] = (byte)((mac16 >> 8) & 0xFF);
//            mac16 = ReadEPROM(2);
//            mac_address[4] = (byte)(mac16 & 0xFF);
//            mac_address[5] = (byte)((mac16 >> 8) & 0xFF);

//            addr = new MACAddress(mac_address);
//            Console.WriteLine("MAC Address: " + addr.ToString());
//        }

//        private void HandleIRQ(ref INTs.IRQContext aContext)
//        {

//        }
//        private ushort ReadEPROM(byte Address)
//        {
//            ushort DATA;
//            uint tmp;
//            mmio_write32(i825xx_REG_EERD, (1) | ((uint)(Address) << 8));

//            while (((tmp = mmio_read32(i825xx_REG_EERD)) & (1 << 4)) == 0)
//            {
//                ; ; ;
//            }

//            DATA = (ushort)((tmp >> 16) & 0xFFFF);
//            return DATA;
//        }

//        private uint mmio_read32(uint register)
//        {
//            unsafe
//            {
//                return *(uint*)(register);
//            }
//        }
//        private void mmio_write32(uint register, uint value)
//        {
//            unsafe
//            {
//                *(uint*)(register) = value;
//            }
//        }

//        public override bool QueueBytes(byte[] buffer, int offset, int length)
//        {
//            Console.WriteLine("QueueBytes()");
//            return true;
//        }

//        public override bool ReceiveBytes(byte[] buffer, int offset, int max)
//        {
//            Console.WriteLine("ReceiveBytes()");
//            return true;
//        }

//        public override byte[] ReceivePacket()
//        {
//            Console.WriteLine("ReceivePacket()");
//            return new byte[] { };
//        }

//        public override int BytesAvailable()
//        {
//            Console.WriteLine("BytesAvailable()");
//            return 325;
//        }

//        public override bool Enable()
//        {
//            // set the LINK UP
//            mmio_write32(i825xx_REG_CTRL, (mmio_read32(i825xx_REG_CTRL) | CTRL_SLU));

//            //Initialize the Multicase Table Array
//            for (int i = 0; i < 128; i++)
//            {
//                mmio_write32((uint)(i825xx_REG_MTA + (i * 4)), 0);
//            }

//            // enable all interrupts (and clear existing pending ones)
//            mmio_write32(i825xx_REG_IMS, 0x1F6DC);
//            mmio_read32(Bar0 + 0xC0);

//            InitRX();
//            return true;
//        }
//        private void InitRX()
//        {
//            unsafe
//            {

//                // unaligned base address
//                ulong tmpbase = (ulong)Cosmos.Core.Memory.Heap.Alloc((uint)(sizeof(E1000RxDescript) * NUM_RX_DESCRIPTORS) + 16);
//                // aligned base address
//                if ((tmpbase % 16 == 0))
//                {
//                    RxdescBase = (byte*)((tmpbase) + 16 - (tmpbase % 16));
//                }
//                else
//                {
//                    RxdescBase = (byte*)tmpbase;
//                }
//                var rxDesc = (E1000RxDescript*)RxdescBase;
//                for (int i = 0; i < NUM_RX_DESCRIPTORS; i++)
//                {
//                    rxDesc[i] = (E1000RxDescript*)(dev->rx_desc_base + (i * 16));
//                    rxDesc[i]->address = (ulong)malloc(8192 + 16); // packet buffer size (8K)
//                    rxDesc[i]->status = 0;
//                }

//                // setup the receive descriptor ring buffer (TODO: >32-bits may be broken in this code)
//                mmio_write32(i825xx_REG_RDBAH, (uint)((ulong)dev->rx_desc_base >> 32));
//                mmio_write32(i825xx_REG_RDBAL, (uint)((uint64_t)dev->rx_desc_base & 0xFFFFFFFF));
//                printf("i825xx[%u]: RDBAH/RDBAL = %p8x:%p8x\n", netdev->hwid, mmio_read32(i825xx_REG_RDBAH), mmio_read32(i825xx_REG_RDBAL));

//                // receive buffer length; NUM_RX_DESCRIPTORS 16-byte descriptors
//                mmio_write32(i825xx_REG_RDLEN, (uint32_t)(NUM_RX_DESCRIPTORS * 16));

//                // setup head and tail pointers
//                mmio_write32(i825xx_REG_RDH, 0);
//                mmio_write32(i825xx_REG_RDT, NUM_RX_DESCRIPTORS);
//                dev->rx_tail = 0;

//                // set the receieve control register (promisc ON, 8K pkt size)
//                mmio_write32(i825xx_REG_RCTL, (RCTL_SBP | RCTL_UPE | RCTL_MPE | RDMTS_HALF | RCTL_SECRC |
//                                                RCTL_LPE | RCTL_BAM | RCTL_BSIZE_8192));
//            }
//        }

//        public override bool IsSendBufferFull()
//        {
//            Console.WriteLine("IsSendBufferFull()");
//            return false;//TODO
//        }

//        public override bool IsReceiveBufferFull()
//        {
//            Console.WriteLine("IsReceiveBufferFull()");
//            return false;//TODO
//        }
//        [StructLayout(LayoutKind.Sequential, Pack = 1)]
//        public struct E1000RxDescript
//        {

//        }
//    }
//}