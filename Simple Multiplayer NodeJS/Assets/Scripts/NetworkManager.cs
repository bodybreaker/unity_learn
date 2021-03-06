using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using Defective.JSON;
using socket.io;
using UnityEngine.Video;


public class NetworkManager : MonoBehaviour {

	public static NetworkManager instance;
	public Canvas canvas;

	public string ServerURL="http://localhost:3000";
    public Socket socket;
	public InputField playerNameInput;
	public GameObject player;

	public VideoPlayer videoPlayer;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);
		DontDestroyOnLoad(gameObject);
	}

	// Use this for initialization
	void Start () {
		// subscribe to all the various websocket events
		socket = Socket.Connect(ServerURL);
        socket.On(SystemEvents.connect,()=>{
           // 1
           Debug.Log("소켓 접속 원료"); 
        });
		
		socket.On("chat",(string data)=>{

			ChatJSON chatJSON = ChatJSON.CreateFromJSON(data);
			//  채팅 발신 캐릭터명이 현재 캐릭터명인 경우
			if (chatJSON.name == playerNameInput.text)
			{
				// 현재 캐릭터(본인)의 메시지박스에 메시지 표시
				MessageBoxController myMc = GameObject.Find(playerNameInput.text).transform.Find("Healthbar Canvas").transform.Find("Message Canvas").GetComponent<MessageBoxController>();
				myMc.ShowMessage(chatJSON.text);
				return;
			}
			// 다른사람으로부터 온 메시지일 경우 현재 캐릭터의 채팅창에 글자 표시
			GameObject p = GameObject.Find(playerNameInput.text);

			if (p != null)
			{
				Transform chatCanvas = p.transform.Find("Chat Canvas");
				ChatController cc = chatCanvas.GetComponent<ChatController>();
				cc.addMessage(chatJSON.name+": "+chatJSON.text+"\n");
				Debug.Log("Chat from >> "+chatJSON.name+": "+chatJSON.text);
			}

			MessageBoxController otherMc = GameObject.Find(chatJSON.name).transform.Find("Healthbar Canvas").transform.Find("Message Canvas").GetComponent<MessageBoxController>();
			otherMc.ShowMessage(chatJSON.text);


			// 캐릭터 위 메시지박스 


		});
		// url 영상
		socket.On("video-play",(string data)=>{

			URLJSON urlJSON = URLJSON.CreateFromJSON(data);
			Debug.Log("video-play >> "+urlJSON.name+": "+urlJSON.url);
			if (urlJSON.name != playerNameInput.text)
			{	
				// 다른사람으로부터 영상재생 요청이 온경우 채팅창에 표시
				GameObject p = GameObject.Find(playerNameInput.text);
				if (p != null)
				{
					Transform chatCanvas = p.transform.Find("Chat Canvas");
					ChatController cc = chatCanvas.GetComponent<ChatController>();
					cc.addMessage(urlJSON.name+" 님이 영상을 재생하였습니다.\n");

				}
			}
			videoPlayer.url = urlJSON.url;
			videoPlayer.Play();			
		});

		socket.On("enemies", (string data)=>{
			EnemiesJSON enemiesJSON = EnemiesJSON.CreateFromJSON(data);
			EnemySpawner es = GetComponent<EnemySpawner>();
			es.SpawnEnemies(enemiesJSON);
        });
		socket.On("other player connected", (string data)=>{
			print("Someone else joined");
			UserJSON userJSON = UserJSON.CreateFromJSON(data);
			Vector3 position = new Vector3(userJSON.position[0], userJSON.position[1], userJSON.position[2]);
			Quaternion rotation = Quaternion.Euler(userJSON.rotation[0], userJSON.rotation[1], userJSON.rotation[2]);
			GameObject o = GameObject.Find(userJSON.name) as GameObject;
			if (o != null)
			{
				return;
			}
			GameObject p = Instantiate(player, position, rotation) as GameObject;
			// here we are setting up their other fields name and if they are local
			PlayerController pc = p.GetComponent<PlayerController>();
			Transform t = p.transform.Find("Healthbar Canvas");
			Transform t1 = t.transform.Find("Player Name");
			Text playerName = t1.GetComponent<Text>();
			playerName.text = userJSON.name;
			pc.isLocalPlayer = false;
			p.name = userJSON.name;
			// we also need to set the health
			Health h = p.GetComponent<Health>();
			h.currentHealth = userJSON.health;
			h.OnChangeHealth();
        });
		socket.On("play", (string data)=>{
			print("you joined");
			UserJSON currentUserJSON = UserJSON.CreateFromJSON(data);
			Vector3 position = new Vector3(currentUserJSON.position[0], currentUserJSON.position[1], currentUserJSON.position[2]);
			Quaternion rotation = Quaternion.Euler(currentUserJSON.rotation[0], currentUserJSON.rotation[1], currentUserJSON.rotation[2]);
			GameObject p = Instantiate(player, position, rotation) as GameObject;
			PlayerController pc = p.GetComponent<PlayerController>();
			Transform t = p.transform.Find("Healthbar Canvas");
			Transform t1 = t.transform.Find("Player Name");
			Text playerName = t1.GetComponent<Text>();
			playerName.text = currentUserJSON.name;
			pc.isLocalPlayer = true;
			p.name = currentUserJSON.name;

        });
		socket.On("player move", (string data)=>{
			UserJSON userJSON = UserJSON.CreateFromJSON(data);
			Vector3 position = new Vector3(userJSON.position[0], userJSON.position[1], userJSON.position[2]);
			//  현재 사용자일 경우
			if (userJSON.name == playerNameInput.text)
			{
				return;
			}
			GameObject p = GameObject.Find(userJSON.name) as GameObject;
			if (p != null)
			{
				p.transform.position = position;
				//Debug.Log("other player move>>"+p.transform.position);
				//PlayerController pc = p.GetComponent<PlayerController>();
				//pc.Animate(true);
			}
        });
		socket.On("player turn", (string data)=>{
			UserJSON userJSON = UserJSON.CreateFromJSON(data);
			Quaternion rotation = Quaternion.Euler(userJSON.rotation[0], userJSON.rotation[1], userJSON.rotation[2]);
			// if it is the current player exit
			if (userJSON.name == playerNameInput.text)
			{
				return;
			}
			GameObject p = GameObject.Find(userJSON.name) as GameObject;
			if (p != null)
			{
				p.transform.rotation = rotation;
			}
        });
		socket.On("player shoot", (string data)=>{
			ShootJSON shootJSON = ShootJSON.CreateFromJSON(data);
			//find the gameobject
			GameObject p = GameObject.Find(shootJSON.name);
			// instantiate the bullet etc from the player script
			PlayerController pc = p.GetComponent<PlayerController>();
			pc.CmdFire();

        });

		socket.On("player shake", (string data)=>{
			ShakeJson shakeJson = ShakeJson.CreateFromJSON(data);
			//find the gameobject
			GameObject p = GameObject.Find(shakeJson.name);
			// instantiate the bullet etc from the player script
			PlayerController pc = p.GetComponent<PlayerController>();
			pc.CmdShake();

        });
		socket.On("health", (string data)=>{
			print("changing the health");
			// get the name of the player whose health changed
			UserHealthJSON userHealthJSON = UserHealthJSON.CreateFromJSON(data);
			GameObject p = GameObject.Find(userHealthJSON.name);
			Health h = p.GetComponent<Health>();
			h.currentHealth = userHealthJSON.health;
			h.OnChangeHealth();

        });
		socket.On("other player disconnected", (string data)=>{
			print("user disconnected");
			UserJSON userJSON = UserJSON.CreateFromJSON(data);
			Destroy(GameObject.Find(userJSON.name));

        });
	}

	public void JoinGame()
	{
		StartCoroutine(ConnectToServer());
	}

	#region Commands

	IEnumerator ConnectToServer()
	{
		yield return new WaitForSeconds(0.5f);

		socket.Emit("player connect");

		yield return new WaitForSeconds(1f);

		string playerName = playerNameInput.text;
		List<SpawnPoint> playerSpawnPoints = GetComponent<PlayerSpawner>().playerSpawnPoints;
		List<SpawnPoint> enemySpawnPoints = GetComponent<EnemySpawner>().enemySpawnPoints;
		PlayerJSON playerJSON = new PlayerJSON(playerName, playerSpawnPoints, enemySpawnPoints);
		string data = JsonUtility.ToJson(playerJSON);
		socket.EmitJson("play", (new JSONObject(data)).ToString());
		canvas.gameObject.SetActive(false);
	}

	public void CommandMove(Vector3 vec3)
	{
		string data = JsonUtility.ToJson(new PositionJSON(vec3));
		socket.EmitJson("player move", (new JSONObject(data)).ToString());
	}

	public void CommandTurn(Quaternion quat)
	{
		string data = JsonUtility.ToJson(new RotationJSON(quat));
		socket.EmitJson("player turn", (new JSONObject(data)).ToString());
	}

	public void CommandShoot()
	{
		print("Shoot");
		socket.Emit("player shoot");
	}

	public void CommandShake()
	{
		print("Shake");
		socket.Emit("player shake");
	}

	public void CommandHealthChange(GameObject playerFrom, GameObject playerTo, int healthChange, bool isEnemy)
	{
		print("health change cmd");
		HealthChangeJSON healthChangeJSON = new HealthChangeJSON(playerTo.name, healthChange, playerFrom.name, isEnemy);
		socket.EmitJson("health", (new JSONObject(JsonUtility.ToJson(healthChangeJSON))).ToString());
	}

	public void CommandChatMessage(string text){
		ChatJSON chatJSON = new ChatJSON(text);
		socket.EmitJson("chat",(new JSONObject(JsonUtility.ToJson(chatJSON))).ToString());
	}

	public void CommandPlayURL(string url)
	{
		print("Play URL");
		URLJSON urlJSON = new URLJSON(url);
		socket.EmitJson("video-url",(new JSONObject(JsonUtility.ToJson(urlJSON))).ToString());
	}

	#endregion

	#region JSONMessageClasses

	[Serializable]

	public class URLJSON
	{
		public string url;
		public string name;

		public URLJSON(string _url){
			url = _url;
		}
		public static URLJSON CreateFromJSON(string data)
		{
			return JsonUtility.FromJson<URLJSON>(data);
		}
	}
	[Serializable]
	public class PlayerJSON
	{
		public string name;
		public List<PointJSON> playerSpawnPoints;
		public List<PointJSON> enemySpawnPoints;

		public PlayerJSON(string _name, List<SpawnPoint> _playerSpawnPoints, List<SpawnPoint> _enemySpawnPoints)
		{
			playerSpawnPoints = new List<PointJSON>();
			enemySpawnPoints = new List<PointJSON>();
			name = _name;
			foreach (SpawnPoint playerSpawnPoint in _playerSpawnPoints)
			{
				PointJSON pointJSON = new PointJSON(playerSpawnPoint);
				playerSpawnPoints.Add(pointJSON);
			}
			foreach (SpawnPoint enemySpawnPoint in _enemySpawnPoints)
			{
				PointJSON pointJSON = new PointJSON(enemySpawnPoint);
				enemySpawnPoints.Add(pointJSON);
			}
		}
	}

	[Serializable]
	public class PointJSON
	{
		public float[] position;
		public float[] rotation;
		public PointJSON(SpawnPoint spawnPoint)
		{
			position = new float[] {
				spawnPoint.transform.position.x,
				spawnPoint.transform.position.y,
				spawnPoint.transform.position.z
			};
			rotation = new float[] {
				spawnPoint.transform.eulerAngles.x,
				spawnPoint.transform.eulerAngles.y,
				spawnPoint.transform.eulerAngles.z
			};
		}
	}

	[Serializable]
	public class PositionJSON
	{
		public float[] position;

		public PositionJSON(Vector3 _position)
		{
			position = new float[] { _position.x, _position.y, _position.z };
		}
	}

	[Serializable]
	public class RotationJSON
	{
		public float[] rotation;

		public RotationJSON(Quaternion _rotation)
		{
			rotation = new float[] { _rotation.eulerAngles.x,
				_rotation.eulerAngles.y, 
				_rotation.eulerAngles.z };
		}
	}

	[Serializable]
	public class UserJSON
	{
		public string name;
		public float[] position;
		public float[] rotation;
		public int health;

		public static UserJSON CreateFromJSON(string data)
		{
			return JsonUtility.FromJson<UserJSON>(data);
		}
	}
	[Serializable]
	public class ChatJSON{
		public string name;
		public string text;
		public ChatJSON(string _text){
			text = _text;
		}
		public static ChatJSON CreateFromJSON(string data)
		{
			return JsonUtility.FromJson<ChatJSON>(data);
		}
	}

	[Serializable]
	public class HealthChangeJSON
	{
		public string name;
		public int healthChange;
		public string from;
		public bool isEnemy;

		public HealthChangeJSON(string _name, int _healthChange, string _from, bool _isEnemy)
		{
			name = _name;
			healthChange = _healthChange;
			from = _from;
			isEnemy = _isEnemy;
		}
	}

	[Serializable]
	public class EnemiesJSON
	{
		public List<UserJSON> enemies;

		public static EnemiesJSON CreateFromJSON(string data)
		{
			return JsonUtility.FromJson<EnemiesJSON>(data);
		}
	}

	[Serializable]
	public class ShootJSON
	{
		public string name;

		public static ShootJSON CreateFromJSON(string data)
		{
			return JsonUtility.FromJson<ShootJSON>(data);
		}
	}

	[Serializable]
	public class ShakeJson
	{
		public string name;

		public static ShakeJson CreateFromJSON(string data)
		{
			return JsonUtility.FromJson<ShakeJson>(data);
		}
	}

	[Serializable]
	public class UserHealthJSON
	{
		public string name;
		public int health;

		public static UserHealthJSON CreateFromJSON(string data)
		{
			return JsonUtility.FromJson<UserHealthJSON>(data);
		}
	}

	#endregion
}
