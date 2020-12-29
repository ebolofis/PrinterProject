using NLog;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Threading;

namespace ExtECRMainLogic.Models.ExtECRModels
{
    /// <summary>
    /// Serial Port class for OPOS3 use.
    /// </summary>
    public class Serial : IDisposable
    {
        #region Properties
        /// <summary>
        /// 
        /// </summary>
        private const byte STX = 0x02;
        /// <summary>
        /// 
        /// </summary>
        private const byte ETX = 0x03;
        /// <summary>
        /// 
        /// </summary>
        private const byte ENQ = 0x05;
        /// <summary>
        /// 
        /// </summary>
        private const byte ACK = 0x06;
        /// <summary>
        /// 
        /// </summary>
        private const byte NAK = 0x21;
        /// <summary>
        /// 
        /// </summary>
        private const byte CAN = 0x24;
        /// <summary>
        /// 
        /// </summary>
        public SerialPort m_Port;
        /// <summary>
        /// 
        /// </summary>
        private int buff;
        /// <summary>
        /// 
        /// </summary>
        private int MessFL;
        /// <summary>
        /// 
        /// </summary>
        private byte[] packet;
        /// <summary>
        /// Error code value
        /// </summary>
        private byte errorcode;
        /// <summary>
        /// Device status value.
        /// </summary>
        private byte devstat;
        /// <summary>
        /// Fiscal status value.
        /// </summary>
        private byte appstat;
        #endregion
        /// <summary>
        /// Logger
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public Serial()
        {
            this.packet = new byte[300];
            var logPath = GetConfigurationPath();
            logger = NLogBuilder.ConfigureNLog(logPath).GetCurrentClassLogger();
        }

        #region Public Methods

        /// <summary>
        /// Open the selected COM port (if it is not open) with the given parameters. 
        /// </summary>
        /// <param name="PortName"></param>
        /// <param name="BaudRate"></param>
        /// <param name="state"></param>
        /// <param name="ReadBufferSize"></param>
        /// <param name="WriteBufferSize"></param>
        /// <returns></returns>
        public bool OpenPort(string PortName, int BaudRate, Handshake state, int ReadBufferSize = 2048, int WriteBufferSize = 512)
        {
            if (logger.IsDebugEnabled)
                logger.Debug("Opening port...");
            try
            {
                string[] AvailablePorts;
                bool res = false;
                m_Port = new SerialPort();
                AvailablePorts = EnumSerialPorts();
                foreach (string pp in AvailablePorts)
                {
                    if (PortName == pp)
                    {
                        res = true;
                        break;
                    }
                }
                if (!res)
                {
                    if (logger.IsDebugEnabled)
                        logger.Debug("Open port return false.");
                    return false;
                }
                if (!m_Port.IsOpen)
                {
                    m_Port.PortName = PortName;
                    m_Port.BaudRate = BaudRate;
                    m_Port.Parity = Parity.None;
                    m_Port.DataBits = 8;
                    m_Port.StopBits = StopBits.One;
                    m_Port.ReadBufferSize = ReadBufferSize;// 16384; 8192; 2048;
                    m_Port.WriteBufferSize = WriteBufferSize;// 16384; 8192; 512;
                    //m_Port.ReadTimeout = 500;
                    //m_Port.WriteTimeout = 500;
                    //m_Port.DtrEnable = true;
                    //m_Port.ReceivedBytesThreshold = 1;
                    //m_Port.Handshake = state;
                    m_Port.Open();
                    m_Port.DiscardInBuffer();
                    m_Port.DiscardOutBuffer();
                }
                if (logger.IsDebugEnabled)
                    logger.Debug("Port " + PortName + " is opened.");
                return true;
            }
            catch (Exception exception)
            {
                logger.Error("Error opening port: " + exception.ToString());
                return false;
            }
        }

