using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Es.InkPainter
{
	using PaintSet = InkCanvas.PaintSet;

	public static class GameObjectExtension
	{
		/// <summary>
		/// Attach InkCanvas to GameObject.
		/// </summary>
		/// <param name="gameObject">GameObject.</param>
		/// <param name="paintDatas">Material data to be painted.</param>
		/// <returns>Generated InkCanvas.</returns>
		public static InkCanvas AddInkCanvas(this GameObject gameObject, List<PaintSet> paintDatas)
		{
			if(paintDatas == null || paintDatas.Count == 0)
			{
				//PaintDatas is null or empty.
				Debug.LogError("Parameter is null or empty.");
				return null;
			}

			var active = gameObject.activeSelf;
			gameObject.SetActive(false);
			var inkCanvas = gameObject.AddComponent<InkCanvas>();
			if(inkCanvas == null)
			{
				//Add component error
				Debug.LogError("Could not attach InkCanvas to GameObject.");
				return null;
			}

			//Init canvas component.
			inkCanvas.OnCanvasAttached += canvas =>
			{
				canvas.PaintDatas = paintDatas;
			};

			gameObject.SetActive(active);
			return inkCanvas;
		}

		/// <summary>
		/// Attach InkCanvas to GameObject.
		/// </summary>
		/// <param name="gameObject">GameObject.</param>
		/// <param name="paintDatas">Material data to be painted.</param>
		/// <returns>Generated InkCanvas.</returns>
		public static InkCanvas AddInkCanvas(this GameObject gameObject, PaintSet paintData)
		{
			if(paintData == null)
			{
				Debug.LogError("Parameter is null or empty.");
				return null;
			}

			return gameObject.AddInkCanvas(new List<PaintSet>() { paintData });
		}

	}
}