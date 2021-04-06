public class MyHashTable<K, V>
{
    private const int INITIAL_SIZE = 16;
    private int keyCount = 0;

    private K[] keys;
    private V[] values;


    public MyHashTable()
    {
        keys = new K[INITIAL_SIZE];
        values = new V[INITIAL_SIZE];
    }

    public void Put(K key, V value)
    {
        int index = GetKeyIndex(key);
        if (index == -1)  // Key is unique
        {
            index = GetFirstEmptyIndex();
            if (index == -1)  // Table is full
            {
                DoubleTableSize();
            }
            Put(key, value, index);
        }
        else
        {
            Put(key, value, index);
        }
    }

    private void Put(K key, V value, int index)
    {
        keys[index] = key;
        values[index] = value;
        keyCount++;
    }

    public V Get(K key)
    {
        int keyIndex = GetKeyIndex(key);
        if (keyIndex == -1)  // Key not found
        {
            return default;  // Default value of V (null or primitive default value)
        }
        return GetAtIndex(keyIndex);
    }

    private V GetAtIndex(int index)
    {
        return values[index];
    }

    public bool Remove(K key)
    {
        int index = GetKeyIndex(key);
        if (index == -1)  // Key not found
        {
            return false;
        }
        RemoveAtIndex(index);
        return true;
    }

    private void RemoveAtIndex(int index)
    {
        if (IsIndexEmpty(index))
        {
            return;
        }
        keys[index] = default;
        values[index] = default;
        keyCount--;
    }

    private int GetKeyIndex(K key)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (Equals(keys[i], key))
            {
                return i;
            }
        }
        return -1;
    }

    public bool ContainsKey(K key)
    {
        foreach (K k in keys)
        {
            if (Equals(k, key))
            {
                return true;
            }
        }
        return false;
    }

    private int GetFirstEmptyIndex()
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (IsIndexEmpty(i))
            {
                return i;
            }
        }
        return -1;
    }

    private bool IsIndexEmpty(int index)
    {
        return Equals(keys[index], default) && Equals(values[index], default);
    }

    private void DoubleTableSize()
    {
        DoubleArraySize<K>(ref keys);
        DoubleArraySize<V>(ref values);
    }

    private void DoubleArraySize<T>(ref T[] arr)
    {
        T[] newArr = new T[arr.Length * 2];

        for (int i = 0; i < arr.Length; i++)
        {
            newArr[i] = arr[i];
        }

        arr = newArr;
    }

    public int GetKeyCount()
    {
        return keyCount;
    }

    public bool IsEmpty()
    {
        return keyCount == 0;
    }
}