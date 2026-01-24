using MudBlazor;

namespace PotoDocs.Blazor.Helpers;

public class TableGroupSorter<T>
{
    private readonly Func<T, DateTime?> _dateSelector;

    public SortDirection CurrentDirection { get; private set; } = SortDirection.Descending;

    public TableGroupDefinition<T> GroupDefinition { get; }

    public TableGroupSorter(Func<T, DateTime?> dateSelector, string groupName = "Data")
    {
        _dateSelector = dateSelector;

        GroupDefinition = new TableGroupDefinition<T>
        {
            GroupName = groupName,
            Indentation = false,
            Expandable = true,
            IsInitiallyExpanded = false,
            Selector = (e) =>
            {
                var date = _dateSelector(e);
                if (date.HasValue)
                {
                    return new DateTime(date.Value.Year, date.Value.Month, 1);
                }
                return DateTime.MinValue;
            }
        };
    }

    public bool UpdateDirection(SortDirection direction)
    {
        if (direction == SortDirection.None)
            direction = SortDirection.Descending;

        if (CurrentDirection == direction)
            return false;

        CurrentDirection = direction;
        return true;
    }

    public Func<T, object> Sort(Func<T, object> propertySelector)
    {
        return x =>
        {
            long ticks = 0;
            var date = _dateSelector(x);

            if (date.HasValue)
            {
                ticks = new DateTime(date.Value.Year, date.Value.Month, 1).Ticks;
            }

            long fixedGroupKey = (CurrentDirection == SortDirection.Ascending) ? -ticks : ticks;

            return Tuple.Create(fixedGroupKey, propertySelector(x));
        };
    }
}