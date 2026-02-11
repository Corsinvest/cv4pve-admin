using System.Globalization;
using System.Text.RegularExpressions;
using Corsinvest.ProxmoxVE.Admin.Core.Query.Models;
using DynamicCore = System.Linq.Dynamic.Core;

namespace Corsinvest.ProxmoxVE.Admin.Core.Query.Executor;

public partial class QueryExecutor(IDataProvider dataProvider)
{
    private static readonly string[] Aggregators = ["Min", "Max", "Average", "Count", "Sum"];

    public async Task<QueryResult> ExecuteAsync(Models.Query query)
    {
        var queryOrig = query;
        query = FixQuery(query);

        var data = await dataProvider.GetAsync(query.From);

        foreach (var join in query.Join)
        {
            var joinData = await dataProvider.GetAsync(join.Table);
            var resultSelector = $"new (inner as {join.Table}, outer as {query.From})";

            data = DynamicCore.DynamicQueryableExtensions.Join(data,
                           joinData,
                           join.ForeignKey,
                           join.PrimaryKey,
                           resultSelector);
        }

        var whereClause = BuildWhereClause(query.Where);
        data = DynamicCore.DynamicQueryableExtensions.Where(data, whereClause);

        var hasAggregates = query.Select.Any(IsAggregate);
        var hasGroupBy = query.GroupBy.Any();

        if (hasGroupBy && hasAggregates)
        {
            var groupByClause = $"new {{ {string.Join(", ", query.GroupBy)} }}";
            data = DynamicCore.DynamicQueryableExtensions.GroupBy(data, groupByClause);
        }

        if (query.Select.Any())
        {
            var selectClause = BuildSelectClause(query.Select, hasGroupBy, hasAggregates);

            if (hasAggregates && !hasGroupBy)
            {
                data = DynamicCore.DynamicQueryableExtensions.GroupBy(data, "1");
            }

            data = DynamicCore.DynamicQueryableExtensions.Select(data, selectClause);
        }

        if (query.OrderBy.Any())
        {
            var orderByClause = string.Join(", ", query.OrderBy.Select(o => $"@{o.Field} {o.Direction}"));
            data = DynamicCore.DynamicQueryableExtensions.OrderBy(data, orderByClause);
        }

        if (query.Offset > 0) { data = DynamicCore.DynamicQueryableExtensions.Skip(data, query.Offset); }
        if (query.Limit > 0) { data = DynamicCore.DynamicQueryableExtensions.Take(data, query.Limit); }

        var results = DynamicCore.DynamicEnumerableExtensions.ToDynamicList(data);

        return new QueryResult(queryOrig, results);
    }

    private static Models.Query FixQuery(Models.Query query)
    {
        query = query.Clone();

        if (!query.Join.Any())
        {
            query.Select = [.. query.Select.Select(f => FixFieldName(f, query.From))];
            query.GroupBy = [.. query.GroupBy.Select(f => FixFieldName(f, query.From))];
            FixConditions(query.Where.Conditions, query.From);

            foreach (var item in query.OrderBy)
            {
                var parts = item.Field.Split('.');
                var tableName = parts.Length > 1 ? parts[0] : query.From;
                item.Field = FixFieldName(item.Field, tableName);
            }
        }
        else
        {
            foreach (var item in query.Join)
            {
                var (foreignKey, primaryKey) = item.ForeignKey.StartsWith($"{query.From}.")
                    ? (item.ForeignKey, item.PrimaryKey)
                    : (item.PrimaryKey, item.ForeignKey);

                item.ForeignKey = FixFieldName(foreignKey, query.From);
                item.PrimaryKey = FixFieldName(primaryKey, item.Table);
            }
        }

        return query;
    }

    private static void FixConditions(List<Condition> conditions, string tableName)
    {
        foreach (var condition in conditions)
        {
            if (!string.IsNullOrEmpty(condition.Field))
            {
                condition.Field = FixFieldName(condition.Field, tableName);
            }
            FixConditions(condition.Conditions, tableName);
        }
    }

    private static string FixFieldName(string fieldName, string tableName)
        => fieldName.Replace($"{tableName}.", string.Empty);

    private static string BuildWhereClause(WhereClause where)
    {
        if (where.Conditions == null || where.Conditions.Count == 0) { return "true"; }

        var conditionStrings = where.Conditions.ConvertAll(c => c.Logic == null
                                                            ? FormatCondition(c)
                                                            : $"({BuildWhereClause(c)})");

        var result = string.Join(where.Logic == "or" ? " || " : " && ", conditionStrings);
        return where.Logic == "or" ? $"({result})" : result;
    }

