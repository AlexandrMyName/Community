using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Client
{

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(HeadController))]
    public class HeadControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {

            HeadController myScript = (HeadController)target;
            if (GUILayout.Button("Generate look"))
            {
                myScript.IdleLooking();
            }
            DrawDefaultInspector();
        }
    }
#endif
    [RequireComponent(typeof(Animator))]

	public class HeadController : MonoBehaviour
	{
        #region Поля для настройки Контроллера в целом
        [SerializeField] Animator animator;
		[SerializeField] bool ikActive = true;
		[SerializeField] float lookWeight = 0.65f;
		[SerializeField] Transform messageViewedObject = null;
		[SerializeField] Transform systemViewedAnchor = null;
		[SerializeField] float delayForLookAway = 0.3f;
        #endregion
        #region Приватные поля
        private string keyBone = "mixamorig:Head";
        private float bodyWeight = 0.15f;
		private float headWeight = 0.4f;
		private float eyesWeight = 0.9f;
        //private float clampWeight = 0.9f;        
        #endregion
        #region Метод для наведения взгляда у модели на просматриваемый объект
        void OnAnimatorIK()
		{
			if (animator)
			{
				if (ikActive)
				{
					if (systemViewedAnchor != null)
					{
						animator.SetLookAtWeight(lookWeight, bodyWeight, headWeight, eyesWeight);

						animator.SetLookAtPosition(systemViewedAnchor.position);
					}
				}
				else
				{
					animator.SetLookAtWeight(0);
				}
			}
        }
        #endregion
        private void Start()
        {
            IdleLooking();
        }

        [Header("Idle Points Settings")]
        [SerializeField] Light _viewField;
        [SerializeField] float radiusViewField = 0.1f;

        [SerializeField] float changingSpeed_MetersPerSecond = 0.1f;
        [SerializeField] float changingSpeed_ViewRadiusPerSecond = 0.5f;
        [SerializeField] bool use_ViewRadiusPerSecond = true;
        [SerializeField] float minDelayForIdlePoints = 0.6f;
        [SerializeField] float maxDelayForIdlePoints = 0.6f;
        [SerializeField] int minNumberPoints = 3;
        [SerializeField] int maxNumberPoints = 5;
        #region сменение взгляда через Idle Points
        Queue<Vector3> lookingPoints = new Queue<Vector3>();
        public void IdleLooking() {            

            if (lookingPoints.Count > 0) {
                NextPoint(); 
                return; 
            }
			if (_viewField == null) return;

            int numberPoints = UnityEngine.Random.Range(minNumberPoints, maxNumberPoints);
            Vector3? last = null, current = null;
            for(int i = 0; i < numberPoints; i++) {
                current = generateRandomVectorToViewField();
                if (last.HasValue) {
                    Debug.DrawLine(last.Value,current.Value, Color.white, 10);
                }
                
                lookingPoints.Enqueue(current.Value);
                last = current;
            }

            if (use_ViewRadiusPerSecond) changingSpeed_MetersPerSecond = radiusViewField * changingSpeed_ViewRadiusPerSecond;
            NextPoint();
        }
        private void NextPoint() {
            if (LookingProcess != null) StopCoroutine(LookingProcess);
            var point = lookingPoints.Dequeue();

            Debug.DrawLine(
                systemViewedAnchor.position,
                point, 
                Color.white,
                Vector3.Distance(point, systemViewedAnchor.position) / changingSpeed_MetersPerSecond
            );

            LookingProcess = LookingTo(
                systemViewedAnchor.position, 
                point, 
                Vector3.Distance(point, systemViewedAnchor.position) / changingSpeed_MetersPerSecond,
                UnityEngine.Random.Range(minDelayForIdlePoints, maxDelayForIdlePoints)
            );
            StartCoroutine(LookingProcess);
        }
        #endregion
        #region MessageLooking
        public void LookWith(CharacterMessage message, AudioClip clip)
        {
			//message.lookPosition not implemented
			if (LookingProcess != null) StopCoroutine(LookingProcess);
			LookingProcess = LookingTo(systemViewedAnchor.position, messageViewedObject.position, delayForLookAway, clip.length + 0.4f);
            StartCoroutine(LookingProcess);
        }
        #endregion
        #region Coroutine и плавные перевод взгляда от точки к точке
        IEnumerator LookingProcess;
		IEnumerator LookingTo(Vector3 from, Vector3 to, float delay, float delayAfter = 0.0f)
		{
			float timer = 0.0f;
			//Vector3.Distance(from,to) / delay
			while ((to - systemViewedAnchor.position).magnitude > 0.02f) {
                systemViewedAnchor.position = Vector3.Lerp(from, to, timer / delay);
				yield return new WaitForEndOfFrame();
				timer += Time.deltaTime;
            }
            if(delayAfter > 0.001f) yield return new WaitForSeconds(delayAfter);
			LookingProcess = null;
            IdleLooking();
        }
        #endregion
        #region CalculatingMethods
        private Vector3 rotateToViewField(Vector3 v) => _viewField.transform.position + _viewField.transform.rotation * v;
        private Vector3 circularClamp(float x, float y) {
            var dir = new Vector2(x, y);
            Vector3 v = dir.magnitude > radiusViewField ? dir / dir.magnitude * radiusViewField : dir;
            v.z = _viewField.range;
            return v;
        }
        private Vector3 generateRandomVectorToViewField() => rotateToViewField(circularClamp(
            UnityEngine.Random.Range(-radiusViewField, radiusViewField), 
            UnityEngine.Random.Range(-radiusViewField, radiusViewField)
            ));
        #endregion
        #region Drawing ViewField Zone
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            if (_viewField == null) return;
            radiusViewField = Mathf.Tan((_viewField.spotAngle / 2) * Mathf.Deg2Rad) * _viewField.range;

            float shag = Mathf.Clamp(20.0f / radiusViewField, 3.0f, 10.0f);
            Vector3 v1;
            Vector3 v2;

            for (float degree = 0.0f; degree <= 360; degree += shag)
            {
                v1 = new Vector3(
                    Mathf.Cos(Mathf.Deg2Rad * (degree - shag)) * radiusViewField,
                    Mathf.Sin(Mathf.Deg2Rad * (degree - shag)) * radiusViewField,
                    _viewField.range);
                v2 = new Vector3(
                    Mathf.Cos(Mathf.Deg2Rad * degree) * radiusViewField,
                    Mathf.Sin(Mathf.Deg2Rad * degree) * radiusViewField,
                    _viewField.range);
                Gizmos.DrawLine(rotateToViewField(v1), rotateToViewField(v2));
            }
        }
        #endregion
        #region OnValidate and Reset
