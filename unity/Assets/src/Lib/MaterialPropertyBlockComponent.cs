using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osakana4242.Lib {
	[UnityEngine.ExecuteInEditMode]
	public class MaterialPropertyBlockComponent : MonoBehaviour {
		readonly int ColorID = Shader.PropertyToID("_Color");
		readonly int MainTexID = Shader.PropertyToID("_MainTex");
		readonly int MainTex_ST_ID = Shader.PropertyToID("_MainTex_ST");
		public Color32 color = new Color(1f, 1f, 1f, 1f);
		public Texture2D texture;
		public Vector4 textureST = new Vector4(1f, 1f, 0f, 0f);
		MaterialPropertyBlock block_;
		Renderer renderer_;
		public void Update() {
			if (block_ == null) {
				block_ = new MaterialPropertyBlock();
			}
			if (renderer_ == null) {
				renderer_ = GetComponent<Renderer>();
			}
			block_.SetColor(ColorID, color);
			if (texture != null) {
				block_.SetTexture(MainTexID, texture);
			}
			block_.SetVector(MainTex_ST_ID, textureST);
			renderer_.SetPropertyBlock(block_);
		}
	}
}

