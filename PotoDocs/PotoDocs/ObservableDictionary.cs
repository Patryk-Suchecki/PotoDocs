using System.Collections.Specialized;

namespace PotoDocs;

public class ObservableDictionary<TKey, TValue> : ObservableCollection<KeyValuePair<TKey, TValue>>
{
    public TValue this[TKey key]
    {
        get => TryGetValue(key, out var value) ? value : default;
        set
        {
            var existingItem = this.FirstOrDefault(kvp => kvp.Key.Equals(key));
            if (existingItem.Key != null)
            {
                Remove(existingItem);
            }
            Add(new KeyValuePair<TKey, TValue>(key, value));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        var item = this.FirstOrDefault(kvp => kvp.Key.Equals(key));
        if (item.Key != null)
        {
            value = item.Value;
            return true;
        }
        value = default;
        return false;
    }

    public bool ContainsKey(TKey key)
    {
        return this.Any(kvp => kvp.Key.Equals(key));
    }

    public void Remove(TKey key)
    {
        var existingItem = this.FirstOrDefault(kvp => kvp.Key.Equals(key));
        if (existingItem.Key != null)
        {
            Remove(existingItem);
        }
    }

    public new void Add(KeyValuePair<TKey, TValue> item)
    {
        base.Add(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Add(TKey key, TValue value)
    {
        Add(new KeyValuePair<TKey, TValue>(key, value));
    }
}
