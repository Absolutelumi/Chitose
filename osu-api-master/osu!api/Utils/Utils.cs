using System;
using System.Globalization;
using static Osu.Utils.ConvertUtils;

namespace Osu.Utils
{
    internal static class MathUtils
    {
        public static int IntLength(ulong i)
        {
            if (i < 10000000000)
            {
                if (i < 10)
                {
                    return 1;
                }
                if (i < 100)
                {
                    return 2;
                }
                if (i < 1000)
                {
                    return 3;
                }
                if (i < 10000)
                {
                    return 4;
                }
                if (i < 100000)
                {
                    return 5;
                }
                if (i < 1000000)
                {
                    return 6;
                }
                if (i < 10000000)
                {
                    return 7;
                }
                if (i < 100000000)
                {
                    return 8;
                }
                if (i < 1000000000)
                {
                    return 9;
                }

                return 10;
            }
            else
            {
                if (i < 100000000000)
                {
                    return 11;
                }
                if (i < 1000000000000)
                {
                    return 12;
                }
                if (i < 10000000000000)
                {
                    return 13;
                }
                if (i < 100000000000000)
                {
                    return 14;
                }
                if (i < 1000000000000000)
                {
                    return 15;
                }
                if (i < 10000000000000000)
                {
                    return 16;
                }
                if (i < 100000000000000000)
                {
                    return 17;
                }
                if (i < 1000000000000000000)
                {
                    return 18;
                }
                if (i < 10000000000000000000)
                {
                    return 19;
                }

                return 20;
            }
        }
    }

    internal static class JsonTokenUtils
    {
        internal static bool IsEndToken(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                case JsonToken.EndConstructor:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsStartToken(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.StartObject:
                case JsonToken.StartArray:
                case JsonToken.StartConstructor:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsPrimitiveToken(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Undefined:
                case JsonToken.Null:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return true;
                default:
                    return false;
            }
        }
    }

    internal static class DateTimeUtils
    {
        internal static readonly long InitialJavaScriptDateTicks = 621355968000000000;
        private const string IsoDateFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";

        private const int DaysPer100Years = 36524;
        private const int DaysPer400Years = 146097;
        private const int DaysPer4Years = 1461;
        private const int DaysPerYear = 365;
        private const long TicksPerDay = 864000000000L;
        private static readonly int[] DaysToMonth365;
        private static readonly int[] DaysToMonth366;

        static DateTimeUtils()
        {
            DaysToMonth365 = new[] { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 };
            DaysToMonth366 = new[] { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };
        }

        public static TimeSpan GetUtcOffset(this DateTime d)
        {
#if NET20
            return TimeZone.CurrentTimeZone.GetUtcOffset(d);
#else
            return TimeZoneInfo.Local.GetUtcOffset(d);
#endif
        }

        internal static DateTime EnsureDateTime(DateTime value, DateTimeZoneHandling timeZone)
        {
            switch (timeZone)
            {
                case DateTimeZoneHandling.Local:
                    value = SwitchToLocalTime(value);
                    break;
                case DateTimeZoneHandling.Utc:
                    value = SwitchToUtcTime(value);
                    break;
                case DateTimeZoneHandling.Unspecified:
                    value = new DateTime(value.Ticks, DateTimeKind.Unspecified);
                    break;
                case DateTimeZoneHandling.RoundtripKind:
                    break;
                default:
                    throw new ArgumentException("Invalid date time handling value.");
            }

            return value;
        }

        private static DateTime SwitchToLocalTime(DateTime value)
        {
            switch (value.Kind)
            {
                case DateTimeKind.Unspecified:
                    return new DateTime(value.Ticks, DateTimeKind.Local);

                case DateTimeKind.Utc:
                    return value.ToLocalTime();

                case DateTimeKind.Local:
                    return value;
            }
            return value;
        }

        private static DateTime SwitchToUtcTime(DateTime value)
        {
            switch (value.Kind)
            {
                case DateTimeKind.Unspecified:
                    return new DateTime(value.Ticks, DateTimeKind.Utc);

                case DateTimeKind.Utc:
                    return value;

                case DateTimeKind.Local:
                    return value.ToUniversalTime();
            }
            return value;
        }

        private static long ToUniversalTicks(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return dateTime.Ticks;
            }

            return ToUniversalTicks(dateTime, dateTime.GetUtcOffset());
        }

