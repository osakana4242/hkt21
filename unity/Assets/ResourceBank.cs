using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Osakana4242 {
	public class ResourceBank : ScriptableObject {
		public Object[] resourceList;
		Dictionary<string, Object> resourceDict_;

		public T Get<T>(string name) where T : Object {
			if ( resourceDict_ == null ) {
				var dict = new Dictionary<string, Object>(resourceList.Length);
				foreach (var item in resourceList) {
					if (item == null) continue;
					dict[item.name] = item;
				}
				resourceDict_ = dict;
			}
			Object o;
			if (!resourceDict_.TryGetValue(name, out o)) throw new System.ArgumentException("not found: '" + name + "'");
			return (T)o;
		}

#if UNITY_EDITOR

		[MenuItem("Osakana4242/Assets/Create/ResourceBank")]
		public static void Create() {
			ScriptalbeObjectUtil.Create<ResourceBank>();
		}
#endif
	}
}
