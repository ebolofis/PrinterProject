namespace ExtECRMainLogic.Models.ExtECRModels
{
    public class HDMErrorCodes
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public HDMErrorCodes()
        {

        }

        /// <summary>
        /// Get error description for specific code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetError(int code)
        {
            string error = "";
            switch (code)
            {
                case 666:
                    error = "Missing the DLL's license";
                    break;
                case 500:
                    error = "CCM internal error";
                    break;
                case 400:
                    error = "Request error";
                    break;
                case 402:
                    error = "Invalid protocol version";
                    break;
                case 403:
                    error = "Unauthorized connection";
                    break;
                case 404:
                    error = "Invalid action code";
                    break;
                case 101:
                    error = "Password Encryption Error";
                    break;
                case 102:
                    error = "Sequential key encryption error";
                    break;
                case 103:
                    error = "Invalid header file";
                    break;
                case 104:
                    error = "incorrect number of request";
                    break;
                case 105:
                    error = "JSON Formatting Error";
                    break;
                case 141:
                    error = "Last coupon subscription is absent";
                    break;
                case 142:
                    error = "The last coupon belongs to another user";
                    break;
                case 143:
                    error = "Printer's general error";
                    break;
                case 144:
                    error = "Printer Initialization Error";
                    break;
                case 145:
                    error = "The paper is over in the printer";
                    break;
                case 111:
                    error = "Operator's password incorrect";
                    break;
                case 112:
                    error = "Such a carrier does not exist";
                    break;
                case 113:
                    error = "The carrier is not active";
                    break;
                case 121:
                    error = "Invalid user";
                    break;
                case 151:
                    error = "There is no such section";
                    break;
                case 152:
                    error = "Amount paid overall less than money";
                    break;
                case 153:
                    error = "The checksum is the threshold set";
                    break;
                case 154:
                    error = "The coupon must have a positive number";
                    break;
                case 155:
                    error = "It is necessary to synchronize the CCM";
                    break;
                case 157:
                    error = "Invalid return receipt";
                    break;
                case 158:
                    error = "Coupon already refunded";
                    break;
                case 159:
                    error = "The price and quantity of the product can not be positive";
                    break;
                case 160:
                    error = "Discount percentage must be a negative number and be less than 100";
                    break;
                case 161:
                    error = "Product code can not be empty";
                    break;
                case 162:
                    error = "Product name can not be empty";
                    break;
                case 163:
                    error = "Product measuring unit can not be empty";
                    break;
                case 164:
                    error = "Nonpayment Disbursement";
                    break;
                case 165:
                    error = "Product can not be 0";
                    break;
                case 166:
                    error = "Final calculation error";
                    break;
                case 167:
                    error = "The non-cash deposit is larger than the total amount of coupon";
                    break;
                case 168:
                    error = "Non-cash deposit covers the total amount (Cash is excessive)";
                    break;
                case 169:
                    error = "Fixed file filters wrong choice (over 1 filename sent)";
                    break;
                case 170:
                    error = "An incorrect date range has been sent during the fiscal reporting. Interval should not exceed 2 months";
                    break;
                case 171:
                    error = "Inadmissible value of product price";
                    break;
                case 172:
                    error = "Coupon is not a coupon for goods";
                    break;
                case 173:
                    error = "Invalid discount type";
                    break;
                case 174:
                    error = "Return receipt does not exist";
                    break;
                case 175:
                    error = "Invalid return receipt for registration";
                    break;
                case 176:
                    error = "The last coupon has no property";
                    break;
                case 177:
                    error = "Returns can not be made for the same type of coupons";
                    break;
                case 178:
                    error = "The requested amount can not be refunded";
                    break;
                case 179:
                    error = "Partial payment coupon must be returned completely";
                    break;
                case 180:
                    error = "Complete return on more money";
                    break;
                case 181:
                    error = "Error returned product quantity";
                    break;
                case 182:
                    error = "The return ticket is a return receipt";
                    break;
                case 183:
                    error = "Incorrect ADD Code";
                    break;
                case 184:
                    error = "An Inadmissible Request for Prepayment";
                    break;
                case 185:
                    error = "Unable to process the coupon returned. Synchronization of CCM software is required";
                    break;
                case 186:
                    error = "Invalid amount for pre-payment";
                    break;
                case 187:
                    error = "Invalid list for pre-payment";
                    break;
                case 188:
                    error = "Invalid amount of money";
                    break;
                case 189:
                    error = "Invalid rounding";
                    break;
                case 190:
                    error = "Payment is unavailable";
                    break;
                case 191:
                    error = "At the cash register, the amount must be greater than 0";
                    break;
                case 192:
                    error = "The FEZ code is mandatory";
                    break;
                case 193:
                    error = "The buyer's TIN is incorrect";
                    break;
            }
            return error;
        }
    }
}