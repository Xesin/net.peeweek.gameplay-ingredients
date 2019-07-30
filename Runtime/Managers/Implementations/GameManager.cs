using GameplayIngredients.Actions;
using GameplayIngredients.LevelStreaming;
using NaughtyAttributes;
using System.Linq;
using UnityEngine;

namespace GameplayIngredients
{
    [ManagerDefaultPrefab("GameManager")]
    public class GameManager : Manager
    {
        [Header("Events")]
        [ReorderableList]
        public Callable[] OnGameStart;
        [ReorderableList]
        public Callable[] OnLevelLoaded;
        [ReorderableList]
        public Callable[] OnMainMenuLoaded;

        [Header("Levels"), NonNullCheck]
        public GameLevel MainMenuGameLevel;
        [ReorderableList, NonNullCheck]
        public GameLevel[] MainGameLevels;

        [Header("Save")]
        public string ProgressSaveName = "Progress";

        [Header("Messages")]
        public string MainMenuStartMessage = "MAINMENU_START";
        public string GameLevelStartMessage = "GAME_START";

        public int currentLevel { get; private set; } = -2;

        public int currentSaveProgress
        {
            get { Manager.Get<GameSaveManager>().LoadUserSave(0); return Manager.Get<GameSaveManager>().GetInt(ProgressSaveName, GameSaveManager.Location.User); }
            set { Manager.Get<GameSaveManager>().SetInt(ProgressSaveName, GameSaveManager.Location.User, value); Manager.Get<GameSaveManager>().SaveUserSave(0); }
        }

        GameObject m_CurrentLevelSwitch;

        public void Start()
        {
            currentLevel = int.MinValue;
            Callable.Call(OnGameStart);
            Manager.Get<GameSaveManager>().LoadUserSave(0);
        }

        Callable GetCurrentLevelSwitch(int targetLevel, bool showUI = false, Callable[] onComplete = null)
        {
            GameObject go = new GameObject();
            go.name = $"LevelSwtich {currentLevel} -> {targetLevel}";
            go.transform.parent = this.transform;
            m_CurrentLevelSwitch = go;

            var cameraFade = go.AddComponent<FullScreenFadeAction>();
            var loadLevel = go.AddComponent<StreamingLevelAction>();
            var sendMessage = go.AddComponent<SendMessageAction>();
            var destroy = go.AddComponent<DestroyObjectAction>();
            var next = go.AddComponent<Logic.Logic>();

            cameraFade.Fading = FullScreenFadeManager.FadeMode.ToBlack;
            cameraFade.Name = "Fade to Black";
            cameraFade.Duration = 1.0f;
            cameraFade.OnComplete = new Callable[] { loadLevel };

            loadLevel.Name = $"Load {(targetLevel < 0 ? "Main menu" : MainGameLevels[targetLevel].name)}";
            loadLevel.ShowUI = showUI;
            loadLevel.Action = LevelStreamingManager.StreamingAction.Replace;
            var level = targetLevel < 0 ? MainMenuGameLevel : MainGameLevels[targetLevel];
            loadLevel.SceneToActivate = level.StartupScenes[0];
            loadLevel.Scenes = level.StartupScenes;
            loadLevel.OnLoadComplete = new Callable[] { sendMessage, destroy, next };

            string message = targetLevel == -1 ? MainMenuStartMessage : GameLevelStartMessage;
            sendMessage.Name = $"Send {message}";
            sendMessage.MessageToSend = message;

            destroy.ObjectsToDestroy = new GameObject[] { go };

            var nextActions = targetLevel < 0 ? OnMainMenuLoaded : OnLevelLoaded;
            next.Calls = nextActions.Concat(onComplete).ToArray();

            // Return first callable
            return cameraFade;
        }

        public void SwitchLevel(int nextLevel, bool showUI = false, Callable[] onComplete = null)
        {
            if (m_CurrentLevelSwitch == null)
            {
                var call = GetCurrentLevelSwitch(nextLevel, showUI, onComplete);
                call.Execute();
                currentLevel = nextLevel;
            }
            else
                Debug.LogWarning("SwitchLevel : an Operation was still in progress and switching level could not be done. ");
        }
    }
}
