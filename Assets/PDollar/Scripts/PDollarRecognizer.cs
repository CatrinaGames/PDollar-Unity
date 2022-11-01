using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using PDollarGestureRecognizer;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace PDollarGestureRecognizer{
    
    [AddComponentMenu("PDollar/Recognizer")]
    public class PDollarRecognizer : MonoBehaviour
    {
        /// <summary>This stores data about a finger that's currently tracing the shape.</summary>
        [System.Serializable]
        public class FingerData : LeanFingerData
        {
            public List<Vector2> Points = new List<Vector2>(); // This stores the current shape this finger has drawn.

            public Vector2 EndPoint
            {
                get
                {
                    return Points[Points.Count - 1];
                }
            }
        }
        

        [Header("LeanTouch Settings")]
        /// <summary>The method used to find fingers to use with this component. See LeanFingerFilter documentation for more information.</summary>
        public LeanFingerFilter Use = new LeanFingerFilter(true);

        /// <summary>The finger must move at least this many scaled pixels for it to record a new point.</summary>
        public float StepThreshold { set { stepThreshold = value; } get { return stepThreshold; } }
        [SerializeField] private float stepThreshold = 1.0f;

        // This stores the currently active finger data.
        private List<FingerData> fingerDatas;
        // Pool the FingerData so we reduce GC alloc!
        private static Stack<FingerData> fingerDataPool = new Stack<FingerData>();

        [Header("PDollar Settings")]
        public LineRenderer DrawLine;
        [Range(0.0f, 100.0f)]
        public float MinScore = 80f;
        // Pattern Managers pre-mades
        public PDollarPatternManager[] Patterns;
        // Patterns DB
        private List<Gesture> GesturePatterns = new List<Gesture>();
        private List<Point> points = new List<Point>();
        private int strokeId = 0;
        private int vertexCount = 0;

        [SerializeField] UnityEvent<PDollarResult> OnRecognized;

        // Start is called before the first frame update
        void Start()
        {
            //Load pre-made patterns
            foreach (PDollarPatternManager gm in Patterns)
            {
                foreach (string xmlGesture in gm.GestureXML)
                {
                    GesturePatterns.Add(GestureIO.ReadGestureFromXML(xmlGesture));
                }
            }
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        void OnEnable()
        {
            LeanTouch.OnFingerDown += HandleFingerDown;
            LeanTouch.OnFingerUpdate += HandleFingerUpdate;
            LeanTouch.OnFingerUp += HandleFingerUp;
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled or inactive.
        /// </summary>
        void OnDisable()
        {
            LeanTouch.OnFingerDown -= HandleFingerDown;
            LeanTouch.OnFingerUpdate -= HandleFingerUpdate;
            LeanTouch.OnFingerUp -= HandleFingerUp;
        }

        // Update is called once per frame
        void Update()
        {

        }

        void HandleFingerDown(LeanFinger finger)
        {
            var fingers = Use.UpdateAndGetFingers();

            if (fingers.Contains(finger) == true)
            {
                AddFinger(finger);

                points = new List<Point>();
                DrawLine.positionCount = 0;
                vertexCount = 0;
            }
        }

        void HandleFingerUpdate(LeanFinger finger)
        {
            // SCREEN POSITION
            var fingerData = LeanFingerData.Find(fingerDatas, finger);

            if (fingerData != null && Vector2.Distance(finger.ScreenPosition, fingerData.EndPoint) > stepThreshold)
            {
                points.Add(new Point(finger.ScreenPosition.x, finger.ScreenPosition.y, strokeId));
                DrawLine.positionCount = ++vertexCount;
                DrawLine.SetPosition(vertexCount - 1, new Vector3(finger.ScreenPosition.x, finger.ScreenPosition.y, 10));
            }
        }

        void HandleFingerUp(LeanFinger finger)
        {
            var fingers = Use.UpdateAndGetFingers();

            if (fingers.Contains(finger) == true)
            {
                var fingerData = LeanFingerData.Find(fingerDatas, finger);
                var points = fingerData.Points;

                LeanFingerData.Remove(fingerDatas, finger, fingerDataPool);

                Recognize();
            }
        }

        public void Recognize()
        {
            Gesture candidate = new Gesture(points.ToArray());
            Result gestureResult = PointCloudRecognizer.Classify(candidate, GesturePatterns.ToArray());

            if(gestureResult.Score >= MinScore/100){
                PDollarResult result = new PDollarResult();
                result.GestureName = gestureResult.GestureClass;
                result.score = gestureResult.Score;
                InvokeREcognized(result);
            }
        }

        protected void InvokeREcognized(PDollarResult recogevnt)
        {
            if (OnRecognized != null)
            {
                OnRecognized.Invoke(recogevnt);
            }
        }

        public void SaveToTest(string name){
            Gesture candidate = new Gesture(points.ToArray(), name);
            GesturePatterns.Add(candidate);
        }

        public void ClearLine()
        {
            points = new List<Point>();
            DrawLine.positionCount = 0;
            vertexCount = 0;
        }

        public string GetXML(string name){
            string xmlV = GestureIO.GetXML(points.ToArray(), name);
            return xmlV;
        }

        /// <summary>If you've set Use to ManuallyAddedFingers, then you can call this method to manually add a finger.</summary>
        public void AddFinger(LeanFinger finger)
        {
            var fingerData = LeanFingerData.FindOrCreate(ref fingerDatas, finger);

            fingerData.Points.Clear();

            fingerData.Points.Add(finger.ScreenPosition);
        }

        [MenuItem("GameObject/PDollar/Recognizer", false, 1)]
        public static void CreateTouch()
        {
            var gameObject = new GameObject(typeof(PDollarRecognizer).Name);

            Undo.RegisterCreatedObjectUndo(gameObject, "Create PDollar Recognizer");

            gameObject.AddComponent<PDollarRecognizer>();

            Selection.activeGameObject = gameObject;
        }
    }

    public class PDollarResult
    {
        public string GestureName;
        public float score;
    }
}