    private static string FormatCondition(Condition condition)
        => condition.Value == null || condition.Value.Count == 0
            ? throw new ArgumentException($"Missing value for condition {condition.Field} {condition.Operator}")
            : condition.Operator.ToUpper() switch
            {
                "IN" => $"new [] {{{string.Join(", ", condition.Value.Select(FormatValue))}}}.Contains({condition.Field})",
                "NOT IN" or "NOTIN" => $"!new [] {{{string.Join(", ", condition.Value.Select(FormatValue))}}}.Contains({condition.Field})",
                "BETWEEN" when condition.Value.Count >= 2 => $"{condition.Field} >= {FormatValue(condition.Value[0])} && {condition.Field} <= {FormatValue(condition.Value[1])}",
                "=" => $"{condition.Field} == {FormatValue(condition.Value[0])}",
                "!=" => $"{condition.Field} != {FormatValue(condition.Value[0])}",
                ">" => $"{condition.Field} > {FormatValue(condition.Value[0])}",
                ">=" => $"{condition.Field} >= {FormatValue(condition.Value[0])}",
                "<" => $"{condition.Field} < {FormatValue(condition.Value[0])}",
                "<=" => $"{condition.Field} <= {FormatValue(condition.Value[0])}",
                "CONTAINS" or "LIKE" => $"{condition.Field}.Contains({FormatValue(condition.Value[0])})",
                "NOTCONTAINS" or "NOT LIKE" => $"!{condition.Field}.Contains({FormatValue(condition.Value[0])})",
                "STARTSWITH" or "STARTWITH" => $"{condition.Field}.StartsWith({FormatValue(condition.Value[0])})",
                "NOTSTARTSWITH" or "NOTSTARTWITH" => $"!{condition.Field}.StartsWith({FormatValue(condition.Value[0])})",
                "ENDSWITH" or "ENDWITH" => $"{condition.Field}.EndsWith({FormatValue(condition.Value[0])})",
                "NOTENDSWITH" or "NOTENDWITH" => $"!{condition.Field}.EndsWith({FormatValue(condition.Value[0])})",
                _ => throw new NotSupportedException($"Operator '{condition.Operator}' not supported")
            };

    private static string FormatValue(object value)
    {
        if (value is string strValue)
        {
            return DateTime.TryParse(strValue, out var dateValue)
                    ? $"DateTime({dateValue.Ticks})"
                    : $"\"{strValue}\"";
        }
        else if (value is decimal decimalValue)
        {
            return decimalValue.ToString("0.0############", CultureInfo.InvariantCulture);
        }
        else if (value is double doubleValue)
        {
            return doubleValue.ToString("0.0############", CultureInfo.InvariantCulture);
        }

        return value.ToString()!;
    }

    private static string BuildSelectClause(IEnumerable<string> selectFields, bool hasGroupBy, bool hasAggregates)
    {
        var selectParts = new List<string>();

        foreach (var field in selectFields)
        {
            var (fixedField, isAggregate) = FixAggregate(field);

            if (hasGroupBy)
            {
                selectParts.Add(isAggregate ? $"It.{fixedField}" : $"Key.{fixedField}");
            }
            else if (hasAggregates)
            {
                selectParts.Add(isAggregate ? $"It.{fixedField}" : fixedField);
            }
            else
            {
                selectParts.Add($"@{fixedField}");
            }
        }

        return $"new {{ {string.Join(", ", selectParts)} }}";
    }

    private static bool IsAggregate(string fieldName)
        => Aggregators.Any(a => fieldName.StartsWith($"{a}(", StringComparison.OrdinalIgnoreCase));

    private static (string fieldName, bool isAggregate) FixAggregate(string fieldName)
    {
        foreach (var aggregator in Aggregators)
        {
            if (fieldName.StartsWith($"{aggregator}(", StringComparison.OrdinalIgnoreCase))
            {
                if (aggregator.Equals("Count", StringComparison.OrdinalIgnoreCase))
                {
                    var alias = GetAlias(fieldName);
                    if (string.IsNullOrEmpty(alias)) { alias = "Count"; }
                    return ($"Count() as {alias}", true);
                }

                var normalized = Regex.Replace(fieldName,
                                               $@"{aggregator}\s*\(",
                                               $"{aggregator}(",
                                               RegexOptions.IgnoreCase);

                return (normalized, true);
            }
        }

        return (fieldName, false);
    }

    private static string GetAlias(string fieldName)
    {
        var match = AliasRegex().Match(fieldName);
        return match.Success
                ? match.Groups[1].Value
                : string.Empty;
    }

    [GeneratedRegex(@"(?i)\s+as\s+(\w+)", RegexOptions.IgnoreCase)]
    private static partial Regex AliasRegex();
}
