﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace AxoCover.Models.Extensions
{
  public static class GenericExtensions
  {
    public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
      TValue value;
      if (dictionary.TryGetValue(key, out value))
      {
        return value;
      }
      else
      {
        return default(TValue);
      }
    }

    public static bool CheckAs<T>(this object value, Func<T, bool> func)
    {
      return value is T ? func((T)value) : false;
    }

    public static T ParseXml<T>(string fileName)
    {
      using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
      {
        return (T)new XmlSerializer(typeof(T)).Deserialize(stream);
      }
    }

    public static IEnumerable<T> Flatten<T>(this T parent, Func<T, IEnumerable<T>> getChildren)
    {
      return getChildren(parent).Flatten(getChildren);
    }

    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> enumeration, Func<T, IEnumerable<T>> getChildren)
    {
      var stack = new Stack<IEnumerator>();
      stack.Push(enumeration.GetEnumerator());
      while (stack.Count > 0)
      {
        var enumerator = stack.Peek();
        if (enumerator.MoveNext())
        {
          var item = (T)enumerator.Current;
          yield return item;

          var children = getChildren(item);
          if (children != null)
          {
            stack.Push(children.GetEnumerator());
          }
        }
        else
        {
          stack.Pop();
        }
      }
    }

    public static IEnumerable<T> Crawl<T>(this T item, Func<T, T> getLayer)
      where T : class
    {
      if (item == null)
        throw new ArgumentNullException(nameof(item));

      item = getLayer(item);
      while (item != null)
      {
        yield return item;
        item = getLayer(item);
      }
    }

    public static void OrderedAdd<T>(this IList<T> list, T item, Comparison<T> onCompare, ReplacementBehavior replacementBehavior = ReplacementBehavior.KeepBoth)
    {
      var index = 0;
      while (index < list.Count && onCompare(list[index], item) <= 0)
      {
        index++;
      }

      switch (replacementBehavior)
      {
        case ReplacementBehavior.Ignore:
          if (index > 0 && onCompare(list[index - 1], item) == 0)
          {
            return;
          }
          goto default;
        case ReplacementBehavior.Replace:
          if (index > 0 && onCompare(list[index - 1], item) == 0)
          {
            list.RemoveAt(index - 1);
            index--;
          }
          goto default;
        default:
          list.Insert(index, item);
          break;
      }
    }
  }

  public enum ReplacementBehavior
  {
    KeepBoth,
    Replace,
    Ignore
  }
}