        private static long ToUniversalTicks(DateTime dateTime, TimeSpan offset)
        {
            // special case min and max value
            // they never have a timezone appended to avoid issues
            if (dateTime.Kind == DateTimeKind.Utc || dateTime == DateTime.MaxValue || dateTime == DateTime.MinValue)
            {
                return dateTime.Ticks;
            }

            long ticks = dateTime.Ticks - offset.Ticks;
            if (ticks > 3155378975999999999L)
            {
                return 3155378975999999999L;
            }

            if (ticks < 0L)
            {
                return 0L;
            }

            return ticks;
        }

        internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime, TimeSpan offset)
        {
            long universialTicks = ToUniversalTicks(dateTime, offset);

            return UniversialTicksToJavaScriptTicks(universialTicks);
        }

        internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime)
        {
            return ConvertDateTimeToJavaScriptTicks(dateTime, true);
        }

        internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime, bool convertToUtc)
        {
            long ticks = (convertToUtc) ? ToUniversalTicks(dateTime) : dateTime.Ticks;

            return UniversialTicksToJavaScriptTicks(ticks);
        }

        private static long UniversialTicksToJavaScriptTicks(long universialTicks)
        {
            long javaScriptTicks = (universialTicks - InitialJavaScriptDateTicks) / 10000;

            return javaScriptTicks;
        }

        internal static DateTime ConvertJavaScriptTicksToDateTime(long javaScriptTicks)
        {
            DateTime dateTime = new DateTime((javaScriptTicks * 10000) + InitialJavaScriptDateTicks, DateTimeKind.Utc);

            return dateTime;
        }

        #region Parse
        internal static bool TryParseDateTimeIso(StringReference text, DateTimeZoneHandling dateTimeZoneHandling, out DateTime dt)
        {
            DateTimeParser dateTimeParser = new DateTimeParser();
            if (!dateTimeParser.Parse(text.Chars, text.StartIndex, text.Length))
            {
                dt = default(DateTime);
                return false;
            }

            DateTime d = CreateDateTime(dateTimeParser);

            long ticks;

            switch (dateTimeParser.Zone)
            {
                case ParserTimeZone.Utc:
                    d = new DateTime(d.Ticks, DateTimeKind.Utc);
                    break;

                case ParserTimeZone.LocalWestOfUtc:
                    {
                        TimeSpan offset = new TimeSpan(dateTimeParser.ZoneHour, dateTimeParser.ZoneMinute, 0);
                        ticks = d.Ticks + offset.Ticks;
                        if (ticks <= DateTime.MaxValue.Ticks)
                        {
                            d = new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
                        }
                        else
                        {
                            ticks += d.GetUtcOffset().Ticks;
                            if (ticks > DateTime.MaxValue.Ticks)
                            {
                                ticks = DateTime.MaxValue.Ticks;
                            }

                            d = new DateTime(ticks, DateTimeKind.Local);
                        }
                        break;
                    }
                case ParserTimeZone.LocalEastOfUtc:
                    {
                        TimeSpan offset = new TimeSpan(dateTimeParser.ZoneHour, dateTimeParser.ZoneMinute, 0);
                        ticks = d.Ticks - offset.Ticks;
                        if (ticks >= DateTime.MinValue.Ticks)
                        {
                            d = new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
                        }
                        else
                        {
                            ticks += d.GetUtcOffset().Ticks;
                            if (ticks < DateTime.MinValue.Ticks)
                            {
                                ticks = DateTime.MinValue.Ticks;
                            }

                            d = new DateTime(ticks, DateTimeKind.Local);
                        }
                        break;
                    }
            }

            dt = EnsureDateTime(d, dateTimeZoneHandling);
            return true;
        }

#if !NET20
        internal static bool TryParseDateTimeOffsetIso(StringReference text, out DateTimeOffset dt)
        {
            DateTimeParser dateTimeParser = new DateTimeParser();
            if (!dateTimeParser.Parse(text.Chars, text.StartIndex, text.Length))
            {
                dt = default(DateTimeOffset);
                return false;
            }

            DateTime d = CreateDateTime(dateTimeParser);

            TimeSpan offset;

            switch (dateTimeParser.Zone)
            {
                case ParserTimeZone.Utc:
                    offset = new TimeSpan(0L);
                    break;
                case ParserTimeZone.LocalWestOfUtc:
                    offset = new TimeSpan(-dateTimeParser.ZoneHour, -dateTimeParser.ZoneMinute, 0);
                    break;
                case ParserTimeZone.LocalEastOfUtc:
                    offset = new TimeSpan(dateTimeParser.ZoneHour, dateTimeParser.ZoneMinute, 0);
                    break;
                default:
                    offset = TimeZoneInfo.Local.GetUtcOffset(d);
                    break;
            }

            long ticks = d.Ticks - offset.Ticks;
            if (ticks < 0 || ticks > 3155378975999999999)
            {
                dt = default(DateTimeOffset);
                return false;
            }

            dt = new DateTimeOffset(d, offset);
            return true;
        }
