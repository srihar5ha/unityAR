using UnityEngine;
using System.Collections;

using com.google.zxing.qrcode;
using Vuforia;
using UnityEngine.UI;
using com.google.zxing;
using System.Collections.Generic;
using System;
using System.Linq;
using com.google.zxing.common;
using com.google.zxing.datamatrix;
using com.google.zxing.datamatrix.decoder;
using com.google.zxing.multi;
using System.Text;

public class CameraImageAccess : MonoBehaviour, ITrackerEventHandler {
	
	private bool isFrameFormatSet;
	
	private Vuforia.Image cameraFeed;
	private string tempText;
	private string qrText;
	public Text _scannedText;
	private GenericMultipleBarcodeReader _reader;
	private Hashtable _hints;

	void Start () {
		_reader = new GenericMultipleBarcodeReader (new QRCodeReader());
		_hints = new Hashtable();
		_hints.Add (DecodeHintType.TRY_HARDER, true);
		_hints.Add (DecodeHintType.POSSIBLE_FORMATS, new List<BarcodeFormat> (){ BarcodeFormat.QR_CODE, BarcodeFormat.DATAMATRIX});

		//var barcodeFormats = Enum.GetValues (typeof(BarcodeFormat)).Cast<BarcodeFormat> ().ToList ();
		//_hints.Add (DecodeHintType.POSSIBLE_FORMATS, new List<BarcodeFormat> (barcodeFormats));

		VuforiaBehaviour qcarBehaviour = GetComponent<VuforiaBehaviour>();
		
		if (qcarBehaviour) {
			qcarBehaviour.RegisterTrackerEventHandler(this);
		}
		
		isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Vuforia.Image.PIXEL_FORMAT.GRAYSCALE, true);
		
		InvokeRepeating("Autofocus", 1f, 2f);
	}

	#region ITrackerEventHandler implementation

	public void OnInitialized ()
	{

	}

	#endregion
	
	void Autofocus () {
		CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
	}

	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}
	}
		
	public void OnTrackablesUpdated () {
		try {
			if(!isFrameFormatSet) {
				isFrameFormatSet = CameraDevice.Instance.SetFrameFormat(Vuforia.Image.PIXEL_FORMAT.GRAYSCALE, true);
			}
			
			cameraFeed = CameraDevice.Instance.GetCameraImage(Vuforia.Image.PIXEL_FORMAT.GRAYSCALE);//GlobalHistogramBinarizer
			var bitmap = new BinaryBitmap (new GlobalHistogramBinarizer (new RGBLuminanceSource (cameraFeed.Pixels, cameraFeed.BufferWidth, cameraFeed.BufferHeight, true)));
			var result = _reader.decodeMultiple(bitmap, _hints);

			if (result != null && result.Length > 0)
			{
				StringBuilder builder = new StringBuilder();
				foreach (var entry in result)
				{
					builder.Append(entry);
					builder.Append("\n");
				}
				tempText = builder.ToString();
			}
			if (!string.IsNullOrEmpty(tempText) && _scannedText != null)
				_scannedText.text = tempText;
		}
		catch {
			// Failed detecting QR Code!
		}
		finally {
			if(!string.IsNullOrEmpty(tempText)) {
				qrText = tempText;
			}
		}
	}
}
