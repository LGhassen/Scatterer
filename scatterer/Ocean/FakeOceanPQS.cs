using System;
using System.Reflection;
		
namespace scatterer
{
	internal class FakeOceanPQS : PQS
	{
		public new bool isFakeBuild { get { return true; } }

		internal void CloneFrom(PQS ocean)
			{
				FieldInfo[] fields = typeof(PQS).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				foreach (FieldInfo f in fields)
				{
					f.SetValue(this, f.GetValue(ocean));
				}
			}
		}
}