using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace UnitTestProjectForTransformers
{
    /// <summary>
    /// source: http://stackoverflow.com/questions/33022993/compare-two-arbitrary-jtoken-s-of-the-same-structure
    ///
    /// In Linq-to-JSON, JValue represents a primitive value (string, number, boolean, and so on). It implements IComparable<JValue>, so Json.NET takes care of sorting primitive values for you.
    /// 
    /// Building off of that, you're going to need to recursively descend the two JToken object hierarchies in parallel. When you encounter the first token with a different .Net type, or different properties (if not a JValue), or /// with a different value (if a JValue), you need to return back the comparison value.
    ///
    /// Keep in mind the following:
    ///
    /// A comparison method should be reflexive, symmetric and transitive.
    /// Container tokens of different .Net type need to be ordered by type in some consistent manner.
    /// the child tokens of JArray and JConstructor are ordered.
    /// the child tokens of JObject are not, so they need to be compared in some stable, symmetric manner. Walking both in order of property name would seem to work.
    /// There is no obvious way to compare JRaw, so don't try, and let an exception get thrown.
    ///
    /// The following is a prototype implementation:    
    /// 
    /// </summary>
    public class JTokenComparer : IComparer<JToken>
    {
        public static JTokenComparer Instance { get { return instance; } }

        static JTokenComparer instance;

        static JTokenComparer()
        {
            instance = new JTokenComparer();
        }

        readonly Dictionary<Type, KeyValuePair<int, IComparer<JToken>>> dict;

        JTokenComparer()
        {
            dict = new Dictionary<Type, KeyValuePair<int, IComparer<JToken>>>
        {
            // Order chosen semi-arbitrarily.  Putting values first seems reasonable though.
            {typeof(JValue), new KeyValuePair<int, IComparer<JToken>>(0, new JValueComparer()) },
            {typeof(JProperty), new KeyValuePair<int, IComparer<JToken>>(1, new JPropertyComparer()) },
            {typeof(JArray), new KeyValuePair<int, IComparer<JToken>>(2, new JArrayComparer()) },
            {typeof(JObject), new KeyValuePair<int, IComparer<JToken>>(3, new JObjectComparer()) },
            {typeof(JConstructor), new KeyValuePair<int, IComparer<JToken>>(4, new JConstructorComparer()) },
        };
        }

        #region IComparer<JToken> Members

        public int Compare(JToken x, JToken y)
        {
            if (x is JRaw || y is JRaw)
                throw new InvalidOperationException("Tokens of type JRaw cannot be sorted");
            if (object.ReferenceEquals(x, y))
                return 0;
            else if (x == null)
                return -1;
            else if (y == null)
                return 1;

            var typeData1 = dict[x.GetType()];
            var typeData2 = dict[y.GetType()];

            int comp;
            if ((comp = typeData1.Key.CompareTo(typeData2.Key)) != 0)
                return comp;
            if (typeData1.Value != typeData2.Value)
                throw new InvalidOperationException("inconsistent dictionary values"); // Internal error
            return typeData2.Value.Compare(x, y);
        }

        #endregion
    }

    abstract class JTokenComparerBase<TJToken> : IComparer<JToken> where TJToken : JToken
    {
        protected TJToken CheckType(JToken item)
        {
            if (item != null && item.GetType() != typeof(TJToken))
                throw new ArgumentException(string.Format("Actual type {0} of token \"{1}\" does not match expected type {2}", item.GetType(), item, typeof(TJToken)));
            return (TJToken)item;
        }

        protected bool TryBaseCompare(TJToken x, TJToken y, out int comparison)
        {
            CheckType(x);
            CheckType(y);
            if (object.ReferenceEquals(x, y))
            {
                comparison = 0;
                return true;
            }
            else if (x == null)
            {
                comparison = -1;
                return true;
            }
            else if (y == null)
            {
                comparison = 1;
                return true;
            }
            comparison = 0;
            return false;
        }

        protected abstract int CompareDerived(TJToken x, TJToken y);

        protected int TokenCompare(JToken x, JToken y)
        {
            var tx = CheckType(x);
            var ty = CheckType(y);
            int comp;
            if (TryBaseCompare(tx, ty, out comp))
                return comp;
            return CompareDerived(tx, ty);
        }

        #region IComparer<JToken> Members

        int IComparer<JToken>.Compare(JToken x, JToken y)
        {
            return TokenCompare(x, y);
        }

        #endregion
    }

    abstract class JContainerOrderedComparerBase<TJToken> : JTokenComparerBase<TJToken> where TJToken : JContainer
    {
        protected int CompareItemsInOrder(TJToken x, TJToken y)
        {
            int comp;
            // Dictionary order: sort on items before number of items.
            for (int i = 0, n = Math.Min(x.Count, y.Count); i < n; i++)
                if ((comp = JTokenComparer.Instance.Compare(x[i], y[i])) != 0)
                    return comp;
            if ((comp = x.Count.CompareTo(y.Count)) != 0)
                return comp;
            return 0;
        }
    }

    class JPropertyComparer : JTokenComparerBase<JProperty>
    {
        protected override int CompareDerived(JProperty x, JProperty y)
        {
            int comp;
            if ((comp = x.Name.CompareTo(y.Name)) != 0)
                return comp;
            return JTokenComparer.Instance.Compare(x.Value, y.Value);
        }
    }

    class JObjectComparer : JTokenComparerBase<JObject>
    {
        protected override int CompareDerived(JObject x, JObject y)
        {
            int comp;
            // Dictionary order: sort on items before number of items.
            // Order both property sequences to preserve reflexivity.
            foreach (var propertyComp in x.Properties().OrderBy(p => p.Name).Zip(y.Properties().OrderBy(p => p.Name), (xp, yp) => JTokenComparer.Instance.Compare(xp, yp)))
                if (propertyComp != 0)
                    return propertyComp;
            if ((comp = x.Count.CompareTo(y.Count)) != 0)
                return comp;
            return 0;
        }
    }

    class JArrayComparer : JContainerOrderedComparerBase<JArray>
    {
        protected override int CompareDerived(JArray x, JArray y)
        {
            int comp;
            if ((comp = CompareItemsInOrder(x, y)) != 0)
                return comp;
            return 0;
        }
    }

    class JConstructorComparer : JContainerOrderedComparerBase<JConstructor>
    {
        protected override int CompareDerived(JConstructor x, JConstructor y)
        {
            int comp;
            if ((comp = x.Name.CompareTo(y.Name)) != 0)
                return comp;
            if ((comp = CompareItemsInOrder(x, y)) != 0)
                return comp;
            return 0;
        }
    }

    class JValueComparer : JTokenComparerBase<JValue>
    {
        protected override int CompareDerived(JValue x, JValue y)
        {
            return Comparer<JToken>.Default.Compare(x, y); // JValue implements IComparable<JValue>
        }
    }

}
