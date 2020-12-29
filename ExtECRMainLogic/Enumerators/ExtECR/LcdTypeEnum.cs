namespace ExtECRMainLogic.Enumerators.ExtECR
{
    /// <summary>
    /// The LCD types supported by our application.
    /// </summary>
    public enum LcdTypeEnum
    {
        Casio = 1,
        NCR = 2,
        IBM = 3,
        NCR_ENG = 4,
        /// <summary>
        /// Customer Display LIUST-5x on Toshiba POS ST-A20 - Greek CodePage 869
        /// </summary>
        TOSHIBA_ST_A20 = 5,
        /// <summary>
        /// Customer Display VF60 - VFD on Fujitsu TeamPOS 3000 - French
        /// </summary>
        Fujitsu_TeamPOS_French = 6,
        /// <summary>
        /// Customer Display VF60 with external utility VF60Commander.exe
        /// </summary>
        Use_VF60Commander = 7,
        /// <summary>
        /// Customer Display BA63 on WINCOR NIXDORF - GREEKLISH character set
        /// </summary>
        WINCOR_NIXDORF_BA63 = 8,
        /// <summary>
        /// Customer Display PD220 - VFD DISPLAY (made in china) - GREEKLISH character set
        /// </summary>
        VFD_PD220 = 9
    }
}