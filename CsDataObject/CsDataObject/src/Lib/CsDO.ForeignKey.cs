/*
 * Created by SharpDevelop.
 * User: carol
 * Date: 28/9/2005
 * Time: 21:37
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;

namespace CsDO.Lib
{

	public class ForeignKey_1_n : ArrayList
	{
		public ForeignKey_1_n() : base() { }
		public ForeignKey_1_n(ICollection c) : base(c) { }
		public ForeignKey_1_n(int capacity) : base(capacity) { }
		
		public new DataObject this[int index] 
		{
			get { return (DataObject) base[index]; }
			set { base[index] = value; }
		}
		
		public int Add(DataObject value) { return base.Add(value); }
		public override int Add(object value) 
		{ 
			if (value.GetType().IsSubclassOf(typeof(DataObject)))
				throw new CsDOException("Should add only subclasses of DataObject");
			
			return Add((DataObject) value);
		}
		
		public override void AddRange(ICollection c)
		{
			if (c.GetType() != typeof(ForeignKey_1_n))
				throw new CsDOException("Should add only ForeignKey");
			
			base.AddRange(c);
		}
		
		public override void Clear() { base.Clear(); }
		
		public int IndexOf(DataObject value) 
		{ 
			return base.IndexOf((DataObject) value); 
		}
		
		public override int IndexOf(object value) 
		{ 
			if (value.GetType().IsSubclassOf(typeof(DataObject)))
				throw new CsDOException("Should add only subclasses of DataObject");
			
			return IndexOf((DataObject) value);
		}
		
		public int IndexOf(DataObject value, int startIndex)
		{ 
			return base.IndexOf((DataObject) value, startIndex);
		}
		
		public override int IndexOf(object value, int startIndex)
		{
			if (value.GetType().IsSubclassOf(typeof(DataObject)))
				throw new CsDOException("Should add only subclasses of DataObject");
			
			return IndexOf((DataObject) value, startIndex);
		}
		
		public int IndexOf(DataObject value, int startIndex, int count)
		{ 
			return base.IndexOf((DataObject) value, startIndex, count);
		}
		
		public override int IndexOf(object value, int startIndex, int count)
		{
			if (value.GetType().IsSubclassOf(typeof(DataObject)))
				throw new CsDOException("Should add only subclasses of DataObject");
			
			return IndexOf((DataObject) value, startIndex, count);
		}
		
		public void Insert(int index, DataObject value) { base.Insert(index, value); }
		public override void Insert(int index, object value)
		{
			if (value.GetType().IsSubclassOf(typeof(DataObject)))
				throw new CsDOException("Should add only subclasses of DataObject");
			
			Insert(index, (DataObject) value);
		}
		
		public override void InsertRange(int index, ICollection c)
		{
			if (c.GetType() != typeof(ForeignKey_1_n))
				throw new CsDOException("Should add only ForeignKey");
			
			base.InsertRange(index, c);
		}
		
		public void Remove(DataObject obj) { base.Remove(obj); }
		public override void Remove(object obj)
		{
			if (obj.GetType().IsSubclassOf(typeof(DataObject)))
				throw new CsDOException("Should add only subclasses of DataObject");
			
			Remove((DataObject) obj);
		}
		
		public override void RemoveRange(int index, int count) 
		{
			base.RemoveRange(index, count);
		}
		
		public int LastIndexOf(DataObject value) 
		{ 
			return base.LastIndexOf((DataObject) value); 
		}
		
		public override int LastIndexOf(object value) 
		{ 
			if (value.GetType().IsSubclassOf(typeof(DataObject)))
				throw new CsDOException("Should add only subclasses of DataObject");
			
			return LastIndexOf((DataObject) value);
		}
		
		public int LastIndexOf(DataObject value, int startIndex)
		{ 
			return base.LastIndexOf((DataObject) value, startIndex);
		}
		
		public override int LastIndexOf(object value, int startIndex)
		{
			if (value.GetType().IsSubclassOf(typeof(DataObject)))
				throw new CsDOException("Should add only subclasses of DataObject");
			
			return LastIndexOf((DataObject) value, startIndex);
		}
		
		public int LastIndexOf(DataObject value, int startIndex, int count)
		{ 
			return base.LastIndexOf((DataObject) value, startIndex, count);
		}
		
		public override int LastIndexOf(object value, int startIndex, int count)
		{
			if (value.GetType().IsSubclassOf(typeof(DataObject)))
				throw new CsDOException("Should add only subclasses of DataObject");
			
			return LastIndexOf((DataObject) value, startIndex, count);
		}
	}
}