        /// <summary>
        /// Closes the open serial port.
        /// </summary>
        public void ClosePort()
        {
            if (m_Port.IsOpen)
            {
                if (logger.IsDebugEnabled)
                    logger.Debug("Closing port...");
                m_Port.DiscardInBuffer();
                m_Port.DiscardOutBuffer();
                m_Port.Close();
            }
            if (logger.IsDebugEnabled)
                logger.Debug("Port is closed.");
        }

        /// <summary>
        /// Send to fiscal device a command (string) and wait for the devices response.
        /// Returns: on communication error 256 (0x100), on OPOS3 error the related error code, on no error 104 (0x68 = ok[???]).
        /// Return value is passed by reference with the 'reply' parameter.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="reply"></param>
        /// <param name="deviceStatus"></param>
        /// <param name="fiscalStatus"></param>
        /// <returns></returns>
        public short Command_Request(string s, ref string reply, out byte? deviceStatus, out byte? fiscalStatus)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug("");
                logger.Debug("==== Command_Request Start ====");
            }
            int i, pos, cntvg = 0;
            byte cc, rc, b;
            deviceStatus = null;
            fiscalStatus = null;
            if (!m_Port.IsOpen)
            {
                logger.Warn("==== Port is not open, return 999. Command: " + s);
                return 999;
            }
        startvg:
            cntvg++;
            //============================ Enquire
            int tries = 2;
            for (i = 1; i <= tries; i++)
            {
                if (logger.IsDebugEnabled)
                    logger.Debug("--- Enquiring port, try #" + i.ToString() + " out of " + tries.ToString());
                SputC(CAN);
                m_Port.DiscardInBuffer();
                m_Port.DiscardOutBuffer();
                SputC(ENQ);
                if (!waitfor(ACK))
                {
                    continue;
                }
                if (logger.IsDebugEnabled)
                    logger.Debug("Breaking For-loop at i=" + i.ToString());
                break;
            }
            if (i >= tries)
            {
                logger.Warn("===  Enquires limit reached, return 0x100 (256). Command: " + s);
                return 0x100;
            }
            //============================ Send request packet
            if (!sendrequest(s))
            {
                logger.Warn("=== return 0x100 (256). Command: " + s);
                return 0x100;
            }
            //============================ Wait reply packet
            int tr = 3;
            for (i = 0; i < tr; i++)
            {
                if (logger.IsDebugEnabled)
                    logger.Debug("--- Waiting for the reply packet, Try #" + (i + 1).ToString() + " out of " + tr.ToString());
                while (true)
                {
                    int delay = 2000;
                    if (!waitc(delay))
                    {
                        logger.Warn("===   return 0x100 (256). Command: " + s);
                        return 0x100;
                    }
                    if ((b = sbuff()) == STX)
                        break;
                }
                pos = 0;
                packet[0] = 0;
            //========================== Read the packet data
            loop1:
                if (logger.IsDebugEnabled)
                    logger.Debug("Reading the reply packet from port");
                if (!waitc(1500))
                {
                    SputC(NAK);
                    continue;
                }
                packet[pos++] = sbuff();
                if (packet[pos - 1] == ETX)
                    packet[--pos] = 0;
                else
                    goto loop1;
                //========================== Verify packet (checksum)
                if (logger.IsDebugEnabled)
                    logger.Debug(" -- Verifing packet (checksum)...");
                if (pos < 8)
                {
                    SputC(NAK);
                    if (logger.IsDebugEnabled)
                        logger.Debug("     Invalid reply");
                    continue;
                }
                rc = (byte)(((packet[pos - 2] - '0') * 10) + (packet[pos - 1] - '0'));
                cc = 0;
                for (int j = 0; j < pos - 2; j++)
                {
                    cc += packet[j];
                }
                cc %= 100;
                if (cc != rc)
                {
                    SputC(NAK);
                    if (logger.IsDebugEnabled)
                        logger.Debug("     Checksum error");
                    continue;
                }
                //========================== Packet ok: Get errorCode, status etc..
                packet[pos - 3] = 0;
                pos -= 3;
                packet[2] = packet[5] = packet[8] = 32;
                string ret = "";
                for (int y = 0; y < pos; y++)
                {
                    ret += (char)packet[y];
                }
                errorcode = Convert.ToByte(ret.Substring(0, 2), 16);
                deviceStatus = devstat = Convert.ToByte(ret.Substring(3, 2), 16);
                fiscalStatus = appstat = Convert.ToByte(ret.Substring(6, 2), 16);
                reply = (pos > 8) ? ret.Substring(9) : "";
                //========================== Ack it
                SputC(ACK);
                if (errorcode == 0x1a)
                {
                    int sleep = 500;
                    if (logger.IsDebugEnabled)
                        logger.Debug("     errorcode = 0x1a (1), sleep:" + sleep.ToString());
                    Thread.Sleep(sleep);
                    goto startvg;
                }
                if (errorcode.Equals(0))
                {
                    if (logger.IsDebugEnabled)
                        logger.Debug("====  return errorcode: " + errorcode.ToString());
                }
                else
                    logger.Warn("====  return errorcode: " + errorcode.ToString() + ". Command: " + s);
                return errorcode;
            }
            logger.Warn("====  return errorcode 104. Command: " + s);
            return 104;
        }

        /// <summary>
        /// Clear both input and output buffers.
        /// </summary>
        public void ResetBuffers()
        {
            if (m_Port.IsOpen)
            {
                m_Port.DiscardInBuffer();
                m_Port.DiscardOutBuffer();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Build an array with the serial port names.
        /// </summary>
        /// <returns></returns>
        private String[] EnumSerialPorts()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// Send one byte using an open serial port.
        /// </summary>
        /// <param name="s"></param>
        private void SputC(byte s)
        {
            if (logger.IsDebugEnabled)
                logger.Debug("          Send '" + s.ToString("X2") + "' to serial port.");
            if (!m_Port.IsOpen)
                return;
            byte[] wout = new byte[1];
            wout[0] = s;
            while (m_Port.CtsHolding) ;
            m_Port.Write(wout, 0, 1);
        }

        /// <summary>
        /// Send one string using an open serial port.
        /// </summary>
        /// <param name="s"></param>
        private void SputC(string s)
        {
            if (logger.IsDebugEnabled)
                logger.Debug("          Send string : " + s);
            if (!m_Port.IsOpen)
                return;
            byte[] wout = new byte[1];
            for (int i = 0; i < s.Length; i++)
            {
                wout[0] = (byte)s[i];
                while (m_Port.CtsHolding) ;
                m_Port.Write(wout, 0, 1);
            }
        }

        /// <summary>
        /// Read one byte from the serial port (wait for 200 milliseconds until incoming data are available).
        /// Return the read-in data (byte), else returns 0.
        /// </summary>
        /// <returns></returns>
        private byte sbuff()
        {
            if (!m_Port.IsOpen)
                return 0;
            if (waitc(250))
            {
                byte b = (byte)m_Port.ReadByte();
                if (logger.IsDebugEnabled)
                    logger.Debug("          Byte '" + b.ToString("X2") + "' is read from serial port");
                return b;
            }
            return 0;
        }

        /// <summary>
        /// Wait for the specified byte and if success return true, else false.
        /// (for an open serial port, wait 5.5 seconds for incoming data and check if data matches the specified parameter)
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool waitfor(byte b)
        {
            if (logger.IsDebugEnabled)
                logger.Debug("        Waiting for the byte :'" + b.ToString("X2") + "'.");
            if (!m_Port.IsOpen)
                return false;
            if (!waitc(5500))
                return false;
            if (logger.IsDebugEnabled)
                logger.Debug("          Reading 1 byte from input buffer");
            byte c = (byte)m_Port.ReadByte();
            if (b == c)
            {
                if (logger.IsDebugEnabled)
                    logger.Debug("        Byte '" + b.ToString("X2") + "' returned. ");
                return true;
            }
            else
            {
                if (logger.IsDebugEnabled)
                    logger.Debug("        Byte '" + b.ToString("X2") + "' DIDN'T return. Port returned '" + c.ToString("X2") + "' insteed!!!");
                return true;
            }
        }

        /// <summary>
        /// Wait for serial port incoming data, for a specific time.
        /// Return true if there are incoming data, else false.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private bool waitc(int timeout)
        {
            if (logger.IsDebugEnabled)
                logger.Debug("          Waiting for serial port to send data for " + timeout.ToString() + " msec");
            int ticks_start;
            int ticks_current = Environment.TickCount;
            if (m_Port.IsOpen)
            {
                ticks_start = Environment.TickCount;
                while (ticks_current < (ticks_start + timeout))
                {
                    buff = m_Port.BytesToRead;
                    if (buff > 0)
                    {
                        if (logger.IsDebugEnabled)
                            logger.Debug("          Port is ready to send " + buff.ToString() + " bytes.");
                        return true;
                    }
                    ticks_current = Environment.TickCount;
                }
            }
            if (logger.IsDebugEnabled)
                logger.Debug("          Serial port did't send data. return false");
            return false;
        }

        /// <summary>
        /// Send one string over the open serial port following the predefined communication protocol.
        /// Upon receipt of ACK returns true, all other cases false.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool sendrequest(string s)
        {
            if (logger.IsDebugEnabled)
                logger.Debug("--- Sending Request: " + s);
            if (!m_Port.IsOpen)
                return false;
            int check = 0;
            for (int i = 0; i < s.Length; i++)
            {
                check += s[i];
                check %= 256;
            }
            check %= 100;
            string chk = check.ToString("00");
            for (int j = 1; j < 3; j++)
            {
                SputC(STX);
                SendData(s);
                SputC(chk.Substring(0, 1));
                SputC(chk.Substring(1, 1));
                SputC(ETX);
                if (waitc(3000))
                {
                    char c = (char)m_Port.ReadByte();
                    if (c == ACK)
                    {
                        if (logger.IsDebugEnabled)
                            logger.Debug("    Port returned the right byte. ");
                        return true;
                    }
                    if (logger.IsDebugEnabled)
                        logger.Debug("    Byte '" + ((int)c).ToString("X2") + "'read from port is not the expected ACK byte '" + ((int)ACK).ToString("X2") + "'");
                }
            }
            return false;
        }

        /// <summary>
        /// Send one string using an open serial port.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool SendData(string s)
        {
            if (logger.IsDebugEnabled)
                logger.Debug("       >> Sending Data : " + (s ?? "<NULL>"));
            if (!m_Port.IsOpen)
                return false;
            if (s.Length <= 0)
                return false;
            bool OutFL = true;
            if (m_Port.Handshake == Handshake.RequestToSend)
            {
                if (logger.IsDebugEnabled)
                    logger.Debug("          Port Handshake");
                while (m_Port.CtsHolding)
                {
                    MessFL++;
                    if (MessFL >= 5000)
                        MessFL = 2005;
                    if (MessFL == 2000)
                        OutFL = false;
                }
                if (MessFL > 0)
                    MessFL = 0;
            }
            if (OutFL)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    SputC((byte)s[i]);
                }
            }
            return OutFL;
        }

        #endregion

        /// <summary>
        /// Dispose the selected COM port.
        /// </summary>
        public void Dispose()
        {
            if (m_Port != null)
            {
                m_Port.Dispose();
                m_Port = null;
            }
        }

        /// <summary>
        /// Get log configuration from application path
        /// </summary>
        /// <returns></returns>
        private string GetConfigurationPath()
        {
            string path;
            var currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var pathComponents = new List<string>() { currentPath, "..", "..", "..", "Config" };
            var logPath = Path.GetFullPath(Path.Combine(pathComponents.ToArray()));
            if (Directory.Exists(logPath))
                path = Path.Combine(logPath, "NLog.config");
            else
                path = Path.Combine(currentPath, "Config", "NLog.config");
            return path;
        }
    }
}