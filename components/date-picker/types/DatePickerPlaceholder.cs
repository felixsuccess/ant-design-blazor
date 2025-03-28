﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace AntDesign
{
    internal class DatePickerPlaceholder
    {
        public static string GetPlaceholderByType(DatePickerType pickerType, DatePickerLocale locale)
        {
            var placeholder = pickerType switch
            {
                DatePickerType.Date => locale.Lang.DateSelect,
                DatePickerType.Week => locale.Lang.WeekSelect,
                DatePickerType.Month => locale.Lang.MonthSelect,
                DatePickerType.Quarter => locale.Lang.QuarterSelect,
                DatePickerType.Year => locale.Lang.YearSelect,
                DatePickerType.Time => locale.Lang.TimeSelect,
                _ => locale.Lang.DateSelect,
            };

            return placeholder;
        }

        public static (string, string) GetRangePlaceHolderByType(DatePickerType pickerType, DatePickerLocale locale)
        {
            var placeholder = pickerType switch
            {
                DatePickerType.Date => (locale.Lang.StartDate, locale.Lang.EndDate),
                DatePickerType.Week => (locale.Lang.StartWeek, locale.Lang.EndWeek),
                DatePickerType.Month => (locale.Lang.StartMonth, locale.Lang.EndMonth),
                DatePickerType.Year => (locale.Lang.StartYear, locale.Lang.EndYear),
                DatePickerType.Time => (locale.Lang.StartDate, locale.Lang.EndDate),
                DatePickerType.Quarter => (locale.Lang.StartQuarter, locale.Lang.EndQuarter),
                _ => (locale.Lang.StartDate, locale.Lang.EndDate),
            };
            return placeholder;
        }
    }
}