#endif

        private static DateTime CreateDateTime(DateTimeParser dateTimeParser)
        {
            bool is24Hour;
            if (dateTimeParser.Hour == 24)
            {
                is24Hour = true;
                dateTimeParser.Hour = 0;
            }
            else
            {
                is24Hour = false;
            }

            DateTime d = new DateTime(dateTimeParser.Year, dateTimeParser.Month, dateTimeParser.Day, dateTimeParser.Hour, dateTimeParser.Minute, dateTimeParser.Second);
            d = d.AddTicks(dateTimeParser.Fraction);

            if (is24Hour)
            {
                d = d.AddDays(1);
            }
            return d;
        }

        internal static bool TryParseDateTime(StringReference s, DateTimeZoneHandling dateTimeZoneHandling, string dateFormatString, CultureInfo culture, out DateTime dt)
        {
            if (s.Length > 0)
            {
                int i = s.StartIndex;
                if (s[i] == '/')
                {
                    if (s.Length >= 9 && s.StartsWith("/Date(") && s.EndsWith(")/"))
                    {
                        if (TryParseDateTimeMicrosoft(s, dateTimeZoneHandling, out dt))
                        {
                            return true;
                        }
                    }
                }
                else if (s.Length >= 19 && s.Length <= 40 && char.IsDigit(s[i]) && s[i + 10] == 'T')
                {
                    if (TryParseDateTimeIso(s, dateTimeZoneHandling, out dt))
                    {
                        return true;
                    }
                }

                if (!string.IsNullOrEmpty(dateFormatString))
                {
                    if (TryParseDateTimeExact(s.ToString(), dateTimeZoneHandling, dateFormatString, culture, out dt))
                    {
                        return true;
                    }
                }
            }

            dt = default(DateTime);
            return false;
        }

        internal static bool TryParseDateTime(string s, DateTimeZoneHandling dateTimeZoneHandling, string dateFormatString, CultureInfo culture, out DateTime dt)
        {
            if (s.Length > 0)
            {
                if (s[0] == '/')
                {
                    if (s.Length >= 9 && s.StartsWith("/Date(", StringComparison.Ordinal) && s.EndsWith(")/", StringComparison.Ordinal))
                    {
                        if (TryParseDateTimeMicrosoft(new StringReference(s.ToCharArray(), 0, s.Length), dateTimeZoneHandling, out dt))
                        {
                            return true;
                        }
                    }
                }
                else if (s.Length >= 19 && s.Length <= 40 && char.IsDigit(s[0]) && s[10] == 'T')
                {
                    if (DateTime.TryParseExact(s, IsoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt))
                    {
                        dt = EnsureDateTime(dt, dateTimeZoneHandling);
                        return true;
                    }
                }

                if (!string.IsNullOrEmpty(dateFormatString))
                {
                    if (TryParseDateTimeExact(s, dateTimeZoneHandling, dateFormatString, culture, out dt))
                    {
                        return true;
                    }
                }
            }

            dt = default(DateTime);
            return false;
        }

