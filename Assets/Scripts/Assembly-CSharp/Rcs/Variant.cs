using System;

namespace Rcs
{
	public class Variant : IDisposable
	{
		internal Variant(IntPtr cPtr, bool cMemoryOwn)
		{
		}

		public Variant()
		{
		}

		public Variant(string value)
		{
		}

		public Variant(bool value)
		{
		}

		public Variant(int value)
		{
		}

		public Variant(long value)
		{
		}

		public Variant(double value)
		{
		}

		public Variant(Variant other)
		{
		}

		internal static int getCPtr(Variant obj)
		{
			return 0;
		}

		public void Dispose()
		{
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		protected void Finalize()
		{
		}

		private void _DisposeUnmanaged()
		{
		}

		public Variant.VariantType GetVariantType()
		{
			return (Variant.VariantType)Variant.VariantType.TypeNull;
		}

		public string StringValue()
		{
			return default(string);
		}

		public long IntValue()
		{
			return 0L;
		}

		public double DoubleValue()
		{
			return 0.0;
		}

		public bool BoolValue()
		{
			return default(bool);
		}

		private IntPtr swigCPtr;

		protected bool swigCMemOwn;

		private bool disposed;

		public enum VariantType
		{
			TypeNull,
			TypeString,
			TypeBoolean,
			TypeInt,
			TypeDouble
		}
	}
}
