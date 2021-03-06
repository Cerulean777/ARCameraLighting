using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.XR.iOS
{
	public class UnityARHitTestExample : MonoBehaviour
	{
		public Transform m_HitTransform;
		public UnityEvent m_PlaneTapped;

		private string m_AnchorId;

        bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes)
        {
            List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, resultTypes);
            if (hitResults.Count > 0) {
                foreach (var hitResult in hitResults) {
                    Debug.Log ("Got hit!");
					if (!string.IsNullOrEmpty(this.m_AnchorId)) {
						UnityARSessionNativeInterface.GetARSessionNativeInterface ().RemoveUserAnchor (this.m_AnchorId); 
					}
                    m_HitTransform.position = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
					m_HitTransform.rotation = Quaternion.identity;// UnityARMatrixOps.GetRotation (hitResult.worldTransform);
					FaceToward.SetFacing(m_HitTransform, Camera.main.transform.position);
					m_PlaneTapped.Invoke ();
					m_AnchorId = UnityARSessionNativeInterface.GetARSessionNativeInterface ().AddUserAnchorFromGameObject(m_HitTransform.gameObject).identifierStr; 
                    Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));
                    return true;
                }
            }
            return false;
        }

		bool EditorHitTest()
		{
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;

				//we'll try to hit one of the plane collider gameobjects that were generated by the plugin
				//effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent
				if (Physics.Raycast (ray, out hit)) {
					//we're going to get the position from the contact point
					m_HitTransform.position = hit.point;
                    Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));

					//and the rotation from the transform of the plane collider
					m_HitTransform.rotation = hit.transform.rotation;
					FaceToward.SetFacing(m_HitTransform, Camera.main.transform.position);
					m_PlaneTapped.Invoke ();
					return true;
				}
			}
			return false;
		}

		void Start()
		{
            FaceToward.SetFacing(m_HitTransform, Camera.main.transform.position);
        }
			
		void Update () 
		{
#if UNITY_EDITOR
			EditorHitTest();
			return;
#endif

			if (Input.touchCount > 0 && m_HitTransform != null)
			{
				var touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
				{
					var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
					ARPoint point = new ARPoint {
						x = screenPosition.x,
						y = screenPosition.y
					};

                    // prioritize reults types
                    ARHitTestResultType[] resultTypes = {
                        //ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, 
                        // if you want to use infinite planes use this:
                        ARHitTestResultType.ARHitTestResultTypeExistingPlane,
                        ARHitTestResultType.ARHitTestResultTypeHorizontalPlane
                        //ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                    }; 
					
                    foreach (ARHitTestResultType resultType in resultTypes)
                    {
                        if (HitTestWithResultType (point, resultType))
                        {
                            return;
                        }
                    }
				}
			}
		}

	
	}
}

