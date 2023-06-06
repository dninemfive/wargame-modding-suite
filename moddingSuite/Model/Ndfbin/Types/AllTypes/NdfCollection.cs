using moddingSuite.BL.Ndf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace moddingSuite.Model.Ndfbin.Types.AllTypes;

public class NdfCollection : NdfValueWrapper, IList<CollectionItemValueHolder>, INotifyCollectionChanged, IList
{
    public NdfCollection()
        : base(NdfType.List)
    {
    }

    public NdfCollection(IEnumerable<CollectionItemValueHolder> list)
        : this()
    {
        if (list != null)
        {
            foreach (CollectionItemValueHolder wrapper in list)
                InnerList.Add(wrapper);
        }
    }

    protected ObservableCollection<CollectionItemValueHolder> InnerList { get; } = new();

    #region IList<CollectionItemValueHolder> Members

    public int IndexOf(CollectionItemValueHolder item) => InnerList.IndexOf(item);

    public void Insert(int index, CollectionItemValueHolder item)
    {
        InnerList.Insert(index, item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void RemoveAt(int index)
    {
        InnerList.RemoveAt(index);

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public CollectionItemValueHolder this[int index]
    {
        get => InnerList[index];
        set => InnerList[index] = value;
    }

    public void Add(CollectionItemValueHolder item)
    {
        InnerList.Add(item);

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Clear()
    {
        InnerList.Clear();

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(CollectionItemValueHolder item) => InnerList.Contains(item);

    public void CopyTo(CollectionItemValueHolder[] array, int arrayIndex) => InnerList.CopyTo(array, arrayIndex);

    public int Count => InnerList.Count;

    public bool IsReadOnly => false;

    public bool Remove(CollectionItemValueHolder item)
    {
        bool res = InnerList.Remove(item);

        if (res)
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        return res;
    }

    public IEnumerator<CollectionItemValueHolder> GetEnumerator() => InnerList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => InnerList.GetEnumerator();

    #endregion

    #region IList

    #region IList Members

    public int Add(object value)
    {
        if (value is not CollectionItemValueHolder val)
            return -1;

        Add(val);

        return IndexOf(value);
    }

    public bool Contains(object value) => value is CollectionItemValueHolder && InnerList.Contains(value as CollectionItemValueHolder);

    public int IndexOf(object value) => IndexOf(value as CollectionItemValueHolder);

    public void Insert(int index, object value) => Insert(index, value as CollectionItemValueHolder);

    public bool IsFixedSize => false;

    public void Remove(object value) => Remove(value as CollectionItemValueHolder);

    object IList.this[int index]
    {
        get => this[index];
        set => this[index] = value as CollectionItemValueHolder;
    }

    public void CopyTo(Array array, int index) => throw new NotImplementedException();

    public bool IsSynchronized => false;

    public object SyncRoot => this;

    #endregion

    #region INotifyCollectionChanged Members

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    #endregion

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);

    #endregion

    public override string ToString() => string.Format("Collection[{0}]", InnerList.Count);

    public override byte[] GetBytes()
    {
        List<byte> data = new();

        data.AddRange(BitConverter.GetBytes(InnerList.Count));

        foreach (CollectionItemValueHolder valueHolder in InnerList)
        {
            byte[] valueDat = valueHolder.Value.GetBytes();

            if (valueHolder.Value.Type is NdfType.ObjectReference or
                NdfType.TransTableReference)
            {
                data.AddRange(BitConverter.GetBytes((uint)NdfType.Reference));
            }

            data.AddRange(BitConverter.GetBytes((uint)valueHolder.Value.Type));
            data.AddRange(valueDat);
        }

        return data.ToArray();
    }

    public override byte[] GetNdfText()
    {
        Encoding enc = NdfTextWriter.NdfTextEncoding;

        using MemoryStream ms = new();
        byte[] buffer = enc.GetBytes("[\n");
        ms.Write(buffer, 0, buffer.Length);

        foreach (CollectionItemValueHolder collectionItemValueHolder in InnerList)
        {
            buffer = collectionItemValueHolder.Value.GetNdfText();
            ms.Write(buffer, 0, buffer.Length);

            if (InnerList.IndexOf(collectionItemValueHolder) < InnerList.Count)
            {
                buffer = enc.GetBytes(",\n");
                ms.Write(buffer, 0, buffer.Length);
            }
        }

        buffer = enc.GetBytes("]\n");
        ms.Write(buffer, 0, buffer.Length);

        return ms.ToArray();
    }
}