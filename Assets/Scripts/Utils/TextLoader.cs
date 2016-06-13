using System;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Utils
{
	public class TextLoader
	{
		public static TextAsset GetFile( string path, string fileName )
		{
			var filePath =path + "/" + fileName;

			TextAsset textData = Resources.Load(filePath, typeof(TextAsset)) as TextAsset;

			if (textData == null)
				throw new NullReferenceException("TextLoader failed get text from Resources:" + filePath);

			return textData;
		}

		public static JSONObject GetFileAsJson( string path, string fileName )
		{
			TextAsset text = GetFile(path, fileName);

			JSONObject root = new JSONObject(text.text);

			if (root == JSONObject.nullJO)
				throw new NullReferenceException("TextLoader failed load text as json:" + fileName);

			return root;
		}

	}
}
