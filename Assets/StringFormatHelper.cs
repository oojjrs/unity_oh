namespace oojjrs.oh
{
    // 매번 검색하기 귀찮아서 만들어둔 거야.
    public static class StringFormatHelper
    {
        /// <summary>
        /// 화폐
        /// </summary>
        public static string Currency(object value, int precision)
        {
            if (precision > 0)
                return string.Format("{0:" + $"C{precision}" + "}", value);
            else
                return $"{value:C}";
        }

        /// <summary>
        /// 숫자 자체만 표현한다.
        /// </summary>
        public static string Decimal(object value, int precision)
        {
            return string.Format("{0:" + $"D{precision}" + "}", value);
        }

        /// <summary>
        /// 부동 소수점
        /// </summary>
        public static string FixedPoint(object value, int precision = 0)
        {
            if (precision > 0)
                return string.Format("{0:" + $"F{precision}" + "}", value);
            else
                return $"{value:F0}";
        }

        /// <summary>
        /// 소수점이 있으면 표시하고 아니면 정수로 표현한다.
        /// </summary>
        public static string FixedPointOrInteger(object value)
        {
            return $"{value:0.#}";
        }

        /// <summary>
        /// 16진수
        /// </summary>
        public static string Hexademical(object value, int precision = 0, bool upperCase = true)
        {
            if (upperCase)
            {
                if (precision > 0)
                    return string.Format("{0:" + $"X{precision}" + "}", value);
                else
                    return $"{value:X}";
            }
            else
            {
                if (precision > 0)
                    return string.Format("{0:" + $"x{precision}" + "}", value);
                else
                    return $"{value:x}";
            }
        }

        /// <summary>
        /// 문화를 고려한 숫자 표현이 포함되어 있다.
        /// </summary>
        public static string LocaleNumber(object value, int precision)
        {
            if (precision > 0)
                return string.Format("{0:" + $"N{precision}" + "}", value);
            else
                return $"{value:N}";
        }

        /// <summary>
        /// 퍼센트
        /// </summary>
        public static string Percent(object value, int precision = 0)
        {
            if (precision > 0)
                return string.Format("{0:" + $"P{precision}" + "}", value);
            else
                return $"{value:P0}";
        }
    }
}