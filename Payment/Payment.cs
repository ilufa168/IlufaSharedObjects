using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IlufaSharedObjects.Payment
{
    public enum PaymentType
    {
        Cash,
        CreditCard,
        DebitCard,
        Voucher,
        StoreCredit,
        Other
    }

    public class PaymentItem
    {
        public string nobukti = "";
        public PaymentType pay_type = PaymentType.Cash;
        public string account_number = "";
        public decimal amount = 0;
        public decimal change = 0;

        public PaymentType determinePayType(string type)
        {
            PaymentType result = new PaymentType();

            switch (type)
            {
                case "TT":
                    result = PaymentType.Cash;
                    break;
                case "TC":
                    result = PaymentType.CreditCard;
                    break;
                case "TO":
                    result = PaymentType.Other;
                    break;
                case "OT":
                    result = PaymentType.StoreCredit;
                    break;
                case "TV":
                    result = PaymentType.Voucher;
                    break;
                default:
                    result = PaymentType.Cash;
                    break;
            }

            return result;
        }
    }



}
