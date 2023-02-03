using System;
using UnityEngine;

namespace GentlyUI.Core {
    public static class GlobalTextFormatter
    {
        public static string RoundWithDecimalsToString(float number, int numberOfDecimals = 2) {
            decimal roundedNumber = Math.Round(Convert.ToDecimal(number), numberOfDecimals);
            return roundedNumber.ToString();
        }
    }
}
