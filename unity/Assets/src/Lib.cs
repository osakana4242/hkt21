using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osakana4242 {
	static class MathUtil {
		public static bool isIn(int a, int min, int max) {
			return min <= a && a < max;
		}
		public static bool isIn(float a, float min, float max) {
			return min <= a && a <= max;
		}
	}
	public static class ScriptalbeObjectUtil {
#if UNITY_EDITOR
		public static void Create<T>() where T : ScriptableObject {
			var instance = ScriptableObject.CreateInstance<ResourceBank>();
			var t = typeof(T);
			UnityEditor.AssetDatabase.CreateAsset(instance, string.Format("Assets/{0}.asset", t.Name));
		}
#endif
	}
	[System.Serializable]
	public class WaveData {
		public BlockData[] blocks;
	}

	[System.Serializable]
	public class BlockData {
		public float[] items;
	}
}
