using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IlufaSharedObjects.Voucher
{

    public enum VoucherStatus
    {
        PreValidated,
        Validated,
        Enabled,
        Used,
        Disabled,
        ErrorAlreadyAssigned
    }

    public class Voucher
    {
        static string[] voucher_ids = new string[2] { "123456789", "123456780" };
        //Voucher attributes
        public string voucher_number;
        public VoucherStatus status;

        public Voucher(string vnum)
        {
            this.voucher_number = vnum;
        }

        //public functions
        public bool validate_voucher()
        {
            return true;
        }

        //Make sure a given voucher number is not already assigned in the database
        //
        public bool validateAsNew()
        {
            bool ret = true;

            return ret;
        }
        //Static Functons
        public static string bonus_voucher_id = "123123123";

        public static string get_bonus_voucher_id()
        {
            return bonus_voucher_id;
        }

        public static bool checkForVouchers(List<SalesOrder> items)
        {
            bool result = false;

            foreach (SalesOrder sold_item in items)
            {
                //see if the item id is any of the voucher ids (in the voucher_ids array);
                if (voucher_ids.Any(item => item == sold_item.item_id))
                    result = true;

                if (result)
                    break;
            }

            return result;
        }

        public static double getVouchersTotal(List<SalesOrder> items)
        {
            double total = 0;
            foreach (SalesOrder sold_item in items)
            {
                //see if the item id is any of the voucher ids (in the voucher_ids array);
                if (voucher_ids.Any(item => item == sold_item.item_id))
                    total += sold_item.total;
            }
            return total;
        }

        public static double getVoucherPlusBonusTotal(List<SalesOrder> items)
        {
            double total = 0;
            foreach (SalesOrder sold_item in items)
            {
                //see if the item id is any of the voucher ids (in the voucher_ids array);
                if (voucher_ids.Any(item => item == sold_item.item_id)) 
                    total += sold_item.total;
                if (bonus_voucher_id == sold_item.item_id)
                {
                    total += sold_item.unit_price; //This item is 100% discount, have to use the price to get the value
                }
            }
            return total;

        }

        public static SalesOrder createBonusSO(double disc, double pct)
        {
            SalesOrder so = new SalesOrder();

            so.item_id = Voucher.bonus_voucher_id;
            so.quantity = 1;
            so.unit_price = disc;
            so.discount = disc;
            so.discount_desc = pct.ToString() + "% Bonus Voucher";
            so.description = "Bonus Voucher";
            

            return so;
        }

        public override string ToString()
        {
            return this.voucher_number + " - " + this.status.ToString();
        }
    }
}
