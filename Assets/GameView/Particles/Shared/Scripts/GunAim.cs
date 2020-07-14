using UnityEngine;
using System.Collections;

public class GunAim:MonoBehaviour
{
	public int borderLeft;
	public int borderRight;
	public int borderTop;
	public int borderBottom;

	private Camera _parentCamera;
	private bool _isOutOfBounds;

	void Start () 
	{
		_parentCamera = GetComponentInParent<Camera>();
	}

	void Update()
	{
		float mouseX = Input.mousePosition.x;
		float mouseY = Input.mousePosition.y;

		if (mouseX <= borderLeft || mouseX >= Screen.width - borderRight || mouseY <= borderBottom || mouseY >= Screen.height - borderTop) 
		{
			_isOutOfBounds = true;
		} 
		else 
		{
			_isOutOfBounds = false;
		}

		if (!_isOutOfBounds)
		{
			transform.LookAt(_parentCamera.ScreenToWorldPoint (new Vector3(mouseX, mouseY, 5.0f)));
		}
	}

	public bool GetIsOutOfBounds()
	{
		return _isOutOfBounds;
	}
}

