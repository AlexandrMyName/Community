using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client {
    sealed class EcsStartup : MonoBehaviour {
        EcsWorld _world;        
        IEcsSystems _systems;

        public List<MonoBehaviour> systemsOnScene;

        void Start() {
            
            _world = new EcsWorld();
            _systems = new EcsSystems(_world);
            systemsOnScene.ForEach(s => _systems.Add(s as IEcsSystem));
            // register your systems here, for example:
            // .Add (new TestSystem1 ())
            _systems.Add(GameObject.FindGameObjectWithTag("SceneLoader").GetComponent<SceneLoader>());
            _systems.DelHere<CharacterMessage>();            
            _systems.DelHere<MessageCameraInfo>();            
            _systems.DelHere<SceneSettings>();            
            _systems.DelHere<CamerasSettings>();            

            // register additional worlds here, for example:
            // .AddWorld (new EcsWorld (), "events")
#if UNITY_EDITOR
            // add debug systems for custom worlds here, for example:
            // .Add (new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem ("events"))
            _systems.Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem());
#endif
            _systems.Init ();
        }

        void Update () {
            // process systems here.
            _systems?.Run ();
        }

        void OnDestroy () {
            if (_systems != null) {
                // list of custom worlds will be cleared
                // during IEcsSystems.Destroy(). so, you
                // need to save it here if you need.
                _systems.Destroy ();
                _systems = null;
            }
            
            // cleanup custom worlds here.
            
            // cleanup default world.
            if (_world != null) {
                _world.Destroy ();
                _world = null;
            }
        }
    }
}