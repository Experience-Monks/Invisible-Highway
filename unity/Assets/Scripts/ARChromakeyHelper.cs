using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/* 
 * A helper object that enables shaders to access the tango camera texture directly
 * The texture can be accessed as "_TangoCamTexture"
 */

public class ARChromakeyHelper : MonoBehaviour
{
	// a default color for the chroma key
	public Color maskColor = Color.green;

	// a default threshold for the chroma key
	public float threshold = 0.5f;

	// the command buffer to add to the rendering pipeline to gain access to the Tango camera texture on the GPU
	private CommandBuffer tangoCamCommandBuffer;

	// the texture ID of the texture
	private int tangoCamTextureId;

	// a name for the tango camera texture
	// NOTE: if this changes it also has to change in the shader
	private const string TANGO_CAM_TEXTURE_NAME = "_TangoCamTexture";

    // camera reference
    private Camera arCamera;

	// singleton
	private static ARChromakeyHelper _instance = null;
	public static ARChromakeyHelper Instance {
		get { return _instance; }
	}

	void Awake()
	{
		_instance = this;
	}

	IEnumerator Start()
	{
        arCamera = FindObjectOfType<ARController>().m_firstPersonCamera;
		// wait until TangoARScreen has added its own Command Buffer
		// more on Unity Command Buffers https://docs.unity3d.com/Manual/GraphicsCommandBuffers.html
		while (arCamera.GetCommandBuffers (CameraEvent.BeforeForwardOpaque).Length <= 0) {
			yield return new WaitForEndOfFrame ();
		}

		// create our new command buffer and add it to the camera
		if (tangoCamCommandBuffer == null) {
            Debug.Log("Adding CommandBuffer");
			tangoCamTextureId = Shader.PropertyToID (TANGO_CAM_TEXTURE_NAME);

			tangoCamCommandBuffer = new CommandBuffer ();
			tangoCamCommandBuffer.name = "arCamCommandBuffer";
			tangoCamCommandBuffer.GetTemporaryRT (tangoCamTextureId, -1, -1, 0);
			tangoCamCommandBuffer.Blit (BuiltinRenderTextureType.CameraTarget, tangoCamTextureId);
			tangoCamCommandBuffer.SetGlobalTexture (TANGO_CAM_TEXTURE_NAME, tangoCamTextureId);

            // add the CB to the same event as Tango uses, but added afterwards
            arCamera.AddCommandBuffer (CameraEvent.BeforeForwardOpaque, tangoCamCommandBuffer);
		}
		// load chroma key parameters from persistent storage
		LoadParams ();
	}

	// apply the mask color to all relevant materials
	// don't call this all the time as it iterates through all renderers in the scene
	// this is used when the scene is created to update the materials of objects that have chroma keying (like the Road)
	public void UpdateMaterials()
	{
		Renderer[] allRenders = FindObjectsOfType<Renderer> ();
		Shader tangoChromakeyShader = Shader.Find ("Custom/TangoChromakey");
		foreach (Renderer r in allRenders) {
			if (r.material.shader == tangoChromakeyShader) {
				r.material.SetColor ("_MaskColor", maskColor);
				r.material.SetFloat ("_Threshold", threshold);
			}
		}

		// save current parameters to persistent storage
		SaveParams ();
	}

	// save to persistent storage
	private void SaveParams()
	{
		PlayerPrefs.SetFloat ("maskR", maskColor.r);
		PlayerPrefs.SetFloat ("maskG", maskColor.g);
		PlayerPrefs.SetFloat ("maskB", maskColor.b);
		PlayerPrefs.SetFloat ("maskT", threshold);
	}

	// load from storage
	private void LoadParams()
	{
		if (PlayerPrefs.HasKey ("maskR")) {
			maskColor.r = PlayerPrefs.GetFloat ("maskR");
			maskColor.g = PlayerPrefs.GetFloat ("maskG");
			maskColor.b = PlayerPrefs.GetFloat ("maskB");
			threshold = PlayerPrefs.GetFloat ("maskT");
		}
	}

	// called when the object is destroyed
	void OnDestroy()
	{
		// remove the command buffer
		if (tangoCamCommandBuffer != null && arCamera) {
            arCamera.RemoveCommandBuffer (CameraEvent.BeforeForwardOpaque, tangoCamCommandBuffer);
			tangoCamCommandBuffer = null;
		}
	}
}
