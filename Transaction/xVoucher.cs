using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/// <summary>
/// Voucher Object - Defines behavior for vouchers in IlufaPos and other Ilufa common components
/// Example:
///   Voucher myVoucher = new Voucher();
///   
///   Voucher.checkForVouchers(SalesItem)  Returns true if there is a voucher in the sales items, false otherwise.
///   
/// Then you can do any of the following three output options:
/// </summary>
namespace IlufaSharedObjects.Transaction
{
    class zzzzzVoucher
    {
        static string[] voucher_ids = new string[2] { "0123456789", "0123456780" };

        public static bool checkForVouchers(List<SalesOrder> items)
        {
            bool result = false;

            foreach(SalesOrder sold_item in items)
            {
                //see if the item id is any of the voucher ids (in the voucher_ids array);
                if (voucher_ids.Any(item => item == sold_item.item_id))
                    result = true;

                if (result)
                    break;
            }

            return result;
        }
    }
}
