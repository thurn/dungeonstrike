using UnityEngine;
public class TestLogging : MonoBehaviour
{
	public GameObject otherObject;
	public JustSomeScript otherMonoBehaviour;

	public string Name = "Joe";
	public int health = 10;
	public int ammo = 5;
	private float stuff = 100;

	public int MyProperty { get; set; }
	void Start()
	{
		float speed = 100.0f;
		MyProperty = 3;

		LoggingHelper.LogValues(new { Name, health, ammo, speed });
		this.LogValues(new { Name, health, ammo, speed, stuff });
		this.LogValues();
		otherMonoBehaviour.LogValues();
	}
}










