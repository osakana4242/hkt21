using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Osakana4242 {
	public sealed class MainPart : MonoBehaviour {
		public Data data;

		// 操作
		// 左右によける、前に進む.
		// 左右にかき分ける

		// 人にぶつかると後ろに戻される

		// スタート
		// 
		// 終了

		public TMPro.TextMeshProUGUI progressTextUI;
		public TMPro.TextMeshProUGUI centerTextUI;

		StateMachine<MainPart> sm_;
		List<MyObject> objectList_;
		public ResourceBank resource;
		public GameObject cameraGo;
		[SerializeField]
		public GameObject dustStart;
		public int playerId;
		public WaveData waveData;
		public float startWaveZ_ = 3f;
		public int blockI_;
		public int waveI_;
		public Vector3 startPosition = new Vector3(-1, 0f, -1f);
		public Vector3[] stonePositions = {
			new Vector3(-15f, 0f, 15f),
			new Vector3(15f, 0f, 15f),
			new Vector3(-15f, 0f, 0f),
			new Vector3(15f, 0f, 0f),
		};

		public sealed class MyObject : MonoBehaviour {
			public bool hasDestroy;
			public int id;
			public string category;
			public Player player;
			public Stone stone;
			public float time;

			public void Destroy() {
				hasDestroy = true;
			}
		}

		public struct CollisionInfo {
			public Collider collider;
			public Collision collision;
		}

		public class CollilsionObserver : MonoBehaviour {
			public System.Action<CollisionInfo> onEvent;
			public void OnDestroy() {
				onEvent = null;
			}
			public void OnTriggerEnter(Collider collider) {
				if (onEvent == null) return;
				onEvent(new CollisionInfo() {
					collider = collider,
				});
			}
			public void OnCollisionEnter(Collision collision) {
				if (onEvent == null) return;
				onEvent(new CollisionInfo() {
					collision = collision,
				});
			}
		}

		public sealed class Player {
			public int score;
		}

		public sealed class Stone {
			public int score = 1;
			public Vector3 startPosition;
			public Vector3 targetPosition;
			public float duration = 2f;
			public bool hasHit;
		}

		void Awake() {
			sm_ = new StateMachine<MainPart>(stateInit_g_);
			objectList_ = new List<MyObject>();
			Application.logMessageReceived += OnLog;
		}
		public void OnLog(string condition, string stackTrace, LogType type) {
			switch (type) {
				case LogType.Exception:
				Debug.Break();
				GameObject.Destroy(gameObject);
				Application.Quit();
				break;
			}
		}

		void OnDestroy() {
			Application.logMessageReceived -= OnLog;
			sm_ = null;
			objectList_ = null;
		}

		float[] dustDataList = {
			2f, 0.5f, -0.1f,
			5f, 0.5f, -0.3f,
			6f, 0.5f, -0.3f,

			9.0f + 0.0f, -0.1f, -0.5f,
			9.0f + 0.2f, -0.1f, -0.5f,
			9.0f + 0.4f, -0.1f, -0.5f,
			9.0f + 0.6f, -0.1f, -0.5f,

			10.0f + 0.0f, 1f, -0.0f,

			11.0f + 0.0f, 1f, -0.2f,
			11.0f + 0.2f, 1f, -0.3f,
			11.0f + 0.4f, 1f, -0.4f,

			13.0f + 0.0f, 0.8f, 0.1f,

			15.0f + 0.0f, 0.0f, 0.0f,
			15.0f + 1.0f, -0.2f, 0.0f,
			15.0f + 2.0f, 0.2f, 0.0f,
			15.0f + 3.0f, -0.2f, -0.2f,

			21.0f + 0.0f, 0.7f, 0.6f,
			21.0f + 0.5f, 0.7f, 0.4f,
			21.0f + 1.0f, 0.7f, 0.2f,
			21.0f + 1.5f, 0.7f, 0.0f,
			21.0f + 2.0f, 0.7f, -0.2f,

			25.0f + 0.0f, -0.5f, -0.1f,
			25.0f + 0.5f, -0.2f, -0.1f,
			25.0f + 1.0f, 0.1f, -0.1f,
			25.0f + 1.5f, 0.4f, -0.1f,
			25.0f + 2.0f, 0.7f, -0.1f,
		};
		public int dustIndex = 0;
		public float dustElapsedTime = 0f;

		void FixedUpdate() {
			if (data.isPlaying) {
				var dustOffset = dustIndex * 3;
				var dustTime = dustDataList[dustOffset + 0];
				var dustX = dustDataList[dustOffset + 1];
				var dustZ = dustDataList[dustOffset + 2];
				if (dustTime <= dustElapsedTime) {
					var prefab = resource.Get<GameObject>("dust");
					var startPos = dustStart.transform.position;
					var rot = Quaternion.Euler(
						Random.Range(-1f, 1f) * 180f,
						Random.Range(-1f, 1f) * 180f,
						Random.Range(-1f, 1f) * 180f
					);
					var go = Object.Instantiate(prefab, startPos, rot, transform);
					var obj = go.AddComponent<MyObject>();
					obj.id = CreateObjectId();
					obj.category = "dust";
					obj.stone = new Stone();
					obj.stone.startPosition = startPos;
					obj.stone.targetPosition = new Vector3(dustX, 0.1f, dustZ);
					obj.gameObject.AddComponent<CollilsionObserver>().onEvent = (_evt) => {
						if (obj.stone == null) return;
						if (_evt.collider == null) {
							obj.stone.hasHit = true;
							return;
						}
						GetPlayer().player.score += 1;
						obj.Destroy();
						var effPrefab = resource.Get<GameObject>("eff_dust_in");
						var eff = Object.Instantiate(effPrefab, obj.transform.position + Vector3.up * 0.3f, Quaternion.identity, transform);
						Object.Destroy(eff, 1f);
					};
					objectList_.Add(obj);

					{
						var effPrefab = resource.Get<GameObject>("eff_target_mark");
						var eff = Object.Instantiate(effPrefab, obj.stone.targetPosition, Quaternion.identity, transform);
						Object.Destroy(eff, 1f);
					}

					dustIndex++;
					if (dustDataList.Length / 3 <= dustIndex) {
						dustIndex = 0;
						dustElapsedTime = 0f;
					}
				}
			}
			dustElapsedTime += Time.deltaTime;



			for (var i = objectList_.Count - 1; 0 <= i; i--) {
				var obj = objectList_[i];
				{
					var player = obj.player;
					if (player != null) {
						UpdatePlayer(obj, player);
					}
				}
				{
					var stone = obj.stone;
					if (stone != null) {
						UpdateStone(obj, stone);
					}
				}
				obj.time += Time.deltaTime;
			}


			for (var i = objectList_.Count - 1; 0 <= i; i--) {
				var obj = objectList_[i];
				if (!obj.hasDestroy) continue;
				objectList_.RemoveAt(i);
				GameObject.Destroy(obj.gameObject);
			}

			if (data.isPlaying) {
				data.time += Time.deltaTime;
			}
		}

		void UpdatePlayer(MyObject obj, Player player) {
			var rb = obj.GetComponent<Rigidbody>();
			var v = rb.velocity;
			// if (0f < v.y) {
			// 	v.y = 0f;
			// 	rb.velocity = v;
			// }
			// var pos = rb.position;
			// if (0f < pos.y) {
			// 	pos.y = 0f;
			// 	rb.position = pos;
			// }
		}

		public Vector3 easingJump(Vector3 a, Vector3 b, float height, float t) {
			var pos = Vector3.Lerp(a, b, t);
			var y = Mathf.Sin(Mathf.PI * t) * height;
			pos.y += y;
			return pos;
		}

		void UpdateStone(MyObject obj, Stone stone) {
			var rb = obj.GetComponent<Rigidbody>();
			if (!rb.isKinematic) return;
			var t = Mathf.Clamp01(obj.time / stone.duration);
			var pos = easingJump(stone.startPosition, stone.targetPosition, 1f, t);
			var delta = pos - rb.position;
			if (1f <= t || obj.stone.hasHit) {
				rb.velocity = delta;
				rb.isKinematic = false;
			} else {
				rb.position += delta;
			}
		}

		void Update() {
			sm_.Update(this);
		}

		public MyObject FindObjectById(int id) {
			foreach (var item in objectList_) {
				if (item.id == id) return item;
			}
			return null;
		}

		public MyObject GetPlayer() {
			return FindObjectById(playerId);
		}
		int autoincrement;
		public int CreateObjectId() {
			return ++autoincrement;
		}


		static StateMachine<MainPart>.StateFunc stateExit_g_ = (_evt) => {
			var self = _evt.owner;
			switch (_evt.type) {
				case StateMachineEventType.Enter: {
						self.data.isPlaying = false;
						UnityEngine.SceneManagement.SceneManager.LoadScene("main");
						return null;
					}
				default:
				return null;
			}
		};

		static StateMachine<MainPart>.StateFunc stateInit_g_ = (_evt) => {
			switch (_evt.type) {
				case StateMachineEventType.Enter: {
						var self = _evt.owner;
						self.progressTextUI.text = "";
						self.centerTextUI.text = "READY";

						{
							var waveJson = self.resource.Get<TextAsset>("wave");
							self.waveData = JsonUtility.FromJson<WaveData>(waveJson.text);
						}

						{
							var prefab = self.resource.Get<GameObject>("player");
							var go = GameObject.Instantiate(prefab, self.startPosition, Quaternion.identity, self.transform);
							var obj = go.AddComponent<MyObject>();
							obj.id = self.CreateObjectId();
							obj.category = "player";
							obj.player = new Player();
							self.objectList_.Add(obj);
							self.playerId = obj.id;
							var camera = self.cameraGo.GetComponent<CameraController>();
							camera.target = go;
						}

						{
							Random.InitState(1);
						}
						return null;
					}
				case StateMachineEventType.Update: {
						if (1f <= _evt.sm.time) {
							return stateMain_g_;
						}
						return null;
					}

				default:
				return null;
			}
		};

		static StateMachine<MainPart>.StateFunc stateMain_g_ = (_evt) => {
			var self = _evt.owner;
			// self.StepWave();
			self.data.isPlaying = true;

			var player = self.GetPlayer();

			{
				var sb = new System.Text.StringBuilder();
				sb.AppendFormat("SCORE: {0:F0}\n", player.player.score);
				sb.AppendFormat("TIME: {0:F2}\n", self.data.RestTime);
				self.progressTextUI.text = sb.ToString();
			}
			{
				self.centerTextUI.text = "";
			}

			var playerPos = player.transform.position;
			self.data.distance = Mathf.Max(self.data.distance, playerPos.z);

			var z = playerPos.z - 20;

			var hasTimeOver = self.data.RestTime <= 0f;
			if (hasTimeOver) {
				return stateTimeOver_g_;
			}

			var isFall = playerPos.y < -5;
			if (isFall) {
				return stateFall_g_;
			}

			if (Input.GetKeyDown(KeyCode.R)) {
				return stateExit_g_;
			}

			return null;
		};

		/** タイムオーバー */
		static StateMachine<MainPart>.StateFunc stateTimeOver_g_ = (_evt) => {
			var self = _evt.owner;
			switch (_evt.type) {
				case StateMachineEventType.Enter:
				self.centerTextUI.text = "TIME OVER";
				self.data.isPlaying = false;
				break;
			}

			if (3f <= _evt.sm.time) {
				return stateResult_g_;
			}

			return null;
		};

		/** 落下 */
		static StateMachine<MainPart>.StateFunc stateFall_g_ = (_evt) => {
			var self = _evt.owner;
			switch (_evt.type) {
				case StateMachineEventType.Enter:
				self.centerTextUI.text = "FALL";
				self.data.isPlaying = false;
				self.cameraGo.GetComponent<CameraController>().target = null;
				break;
			}

			if (3f <= _evt.sm.time) {
				return stateResult_g_;
			}

			return null;
		};

		static StateMachine<MainPart>.StateFunc stateResult_g_ = (_evt) => {
			var self = _evt.owner;
			switch (_evt.type) {
				case StateMachineEventType.Enter:
				self.centerTextUI.text = "PRESS Z KEY";
				self.data.isPlaying = false;
				self.cameraGo.GetComponent<CameraController>().target = null;
				break;
			}

			if (Input.GetKeyDown(KeyCode.Z)) {
				return stateExit_g_;
			}

			return null;
		};

		[System.Serializable]
		public class Data {
			public bool isPlaying;
			/** 経過時間 */
			public float time;
			/** 制限時間 */
			public float duration = 90f;
			/** 走行距離 */
			public float distance;
			public float speed;
			public float speedMax;
			public float RestTime => Mathf.Max(0f, duration - time);
		}

	}
}
