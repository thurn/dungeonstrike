using UnityEngine;
public static class LoggingHelper
{
	public static void LogValues<T>(T foo)
	{
		var props = foo.GetType().GetProperties();
		var sb = new System.Text.StringBuilder();
		foreach (var p in props)
		{
			var name = p.Name;
			sb.AppendLine(string.Format("{0}={1}", name, p.GetValue(foo, null)));
		}
		Debug.Log(sb.ToString());
	}
	public static void LogValues<T>(this MonoBehaviour ob, T foo)
	{
		var props = foo.GetType().GetProperties();
		var name = ob.GetType().Name;
		var sb = new System.Text.StringBuilder(name + " on " + ob.name + " properties log.\n");
		foreach (var p in props)
		{
			name = p.Name;
			sb.AppendLine(string.Format("{0}={1}", name, p.GetValue(foo, null)));
		}
		Debug.Log(sb.ToString(), ob);
	}
	public static void LogValues(this MonoBehaviour ob)
	{
		var type = ob.GetType();
		var name = type.Name;
		var sb = new System.Text.StringBuilder(name + " on " + ob.name + " properties log.\n");
		var props = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		foreach (var p in props)
		{
			name = p.Name;
			sb.AppendLine(string.Format("{0}={1}", name, p.GetValue(ob)));
		}
		Debug.Log(sb.ToString(), ob);
	}
}