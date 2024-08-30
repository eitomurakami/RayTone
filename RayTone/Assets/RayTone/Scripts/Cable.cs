// Code inspired by "Real-Time Procedural Cable Simple" Unity Asset by DrinkingWindGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RayTone
{
	public class Cable : MonoBehaviour
	{
		// reference to line
		[SerializeField] private LineRenderer line;

		// target position
		public Vector3 targetPosition = default;

		// private variables
		private float frequency = 1;
		private CameraController cameraController;

		/////
		//Awake
		void Awake()
        {
			targetPosition = this.transform.position;
        }

		/////
		//Start
		void Start()
		{
			cameraController = CameraController.Instance;

			frequency = Random.Range(0.8f, 1f);
			UpdateCable();
		}

		/////
		//UPDATE
		void Update()
		{
			if (cameraController.GetVisibility())
            {
				UpdateCable();
			}
		}

		/// <summary>
		/// Update cable point positions
		/// </summary>
		void UpdateCable()
		{
			// Set number of points in line based on distance
			line.positionCount = (int)(Mathf.Clamp(Vector3.Distance(transform.position, targetPosition), 2f, 100f));

			// Calculate sway direction scaled by distance
			Vector3 direction = transform.TransformDirection(new Vector3(Mathf.Sin(Time.time * frequency), 0, 0)) * Mathf.Pow((line.positionCount * 0.02f), 0.75f);

			// Calculate line point positions
			for (int i = 0; i < line.positionCount; i++)
			{
				float fract = (float)i / (line.positionCount - 1);
				float sineCurve = Mathf.Sin(fract * Mathf.PI);
				Vector3 position = Vector3.Lerp(transform.position, targetPosition, fract) + sineCurve * direction;
				line.SetPosition(i, position);
			}
		}
	}
}