#if UNITY_EDITOR 
        private void Reset() => OnValidate();
        private void OnValidate() => UnityEditor.EditorApplication.delayCall += _OnValidate;

        private void _OnValidate()
        {
            UnityEditor.EditorApplication.delayCall -= _OnValidate;
            if (this == null) return;

            animator = GetComponent<Animator>();
            if (_viewField == null)
            {
                var eyesAnchor = Utils.GameobjectFinder.FindObject(transform, keyBone);
                if (eyesAnchor != null)
                {
                    var viewField = new GameObject("View Field").transform;
                    viewField.position = eyesAnchor.position;
                    viewField.rotation = eyesAnchor.rotation;
                    viewField.parent = transform;
                    _viewField = viewField.AddComponent<Light>();
                    _viewField.enabled = false;
                    _viewField.type = UnityEngine.LightType.Spot;
                    _viewField.intensity = 0;
                    _viewField.bounceIntensity = 0;
                    _viewField.range = 5;
                    _viewField.innerSpotAngle = 0;
                    _viewField.spotAngle = 60;
                }
            }
            if (messageViewedObject == null) {
                messageViewedObject = new GameObject("Message Viewed Object").transform;
                messageViewedObject.parent = transform;
            }
            if (systemViewedAnchor == null) {
                systemViewedAnchor = new GameObject("System Viewed Object").transform;
                systemViewedAnchor.parent = transform;
            }
        }
#endif
        #endregion
    }
}