#if !NET20
        internal static bool TryParseDateTimeOffset(StringReference s, string dateFormatString, CultureInfo culture, out DateTimeOffset dt)
        {
            if (s.Length > 0)
            {
                int i = s.StartIndex;
                if (s[i] == '/')
                {
                    if (s.Length >= 9 && s.StartsWith("/Date(") && s.EndsWith(")/"))
                    {
                        if (TryParseDateTimeOffsetMicrosoft(s, out dt))
                        {
                            return true;
                        }
                    }
                }
                else if (s.Length >= 19 && s.Length <= 40 && char.IsDigit(s[i]) && s[i + 10] == 'T')
                {
                    if (TryParseDateTimeOffsetIso(s, out dt))
                    {
                        return true;
                    }
                }

                if (!string.IsNullOrEmpty(dateFormatString))
                {
                    if (TryParseDateTimeOffsetExact(s.ToString(), dateFormatString, culture, out dt))
                    {
                        return true;
                    }
                }
            }

            dt = default(DateTimeOffset);
            return false;
        }

        internal static bool TryParseDateTimeOffset(string s, string dateFormatString, CultureInfo culture, out DateTimeOffset dt)
        {
            if (s.Length > 0)
            {
                if (s[0] == '/')
                {
                    if (s.Length >= 9 && s.StartsWith("/Date(", StringComparison.Ordinal) && s.EndsWith(")/", StringComparison.Ordinal))
                    {
                        if (TryParseDateTimeOffsetMicrosoft(new StringReference(s.ToCharArray(), 0, s.Length), out dt))
                        {
                            return true;
                        }
                    }
                }
                else if (s.Length >= 19 && s.Length <= 40 && char.IsDigit(s[0]) && s[10] == 'T')
                {
                    if (DateTimeOffset.TryParseExact(s, IsoDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt))
                    {
                        if (TryParseDateTimeOffsetIso(new StringReference(s.ToCharArray(), 0, s.Length), out dt))
                        {
                            return true;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(dateFormatString))
                {
                    if (TryParseDateTimeOffsetExact(s, dateFormatString, culture, out dt))
                    {
                        return true;
                    }
                }
            }

            dt = default(DateTimeOffset);
            return false;
        }
#endif

        private static bool TryParseMicrosoftDate(StringReference text, out long ticks, out TimeSpan offset, out DateTimeKind kind)
        {
            kind = DateTimeKind.Utc;

            int index = text.IndexOf('+', 7, text.Length - 8);

            if (index == -1)
            {
                index = text.IndexOf('-', 7, text.Length - 8);
            }

            if (index != -1)
            {
                kind = DateTimeKind.Local;

                if (!TryReadOffset(text, index + text.StartIndex, out offset))
                {
                    ticks = 0;
                    return false;
                }
            }
            else
            {
                offset = TimeSpan.Zero;
                index = text.Length - 2;
            }

            return (ConvertUtils.Int64TryParse(text.Chars, 6 + text.StartIndex, index - 6, out ticks) == ParseResult.Success);
        }

        private static bool TryParseDateTimeMicrosoft(StringReference text, DateTimeZoneHandling dateTimeZoneHandling, out DateTime dt)
        {
            long ticks;
            TimeSpan offset;
            DateTimeKind kind;

            if (!TryParseMicrosoftDate(text, out ticks, out offset, out kind))
            {
                dt = default(DateTime);
                return false;
            }

            DateTime utcDateTime = ConvertJavaScriptTicksToDateTime(ticks);

            switch (kind)
            {
                case DateTimeKind.Unspecified:
                    dt = DateTime.SpecifyKind(utcDateTime.ToLocalTime(), DateTimeKind.Unspecified);
                    break;
                case DateTimeKind.Local:
                    dt = utcDateTime.ToLocalTime();
                    break;
                default:
                    dt = utcDateTime;
                    break;
            }

            dt = EnsureDateTime(dt, dateTimeZoneHandling);
            return true;
        }

        private static bool TryParseDateTimeExact(string text, DateTimeZoneHandling dateTimeZoneHandling, string dateFormatString, CultureInfo culture, out DateTime dt)
        {
            DateTime temp;
            if (DateTime.TryParseExact(text, dateFormatString, culture, DateTimeStyles.RoundtripKind, out temp))
            {
                temp = EnsureDateTime(temp, dateTimeZoneHandling);
                dt = temp;
                return true;
            }

            dt = default(DateTime);
            return false;
        }

#if !NET20
        private static bool TryParseDateTimeOffsetMicrosoft(StringReference text, out DateTimeOffset dt)
        {
            long ticks;
            TimeSpan offset;
            DateTimeKind kind;

            if (!TryParseMicrosoftDate(text, out ticks, out offset, out kind))
            {
                dt = default(DateTime);
                return false;
            }

            DateTime utcDateTime = ConvertJavaScriptTicksToDateTime(ticks);

            dt = new DateTimeOffset(utcDateTime.Add(offset).Ticks, offset);
            return true;
        }

        private static bool TryParseDateTimeOffsetExact(string text, string dateFormatString, CultureInfo culture, out DateTimeOffset dt)
        {
            DateTimeOffset temp;
            if (DateTimeOffset.TryParseExact(text, dateFormatString, culture, DateTimeStyles.RoundtripKind, out temp))
            {
                dt = temp;
                return true;
            }

            dt = default(DateTimeOffset);
            return false;
        }
#endif

        private static bool TryReadOffset(StringReference offsetText, int startIndex, out TimeSpan offset)
        {
            bool negative = (offsetText[startIndex] == '-');

            int hours;
            if (ConvertUtils.Int32TryParse(offsetText.Chars, startIndex + 1, 2, out hours) != ParseResult.Success)
            {
                offset = default(TimeSpan);
                return false;
            }

            int minutes = 0;
            if (offsetText.Length - startIndex > 5)
            {
                if (ConvertUtils.Int32TryParse(offsetText.Chars, startIndex + 3, 2, out minutes) != ParseResult.Success)
                {
                    offset = default(TimeSpan);
                    return false;
                }
            }

            offset = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);
            if (negative)
            {
                offset = offset.Negate();
            }

            return true;
        }
        #endregion

        #region Write
        //internal static void WriteDateTimeString(TextWriter writer, DateTime value, DateFormatHandling format, string formatString, CultureInfo culture)
        //{
        //    if (string.IsNullOrEmpty(formatString))
        //    {
        //        char[] chars = new char[64];
        //        int pos = WriteDateTimeString(chars, 0, value, null, value.Kind, format);
        //        writer.Write(chars, 0, pos);
        //    }
        //    else
        //    {
        //        writer.Write(value.ToString(formatString, culture));
        //    }
        //}

        internal static int WriteDateTimeString(char[] chars, int start, DateTime value, TimeSpan? offset, DateTimeKind kind, DateFormatHandling format)
        {
            int pos = start;

            if (format == DateFormatHandling.MicrosoftDateFormat)
            {
                TimeSpan o = offset ?? value.GetUtcOffset();

                long javaScriptTicks = ConvertDateTimeToJavaScriptTicks(value, o);

                @"\/Date(".CopyTo(0, chars, pos, 7);
                pos += 7;

                string ticksText = javaScriptTicks.ToString(CultureInfo.InvariantCulture);
                ticksText.CopyTo(0, chars, pos, ticksText.Length);
                pos += ticksText.Length;

                switch (kind)
                {
                    case DateTimeKind.Unspecified:
                        if (value != DateTime.MaxValue && value != DateTime.MinValue)
                        {
                            pos = WriteDateTimeOffset(chars, pos, o, format);
                        }
                        break;
                    case DateTimeKind.Local:
                        pos = WriteDateTimeOffset(chars, pos, o, format);
                        break;
                }

                @")\/".CopyTo(0, chars, pos, 3);
                pos += 3;
            }
            else
            {
                pos = WriteDefaultIsoDate(chars, pos, value);

                switch (kind)
                {
                    case DateTimeKind.Local:
                        pos = WriteDateTimeOffset(chars, pos, offset ?? value.GetUtcOffset(), format);
                        break;
                    case DateTimeKind.Utc:
                        chars[pos++] = 'Z';
                        break;
                }
            }

            return pos;
        }

        internal static int WriteDefaultIsoDate(char[] chars, int start, DateTime dt)
        {
            int length = 19;

            int year;
            int month;
            int day;
            GetDateValues(dt, out year, out month, out day);

            CopyIntToCharArray(chars, start, year, 4);
            chars[start + 4] = '-';
            CopyIntToCharArray(chars, start + 5, month, 2);
            chars[start + 7] = '-';
            CopyIntToCharArray(chars, start + 8, day, 2);
            chars[start + 10] = 'T';
            CopyIntToCharArray(chars, start + 11, dt.Hour, 2);
            chars[start + 13] = ':';
            CopyIntToCharArray(chars, start + 14, dt.Minute, 2);
            chars[start + 16] = ':';
            CopyIntToCharArray(chars, start + 17, dt.Second, 2);

            int fraction = (int)(dt.Ticks % 10000000L);

            if (fraction != 0)
            {
                int digits = 7;
                while ((fraction % 10) == 0)
                {
                    digits--;
                    fraction /= 10;
                }

                chars[start + 19] = '.';
                CopyIntToCharArray(chars, start + 20, fraction, digits);

                length += digits + 1;
            }

            return start + length;
        }

        private static void CopyIntToCharArray(char[] chars, int start, int value, int digits)
        {
            while (digits-- != 0)
            {
                chars[start + digits] = (char)((value % 10) + 48);
                value /= 10;
            }
        }

        internal static int WriteDateTimeOffset(char[] chars, int start, TimeSpan offset, DateFormatHandling format)
        {
            chars[start++] = (offset.Ticks >= 0L) ? '+' : '-';

            int absHours = Math.Abs(offset.Hours);
            CopyIntToCharArray(chars, start, absHours, 2);
            start += 2;

            if (format == DateFormatHandling.IsoDateFormat)
            {
                chars[start++] = ':';
            }

            int absMinutes = Math.Abs(offset.Minutes);
            CopyIntToCharArray(chars, start, absMinutes, 2);
            start += 2;

            return start;
        }

        //#if !NET20
        //        internal static void WriteDateTimeOffsetString(TextWriter writer, DateTimeOffset value, DateFormatHandling format, string formatString, CultureInfo culture)
        //        {
        //            if (string.IsNullOrEmpty(formatString))
        //            {
        //                char[] chars = new char[64];
        //                int pos = WriteDateTimeString(chars, 0, (format == DateFormatHandling.IsoDateFormat) ? value.DateTime : value.UtcDateTime, value.Offset, DateTimeKind.Local, format);

        //                writer.Write(chars, 0, pos);
        //            }
        //            else
        //            {
        //                writer.Write(value.ToString(formatString, culture));
        //            }
        //        }
        //#endif
        #endregion

        private static void GetDateValues(DateTime td, out int year, out int month, out int day)
        {
            long ticks = td.Ticks;
            // n = number of days since 1/1/0001
            int n = (int)(ticks / TicksPerDay);
            // y400 = number of whole 400-year periods since 1/1/0001
            int y400 = n / DaysPer400Years;
            // n = day number within 400-year period
            n -= y400 * DaysPer400Years;
            // y100 = number of whole 100-year periods within 400-year period
            int y100 = n / DaysPer100Years;
            // Last 100-year period has an extra day, so decrement result if 4
            if (y100 == 4)
            {
                y100 = 3;
            }
            // n = day number within 100-year period
            n -= y100 * DaysPer100Years;
            // y4 = number of whole 4-year periods within 100-year period
            int y4 = n / DaysPer4Years;
            // n = day number within 4-year period
            n -= y4 * DaysPer4Years;
            // y1 = number of whole years within 4-year period
            int y1 = n / DaysPerYear;
            // Last year has an extra day, so decrement result if 4
            if (y1 == 4)
            {
                y1 = 3;
            }

            year = y400 * 400 + y100 * 100 + y4 * 4 + y1 + 1;

            // n = day number within year
            n -= y1 * DaysPerYear;

            // Leap year calculation looks different from IsLeapYear since y1, y4,
            // and y100 are relative to year 1, not year 0
            bool leapYear = y1 == 3 && (y4 != 24 || y100 == 3);
            int[] days = leapYear ? DaysToMonth366 : DaysToMonth365;
            // All months have less than 32 days, so n >> 5 is a good conservative
            // estimate for the month
            int m = n >> 5 + 1;
            // m = 1-based month number
            while (n >= days[m])
            {
                m++;
            }

            month = m;

            // Return 1-based day-of-month
            day = n - days[m - 1] + 1;
        }
    }

    internal static class ConvertUtils
    {
        public static int HexTextToInt(char[] text, int start, int end)
        {
            int value = 0;
            for (int i = start; i < end; i++)
            {
                value += HexCharToInt(text[i]) << ((end - 1 - i) * 4);
            }
            return value;
        }

        private static int HexCharToInt(char ch)
        {
            if (ch <= 57 && ch >= 48)
            {
                return ch - 48;
            }

            if (ch <= 70 && ch >= 65)
            {
                return ch - 55;
            }

            if (ch <= 102 && ch >= 97)
            {
                return ch - 87;
            }

            throw new FormatException("Invalid hex character: " + ch);
        }

        public static bool TryConvertGuid(string s, out Guid g)
        {
            // GUID has to have format 00000000-0000-0000-0000-000000000000
#if NET20 || NET35
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            Regex format = new Regex("^[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}$");
            Match match = format.Match(s);
            if (match.Success)
            {
                g = new Guid(s);
                return true;
            }

            g = Guid.Empty;
            return false;
#else
            return Guid.TryParseExact(s, "D", out g);
#endif
        }

        public static ParseResult Int32TryParse(char[] chars, int start, int length, out int value)
        {
            value = 0;

            if (length == 0)
            {
                return ParseResult.Invalid;
            }

            bool isNegative = (chars[start] == '-');

            if (isNegative)
            {
                // text just a negative sign
                if (length == 1)
                {
                    return ParseResult.Invalid;
                }

                start++;
                length--;
            }

            int end = start + length;

            // Int32.MaxValue and MinValue are 10 chars
            // Or is 10 chars and start is greater than two
            // Need to improve this!
            if (length > 10 || (length == 10 && chars[start] - '0' > 2))
            {
                // invalid result takes precedence over overflow
                for (int i = start; i < end; i++)
                {
                    int c = chars[i] - '0';

                    if (c < 0 || c > 9)
                    {
                        return ParseResult.Invalid;
                    }
                }

                return ParseResult.Overflow;
            }

            for (int i = start; i < end; i++)
            {
                int c = chars[i] - '0';

                if (c < 0 || c > 9)
                {
                    return ParseResult.Invalid;
                }

                int newValue = (10 * value) - c;

                // overflow has caused the number to loop around
                if (newValue > value)
                {
                    i++;

                    // double check the rest of the string that there wasn't anything invalid
                    // invalid result takes precedence over overflow result
                    for (; i < end; i++)
                    {
                        c = chars[i] - '0';

                        if (c < 0 || c > 9)
                        {
                            return ParseResult.Invalid;
                        }
                    }

                    return ParseResult.Overflow;
                }

                value = newValue;
            }

            // go from negative to positive to avoids overflow
            // negative can be slightly bigger than positive
            if (!isNegative)
            {
                // negative integer can be one bigger than positive
                if (value == int.MinValue)
                {
                    return ParseResult.Overflow;
                }

                value = -value;
            }

            return ParseResult.Success;
        }

        public static ParseResult Int64TryParse(char[] chars, int start, int length, out long value)
        {
            value = 0;

            if (length == 0)
            {
                return ParseResult.Invalid;
            }

            bool isNegative = (chars[start] == '-');

            if (isNegative)
            {
                // text just a negative sign
                if (length == 1)
                {
                    return ParseResult.Invalid;
                }

                start++;
                length--;
            }

            int end = start + length;

            // Int64.MaxValue and MinValue are 19 chars
            if (length > 19)
            {
                // invalid result takes precedence over overflow
                for (int i = start; i < end; i++)
                {
                    int c = chars[i] - '0';

                    if (c < 0 || c > 9)
                    {
                        return ParseResult.Invalid;
                    }
                }

                return ParseResult.Overflow;
            }

            for (int i = start; i < end; i++)
            {
                int c = chars[i] - '0';

                if (c < 0 || c > 9)
                {
                    return ParseResult.Invalid;
                }

                long newValue = (10 * value) - c;

                // overflow has caused the number to loop around
                if (newValue > value)
                {
                    i++;

                    // double check the rest of the string that there wasn't anything invalid
                    // invalid result takes precedence over overflow result
                    for (; i < end; i++)
                    {
                        c = chars[i] - '0';

                        if (c < 0 || c > 9)
                        {
                            return ParseResult.Invalid;
                        }
                    }

                    return ParseResult.Overflow;
                }

                value = newValue;
            }

            // go from negative to positive to avoids overflow
            // negative can be slightly bigger than positive
            if (!isNegative)
            {
                // negative integer can be one bigger than positive
                if (value == long.MinValue)
                {
                    return ParseResult.Overflow;
                }

                value = -value;
            }

            return ParseResult.Success;
        }


        internal enum ParseResult
        {
            None = 0,
            Success = 1,
            Overflow = 2,
            Invalid = 3
        }
    }

    internal static class BufferUtils
    {
        public static char[] RentBuffer(IArrayPool<char> bufferPool, int minSize)
        {
            if (bufferPool == null)
            {
                return new char[minSize];
            }

            char[] buffer = bufferPool.Rent(minSize);
            return buffer;
        }

        public static void ReturnBuffer(IArrayPool<char> bufferPool, char[] buffer)
        {
            if (bufferPool == null)
            {
                return;
            }

            bufferPool.Return(buffer);
        }

        public static char[] EnsureBufferSize(IArrayPool<char> bufferPool, int size, char[] buffer)
        {
            if (bufferPool == null)
            {
                return new char[size];
            }

            if (buffer != null)
            {
                bufferPool.Return(buffer);
            }

            return bufferPool.Rent(size);
        }


    }

    internal static class StringUtils
    {
        public const string CarriageReturnLineFeed = "\r\n";
        public const string Empty = "";
        public const char CarriageReturn = '\r';
        public const char LineFeed = '\n';
        public const char Tab = '\t';

        public static bool IsLowSurrogate(char c)
        {
#if !(PORTABLE40 || PORTABLE)
            return char.IsLowSurrogate(c);
#else
            return (c >= 56320 && c <= 57343);
#endif
        }
        public static bool IsHighSurrogate(char c)
        {
#if !(PORTABLE40 || PORTABLE)
            return char.IsHighSurrogate(c);
#else
            return (c >= 55296 && c <= 56319);
#endif
        }

        public static string FormatWith(this string format, IFormatProvider provider, object arg0)
        {
            return format.FormatWith(provider, new[] { arg0 });
        }
    }

    public static class JsonConvert
    {
        /// <summary>
        /// Represents JavaScript's boolean value true as a string. This field is read-only.
        /// </summary>
        public static readonly string True = "true";

        /// <summary>
        /// Represents JavaScript's boolean value false as a string. This field is read-only.
        /// </summary>
        public static readonly string False = "false";

        /// <summary>
        /// Represents JavaScript's null as a string. This field is read-only.
        /// </summary>
        public static readonly string Null = "null";

        /// <summary>
        /// Represents JavaScript's undefined as a string. This field is read-only.
        /// </summary>
        public static readonly string Undefined = "undefined";

        /// <summary>
        /// Represents JavaScript's positive infinity as a string. This field is read-only.
        /// </summary>
        public static readonly string PositiveInfinity = "Infinity";

        /// <summary>
        /// Represents JavaScript's negative infinity as a string. This field is read-only.
        /// </summary>
        public static readonly string NegativeInfinity = "-Infinity";

        /// <summary>
        /// Represents JavaScript's NaN as a string. This field is read-only.
        /// </summary>
        public static readonly string NaN = "NaN";

    }

    internal class PropertyNameTable
    {
        // used to defeat hashtable DoS attack where someone passes in lots of strings that hash to the same hash code
        private static readonly int HashCodeRandomizer;

        private int _count;
        private Entry[] _entries;
        private int _mask = 31;

        static PropertyNameTable()
        {
            HashCodeRandomizer = Environment.TickCount;
        }

        public PropertyNameTable()
        {
            _entries = new Entry[_mask + 1];
        }

        public string Get(char[] key, int start, int length)
        {
            if (length == 0)
            {
                return string.Empty;
            }

            int hashCode = length + HashCodeRandomizer;
            hashCode += (hashCode << 7) ^ key[start];
            int end = start + length;
            for (int i = start + 1; i < end; i++)
            {
                hashCode += (hashCode << 7) ^ key[i];
            }
            hashCode -= hashCode >> 17;
            hashCode -= hashCode >> 11;
            hashCode -= hashCode >> 5;
            for (Entry entry = _entries[hashCode & _mask]; entry != null; entry = entry.Next)
            {
                if (entry.HashCode == hashCode && TextEquals(entry.Value, key, start, length))
                {
                    return entry.Value;
                }
            }

            return null;
        }

        public string Add(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            int length = key.Length;
            if (length == 0)
            {
                return string.Empty;
            }

            int hashCode = length + HashCodeRandomizer;
            for (int i = 0; i < key.Length; i++)
            {
                hashCode += (hashCode << 7) ^ key[i];
            }
            hashCode -= hashCode >> 17;
            hashCode -= hashCode >> 11;
            hashCode -= hashCode >> 5;
            for (Entry entry = _entries[hashCode & _mask]; entry != null; entry = entry.Next)
            {
                if (entry.HashCode == hashCode && entry.Value.Equals(key))
                {
                    return entry.Value;
                }
            }

            return AddEntry(key, hashCode);
        }

        private string AddEntry(string str, int hashCode)
        {
            int index = hashCode & _mask;
            Entry entry = new Entry(str, hashCode, _entries[index]);
            _entries[index] = entry;
            if (_count++ == _mask)
            {
                Grow();
            }
            return entry.Value;
        }

        private void Grow()
        {
            Entry[] entries = _entries;
            int newMask = (_mask * 2) + 1;
            Entry[] newEntries = new Entry[newMask + 1];

            for (int i = 0; i < entries.Length; i++)
            {
                Entry next;
                for (Entry entry = entries[i]; entry != null; entry = next)
                {
                    int index = entry.HashCode & newMask;
                    next = entry.Next;
                    entry.Next = newEntries[index];
                    newEntries[index] = entry;
                }
            }
            _entries = newEntries;
            _mask = newMask;
        }

        private static bool TextEquals(string str1, char[] str2, int str2Start, int str2Length)
        {
            if (str1.Length != str2Length)
            {
                return false;
            }

            for (int i = 0; i < str1.Length; i++)
            {
                if (str1[i] != str2[str2Start + i])
                {
                    return false;
                }
            }
            return true;
        }

        private class Entry
        {
            internal readonly string Value;
            internal readonly int HashCode;
            internal Entry Next;

            internal Entry(string value, int hashCode, Entry next)
            {
                Value = value;
                HashCode = hashCode;
                Next = next;
            }
        }
    }
}