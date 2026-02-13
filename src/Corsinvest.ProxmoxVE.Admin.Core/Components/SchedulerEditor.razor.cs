/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class SchedulerEditor
{
    private ScheduleTypeEnum _scheduleType = ScheduleTypeEnum.Periodic;
    private int _periodicValue = 1;
    private TimeUnitEnum _selectedTimeUnit = TimeUnitEnum.Hours;
    private DateTime _dailyTime = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
    private HashSet<DayOfWeek> _selectedDays = [];
    private DateTime _monthlyStartTime = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
    private HashSet<MonthEnum> _selectedMonths = [];
    private int _monthlyDay = 1;
    private MonthlyTypeEnum _monthlyType = MonthlyTypeEnum.First;
    private DayOfWeek _monthlyDayOfWeek = DayOfWeek.Monday;
    private MonthlyModeEnum _monthlyMode = MonthlyModeEnum.DayOfMonth;
    private bool _isUpdatingFromCron;
    private string _cronExpression = "0 * * * *"; // Default to every hour

    private static readonly Dictionary<DayOfWeek, int> CronDayMap = new()
    {
        [DayOfWeek.Sunday] = 0,
        [DayOfWeek.Monday] = 1,
        [DayOfWeek.Tuesday] = 2,
        [DayOfWeek.Wednesday] = 3,
        [DayOfWeek.Thursday] = 4,
        [DayOfWeek.Friday] = 5,
        [DayOfWeek.Saturday] = 6
    };

    private static readonly Dictionary<int, DayOfWeek> ReverseCronDayMap = CronDayMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    private static readonly Dictionary<MonthEnum, int> CronMonthMap = Enum.GetValues<MonthEnum>().ToDictionary(m => m, m => (int)m);
    private static readonly Dictionary<int, MonthEnum> ReverseCronMonthMap = CronMonthMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    private static readonly IReadOnlyList<TimeUnitEnum> AvailableTimeUnits =
        [TimeUnitEnum.Minutes, TimeUnitEnum.Hours, TimeUnitEnum.Days, TimeUnitEnum.Months];

    [Parameter]
    public string CronExpression
    {
        get => _cronExpression;
        set => SetCronExpression(value);
    }

    [Parameter]
    public EventCallback<string> CronExpressionChanged { get; set; }

    private ScheduleTypeEnum ScheduleType
    {
        get => _scheduleType;
        set => SetProperty(ref _scheduleType, value);
    }

    private int PeriodicValue
    {
        get => _periodicValue;
        set => SetProperty(ref _periodicValue, Math.Clamp(value, 1, PeriodicMax));
    }

    private TimeUnitEnum SelectedTimeUnit
    {
        get => _selectedTimeUnit;
        set => SetProperty(ref _selectedTimeUnit, value);
    }

    private DateTime DailyTime
    {
        get => _dailyTime;
        set => SetProperty(ref _dailyTime, value);
    }

    private IEnumerable<DayOfWeek> SelectedDays
    {
        get => _selectedDays;
        set => SetCollectionProperty(ref _selectedDays, value);
    }

    private DateTime MonthlyStartTime
    {
        get => _monthlyStartTime;
        set => SetProperty(ref _monthlyStartTime, value);
    }

    private IEnumerable<MonthEnum> SelectedMonths
    {
        get => _selectedMonths;
        set => SetCollectionProperty(ref _selectedMonths, value ?? Enum.GetValues<MonthEnum>());
    }

    private int MonthlyDay
    {
        get => _monthlyDay;
        set => SetProperty(ref _monthlyDay, Math.Clamp(value, 1, 31));
    }

    private MonthlyTypeEnum MonthlyType
    {
        get => _monthlyType;
        set => SetProperty(ref _monthlyType, value);
    }

    private DayOfWeek MonthlyDayOfWeek
    {
        get => _monthlyDayOfWeek;
        set => SetProperty(ref _monthlyDayOfWeek, value);
    }

    private MonthlyModeEnum MonthlyMode
    {
        get => _monthlyMode;
        set => SetProperty(ref _monthlyMode, value);
    }

    private int PeriodicMax => SelectedTimeUnit switch
    {
        TimeUnitEnum.Minutes => 59,
        TimeUnitEnum.Hours => 23,
        TimeUnitEnum.Days => 31,
        TimeUnitEnum.Months => 12,
        _ => 23
    };

    private bool IsPeriodicMode => ScheduleType == ScheduleTypeEnum.Periodic;
    private bool IsDailyMode => ScheduleType == ScheduleTypeEnum.Daily;
    private bool IsMonthlyMode => ScheduleType == ScheduleTypeEnum.Monthly;
    private bool IsMonthlyDayMode => MonthlyMode == MonthlyModeEnum.DayOfMonth;
    private bool IsMonthlyWeekMode => MonthlyMode == MonthlyModeEnum.DayOfWeek;

    private enum ScheduleTypeEnum
    {
        Periodic,
        Daily,
        Monthly
    }

    private enum MonthlyModeEnum
    {
        DayOfMonth,
        DayOfWeek
    }

    private enum TimeUnitEnum
    {
        Seconds,
        Minutes,
        Hours,
        Days,
        Weeks,
        Months
    }

    private enum MonthEnum
    {
        January = 1,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }

    private enum MonthlyTypeEnum
    {
        First = 0,
        Second,
        Third,
        Fourth,
        Fifth,
        Last
    }

    private void SetCronExpression(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || _cronExpression == value) { return; }

        _cronExpression = value;
        if (!_isUpdatingFromCron)
        {
            ParseCronExpression(_cronExpression);
            StateHasChanged();
        }
    }

    private void SetProperty<T>(ref T field, T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) { return; }

        field = value;
        _ = GenerateAndUpdateCronExpression();
        StateHasChanged();
    }

    private void SetCollectionProperty<T>(ref HashSet<T> field, IEnumerable<T> value)
    {
        var newSet = value?.ToHashSet() ?? [];
        if (field.SetEquals(newSet)) { return; }

        field = newSet;
        _ = GenerateAndUpdateCronExpression();
        StateHasChanged();
    }

    private async Task GenerateAndUpdateCronExpression()
    {
        if (_isUpdatingFromCron) { return; }

        try
        {
            var newExpression = GenerateCronExpression();
            if (newExpression != _cronExpression)
            {
                _cronExpression = newExpression;
                await CronExpressionChanged.InvokeAsync(_cronExpression);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
        }
    }

    private string GenerateCronExpression() => ScheduleType switch
    {
        ScheduleTypeEnum.Periodic => GeneratePeriodicCron(),
        ScheduleTypeEnum.Daily => GenerateDailyCron(),
        ScheduleTypeEnum.Monthly => GenerateMonthlyCron(),
        _ => "0 * * * *"
    };

    private string GeneratePeriodicCron() => SelectedTimeUnit switch
    {
        TimeUnitEnum.Minutes => $"*/{PeriodicValue} * * * *",
        TimeUnitEnum.Hours => $"0 */{PeriodicValue} * * *",
        TimeUnitEnum.Days => $"0 0 */{PeriodicValue} * *",
        TimeUnitEnum.Months => $"0 0 1 */{PeriodicValue} *",
        _ => "0 * * * *"
    };

    private string GenerateDailyCron()
    {
        var dayString = _selectedDays.Count > 0
            ? _selectedDays.Select(d => CronDayMap[d]).Order().JoinAsString(",")
            : "*";

        return $"{DailyTime.Minute} {DailyTime.Hour} * * {dayString}";
    }

    private string GenerateMonthlyCron()
    {
        var minute = MonthlyStartTime.Minute;
        var hour = MonthlyStartTime.Hour;
        var months = _selectedMonths.Count > 0
            ? _selectedMonths.Select(m => CronMonthMap[m]).Order().JoinAsString(",")
            : "*";

        return MonthlyMode switch
        {
            MonthlyModeEnum.DayOfMonth => $"{minute} {hour} {MonthlyDay} {months} *",
            MonthlyModeEnum.DayOfWeek => GenerateWeeklyMonthlyCron(minute, hour, months),
            _ => "0 * * * *"
        };
    }

    private string GenerateWeeklyMonthlyCron(int minute, int hour, string months)
    {
        var dayIndex = CronDayMap[MonthlyDayOfWeek];
        var weekPart = MonthlyType == MonthlyTypeEnum.Last ? "L" : $"#{(int)MonthlyType + 1}";
        return $"{minute} {hour} * {months} {dayIndex}{weekPart}";
    }

    private void ParseCronExpression(string cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            ResetToDefault();
            return;
        }

        try
        {
            _isUpdatingFromCron = true;
            var parts = cronExpression.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 5)
            {
                ResetToDefault();
                return;
            }

            var cronParts = new CronParts(parts[0], parts[1], parts[2], parts[3], parts[4]);

            if (!IsValidTimeFormat(cronParts.Minute, cronParts.Hour))
            {
                ResetToDefault();
                return;
            }

            if (TryParseDailyCron(cronParts) ||
                TryParseMonthlyCron(cronParts) ||
                TryParsePeriodicCron(cronParts))
            {
                return;
            }

            ResetToDefault();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error parsing cron expression: {Message}", ex.Message);
            ResetToDefault();
        }
        finally
        {
            _isUpdatingFromCron = false;
        }
    }

    private readonly record struct CronParts(string Minute, string Hour, string DayOfMonth, string Month, string DayOfWeek);

    private static bool IsValidTimeFormat(string minute, string hour) =>
        IsValidCronPart(minute, 0, 59) && IsValidCronPart(hour, 0, 23);

    private bool TryParseDailyCron(CronParts parts)
    {
        if (!IsDailyPattern(parts)) { return false; }

        _scheduleType = ScheduleTypeEnum.Daily;
        _selectedDays = ParseDaysOfWeek(parts.DayOfWeek);

        return ParseTime(parts.Minute, parts.Hour, ref _dailyTime);
    }

    private static bool IsDailyPattern(CronParts parts) =>
        IsWildcardOrQuestion(parts.DayOfMonth) && parts.Month == "*";

    private bool TryParseMonthlyCron(CronParts parts)
    {
        // Day of month mode
        if (TryParseMonthlyDayMode(parts)) { return true; }

        // Day of week mode
        return TryParseMonthlyWeekMode(parts);
    }

    private bool TryParseMonthlyDayMode(CronParts parts)
    {
        if (!int.TryParse(parts.DayOfMonth, out var day)
            || day < 1
            || day > 31
            || !IsWildcardOrQuestion(parts.DayOfWeek))
        {
            return false;
        }

        _scheduleType = ScheduleTypeEnum.Monthly;
        _monthlyMode = MonthlyModeEnum.DayOfMonth;
        _monthlyDay = day;
        ParseMonths(parts.Month);

        return ParseTime(parts.Minute, parts.Hour, ref _monthlyStartTime);
    }

    private bool TryParseMonthlyWeekMode(CronParts parts)
    {
        if (!IsWildcardOrQuestion(parts.DayOfMonth)
            || !(parts.DayOfWeek.Contains('#')
            || parts.DayOfWeek.EndsWith('L')))
        {
            return false;
        }

        _scheduleType = ScheduleTypeEnum.Monthly;
        _monthlyMode = MonthlyModeEnum.DayOfWeek;

        if (!ParseWeeklyMonthly(parts.DayOfWeek)) { return false; }

        ParseMonths(parts.Month);
        return ParseTime(parts.Minute, parts.Hour, ref _monthlyStartTime);
    }

    private bool TryParsePeriodicCron(CronParts parts)
    {
        var periodicPatterns = new[]
        {
            (TimeUnitEnum.Minutes, parts.Minute.StartsWith("*/"), parts.Hour == "*", parts.DayOfMonth == "*", parts.Month == "*", parts.DayOfWeek == "*", parts.Minute),
            (TimeUnitEnum.Hours, parts.Hour.StartsWith("*/"), parts.Minute == "0", parts.DayOfMonth == "*", parts.Month == "*", parts.DayOfWeek == "*", parts.Hour),
            (TimeUnitEnum.Days, parts.DayOfMonth.StartsWith("*/"), parts.Minute == "0", parts.Hour == "0", parts.Month == "*", parts.DayOfWeek == "*", parts.DayOfMonth),
            (TimeUnitEnum.Months, parts.Month.StartsWith("*/"), parts.Minute == "0", parts.Hour == "0", parts.DayOfMonth == "1", parts.DayOfWeek == "*", parts.Month)
        };

        foreach (var (unit, hasPattern, cond1, cond2, cond3, cond4, valuePart) in periodicPatterns)
        {
            if (hasPattern && cond1 && cond2 && cond3 && cond4)
            {
                var value = ParsePeriodicValue(valuePart);
                var maxValue = unit switch
                {
                    TimeUnitEnum.Minutes => 59,
                    TimeUnitEnum.Hours => 23,
                    TimeUnitEnum.Days => 31,
                    TimeUnitEnum.Months => 12,
                    _ => 0
                };

                if (value > 0 && value <= maxValue)
                {
                    _scheduleType = ScheduleTypeEnum.Periodic;
                    _selectedTimeUnit = unit;
                    _periodicValue = value;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsWildcardOrQuestion(string part) => part is "*" or "?";

    private static bool IsValidCronPart(string part, int min, int max)
        => part switch
        {
            "*" or "?" => true,
            _ when part.StartsWith("*/") => int.TryParse(part[2..], out var val) && val >= min && val <= max,
            _ when int.TryParse(part, out var value) => value >= min && value <= max,
            _ when part.Contains(',') => part.Split(',').All(p => int.TryParse(p.Trim(), out var v) && v >= min && v <= max),
            _ => false
        };

    private static HashSet<DayOfWeek> ParseDaysOfWeek(string dayOfWeekPart)
    {
        if (IsWildcardOrQuestion(dayOfWeekPart)) { return [.. Enum.GetValues<DayOfWeek>()]; }

        try
        {
            return [.. dayOfWeekPart.Split(',')
                                    .Select(d => d.Trim())
                                    .Where(d => int.TryParse(d, out var day) && ReverseCronDayMap.ContainsKey(day))
                                    .Select(d => ReverseCronDayMap[int.Parse(d)])];
        }
        catch
        {
            return [];
        }
    }

    private bool ParseWeeklyMonthly(string dayOfWeek)
    {
        try
        {
            if (dayOfWeek.EndsWith('L'))
            {
                var dayPart = dayOfWeek[..^1];
                if (int.TryParse(dayPart, out var dayIndex) && ReverseCronDayMap.TryGetValue(dayIndex, out var dayOfWeekValue))
                {
                    _monthlyDayOfWeek = dayOfWeekValue;
                    _monthlyType = MonthlyTypeEnum.Last;
                    return true;
                }
            }
            else if (dayOfWeek.Contains('#'))
            {
                var parts = dayOfWeek.Split('#');
                if (parts.Length == 2
                    && int.TryParse(parts[0], out var dayIndex)
                    && int.TryParse(parts[1], out var weekNumber)
                    && ReverseCronDayMap.TryGetValue(dayIndex, out var dayOfWeekValue)
                    && weekNumber is >= 1 and <= 5)
                {
                    _monthlyDayOfWeek = dayOfWeekValue;
                    _monthlyType = (MonthlyTypeEnum)(weekNumber - 1);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error parsing weekly monthly pattern: {Message}", ex.Message);
        }

        return false;
    }

    private static bool ParseTime(string minute, string hour, ref DateTime timeField)
    {
        if (!int.TryParse(minute, out var m)
            || !int.TryParse(hour, out var h)
            || m is < 0 or > 59
            || h is < 0 or > 23)
        {
            return false;
        }

        timeField = new DateTime(timeField.Year, timeField.Month, timeField.Day, h, m, 0);
        return true;
    }

    private static int ParsePeriodicValue(string value) =>
        int.TryParse(value.Replace("*/", ""), out var result) ? Math.Max(1, result) : 1;

    private void ParseMonths(string monthPart)
        => _selectedMonths = monthPart switch
        {
            "*" => [.. Enum.GetValues<MonthEnum>()],
            _ when monthPart.Contains(',') => [.. ParseCommaSeparatedMonths(monthPart)],
            _ when int.TryParse(monthPart, out var month) && ReverseCronMonthMap.ContainsKey(month) => [ReverseCronMonthMap[month]],
            _ => [.. Enum.GetValues<MonthEnum>()]
        };

    private static IEnumerable<MonthEnum> ParseCommaSeparatedMonths(string monthPart)
        => monthPart.Split(',')
                    .Where(m => int.TryParse(m.Trim(), out var month) && ReverseCronMonthMap.ContainsKey(month))
                    .Select(m => ReverseCronMonthMap[int.Parse(m.Trim())]);

    private void ResetToDefault()
    {
        _scheduleType = ScheduleTypeEnum.Periodic;
        _selectedTimeUnit = TimeUnitEnum.Hours;
        _periodicValue = 1;
        _selectedDays = [];
        _selectedMonths = [.. Enum.GetValues<MonthEnum>()];
    }
}
